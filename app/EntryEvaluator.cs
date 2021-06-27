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

		private object queuelock = new object();
		private List<Tuple<Entry, EntryStatus, Action<string>>> queue = new List<Tuple<Entry, EntryStatus, Action<string>>>();
		private Thread worker = null;

		private List<string> defaultBranches = new List<string>();

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
				foreach (var i in queue) if (i.Item1 == entry) return null; // already in queue

				EntryStatus es = new EntryStatus() { Evaluating = true };
				queue.Add(new Tuple<Entry, EntryStatus, Action<string>>(entry, es, setLastMessage));

				if (worker == null)
				{
					worker = new Thread(work);
					worker.Start();
				}

				return es;
			}
		}

		private void work()
		{
			while (true)
			{
				Tuple<Entry, EntryStatus, Action<string>> it = null;
				lock (queuelock)
				{
					if (queue.Count <= 0)
					{
						worker = null;
						return;
					}
					it = queue.First();
				}

				try
				{
					evaluateEntry(it.Item1, it.Item2, it.Item3);

					it.Item2.FailedStatus = false;
				}
				catch (Exception ex)
				{
					it.Item2.FailedStatus = true;
					it.Item3("Failed to evaluate: " + ex);
				}

				// work completed
				lock (queuelock)
				{
					queue.Remove(it);
					it.Item2.Evaluating = false;
				}
			}
		}

		private Random rnd = new Random();

		private void evaluateEntry(Entry entry, EntryStatus status, Action<string> setLastMessage)
		{
			if (!System.IO.Directory.Exists(entry.Path))
			{
				status.Available = false;

				status.LocalChanges = false;
				status.IncomingChanges = false;
				status.OutgoingChanges = false;
				return;
			}
			status.Available = true;

			switch (entry.Type.ToLower())
			{
				case "git":
					evaluateGit(entry, status, setLastMessage);
					break;
				default:
					throw new Exception("Unknown type");
			}
		}

		private string gitBin()
		{
			return
				System.IO.File.Exists(Properties.Settings.Default.gitBin)
				? Properties.Settings.Default.gitBin
				: "git.exe";
		}

		private void evaluateGit(Entry entry, EntryStatus status, Action<string> setLastMessage)
		{
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
			p.StartInfo.WorkingDirectory = entry.Path;
			p.StartInfo.CreateNoWindow = true;
			p.Start();
			var result = p.StandardOutput.ReadToEnd().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
			p.WaitForExit();

			string branch = result.FirstOrDefault((s) => { return s.StartsWith("# branch.head "); });
			if (!string.IsNullOrWhiteSpace(branch))
			{
				status.BranchName = branch.Substring(13).Trim().ToLower();

				if (defaultBranches.Contains(status.BranchName))
				{
					status.OnBranch = false;
				}
				else
				{
					status.OnBranch = true;
				}

			}
			else
			{
				status.OnBranch = true;
				status.BranchName = null;
			}

			string upstream = result.FirstOrDefault((s) => { return s.StartsWith("# branch.upstream "); });
			status.RemoteTracked = upstream?.Length > 18;

			string ab = result.FirstOrDefault((s) => { return s.StartsWith("# branch.ab "); });
			if (!string.IsNullOrWhiteSpace(ab))
			{
				var abm = Regex.Match(ab, @"^\#\s+branch\.ab\s+\+(\d+)\s+-(\d+)");
				if (abm.Success && abm.Groups[1].Success && abm.Groups[2].Success)
				{
					status.OutgoingChanges = int.Parse(abm.Groups[1].Value) > 0;
					status.IncomingChanges = int.Parse(abm.Groups[2].Value) > 0;
				}
				else
				{
					throw new Exception("Failed to parse ahead-begin info: " + ab);
				}
			}

			result = result.Where((s) => { return !s.StartsWith("#"); }).ToArray();
			if (result == null || result.Length <= 0)
			{
				status.LocalChanges = false;
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
				status.LocalChanges = changes;
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
	}

}
