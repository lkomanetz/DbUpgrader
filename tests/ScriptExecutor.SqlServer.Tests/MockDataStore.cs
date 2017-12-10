
using Executioner.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptExecutor.SqlServer.Tests {

	public class MockDataStore : IDataStore {

		public void CreateLogFile(Guid docId) { }

		public void Add(Script script) { }

		public void Add(ScriptDocument document) { }

		public void Clean() { }

		public IList<Guid> GetCompletedDocumentIds() { return new List<Guid>(); }

		public IList<Guid> GetCompletedScriptIdsFor(Guid documentId) { return new List<Guid>(); }

		public void Update(Script script) { }

	}

}