using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

			DataContext = entries;
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
	}
}
