using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Executioner.Contracts;
using System.Collections.Generic;
using Executioner;
using System.Linq;

namespace BackingStore.FileSystem.Tests {

	[TestClass]
	public class FileSystemTests {
		private static FileSystemStore _backingStore;
		private static string _rootDir;

		[ClassInitialize]
		public static void Initialize(TestContext context) {
			_rootDir = @"C:\TestDir";
			_backingStore = new FileSystemStore(_rootDir);
		}

		[TestCleanup]
		public void Cleanup() {
			_backingStore.Clean();
		}

		[TestMethod]
		public void AddDocument_Succeeds() {
			ScriptDocument doc = CreateDocument();
			_backingStore.Add(doc);

			Assert.IsTrue(
				File.Exists($@"{_rootDir}\{doc.SysId}{ScriptLoaderConstants.FILE_EXTENSION}"),
				$"Document Id '{doc.SysId}' file not created"
			);
		}

		[TestMethod]
		public void DeleteDocument_Succeeds() {
			ScriptDocument doc = CreateDocument();
			_backingStore.Add(doc);
			_backingStore.Delete(doc);

			Assert.IsTrue(
				!File.Exists($@"{_rootDir}\{doc.SysId}{ScriptLoaderConstants.FILE_EXTENSION}"),
				$"Document Id '{doc.SysId}' file still exists."
			);
		}

		[TestMethod]
		public void RetrievingScriptsFromDocument_Succeeds() {
			ScriptDocument doc = CreateDocument();
			_backingStore.Add(doc);

			IList<Script> foundScripts = _backingStore.GetScriptsFor(doc.SysId);
			Assert.IsTrue(
				foundScripts.Count == doc.Scripts.Count,
				"Incorrect number of scripts returned."
			);

			IList<Script> differences = doc.Scripts.Except(foundScripts).ToList();
			Assert.IsTrue(
				differences.Count == 0,
				$"Script differences found: {differences.Count}."
			);
		}

		[TestMethod]
		public void FileSystemStore_Clear_Succeeds() {
			ScriptDocument doc = CreateDocument();
			ScriptDocument anotherDoc = CreateDocument();
			_backingStore.Add(doc);
			_backingStore.Add(doc);

			_backingStore.Clean();
			Assert.IsTrue(
				!Directory.Exists(_rootDir),
				$"Directory '{_rootDir}' still exists."
			);
		}

		[TestMethod]
		[ExpectedException(typeof(FileNotFoundException), "Script file unexpectedly found.")]
		public void AddScriptForInvalidDocId_Fails() {
			Script fakeScript = new Script() {
				DocumentId = Guid.NewGuid(),
				SysId = Guid.NewGuid(),
				DateCreatedUtc = DateTime.UtcNow
			};

			_backingStore.Add(fakeScript);
		}

		[TestMethod]
		public void AddScriptForValidDocId_Succeeds() {
			ScriptDocument doc = CreateDocument();
			_backingStore.Add(doc);

			Guid newScriptId = Guid.NewGuid();
			Script newScript = new Script() {
				SysId = newScriptId,
				DocumentId = doc.SysId,
				DateCreatedUtc = DateTime.UtcNow
			};

			_backingStore.Add(newScript);
			IList<Script> foundScripts = _backingStore.GetScriptsFor(doc.SysId);

			Assert.IsTrue(
				foundScripts.Where(x => x.SysId == newScriptId).SingleOrDefault() != null,
				$"Script Id '{newScriptId}' not found after add."
			);
		}

		[TestMethod]
		public void DeleteScript_Succeeds() {
			ScriptDocument doc = CreateDocument();
			doc.Scripts.Add(new Script() {
				SysId = Guid.NewGuid(),
				DocumentId = doc.SysId,
				DateCreatedUtc = DateTime.UtcNow,
				Order = 0,
				IsComplete = false
			});
			Guid scriptIdDeleted = doc.Scripts[0].SysId;
			_backingStore.Add(doc);

			bool scriptDeleted = _backingStore.Delete(doc.Scripts[0]);
			Assert.IsTrue(scriptDeleted, $"Script Id '{scriptIdDeleted}' not deleted.");

			IList<Script> scripts = _backingStore.GetScriptsFor(doc.SysId);
			if (scripts.Count > 0) {
				Assert.IsTrue(
					scripts.Where(x => x.SysId == scriptIdDeleted).SingleOrDefault() == null,
					$"Script Id '{scriptIdDeleted}' not deleted."
				);
			}
		}

