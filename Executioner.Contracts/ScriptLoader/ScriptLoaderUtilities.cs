using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Executioner.Contracts {

	public class ScriptLoaderUtilities {

		public static ScriptDocument CreateScriptDocument(Stream stream, string resourceName) {
			ScriptDocument doc = null;

			using (StreamReader reader = new StreamReader(stream)) {
				string xmlStr = reader.ReadToEnd();
				XmlDocument xmlDoc = new XmlDocument();

				using (StringReader sr = new StringReader(xmlStr))
				using (XmlTextReader xtr = new XmlTextReader(sr) { Namespaces = false }) {
					xmlDoc.Load(xtr);
				}

				Tuple<DateTime, int> orderValues = ParseOrderXmlAttribute(
					xmlDoc.SelectSingleNode($"{ScriptLoaderConstants.ROOT_NODE}/{ScriptLoaderConstants.DOCUMENT_ORDER_NODE}").InnerText
				);

				Guid docId = Guid.Parse(
					xmlDoc.SelectSingleNode($"{ScriptLoaderConstants.ROOT_NODE}/{ScriptLoaderConstants.DOCUMENT_ID_NODE}").InnerText
				);

				doc = new ScriptDocument() {
					SysId = docId,
					DateCreatedUtc = orderValues.Item1,
					Order = orderValues.Item2,
					ResourceName = resourceName,
					Scripts = GetScriptsFrom(xmlDoc)
				};
			}

			return doc;
		}

		internal static Tuple<DateTime, int> ParseOrderXmlAttribute(string value) {
			string[] values = value.Split(':');

			DateTime date = default(DateTime);
			if (!DateTime.TryParse(values[0], out date)) {
				throw new FormatException("Invalid date value found in XML 'Order' attribute.");
			}

			int order = 0;
			if (values.Length == 2) {
				if (!Int32.TryParse(values[1], out order)) {
					throw new FormatException("Non-numeric value found in XML 'Order' attribute.");
				}
			}

			return Tuple.Create<DateTime, int>(date, order);
		}

		internal static List<Script> GetScriptsFrom(XmlDocument xmlDoc) {
			XmlNodeList scriptNodes = xmlDoc.GetElementsByTagName(ScriptLoaderConstants.SCRIPT_NODE);
			List<Script> scripts = new List<Script>();

			Guid docId = Guid.Parse(xmlDoc.SelectSingleNode(
				$"{ScriptLoaderConstants.ROOT_NODE}/{ScriptLoaderConstants.DOCUMENT_ID_NODE}").InnerText
			);

			for (short i = 0; i < scriptNodes.Count; ++i) {
				Tuple<DateTime, int> orderValues = ParseOrderXmlAttribute(
					scriptNodes[i].Attributes[ScriptLoaderConstants.ORDER_ATTRIBUTE].Value
				);

				scripts.Add(new Script() {
					SysId = Guid.Parse(scriptNodes[i].Attributes[ScriptLoaderConstants.SCRIPT_ID_ATTRIBUTE].Value),
					ScriptText = scriptNodes[i].InnerText,
					DateCreatedUtc = orderValues.Item1,
					Order = orderValues.Item2,
					ExecutorName = scriptNodes[i].Attributes[ScriptLoaderConstants.EXECUTOR_NAME_ATTRIBUTE].Value,
					DocumentId = docId
				});
			}

			return scripts;
		}

	}

}
