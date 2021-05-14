using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace SG.Checkouts_Overview.Test
{
	[TestClass]
	public class TestEntryListIO
	{
		[TestMethod]
		public void Serialize()
		{
			Dictionary<string, string> paths = new Dictionary<string, string>();
			paths["Name 1"] = @"C:\somewhere\here";
			paths["Name 2"] = @"C:\somewhere\there";
			paths["Name 3"] = @"D:\else\where";

			Dictionary<string, string> types = new Dictionary<string, string>();
			types["Name 1"] = @"git";
			types["Name 2"] = @"gut";
			types["Name 3"] = @"good";

			ObservableCollection<Entry> entries = new ObservableCollection<Entry>();
			entries.Add(
				new Entry()
				{
					Name = "Name 1",
					Path = paths["Name 1"],
					Type = types["Name 1"],
					Available = false,
					Evaluating = true,
					StatusKnown = false,
					FailedStatus = true,
					LocalChanges = false,
					IncomingChanges = false,
					OutgoingChanges = true,
					LastMessage = "Dummy"
				});
			entries.Add(
				new Entry()
				{
					Name = "Name 2",
					Path = paths["Name 2"],
					Type = types["Name 2"],
					Available = true,
					Evaluating = true,
					StatusKnown = false,
					FailedStatus = false,
					LocalChanges = false,
					IncomingChanges = false,
					OutgoingChanges = true,
					LastMessage = "Dummy-2"
				});
			entries.Add(
				new Entry()
				{
					Name = "Name 3",
					Path = paths["Name 3"],
					Type = types["Name 3"],
					Available = true,
					Evaluating = true,
					StatusKnown = true,
					FailedStatus = true,
					LocalChanges = true,
					IncomingChanges = true,
					OutgoingChanges = true,
					LastMessage = "Dummy-3"
				});

			string serialized = null;
			using (StringWriter wrtr = new StringWriter())
			{
				XmlSerializer ser = new XmlSerializer(typeof(Entry[]));
				ser.Serialize(wrtr, entries.ToArray());
				serialized = wrtr.ToString();
			}

			Assert.IsFalse(string.IsNullOrWhiteSpace(serialized));

			XmlDocument doc = new XmlDocument();
			doc.LoadXml(serialized);

			Assert.AreEqual(doc.DocumentElement.LocalName, "ArrayOfEntry");

			XmlNodeList entryNodes = doc.SelectNodes("//Entry");
			Assert.AreEqual(3, entryNodes.Count);

			Assert.AreEqual(3, doc.DocumentElement.ChildNodes.Count);

			foreach (XmlNode entryNode in entryNodes)
			{
				Assert.AreEqual(3, entryNode.ChildNodes.Count);

				string name = entryNode.SelectSingleNode("./Name").InnerText;
				Assert.IsFalse(string.IsNullOrWhiteSpace(name));
				string path = entryNode.SelectSingleNode("./Path").InnerText;
				Assert.IsFalse(string.IsNullOrWhiteSpace(path));
				string type = entryNode.SelectSingleNode("./Type").InnerText;
				Assert.IsFalse(string.IsNullOrWhiteSpace(type));

				Assert.IsTrue(paths.ContainsKey(name));
				Assert.AreEqual(paths[name], path);
				paths.Remove(name);

				Assert.IsTrue(types.ContainsKey(name));
				Assert.AreEqual(types[name], type);
				types.Remove(name);
			}
		}

		[TestMethod]
		public void Deserialize()
		{
			string xml = @"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfEntry xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <Entry>
    <Name>Name 1</Name>
    <Path>C:\somewhere\here</Path>
    <Type>git</Type>
  </Entry>
  <Entry>
    <Name>Name 2</Name>
    <Path>C:\somewhere\there</Path>
    <Type>gut</Type>
  </Entry>
  <Entry>
    <Name>Name 3</Name>
    <Path>D:\else\where</Path>
    <Type>good</Type>
  </Entry>
</ArrayOfEntry>";
			Dictionary<string, string> paths = new Dictionary<string, string>();
			paths["Name 1"] = @"C:\somewhere\here";
			paths["Name 2"] = @"C:\somewhere\there";
			paths["Name 3"] = @"D:\else\where";

			Dictionary<string, string> types = new Dictionary<string, string>();
			types["Name 1"] = @"git";
			types["Name 2"] = @"gut";
			types["Name 3"] = @"good";

			Entry ee = new Entry();

			using (StringReader rdr = new StringReader(xml))
			{
				XmlSerializer ser = new XmlSerializer(typeof(Entry[]));
				Entry[] es = ser.Deserialize(rdr) as Entry[];

				Assert.AreEqual(3, es.Length);

				foreach (Entry e in es)
				{
					Assert.IsTrue(paths.ContainsKey(e.Name));
					Assert.AreEqual(paths[e.Name], e.Path);
					paths.Remove(e.Name);

					Assert.IsTrue(types.ContainsKey(e.Name));
					Assert.AreEqual(types[e.Name], e.Type);
					types.Remove(e.Name);

					Assert.AreEqual(ee.Available, e.Available);
					Assert.AreEqual(ee.Evaluating, e.Evaluating);
					Assert.AreEqual(ee.StatusKnown, e.StatusKnown);
					Assert.AreEqual(ee.FailedStatus, e.FailedStatus);
					Assert.AreEqual(ee.LocalChanges, e.LocalChanges);
					Assert.AreEqual(ee.IncomingChanges, e.IncomingChanges);
					Assert.AreEqual(ee.OutgoingChanges, e.OutgoingChanges);
					Assert.AreEqual(ee.LastMessage, e.LastMessage);
				}
			}
		}

		[TestMethod]
		public void RoundTrip()
		{
			Dictionary<string, string> paths = new Dictionary<string, string>();
			paths["Name 1"] = @"C:\somewhere\here";
			paths["Name 2"] = @"C:\somewhere\there";
			paths["Name 3"] = @"D:\else\where";

			Dictionary<string, string> types = new Dictionary<string, string>();
			types["Name 1"] = @"git";
			types["Name 2"] = @"gut";
			types["Name 3"] = @"good";

			ObservableCollection<Entry> entries = new ObservableCollection<Entry>();
			entries.Add(
				new Entry()
				{
					Name = "Name 1",
					Path = paths["Name 1"],
					Type = types["Name 1"],
					Available = false,
					Evaluating = true,
					StatusKnown = false,
					FailedStatus = true,
					LocalChanges = false,
					IncomingChanges = false,
					OutgoingChanges = true,
					LastMessage = "Dummy"
				});
			entries.Add(
				new Entry()
				{
					Name = "Name 2",
					Path = paths["Name 2"],
					Type = types["Name 2"],
					Available = true,
					Evaluating = true,
					StatusKnown = false,
					FailedStatus = false,
					LocalChanges = false,
					IncomingChanges = false,
					OutgoingChanges = true,
					LastMessage = "Dummy-2"
				});
			entries.Add(
				new Entry()
				{
					Name = "Name 3",
					Path = paths["Name 3"],
					Type = types["Name 3"],
					Available = true,
					Evaluating = true,
					StatusKnown = true,
					FailedStatus = true,
					LocalChanges = true,
					IncomingChanges = true,
					OutgoingChanges = true,
					LastMessage = "Dummy-3"
				});

			string serialized = null;
			using (StringWriter wrtr = new StringWriter())
			{
				XmlSerializer ser = new XmlSerializer(typeof(Entry[]));
				ser.Serialize(wrtr, entries.ToArray());
				serialized = wrtr.ToString();
			}

			Assert.IsFalse(string.IsNullOrWhiteSpace(serialized));

			XmlDocument doc = new XmlDocument();
			doc.LoadXml(serialized);

			Assert.AreEqual(doc.DocumentElement.LocalName, "ArrayOfEntry");

			XmlNodeList entryNodes = doc.SelectNodes("//Entry");
			Assert.AreEqual(3, entryNodes.Count);

			Assert.AreEqual(3, doc.DocumentElement.ChildNodes.Count);

			Entry ee = new Entry();

			using (StringReader rdr = new StringReader(serialized))
			{
				XmlSerializer ser = new XmlSerializer(typeof(Entry[]));
				Entry[] es = ser.Deserialize(rdr) as Entry[];

				Assert.AreEqual(3, es.Length);

				foreach (Entry e in es)
				{
					Assert.IsTrue(paths.ContainsKey(e.Name));
					Assert.AreEqual(paths[e.Name], e.Path);
					paths.Remove(e.Name);

					Assert.IsTrue(types.ContainsKey(e.Name));
					Assert.AreEqual(types[e.Name], e.Type);
					types.Remove(e.Name);

					Assert.AreEqual(ee.Available, e.Available);
					Assert.AreEqual(ee.Evaluating, e.Evaluating);
					Assert.AreEqual(ee.StatusKnown, e.StatusKnown);
					Assert.AreEqual(ee.FailedStatus, e.FailedStatus);
					Assert.AreEqual(ee.LocalChanges, e.LocalChanges);
					Assert.AreEqual(ee.IncomingChanges, e.IncomingChanges);
					Assert.AreEqual(ee.OutgoingChanges, e.OutgoingChanges);
					Assert.AreEqual(ee.LastMessage, e.LastMessage);
				}
			}

		}

		[TestMethod]
		public void RoundTripOneEntry()
		{
			Dictionary<string, string> paths = new Dictionary<string, string>();
			paths["Name 2"] = @"C:\somewhere\there";

			Dictionary<string, string> types = new Dictionary<string, string>();
			types["Name 2"] = @"gut";

			ObservableCollection<Entry> entries = new ObservableCollection<Entry>();
			entries.Add(
				new Entry()
				{
					Name = "Name 2",
					Path = paths["Name 2"],
					Type = types["Name 2"],
					Available = true,
					Evaluating = true,
					StatusKnown = false,
					FailedStatus = false,
					LocalChanges = false,
					IncomingChanges = false,
					OutgoingChanges = true,
					LastMessage = "Dummy-2"
				});

			string serialized = null;
			using (StringWriter wrtr = new StringWriter())
			{
				XmlSerializer ser = new XmlSerializer(typeof(Entry[]));
				ser.Serialize(wrtr, entries.ToArray());
				serialized = wrtr.ToString();
			}

			Assert.IsFalse(string.IsNullOrWhiteSpace(serialized));

			XmlDocument doc = new XmlDocument();
			doc.LoadXml(serialized);

			Assert.AreEqual(doc.DocumentElement.LocalName, "ArrayOfEntry");

			XmlNodeList entryNodes = doc.SelectNodes("//Entry");
			Assert.AreEqual(1, entryNodes.Count);

			Assert.AreEqual(1, doc.DocumentElement.ChildNodes.Count);

			Entry ee = new Entry();

			using (StringReader rdr = new StringReader(serialized))
			{
				XmlSerializer ser = new XmlSerializer(typeof(Entry[]));
				Entry[] es = ser.Deserialize(rdr) as Entry[];

				Assert.AreEqual(1, es.Length);

				foreach (Entry e in es)
				{
					Assert.IsTrue(paths.ContainsKey(e.Name));
					Assert.AreEqual(paths[e.Name], e.Path);
					paths.Remove(e.Name);

					Assert.IsTrue(types.ContainsKey(e.Name));
					Assert.AreEqual(types[e.Name], e.Type);
					types.Remove(e.Name);

					Assert.AreEqual(ee.Available, e.Available);
					Assert.AreEqual(ee.Evaluating, e.Evaluating);
					Assert.AreEqual(ee.StatusKnown, e.StatusKnown);
					Assert.AreEqual(ee.FailedStatus, e.FailedStatus);
					Assert.AreEqual(ee.LocalChanges, e.LocalChanges);
					Assert.AreEqual(ee.IncomingChanges, e.IncomingChanges);
					Assert.AreEqual(ee.OutgoingChanges, e.OutgoingChanges);
					Assert.AreEqual(ee.LastMessage, e.LastMessage);
				}
			}

		}

		[TestMethod]
		public void RoundTripEmpty()
		{
			ObservableCollection<Entry> entries = new ObservableCollection<Entry>();

			string serialized = null;
			using (StringWriter wrtr = new StringWriter())
			{
				XmlSerializer ser = new XmlSerializer(typeof(Entry[]));
				ser.Serialize(wrtr, entries.ToArray());
				serialized = wrtr.ToString();
			}

			Assert.IsFalse(string.IsNullOrWhiteSpace(serialized));

			XmlDocument doc = new XmlDocument();
			doc.LoadXml(serialized);
			Assert.AreEqual(doc.DocumentElement.LocalName, "ArrayOfEntry");
			Assert.AreEqual(0, doc.DocumentElement.ChildNodes.Count);

			using (StringReader rdr = new StringReader(serialized))
			{
				XmlSerializer ser = new XmlSerializer(typeof(Entry[]));
				Entry[] es = ser.Deserialize(rdr) as Entry[];

				Assert.IsNotNull(es);
				Assert.AreEqual(0, es.Length);
			}
		}
	}
}
