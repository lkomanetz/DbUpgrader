using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DbUpgrader.DataService.Contracts;
using DbUpgrader.Contracts;
using System.Collections;
using System.Collections.Generic;

namespace DbUpgrader.DataService.Tests {

	[TestClass]
	public class InMemoryServiceTests {
		private static IDataService _memoryService;

		[ClassInitialize]
		public static void Initialize(TestContext context) {
			_memoryService = new InMemoryService();
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

			_memoryService.Add(doc);
			_memoryService.Add(script);
			IList<Guid> completedScripts = _memoryService.GetCompletedScriptsFor(doc.SysId);
			Assert.IsTrue(completedScripts.Contains(script.SysId), $"Created script not found for doc id '{doc.SysId}'.");
		}

		private ScriptDocument CreateNewDocument()
		{
			return new ScriptDocument() {
				SysId = Guid.NewGuid(),
				DateCreatedUtc = DateTime.UtcNow,
				Order = 0
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
