using Executioner.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptLoader.Contracts;
using BackingStore.Contracts;
using ScriptExecutor.Contracts;

namespace Executioner {

	//TODO(Logan) -> Rename namespaces so they make more sense.
	public class Executioner : IExecutioner, IDisposable {
		private IScriptLoader _scriptLoader;
		private IBackingStore _storage;

		public Executioner(IScriptLoader loader, IBackingStore storage) {
			_scriptLoader = loader;
			_storage = storage;

			_scriptLoader.LoadDocuments(_storage);
			this.ScriptExecutors = new List<IScriptExecutor>();
		}

		public IList<ScriptDocument> ScriptDocuments { get { return _storage.GetDocuments(); } }
		public IList<IScriptExecutor> ScriptExecutors { get; private set; }

		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				_storage.Dispose();
			}
		}

		public void Dispose() {
			Dispose(true);
		}

		public ExecutionResult Run(ExecutionRequest request = null) {
			if (ScriptExecutors == null) {
				throw new InvalidOperationException("Unable to run Executioner without any script executors.");
			}
			request = request ?? new ExecutionRequest();
			int docsCompleted = 0;
			int scriptsCompleted = 0;

			var docsToExecute = _storage.GetDocuments(
				new GetDocumentsRequest() {
					IsComplete = request.ExecuteAllScripts
				}
			);

			for (short i = 0; i < docsToExecute.Count; ++i) {
				IList<Script> scriptsToRun = GetScriptsToRun(request, docsToExecute[i]);
				for (short j = 0; j < scriptsToRun.Count; ++j) {
					IScriptExecutor executor = FindExecutorFor(scriptsToRun[j].ExecutorName);
					executor.Execute(scriptsToRun[j].ScriptText);
					scriptsToRun[j].IsComplete = true;
					_storage.Update(scriptsToRun[j]);
					++scriptsCompleted;
				}
				docsToExecute[i].IsComplete = true;
				_storage.Update(docsToExecute[i]);
				++docsCompleted;
			}

			return new ExecutionResult() {
				ScriptDocumentsCompleted = docsCompleted,
				ScriptsCompleted = scriptsCompleted
			};
		}

		//TODO(Logan) -> Add overloaded method to add an array of IScriptExecutor items.
		public void AddExecutor(IScriptExecutor executor) {
			if (this.ScriptExecutors.Any(x => x.GetType().Name.Equals(executor.GetType().Name))) {
				return;
			}

			this.ScriptExecutors.Add(executor);
		}

		private IList<Script> GetScriptsToRun(ExecutionRequest request, ScriptDocument doc) {
			if (request.ExecuteAllScripts) {
				return new List<Script>(doc.Scripts);
			}
			else {
				return doc.Scripts.Where(x => !x.IsComplete).ToList();
			}
		}

		private IScriptExecutor FindExecutorFor(string executorName) {
			if (this.ScriptExecutors == null) {
				return null;
			}

			return this.ScriptExecutors
				.Where(x => x.GetType().Name.Equals(executorName))
				.SingleOrDefault();
		}

	}

}
