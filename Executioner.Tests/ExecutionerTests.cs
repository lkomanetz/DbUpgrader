using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Executioner.Contracts;
using System.Linq;
using Executioner.Tests.Classes;
using System.Reflection;

namespace Executioner.Tests {

	[TestClass]
	public class ExecutionerTests {
		private static ILogger _logger;

		[TestCleanup]
		public void Cleanup() {
			_logger.Clean();
		}

		[TestInitialize]
		public void Initialize() {
			_logger = new MockLogger();
		}

		[TestMethod]
		public void CreateExecutorsFromScriptsSucceeds() {
			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='MockScriptExecutor' Order='2016-06-21'></Script>"
			};
			var loader = new BaseMockLoader(scripts);
			ScriptExecutioner executioner = new ScriptExecutioner(loader, _logger);

			Assert.IsTrue(
				executioner.ScriptExecutors.Count == 1,
				$"Expected {1} executor\nActual count {executioner.ScriptExecutors.Count}"
			);

			Assert.IsTrue(
				executioner.ScriptExecutors[0].GetType() == typeof(MockScriptExecutor),
				"Incorrect executor loaded."
			);
		}

		[TestMethod]
		public void CreateMultipleExecutorsFromScriptsSucceeds() {
			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='MockScriptExecutor' Order='2016-06-21'></Script>",
				$"<Script Id='{Guid.NewGuid()}' Executor='SecondScriptExecutor' Order='2016-06-21'></Script>"
			};
			var loader = new BaseMockLoader(scripts);
			ScriptExecutioner executioner = new ScriptExecutioner(loader, _logger);
			SetExecutionStatus(executioner, true);

