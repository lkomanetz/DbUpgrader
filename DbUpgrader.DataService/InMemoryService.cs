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

		public void Add(Script script) {
			ScriptDocument doc = null;
			if (!_completedDocuments.TryGetValue(script.DocumentId, out doc)) {
				throw new Exception($"No document found for doc id '{script.DocumentId}'");
			}

			if (!script.IsComplete)
			{
				throw new Exception($"Script id '{script.SysId}' is not complete.");
			}

			doc.Scripts.Add(script);
		}

		public void Add(ScriptDocument document) {
			if (!_completedDocuments.ContainsKey(document.SysId)) {
				_completedDocuments.Add(document.SysId, document);
			}
		}

		public void SetComplete(ScriptDocument document) {
			ScriptDocument doc = null;
			if (!_completedDocuments.TryGetValue(document.SysId, out doc)) {
				throw new Exception($"No document found for doc id '{document.SysId}'.");
			}

			doc.IsComplete = true;
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
