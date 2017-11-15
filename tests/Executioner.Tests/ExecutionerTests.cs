using System;
using System.Collections.Generic;
using Executioner.Contracts;
using System.Linq;
using System.Reflection;
using Executioner.Tests.Classes;
using Xunit;

namespace Executioner.Tests {

	public class ExecutionerTests {
		private static IDataStore _logger;

		public ExecutionerTests() {
			_logger = new MockDataStore();
		}

		[Fact]
		public void CreateExecutorsFromScriptsSucceeds() {
			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='MockScriptExecutor' Order='2016-06-21'></Script>"
			};
			var loader = new BaseMockLoader(scripts);
			ScriptExecutioner executioner = new ScriptExecutioner(loader, _logger);

			Assert.True(
				executioner.ScriptExecutors.Count == 1,
				$"Expected {1} executor\nActual count {executioner.ScriptExecutors.Count}"
			);

			Assert.True(
				executioner.ScriptExecutors[0].GetType() == typeof(MockScriptExecutor),
				"Incorrect executor loaded."
			);
			_logger.Clean();
		}

		[Fact]
		public void CreateMultipleExecutorsFromScriptsSucceeds() {
			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='MockScriptExecutor' Order='2016-06-21'></Script>",
				$"<Script Id='{Guid.NewGuid()}' Executor='SecondScriptExecutor' Order='2016-06-21'></Script>"
			};
			var loader = new BaseMockLoader(scripts);
			ScriptExecutioner executioner = new ScriptExecutioner(loader, _logger);
			SetExecutionStatus(executioner, true);

			Assert.True(
				executioner.ScriptExecutors.Count == scripts.Length,
				$"Expected {scripts.Length} executors\nActual count {executioner.ScriptExecutors.Count}"
			);

