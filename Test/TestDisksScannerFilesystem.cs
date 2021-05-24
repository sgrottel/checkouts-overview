using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
			string tdp = System.IO.Path.Combine(Utility.FindMyGit(), "TestData");

			List<Entry> es = new List<Entry>();
			DisksScannerFilesystem dsfs = new DisksScannerFilesystem()
			{
				Root = tdp
			};
			dsfs.EntryFound += (Entry e) => { es.Add(e); return true; };

			dsfs.Scan();

			Assert.IsTrue(es.Count >= 8); // 8 test repos "a"-"h", and optionally the one initialization repo "i"

			for (char sd = 'a'; sd <= 'h'; sd++)
			{
				bool found = false;
				foreach (Entry e in es)
				{
					if (string.Equals(System.IO.Path.GetFileName(e.Path), sd.ToString(), StringComparison.CurrentCultureIgnoreCase))
					{
						found = true;
						break;
					}
				}
				Assert.IsTrue(found);
			}
		}

	}

}
