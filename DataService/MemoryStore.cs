﻿using BackingStore.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Executioner.Contracts;
using Executioner.ExtensionMethods;
using System.Runtime.CompilerServices;

namespace BackingStore {

	public class MemoryStore : IBackingStore {
		private IDictionary<Guid, ScriptDocument> _documents;

		public MemoryStore() {
			_documents = new Dictionary<Guid, ScriptDocument>();
		}

		public IList<ScriptDocument> GetDocuments(GetDocumentsRequest request) {
			request = request ?? new GetDocumentsRequest() { IncludeCompletedDocuments = true };
			if (_documents.Count == 0) {
				return null;
			}

			List<ScriptDocument> documents = new List<ScriptDocument>();

			foreach (var kvp in _documents) {
				documents.Add(kvp.Value);
			}

			if (request.IncludeCompletedDocuments) {
				documents = documents.Where(x => x.IsComplete || !x.IsComplete).ToList();
			}
			else {
				documents = documents.Where(x => !x.IsComplete).ToList();
			}

			return Sort(documents);
		}

		public IList<Script> GetScriptsFor(Guid documentId) {
			ScriptDocument doc = this.GetDocument(documentId);
			return Sort(doc.Scripts);
		}

		public void Add(ScriptDocument document) {
			if (!_documents.ContainsKey(document.SysId)) {
				_documents.Add(document.SysId, document);
			}
		}

		public void Add(Script script) {
			if (script == null) {
				throw new ArgumentNullException("script");
			}
			if (script.DocumentId == Guid.Empty) {
				throw new ArgumentNullException("script.DocumentId");
			}

			ScriptDocument doc = GetDocument(script.DocumentId);
			if (doc.Scripts.Any(x => x.SysId == script.SysId)) {
				throw new Exception($"Script id '{script.SysId}' already exists in document id '{doc.SysId}'.");
			}

			doc.Scripts.Add(script);
			doc.IsComplete = AreComplete(doc.Scripts);
		}

		public void Update(ScriptDocument document) {
			if (!_documents.ContainsKey(document.SysId)) {
				throw new Exception($"Document id '{document.SysId}' not found to update.");
			}

			_documents[document.SysId] = document;
		}

		public void Update(Script script) {
			ScriptDocument doc = GetDocument(script.DocumentId);
			int index = doc.Scripts.FindIndex(x => x.SysId == script.SysId);
			doc.Scripts[index] = script;
			doc.IsComplete = AreComplete(doc.Scripts);
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

		protected virtual void Dispose(bool disposing) {
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

		private IList<ScriptDocument> Sort(IList<ScriptDocument> docs) {
			docs = docs.OrderBy(doc => doc.DateCreatedUtc)
				.ThenBy(doc => doc.Order)
				.ToList();

			return docs;
		}

		private IList<Script> Sort(IList<Script> scripts) {
			scripts = scripts
				.OrderBy(script => script.DateCreatedUtc)
				.ThenBy(script => script.Order)
				.ToList();

			return scripts;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool AreComplete(IList<Script> scripts) {
			return scripts.All(x => x.IsComplete);
		}

	}

}
