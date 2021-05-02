using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

		public MainWindow()
		{
			InitializeComponent();

			ObservableCollection<Entry> entries = new ObservableCollection<Entry>();
			DataContext = entries;

			entries.Add(new Entry()
			{
				Name = "Checkouts-Overview",
				Path = @"C:\dev\Checkouts-Overview",
				Type = "git"
			});
			entries.Add(new Entry()
			{
				Name = "GetGallery",
				Path = @"C:\dev\GetGallery",
				Type = "git"
			});
			entries.Add(new Entry()
			{
				Name = "$livil",
				Path = @"C:\dev\livil",
				Type = "git",
				LastMessage = "Just as a test."
			});

			if (Properties.Settings.Default.loadOnStart)
			{
				try
				{
					loadFile(Properties.Settings.Default.lastFile);
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
	}
}
