using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SG.Checkouts_Overview
{
	public class DisksScannerEverything
	{
		public Collection<Entry> Entries { get; set; } = null;

		public void Scan()
		{
			string everythingSearch =
				System.IO.Path.Combine(
					System.IO.Path.GetDirectoryName(
						System.Reflection.Assembly.GetExecutingAssembly().Location),
					"es.exe");
			if (!System.IO.File.Exists(everythingSearch))
			{
				throw new InvalidOperationException("Unable to find `es.exe` search utility.");
			}

			string tempFile = System.IO.Path.GetTempFileName();

			Process p = new Process();

			p.StartInfo.UseShellExecute = false;
			p.StartInfo.CreateNoWindow = true;
			p.StartInfo.FileName = everythingSearch;
			p.StartInfo.ArgumentList.Clear();
			// -r "^.git$" -ww /ad
			p.StartInfo.ArgumentList.Add("-r");
			p.StartInfo.ArgumentList.Add("^.git$");
			p.StartInfo.ArgumentList.Add("-ww");
			p.StartInfo.ArgumentList.Add("/ad");
			p.StartInfo.ArgumentList.Add("-export-m3u8");
			p.StartInfo.ArgumentList.Add(tempFile);

			p.Start();
			p.WaitForExit();

			var result = System.IO.File.ReadAllLines(tempFile, Encoding.UTF8);
			System.IO.File.Delete(tempFile);

			if (result == null || result.Length <= 0) return;

			foreach (string dgit in result)
			{
				if (dgit.Contains("\\$RECYCLE.BIN\\", StringComparison.CurrentCultureIgnoreCase))
				{
					continue; // skip entries already recycled
				}

				string d = System.IO.Path.GetDirectoryName(dgit);
				if (Entries.FirstOrDefault((Entry e) => { return string.Equals(e.Path, d, StringComparison.CurrentCultureIgnoreCase); }) != null) continue; // entry known

				Entries.Add(new Entry()
				{
					Name = System.IO.Path.GetFileName(d),
					Path = d,
					Type = "git"
				});
			}

		}

	}
}
