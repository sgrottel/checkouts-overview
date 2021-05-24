using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace SG.Checkouts_Overview
{

	public interface IDisksScanner
	{

		void Scan();

		void AbortScan();

		event Func<Entry, bool> EntryFound;

		event EventHandler<string> ScanMessage;

	}

}
