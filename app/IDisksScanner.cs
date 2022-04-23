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
		/// Blocking
		/// </summary>
		void Scan();

		/// <summary>
		/// Tells the scanner to abort as quickly as possible
		/// </summary>
		void AbortScan();

		string Root { get; set; }

		string[] IgnorePattern { get; set; }

		bool ScanCheckoutSubdirs { get; set; }

		event Func<Entry, bool> EntryFound;

		event EventHandler<string> ScanMessage;

	}

}
