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

	}

}
