﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SG.Checkouts_Overview
{
	/// <summary>
	/// TODO: https://docs.microsoft.com/en-us/dotnet/api/system.threading.threadpool.queueuserworkitem?view=net-6.0#system-threading-threadpool-queueuserworkitem(system-threading-waitcallback-system-object)
	/// </summary>
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

		private DateTime GetCommitDateGit(Entry entry)
		{
			Process p = new Process();
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.FileName = gitBin();
			p.StartInfo.ArgumentList.Clear();
			p.StartInfo.ArgumentList.Add("log");
			p.StartInfo.ArgumentList.Add("-n");
			p.StartInfo.ArgumentList.Add("1");
			p.StartInfo.ArgumentList.Add("--format=%aI");
			p.StartInfo.WorkingDirectory = entry.Path;
			p.StartInfo.CreateNoWindow = true;
			p.Start();
			var result = p.StandardOutput.ReadToEnd();
			p.WaitForExit();
			DateTime d = DateTime.MinValue;
			DateTime.TryParse(result, out d);
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
        };

		private object queuelock = new object();
		private List<Job> queue = new List<Job>();
		private Thread worker = null;

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
			Thread t = null;
			lock (queuelock)
			{
				queue.Clear();
				t = worker;
			}
			if (t != null)
            {
				t.Join();
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

				Job job = new Job()
				{
					Entry = entry,
					Status = new EntryStatus() { Evaluating = true },
					SetLastMessage = setLastMessage,
					Work = workGitUpdate
				};

				queue.Add(job);

				if (worker == null)
				{
					worker = new Thread(work);
					worker.Start();
				}

				return job.Status;
			}
		}

        #endregion

        #region private implementation

        private void work()
		{
			while (true)
			{
				Job job = null;
				lock (queuelock)
				{
					if (queue.Count <= 0)
					{
						worker = null;
						return;
					}
					job = queue.First();
				}

				try
				{
					job.Work(job);

					job.Status.FailedStatus = false;
				}
				catch (Exception ex)
				{
					job.Status.FailedStatus = true;
					job.SetLastMessage("Failed to evaluate: " + ex);
				}

				// work completed
				lock (queuelock)
				{
					queue.Remove(job);
					job.Status.Evaluating = false;
				}
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

			Process p = new Process();
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.FileName = gitBin();
			p.StartInfo.ArgumentList.Clear();
			p.StartInfo.ArgumentList.Add("status");
			p.StartInfo.ArgumentList.Add("--short");
			p.StartInfo.ArgumentList.Add("--branch");
			p.StartInfo.ArgumentList.Add("--porcelain=v2");
			p.StartInfo.ArgumentList.Add("--ahead-behind");
			p.StartInfo.WorkingDirectory = job.Entry.Path;
			p.StartInfo.CreateNoWindow = true;
			p.Start();
			var result = p.StandardOutput.ReadToEnd().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
			p.WaitForExit();

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

        #endregion
        #endregion
    }

}
