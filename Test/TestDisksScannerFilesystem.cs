using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SG.Checkouts_Overview.Test
{
	[TestClass]
	public class TestDisksScannerFilesystem
	{

		[TestMethod]
		public void Invoke()
		{
			string tdp = System.IO.Path.Combine(Utility.FindMyGitCloneDir(), "TestData");

			List<Entry> es = new List<Entry>();
			DisksScannerFilesystem dsfs = new DisksScannerFilesystem()
			{
				Root = tdp
			};
			dsfs.EntryFound += (Entry e) => { es.Add(e); return true; };

			dsfs.Scan();

			Assert.IsTrue(es.Count >= 32);

			for (byte i = 0; i < 32; ++i)
			{
				BitArray bits = new BitArray(new byte[] { i });
				string n = "c";
				n += bits[0] ? "-branch" : "-main";
				if (bits[1]) n += "-untracked";
				if (bits[2]) n += "-behind";
				if (bits[3]) n += "-ahead";
				if (bits[4]) n += "-changed";

				bool found = false;
				foreach (Entry e in es)
				{
					if (string.Equals(System.IO.Path.GetFileName(e.Path), n, StringComparison.CurrentCultureIgnoreCase))
					{
						found = true;
						break;
					}
				}
				Assert.IsTrue(found);

				Assert.IsTrue(System.IO.Directory.Exists(
					System.IO.Path.Combine(
						Utility.FindMyGitCloneDir(),
						"TestData",
						n,
						".git")
					));
			}
		}

	}

}
