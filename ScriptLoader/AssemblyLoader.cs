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

namespace ScriptLoader {

	public class AssemblyLoader : IScriptLoader {
		private string _currentAssemblyName;

		private const string ROOT_NODE = "ScriptDocument";
		private const string SCRIPT_NODE = "Script";

		public AssemblyLoader() { }

		public IList<ScriptDocument> Documents { get; private set; }

		public void LoadDocuments(Assembly assembly) {
			_currentAssemblyName = assembly.FullName;
			Documents = GetDocumentsToRun(assembly);
		}

		internal ScriptDocument[] GetDocumentsToRun(Assembly assembly) {
			string[] resources = assembly.GetManifestResourceNames()
				.Where(x => x.Contains(".sql.xml"))
				.ToArray<String>();

			ScriptDocument[] documents = new ScriptDocument[resources.Length];
			for (short i = 0; i < resources.Length; ++i) {
				using (Stream stream = assembly.GetManifestResourceStream(resources[i]))
				using (StreamReader reader = new StreamReader(stream)) {
					string xmlStr = reader.ReadToEnd();

					XmlDocument xmlDoc = new XmlDocument();
					xmlDoc.LoadXml(xmlStr);

					Tuple<DateTime, int> orderValues = ParseOrderAttribute(xmlDoc.SelectSingleNode($"{ROOT_NODE}/Order").InnerText);

					documents[i] = new ScriptDocument() {
						SysId = Guid.Parse(xmlDoc.SelectSingleNode($"{ROOT_NODE}/SysId").InnerText),
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
			XmlNodeList scriptNodes = xmlDoc.GetElementsByTagName(SCRIPT_NODE);
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
					AssemblyName = _currentAssemblyName
				};
			}

			return scripts.OrderBy(x => x.DateCreatedUtc)
				.ThenBy(x => x.Order)
				.ToArray<Script>();
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
