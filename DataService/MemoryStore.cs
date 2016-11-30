using BackingStore.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Executioner.Contracts;

namespace BackingStore {

	public class MemoryStore : IBackingStore {
		private IDictionary<Guid, ScriptDocument> _documents;

		public MemoryStore() {
			_documents = new Dictionary<Guid, ScriptDocument>();
		}

		public IList<ScriptDocument> GetDocuments() {
			if (_documents.Count == 0) {
				return null;
			}

			List<ScriptDocument> documents = new List<ScriptDocument>();

			foreach (var kvp in _documents) {
				documents.Add(kvp.Value);
			}

			return documents;
		}

		public IList<Script> GetScriptsFor(Guid documentId) {
			ScriptDocument doc = this.GetDocument(documentId);
			return doc.Scripts;
		}

		public void Add(ScriptDocument document) {
			if (!_documents.ContainsKey(document.SysId)) {
				_documents.Add(document.SysId, document);
			}
		}

		public void Update(ScriptDocument document) {
			if (!_documents.ContainsKey(document.SysId)) {
				throw new Exception($"Document id '{document.SysId}' not found to update.");
			}

			_documents[document.SysId] = document;
		}

		public void Clean() {
			_documents.Clear();
		}

		public IList<Guid> GetCompletedDocumentIds() {
			return _documents
				.Where(x => x.Value.IsComplete)
				.Select(x => x.Key)
				.ToList();
		}

		public IList<Guid> GetCompletedScriptIdsFor(Guid documentId) {
			ScriptDocument doc = null;
			_documents.TryGetValue(documentId, out doc);
			return doc.Scripts
				.Where(x => x.IsComplete)
				.Select(x => x.SysId)
				.ToList();
		}

		public bool Delete(ScriptDocument document) {
			return _documents.Remove(
				_documents.Where(x => x.Key == document.SysId).SingleOrDefault()
			);
		}

		public bool Delete(Script script) {
			ScriptDocument doc = GetDocument(script.DocumentId);

			return doc.Scripts.Remove(
				doc.Scripts.Where(x => x.SysId == script.SysId).SingleOrDefault()
			);
		}

		public void Dispose() {
			Dispose(true);
		}

		protected void Dispose(bool disposing) {
			if (disposing) {
				this.Clean();
			}
		}

		private ScriptDocument GetDocument(Guid docId) {
			ScriptDocument doc = null;
			if (!_documents.TryGetValue(docId, out doc)) {
				throw new Exception($"Document id '{doc.SysId}' not found to update.");
			}

			return doc;
		}

	}

}
