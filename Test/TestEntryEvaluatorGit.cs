using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SG.Checkouts_Overview.Test
{

	[TestClass]
	public class TestEntryEvaluatorGit
	{

		private string findMyGit()
		{
			string p = Assembly.GetExecutingAssembly().Location;
			while (p.Length > 2 && !System.IO.Directory.Exists(System.IO.Path.Combine(p, ".git"))) p = System.IO.Path.GetDirectoryName(p);
			return p;
		}

		private Entry createTestEntry()
		{
			return new Entry() { Name = "Test", Path = findMyGit(), Type = "git" };
		}

		[TestMethod]
		public void Evaluate()
		{
			EntryEvaluator ee = new EntryEvaluator();
			ee.Start();
			Entry e = createTestEntry();
			ee.BeginEvaluate(e);
			while (e.Evaluating) System.Threading.Thread.Sleep(20);
			ee.Shutdown();

			Assert.IsTrue(e.Available);
			Assert.IsFalse(e.FailedStatus);
			Assert.IsTrue(e.StatusKnown);
			Assert.IsTrue(e.LocalChanges);
		}

		[TestMethod]
		public void GetCommitDate()
		{
			EntryEvaluator ee = new EntryEvaluator();
			ee.Start();
			Entry e = createTestEntry();
			DateTime dt = ee.GetCommitDate(e);
			ee.Shutdown();

			Assert.IsTrue(dt > DateTime.MinValue);
			Assert.IsTrue(dt.Year >= 2021);
		}

	}
}
