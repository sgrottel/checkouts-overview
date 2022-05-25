using Microsoft.Win32;
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
		private EntryViewsCollection entries = new EntryViewsCollection();

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
					DisksScannerEverything scanner = new DisksScannerEverything();
					scanner.EntryFound += (Entry e) =>
					{
						foreach (var ke in entries)
						{
							if (string.Equals(ke.Entry.Path, e.Path, StringComparison.CurrentCultureIgnoreCase)) return false;
						}
						Dispatcher.Invoke(() =>
						{
							EntryView ev = new EntryView() { Entry = e };
							entries.Add(ev);
							if (Properties.Settings.Default.updateOnStart)
							{
								ev.Status = evaluator.BeginEvaluate(e, (string s) => { ev.LastMessage = s; });
							}
						});
						return true;
					};
					scanner.Scan();
				}
				catch { }
			}
			if (Properties.Settings.Default.updateOnStart)
			{
				try
				{
					foreach (EntryView entry in entries)
					{
						entry.Status = evaluator.BeginEvaluate(entry.Entry, (string s) => { entry.LastMessage = s; });
					}
				}
				catch { }
			}

		}

		/// <summary>
		/// Deselect all when clicking on empty space
		/// </summary>
		private void EntriesView_MouseDown(object sender, MouseButtonEventArgs e)
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
		private void EntriesView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (EntriesView.SelectedItems.Count > 0)
			{
				OpenClientButton_Click(sender, null);
			}
		}

		private void AddEntryButton_Click(object sender, RoutedEventArgs e)
		{
			Entry entry = new Entry()
			{
				Name = "New Entry"
			};
			entries.Add(new EntryView() { Entry = entry });
			EntriesView.SelectedItem = entry;
		}

		private void DeleteEntriesButton_Click(object sender, RoutedEventArgs e)
		{
			var selEntries = EntriesView.SelectedItems.Cast<EntryView>().ToArray();
			foreach (EntryView entry in selEntries)
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
						ser.Serialize(wrtr,
							Array.ConvertAll<EntryView, Entry>(entries.ToArray(), (EntryView ev) => { return ev.Entry; })
							);
					}
					entries.IsDirty = false;
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

		/// <summary>
		/// If there are unsaved changes, aka the entries are dirty, the user will asked to save those.
		/// </summary>
		/// <returns>
		/// Yes, if the user wants to continue.
		/// </returns>
		private MessageBoxResult AssertChangesSaved()
        {
			if (entries.IsDirty)
            {
				SaveConfirmDialogWindow dlg = new SaveConfirmDialogWindow();
				dlg.ShowDialog();
				if (dlg.Result == MessageBoxResult.Yes)
                {
					SaveButton_Click(this, null);
					if (entries.IsDirty)
                    {
						// still dirty? Then, no save happend (error or cancel)
						return MessageBoxResult.Cancel;
					}
				} else if (dlg.Result == MessageBoxResult.Cancel)
                {
					return MessageBoxResult.Cancel;
                }
            }

			return MessageBoxResult.Yes;
        }

		private void LoadButton_Click(object sender, RoutedEventArgs e)
		{
			if (AssertChangesSaved() != MessageBoxResult.Yes) return;

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
					entries.Clear();
					foreach (Entry e in es)
					{
						entries.Add(new EntryView() { Entry = e });
					}
					entries.IsDirty = false;
				}
			}
		}

		private void ScanDisksButton_Click(object sender, RoutedEventArgs e)
		{
			DisksScannerDialogWindow dlg = new DisksScannerDialogWindow();
			dlg.Owner = this;
			dlg.CurrentEntries = entries;
			dlg.ShowDialog();
		}

		private void EntryTypeButton_Click(object sender, RoutedEventArgs e)
		{
			EntryView entry = (sender as Button)?.DataContext as EntryView;
			if (entry == null) return;
			evaluator.CheckType(entry.Entry, (string s) => { entry.LastMessage = s; });
		}

		private void EntryBrowsePathButton_Click(object sender, RoutedEventArgs e)
		{
			EntryView entry = (sender as Button)?.DataContext as EntryView;
			if (entry == null || entry.Entry == null) return;

			var dlg = new FolderPicker();
			dlg.InputPath = entry.Entry.Path;
			dlg.Title = entry.Name;
			dlg.ForceFileSystem = true;
			if (dlg.ShowDialog() == true)
			{
				entry.Entry.Path = dlg.ResultPath;
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (AssertChangesSaved() != MessageBoxResult.Yes)
			{
				e.Cancel = true;
				return;
			}

			evaluator.Shutdown();
		}

		private void UpdateButton_Click(object sender, RoutedEventArgs e)
		{

			var sel = EntriesView.SelectedItems.Cast<EntryView>().ToList();
			if (sel.Count == 0)
			{
				sel.AddRange(entries);
			}
			foreach (EntryView entry in sel)
			{
				EntryStatus es = evaluator.BeginEvaluate(entry.Entry, (string s) => { entry.LastMessage = s; });
				if (es != null) entry.Status = es;
			}
		}

		private void ExploreButton_Click(object sender, RoutedEventArgs e)
		{
			foreach (EntryView entry in EntriesView.SelectedItems.Cast<EntryView>())
			{
				if (entry == null || entry.Entry == null) continue;
				Process.Start("explorer.exe", entry.Entry.Path);
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

			foreach (EntryView entry in EntriesView.SelectedItems.Cast<EntryView>())
			{
				if (entry == null || entry.Entry == null) continue;
				Process p = new Process();
				p.StartInfo.FileName = gc;
				p.StartInfo.ArgumentList.Clear();
				p.StartInfo.ArgumentList.Add(entry.Entry.Path);
				p.Start();
			}
		}

		private void sortEntries(Action<List<EntryView>> sorter)
		{
			int p = 0;

			List<EntryView> sel = EntriesView.SelectedItems.Cast<EntryView>().ToList();
			if (sel == null || sel.Count <= 0)
			{
				sel = entries.ToList();
			}
			else
			{
				p = entries.Count;
				foreach (EntryView a in sel) p= Math.Min(p, entries.IndexOf(a));
			}

			sorter(sel);

			foreach (EntryView a in sel)
			{
				entries.Remove(a);
				entries.Insert(p++, a);
			}
		}

		private void SortByNameButton_Click(object sender, RoutedEventArgs e)
		{
			sortEntries((List<EntryView> te) =>
			{
				te.Sort((EntryView a, EntryView b) => { return string.Compare(a.Entry.Name, b.Entry.Name); });
			});
		}

		private void SortByPathButton_Click(object sender, RoutedEventArgs e)
		{
			sortEntries((List<EntryView> te) =>
			{
				te.Sort((EntryView a, EntryView b) => { return string.Compare(a.Entry.Path, b.Entry.Path); });
			});
		}

		private void SortReverseButton_Click(object sender, RoutedEventArgs e)
		{
			sortEntries((List<EntryView> te) =>
			{
				te.Reverse();
			});
		}

		private void SortMoveUpButton_Click(object sender, RoutedEventArgs e)
		{
			List<EntryView> sel = EntriesView.SelectedItems.Cast<EntryView>().ToList();
			if (sel == null || sel.Count <= 0) return;

			sel.Sort((EntryView a, EntryView b) => { return entries.IndexOf(a) - entries.IndexOf(b); });

			int minIdx = entries.Count;
			foreach (EntryView entry in sel) minIdx = Math.Min(minIdx, entries.IndexOf(entry));
			minIdx = Math.Max(0, minIdx - 1);

			foreach (EntryView a in sel)
			{
				entries.Remove(a);
				entries.Insert(minIdx++, a);
			}
		}

		private void SortMoveTopButton_Click(object sender, RoutedEventArgs e)
		{
			List<EntryView> sel = EntriesView.SelectedItems.Cast<EntryView>().ToList();
			if (sel == null || sel.Count <= 0) return;

			sel.Sort((EntryView a, EntryView b) => { return entries.IndexOf(a) - entries.IndexOf(b); });

			int minIdx = 0;
			foreach (EntryView a in sel)
			{
				entries.Remove(a);
				entries.Insert(minIdx++, a);
			}
		}

		private void SortMoveDownButton_Click(object sender, RoutedEventArgs e)
		{
			List<EntryView> sel = EntriesView.SelectedItems.Cast<EntryView>().ToList();
			if (sel == null || sel.Count <= 0) return;

			sel.Sort((EntryView a, EntryView b) => { return entries.IndexOf(a) - entries.IndexOf(b); });

			int maxIdx = 0;
			foreach (EntryView entry in sel) maxIdx = Math.Max(maxIdx, entries.IndexOf(entry));
			maxIdx++;

			foreach (EntryView a in sel)
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
			List<EntryView> sel = EntriesView.SelectedItems.Cast<EntryView>().ToList();
			if (sel == null || sel.Count <= 0) return;

			sel.Sort((EntryView a, EntryView b) => { return entries.IndexOf(a) - entries.IndexOf(b); });

			foreach (EntryView a in sel)
			{
				entries.Remove(a);
				entries.Add(a);
			}
		}

		private void SortByLastChangedDateButton_Click(object sender, RoutedEventArgs e)
		{
			sortEntries((List<EntryView> te) =>
			{
				Dictionary<EntryView, DateTime> cd = new Dictionary<EntryView, DateTime>();
				foreach (EntryView e in te)
				{
					cd[e] = getChangedDate(e.Entry.Path);
					// Debug.WriteLine("{0}  ==>  {1}", e.Path, cd[e]);
				}
				te.Sort((EntryView a, EntryView b) => { return DateTime.Compare(cd[b], cd[a]); });
			});
		}

		private void SortByLastCommitDateButton_Click(object sender, RoutedEventArgs e)
		{
			sortEntries((List<EntryView> te) =>
			{
				Dictionary<EntryView, DateTime> cd = new Dictionary<EntryView, DateTime>();
				foreach (EntryView e in te)
				{
					cd[e] = evaluator.GetCommitDate(e.Entry);
				}
				te.Sort((EntryView a, EntryView b) => { return DateTime.Compare(cd[b], cd[a]); });
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
				if (string.Equals(sdi.Name, ".git", StringComparison.CurrentCultureIgnoreCase)) continue;
				DateTime sdd = getChangedDate(sdi);
				if (sdd > d)
					d = sdd;
			}
			return d;
		}

		private void Window_PreviewDragOver(object sender, DragEventArgs e)
		{
			string[] fdo = (e.Data.GetData(DataFormats.FileDrop) as string[]).Where((string p) => { return System.IO.Directory.Exists(p); }).ToArray();
			if (fdo == null || fdo.Length <= 0)
			{
				e.Effects = DragDropEffects.None;
				e.Handled = true;
				return;
			}
			e.Effects = DragDropEffects.Link;
			e.Handled = true;
		}

		private void Window_Drop(object sender, DragEventArgs e)
		{
			string[] fdo = (e.Data.GetData(DataFormats.FileDrop) as string[]).Where((string p) => { return System.IO.Directory.Exists(p); }).ToArray();
			if (fdo == null || fdo.Length <= 0)
			{
				e.Handled = true;
				return;
			}
			List<EntryView> toSelect = new List<EntryView>();
			foreach (string p in fdo)
			{
				bool found = false;
				foreach (EntryView ee in entries)
				{
					if (string.Equals(ee.Entry.Path, p, StringComparison.CurrentCultureIgnoreCase))
					{
						toSelect.Add(ee);
						found = true;
						break;
					}
				}
				if (!found)
				{
					EntryView ne = new EntryView()
					{
						Entry = new Entry()
						{
							Name = System.IO.Path.GetFileName(p),
							Path = p
						}
					};
					evaluator.CheckType(ne.Entry, (string s) => { ne.LastMessage = s; });
					toSelect.Add(ne);
					entries.Add(ne);
				}
			}

			if (toSelect.Count > 0)
			{
				EntriesView.SelectedItem = null;
				foreach (EntryView se in toSelect)
				{
					se.IsSelected = true;
				}
			}

			e.Handled = true;
		}

		private void OnlineHelpButton_Click(object sender, RoutedEventArgs e)
		{
			const string knownUrl = "https://github.com/sgrottel/checkouts-overview/blob/main/doc/manual.md";
			const string fallbackUrl = "https://go.grottel.net/checkouts-overview-manual";

			System.Net.Http.HttpClient hc = new System.Net.Http.HttpClient();
			try
			{
				var call = hc.GetStringAsync(knownUrl);
				call.Wait();
				string page = call.Result;
				if (page.Contains("Checkouts Overview - Usage Manual"))
				{
					try
					{
						System.Diagnostics.Process.Start(
							new System.Diagnostics.ProcessStartInfo()
							{
								UseShellExecute = true,
								FileName = knownUrl
							});
						return;
					}
					catch { }
				}
			}
			catch { }
			try
			{
				System.Diagnostics.Process.Start(
					new System.Diagnostics.ProcessStartInfo()
					{
						UseShellExecute = true,
						FileName = fallbackUrl
					});
				return;
			}
			catch { }

		}
	}
}
