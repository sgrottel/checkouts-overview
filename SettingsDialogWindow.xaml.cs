using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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
			getGitBinInfo();
		}

		private void GitInfoUpdateTimer_Tick(object sender, EventArgs e)
		{
			gitInfoUpdateTimer.Stop();

			Process p;
			string result;
			string path = gitBin.Text.Trim();
			if (string.IsNullOrEmpty(path))
			{
				p = new Process();
				p.StartInfo.UseShellExecute = false;
				p.StartInfo.RedirectStandardOutput = true;
				p.StartInfo.FileName = "where.exe";
				p.StartInfo.ArgumentList.Clear();
				p.StartInfo.ArgumentList.Add("git.exe");
				p.StartInfo.CreateNoWindow = true;
				p.Start();
				result = p.StandardOutput.ReadToEnd().Trim();
				p.WaitForExit();
				if (System.IO.File.Exists(result))
				{
					path = result;
				}
				if (string.IsNullOrEmpty(path))
				{
					gitBinInfo.Text = "ERROR: Git not found.";
					return;
				}
			}

			if (!System.IO.File.Exists(path))
			{
				gitBinInfo.Text = "ERROR: File does not exist.";
				return;
			}

			p = new Process();
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.FileName = path;
			p.StartInfo.ArgumentList.Clear();
			p.StartInfo.ArgumentList.Add("--version");
			p.StartInfo.CreateNoWindow = true;
			p.Start();
			result = p.StandardOutput.ReadToEnd().Trim();
			p.WaitForExit();

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

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}

		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			Properties.Settings.Default.lastFile = lastFile.Text;
			Properties.Settings.Default.loadOnStart = loadOnStart.IsChecked ?? false;
			Properties.Settings.Default.scanOnStart = scanOnStart.IsChecked ?? false;
			Properties.Settings.Default.updateOnStart = updateOnStart.IsChecked ?? false;
			Properties.Settings.Default.gitClient = gitClient.Text;
			Properties.Settings.Default.gitBin = gitBin.Text;
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
	}
}
