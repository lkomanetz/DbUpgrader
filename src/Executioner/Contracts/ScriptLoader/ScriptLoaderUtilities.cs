using Executioner.Serializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Executioner {
	namespace Contracts {

		public class ScriptLoaderUtilities {

			public static ScriptDocument CreateScriptDocument(Stream stream, string resourceName) {
				DocumentSerializer serializer = new DocumentSerializer();
				string content = String.Empty;
				using (StreamReader sr = new StreamReader(stream)) {
					content = sr.ReadToEnd();
				}

				ScriptDocument doc = serializer.Deserialize<ScriptDocument>(content);
				doc.Scripts = doc.Scripts.Select(x => { x.DocumentId = doc.SysId; return x; }).ToList();

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

			internal static List<Script> GetScriptsFrom(XDocument xmlDoc, Guid docId) {
				IList<XElement> scriptNodes = xmlDoc.Descendants(ScriptLoaderConstants.SCRIPT_NODE).ToList();
				List<Script> scripts = new List<Script>();

				for (short i = 0; i < scriptNodes.Count; ++i) {
					Tuple<DateTime, int> orderValues = ParseOrderXmlAttribute(
						scriptNodes[i].Attributes(ScriptLoaderConstants.ORDER_ATTRIBUTE).Single().Value
					);

					string test = new XText(scriptNodes[i].Value).ToString();
					var executorAttribute = scriptNodes[i]
						.Attributes(ScriptLoaderConstants.EXECUTOR_NAME_ATTRIBUTE)
						.Single();
					string executor = (executorAttribute.Value == null) ? String.Empty : executorAttribute.Value;

					var idAttribute = scriptNodes[i].Attributes(ScriptLoaderConstants.SCRIPT_ID_ATTRIBUTE).Single();
					scripts.Add(new Script() {
						SysId = Guid.Parse(idAttribute.Value),
						ScriptText = scriptNodes[i].Value,
						DateCreatedUtc = orderValues.Item1,
						Order = orderValues.Item2,
						ExecutorName = executor,
						DocumentId = docId
					});
				}

				return scripts;
			}

		}

	}

}