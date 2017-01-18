using Executioner.Contracts;
using Executioner.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace Executioner {

	//TODO(Logan) -> Add events for scripts that have loaded and executed.
	public class ScriptExecutioner : IExecutioner, IDisposable {
		private IScriptLoader _scriptLoader;
		private ILogger _logger;

		public ScriptExecutioner(IScriptLoader loader, ILogger logger) {
			_scriptLoader = loader;
			_logger = logger;

			_scriptLoader.LoadDocuments();

			this.ScriptExecutors = new List<IScriptExecutor>();
			CreateExecutors();
		}

		public IList<ScriptDocument> ScriptDocuments { get { return _scriptLoader.Documents; } }
		public IList<IScriptExecutor> ScriptExecutors { get; private set; }

		protected virtual void Dispose(bool disposing) { }

		public void Dispose() {
			Dispose(true);
		}

		public ExecutionResult Run(ExecutionRequest request = null) {
			if (ScriptExecutors == null || ScriptExecutors.Count == 0) {
				throw new InvalidOperationException("Unable to run ScriptExecutioner without any script executors.");
			}
			request = request ?? new ExecutionRequest();
			int docsCompleted = 0;
			int scriptsCompleted = 0;

			IList<ScriptDocument> docsToExecute = GetDocumentsToRun(request, _scriptLoader.Documents);
			for (short i = 0; i < docsToExecute.Count; ++i) {
				_logger.Add(docsToExecute[i]);
				IList<Script> scriptsToRun = GetScriptsToRun(request, docsToExecute[i]);
				for (short j = 0; j < scriptsToRun.Count; ++j) {
					IScriptExecutor executor = FindExecutorFor(scriptsToRun[j].ExecutorName);
					if (executor == null) {
						throw new NullReferenceException("ScriptExecutioner.FindExecutorFor(string) failed to find ScriptExecutor.");
					}

					bool executed = executor.Execute(scriptsToRun[j].ScriptText);
					if (!executed) {
						throw new Exception($"Script id '{scriptsToRun[j].SysId}' failed to execute.");
					}
					scriptsToRun[j].IsComplete = true;
					_logger.Update(scriptsToRun[j]);
					++scriptsCompleted;
				}
				docsToExecute[i].IsComplete = true;
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
			else {
				IList<Guid> completedDocIds = _logger.GetCompletedDocumentIds();
				return docs.Where(x => !completedDocIds.Contains(x.SysId))
					.ToList()
					.SortOrderedItems();
			}
		}

		private IList<Script> GetScriptsToRun(ExecutionRequest request, ScriptDocument doc) {
			if (request.ExecuteAllScripts) {
				return new List<Script>(doc.Scripts);
			}
			else {
				IList<Guid> completedScriptIds = _logger.GetCompletedScriptIdsFor(doc.SysId);
				return doc.Scripts
					.Where(x => !completedScriptIds.Contains(x.SysId))
					.ToList()
					.SortOrderedItems();
			}
		}

		private IScriptExecutor FindExecutorFor(string executorName) {
			if (this.ScriptExecutors == null) {
				string msg = "ScriptExecutioner.FindExecutorFor(string) failed.\n";
				msg += "ScriptExecutioner.ScriptExecutors is null.";
				throw new InvalidOperationException(msg);
			}

			return this.ScriptExecutors
				.Where(x => x.GetType().Name.Equals(executorName))
				.SingleOrDefault();
		}

		/*
		 * In order to get the type of the executor name passed in, I ended up having to go through all assemblies
		 * in the current application domain.  I wanted to keep the ScriptDocument simple without having to add more
		 * things to it in order to load the type through reflection.  This is more of a brute force method to find
		 * the type with just a name.
		 */
		private Type GetClassType(string executorName) {
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			Type objectType = null;

			foreach (Assembly assembly in assemblies) {
				objectType = assembly.GetTypes()
					.Where(x => x.IsClass && x.Name == executorName)
					.SingleOrDefault();
				
				if (objectType != null) {
					break;
				}	
			}

			return objectType;
		}

		internal void CreateExecutors() {
			foreach (ScriptDocument doc in this.ScriptDocuments) {
				foreach (Script script in doc.Scripts) {
					string className = script.ExecutorName;
					if (String.IsNullOrEmpty(className)) {
						throw new NullReferenceException("Script.ExecutorName property cannot be null.");
					}

					bool typeExists = this.ScriptExecutors.Any(x => x.GetType().Name == script.ExecutorName);
					if (typeExists) {
						continue;
					}

					Type objectType = GetClassType(script.ExecutorName);
					if (objectType == null) {
						throw new Exception($"Unable to find C# type for executor '{script.ExecutorName}'.");
					}

					IScriptExecutor executor = (IScriptExecutor)Activator.CreateInstance(objectType);
					this.ScriptExecutors.Add(executor);
				}
			}
		}

	}

}
