using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        public class MarkableEntry: INotifyPropertyChanged
        {
            private Entry entry = null;
            private bool marked = true;

            public event PropertyChangedEventHandler PropertyChanged;

            public Entry Entry
            {
                get { return entry; }
                set
                {
                    if (entry != value)
                    {
                        if (entry != null)
                        {
                            entry.PropertyChanged -= Entry_PropertyChanged;
                        }
                        entry = value;
                        if (entry != null)
                        {
                            entry.PropertyChanged += Entry_PropertyChanged;
                        }
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Entry)));
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Path)));
                    }
                }
            }

            private void Entry_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e == null || string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(Path))
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Path)));
                }
            }

            public string Path
            {
                get { return entry?.Path ?? ""; }
            }

            public bool Marked
            {
                get { return marked; }
                set
                {
                    if (marked != value)
                    {
                        marked = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Marked)));
                    }
                }
            }

        }

        private IDisksScanner DisksScanner { get; set; } = null;
        private Thread ScannerWorker = null;

        public ObservableCollection<MarkableEntry> Entries { get; set; } = new ObservableCollection<MarkableEntry>();

        /// <summary>
        /// Entries currently stored in the main window
        /// </summary>
        internal EntryViewsCollection CurrentEntries { get; set; } = null;

        public DisksScannerDialogWindow()
		{
			InitializeComponent();
            DataContext = Entries;

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
            Entries.Clear();

            ScannerWorker = new Thread(() =>
            {
                Dispatcher.Invoke(() => {
                    stopScanButton.IsEnabled = true;
                });

                scanner.Scan();

                Dispatcher.Invoke(() => {

                    FinalFilterEntries();

                    DisksScanner = null;
                    startScanButton.IsEnabled = true;
                    stopScanButton.IsEnabled = false;
                });
            });
            ScannerWorker.Start();
        }

        private bool Scanner_EntryFound(Entry entry)
        {
            if (CurrentEntries != null)
            {
                // TODO: reject entry which is already known
            }

            Dispatcher.Invoke(() =>
            {
                Entries.Add(new MarkableEntry() { Entry = entry });
            });
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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentEntries != null)
            {
                // TODO: Add marked entries to the current entries collection
            }
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Filters list of entries after detection
        /// </summary>
        /// <example>
        /// Search with everything is so fast, we don't need to filter (optimize search) while running.
        /// Instead, we filter the found items in the end, as soon as we got all of them.
        /// </example>
        private void FinalFilterEntries()
        {
            // TODO
            //throw new NotImplementedException();
        }
    }
}
