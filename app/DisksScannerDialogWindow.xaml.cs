using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
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
        //public Observable<bool>


		//public IDisksScanner DisksScanner {get;set;} = null;

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


            //DisksScanner = null;

            //string d = Assembly.GetExecutingAssembly().Location;
            //string pd = System.IO.Path.GetDirectoryName(d);
            //while (!string.IsNullOrEmpty(pd)) {
            //	d = pd;
            //	pd = System.IO.Path.GetDirectoryName(d);
            //}
            //SearchDir.Text = d;
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

        //private void SearchWithEverythingButton_Click(object sender, RoutedEventArgs e)
        //{
        //	DisksScanner = new DisksScannerEverything();
        //	Close();
        //}

        //private void BrowseSearchDirButton_Click(object sender, RoutedEventArgs e)
        //{
        //	var dialog = new CommonOpenFileDialog();
        //	dialog.IsFolderPicker = true;
        //	dialog.InitialDirectory = SearchDir.Text;
        //	dialog.DefaultFileName = SearchDir.Text;
        //	dialog.Title = "Checkouts Overview - Scan Disks...";
        //	dialog.EnsurePathExists = true;
        //	CommonFileDialogResult result = dialog.ShowDialog();
        //	if (result == CommonFileDialogResult.Ok)
        //	{
        //		SearchDir.Text = dialog.FileName;
        //	}
        //}

        //private void SearchFileSystemButton_Click(object sender, RoutedEventArgs e)
        //{
        //	if (!System.IO.Directory.Exists(SearchDir.Text))
        //	{
        //		MessageBox.Show("The specified search directory does not seem to exist. Please, correct your input.");
        //		return;
        //	}

        //	DisksScanner = new DisksScannerFilesystem()
        //	{
        //		Root = SearchDir.Text
        //	};
        //	Close();
        //}
    }
}
