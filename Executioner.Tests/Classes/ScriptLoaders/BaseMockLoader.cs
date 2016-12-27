using Executioner.Contracts;
using Executioner.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Executioner.Tests.Classes {

	public class BaseMockLoader : IScriptLoader {
		private string[] _scriptElements;

		public BaseMockLoader(string[] scriptElements) {
			_scriptElements = scriptElements;
		}

		public IList<ScriptDocument> Documents { get; set; }

		public void LoadDocuments() {
			XmlDocument doc = GetXmlDoc();
			string orderNodeName = String.Format(
				"{0}/{1}",
				ScriptLoaderConstants.ROOT_NODE,
				ScriptLoaderConstants.DOCUMENT_ORDER_NODE
			);
			Tuple<DateTime, int> orderValues = ScriptLoaderUtilities.ParseOrderXmlAttribute(
				doc.SelectSingleNode(orderNodeName).InnerText
			);

			string docIdNodeName = String.Format(
				"{0}/{1}",
				ScriptLoaderConstants.ROOT_NODE,
				ScriptLoaderConstants.DOCUMENT_ID_NODE
			);
			Guid docId = Guid.Parse(doc.SelectSingleNode(docIdNodeName).InnerText);
			ScriptDocument sDoc = new ScriptDocument() {
				SysId = docId,
				DateCreatedUtc = orderValues.Item1,
				Order = orderValues.Item2,
				ResourceName = "System.String",
				Scripts = ScriptLoaderUtilities.GetScriptsFrom(doc)
			};

			this.Documents = new List<ScriptDocument>() { sDoc };
		}

		public void Add(Script script) {
			ScriptDocument doc = this.Documents
				.Where(x => x.SysId == script.DocumentId)
				.SingleOrDefault();

			if (doc == null) {
				return;
			}
			int index = doc.Scripts.FindIndex(x => x.SysId == script.SysId);
			if (index == -1) {
				doc.Scripts.Add(script);
			}
			else {
				doc.Scripts[index] = script;
			}
		}

		private XmlDocument GetXmlDoc() {
			string scriptStr = String.Empty;
			foreach (string item in _scriptElements) {
				scriptStr += $"{item}\n";
			}
			string xmlStr = $@"<?xml version='1.0' encoding='utf-8'?>
				<ScriptDocument>
					<Id>ac04f1b3-219a-4a40-8d7d-869dac218cca</Id>
					<Order>2016-06-21</Order>
					{scriptStr}	
				</ScriptDocument>";

			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(xmlStr);

			return xmlDoc;
		}

	}

}
