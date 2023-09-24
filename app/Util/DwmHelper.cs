using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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
		internal static bool UseImmersiveDarkMode(IntPtr hWnd, bool enabled)
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
	}
}
