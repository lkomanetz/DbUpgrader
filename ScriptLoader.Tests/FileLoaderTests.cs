using Executioner;
using Executioner.Contracts;
using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace ScriptLoader.Tests {

	[TestClass]
	public class FileLoaderTests {
		private string rootDir;
		private int documentCount;
		private IList<Guid> documentIds;

		[TestInitialize]
		public void Initialize() {
			rootDir = "C:\\FileLoaderTests";
			documentCount = 5;
			documentIds = new List<Guid>();

			Directory.CreateDirectory(rootDir);
			for (short i = 0; i < documentCount; ++i) {
				Guid sysId = Guid.NewGuid();
				string doc = $"<ScriptDocument><Id>{sysId}</Id><Order>2017-01-09:{i}</Order>{GetScripts()}</ScriptDocument>";
				File.WriteAllText($"{rootDir}\\Doc_{i}.sdoc", doc);

				documentIds.Add(sysId);
			}
		}

		[TestCleanup]
		public void Cleanup() {
			if (Directory.Exists(rootDir)) {
				Directory.Delete(rootDir, true);	
			}
		}

		[TestMethod]
		public void FileLoader_LoadDocumentsSucceeds() {
			FileSystemLoader loader = new FileSystemLoader(rootDir);
			loader.LoadDocuments();

			Assert.IsTrue(
				loader.Documents.Count == documentCount,
				$"Expected {documentCount} documents\nActual: {loader.Documents.Count}"
			);

			foreach (Guid id in documentIds) {
				ScriptDocument doc = loader.Documents
					.Where(x => x.SysId == id)
					.SingleOrDefault();

				Assert.IsTrue(doc != null, $"Unable to find doc id '{id}'");
			}
		}

		[TestMethod]
		[ExpectedException(typeof(DirectoryNotFoundException), "Directory found when it should be missing.")]
		public void FileLoader_MissingRootDirectoryThrowsException() {
			Directory.Delete(rootDir, true);
			FileSystemLoader loader = new FileSystemLoader(rootDir);
			loader.LoadDocuments();
		}

		[TestMethod]
		[ExpectedException(typeof(FileNotFoundException), "Documents found when directory was empty.")]
		public void FileLoader_MissingDocumentsThrowsException() {
			IEnumerable<string> files = Directory.EnumerateFiles(rootDir);
			foreach (string file in files) {
				File.Delete(file);
			}

			FileSystemLoader loader = new FileSystemLoader(rootDir);
			loader.LoadDocuments();
		}

		private string GetScripts() {
			return $@"
				<Scripts>
					<Script Id='{Guid.NewGuid()}' Executor='FakeExecutor' Order='2017-01-09'>PRINT 'Hello'</Script>
					<Script Id='{Guid.NewGuid()}' Executor='FakeExecutor' Order='2017-01-09:1'>PRINT 'Hello'</Script>
				</Scripts>";
		}

	}

}
