using System;
using System.IO;
using Executioner.Contracts;
using System.Collections.Generic;
using Executioner;
using System.Linq;
using Xunit;

namespace Logger.FileSystem.Tests {

	public class FileSystemTests : IDisposable {
		private FileSystemStore _backingStore;
		private string _rootDir;

		public FileSystemTests() {
			_rootDir = @"C:\TestDir";
			_backingStore = new FileSystemStore(_rootDir);
		}

		public void Dispose() {
			_backingStore.Clean();
		}

		[Fact]
		public void AddDocument_Succeeds() {
			ScriptDocument doc = CreateDocument();
			_backingStore.Add(doc);

			Assert.True(
				File.Exists($@"{_rootDir}\{doc.SysId}.json"),
				$"Document Id '{doc.SysId}' file not created"
			);
		}

		[Fact]
		public void AddSameDocumentMakesNoChanges() {
			ScriptDocument doc = CreateDocument();
			_backingStore.Add(doc);
			doc.IsComplete = true;
			_backingStore.Add(doc);

			IList<Guid> completedDocIds = _backingStore.GetCompletedDocumentIds();
			Assert.True(
				completedDocIds == null || completedDocIds.Count == 0,
				"Script Document changed after second add."
			);

		}

		[Fact]
		public void AddScriptOnlyAddsIfNew() {
			ScriptDocument doc = CreateDocument();
			doc.Scripts[0].IsComplete = true;
			_backingStore.Add(doc);

			Script existingScript = doc.Scripts[0];
			_backingStore.Add(existingScript);

			IDataStore storage = new FileSystemStore(_rootDir);
			int scriptCount = _backingStore.GetCompletedScriptIdsFor(doc.SysId).Count;
			Assert.True(
				scriptCount == 1,
				$"Expected completed scripts: {1}\nActual completed scripts: {scriptCount}"
			);
		}

		[Fact]
		public void FileSystemStore_Clear_Succeeds() {
			ScriptDocument doc = CreateDocument();
			ScriptDocument anotherDoc = CreateDocument();
			_backingStore.Add(doc);
			_backingStore.Add(doc);

			_backingStore.Clean();
			Assert.True(
				!Directory.Exists(_rootDir),
				$"Directory '{_rootDir}' still exists."
			);
		}

		[Fact]
		public void AddScriptForInvalidDocId_Fails() {
			Exception ex = Record.Exception(() => {
				Script fakeScript = new Script() {
					DocumentId = Guid.NewGuid(),
					SysId = Guid.NewGuid(),
					DateCreatedUtc = DateTime.UtcNow
				};

				_backingStore.Add(fakeScript);
			});
			Assert.NotNull(ex);
		}

		[Fact]
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

			Assert.True(
				foundScripts.Where(x => x == newScriptId).SingleOrDefault() != null,
				$"Script Id '{newScriptId}' not found after add."
			);
		}

		[Fact]
		public void AddingMultipleScripts_Succeeds() {
			ScriptDocument doc = CreateDocument();
			_backingStore.Add(doc);

			IList<Guid> scriptIds = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid() };
			foreach (Guid id in scriptIds) {
				Script newScript = new Script() {
					SysId = id,
					DocumentId = doc.SysId,
					DateCreatedUtc = DateTime.UtcNow,
					IsComplete = true
				};
				_backingStore.Add(newScript);
			}

			IList<Guid> foundScripts = _backingStore.GetCompletedScriptIdsFor(doc.SysId);
			Assert.True(
				foundScripts.Where(x => !scriptIds.Contains(x)).Count() == 0,
				"Scripts not added to log."
			);
		}

		[Fact]
		public void UpdateDocument_Succeeds() {
			ScriptDocument doc = CreateDocument();
			bool previousState = doc.IsComplete;
			_backingStore.Add(doc);

			foreach (Script script in doc.Scripts) {
				script.IsComplete = true;
				_backingStore.Update(script);
			}
			IList<Guid> docs = _backingStore.GetCompletedDocumentIds();
			Assert.True(docs.Count == 1, "Incorrect number of documents returned.");
		}

		[Fact]
		public void UpdateScript_Succeeds() {
			ScriptDocument doc = CreateDocument();
			_backingStore.Add(doc);

			doc.Scripts[0].IsComplete = true;
			doc.Scripts[0].ScriptText = "Test";
			_backingStore.Update(doc.Scripts[0]);

			IList<Guid> foundScripts = _backingStore.GetCompletedScriptIdsFor(doc.SysId);
			int completedDocCount = _backingStore.GetCompletedDocumentIds().Count;
			Assert.True(foundScripts.Count == 1, "Incorrect number of scripts returned.");
			Assert.True(completedDocCount == 1, $"Expected '{1}' completed documents.  Was '{completedDocCount}'");
		}

		[Fact]
		public void GetCompletedDocumentIds_Succeeds() {
			ScriptDocument doc = CreateDocument();
			ScriptDocument anotherDoc = CreateDocument();
			anotherDoc.Scripts[0].IsComplete = true;
			anotherDoc.IsComplete = true;

			IList<Guid> completedIds = _backingStore.GetCompletedDocumentIds();
			Assert.True(completedIds.Count == 0, "List of completed Ids greater than zero.");

			_backingStore.Add(doc);
			_backingStore.Add(anotherDoc);

			completedIds = _backingStore.GetCompletedDocumentIds();
			Assert.True(completedIds.Count == 1, "Incorrect completed Ids returned.");
			Assert.True(
				completedIds[0] == anotherDoc.SysId,
				$"Expected completed Id {anotherDoc.SysId}\nActual completed Id {completedIds[0]}"
			);
		}

		[Fact]
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
			Assert.True(completedScriptIds.Count == 1, "Incorrect amount of script Ids returned.");
			Assert.True(
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