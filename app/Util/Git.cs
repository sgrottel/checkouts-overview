using EverythingSearchClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SG.Checkouts_Overview.Util
{
	internal class Git
	{
		public string Path { get; private set; }

		public Git(string? path = null)
		{
			const string gitExe = "git.exe";

			path ??= Properties.Settings.Default.gitBin;

			if (string.IsNullOrWhiteSpace(path)
				|| !File.Exists(path)
				|| !FindExecutable.FindExecutable.IsExecutable(path))
			{
				path = FindExecutable.FindExecutable.FullPath(gitExe);
			}

			Path = path ?? gitExe;
		}

		public class RunResult
		{
			public string StdOut { get; set; } = string.Empty;
			public string StdErr { get; set; } = string.Empty;
			public int ExitCode { get; set; }
		}

		public RunResult Invoke(string[]? args, string? workingDir = null)
		{
			Process p = new();

			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.RedirectStandardError = true;

			p.StartInfo.FileName = Path;
			p.StartInfo.ArgumentList.Clear();
			if (args != null)
				foreach (string arg in args)
					p.StartInfo.ArgumentList.Add(arg);

			if (workingDir != null)
			{
				p.StartInfo.WorkingDirectory = workingDir;
			}

			p.StartInfo.CreateNoWindow = true;
			p.Start();

			RunResult rr = new();
			rr.StdOut = p.StandardOutput.ReadToEnd().Trim();
			rr.StdErr = p.StandardError.ReadToEnd().Trim();
			
			p.WaitForExit();

			rr.ExitCode = p.ExitCode;

			return rr;
		}

	}
}
