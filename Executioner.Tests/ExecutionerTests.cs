using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Executioner.Contracts;
using System.Linq;
using Executioner.Tests.Classes;

namespace Executioner.Tests {

	[TestClass]
	public class ExecutionerTests {

		[TestMethod]
		public void ExecuteUpdatesCompletionProperty() {
			ScriptExecutioner executioner = new ScriptExecutioner(new MockScriptLoader(), new MemoryStore());
			executioner.Add(new MockScriptExecutor());
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
			ScriptExecutioner executioner = new ScriptExecutioner(new MockScriptLoader(), new MemoryStore());
			executioner.Add(new MockScriptExecutor());

			var result = executioner.Run();
			var secondResult = executioner.Run();

			Assert.IsTrue(result != secondResult, "Multiple executes should not produce same result.");
		}

		[TestMethod]
		public void ExecuteOnlyRunsNewScripts() {
			IBackingStore memoryStore = new MemoryStore();
			ScriptExecutioner executioner = new ScriptExecutioner(new MockScriptLoader(), memoryStore);
			executioner.Add(new MockScriptExecutor());

			var firstResult = executioner.Run();

			IList<ScriptDocument> docs = memoryStore.GetDocuments();
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
					DocumentId = docs[0].SysId
				};
				memoryStore.Add(newScript);
			}

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
			ScriptExecutioner executioner = new ScriptExecutioner(new MockScriptLoader(), new MemoryStore());
			executioner.Add(new IScriptExecutor[2] {
				new SecondScriptExecutor(),
				new MockScriptExecutor()
			});

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
			ScriptExecutioner executioner = new ScriptExecutioner(new MockScriptLoader(), new MemoryStore());
			executioner.Add(new MockScriptExecutor());
			executioner.Add(new IScriptExecutor[2] {
				new SecondScriptExecutor(),
				new MockScriptExecutor()
			});

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
			ScriptExecutioner executioner = new ScriptExecutioner(new MultipleExecutorLoader(), new MemoryStore());
			executioner.Add(new IScriptExecutor[2] {
				new SecondScriptExecutor(),
				new MockScriptExecutor()
			});
			var result = executioner.Run(new ExecutionRequest() { ExecuteAllScripts = true });
			Assert.IsTrue(
				result.ScriptDocumentsCompleted == 1,
				"Executioner failed to complete document with multiple executors."
			);
			Assert.IsTrue(
				result.ScriptsCompleted == 4,
				"Executioner failed to complete all scripts with multiple executors."
			);
		}

		[TestMethod]
		[ExpectedException(typeof(NullReferenceException), "Executioner executed incorrect script executor.")]
		public void ExecutionerWithMissingExecutorFails() {
			ScriptExecutioner executioner = new ScriptExecutioner(new MockScriptLoader(), new MemoryStore());
			executioner.Add(new SecondScriptExecutor());
			executioner.Run();
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException), "Executioner.ScriptExecutors was not null.")]
		public void ExecutionerWithNoExecutorsFail() {
			ScriptExecutioner executioner = new ScriptExecutioner(new MockScriptLoader(), new MemoryStore());
			executioner.Run();
		}

		[TestMethod]
		public void ExecutionerCanRunAllScriptsMultipleTimes() {
			ScriptExecutioner executioner = new ScriptExecutioner(new MockScriptLoader(), new MemoryStore());
			executioner.Add(new MockScriptExecutor());
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
				"Executioner results unequal.FirstResult completed {0} scripts\nSecondResult completed {1} scripts",
				firstResult.ScriptsCompleted,
				secondResult.ScriptsCompleted
			);
			Assert.IsTrue(
				firstResult.ScriptsCompleted == secondResult.ScriptsCompleted,
				errorMsg
			);
		}

	}

}
