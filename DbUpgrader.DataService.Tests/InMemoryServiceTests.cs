using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DbUpgrader.DataService.Contracts;
using DbUpgrader.Contracts;
using System.Collections;
using System.Collections.Generic;
using DbUpgrader.Contracts.Interfaces;
using System.Linq;

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

		[TestCleanup]
		public void TestCleanup() {
			_memoryService.Clean();
		}

		[TestMethod]
		public void InMemory_GetDocumentsSucceeds() {
			IList<ScriptDocument> docs = _memoryService.GetDocuments();
			Assert.IsTrue(docs == null, "Memory data service retrieved documents from empty collection.");

			docs = new List<ScriptDocument>() {
				CreateNewDocument(),
				CreateNewDocument(),
				CreateNewDocument()
			};

			foreach (var doc in docs) {
				_memoryService.Add(doc);
			}

			IList<ScriptDocument> retrievedDocs = _memoryService.GetDocuments();
			var exceptions = docs.Except(retrievedDocs).ToList();
			Assert.IsTrue(exceptions.Count == 0, "GetDocuments() failed to retrieve appropriate list.");
		}

		[TestMethod]
		public void InMemory_CleanSucceeds() {
			IList<ScriptDocument> docs = new List<ScriptDocument>() {
				CreateNewDocument(),
				CreateNewDocument(),
				CreateNewDocument()
			};

			foreach (var doc in docs) {
				_memoryService.Add(doc);
			}

			_memoryService.Clean();
			docs = _memoryService.GetDocuments();

			Assert.IsTrue(docs == null, "MemoryService.Clean() failed.");
		}

		[TestMethod]
		public void InMemory_AddDocumentSucceeds() {
			ScriptDocument doc = CreateNewDocument();

			_memoryService.Add(doc);
			IList<Guid> completedDocs = _memoryService.GetCompletedDocumentIds();
			Assert.IsTrue(completedDocs.Count == 1, "AddDocument added more than one document.");
			Assert.IsTrue(completedDocs.Contains(doc.SysId), "Created document not found.");
		}

		[TestMethod]
		public void InMemory_UpdateDocumentSucceeds() {
			ScriptDocument doc = CreateNewDocument();
			_memoryService.Add(doc);
			ScriptDocument foundDoc = _memoryService.GetDocuments()
				.Where(x => x.SysId == doc.SysId)
				.SingleOrDefault();

			Assert.IsTrue(foundDoc != null, $"Document id '{doc.SysId}' not found.");
			Assert.IsTrue(!foundDoc.IsComplete, $"Document id '{doc.SysId}' is complete.");

			foundDoc.IsComplete = true;
			_memoryService.Update(foundDoc);

			foundDoc = _memoryService.GetDocuments()
				.Where(x => x.SysId == doc.SysId)
				.SingleOrDefault();

			Assert.IsTrue(foundDoc != null && foundDoc.IsComplete, "Update failed.");
		}

		[TestMethod]
		public void InMemory_AddScriptSucceeds() {
			ScriptDocument doc = CreateNewDocument();
			Script script = CreateNewScript(doc.SysId);
			script.IsComplete = true;

			_memoryService.Add(doc);
			_memoryService.Add(script);
			IList<Guid> completedScripts = _memoryService.GetCompletedScriptsFor(doc.SysId);
			Assert.IsTrue(completedScripts.Count == 1, "AddScript added more than one script.");
			Assert.IsTrue(completedScripts.Contains(script.SysId), $"Created script not found for doc id '{doc.SysId}'.");
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
