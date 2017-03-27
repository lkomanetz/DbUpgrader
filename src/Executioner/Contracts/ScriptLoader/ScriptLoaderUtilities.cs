using Executioner.Serializers;
using System;
using System.IO;
using System.Linq;

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

		}

	}

}