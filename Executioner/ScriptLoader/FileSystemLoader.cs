using Executioner.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Executioner {

	public class FileSystemLoader : IScriptLoader {
		private string _rootDir;

		public FileSystemLoader() {
			_rootDir = String.Empty;
		}

		public IList<ScriptDocument> Documents { get; internal set; }
		public string Location {
			get { return _rootDir; }
			set { _rootDir = value; }
		}

		public void LoadDocuments() {
			if (this.Documents == null) {
				this.Documents = new List<ScriptDocument>();
			}

			IEnumerable<string> files = Directory.EnumerateFiles(_rootDir);
			foreach (string file in files) {
				this.Documents.Add(CreateScriptDocument(file));
			}
		}

		private ScriptDocument CreateScriptDocument(string fileLocation) {
			ScriptDocument doc = null;

			using (Stream stream = new FileStream(fileLocation, FileMode.Open))
			using (StreamReader reader = new StreamReader(stream)) {
				string xmlStr = reader.ReadToEnd();
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(xmlStr);

				Tuple<DateTime, int> orderValues = ScriptLoaderUtilities.ParseOrderXmlAttribute(
					xmlDoc.SelectSingleNode($"{ScriptLoaderConstants.ROOT_NODE}/{ScriptLoaderConstants.DOCUMENT_ORDER_NODE}").InnerText
				);

				Guid docId = Guid.Parse(
					xmlDoc.SelectSingleNode($"{ScriptLoaderConstants.ROOT_NODE}/{ScriptLoaderConstants.DOCUMENT_ID_NODE}").InnerText
				);

				doc = new ScriptDocument() {
					SysId = docId,
					DateCreatedUtc = orderValues.Item1,
					Order = orderValues.Item2,
					ResourceName = fileLocation,
					Scripts = ScriptLoaderUtilities.GetScriptsFrom(xmlDoc)
				};
			}

			return doc;
		}
	}

}