			_logger.Clean();
		}

		[Fact]
		public void CreateExecutorFromMultipleScriptsSucceeds() {
			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='MockScriptExecutor' Order='2016-06-21'></Script>",
				$"<Script Id='{Guid.NewGuid()}' Executor='MockScriptExecutor' Order='2016-06-21'></Script>"
			};
			var loader = new BaseMockLoader(scripts);
			ScriptExecutioner executioner = new ScriptExecutioner(loader, _logger);
			SetExecutionStatus(executioner, true);

			Assert.True(
				executioner.ScriptExecutors.Count == 1,
				$"Expected {1} executor\nActual count {executioner.ScriptExecutors.Count}"
			);
			_logger.Clean();
		}

		[Fact]
		public void CreateExecutorWithNamespaceFails() {
			Exception ex = Record.Exception(() => {
				string[] scripts = new string[] {
					$"<Script Id='{Guid.NewGuid()}' Executor='Executioner.Tests.Classes.MockScriptExecutor' Order='2016-06-21'></Script>"
				};
				var loader = new BaseMockLoader(scripts);
				ScriptExecutioner executioner = new ScriptExecutioner(loader, _logger);
			});
			Assert.NotNull(ex);
			_logger.Clean();
		}

		[Fact]
		public void ExecuteUpdatesCompletionProperty() {
			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='MockScriptExecutor' Order='2016-06-21'></Script>"
			};
			ScriptExecutioner executioner = new ScriptExecutioner(new BaseMockLoader(scripts), _logger);
			SetExecutionStatus(executioner, true);
			var result = executioner.Run();

			Assert.True(
				result.ScriptDocumentsCompleted == 1,
				"Script executor did not complete the document loaded by script loader."
			);

			IList<ScriptDocument> docs = executioner.ScriptDocuments;
			foreach (ScriptDocument doc in docs) {
				Assert.True(doc.IsComplete, "Executor failed to complete a script document.");
				Assert.True(
					!doc.Scripts.Any(x => x.IsComplete == false),
					"Executor failed to complete a script."
				);
			}
			_logger.Clean();
		}

		[Fact]
		public void ExecuteOnlyRunsNonCompletedDocuments() {
			string[] scripts = new string[] {
				"<Script Id='278b7ef3-09da-4d1b-a101-390f4e6a5407' Executor='MockScriptExecutor' Order='2016-06-21'></Script>"
			};
			ScriptExecutioner executioner = new ScriptExecutioner(new BaseMockLoader(scripts), _logger);
			SetExecutionStatus(executioner, true);

			var result = executioner.Run();
			var secondResult = executioner.Run();

			Assert.True(result != secondResult, "Multiple executes should not produce same result.");
			Assert.True(
				secondResult.ScriptDocumentsCompleted == 0,
				$"No documents should have completed on second run. {secondResult.ScriptDocumentsCompleted} have completed."
			);
			_logger.Clean();
		}

		[Fact]
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
			Assert.True(
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
				_logger.Add(newScript);
			}

			executioner = new ScriptExecutioner(loader, _logger);
			Assert.True(loader.Documents.Count == 1, $"Expecting 1 document.  Actual: {loader.Documents.Count}");
			SetExecutionStatus(executioner, true);
			var secondResult = executioner.Run();
			Assert.True(
				secondResult.ScriptsCompleted != firstResult.ScriptsCompleted,
				"Incorrect number of scripts executed on second run."
			);
			Assert.True(
				secondResult.ScriptsCompleted == scriptsToAdd,
				$"Expected {scriptsToAdd} scripts to run.\nActual scripts ran = {secondResult.ScriptsCompleted}."
			);
			_logger.Clean();
		}

		[Fact]
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

			Assert.True(itemsFound.Count == 2, "Executioner.Add() failed to add both executors.");
			bool typesFound = itemsFound.Where(
					x => x.GetType() == typeof(SecondScriptExecutor) ||
						x.GetType() == typeof(MockScriptExecutor)
				)
				.Count() == 2;
			Assert.True(typesFound, "Unable to find either executor added to Executioner class.");
			_logger.Clean();
		}

		[Fact]
		public void AddingIgnoresExecutorsThatAlreadyExist() {
			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='MockScriptExecutor' Order='2016-06-21'></Script>",
				$"<Script Id='{Guid.NewGuid()}' Executor='SecondScriptExecutor' Order='2016-06-21:1'></Script>"
			};
			ScriptExecutioner executioner = new ScriptExecutioner(new BaseMockLoader(scripts), _logger);
			SetExecutionStatus(executioner, true);

			Assert.True(executioner.ScriptExecutors.Count == 2, "Executioner.Add() added same executor twice.");
			IList<IScriptExecutor> items = executioner.ScriptExecutors
				.Where(
					x => x.GetType() == typeof(MockScriptExecutor) ||
						x.GetType() == typeof(SecondScriptExecutor)
				)
				.ToList();
			Assert.True(items.Count == 2, "Executioner.Add() added same executor twice.");
			_logger.Clean();
		}

		[Fact]
		public void ScriptDocumentWithMultipleExecutorsSucceeds() {
			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='MockScriptExecutor' Order='2016-06-21'></Script>",
				$"<Script Id='{Guid.NewGuid()}' Executor='SecondScriptExecutor' Order='2016-06-21:1'></Script>"
			};
			ScriptExecutioner executioner = new ScriptExecutioner(new BaseMockLoader(scripts), _logger);
			SetExecutionStatus(executioner, true);
			var result = executioner.Run(new ExecutionRequest() { ExecuteAllScripts = true });
			Assert.True(
				result.ScriptDocumentsCompleted == 1,
				"Executioner failed to complete document with multiple executors."
			);
			Assert.True(
				result.ScriptsCompleted == scripts.Length,
				"Executioner failed to complete all scripts with multiple executors."
			);
			_logger.Clean();
		}

		[Fact]
		public void ExecutionerWithNoExecutorsFail() {
			Exception ex = Record.Exception(() => {
				string[] scripts = new string[] {
					$"<Script Id='{Guid.NewGuid()}' Order='2016-06-21'></Script>",
					$"<Script Id='{Guid.NewGuid()}' Order='2016-06-21:1'></Script>"
				};
				ScriptExecutioner executioner = new ScriptExecutioner(new BaseMockLoader(scripts), _logger);
				SetExecutionStatus(executioner, true);
				executioner.Run();
			});

			Assert.NotNull(ex);
			_logger.Clean();
		}

		[Fact]
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
			Assert.True(
				firstResult.ScriptDocumentsCompleted == firstResult.ScriptDocumentsCompleted,
				errorMsg
			);

			errorMsg = String.Format(
				"Executioner results unequal.\nFirstResult completed {0} scripts\nSecondResult completed {1} scripts",
				firstResult.ScriptsCompleted,
				secondResult.ScriptsCompleted
			);
			Assert.True(
				firstResult.ScriptsCompleted == secondResult.ScriptsCompleted,
				errorMsg
			);
			_logger.Clean();
		}

		[Fact]
		public void MultipleInstantiationsExecutesCorrectNumberOfScripts() {
			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='MockScriptExecutor' Order='2016-06-21'></Script>",
				$"<Script Id='{Guid.NewGuid()}' Executor='MockScriptExecutor' Order='2016-06-21:1'></Script>"
			};
			var loader = new BaseMockLoader(scripts);
			ScriptExecutioner executioner = new ScriptExecutioner(loader, _logger);
			SetExecutionStatus(executioner, true);
			var firstResult = executioner.Run();

			executioner = new ScriptExecutioner(loader, _logger);
			SetExecutionStatus(executioner, true);
			var secondResult = executioner.Run();

			Assert.True(
				secondResult.ScriptDocumentsCompleted == 0 && secondResult.ScriptsCompleted == 0,
				"Incorrect number of scripts ran on second run."
			);
			_logger.Clean();
		}

		[Fact]
		public void FailedScriptThrowsException() {
			Exception ex = Record.Exception(() => {
				string[] scripts = new string[] {
					$"<Script Id='{Guid.NewGuid()}' Executor='MockScriptExecutor' Order='2016-06-21'></Script>",
					$"<Script Id='{Guid.NewGuid()}' Executor='MockScriptExecutor' Order='2016-06-21:1'></Script>"
				};
				ScriptExecutioner executioner = new ScriptExecutioner(new BaseMockLoader(scripts), _logger);
				SetExecutionStatus(executioner, false);
				executioner.Run();
			});
			Assert.NotNull(ex);
			_logger.Clean();
		}

		[Fact]
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

			Assert.True(executedScripts.Count == scripts.Length, "Incorrect number of scripts executed.");
			Assert.True(executedScripts.Contains(scriptId), $"Script Id '{scriptId}' not found.");
			Assert.True(
				executedScripts.Except(executingScripts).Count() == 0,
				"Script events not in sync with what is executing and executed."
			);
			_logger.Clean();
		}

		[Theory]
		[InlineData("", "", 6)]
		[InlineData("2016-06-21", "2016-06-22", 3)]
		[InlineData("2016-06-22", "2016-06-24", 4)]
		public void ExecutesCorrectSubsetOfScripts(string from = "", string to = "", int scriptsExecuted = 0) {
			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='MockScriptExecutor' Order='2016-06-21'></Script>",
				$"<Script Id='{Guid.NewGuid()}' Executor='MockScriptExecutor' Order='2016-06-22'></Script>",
				$"<Script Id='{Guid.NewGuid()}' Executor='MockScriptExecutor' Order='2016-06-22:1'></Script>",
				$"<Script Id='{Guid.NewGuid()}' Executor='MockScriptExecutor' Order='2016-06-23'></Script>",
				$"<Script Id='{Guid.NewGuid()}' Executor='MockScriptExecutor' Order='2016-06-24'></Script>",
				$"<Script Id='{Guid.NewGuid()}' Executor='MockScriptExecutor' Order='2016-06-25'></Script>"
			};
			
			bool fromSucceeded = DateTime.TryParse(from, out DateTime fromDt);
			bool toSucceeded = DateTime.TryParse(to, out DateTime toDt);

			Func<Script, bool> predicate = null;
			if (fromSucceeded && toSucceeded) {
				predicate = (s) =>
					s.DateCreatedUtc >= fromDt && s.DateCreatedUtc <= toDt;
			}
			else if (fromSucceeded && !toSucceeded) {
				predicate = (s) => s.DateCreatedUtc >= fromDt;
			}

			ScriptExecutioner executioner = new ScriptExecutioner(new BaseMockLoader(scripts), _logger);
			SetExecutionStatus(executioner, true);
			var result = executioner.Run(new ExecutionRequest() {
				ExecuteScriptsBetween = predicate
			});

			Assert.True(result.ScriptsCompleted == scriptsExecuted);
		}

		private void SetExecutionStatus(ScriptExecutioner executioner, bool executed) {
			IEnumerable<IMockScriptExecutor> executors = executioner.ScriptExecutors
				.Where(x => typeof(IMockScriptExecutor).IsAssignableFrom(x.GetType()))
				.Select(x => (IMockScriptExecutor)x);

			foreach (var executor in executors) {
				executor.ScriptExecuted = executed;
			}
		}

	}

}