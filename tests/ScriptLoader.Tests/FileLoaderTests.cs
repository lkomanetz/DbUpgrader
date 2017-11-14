using Executioner;
using Executioner.Contracts;
using Executioner.Sorters;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ScriptLoader.Tests {

	public class FileLoaderTests {
		private string rootDir;
		private int documentCount;
		private IList<Guid> documentIds;
		private Sorter<IOrderedItem> sorter;

		public FileLoaderTests() {
			sorter = (collection) => collection.OrderBy(x => x.Order);
		}

		[Fact]
		public void FileLoader_LoadDocumentsSucceeds() {
			Initialize();
			Exception ex = Record.Exception(() => {
				FileSystemLoader loader = new FileSystemLoader(rootDir, sorter);
				loader.LoadDocuments();

				Assert.True(
					loader.Documents.Count == documentCount,
					$"Expected {documentCount} documents\nActual: {loader.Documents.Count}"
				);

				foreach (Guid id in documentIds) {
					ScriptDocument doc = loader.Documents
						.Where(x => x.SysId == id)
						.SingleOrDefault();

					Assert.True(doc != null, $"Unable to find doc id '{id}'");
				}
				Cleanup();
			});

			Assert.True(ex == null, ex.Message);
		}

		[Fact]
		public void FileLoader_MissingRootDirectoryCreatesNew() {
			Initialize();
			Directory.Delete(rootDir, true);
			FileSystemLoader loader = new FileSystemLoader(rootDir, sorter);
			Assert.True(Directory.Exists(rootDir), "Root directory not found.");
			Cleanup();
		}

		[Fact]
		public void FileLoader_MissingDocumentsThrowsException() {
			Initialize();
			Exception ex = Record.Exception(() => {
				IEnumerable<string> files = Directory.EnumerateFiles(rootDir);
				foreach (string file in files) {
					File.Delete(file);
				}

				FileSystemLoader loader = new FileSystemLoader(rootDir, sorter);
				loader.LoadDocuments();
			});
			Assert.NotNull(ex);
			Cleanup();
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

		private void Initialize() {
			rootDir = "C:\\FileLoaderTests";
			documentCount = 5;
			documentIds = new List<Guid>();

			GenerateDocuments(GetScripts());
		}

		private void Cleanup() {
			if (Directory.Exists(rootDir)) {
				Directory.Delete(rootDir, true);	
			}
		}

	}

}
