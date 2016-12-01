using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BackingStore.Contracts;
using Executioner.Contracts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BackingStore.Tests {

	//TODO(Logan) -> Add tests to make sure backing store will sort documents and scripts correctly.
	[TestClass]
	public class MemoryStoreTests {
		private static IBackingStore _memoryService;

		[ClassInitialize]
		public static void Initialize(TestContext context) {
			_memoryService = new MemoryStore();
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
				CreateNewDocument(numOfScripts: 0),
				CreateNewDocument(numOfScripts: 0),
				CreateNewDocument(numOfScripts: 0)
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
				CreateNewDocument(numOfScripts: 0),
				CreateNewDocument(numOfScripts: 0),
				CreateNewDocument(numOfScripts: 0)
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
			ScriptDocument doc = CreateNewDocument(numOfScripts: 0);

			_memoryService.Add(doc);
			IList<ScriptDocument> completedDocs = _memoryService.GetDocuments();
			Assert.IsTrue(completedDocs.Count == 1, "AddDocument added more than one document.");
			Assert.IsTrue(completedDocs.Contains(doc), "Created document not found.");
		}

		[TestMethod]
		public void InMemory_ReturnOnlyCompleteDocumentsSucceeds() {
			ScriptDocument docOne = CreateNewDocument(numOfScripts: 0);
			ScriptDocument docTwo = CreateNewDocument(numOfScripts: 0);

			docOne.IsComplete = true;
			_memoryService.Add(docOne);
			_memoryService.Add(docTwo);

			IList<Guid> completedDocIds = _memoryService.GetCompletedDocumentIds();
			Assert.IsTrue(completedDocIds.Count == 1, "Incorrect number of completed documents returned.");
			Assert.IsTrue(docOne.SysId == completedDocIds[0], "Incorrect document id returned.");
		}

		[TestMethod]
		public void InMemory_ReturnOnlyCompletedScriptsForDocumentSucceeds() {
			ScriptDocument doc = CreateNewDocument(numOfScripts: 3);
			doc.Scripts[1].IsComplete = true;
			_memoryService.Add(doc);

			IList<Guid> completedScripts = _memoryService.GetCompletedScriptIdsFor(doc.SysId);
			Assert.IsTrue(completedScripts.Count == 1, "Incorrect number of completed scripts returned.");
			Assert.IsTrue(completedScripts[0] == doc.Scripts[1].SysId, "Incorrect completed script returned.");
		}

		[TestMethod]
		public void InMemory_UpdateDocumentSucceeds() {
			ScriptDocument doc = CreateNewDocument(numOfScripts: 0);
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
		public void InMemory_GetScriptsForDocumentSucceeds() {
			int numberOfScripts = 3;
			ScriptDocument doc = CreateNewDocument(numOfScripts: numberOfScripts);

			_memoryService.Add(doc);
			IList<Script> scripts = _memoryService.GetScriptsFor(doc.SysId);
			Assert.IsTrue(scripts.Count == numberOfScripts, "Add(ScriptDocument) did not add correct number of scripts.");
		}

		[TestMethod]
		public void InMemory_ScriptDeleteSucceeds() {
			int numberOfScripts = 3;
			ScriptDocument doc = CreateNewDocument(numOfScripts: numberOfScripts);
			_memoryService.Add(doc);

			Script scriptToDelete = new Script() {
				SysId = doc.Scripts[1].SysId,
				DateCreatedUtc = doc.Scripts[1].DateCreatedUtc,
				Order = doc.Scripts[1].Order,
				DocumentId = doc.Scripts[1].DocumentId,
				AssemblyName = doc.Scripts[1].AssemblyName,
				ScriptText = doc.Scripts[1].ScriptText
			};

			bool scriptDeleted = _memoryService.Delete(scriptToDelete);
			IList<Script> scriptsAfterDelete = _memoryService.GetScriptsFor(doc.SysId);
			Script deletedScript = scriptsAfterDelete.Where(x => x.SysId == scriptToDelete.SysId).SingleOrDefault();
			Assert.IsTrue(scriptDeleted && deletedScript == null, "Script not deleted.");
		}

		[TestMethod]
		public void InMemory_DocumentDeleteSucceeds() {
			ScriptDocument doc = CreateNewDocument(numOfScripts: 3);
			_memoryService.Add(doc);
			ScriptDocument scriptToDelete = new ScriptDocument() {
				SysId = doc.SysId,
				DateCreatedUtc = doc.DateCreatedUtc,
				Order = doc.Order,
				Scripts = doc.Scripts,
				ResourceName = doc.ResourceName,
				IsComplete = doc.IsComplete
			};

			bool isDeleted = _memoryService.Delete(doc);
			IList<ScriptDocument> docsAfterDelete = _memoryService.GetDocuments();

			Assert.IsTrue(isDeleted && docsAfterDelete == null, "ScriptDocument not deleted.");
		}

		[TestMethod]
		public void InMemory_DocumentsAreInCorrectOrder_BasedOnOrder() {
			DateTime currentTime = DateTime.UtcNow;
			IList<ScriptDocument> docs = new List<ScriptDocument>() {
				CreateNewDocument(numOfScripts: 3, dateCreated: currentTime, order: 2),
				CreateNewDocument(numOfScripts: 3, dateCreated: currentTime, order: 1),
				CreateNewDocument(numOfScripts: 3, dateCreated: currentTime, order: 3)
			};
			
			foreach (ScriptDocument doc in docs) {
				_memoryService.Add(doc);
			}	
		}

		[TestMethod]
		public void InMemory_DocumentsAreInCorrectOrder_BasedOnDate() {
			DateTime currentTime = DateTime.UtcNow;
			IList<ScriptDocument> docs = new List<ScriptDocument>() {
				CreateNewDocument(numOfScripts: 3, dateCreated: currentTime.AddDays(1), order: 0),
				CreateNewDocument(numOfScripts: 3, dateCreated: currentTime.AddDays(2), order: 0),
				CreateNewDocument(numOfScripts: 3, dateCreated: currentTime, order: 0)
			};

			foreach (ScriptDocument doc in docs) {
				_memoryService.Add(doc);
			}

			IList<ScriptDocument> docsReturned = _memoryService.GetDocuments();
			AssertOrder(
				docsReturned.Select(x => (IOrderedItem)x).ToList(),
				$"Date: {currentTime.ToShortDateString()} Order: 0Date: {currentTime.AddDays(1).ToShortDateString()} Order: 0Date: {currentTime.AddDays(2).ToShortDateString()} Order: 0"
			);
		}

		[TestMethod]
		public void InMemory_ScriptsAreInCorrectOrder() {
			DateTime currentTime = DateTime.UtcNow;
			ScriptDocument doc = CreateNewDocument(numOfScripts: 0);

			IList<Script> scripts = new List<Script>() {
				CreateScriptFor(docId: doc.SysId, dateCreated: currentTime.AddDays(2), order: 0),
				CreateScriptFor(docId: doc.SysId, dateCreated: currentTime.AddDays(1), order: 0),
				CreateScriptFor(docId: doc.SysId, dateCreated: currentTime, order: 0)
			};

			doc.Scripts = scripts;
			_memoryService.Add(doc);

			IList<Script> scriptsReturned = _memoryService.GetScriptsFor(doc.SysId);
			AssertOrder(
				scriptsReturned.Select(x => (IOrderedItem)x).ToList(),
				$"Date: {currentTime.ToShortDateString()} Order: 0Date: {currentTime.AddDays(1).ToShortDateString()} Order: 0Date: {currentTime.AddDays(2).ToShortDateString()} Order: 0"
			);
		}

		private void AssertOrder(IList<IOrderedItem> items, string expectedOrder) {
			string actualOrder = String.Empty;
			foreach (IOrderedItem item in items) {
				actualOrder += $"Date: {item.DateCreatedUtc.ToShortDateString()} Order: {item.Order}";
			}

			Assert.AreEqual(expectedOrder, actualOrder);
		}

		private ScriptDocument CreateNewDocument(
			int numOfScripts,
			DateTime dateCreated = default(DateTime),
			int order = 0
		) {
			ScriptDocument doc = new ScriptDocument() {
				SysId = Guid.NewGuid(),
				DateCreatedUtc = dateCreated,
				Order = order,
				Scripts = new List<Script>(),
				ResourceName = "TestResourceName",
				IsComplete = false
			};

			if (numOfScripts > 0) {
				doc.Scripts = new List<Script>();
			}

			for (int i = 0; i < numOfScripts; ++i) {
				doc.Scripts.Add(new Script() {
					SysId = Guid.NewGuid(),
					DocumentId = doc.SysId,
					DateCreatedUtc = dateCreated,
					Order = order
				});	
			}

			return doc;
		}

		private Script CreateScriptFor(
			Guid docId,
			DateTime dateCreated = default(DateTime),
			int order = 0
		) {
			return new Script() {
				SysId = Guid.NewGuid(),
				DocumentId = docId,
				DateCreatedUtc = dateCreated,
				Order = order
			};
		}
	}

}
