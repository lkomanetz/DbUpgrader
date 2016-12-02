using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Executioner.Contracts;
using ScriptLoader.Contracts;

namespace Executioner.ExtensionMethods {

	public static class XmlDocumentExtensions {

		public static Script[] GetScripts(this XmlDocument xmlDoc, Guid docId) {
			XmlNodeList scriptNodes = xmlDoc.GetElementsByTagName(ScriptLoaderConstants.SCRIPT_NODE);
			Script[] scripts = new Script[scriptNodes.Count];

			for (short i = 0; i < scriptNodes.Count; ++i) {
				Tuple<DateTime, int> orderValues = scriptNodes[i].Attributes["Order"].Value.ParseOrderXmlAttribute();

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

	}

}
