using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SG.Checkouts_Overview.Test
{
	internal static class Utility
	{
		static internal string FindMyGit()
		{
			string p = Assembly.GetExecutingAssembly().Location;
			while (p.Length > 2 && !System.IO.Directory.Exists(System.IO.Path.Combine(p, ".git"))) p = System.IO.Path.GetDirectoryName(p);
			return p;
		}
	}
}
