using Executioner.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ScriptLoader.Contracts {

	public class ScriptLoaderUtilities {

		public static Script[] GetScriptsFrom(XmlDocument xmlDoc) {
			XmlNodeList scriptNodes = xmlDoc.GetElementsByTagName(ScriptLoaderConstants.SCRIPT_NODE);
			Script[] scripts = new Script[scriptNodes.Count];

			Guid docId = Guid.Parse(xmlDoc.SelectSingleNode($"{ScriptLoaderConstants.ROOT_NODE}/SysId").InnerText);
			for (short i = 0; i < scriptNodes.Count; ++i) {
				Tuple<DateTime, int> orderValues = ParseOrderXmlAttribute(
					scriptNodes[i].Attributes["Order"].Value
				);

				scripts[i] = new Script() {
					SysId = Guid.Parse(scriptNodes[i].Attributes["Id"].Value),
					ScriptText = scriptNodes[i].InnerText,
					DateCreatedUtc = orderValues.Item1,
					Order = orderValues.Item2,
					AssemblyName = "System.String",
					DocumentId = docId
				};
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
