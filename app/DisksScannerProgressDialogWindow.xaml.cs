using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SG.Checkouts_Overview
{
	/// <summary>
	/// Interaction logic for DisksScannerProgressDialogWindow.xaml
	/// </summary>
	public partial class DisksScannerProgressDialogWindow : Window
	{
		public IDisksScanner DisksScanner {
			get {
				lock (disksScannerLock)
				{
					return disksScanner;
				}
			}
			set {
				lock (disksScannerLock)
				{
					disksScanner = value;
				}
			}
		}
		private Thread worker = null;
		private IDisksScanner disksScanner = null;
		private object disksScannerLock = new object();

		public DisksScannerProgressDialogWindow()
		{
			InitializeComponent();
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			lock (disksScannerLock)
			{
				if (disksScanner != null)
				{
					e.Cancel = true;
					disksScanner.AbortScan();
				}
			}
		}

		internal void Start()
		{
			lock (disksScannerLock)
			{
				if (disksScanner != null)
				{
					worker = new Thread(work);
					worker.Start();
				}
			}
		}

		void work()
		{
			IDisksScanner ds = DisksScanner;
			// TODO: Fix me
			//ds.Dispatcher = Dispatcher;
			if (ds == null) return;
			ds.ScanMessage += Ds_ScanMessage;
			try
			{
				ds.Scan();
			}
			catch(Exception ex)
			{
				Dispatcher.Invoke(new Action(() => {
					StatusText.Content = "Failed to scan: " + ex;
					AbortButton.Content = "Close";
				}));
				DisksScanner = null;
				return;
			}

			DisksScanner = null;
			try
			{
				Dispatcher.Invoke(new Action(() =>
				{
					StatusText.Content += " Closing in five seconds.";
					AbortButton.Content = "Close";
				}));
				for (int i = 0; i < 5000; i += 25)
					Thread.Sleep(25);
				Dispatcher.Invoke(new Action(() => { Close(); }));
			} catch { }
		}

		private void Ds_ScanMessage(object sender, string e)
		{
			Dispatcher.Invoke(new Action<string>((string s) => {
				StatusText.Content = s;
			}), new object[] { e });
		}

		private void AbortButton_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
