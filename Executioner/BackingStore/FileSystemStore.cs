using Executioner.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Executioner.BackingStore {

	public class FileSystemStore : IBackingStore {

		public void Add(Script script) {
			throw new NotImplementedException();
		}

		public void Add(ScriptDocument document) {
			throw new NotImplementedException();
		}

		public void Clean() {
			throw new NotImplementedException();
		}

		public bool Delete(Script script) {
			throw new NotImplementedException();
		}

		public bool Delete(ScriptDocument document) {
			throw new NotImplementedException();
		}

		public void Dispose() {
			throw new NotImplementedException();
		}

		public IList<Guid> GetCompletedDocumentIds() {
			throw new NotImplementedException();
		}

		public IList<Guid> GetCompletedScriptIdsFor(Guid documentId) {
			throw new NotImplementedException();
		}

		public IList<ScriptDocument> GetDocuments(GetDocumentsRequest request = null) {
			throw new NotImplementedException();
		}

		public IList<Script> GetScriptsFor(Guid documentId) {
			throw new NotImplementedException();
		}

		public void Update(Script script) {
			throw new NotImplementedException();
		}

		public void Update(ScriptDocument document) {
			throw new NotImplementedException();
		}

	}

}
