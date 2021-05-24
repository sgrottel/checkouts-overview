using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace SG.Checkouts_Overview
{
	public class DisksScannerFilesystem : IDisksScanner
	{
		public string Root { get; set; }

		public event EventHandler<string> ScanMessage;
		public event Func<Entry, bool> EntryFound;

		private bool abort = false;
		private object abortLock = new object();
		private bool Abort {
			get { lock (abortLock) { return abort; } }
			set { lock (abortLock) { abort = value; } }
		}

		public void AbortScan()
		{
			Abort = true;
		}

		public void Scan()
		{
			ScanMessage?.Invoke(this, "Scan started...");
			Abort = false;

			List<string> paths = new List<string>();
			paths.Add(Root);

			int added = 0;
			while (paths.Count > 0)
			{
				try
				{
					if (Abort) break;
					string d = paths[0];
					paths.RemoveAt(0);
					report(d, added);

					if (!System.IO.Directory.Exists(d)) continue;
					if (d.Contains("\\$RECYCLE.BIN\\", StringComparison.CurrentCultureIgnoreCase)) continue; // skip entries already recycled

					if (System.IO.Directory.Exists(System.IO.Path.Combine(d, ".git")))
					{
						if (EntryFound?.Invoke(new Entry()
							{
								Name = System.IO.Path.GetFileName(d),
								Path = d,
								Type = "git"
							}) ?? false)
						{
							added++;
						}
					}

					paths.AddRange(System.IO.Directory.GetDirectories(d));
				} catch { }
			}

			ScanMessage?.Invoke(this, string.Format("Scan completed. {0} entr{1} added.", added, (added == 1) ? "y" : "ies"));
		}

		private DateTime lastReported = DateTime.MinValue;

		private void report(string d, int added)
		{
			DateTime n = DateTime.Now;
			if ((n - lastReported).TotalSeconds < 0.25) return;
			lastReported = n;
			string msg = string.Format("Scanning: {0}", d, added);

			ScanMessage?.Invoke(this, msg);
		}
	}
}
