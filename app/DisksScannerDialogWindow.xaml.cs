using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
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
	/// Interaction logic for DisksScannerDialogWindow.xaml
	/// </summary>
	public partial class DisksScannerDialogWindow : Window
	{

        private IDisksScanner DisksScanner { get; set; } = null;
        private Thread ScannerWorker = null;

		public DisksScannerDialogWindow()
		{
			InitializeComponent();

            string se = Properties.Settings.Default.scannerEngine?.ToLowerInvariant() ?? "";
            if (se == "filesystem")
            {
                scannerEngineFilesystem.IsChecked = true;
            }
            else if (se == "everything")
            {
                scannerEngineEverything.IsChecked = true;
            }
            else
            {
                scannerEngineEverything.IsChecked = true;
            }
            scannerRoot.Text = Properties.Settings.Default.scannerRoot;
            scannerIgnore.Text = string.Join(Environment.NewLine, Properties.Settings.Default.scannerIgnorePatterns);
            scannerEntrySubdir.IsChecked = Properties.Settings.Default.scannerEntrySubdir;

            startScanButton.IsEnabled = true;
            stopScanButton.IsEnabled = false;
            scanStatus.Text = "";
        }

        private void EverythingHyperlink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo()
                    {
                        UseShellExecute = true,
                        FileName = "https://www.voidtools.com/"
                    });
            }
            catch { }
        }

        private void BrowseScannerRootButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new FolderPicker();
            dlg.InputPath = scannerRoot.Text;
            dlg.Title = "Select Filesystem Scanner Root...";
            dlg.ForceFileSystem = true;
            if (dlg.ShowDialog() == true)
            {
                scannerRoot.Text = dlg.ResultPath;
            }
        }

        private void startScanButton_Click(object sender, RoutedEventArgs e)
        {
            startScanButton.IsEnabled = false;
            stopScanButton.IsEnabled = false;
            scanStatus.Text = "Starting scan...";

            if (DisksScanner != null)
            {
                startScanButton.IsEnabled = false;
                stopScanButton.IsEnabled = true;
                scanStatus.Text = "Scanner still running?";
                return;
            }

            IDisksScanner scanner = null;
            if (scannerEngineEverything.IsChecked ?? false)
            {
                scanner = new DisksScannerEverything();
            }
            else if (scannerEngineFilesystem.IsChecked ?? false)
            {
                scanner = new DisksScannerFilesystem();
            }
            else
            {
                scanStatus.Text = "ERROR: Scanner type not configured";
                startScanButton.IsEnabled = DisksScanner == null;
                stopScanButton.IsEnabled = DisksScanner != null;
                return;
            }
            scanner.Root = scannerRoot.Text;
            scanner.IgnorePattern = scannerIgnore.Text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            scanner.ScanCheckoutSubdirs = scannerEntrySubdir.IsChecked ?? false;

            scanner.ScanMessage += Scanner_ScanMessage;
            scanner.EntryFound += Scanner_EntryFound;

            DisksScanner = scanner;

            ScannerWorker = new Thread(() =>
            {
                Dispatcher.Invoke(() => {
                    stopScanButton.IsEnabled = true;
                });

                scanner.Scan();

                Dispatcher.Invoke(() => {
                    DisksScanner = null;
                    startScanButton.IsEnabled = true;
                    stopScanButton.IsEnabled = false;
                });
            });
            ScannerWorker.Start();
        }

        private bool Scanner_EntryFound(Entry arg)
        {
            //throw new NotImplementedException();
            return true;
        }

        private void Scanner_ScanMessage(object sender, string e)
        {
            Dispatcher.Invoke(() =>
            {
                scanStatus.Text = e;
            });
        }

        private void stopScanButton_Click(object sender, RoutedEventArgs e)
        {
            startScanButton.IsEnabled = false;
            stopScanButton.IsEnabled = false;
            scanStatus.Text = "Stopping scan...";

            // The following should be async
            Thread worker = ScannerWorker;
            ScannerWorker = new Thread(() =>
            {
                IDisksScanner s = DisksScanner;
                if (s != null)
                {
                    s.AbortScan();
                }
                worker?.Join();

                Dispatcher.Invoke(() => {
                    DisksScanner = null;
                    startScanButton.IsEnabled = true;
                    stopScanButton.IsEnabled = false;
                });
            });
            ScannerWorker.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ScannerWorker != null)
            {
                stopScanButton_Click(null, null);
            }
        }
    }
}
