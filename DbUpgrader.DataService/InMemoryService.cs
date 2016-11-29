using DbUpgrader.DataService.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbUpgrader.Contracts;

namespace DbUpgrader.DataService {

	public class InMemoryService : IDataService
	{
		private IDictionary<Guid, ScriptDocument> _completedDocuments;

		public InMemoryService()
		{
			_completedDocuments = new Dictionary<Guid, ScriptDocument>();
		}

		public IList<ScriptDocument> GetDocuments() {
			if (_completedDocuments.Count == 0) {
				return null;
			}

			List<ScriptDocument> documents = new List<ScriptDocument>();

			foreach (var kvp in _completedDocuments) {
				documents.Add(kvp.Value);
			}

			return documents;
		}

		public void Add(Script script) {
			ScriptDocument doc = null;
			if (!_completedDocuments.TryGetValue(script.DocumentId, out doc)) {
				throw new Exception($"No document found for doc id '{script.DocumentId}'");
			}

			if (!script.IsComplete) {
				throw new Exception($"Script id '{script.SysId}' is not complete.");
			}

			doc.Scripts.Add(script);
		}

		public void Add(ScriptDocument document) {
			if (!_completedDocuments.ContainsKey(document.SysId)) {
				_completedDocuments.Add(document.SysId, document);
			}
		}

		public void Update(ScriptDocument document) {
			if (!_completedDocuments.ContainsKey(document.SysId)) {
				throw new Exception($"Document id '{document.SysId}' not found to update.");
			}

			_completedDocuments[document.SysId] = document;
		}

		public void Clean() {
			_completedDocuments.Clear();
		}

		public IList<Guid> GetCompletedDocumentIds() {
			IList<Guid> completedDocumentIds = new List<Guid>();

			foreach (Guid key in _completedDocuments.Keys) {
				completedDocumentIds.Add(key);
			}

			return completedDocumentIds;
		}

		public IList<Guid> GetCompletedScriptsFor(Guid documentId) {
			ScriptDocument doc = null;
			_completedDocuments.TryGetValue(documentId, out doc);
			return doc.Scripts
				.Select(x => x.SysId)
				.ToList();
		}

	}

}
