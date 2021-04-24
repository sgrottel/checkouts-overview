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

			List<Entry> entries = new List<Entry>();
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
				Type = "git"
			});

			DataContext = entries;
		}

	}
}
