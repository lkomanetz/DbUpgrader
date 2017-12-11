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

		public MockDataStore() => _log = new Dictionary<Guid, ScriptDocument>();

		public void CreateLogFile(Guid docId) {}

		public void Add(Script script) {
			ScriptDocument doc = GetDocument(script.DocumentId);
			bool addNewDoc = (doc == null);
			if (addNewDoc) doc = new ScriptDocument() { SysId = script.DocumentId };

			int index = doc.Scripts.FindIndex(x => x.SysId == script.SysId);
			if (index > -1) doc.Scripts[index] = script;
			else doc.Scripts.Add(script);
			
			doc.Scripts = (List<Script>)doc.Scripts.SortOrderedItems();
			doc.IsComplete = doc.Scripts.All(x => x.IsComplete);

			if (addNewDoc) Add(doc);
		}

		public void Add(ScriptDocument document) {
			if (!_log.ContainsKey(document.SysId)) {
				_log.Add(document.SysId, document);
			}
		}

		public void Clean() => _log.Clear();

		public IList<Guid> GetCompletedScriptIdsFor(Guid documentId) {
			ScriptDocument doc = GetDocument(documentId);
			if (doc == null) return new List<Guid>();
			
			return doc.Scripts
				.Where(x => x.IsComplete)
				.Select(x => x.SysId)
				.ToList();
		}

		private ScriptDocument GetDocument(Guid docId) {
			KeyValuePair<Guid, ScriptDocument> kvp = _log
				.Where(x => x.Key == docId)
				.SingleOrDefault();

			if (kvp.Key != docId) {
				return null;
			}
			else {
				if (kvp.Value.Scripts == null) kvp.Value.Scripts = new List<Script>();
				return kvp.Value;
			}
		}

	}

}