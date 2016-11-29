using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbUpgrader.Contracts;

namespace DbUpgrader.DataService.Contracts {

	public interface IDataService {

		IList<ScriptDocument> GetDocuments();
		IList<Guid> GetCompletedDocumentIds();
		IList<Guid> GetCompletedScriptsFor(Guid documentId);
		void Add(ScriptDocument document);
		void Add(Script script);
		void Update(ScriptDocument document);
		void Clean();

	}

}
