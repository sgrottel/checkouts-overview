using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace SG.Checkouts_Overview
{
	/// <summary>
	/// Interaction logic for SettingsDialogWindow.xaml
	/// </summary>
	public partial class SettingsDialogWindow : Window
	{
		public SettingsDialogWindow()
		{
			InitializeComponent();

			lastFile.Text = Properties.Settings.Default.lastFile;
			loadOnStart.IsChecked = Properties.Settings.Default.loadOnStart;
			scanOnStart.IsChecked = Properties.Settings.Default.scanOnStart;
			updateOnStart.IsChecked = Properties.Settings.Default.updateOnStart;
			gitClient.Text = Properties.Settings.Default.gitClient;
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
			Properties.Settings.Default.Save();
			DialogResult = true;
			Close();
		}
	}
}
