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
		public void AddScriptOnlyAddsIfNew() {
			ScriptDocument doc = CreateDocument();
			_backingStore.CreateLogFile(doc.SysId);

			Script existingScript = doc.Scripts[0];
			existingScript.IsComplete = true;

			_backingStore.Add(existingScript);
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
			_backingStore.CreateLogFile(doc.SysId);
			_backingStore.CreateLogFile(anotherDoc.SysId);
			_backingStore.Add(doc.Scripts[0]);
			_backingStore.Add(doc.Scripts[0]);

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
			_backingStore.CreateLogFile(doc.SysId);
			_backingStore.Add(doc.Scripts[0]);

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
			_backingStore.CreateLogFile(doc.SysId);
			_backingStore.Add(doc.Scripts[0]);

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
				foundScripts.Count == (doc.Scripts.Count + scriptIds.Count),
				"Scripts not added to log."
			);
		}

		[Fact]
		public void GetCompletedScriptIds_Succeeds() {
			Guid completedScriptId = Guid.NewGuid();
			ScriptDocument doc = CreateDocument();
			_backingStore.CreateLogFile(doc.SysId);
			doc.Scripts.Add(new Script() {
				SysId = completedScriptId,
				DocumentId = doc.SysId,
				IsComplete = true,
				DateCreatedUtc = DateTime.UtcNow,
				Order = 0,
				ExecutorName = "TestExecutor",
				ScriptText = String.Empty
			});

			_backingStore.Add(doc.Scripts[1]);
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