using Executioner.Contracts;
using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Executioner {
	namespace Serializers {

		public class JsonSerializer : IDocumentSerializer
		{
			private DataContractJsonSerializer _serializer;

			public JsonSerializer(Type objType) {
				_serializer = new DataContractJsonSerializer(objType);
			}

			public T Deserialize<T>(string serializedStr) {
				byte[] stringAsBytes = Encoding.UTF8.GetBytes(serializedStr);
				using (MemoryStream ms = new MemoryStream(stringAsBytes)) {
					return (T)_serializer.ReadObject(ms);
				}
			}

			public string Serialize(object obj) {
				using (MemoryStream ms = new MemoryStream()) {
					_serializer.WriteObject(ms, obj);
					return Encoding.UTF8.GetString(ms.ToArray());
				}
			}

		}

	}

}