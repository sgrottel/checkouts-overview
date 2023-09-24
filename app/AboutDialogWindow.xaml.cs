using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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

namespace SG.Checkouts_Overview
{
	/// <summary>
	/// Interaction logic for AboutDialogWindow.xaml
	/// </summary>
	public partial class AboutDialogWindow : Window
	{

		public string Description {
			get {
				var ea = System.Reflection.Assembly.GetExecutingAssembly();
				object[] attrs = ea.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), true);
				if (attrs != null && attrs.Length > 0)
				{
					return ((AssemblyDescriptionAttribute)attrs[0]).Description;
				}
				return "";
			}
		}
		public string Version {
			get {
				var ea = System.Reflection.Assembly.GetExecutingAssembly();
				return ea.GetName().Version.ToString();
			}
		}
		public string Copyright {
			get {
				var ea = System.Reflection.Assembly.GetExecutingAssembly();
				object[] attrs = ea.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), true);
				if (attrs != null && attrs.Length > 0)
				{
					return ((AssemblyCopyrightAttribute)attrs[0]).Copyright;
				}
				return "";
			}
		}
		public string Url { get { return "https://go.grottel.net/checkouts-overview"; } }

		public AboutDialogWindow()
		{
			InitializeComponent();
			DataContext = this;
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);
			Util.DwmHelper.UseImmersiveDarkMode((PresentationSource.FromVisual(this) as HwndSource)?.Handle ?? IntPtr.Zero, true);
		}

		private void Hyperlink_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				System.Diagnostics.Process.Start(
					new System.Diagnostics.ProcessStartInfo()
					{
						UseShellExecute = true,
						FileName = Url
					});
			}
			catch { }
		}
	}
}
