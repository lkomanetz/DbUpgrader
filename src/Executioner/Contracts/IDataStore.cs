using System;
using System.Collections.Generic;

namespace Executioner {
	namespace Contracts {

		public interface IDataStore {

			void CreateLogFile(Guid documentId);
			IList<Guid> GetCompletedScriptIdsFor(Guid documentId);
			void Add(Script script);
			void Clean();

		}

	}

}