using ScriptLoader.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackingStore.Contracts;
using Executioner.Contracts;
using System.Xml;
using System.Text.RegularExpressions;

namespace ScriptExecutor.Tests.Classes {

	public class MockScriptLoader : IScriptLoader
	{
		public IList<ScriptDocument> Documents { get; private set; }

		public void LoadDocuments(IBackingStore backingStore)
		{
			XmlDocument doc = GetXmlDoc();
			Tuple<DateTime, int> orderValues = ParseOrderAttribute(doc.SelectSingleNode("ScriptDocument/Order").InnerText);
			Guid docId = Guid.Parse(doc.SelectSingleNode("ScriptDocument/SysId").InnerText);
			ScriptDocument sDoc = new ScriptDocument() {
				SysId = docId,
				DateCreatedUtc = orderValues.Item1,
				Order = orderValues.Item2,
				ResourceName = "System.String",
				Scripts = GetScriptsFrom(doc, docId)
			};

			this.Documents = new List<ScriptDocument>() { sDoc };
			backingStore.Add(sDoc);
		}

		//TODO(Logan) -> This goes against the DRY principle.  This same code is in AssemblyLoader.cs.  Abstract this.
		internal Script[] GetScriptsFrom(XmlDocument xmlDoc, Guid docId)
		{
			XmlNodeList scriptNodes = xmlDoc.GetElementsByTagName(ScriptLoaderConstants.SCRIPT_NODE);
			Script[] scripts = new Script[scriptNodes.Count];

			for (short i = 0; i < scriptNodes.Count; ++i) {
				Tuple<DateTime, int> orderValues = ParseOrderAttribute(
					scriptNodes[i].Attributes["Order"].Value
				);

				scripts[i] = new Script() {
					SysId = Guid.Parse(scriptNodes[i].Attributes["Id"].Value),
					ScriptText = scriptNodes[i].InnerText,
					DateCreatedUtc = orderValues.Item1,
					Order = orderValues.Item2,
					AssemblyName = "System.String",
					DocumentId = docId
				};
			}

			return scripts;
		}

		//TODO(Logan) -> This goes against the DRY principle.  This same code is in AssemblyLoader.cs.  Abstract this.
		private Tuple<DateTime, int> ParseOrderAttribute(string value) {
			string pattern = @"(\d{2,4}-\d{1,2}-\d{1,2}):*(\d{1,3})*";
			Match match = Regex.Match(value, pattern);

			if (!match.Success) {
				return null;
			}

			DateTime date = DateTime.Parse(match.Groups[1].Value);
			string orderStr = match.Groups[2].Value;
			int order = String.IsNullOrEmpty(orderStr) ? Int32.Parse("0") : Int32.Parse(orderStr);

			return Tuple.Create<DateTime, int>(date, order);
		}

		private XmlDocument GetXmlDoc() {
			string xmlStr = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
				<ScriptDocument>
					<SysId>ac04f1b3-219a-4a40-8d7d-869dac218cca</SysId>
					<Order>2016-06-21</Order>
					<Scripts>
						<Script Id=""278b7ef3-09da-4d1b-a101-390f4e6a5407"" Order=""2016-06-21"">
							CREATE TABLE [FirstTable] (
								Value1 NVARCHAR(25)
								, Value2 NVARCHAR(25)
							)
							CREATE TABLE [AnotherTable] (
								ValueA NVARCHAR(25)
								, ValueB NVARCHAR(25)
							)
							CREATE TABLE [ThirdTable] (
								Value9 NVARCHAR(25)
								, Value8 NVARCHAR(25)
							)
						</Script>
						<Script Id=""17a49779-1fec-4624-9da8-3bfde0cd4852"" Order=""2016-06-22:1"">
							INSERT INTO AnotherTable (ValueA, ValueB)
							VALUES ('TestA', 'TestB')
						</Script>
						<Script Id=""40b44acb-8bc5-4a8f-a318-2148052b37a5"" Order=""2016-06-22"">
							INSERT INTO FirstTable (Value1, Value2)
							VALUES ('Test1', 'Test2')
						</Script>
						<Script Id=""e511aaaa-f546-4e60-9146-7a9c30d1ef9e"" Order=""2016-06-23"">
							INSERT INTO ThirdTable (Value9, Value8)
							VALUES ('Test9', 'Test8')
						</Script>
					</Scripts>
				</ScriptDocument>";
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xmlStr);

			return doc;
		}

	}

}