		[TestMethod]
		public void GetDocuments_Succeeds() {
			ScriptDocument doc = CreateDocument();
			ScriptDocument anotherDoc = CreateDocument();
			_backingStore.Add(doc);
			_backingStore.Add(anotherDoc);

			IList<ScriptDocument> docs = _backingStore.GetDocuments();
			Assert.IsTrue(
				docs.Count == 2,
				$"Incorrect number of documents returned.\nExpected {2}\nActual {docs.Count}"
			);

			int docsFound = docs.Where(
				x => x.SysId == doc.SysId ||
					x.SysId == anotherDoc.SysId
				)
				.Count();
			Assert.IsTrue(
				docsFound == 2,
				$"Incorrect number of documents returned.\nExpected {2}\nActual {docsFound}"
			);
		}

		[TestMethod]
		public void UpdateDocument_Succeeds() {
			ScriptDocument doc = CreateDocument();
			bool previousState = doc.IsComplete;
			_backingStore.Add(doc);
			doc.IsComplete = true;

			_backingStore.Update(doc);
			IList<ScriptDocument> docs = _backingStore.GetDocuments();
			Assert.IsTrue(docs.Count == 1, "Incorrect number of documents returned.");
			Assert.IsTrue(
				docs[0].IsComplete && !previousState,
				"Document did not update."
			);
		}

		[TestMethod]
		public void UpdateScript_Succeeds() {
			ScriptDocument doc = CreateDocument();
			_backingStore.Add(doc);

			doc.Scripts[0].IsComplete = true;
			doc.Scripts[0].ScriptText = "Test";
			_backingStore.Update(doc.Scripts[0]);

			IList<Script> foundScripts = _backingStore.GetScriptsFor(doc.SysId);
			Assert.IsTrue(foundScripts.Count == 1, "Incorrect number of scripts returned.");
			Assert.IsTrue(
				foundScripts[0].IsComplete && foundScripts[0].ScriptText.Equals("Test"),
				"Script not updated"
			);
		}

		[TestMethod]
		public void GetCompletedDocumentIds_Succeeds() {
			ScriptDocument doc = CreateDocument();
			ScriptDocument anotherDoc = CreateDocument();
			anotherDoc.Scripts[0].IsComplete = true;
			anotherDoc.IsComplete = true;

			_backingStore.Add(doc);
			_backingStore.Add(anotherDoc);

			IList<Guid> completedIds = _backingStore.GetCompletedDocumentIds();
			Assert.IsTrue(completedIds.Count == 1, "Incorrect completed Ids returned.");
			Assert.IsTrue(
				completedIds[0] == anotherDoc.SysId,
				$"Expected completed Id {anotherDoc.SysId}\nActual completed Id {completedIds[0]}"
			);
		}

		[TestMethod]
		public void GetCompletedScriptIds_Succeeds() {
			Guid completedScriptId = Guid.NewGuid();
			ScriptDocument doc = CreateDocument();
			doc.Scripts.Add(new Script() {
				SysId = completedScriptId,
				DocumentId = doc.SysId,
				IsComplete = true,
				DateCreatedUtc = DateTime.UtcNow,
				Order = 0,
				ExecutorName = "TestExecutor",
				ScriptText = String.Empty
			});

			_backingStore.Add(doc);
			IList<Guid> completedScriptIds = _backingStore.GetCompletedScriptIdsFor(doc.SysId);
			Assert.IsTrue(completedScriptIds.Count == 1, "Incorrect amount of script Ids returned.");
			Assert.IsTrue(
				completedScriptIds[0] == completedScriptId,
				$"Expected Id {completedScriptId}\nActual Id {completedScriptIds[0]}"
			);
		}

		private ScriptDocument CreateDocument() {
			Guid docId = Guid.NewGuid();

			return new ScriptDocument() {
				SysId = docId,
				Order = 0,
				DateCreatedUtc = DateTime.UtcNow,
				IsComplete = false,
				ResourceName = "TestResource",
				Scripts = new List<Script>() {
					new Script() {
						SysId = Guid.NewGuid(),
						DateCreatedUtc = DateTime.UtcNow,
						Order = 0,
						DocumentId = docId,
						ExecutorName = "TestExecutor",
						ScriptText = String.Empty,
						IsComplete = false
					}	
				}
			};
		}

	}

}
