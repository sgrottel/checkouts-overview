using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Media;

namespace SG.Checkouts_Overview.Util
{
	internal static class DwmHelper
	{

		#region Immersive Dark Mode -- for Window Decorations

		[DllImport("dwmapi.dll")]
		private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

		private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
		private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

		/// <summary>
		/// Switches on/off Immersive Dark Mode, aka dark windows title bars from the OS
		/// </summary>
		/// <remarks>
		/// https://www.meziantou.net/detecting-dark-and-light-themes-in-a-wpf-application.htm#updating-the-title-b-2cbb0b
		/// </remarks>
		/// <param name="hWnd">Handle to the Window</param>
		/// <param name="enabled">True for Dark, False for Standard</param>
		/// <returns></returns>
		private static bool UseImmersiveDarkMode(IntPtr hWnd, bool enabled)
		{
			if (hWnd == IntPtr.Zero) return false;

			if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
			{
				var attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
				if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18985))
				{
					attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
				}

				int useImmersiveDarkMode = enabled ? 1 : 0;
				return DwmSetWindowAttribute(hWnd, attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
			}

			return false;
		}

		#endregion

		#region DWMWA Colors
		private const int DWMWA_BORDER_COLOR = 34;
		private const int DWMWA_CAPTION_COLOR = 35;
		private const int DWMWA_TEXT_COLOR = 36;

		private static readonly int ColorLight = initStyleColor("Light");
		private static readonly int ColorGrey = initStyleColor("Grey");
		private static readonly int ColorDark = initStyleColor("Dark");
		private static readonly int ColorDarker = initStyleColor("Darker");

		private static int initStyleColor(string name)
		{
			object? o = Application.Current.FindResource(name);
			if (o is SolidColorBrush brush)
			{
				return System.Drawing.ColorTranslator.ToWin32(
					System.Drawing.Color.FromArgb(
						brush.Color.R,
						brush.Color.G,
						brush.Color.B));
			}

			return 0;
		}

		private static void SetCaptionColor(IntPtr hWnd, bool focussed)
		{
			if (hWnd == IntPtr.Zero) return;

			int fg = focussed ? ColorLight : ColorGrey;
			int bg = focussed ? ColorDark : ColorDarker;

			DwmSetWindowAttribute(hWnd, DWMWA_BORDER_COLOR, ref bg, sizeof(int));
			DwmSetWindowAttribute(hWnd, DWMWA_CAPTION_COLOR, ref bg, sizeof(int));
			DwmSetWindowAttribute(hWnd, DWMWA_TEXT_COLOR, ref fg, sizeof(int));
		}
		#endregion

		internal static bool UseDarkWindowDecorations(System.Windows.Window wnd, bool enabled)
		{
			IntPtr hWnd = (PresentationSource.FromVisual(wnd) as HwndSource)?.Handle ?? IntPtr.Zero;
			if (hWnd == IntPtr.Zero) return false;

			bool suc = UseImmersiveDarkMode(hWnd, enabled);
			if (suc && enabled)
			{
				SetCaptionColor(hWnd, true);
				wnd.Activated += (object? _, EventArgs __) => SetCaptionColor(hWnd, true);
				wnd.Deactivated += (object? _, EventArgs __) => SetCaptionColor(hWnd, false);
			}

			return suc;
		}

	}
}
