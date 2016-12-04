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
			if (ScriptExecutors == null || ScriptExecutors.Count == 0) {
				throw new InvalidOperationException("Unable to run Executioner without any script executors.");
			}
			request = request ?? new ExecutionRequest();
			int docsCompleted = 0;
			int scriptsCompleted = 0;

			//TODO(Logan) -> This is ugly and needs to be changed to an enumeration.
			bool? getCompleteDocs = (request.ExecuteAllScripts) ? null : (bool?)false;
			var docsToExecute = _storage.GetDocuments(
				new GetDocumentsRequest() {
					IsComplete = getCompleteDocs
				}
			);

			for (short i = 0; i < docsToExecute.Count; ++i) {
				IList<Script> scriptsToRun = GetScriptsToRun(request, docsToExecute[i]);
				for (short j = 0; j < scriptsToRun.Count; ++j) {
					IScriptExecutor executor = FindExecutorFor(scriptsToRun[j].ExecutorName);
					if (executor == null) {
						throw new NullReferenceException("Executioner.FindExecutorFor(string) failed to find ScriptExecutor.");
					}

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

		public void Add(IScriptExecutor executor) {
			bool executorExists = this.ScriptExecutors
				.Any(x => x.GetType().Name.Equals(executor.GetType().Name));

			if (executorExists) {
				return;
			}

			this.ScriptExecutors.Add(executor);
		}

		public void Add(IScriptExecutor[] executors) {
			for (short i = 0; i < executors.Length; ++i) {
				bool typeExists = this.ScriptExecutors.Any(x => x.GetType() == executors[i].GetType());
				if (typeExists) {
					continue;
				}
				this.ScriptExecutors.Add(executors[i]);
			}
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
				string msg = "Executioner.FindExecutoFor(string) failed.\n";
				msg += "Executioner.ScriptExecutors is null.";
				throw new InvalidOperationException(msg);
			}

			return this.ScriptExecutors
				.Where(x => x.GetType().Name.Equals(executorName))
				.SingleOrDefault();
		}

	}

}
