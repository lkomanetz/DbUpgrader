using BackingStore.Contracts;
using Executioner.Contracts;
using ScriptLoader.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Executioner.Tests.Classes {

	public abstract class BaseMockLoader : IScriptLoader {
		public IList<ScriptDocument> Documents { get; protected set; }

		public void LoadDocuments(IBackingStore storage) {
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
			storage.Add(sDoc);
		}

		public abstract XmlDocument GetXmlDoc();

	}

}
