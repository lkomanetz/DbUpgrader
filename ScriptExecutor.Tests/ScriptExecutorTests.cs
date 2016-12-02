using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScriptExecutor;
using ScriptLoader.Contracts;
using BackingStore.Contracts;
using BackingStore;
using System.Collections.Generic;
using Executioner.Contracts;
using System.Linq;
using ScriptExecutor.Tests.Classes;

namespace ScriptExecutor.Tests {

	[TestClass]
	public class ScriptExecutorTests {
		private static MockScriptExecutor _executor;

		[ClassInitialize]
		public static void Initialize(TestContext context) {
			_executor = new MockScriptExecutor(new MockScriptLoader(), new MemoryStore());
		}

		[TestMethod]
		public void ExecuteUpdatesCompletionProperty() {
			var result = _executor.Execute();
			Assert.IsTrue(
				result.ScriptDocumentsCompleted == 1,
				"Script executor did not complete the document loaded by script loader."
			);

			IList<ScriptDocument> docs = _executor.ScriptDocuments;
			foreach (ScriptDocument doc in docs) {
				Assert.IsTrue(doc.IsComplete, "Executor failed to complete a script document.");
				Assert.IsTrue(
					!doc.Scripts.Any(x => x.IsComplete == false),
					"Executor failed to complete a script."
				);
			}
		}

		[TestMethod]
		public void ExecuteOnlyRunsNonCompletedTests() {
			MockScriptExecutor executor = new MockScriptExecutor(new MockScriptLoader(), new MemoryStore());
			var result = executor.Execute();
			var secondResult = executor.Execute();

			Assert.IsTrue(result != secondResult, "Multiple executes should not produce same result.");
		}

		[TestMethod]
		public void ExecuteOnlyRunsNewScripts() {
			IBackingStore memoryStore = new MemoryStore();
			MockScriptExecutor executor = new MockScriptExecutor(new MockScriptLoader(), memoryStore);

			var firstResult = executor.Execute();

			IList<ScriptDocument> docs = memoryStore.GetDocuments();
			Assert.IsTrue(docs.Count == 1, "Unit test expecting only one script document.");

			short scriptsToAdd = 1;
			for (short i = 0; i < scriptsToAdd; ++i) {
				Script newScript = new Script() {
					SysId = Guid.NewGuid(),
					DateCreatedUtc = DateTime.UtcNow,
					Order = 0,
					AssemblyName = "TestAssembly",
					IsComplete = false,
					DocumentId = docs[0].SysId
				};
				memoryStore.Add(newScript);
			}

			var secondResult = executor.Execute();
			Assert.IsTrue(
				secondResult.ScriptsCompleted != firstResult.ScriptsCompleted,
				"Incorrect number of scripts executed on second run."
			);
			Assert.IsTrue(
				secondResult.ScriptsCompleted == scriptsToAdd,
				$"Expected {scriptsToAdd} scripts to run.\nActual scripts ran = {secondResult.ScriptsCompleted}."
			);
		}

	}

}
