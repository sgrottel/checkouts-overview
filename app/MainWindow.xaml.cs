using Microsoft.Win32;
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

			if (Properties.Settings.Default.upgradeSettings)
			{
				Properties.Settings.Default.Upgrade();
				Properties.Settings.Default.upgradeSettings = false;
				Properties.Settings.Default.Save();
			}

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
					DisksScannerEverything scanner = new DisksScannerEverything() { Entries = entries };
					scanner.Scan();
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

		/// <summary>
		/// Item double clicking opens client
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Entries_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (Entries.SelectedItems.Count > 0)
			{
				OpenClientButton_Click(sender, null);
			}
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
			dlg.Owner = this;
			dlg.ShowDialog();
		}

		private void AboutButton_Click(object sender, RoutedEventArgs e)
		{
			AboutDialogWindow dlg = new AboutDialogWindow();
			dlg.Owner = this;
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

		internal static OpenFileDialog CreateOpenProjectFileDialog()
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "Xml files|*.xml|All files|*.*";
			dlg.FileName = Properties.Settings.Default.lastFile;
			dlg.Title = "Checkouts Overview Load ...";
			dlg.CheckFileExists = true;
			dlg.InitialDirectory = System.IO.Path.GetDirectoryName(dlg.FileName);
			dlg.RestoreDirectory = false;
			return dlg;
		}

		private void LoadButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dlg = CreateOpenProjectFileDialog();
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

		private void ScanDisksButton_Click(object sender, RoutedEventArgs e)
		{
			DisksScannerDialogWindow dlg = new DisksScannerDialogWindow();
			dlg.Owner = this;
			dlg.ShowDialog();
			if (dlg.DisksScanner != null)
			{
				dlg.DisksScanner.Entries = (ObservableCollection<Entry>)DataContext;
				DisksScannerProgressDialogWindow prgDlg = new DisksScannerProgressDialogWindow();
				prgDlg.Owner = this;
				prgDlg.DisksScanner = dlg.DisksScanner;
				prgDlg.Start();
				prgDlg.ShowDialog();
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

		private void ExploreButton_Click(object sender, RoutedEventArgs e)
		{
			foreach (Entry entry in Entries.SelectedItems.Cast<Entry>())
			{
				Process.Start("explorer.exe", entry.Path);
			}
		}

		private void OpenClientButton_Click(object sender, RoutedEventArgs e)
		{
			string gc = Properties.Settings.Default.gitClient;
			if (!System.IO.File.Exists(gc))
			{
				MessageBox.Show("Please configure Visual Git client in Settings", "Checkouts Overview Open Git...", MessageBoxButton.OK, MessageBoxImage.Error);
				return;

			}

			foreach (Entry entry in Entries.SelectedItems.Cast<Entry>())
			{
				Process p = new Process();
				p.StartInfo.FileName = gc;
				p.StartInfo.ArgumentList.Clear();
				p.StartInfo.ArgumentList.Add(entry.Path);
				p.Start();
			}
		}

		private void sortEntries(Action<List<Entry>> sorter)
		{
			ObservableCollection<Entry> entries = (ObservableCollection<Entry>)DataContext;
			int p = 0;

			List<Entry> sel = Entries.SelectedItems.Cast<Entry>().ToList();
			if (sel == null || sel.Count <= 0)
			{
				sel = entries.ToList();
			}
			else
			{
				p = entries.Count;
				foreach (Entry a in sel) p= Math.Min(p, entries.IndexOf(a));
			}

			sorter(sel);

			foreach (Entry a in sel)
			{
				entries.Remove(a);
				entries.Insert(p++, a);
			}
		}

		private void SortByNameButton_Click(object sender, RoutedEventArgs e)
		{
			sortEntries((List<Entry> te) =>
			{
				te.Sort((Entry a, Entry b) => { return string.Compare(a.Name, b.Name); });
			});
		}

		private void SortByPathButton_Click(object sender, RoutedEventArgs e)
		{
			sortEntries((List<Entry> te) =>
			{
				te.Sort((Entry a, Entry b) => { return string.Compare(a.Path, b.Path); });
			});
		}

		private void SortReverseButton_Click(object sender, RoutedEventArgs e)
		{
			sortEntries((List<Entry> te) =>
			{
				te.Reverse();
			});
		}

		private void SortMoveUpButton_Click(object sender, RoutedEventArgs e)
		{
			List<Entry> sel = Entries.SelectedItems.Cast<Entry>().ToList();
			if (sel == null || sel.Count <= 0) return;
			ObservableCollection<Entry> entries = (ObservableCollection<Entry>)DataContext;

			sel.Sort((Entry a, Entry b) => { return entries.IndexOf(a) - entries.IndexOf(b); });

			int minIdx = entries.Count;
			foreach (Entry entry in sel) minIdx = Math.Min(minIdx, entries.IndexOf(entry));
			minIdx = Math.Max(0, minIdx - 1);

			foreach (Entry a in sel)
			{
				entries.Remove(a);
				entries.Insert(minIdx++, a);
			}
		}

		private void SortMoveTopButton_Click(object sender, RoutedEventArgs e)
		{
			List<Entry> sel = Entries.SelectedItems.Cast<Entry>().ToList();
			if (sel == null || sel.Count <= 0) return;
			ObservableCollection<Entry> entries = (ObservableCollection<Entry>)DataContext;

			sel.Sort((Entry a, Entry b) => { return entries.IndexOf(a) - entries.IndexOf(b); });

			int minIdx = 0;
			foreach (Entry a in sel)
			{
				entries.Remove(a);
				entries.Insert(minIdx++, a);
			}
		}

		private void SortMoveDownButton_Click(object sender, RoutedEventArgs e)
		{
			List<Entry> sel = Entries.SelectedItems.Cast<Entry>().ToList();
			if (sel == null || sel.Count <= 0) return;
			ObservableCollection<Entry> entries = (ObservableCollection<Entry>)DataContext;

			sel.Sort((Entry a, Entry b) => { return entries.IndexOf(a) - entries.IndexOf(b); });

			int maxIdx = 0;
			foreach (Entry entry in sel) maxIdx = Math.Max(maxIdx, entries.IndexOf(entry));
			maxIdx++;

			foreach (Entry a in sel)
			{
				entries.Remove(a);
				if (maxIdx >= entries.Count)
					entries.Add(a);
				else
					entries.Insert(maxIdx, a);
			}
		}

		private void SortMoveBottomButton_Click(object sender, RoutedEventArgs e)
		{
			List<Entry> sel = Entries.SelectedItems.Cast<Entry>().ToList();
			if (sel == null || sel.Count <= 0) return;
			ObservableCollection<Entry> entries = (ObservableCollection<Entry>)DataContext;

			sel.Sort((Entry a, Entry b) => { return entries.IndexOf(a) - entries.IndexOf(b); });

			foreach (Entry a in sel)
			{
				entries.Remove(a);
				entries.Add(a);
			}
		}

		private void SortByLastChangedDateButton_Click(object sender, RoutedEventArgs e)
		{
			sortEntries((List<Entry> te) =>
			{
				Dictionary<Entry, DateTime> cd = new Dictionary<Entry, DateTime>();
				foreach (Entry e in te)
				{
					cd[e] = getChangedDate(e.Path);
					// Debug.WriteLine("{0}  ==>  {1}", e.Path, cd[e]);
				}
				te.Sort((Entry a, Entry b) => { return DateTime.Compare(cd[b], cd[a]); });
			});
		}

		private void SortByLastCommitDateButton_Click(object sender, RoutedEventArgs e)
		{
			sortEntries((List<Entry> te) =>
			{
				Dictionary<Entry, DateTime> cd = new Dictionary<Entry, DateTime>();
				foreach (Entry e in te)
				{
					cd[e] = evaluator.GetCommitDate(e);
				}
				te.Sort((Entry a, Entry b) => { return DateTime.Compare(cd[b], cd[a]); });
			});
		}

		private DateTime getChangedDate(string path)
		{
			return getChangedDate(new DirectoryInfo(path));
		}
		private DateTime getChangedDate(DirectoryInfo di)
		{
			if (!di.Exists) return DateTime.MinValue;
			DateTime d = DateTime.MinValue;
			foreach (FileInfo fi in di.GetFiles())
				if (fi.LastWriteTime > d)
					d = fi.LastWriteTime;
			foreach (DirectoryInfo sdi in di.GetDirectories())
			{
				if (String.Equals(sdi.Name, ".git", StringComparison.CurrentCultureIgnoreCase)) continue;
				DateTime sdd = getChangedDate(sdi);
				if (sdd > d)
					d = sdd;
			}
			return d;
		}

	}
}
