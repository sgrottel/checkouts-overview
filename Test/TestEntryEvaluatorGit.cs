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
		public void EvaluateSimple()
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

		[TestMethod]
		public void EvaluateTestData()
		{
			List<Entry> entries = new List<Entry>();
			string tdp = System.IO.Path.Combine(findMyGit(), "TestData");
			EntryEvaluator ee = new EntryEvaluator();
			ee.Start();
			for (char c = 'a'; c <= 'h'; ++c)
			{
				Entry e = new Entry()
				{
					Name = c.ToString(),
					Path = System.IO.Path.Combine(tdp, c.ToString()),
					Type = "git"
				};
				entries.Add(e);
				ee.BeginEvaluate(e);
			}
			foreach (Entry e in entries)
				while (e.Evaluating) System.Threading.Thread.Sleep(20);
			ee.Shutdown();

			Dictionary<string, bool> localchanges = new Dictionary<string, bool>();
			localchanges["a"] = false;
			localchanges["b"] = false;
			localchanges["c"] = false;
			localchanges["d"] = false;
			localchanges["e"] = true;
			localchanges["f"] = true;
			localchanges["g"] = true;
			localchanges["h"] = true;

			Dictionary<string, bool> incoming = new Dictionary<string, bool>();
			incoming["a"] = true;
			incoming["b"] = true;
			incoming["c"] = false;
			incoming["d"] = false;
			incoming["e"] = true;
			incoming["f"] = true;
			incoming["g"] = false;
			incoming["h"] = false;

			Dictionary<string, bool> outgoing = new Dictionary<string, bool>();
			outgoing["a"] = false;
			outgoing["b"] = true;
			outgoing["c"] = false;
			outgoing["d"] = true;
			outgoing["e"] = false;
			outgoing["f"] = true;
			outgoing["g"] = false;
			outgoing["h"] = true;

			foreach (Entry e in entries)
			{
				Assert.IsTrue(e.Available);
				Assert.IsFalse(e.FailedStatus);
				Assert.IsTrue(e.StatusKnown);
				Assert.AreEqual(localchanges[e.Name], e.LocalChanges);
				Assert.AreEqual(incoming[e.Name], e.IncomingChanges);
				Assert.AreEqual(outgoing[e.Name], e.OutgoingChanges);
			}

		}

	}
}
