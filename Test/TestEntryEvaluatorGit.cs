using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
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

		private Entry createTestEntry()
		{
			return new Entry() { Name = "Test", Path = Utility.FindMyGit(), Type = "git" };
		}

		[TestMethod]
		public void EvaluateSimple()
		{
			EntryEvaluator ee = new EntryEvaluator();
			ee.Start();
			Entry e = createTestEntry();
			EntryStatus es = ee.BeginEvaluate(e, (string s) => { });
			while (es.Evaluating) System.Threading.Thread.Sleep(20);
			ee.Shutdown();

			Assert.IsTrue(es.Available);
			Assert.IsFalse(es.FailedStatus);
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
			string tdp = System.IO.Path.Combine(Utility.FindMyGit(), "TestData");
			EntryEvaluator ee = new EntryEvaluator();
			ee.Start();
			Dictionary<Entry, EntryStatus> es = new Dictionary<Entry, EntryStatus>();

			for (byte i = 0; i < 32; ++i)
			{
				BitArray bits = new BitArray(new byte[] { i });
				string n = "c";
				n += bits[0] ? "-branch" : "-main";
				if (bits[1]) n += "-untracked";
				if (bits[2]) n += "-behind";
				if (bits[3]) n += "-ahead";
				if (bits[4]) n += "-changed";

				Entry e = new Entry()
				{
					Name = (bits[0] ? "1" : "0")
						+ (bits[1] ? "1" : "0")
						+ (bits[2] ? "1" : "0")
						+ (bits[3] ? "1" : "0")
						+ (bits[4] ? "1" : "0"),
					Path = System.IO.Path.Combine(tdp, n),
					Type = "git"
				};
				entries.Add(e);
				es[e] = ee.BeginEvaluate(e, (string s) => { });
			}

			foreach (Entry e in entries)
				while (es[e].Evaluating) System.Threading.Thread.Sleep(10);

			ee.Shutdown();

			foreach (Entry e in entries)
			{
				Assert.IsTrue(es[e].Available);
				Assert.IsFalse(es[e].FailedStatus);
				bool branch = e.Name[0] == '1';
				bool untracked = e.Name[1] == '1';
				bool behind = e.Name[2] == '1';
				bool ahead = e.Name[3] == '1';
				bool changed = e.Name[4] == '1';

				Assert.AreEqual(changed, es[e].LocalChanges);
				Assert.AreEqual(branch, es[e].OnBranch);
				Assert.AreEqual(untracked, !es[e].RemoteTracked);

				if (untracked) continue;

				Assert.AreEqual(behind, es[e].IncomingChanges);
				Assert.AreEqual(ahead, es[e].OutgoingChanges);
			}

		}

	}
}
