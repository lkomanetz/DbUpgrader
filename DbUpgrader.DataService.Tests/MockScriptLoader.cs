using DbUpgrader.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbUpgrader.Contracts;
using System.Reflection;

namespace DbUpgrader.DataService.Tests {
	//TODO(Logan) -> Implement the mock loader for data service tests.
	public class MockScriptLoader : IScriptLoader {
		private IList<ScriptDocument> _scriptDocuments;

		public MockScriptLoader() {
			_scriptDocuments = new List<ScriptDocument>();
		}

		public IList<ScriptDocument> Documents {
			get { return _scriptDocuments; }
		}

		public void LoadDocuments(Assembly assembly) {
			throw new NotImplementedException();
		}
	}
}
