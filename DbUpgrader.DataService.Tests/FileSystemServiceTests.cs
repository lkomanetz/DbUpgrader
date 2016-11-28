using DbUpgrader.Contracts;
using DbUpgrader.DataService.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DbUpgrader.DataService.Tests {

	[TestClass]
	public class FileSystemServiceTests {
		private static IDataService _fileSystem;

		[ClassInitialize]
		public static void Initialize(TestContext context) {
			_fileSystem = new FileSystemService(Assembly.GetExecutingAssembly());
		}

		[ClassCleanup]
		public static void Cleanup() {
			_fileSystem.Clean();
		}

		[TestMethod]
		public void FileSystem_AddDocumentSucceeds() {
			ScriptDocument newDoc = new ScriptDocument() {
				SysId = Guid.NewGuid(),
				DateCreatedUtc = DateTime.UtcNow,
				Order = 0
			};

			_fileSystem.Add(newDoc);

			IList<Guid> completedDocs = _fileSystem.GetCompletedDocumentIds();
			Assert.IsTrue(completedDocs.Contains(newDoc.SysId), "Newly created document not found.");
			_fileSystem.Clean();
		}

	}

}
