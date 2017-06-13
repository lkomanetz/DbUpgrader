using Executioner.Contracts;
using System;

namespace Executioner {
	namespace Serializers {

		public interface IDocumentSerializer {

			string Serialize(object obj);
			T Deserialize<T>(string serializedStr);

		}

	}

}