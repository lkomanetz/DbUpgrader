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
		private static IScriptLoader _loader;
		private static IBackingStore _backingStore;

		[ClassInitialize]
		public static void Initialize(TestContext context) {
			//TOOD(Logan) -> I don't like how each part is interacting and it feels clunky to me.  I'll need to clean this up.
			_backingStore = new MemoryStore();
			_loader = new MockScriptLoader();
			_loader.LoadDocuments(_backingStore);

			_executor = new MockScriptExecutor(_loader, _backingStore);
		}

		[ClassCleanup]
		public static void Cleanup() {
			_backingStore.Dispose();
		}

		[TestMethod]
		public void SqlServer_ExecuteUpdatesCompletionProperty() {
			_executor.Execute();
			IList<Guid> completedDocIds = _backingStore.GetCompletedDocumentIds();
			Assert.IsTrue(completedDocIds.Count == 1, "Script executor did not complete the document loaded by script loader.");

			IList<ScriptDocument> docs = _backingStore.GetDocuments();
			foreach (ScriptDocument doc in docs) {
				Assert.IsTrue(doc.IsComplete, "Executor failed to complete a script document.");
				Assert.IsTrue(!doc.Scripts.Any(x => x.IsComplete == false), "Executor failed to complete a script.");
			}
		}

	}

}
