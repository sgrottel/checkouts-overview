using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace SG.Checkouts_Overview
{
	public class DisksScannerEverything : IDisksScanner
	{
		public string Root { get; set; } = null;
		public string[] IgnorePattern { get; set; } = null;
		public bool ScanCheckoutSubdirs { get; set; } = false;

        public event EventHandler<string> ScanMessage;
		public event Func<Entry, bool> EntryFound;

		public void AbortScan()
		{
			// scan is so fast, no abort needed
		}

		public void Scan()
		{
			ScanMessage?.Invoke(this, "Scan started...");

			if (!EverythingSearchClient.SearchClient.IsEverythingAvailable())
			{
				// everything is likely not installed.
				return;
			}

			EverythingSearchClient.SearchClient everything = new();
			EverythingSearchClient.Result res = everything.Search("^\\.git$", EverythingSearchClient.SearchClient.SearchFlags.RegEx | EverythingSearchClient.SearchClient.SearchFlags.MatchWholeWord);

			int added = 0;
			foreach (EverythingSearchClient.Result.Item dgit in res.Items)
			{
				if (!dgit.Flags.HasFlag(EverythingSearchClient.Result.ItemFlags.Folder))
				{
					continue;
				}
				if (dgit.Path.Contains("\\$RECYCLE.BIN\\", StringComparison.CurrentCultureIgnoreCase))
				{
					continue; // skip entries already recycled
				}

				if (EntryFound?.Invoke(new Entry()
					{
						Name = System.IO.Path.GetFileName(dgit.Path),
						Path = dgit.Path,
						Type = "git"
					}) ?? false)
				{
					added++;
				}
			}

			ScanMessage?.Invoke(this, string.Format("Scan completed. {0} entr{1} added.", added, (added == 1) ? "y" : "ies"));
		}

	}
}
