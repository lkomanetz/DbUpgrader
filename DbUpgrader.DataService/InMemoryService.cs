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
		private IDictionary<Guid, IList<Guid>> _completedItems;

		public InMemoryService()
		{
			_completedItems = new Dictionary<Guid, IList<Guid>>();
		}

		public void Add(Script script) {
			IList<Guid> scripts = null;
			if (!_completedItems.TryGetValue(script.DocumentId, out scripts)) {
				throw new Exception($"No document found for script id '{script.SysId}'");
			}

			scripts.Add(script.SysId);
		}

		public void Add(ScriptDocument document) {
			if (!_completedItems.ContainsKey(document.SysId)) {
				_completedItems.Add(document.SysId, new List<Guid>());
			}
		}

		public void Clean() {
			_completedItems.Clear();
		}

		public IList<Guid> GetCompletedDocumentIds() {
			IList<Guid> completedDocumentIds = new List<Guid>();

			foreach (Guid key in _completedItems.Keys) {
				completedDocumentIds.Add(key);
			}

			return completedDocumentIds;
		}

		public IList<Guid> GetCompletedScriptsFor(Guid documentId) {
			IList<Guid> completedScripts = null;
			_completedItems.TryGetValue(documentId, out completedScripts);
			return completedScripts;
		}

	}

}
