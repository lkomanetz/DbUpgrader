using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Executioner.Contracts;
using System.Collections.Generic;
using Executioner.BackingStore;
using System.Reflection;
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

		[ClassCleanup]
		public static void Cleanup() {
			Directory.Delete(_rootDir, true);	
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
