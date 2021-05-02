using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
			entry.Available = true;

			bool localChanges = false;
			bool incomingChanges = false;
			bool outgoingChanges = false;

			// TODO: Implement
			Thread.Sleep(1000);

			entry.LocalChanges = localChanges;
			entry.IncomingChanges = incomingChanges;
			entry.OutgoingChanges = outgoingChanges;

			throw new NotImplementedException();

			entry.StatusKnown = true;
		}
	}

}
