using Executioner.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using Executioner.ExtensionMethods;

namespace Executioner {

	public class AssemblyLoader : IScriptLoader {
		private IList<Assembly> _assemblies;

		public AssemblyLoader(IList<Assembly> assemblies) {
			_assemblies = assemblies;
		}

		public IList<ScriptDocument> Documents { get; internal set; }

		public void LoadDocuments() {
			this.Documents = GetDocumentsToRun(_assemblies);
		}

		internal ScriptDocument[] GetDocumentsToRun(IList<Assembly> assemblies) {
			IList<ScriptDocument> documents = new List<ScriptDocument>();
			foreach (Assembly assembly in assemblies) {
				string[] resources = assembly.GetManifestResourceNames()
					.Where(x => x.Contains(ScriptLoaderConstants.FILE_EXTENSION))
					.ToArray<string>();

				for (short i = 0; i < resources.Length; ++i) {
					documents.Add(CreateScriptDocument(assembly, resources[i]));
				}
			}

			return documents.ToArray();
		}

		private ScriptDocument CreateScriptDocument(Assembly assembly, string resource) {
			ScriptDocument doc = null;

			using (Stream stream = assembly.GetManifestResourceStream(resource))
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
					ResourceName = resource,
					Scripts = ScriptLoaderUtilities.GetScriptsFrom(xmlDoc)
				};
			}

			return doc;
		}

	}

}
