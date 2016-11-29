using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptLoader.Contracts;

namespace DataService.Contracts {

	public interface IDataService {

		IList<ScriptDocument> GetDocuments();
		IList<Guid> GetCompletedDocumentIds();
		IList<Script> GetScriptsFor(Guid documentId);
		IList<Guid> GetCompletedScriptIdsFor(Guid documentId);
		void Add(ScriptDocument document);
		void Update(ScriptDocument document);
		bool Delete(ScriptDocument document);
		bool Delete(Script script);
		void Clean();

	}

}
