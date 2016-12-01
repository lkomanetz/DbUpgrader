using BackingStore.Contracts;
using Executioner.Contracts;
using ScriptExecutor.Contracts;
using ScriptLoader.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptExecutor.Tests {

	public class MockScriptExecutor : IScriptExecutor {
		private IBackingStore _backingStore;
		private IScriptLoader _scriptLoader;

		public MockScriptExecutor(IScriptLoader scriptLoader, IBackingStore backingStore) {
			_scriptLoader = scriptLoader;
			_backingStore = backingStore;
		}

		public IList<Guid> CompletedDocuments { get { return _backingStore.GetCompletedDocumentIds(); } }

		public void Execute() {
			IList<ScriptDocument> docsToExecute = _backingStore.GetDocuments();
			for (short i = 0; i < docsToExecute.Count; ++i) {
				for (short j = 0; j < docsToExecute[i].Scripts.Count; ++j) {
					Execute(docsToExecute[i].Scripts[j]);
				}
				docsToExecute[i].IsComplete = true;
				_backingStore.Update(docsToExecute[i]);
			}
		}

		private void Execute(Script script) {
			script.IsComplete = true;
			_backingStore.Update(script);
		}

	}

}
