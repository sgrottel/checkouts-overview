using Microsoft.Win32;
using SG.Checkouts_Overview.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SG.Checkouts_Overview
{
	/// <summary>
	/// Interaction logic for SettingsDialogWindow.xaml
	/// </summary>
	public partial class SettingsDialogWindow : Window
	{
		public DispatcherTimer gitInfoUpdateTimer;

		public SettingsDialogWindow()
		{
			InitializeComponent();

			gitInfoUpdateTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.5) };
			gitInfoUpdateTimer.Tick += GitInfoUpdateTimer_Tick;

			lastFile.Text = Properties.Settings.Default.lastFile;
			loadOnStart.IsChecked = Properties.Settings.Default.loadOnStart;
			scanOnStart.IsChecked = Properties.Settings.Default.scanOnStart;
			updateOnStart.IsChecked = Properties.Settings.Default.updateOnStart;
			gitClient.Text = Properties.Settings.Default.gitClient;
			gitBin.Text = Properties.Settings.Default.gitBin;
			gitMain.Text = Properties.Settings.Default.gitDefaultBranches;
			string se = Properties.Settings.Default.scannerEngine?.ToLowerInvariant() ?? "";
			if (se == "filesystem")
			{
				scannerEngineFilesystem.IsChecked = true;
			} else if (se == "everything")
			{
				scannerEngineEverything.IsChecked = true;
			} else
			{
				scannerEngineEverything.IsChecked = true;
			}
			scannerRoot.Text = Properties.Settings.Default.scannerRoot;
			scannerIgnore.Text = Properties.Settings.Default.scannerIgnorePatterns;
			scannerEntrySubdir.IsChecked = Properties.Settings.Default.scannerEntrySubdir;
			fetchOnUpdate.IsChecked = Properties.Settings.Default.gitFetchAllOnUpdate;

			int i = Properties.Settings.Default.iconSize;
			if (i <= 0) i = (int)new EntryViewsCollection().IconSize;
			iconSize.Text = i.ToString();
			i = Properties.Settings.Default.iconMargin;
			if (i <= 0) i = (int)new EntryViewsCollection().IconMargin;
			iconMargin.Text = i.ToString();

			getGitBinInfo();
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);
			Util.DwmHelper.UseImmersiveDarkMode((PresentationSource.FromVisual(this) as HwndSource)?.Handle ?? IntPtr.Zero, true);
		}

		private void GitInfoUpdateTimer_Tick(object? sender, EventArgs e)
		{
			gitInfoUpdateTimer.Stop();

			string path = gitBin.Text.Trim();
			Git git = new(path);
			string result = git.Invoke(new[] { "--version" }).StdOut;

			if (!string.Equals(path, git.Path))
			{
				result = $"{git.Path}\n{result}";
			}

			gitBinInfo.Text = result;

		}

		private void gitBin_TextChanged(object sender, TextChangedEventArgs e)
		{
			getGitBinInfo();
		}

		private void getGitBinInfo()
		{
			gitInfoUpdateTimer?.Stop();
			gitInfoUpdateTimer?.Start();
		}

		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			Properties.Settings.Default.lastFile = lastFile.Text;
			Properties.Settings.Default.loadOnStart = loadOnStart.IsChecked ?? false;
			Properties.Settings.Default.scanOnStart = scanOnStart.IsChecked ?? false;
			Properties.Settings.Default.updateOnStart = updateOnStart.IsChecked ?? false;
			Properties.Settings.Default.gitClient = gitClient.Text;
			Properties.Settings.Default.gitBin = gitBin.Text;
			Properties.Settings.Default.gitDefaultBranches = gitMain.Text;
			if (scannerEngineEverything.IsChecked??false)
			{
				Properties.Settings.Default.scannerEngine = "everything";
			} else if (scannerEngineFilesystem.IsChecked??false)
			{
				Properties.Settings.Default.scannerEngine = "filesystem";
			} else
			{
				Properties.Settings.Default.scannerEngine = null;
			}
			Properties.Settings.Default.scannerRoot = scannerRoot.Text;
			Properties.Settings.Default.scannerIgnorePatterns = scannerIgnore.Text;
			Properties.Settings.Default.scannerEntrySubdir = scannerEntrySubdir.IsChecked ?? false;
			Properties.Settings.Default.gitFetchAllOnUpdate = fetchOnUpdate.IsChecked ?? false;

			int defI = (int)new EntryViewsCollection().IconSize;
			if (int.TryParse(iconSize.Text, out int i))
			{
				if (i < 0) i = 0;
				if (i == defI) i = 0;
				Properties.Settings.Default.iconSize = i;
			}
			defI = (int)new EntryViewsCollection().IconMargin;
			if (int.TryParse(iconMargin.Text, out i))
			{
				if (i < 0) i = 0;
				if (i == defI) i = 0;
				Properties.Settings.Default.iconMargin = i;
			}

			Properties.Settings.Default.Save();
			DialogResult = true;
			Close();
		}

		private void BrowseLastFileButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dlg = MainWindow.CreateOpenProjectFileDialog();
			dlg.Title = "Checkouts Overview Select Last File ...";
			dlg.FileName = lastFile.Text;
			dlg.RestoreDirectory = true;
			if (dlg.ShowDialog() ?? false)
			{
				lastFile.Text = dlg.FileName;
			}
		}

		private void BrowseGitBinButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "Executables|*.exe;*.com|All files|*.*";
			dlg.FileName = gitBin.Text;
			dlg.Title = "Checkouts Overview Select Git ...";
			dlg.CheckFileExists = true;
			dlg.InitialDirectory = System.IO.Path.GetDirectoryName(dlg.FileName);
			dlg.RestoreDirectory = true;
			if (dlg.ShowDialog() ?? false)
			{
				gitBin.Text = dlg.FileName;
			}
		}

		private void BrowseGitUIClientButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "Executables|*.exe;*.com|All files|*.*";
			dlg.FileName = gitClient.Text;
			dlg.Title = "Checkouts Overview Select Git UI Client ...";
			dlg.CheckFileExists = true;
			dlg.InitialDirectory = System.IO.Path.GetDirectoryName(dlg.FileName);
			dlg.RestoreDirectory = true;
			if (dlg.ShowDialog() ?? false)
			{
				gitClient.Text = dlg.FileName;
			}
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

		private void PreviewTextInputNumberOnly(object sender, TextCompositionEventArgs e)
		{
			e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
		}
	}
}