			Assert.IsTrue(
				executioner.ScriptExecutors.Count == scripts.Length,
				$"Expected {scripts.Length} executors\nActual count {executioner.ScriptExecutors.Count}"
			);
		}

		[TestMethod]
		public void CreateExecutorFromMultipleScriptsSucceeds() {
			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='MockScriptExecutor' Order='2016-06-21'></Script>",
				$"<Script Id='{Guid.NewGuid()}' Executor='MockScriptExecutor' Order='2016-06-21'></Script>"
			};
			var loader = new BaseMockLoader(scripts);
			ScriptExecutioner executioner = new ScriptExecutioner(loader, _logger);
			SetExecutionStatus(executioner, true);

			Assert.IsTrue(
				executioner.ScriptExecutors.Count == 1,
				$"Expected {1} executor\nActual count {executioner.ScriptExecutors.Count}"
			);
		}

		[TestMethod]
		[ExpectedException(typeof(Exception), "Executor loaded with full namespace used.")]
		public void CreateExecutorWithNamespaceFails() {
			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='Executioner.Tests.Classes.MockScriptExecutor' Order='2016-06-21'></Script>"
			};
			var loader = new BaseMockLoader(scripts);
			ScriptExecutioner executioner = new ScriptExecutioner(loader, _logger);
		}

		[TestMethod]
		public void ExecuteUpdatesCompletionProperty() {
			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='MockScriptExecutor' Order='2016-06-21'></Script>"
			};
			ScriptExecutioner executioner = new ScriptExecutioner(new BaseMockLoader(scripts), _logger);
			SetExecutionStatus(executioner, true);
			var result = executioner.Run();

			Assert.IsTrue(
				result.ScriptDocumentsCompleted == 1,
				"Script executor did not complete the document loaded by script loader."
			);

			IList<ScriptDocument> docs = executioner.ScriptDocuments;
			foreach (ScriptDocument doc in docs) {
				Assert.IsTrue(doc.IsComplete, "Executor failed to complete a script document.");
				Assert.IsTrue(
					!doc.Scripts.Any(x => x.IsComplete == false),
					"Executor failed to complete a script."
				);
			}
		}

		[TestMethod]
		public void ExecuteOnlyRunsNonCompletedDocuments() {
			string[] scripts = new string[] {
				"<Script Id='278b7ef3-09da-4d1b-a101-390f4e6a5407' Executor='MockScriptExecutor' Order='2016-06-21'></Script>"
			};
			ScriptExecutioner executioner = new ScriptExecutioner(new BaseMockLoader(scripts), _logger);
			SetExecutionStatus(executioner, true);

			var result = executioner.Run();
			var secondResult = executioner.Run();

			Assert.IsTrue(result != secondResult, "Multiple executes should not produce same result.");
		}

		[TestMethod]
		public void ExecuteOnlyRunsNewScripts() {
			List<string> scripts = new List<string>() {
				$"<Script Id='{Guid.NewGuid()}' Executor='MockScriptExecutor' Order='2016-06-21'></Script>",
				$"<Script Id='{Guid.NewGuid()}' Executor='MockScriptExecutor' Order='2016-06-21:1'></Script>"
			};
			BaseMockLoader loader = new BaseMockLoader(scripts.ToArray());

			ScriptExecutioner executioner = new ScriptExecutioner(loader, _logger);
			SetExecutionStatus(executioner, true);
			var firstResult = executioner.Run();

			IList<ScriptDocument> docs = executioner.ScriptDocuments;
			Assert.IsTrue(
				docs.Count == 1,
				$"Unit test expecting only one script document.\nActul: {docs.Count}"
			);

			short scriptsToAdd = 1;
			for (short i = 0; i < scriptsToAdd; ++i) {
				Script newScript = new Script() {
					SysId = Guid.NewGuid(),
					DateCreatedUtc = DateTime.UtcNow,
					Order = 0,
					ExecutorName = "MockScriptExecutor",
					IsComplete = false,
					DocumentId = docs[0].SysId,
					ScriptText = String.Empty
				};

				loader.Add(newScript);	
			}

			executioner = new ScriptExecutioner(loader, _logger);
			SetExecutionStatus(executioner, true);
			var secondResult = executioner.Run();
			Assert.IsTrue(
				secondResult.ScriptsCompleted != firstResult.ScriptsCompleted,
				"Incorrect number of scripts executed on second run."
			);
			Assert.IsTrue(
				secondResult.ScriptsCompleted == scriptsToAdd,
				$"Expected {scriptsToAdd} scripts to run.\nActual scripts ran = {secondResult.ScriptsCompleted}."
			);
		}

		[TestMethod]
		public void AddingMultipleExecutorsSucceeds() {
			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='MockScriptExecutor' Order='2016-06-21'></Script>",
				$"<Script Id='{Guid.NewGuid()}' Executor='SecondScriptExecutor' Order='2016-06-21:1'></Script>"
			};

			ScriptExecutioner executioner = new ScriptExecutioner(new BaseMockLoader(scripts), _logger);
			SetExecutionStatus(executioner, true);
			IList<IScriptExecutor> itemsFound = executioner.ScriptExecutors
				.Where(
					x => x.GetType() == typeof(SecondScriptExecutor) ||
						x.GetType() == typeof(MockScriptExecutor)
				)
				.ToList();

			Assert.IsTrue(itemsFound.Count == 2, "Executioner.Add() failed to add both executors.");
			bool typesFound = itemsFound.Where(
					x => x.GetType() == typeof(SecondScriptExecutor) ||
						x.GetType() == typeof(MockScriptExecutor)
				)
				.Count() == 2;
			Assert.IsTrue(typesFound, "Unable to find either executor added to Executioner class.");
		}

		[TestMethod]
		public void AddingIgnoresExecutorsThatAlreadyExist() {
			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='MockScriptExecutor' Order='2016-06-21'></Script>",
				$"<Script Id='{Guid.NewGuid()}' Executor='SecondScriptExecutor' Order='2016-06-21:1'></Script>"
			};
			ScriptExecutioner executioner = new ScriptExecutioner(new BaseMockLoader(scripts), _logger);
			SetExecutionStatus(executioner, true);

			Assert.IsTrue(executioner.ScriptExecutors.Count == 2, "Executioner.Add() added same executor twice.");
			IList<IScriptExecutor> items = executioner.ScriptExecutors
				.Where(
					x => x.GetType() == typeof(MockScriptExecutor) ||
						x.GetType() == typeof(SecondScriptExecutor)
				)
				.ToList();
			Assert.IsTrue(items.Count == 2, "Executioner.Add() added same executor twice.");
		}

		[TestMethod]
		public void ScriptDocumentWithMultipleExecutorsSucceeds() {
			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='MockScriptExecutor' Order='2016-06-21'></Script>",
				$"<Script Id='{Guid.NewGuid()}' Executor='SecondScriptExecutor' Order='2016-06-21:1'></Script>"
			};
			ScriptExecutioner executioner = new ScriptExecutioner(new BaseMockLoader(scripts), _logger);
			SetExecutionStatus(executioner, true);
			var result = executioner.Run(new ExecutionRequest() { ExecuteAllScripts = true });
			Assert.IsTrue(
				result.ScriptDocumentsCompleted == 1,
				"Executioner failed to complete document with multiple executors."
			);
			Assert.IsTrue(
				result.ScriptsCompleted == scripts.Length,
				"Executioner failed to complete all scripts with multiple executors."
			);
		}

		[TestMethod]
		[ExpectedException(typeof(NullReferenceException), "Executioner.ScriptExecutors was not null.")]
		public void ExecutionerWithNoExecutorsFail() {
			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Order='2016-06-21'></Script>",
				$"<Script Id='{Guid.NewGuid()}' Order='2016-06-21:1'></Script>"
			};
			ScriptExecutioner executioner = new ScriptExecutioner(new BaseMockLoader(scripts), _logger);
			SetExecutionStatus(executioner, true);
			executioner.Run();
		}

		[TestMethod]
		public void ExecutionerCanRunAllScriptsMultipleTimes() {
			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='MockScriptExecutor' Order='2016-06-21'></Script>",
				$"<Script Id='{Guid.NewGuid()}' Executor='MockScriptExecutor' Order='2016-06-21:1'></Script>"
			};
			ScriptExecutioner executioner = new ScriptExecutioner(new BaseMockLoader(scripts), _logger);
			SetExecutionStatus(executioner, true);

			var firstResult = executioner.Run();
			var secondResult = executioner.Run(new ExecutionRequest() { ExecuteAllScripts = true });

			string errorMsg = String.Format(
				"Executioner results unequal.\nFirstResult completed {0} docs\nSecondResult completed {1} docs",
				firstResult.ScriptDocumentsCompleted,
				secondResult.ScriptDocumentsCompleted
			);
			Assert.IsTrue(
				firstResult.ScriptDocumentsCompleted == firstResult.ScriptDocumentsCompleted,
				errorMsg
			);

			errorMsg = String.Format(
				"Executioner results unequal.\nFirstResult completed {0} scripts\nSecondResult completed {1} scripts",
				firstResult.ScriptsCompleted,
				secondResult.ScriptsCompleted
			);
			Assert.IsTrue(
				firstResult.ScriptsCompleted == secondResult.ScriptsCompleted,
				errorMsg
			);
		}

		[TestMethod]
		public void MultipleInstantiationsExecutesCorrectNumberOfScripts() {
			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='MockScriptExecutor' Order='2016-06-21'></Script>",
				$"<Script Id='{Guid.NewGuid()}' Executor='MockScriptExecutor' Order='2016-06-21:1'></Script>"
			};
			ScriptExecutioner executioner = new ScriptExecutioner(new BaseMockLoader(scripts), _logger);
			SetExecutionStatus(executioner, true);
			var firstResult = executioner.Run();

			executioner = new ScriptExecutioner(new BaseMockLoader(scripts), _logger);
			SetExecutionStatus(executioner, true);
			var secondResult = executioner.Run();

			Assert.IsTrue(
				secondResult.ScriptDocumentsCompleted == 0 && secondResult.ScriptsCompleted == 0,
				"Incorrect number of scripts ran on second run."
			);
		}

		[TestMethod]
		[ExpectedException(typeof(Exception), "Failed script did not throw exception.")]
		public void FailedScriptThrowsException() {
			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='MockScriptExecutor' Order='2016-06-21'></Script>",
				$"<Script Id='{Guid.NewGuid()}' Executor='MockScriptExecutor' Order='2016-06-21:1'></Script>"
			};
			ScriptExecutioner executioner = new ScriptExecutioner(new BaseMockLoader(scripts), _logger);
			SetExecutionStatus(executioner, false);
			executioner.Run();
		}

		[TestMethod]
		public void ScriptEventsSucceeds() {
			IList<Guid> executedScripts = new List<Guid>();
			IList<Guid> executingScripts = new List<Guid>();
			Guid scriptId = Guid.NewGuid();
			string[] scripts = new string[] {
				$"<Script Id='{scriptId}' Executor='MockScriptExecutor' Order='2016-06-21'></Script>"
			};

			ScriptExecutioner executioner = new ScriptExecutioner(new BaseMockLoader(scripts), _logger);
			SetExecutionStatus(executioner, true);
			executioner.OnScriptExecuted += (obj, args) => {
				executedScripts.Add(args.Script.SysId);
			};
			executioner.OnScriptExecuting += (obj, args) => {
				executingScripts.Add(args.Script.SysId);
			};
			executioner.Run();

			Assert.IsTrue(executedScripts.Count == scripts.Length, "Incorrect number of scripts executed.");
			Assert.IsTrue(executedScripts.Contains(scriptId), $"Script Id '{scriptId}' not found.");
			Assert.IsTrue(
				executedScripts.Except(executingScripts).Count() == 0,
				"Script events not in sync with what is executing and executed."
			);
		}

		private void SetExecutionStatus(ScriptExecutioner executioner, bool executed) {
			MockScriptExecutor mockExecutor = (MockScriptExecutor)executioner.ScriptExecutors
				.Where(x => x.GetType() == typeof(MockScriptExecutor))
				.SingleOrDefault();

			if (mockExecutor != null) {
				mockExecutor.ScriptExecuted = executed;
			}

			SecondScriptExecutor secondExecutor = (SecondScriptExecutor)executioner.ScriptExecutors
				.Where(x => x.GetType() == typeof(SecondScriptExecutor))
				.SingleOrDefault();

			if (secondExecutor != null) {
				secondExecutor.ScriptExecuted = executed;
			}
		}

	}

}
