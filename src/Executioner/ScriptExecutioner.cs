using Executioner.Contracts;
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
				for (short j = 0; j < scriptsToRun.Count; ++j) {
					IScriptExecutor executor = FindExecutorFor(scriptsToRun[j].ExecutorName);
					Execute(executor, scriptsToRun[j]);
					++scriptsCompleted;
				}
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

			AddNewScriptsToStorage(request, docs);
			IList<Guid> completedDocIds = _storage.GetCompletedDocumentIds();
			return docs.Where(x => !completedDocIds.Contains(x.SysId))
				.ToList();
		}

		private IList<Script> GetScriptsToRun(ExecutionRequest request, ScriptDocument doc) {
			if (request.ExecuteAllScripts) {
				return new List<Script>(doc.Scripts);
			}

			IList<Guid> completedScriptIds = _storage.GetCompletedScriptIdsFor(doc.SysId);
			if (completedScriptIds == null) {
				return null;
			}

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

		private void AddNewScriptsToStorage(ExecutionRequest request, IList<ScriptDocument> docs) {
			foreach (ScriptDocument doc in docs) {
				IList<Script> scriptsToRun = GetScriptsToRun(request, doc);
				if (scriptsToRun == null) {
					continue;
				}

				foreach (Script script in scriptsToRun) {
					_storage.Add(script);
				}
			}
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
				Type[] executorTypes = FindExecutorsIn(assembly);

				objectType = executorTypes
					.Where(x => x.Name.Equals(executorName))
					.SingleOrDefault();
				
				if (objectType != null) {
					break;
				}	
			}

			return objectType;
		}

		private Type[] FindExecutorsIn(Assembly assembly) {
			string interfaceName = typeof(IScriptExecutor).Name;
			return assembly.GetTypes()
				.Where(x => x.GetInterfaces().Any(y => y.Name.Equals(interfaceName)))
				.ToArray<Type>();
		}

		private void CreateExecutors() {
			foreach (ScriptDocument doc in this.ScriptDocuments) {
				foreach (Script script in doc.Scripts) {
					string className = script.ExecutorName;
					if (String.IsNullOrEmpty(className)) {
						throw new ScriptExecutorNotFoundException(className);
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