using Executioner.Contracts;
using Executioner.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Executioner {

	public class ScriptExecutioner : IExecutioner, IDisposable {
		private IScriptLoader _scriptLoader;
		private IDataStore _storage;

		public ScriptExecutioner(IScriptLoader loader, IDataStore storage) {
			_scriptLoader = loader;
			_storage = storage;

			_scriptLoader.LoadDocuments();

			this.ScriptExecutors = new List<IScriptExecutor>();
			CreateExecutors();
		}

		public IList<ScriptDocument> ScriptDocuments => _scriptLoader.Documents;
		public IList<IScriptExecutor> ScriptExecutors { get; private set; }
		public EventHandler<ScriptExecutedEventArgs> OnScriptExecuted;
		public EventHandler<ScriptExecutingEventArgs> OnScriptExecuting;

		protected virtual void Dispose(bool disposing) { }

		public void Dispose() => Dispose(true);

		public ExecutionResult Run(ExecutionRequest request = null) {
			if (ScriptExecutors == null || ScriptExecutors.Count == 0) {
				throw new InvalidOperationException("Unable to run ScriptExecutioner without any script executors.");
			}
			request = request ?? new ExecutionRequest();
			int docsCompleted = 0;
			int scriptsCompleted = 0;

			IList<ScriptDocument> docsToExecute = GetDocumentsToRun(request, _scriptLoader.Documents);
			for (short i = 0; i < docsToExecute.Count; ++i) {
				_storage.Add(docsToExecute[i]);

				IList<Script> scriptsToRun = GetScriptsToRun(request, docsToExecute[i]);
				scriptsToRun.ForEach(script => {
					_storage.Add(script);
					IScriptExecutor executor = FindExecutorFor(script.ExecutorName);
					Execute(executor, script);
					++scriptsCompleted;
				});

				++docsCompleted;
			}

			return new ExecutionResult() {
				ScriptDocumentsCompleted = docsCompleted,
				ScriptsCompleted = scriptsCompleted
			};
		}

		private IList<ScriptDocument> GetDocumentsToRun(ExecutionRequest request, IList<ScriptDocument> docs) {
			if (request.ExecuteAllScripts) {
				return new List<ScriptDocument>(docs);
			}

			IList<Guid> completedDocIds = _storage.GetCompletedDocumentIds();
			return docs
				.Where(doc => doc.Scripts.Any(script => !script.IsComplete))
				.ToList();
		}

		private IList<Script> GetScriptsToRun(ExecutionRequest request, ScriptDocument doc) {
			if (request.ExecuteAllScripts) {
				return new List<Script>(doc.Scripts);
			}

			IList<Guid> completedScriptIds = _storage.GetCompletedScriptIdsFor(doc.SysId);
			return doc.Scripts
				.Where(x => !completedScriptIds.Contains(x.SysId))
				.ToList();
		}

		private IScriptExecutor FindExecutorFor(string executorName) {
			if (this.ScriptExecutors == null) {
				string msg = "ScriptExecutioner.FindExecutorFor(string) failed.\n";
				msg += "ScriptExecutioner.ScriptExecutors is null.";
				throw new InvalidOperationException(msg);
			}

			IScriptExecutor foundExecutor = this.ScriptExecutors
				.Where(x => x.GetType().Name.Equals(executorName))
				.SingleOrDefault();

			if (foundExecutor == null) {
				throw new ScriptExecutorNotFoundException(executorName);
			}

			return foundExecutor;
		}

		private void CreateExecutors() {
			IEnumerable<Script> scripts = this.ScriptDocuments.SelectMany(x => x.Scripts);
			foreach (Script script in scripts) {
				string className = script.ExecutorName;
				if (String.IsNullOrEmpty(className)) {
					throw new ScriptExecutorNotFoundException(className);
				}

				bool typeExists = this.ScriptExecutors.Any(x => x.GetType().Name == script.ExecutorName);
				if (typeExists) {
					continue;
				}

				IScriptExecutor executor = ExecutorCreator.Create(script.ExecutorName);
				this.ScriptExecutors.Add(executor);
			}
		}

		private void Execute(IScriptExecutor executor, Script script) {
			OnScriptExecuting?.Invoke(this, new ScriptExecutingEventArgs(script));
			bool executed = executor.Execute(script.ScriptText);
			if (!executed) {
				throw new Exception($"Script id '{script.SysId}' failed to execute.");
			}

			script.IsComplete = true;
			_storage.Update(script);
			OnScriptExecuted?.Invoke(this, new ScriptExecutedEventArgs(script));
		}

	}

}