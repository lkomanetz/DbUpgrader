using System;
using System.Collections.Generic;

namespace Executioner {
	namespace Contracts {

		public interface ILogger {

			IList<Guid> GetCompletedDocumentIds();
			IList<Guid> GetCompletedScriptIdsFor(Guid documentId);
			void Add(ScriptDocument document);
			void Add(Script script);
			void Update(Script script);
			void Clean();

		}

	}

}