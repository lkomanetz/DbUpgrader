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

					Tuple<DateTime, int> orderValues = ParseOrderAttribute(
						xmlDoc.SelectSingleNode($"{ScriptLoaderConstants.ROOT_NODE}/Order").InnerText
					);

					documents[i] = new ScriptDocument() {
						SysId = Guid.Parse(xmlDoc.SelectSingleNode($"{ScriptLoaderConstants.ROOT_NODE}/SysId").InnerText),
						DateCreatedUtc = orderValues.Item1,
						Order = orderValues.Item2,
						ResourceName = resources[i],
						Scripts = GetScriptsFrom(xmlDoc)
					};
				}
			}

			return documents;
		}

		internal Script[] GetScriptsFrom(XmlDocument xmlDoc)
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
					AssemblyName = _assembly.FullName
				};
			}

			return scripts;
		}

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

	}

}
