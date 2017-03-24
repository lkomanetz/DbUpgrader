using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Text;
using Executioner.Contracts;
using System.Xml;
using System.IO;
using System.Linq;

namespace Executioner {
	namespace Serializers {
		public class DocumentSerializer : IDocumentSerializer {

			private XmlSerializer _xmlSerializer;

			public DocumentSerializer() {
				Type[] extraTypes = new Type[] { typeof(Script) };
				_xmlSerializer = new XmlSerializer(typeof(ScriptDocument), extraTypes);
			}

			public T Deserialize<T>(string serializedStr) {
				using (var reader = new StringReader(serializedStr)) {
					return (T)_xmlSerializer.Deserialize(reader);
				}	
			}

			public string Serialize(object obj) {
				using (var writer = new StringWriter()) {
					_xmlSerializer.Serialize(writer, obj);
					return writer.ToString();
				}
			}

		}

	}

}
