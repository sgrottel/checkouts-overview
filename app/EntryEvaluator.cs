using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SG.Checkouts_Overview
{

	public class EntryEvaluator
	{

		#region public Utility

		public void CheckType(Entry entry, Action<string> setLastMessage)
		{
			if (!System.IO.Directory.Exists(entry.Path))
			{
				setLastMessage?.Invoke("ERROR: Path is not available");
				return;
			}

			if (System.IO.Directory.Exists(
				System.IO.Path.Combine(entry.Path, ".git")))
			{
				entry.Type = "git";
				setLastMessage?.Invoke("Git clone detected.");
				return;
			}
		}

		public DateTime GetCommitDate(Entry entry)
		{
			switch (entry.Type.ToLower())
			{
				case "git":
					return GetCommitDateGit(entry);
				default:
					return DateTime.MinValue;
			}
		}

		#endregion

		#region private utility

		private string gitBin()
		{
			return
				System.IO.File.Exists(Properties.Settings.Default.gitBin)
				? Properties.Settings.Default.gitBin
				: "git.exe";
		}

		private class RunResult
		{
			public string StdOut { get; set; }
			public string StdErr { get; set; }
			public int ExitCode { get; set; }
		}

		private RunResult runGit(string path, string[] args)
		{
			RunResult rr = new RunResult();

			if (!System.IO.Directory.Exists(path))
			{
				rr.StdOut = string.Empty;
				rr.StdErr = "Path does not exist";
				rr.ExitCode = -1;
				return rr;
			}

			Process p = new Process();
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.RedirectStandardError = true;
			p.StartInfo.FileName = gitBin();
			p.StartInfo.ArgumentList.Clear();
			foreach (string a in args) {
				p.StartInfo.ArgumentList.Add(a);
			}
			p.StartInfo.WorkingDirectory = path;
			p.StartInfo.CreateNoWindow = true;
			p.Start();
			rr.StdOut = p.StandardOutput.ReadToEnd();
			rr.StdErr = p.StandardError.ReadToEnd();
			p.WaitForExit();
			rr.ExitCode = p.ExitCode;
			return rr;
		}

		private DateTime GetCommitDateGit(Entry entry)
		{
			var result = runGit(entry.Path, new string[] { "log", "-n", "1", "--format=%aI" });
			DateTime d = DateTime.MinValue;
			DateTime.TryParse(result.StdOut, out d);
			return d;
		}

		#endregion

		#region evaluation queue

		#region private state

		private class Job
		{
			public Entry Entry { get; set; }
			public EntryStatus Status { get; set; }	
			public Action<string> SetLastMessage { get; set; }
			public Action<Job> Work { get; set; }
			public Action<Job> Next { get; set; } = null;
		};

		private object queuelock = new object();
		private List<Job> queue = new List<Job>();

		private List<string> defaultBranches = new List<string>();

		#endregion

		#region public api

		public void Start()
		{
			defaultBranches.Clear();
			string db = Properties.Settings.Default.gitDefaultBranches;
			if (string.IsNullOrWhiteSpace(db))
			{
				defaultBranches.Add("main");
				defaultBranches.Add("master");
			}
			else
			{
				defaultBranches = db
					.Split(";", StringSplitOptions.RemoveEmptyEntries)
					.Select(s => s.Trim().ToLower())
					.ToList();
			}

		}

		public void Shutdown()
		{
			lock (queuelock)
			{
				queue.Clear();
			}
		}

		public EntryStatus BeginEvaluate(Entry entry, Action<string> setLastMessage)
		{
			lock (queuelock)
			{
				foreach (var i in queue)
				{
					if (i.Entry == entry)
					{
						return null; // already in queue
					}
				}

				if (entry.Type.ToLowerInvariant() != "git")
				{
					setLastMessage("Updating this type of entry is not implemented.");
					return null; // unsupported type
				}

				bool doFetch = false;
				switch (entry.GitFetchOnUpdate)
				{
					case Tristate.False: doFetch = false; break;
					case Tristate.True: doFetch = true; break;
					case Tristate.Default:
						doFetch = Properties.Settings.Default.gitFetchAllOnUpdate;
						break;
					default:
						doFetch = false;
						break;
				}

				Job job = new Job()
				{
					Entry = entry,
					Status = new EntryStatus() { Evaluating = true },
					SetLastMessage = setLastMessage,
					Work = doFetch ? workGitUpdateFetch : workGitUpdate
				};

				queue.Add(job);
				if (!ThreadPool.QueueUserWorkItem(processJob, job, false))
				{
					queue.Remove(job);
					setLastMessage("Failed to queue the update job");
					return null; // unsupported type
				}

				return job.Status;
			}
		}

		#endregion

		#region private implementation

		private void processJob(Job job)
		{
			try
			{
				job.Work(job);

				job.Status.FailedStatus = false;
			}
			catch (Exception ex)
			{
				job.Status.FailedStatus = true;
				job.Next = null;
				job.SetLastMessage("Failed to evaluate: " + ex);
			}

			// work completed
			lock (queuelock)
			{
				if (job.Next != null)
				{
					job.Work = job.Next;
					job.Next = null;

					if (ThreadPool.QueueUserWorkItem(processJob, job, false))
					{
						// reuse job object
						return;
					}
					else
					{
						job.SetLastMessage("Failed to queue the update job");
					}
				}

				queue.Remove(job);
				job.Status.Evaluating = false;
			}
		}

		private void workGitUpdate(Job job)
		{
			if (!System.IO.Directory.Exists(job.Entry.Path))
			{
				job.Status.Available = false;

				job.Status.LocalChanges = false;
				job.Status.IncomingChanges = false;
				job.Status.OutgoingChanges = false;
				return;
			}
			job.Status.Available = true;

			var result = runGit(job.Entry.Path, new string[]{
				"status", "--short", "--branch", "--porcelain=v2", "--ahead-behind"
			}).StdOut.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

			string branch = result.FirstOrDefault((s) => { return s.StartsWith("# branch.head "); });
			if (!string.IsNullOrWhiteSpace(branch))
			{
				job.Status.BranchName = branch.Substring(13).Trim().ToLower();
				if (string.IsNullOrWhiteSpace(job.Entry.MainBranch))
				{
					job.Status.OnBranch = (defaultBranches.Contains(job.Status.BranchName) == false);
				}
				else
				{
					job.Status.OnBranch = (job.Entry.MainBranch.Equals(job.Status.BranchName) == false);
				}

			}
			else
			{
				job.Status.OnBranch = true;
				job.Status.BranchName = null;
			}

			string upstream = result.FirstOrDefault((s) => { return s.StartsWith("# branch.upstream "); });
			job.Status.RemoteTracked = upstream?.Length > 18;

			string ab = result.FirstOrDefault((s) => { return s.StartsWith("# branch.ab "); });
			if (!string.IsNullOrWhiteSpace(ab))
			{
				var abm = Regex.Match(ab, @"^\#\s+branch\.ab\s+\+(\d+)\s+-(\d+)");
				if (abm.Success && abm.Groups[1].Success && abm.Groups[2].Success)
				{
					job.Status.OutgoingChanges = int.Parse(abm.Groups[1].Value) > 0;
					job.Status.IncomingChanges = int.Parse(abm.Groups[2].Value) > 0;
				}
				else
				{
					throw new Exception("Failed to parse ahead-begin info: " + ab);
				}
			}

			result = result.Where((s) => { return !s.StartsWith("#"); }).ToArray();
			if (result == null || result.Length <= 0)
			{
				job.Status.LocalChanges = false;
			}
			else
			{
				bool changes = false;
				foreach (string s in result)
				{
					if (Regex.IsMatch(s, @"^[12]\s\S\S\s"))
					{
						changes = true;
					}
				}
				job.Status.LocalChanges = changes;
			}
		}

		private void workGitUpdateFetch(Job job)
		{
			workGitUpdate(job);
			job.Next = workGitFetch;
		}

		private void workGitFetch(Job job)
		{
			var res = runGit(job.Entry.Path, new string[] { "fetch", "--all" });
			if (res.ExitCode == 0)
			{
				if (!string.IsNullOrWhiteSpace(res.StdOut)
					|| !string.IsNullOrWhiteSpace(res.StdErr))
				{
					job.Next = workGitUpdate;
				}
			}
		}

		#endregion
		#endregion
	}

}
