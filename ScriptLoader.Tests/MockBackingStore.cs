using BackingStore.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Executioner.Contracts;

namespace ScriptLoader.Tests {

	/*
	 * I purposely don't want this object to do anything.  It's only being used in the 
	 * unit tests of the AssemblyLoader.
	 */
	public class MockBackingStore : IBackingStore {

		public void Add(ScriptDocument document) { }
		public void Add(Script script) { }
		public void Clean() { }
		public bool Delete(Script script) { return true; }
		public bool Delete(ScriptDocument document) { return true; }
		public IList<Guid> GetCompletedDocumentIds() { return null; }
		public IList<Guid> GetCompletedScriptIdsFor(Guid documentId) { return null; }
		public IList<ScriptDocument> GetDocuments(GetDocumentsRequest request) { return null; }
		public IList<Script> GetScriptsFor(Guid documentId) { return null; }
		public void Update(ScriptDocument document) { }
		public void Update(Script script) { }
		public void Dispose() { }

	}

}
