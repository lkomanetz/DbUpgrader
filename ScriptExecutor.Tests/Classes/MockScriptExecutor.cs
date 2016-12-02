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

namespace ScriptExecutor.Tests.Classes {

	public class MockScriptExecutor : BaseScriptExecutor {

		public MockScriptExecutor(IScriptLoader scriptLoader, IBackingStore backingStore) :
			base(scriptLoader, backingStore) {
		}

		public override ExecutionResult Execute(ExecutionRequest request = null) {
			request = request ?? new ExecutionRequest();
			int docsCompleted = 0;
			int scriptsCompleted = 0;

			IList<ScriptDocument> docsToExecute = _backingStore.GetDocuments(
				new GetDocumentsRequest() {
					IsComplete = false
				}
			);

			for (short i = 0; i < docsToExecute.Count; ++i) {
				IList<Script> scriptsToRun = GetScriptsToRun(request, docsToExecute[i]);
				for (short j = 0; j < scriptsToRun.Count; ++j) {
					Execute(docsToExecute[i].Scripts[j]);
					++scriptsCompleted;
				}
				docsToExecute[i].IsComplete = true;
				_backingStore.Update(docsToExecute[i]);
				++docsCompleted;
			}

			return new ExecutionResult() {
				ScriptDocumentsCompleted = docsCompleted,
				ScriptsCompleted = scriptsCompleted
			};
		}

		private void Execute(Script script) {
			script.IsComplete = true;
			_backingStore.Update(script);
		}

		//TODO(Logan) -> Refactor this.  Possibly in the BaseScriptExecutor class.  I don't like where it is right now.
		private IList<Script> GetScriptsToRun(ExecutionRequest request, ScriptDocument doc) {
			if (request.ExecuteAllScripts) {
				return new List<Script>(doc.Scripts);
			}
			else {
				return doc.Scripts.Where(x => !x.IsComplete).ToList();
			}			
		}

	}

}
