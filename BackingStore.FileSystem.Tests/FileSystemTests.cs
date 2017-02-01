using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Executioner.Contracts;
using System.Collections.Generic;
using Executioner;
using System.Linq;

namespace Logger.FileSystem.Tests {

	[TestClass]
	public class FileSystemTests {
		private FileSystemLogger _backingStore;
		private string _rootDir;

		[TestInitialize]
		public void Initialize() {
			_rootDir = @"C:\TestDir";
			_backingStore = new FileSystemLogger(_rootDir);
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
				File.Exists($@"{_rootDir}\{doc.SysId}.json"),
				$"Document Id '{doc.SysId}' file not created"
			);
		}

		[TestMethod]
		public void AddSameDocumentMakesNoChanges() {
			ScriptDocument doc = CreateDocument();
			_backingStore.Add(doc);
			doc.IsComplete = true;
			_backingStore.Add(doc);

			IList<Guid> completedDocIds = _backingStore.GetCompletedDocumentIds();
			Assert.IsTrue(
				completedDocIds == null || completedDocIds.Count == 0,
				"Script Document changed after second add."
			);
		}

		[TestMethod]
		public void AddScriptOnlyAddsIfNew() {
			ScriptDocument doc = CreateDocument();
			doc.Scripts[0].IsComplete = true;
			_backingStore.Add(doc);

			Script existingScript = doc.Scripts[0];
			_backingStore.Add(existingScript);

			int scriptCount = _backingStore.GetCompletedScriptIdsFor(doc.SysId).Count;
			Assert.IsTrue(
				scriptCount == 1,
				$"Expected completed scripts: {1}\nActual completed scripts: {scriptCount}"
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
				DateCreatedUtc = DateTime.UtcNow,
				IsComplete = true
			};

			_backingStore.Add(newScript);
			IList<Guid> foundScripts = _backingStore.GetCompletedScriptIdsFor(doc.SysId);

			Assert.IsTrue(
				foundScripts.Where(x => x == newScriptId).SingleOrDefault() != null,
				$"Script Id '{newScriptId}' not found after add."
			);
		}

		[TestMethod]
		public void UpdateDocument_Succeeds() {
			ScriptDocument doc = CreateDocument();
			bool previousState = doc.IsComplete;
			_backingStore.Add(doc);

			foreach (Script script in doc.Scripts) {
				script.IsComplete = true;
				_backingStore.Update(script);
			}
			IList<Guid> docs = _backingStore.GetCompletedDocumentIds();
			Assert.IsTrue(docs.Count == 1, "Incorrect number of documents returned.");
		}

		[TestMethod]
		public void UpdateScript_Succeeds() {
			ScriptDocument doc = CreateDocument();
			_backingStore.Add(doc);

			doc.Scripts[0].IsComplete = true;
			doc.Scripts[0].ScriptText = "Test";
			_backingStore.Update(doc.Scripts[0]);

			IList<Guid> foundScripts = _backingStore.GetCompletedScriptIdsFor(doc.SysId);
			Assert.IsTrue(foundScripts.Count == 1, "Incorrect number of scripts returned.");
		}

		[TestMethod]
		public void GetCompletedDocumentIds_Succeeds() {
			ScriptDocument doc = CreateDocument();
			ScriptDocument anotherDoc = CreateDocument();
			anotherDoc.Scripts[0].IsComplete = true;
			anotherDoc.IsComplete = true;

			IList<Guid> completedIds = _backingStore.GetCompletedDocumentIds();
			Assert.IsTrue(completedIds.Count == 0, "List of completed Ids greater than zero.");

			_backingStore.Add(doc);
			_backingStore.Add(anotherDoc);

			completedIds = _backingStore.GetCompletedDocumentIds();
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
			DateTime now = DateTime.UtcNow;

			return new ScriptDocument() {
				SysId = docId,
				Order = 0,
				DateCreatedUtc = now,
				IsComplete = false,
				ResourceName = "TestResource",
				Scripts = new List<Script>() {
					new Script() {
						SysId = Guid.NewGuid(),
						DateCreatedUtc = now.AddSeconds(1),
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
