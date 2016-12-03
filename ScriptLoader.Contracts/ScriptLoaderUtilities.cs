using Executioner.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ScriptLoader.Contracts {

	public class ScriptLoaderUtilities {

		public static IList<Script> GetScriptsFrom(XmlDocument xmlDoc) {
			XmlNodeList scriptNodes = xmlDoc.GetElementsByTagName(ScriptLoaderConstants.SCRIPT_NODE);
			IList<Script> scripts = new List<Script>();

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

		public static Tuple<DateTime, int> ParseOrderXmlAttribute(string value) {
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
	}

}
