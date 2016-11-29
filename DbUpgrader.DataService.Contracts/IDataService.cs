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
		IList<Guid> GetCompletedScriptsFor(Guid documentId);
		void Add(ScriptDocument document);
		void Add(Script script);
		void Update(ScriptDocument document);
		void Clean();

	}

}
