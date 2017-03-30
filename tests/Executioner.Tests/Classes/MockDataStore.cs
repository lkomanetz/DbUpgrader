using Executioner.Contracts;
using Executioner.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Executioner.Tests.Classes
{
	public class MockDataStore : IDataStore {
		private IDictionary<Guid, ScriptDocument> _log;

		public MockDataStore() {
			_log = new Dictionary<Guid, ScriptDocument>();
		}

		public void Add(Script script) {
			ScriptDocument doc = GetDocument(script.DocumentId);

			int index = doc.Scripts.FindIndex(x => x.SysId == script.SysId);
			if (index > -1) {
				doc.Scripts[index] = script;
			}
			else {
				doc.Scripts.Add(script);
			}
			doc.Scripts = (List<Script>)doc.Scripts.SortOrderedItems();
			doc.IsComplete = doc.Scripts.All(x => x.IsComplete);
		}

		public void Add(ScriptDocument document) {
			if (!_log.ContainsKey(document.SysId)) {
				_log.Add(document.SysId, document);
			}
		}

		public void Clean() {
			_log.Clear();
		}

		public IList<Guid> GetCompletedDocumentIds() {
			return _log.Where(x => x.Value.IsComplete)
				.Select(x => x.Key)
				.ToList();
		}

		public IList<Guid> GetCompletedScriptIdsFor(Guid documentId) {
			ScriptDocument doc = GetDocument(documentId);
			if (doc == null) {
				return null;
			}

			return doc.Scripts
				.Where(x => x.IsComplete)
				.Select(x => x.SysId)
				.ToList();
		}

		public void Update(Script script) {
			ScriptDocument doc = GetDocument(script.DocumentId);
			int index = doc.Scripts.FindIndex(x => x.SysId == script.SysId);
			doc.Scripts[index] = script;
			doc.IsComplete = doc.Scripts.All(x => x.IsComplete);
		}

		private ScriptDocument GetDocument(Guid docId) {
			KeyValuePair<Guid, ScriptDocument> kvp = _log
				.Where(x => x.Key == docId)
				.SingleOrDefault();

			if (kvp.Key != docId) {
				return null;
			}
			else {
				return kvp.Value;
			}
		}

	}

}