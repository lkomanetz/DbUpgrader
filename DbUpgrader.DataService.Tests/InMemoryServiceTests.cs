using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DbUpgrader.DataService.Contracts;
using DbUpgrader.Contracts;
using System.Collections;
using System.Collections.Generic;
using DbUpgrader.Contracts.Interfaces;

namespace DbUpgrader.DataService.Tests {

	[TestClass]
	public class InMemoryServiceTests {
		private static IDataService _memoryService;
		private static IDbUpgrader _upgrader;
		private static IScriptLoader _scriptLoader;


		[ClassInitialize]
		public static void Initialize(TestContext context) {
			_memoryService = new InMemoryService();
			_scriptLoader = new MockScriptLoader();
			_upgrader = new MockUpgrader(_memoryService, _scriptLoader);
		}

		[ClassCleanup]
		public static void Cleanup() {
			_memoryService.Clean();
		}

		[TestMethod]
		public void InMemory_AddDocumentSucceeds() {
			ScriptDocument doc = CreateNewDocument();

			_memoryService.Add(doc);
			IList<Guid> completedDocs = _memoryService.GetCompletedDocumentIds();
			Assert.IsTrue(completedDocs.Contains(doc.SysId), "Created document not found.");
		}

		[TestMethod]
		public void InMemory_AddScriptSucceeds() {
			ScriptDocument doc = CreateNewDocument();
			Script script = CreateNewScript(doc.SysId);
			script.IsComplete = true;

			_memoryService.Add(doc);
			_memoryService.Add(script);
			IList<Guid> completedScripts = _memoryService.GetCompletedScriptsFor(doc.SysId);
			Assert.IsTrue(completedScripts.Contains(script.SysId), $"Created script not found for doc id '{doc.SysId}'.");
		}

		//TODO(Logan) -> Complete the DocumentMarkedAsComplete unit test.
		[TestMethod]
		public void InMemory_DocumentMarkedAsCompleteSucceeds() {
			ScriptDocument doc = CreateNewDocument();
			Script script = CreateNewScript(doc.SysId);

			doc.Scripts = new List<Script>() { script };
		}

		private ScriptDocument CreateNewDocument()
		{
			return new ScriptDocument() {
				SysId = Guid.NewGuid(),
				DateCreatedUtc = DateTime.UtcNow,
				Order = 0,
				Scripts = new List<Script>()
			};
		}

		private Script CreateNewScript(Guid docId) {
			return new Script() {
				SysId = Guid.NewGuid(),
				DateCreatedUtc = DateTime.UtcNow,
				Order = 0,
				DocumentId = docId
			};
		}
	}

}
