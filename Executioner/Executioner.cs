﻿using Executioner.Contracts;
using Executioner.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Executioner {

	public class ScriptExecutioner : IExecutioner, IDisposable {
		private IScriptLoader _scriptLoader;
		private ILogger _logger;

		public ScriptExecutioner(IScriptLoader loader, ILogger logger) {
			_scriptLoader = loader;
			_logger = logger;

			_scriptLoader.LoadDocuments();
			this.ScriptExecutors = new List<IScriptExecutor>();
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

					executor.Execute(scriptsToRun[j].ScriptText);
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

		private IList<ScriptDocument> GetDocumentsToRun(ExecutionRequest request, IList<ScriptDocument> docs)
		{
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
				return doc.Scripts.Where(x => !completedScriptIds.Contains(x.SysId)).ToList();
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

	}

}
