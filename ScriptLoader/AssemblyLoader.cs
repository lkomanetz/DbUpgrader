using Executioner.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptLoader.Contracts;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using BackingStore.Contracts;
using Executioner.ExtensionMethods;

namespace ScriptLoader {

	public class AssemblyLoader : IScriptLoader {
		private Assembly _assembly;

		public AssemblyLoader(Assembly assembly) {
			_assembly = assembly;
		}

		public IList<ScriptDocument> Documents { get; private set; }

		public void LoadDocuments(IBackingStore backingStore) {
			Documents = GetDocumentsToRun(_assembly);
			foreach (ScriptDocument doc in this.Documents) {
				backingStore.Add(doc);
			}
		}

		internal ScriptDocument[] GetDocumentsToRun(Assembly assembly) {
			string[] resources = assembly.GetManifestResourceNames()
				.Where(x => x.Contains(ScriptLoaderConstants.FILE_EXTENSION))
				.ToArray<String>();

			ScriptDocument[] documents = new ScriptDocument[resources.Length];
			for (short i = 0; i < resources.Length; ++i) {
				using (Stream stream = assembly.GetManifestResourceStream(resources[i]))
				using (StreamReader reader = new StreamReader(stream)) {
					string xmlStr = reader.ReadToEnd();

					XmlDocument xmlDoc = new XmlDocument();
					xmlDoc.LoadXml(xmlStr);

					Tuple<DateTime, int> orderValues = xmlDoc.SelectSingleNode($"{ScriptLoaderConstants.ROOT_NODE}/Order").InnerText.ParseOrderXmlAttribute();

					Guid docId = Guid.Parse(xmlDoc.SelectSingleNode($"{ScriptLoaderConstants.ROOT_NODE}/SysId").InnerText);
					documents[i] = new ScriptDocument() {
						SysId = docId,
						DateCreatedUtc = orderValues.Item1,
						Order = orderValues.Item2,
						ResourceName = resources[i],
						Scripts = xmlDoc.GetScripts(docId)
					};
				}
			}

			return documents;
		}

	}

}
