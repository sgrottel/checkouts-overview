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

		/// <summary>
		/// This is the reference to the live collection of the application.
		/// Don't mess around, unless it's the real result.
		/// </summary>
		Collection<Entry> Entries { get; set; }

		/// <summary>
		/// Use this dispatcher to write to Entries
		/// </summary>
		Dispatcher Dispatcher { get; set; }

		void Scan();

		void AbortScan();

		event EventHandler<string> ScanMessage;

	}

}
