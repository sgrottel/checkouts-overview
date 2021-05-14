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

			DisksScannerFilesystem dsfs = new DisksScannerFilesystem()
			{
				Entries = new System.Collections.ObjectModel.Collection<Entry>(),
				Dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher,
				Root = tdp
			};

			dsfs.Scan();

			Assert.IsTrue(dsfs.Entries.Count >= 8); // 8 test repos "a"-"h", and optionally the one initialization repo "i"

			for (char sd = 'a'; sd <= 'h'; sd++)
			{
				bool found = false;
				foreach (Entry e in dsfs.Entries)
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
