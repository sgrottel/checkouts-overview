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
	internal class EntryEvaluator
	{

		internal void CheckType(Entry entry)
		{
			if (!System.IO.Directory.Exists(entry.Path))
			{
				entry.LastMessage = "ERROR: Path is not available";
				return;
			}

			if (System.IO.Directory.Exists(
				System.IO.Path.Combine(entry.Path, ".git")))
			{
				entry.Type = "git";
				entry.LastMessage = "Git clone detected.";
				return;
			}
		}

		private object queuelock = new object();
		private List<Entry> queue = new List<Entry>();
		private Thread worker = null;

		internal void Start()
		{
		}

		internal void Shutdown()
		{
			lock (queuelock)
			{
				queue.Clear();
			}
		}

		internal void BeginEvaluate(Entry entry)
		{
			lock (queuelock)
			{
				if (queue.Contains(entry)) return;
				queue.Add(entry);
				entry.Evaluating = true;
				if (worker == null)
				{
					worker = new Thread(work);
					worker.Start();
				}
			}
		}

		private void work()
		{
			while (true)
			{
				Entry entry = null;
				lock (queuelock)
				{
					if (queue.Count <= 0)
					{
						worker = null;
						return;
					}
					entry = queue.First();
				}

				try
				{
					evaluateEntry(entry);

					entry.FailedStatus = false;
				}
				catch (Exception ex)
				{
					entry.FailedStatus = true;
					entry.LastMessage = "Failed to evaluate: " + ex;
				}

				// work completed
				lock (queuelock)
				{
					queue.Remove(entry);
					entry.Evaluating = false;
				}
			}
		}

		private Random rnd = new Random();

		private void evaluateEntry(Entry entry)
		{
			if (!System.IO.Directory.Exists(entry.Path))
			{
				entry.StatusKnown = true;
				entry.Available = false;

				entry.LocalChanges = false;
				entry.IncomingChanges = false;
				entry.OutgoingChanges = false;
				return;
			}
			if (!entry.Available) entry.StatusKnown = false;
			entry.Available = true;

			switch (entry.Type.ToLower())
			{
				case "git":
					evaluateGit(entry);
					break;
				default:
					throw new Exception("Unknown type");
			}

			if (!entry.StatusKnown)
				throw new Exception("Failed to analyse entry: " + entry.LastMessage);
		}

		private void evaluateGit(Entry entry)
		{
			Process p = new Process();

			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.FileName = "git.exe";
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

			string ab = result.FirstOrDefault((s) => { return s.StartsWith("# branch.ab "); });
			if (!string.IsNullOrWhiteSpace(ab))
			{
				var abm = Regex.Match(ab, @"^\#\s+branch\.ab\s+\+(\d+)\s+-(\d+)");
				if (abm.Success && abm.Groups[1].Success && abm.Groups[2].Success)
				{
					entry.OutgoingChanges = int.Parse(abm.Groups[1].Value) > 0;
					entry.IncomingChanges = int.Parse(abm.Groups[2].Value) > 0;
				}
				else
				{
					throw new Exception("Failed to parse ahead-begin info: " + ab);
				}
			}

			result = result.Where((s) => { return !s.StartsWith("#"); }).ToArray();
			if (result == null || result.Length <= 0)
			{
				entry.LocalChanges = false;
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
				entry.LocalChanges = changes;
			}
			entry.StatusKnown = true;
		}

	}

}
