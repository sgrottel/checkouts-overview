﻿using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace SG.Checkouts_Overview
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{

		private EntryEvaluator evaluator;

		public MainWindow()
		{
			InitializeComponent();

			evaluator = new EntryEvaluator();
			evaluator.Start();

			ObservableCollection<Entry> entries = new ObservableCollection<Entry>();
			DataContext = entries;

			if (Properties.Settings.Default.loadOnStart)
			{
				try
				{
					loadFile(Properties.Settings.Default.lastFile);
				}
				catch { }
			}
			if (Properties.Settings.Default.scanOnStart)
			{
				try
				{
					scanDisks();
				}
				catch { }
			}
			if (Properties.Settings.Default.updateOnStart)
			{
				try
				{
					foreach (Entry entry in entries)
					{
						evaluator.BeginEvaluate(entry);
					}
				}
				catch { }
			}

		}

		/// <summary>
		/// Deselect all when clicking on empty space
		/// </summary>
		private void Entries_MouseDown(object sender, MouseButtonEventArgs e)
		{
			HitTestResult r = VisualTreeHelper.HitTest(this, e.GetPosition(this));
			if (r.VisualHit.GetType() != typeof(ListBoxItem))
				((ListView)sender).UnselectAll();
		}

		private void AddEntryButton_Click(object sender, RoutedEventArgs e)
		{
			var entries = (ObservableCollection<Entry>)DataContext;
			Entry entry = new Entry()
			{
				Name = "New Entry"
			};
			entries.Add(entry);
			Entries.SelectedItem = entry;
		}

		private void DeleteEntriesButton_Click(object sender, RoutedEventArgs e)
		{
			var entries = (ObservableCollection<Entry>)DataContext;
			var selEntries = Entries.SelectedItems.Cast<Entry>().ToArray();
			foreach (Entry entry in selEntries)
			{
				entries.Remove(entry);
			}
		}

		private void SettingsButton_Click(object sender, RoutedEventArgs e)
		{
			SettingsDialogWindow dlg = new SettingsDialogWindow();
			dlg.ShowDialog();
		}

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.Filter = "Xml files|*.xml|All files|*.*";
			dlg.FileName = Properties.Settings.Default.lastFile;
			dlg.DefaultExt = "xml";
			dlg.Title = "Checkouts Overview Save ...";
			dlg.CheckPathExists = true;
			dlg.InitialDirectory = System.IO.Path.GetDirectoryName(dlg.FileName);
			dlg.OverwritePrompt = true;
			dlg.RestoreDirectory = false;

			if (dlg.ShowDialog() ?? false)
			{
				try
				{
					using (TextWriter wrtr = new StreamWriter(dlg.FileName))
					{
						XmlSerializer ser = new XmlSerializer(typeof(Entry[]));
						ser.Serialize(wrtr, ((ObservableCollection<Entry>)DataContext).ToArray());
					}
					Properties.Settings.Default.lastFile = dlg.FileName;
					Properties.Settings.Default.Save();
				}
				catch(Exception ex)
				{
					MessageBox.Show("Failed to save: " + ex, dlg.Title, MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		private void LoadButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "Xml files|*.xml|All files|*.*";
			dlg.FileName = Properties.Settings.Default.lastFile;
			dlg.Title = "Checkouts Overview Load ...";
			dlg.CheckFileExists = true;
			dlg.InitialDirectory = System.IO.Path.GetDirectoryName(dlg.FileName);
			dlg.RestoreDirectory = false;

			if (dlg.ShowDialog() ?? false)
			{
				try
				{
					loadFile(dlg.FileName);
					Properties.Settings.Default.lastFile = dlg.FileName;
					Properties.Settings.Default.Save();
				}
				catch (Exception ex)
				{
					MessageBox.Show("Failed to load: " + ex, dlg.Title, MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		private void loadFile(string filename)
		{
			using (TextReader rdr = new StreamReader(filename))
			{
				XmlSerializer ser = new XmlSerializer(typeof(Entry[]));
				Entry[] es = ser.Deserialize(rdr) as Entry[];
				if (es != null && es.Length > 0)
				{
					var entries = (ObservableCollection<Entry>)DataContext;
					entries.Clear();
					foreach (Entry e in es)
					{
						entries.Add(e);
					}
				}
			}
		}

		private void scanDisks()
		{
			var entries = (ObservableCollection<Entry>)DataContext;
			string everythingSearch =
				System.IO.Path.Combine(
					System.IO.Path.GetDirectoryName(
						System.Reflection.Assembly.GetExecutingAssembly().Location),
					"es.exe");
			if (!System.IO.File.Exists(everythingSearch))
			{
				throw new InvalidOperationException("Unable to find `es.exe` search utility.");
			}

			Process p = new Process();

			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.FileName = everythingSearch;
			p.StartInfo.ArgumentList.Clear();
			// -r "^.git$" -ww /ad
			p.StartInfo.ArgumentList.Add("-r");
			p.StartInfo.ArgumentList.Add("^.git$");
			p.StartInfo.ArgumentList.Add("-ww");
			p.StartInfo.ArgumentList.Add("/ad");

			p.Start();

			var result = p.StandardOutput.ReadToEnd().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
			p.WaitForExit();

			if (result == null || result.Length <= 0) return;

			foreach (string dgit in result)
			{
				string d = System.IO.Path.GetDirectoryName(dgit);
				if (entries.FirstOrDefault((Entry e) => { return string.Equals(e.Path, d, StringComparison.CurrentCultureIgnoreCase); }) != null) continue; // entry known

				entries.Add(new Entry()
				{
					Name = System.IO.Path.GetFileName(d),
					Path = d,
					Type = "git"
				});
			}
		}

		private void ScanDisksButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				scanDisks();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Failed to scan disks: " + ex, "Checkouts Overview Scan Disks", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void EntryTypeButton_Click(object sender, RoutedEventArgs e)
		{
			Entry entry = (sender as Button)?.DataContext as Entry;
			if (entry == null) return;
			evaluator.CheckType(entry);
		}

		private void EntryBrowsePathButton_Click(object sender, RoutedEventArgs e)
		{
			Entry entry = (sender as Button)?.DataContext as Entry;
			if (entry == null) return;

			var dlg = new CommonOpenFileDialog();
			dlg.IsFolderPicker = true;
			dlg.InitialDirectory = entry.Path;
			dlg.DefaultFileName = entry.Path;
			dlg.Title = "";
			dlg.Multiselect = false;

			if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
			{
				entry.Path = dlg.FileName;
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			evaluator.Shutdown();
		}

		private void UpdateButton_Click(object sender, RoutedEventArgs e)
		{
			var entries = Entries.SelectedItems.Cast<Entry>().ToList();
			if (entries.Count == 0)
			{
				entries.AddRange((ObservableCollection<Entry>)DataContext);
			}
			foreach (Entry entry in entries)
			{
				evaluator.BeginEvaluate(entry);
			}
		}
	}
}
