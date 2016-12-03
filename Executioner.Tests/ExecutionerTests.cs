﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScriptExecutor;
using ScriptLoader.Contracts;
using BackingStore.Contracts;
using BackingStore;
using System.Collections.Generic;
using Executioner.Contracts;
using System.Linq;
using Executioner.Tests.Classes;
using ScriptExecutor.Contracts;

namespace Executioner.Tests {

	//TODO(Logan) -> Test multiple executors.
	[TestClass]
	public class ExecutionerTests {

		[TestMethod]
		public void ExecuteUpdatesCompletionProperty() {
			Executioner executioner = new Executioner(new MockScriptLoader(), new MemoryStore());
			executioner.AddExecutor(new MockScriptExecutor());
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
			Executioner executioner = new Executioner(new MockScriptLoader(), new MemoryStore());
			executioner.AddExecutor(new MockScriptExecutor());

			var result = executioner.Run();
			var secondResult = executioner.Run();

			Assert.IsTrue(result != secondResult, "Multiple executes should not produce same result.");
		}

		[TestMethod]
		public void ExecuteOnlyRunsNewScripts() {
			IBackingStore memoryStore = new MemoryStore();
			Executioner executioner = new Executioner(new MockScriptLoader(), memoryStore);
			executioner.AddExecutor(new MockScriptExecutor());

			var firstResult = executioner.Run();

			IList<ScriptDocument> docs = memoryStore.GetDocuments();
			Assert.IsTrue(docs.Count == 1, "Unit test expecting only one script document.");

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

	}

}
