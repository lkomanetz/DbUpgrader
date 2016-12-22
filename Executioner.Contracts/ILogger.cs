using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Executioner.Contracts {

	public interface ILogger {
		IList<Guid> GetCompletedDocumentIds();
		IList<Guid> GetCompletedScriptIdsFor(Guid documentId);
		void Add(ScriptDocument document);
		void Add(Script script);
		void Update(Script script);
		void Clean();
	}

}
