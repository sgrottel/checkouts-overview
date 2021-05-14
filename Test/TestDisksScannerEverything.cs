using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SG.Checkouts_Overview.Test
{
	[TestClass]
	public class TestDisksScannerEverything
	{

		[TestMethod]
		public void Invoke()
		{
			DisksScannerEverything ds = new DisksScannerEverything();
			ds.Entries = new ObservableCollection<Entry>();
			ds.Dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
			ds.Scan();
			// assuming "Everything" might not be installed, the whole thing should just not crash.
		}
	}
}
