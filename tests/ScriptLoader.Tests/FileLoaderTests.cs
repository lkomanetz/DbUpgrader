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

			GenerateDocuments(GetScripts());
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
		public void FileLoader_MissingRootDirectoryCreatesNew() {
			Directory.Delete(rootDir, true);
			FileSystemLoader loader = new FileSystemLoader(rootDir);
			Assert.IsTrue(Directory.Exists(rootDir), "Root directory not found.");
		}

		[TestMethod]
		public void FileLoader_MissingDocumentsThrowsException() {
			Assert.ThrowsException<FileNotFoundException>(() => {
				IEnumerable<string> files = Directory.EnumerateFiles(rootDir);
				foreach (string file in files) {
					File.Delete(file);
				}

				FileSystemLoader loader = new FileSystemLoader(rootDir);
				loader.LoadDocuments();
			});
		}

		// This is only used in the Initialize() method.
		private string GetScripts() {
			return $@"
				<Scripts>
					<Script Id='{Guid.NewGuid()}' Executor='FakeExecutor' Order='2017-01-09'>PRINT 'Hello'</Script>
					<Script Id='{Guid.NewGuid()}' Executor='FakeExecutor' Order='2017-01-09:1'>PRINT 'Hello'</Script>
				</Scripts>";
		}

		private void GenerateDocuments(string scripts) {
			documentIds.Clear();

			Directory.CreateDirectory(rootDir);
			for (short i = 0; i < documentCount; ++i) {
				Guid sysId = Guid.NewGuid();
				string doc = $"<ScriptDocument><Id>{sysId}</Id><Order>2017-01-09:{i}</Order>{scripts}</ScriptDocument>";
				File.WriteAllText($"{rootDir}\\Doc_{i}.sdoc", doc);
				documentIds.Add(sysId);
			}
		}

	}

}
