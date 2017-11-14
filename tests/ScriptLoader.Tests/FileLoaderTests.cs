using Executioner;
using Executioner.Contracts;
using Executioner.Sorters;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ScriptLoader.Tests {

	public class FileLoaderTests : IDisposable {
		private string _rootDir;
		private int _documentCount;
		private IList<Guid> _docIds;
		private Sorter<IOrderedItem> _sorter;

		public FileLoaderTests() {
			_sorter = (collection) => collection.OrderBy(x => x.Order);
			_rootDir = "C:\\FileLoaderTests";
			_documentCount = 5;
			_docIds = new List<Guid>();

			GenerateDocuments();
		}

		public void Dispose() {
			if (Directory.Exists(_rootDir)) Directory.Delete(_rootDir, true);	
		}

		[Fact]
		public void FileLoader_LoadDocumentsSucceeds() {
			FileSystemLoader loader = new FileSystemLoader(_rootDir, _sorter);
			loader.LoadDocuments();

			Assert.True(
				loader.Documents.Count == _documentCount,
				$"Expected {_documentCount} documents\nActual: {loader.Documents.Count}"
			);

			int unmatchedDocsCount = loader.Documents
				.Where(x => !_docIds.Contains(x.SysId))
				.Count();

			Assert.True(unmatchedDocsCount == 0, "Not all documents were created.");
		}

		[Fact]
		public void FileLoader_MissingRootDirectoryCreatesNew() {
			Directory.Delete(_rootDir, true);
			FileSystemLoader loader = new FileSystemLoader(_rootDir, _sorter);
			Assert.True(Directory.Exists(_rootDir), "Root directory not found.");
		}

		[Fact]
		public void FileLoader_MissingDocumentsThrowsException() {
			Exception ex = Record.Exception(() => {
				IEnumerable<string> files = Directory.EnumerateFiles(_rootDir);
				foreach (string file in files) {
					File.Delete(file);
				}

				FileSystemLoader loader = new FileSystemLoader(_rootDir, _sorter);
				loader.LoadDocuments();
			});
			Assert.NotNull(ex);
		}

		private void GenerateDocuments() {
			_docIds.Clear();

			Directory.CreateDirectory(_rootDir);
			for (short i = 0; i < _documentCount; ++i) {
				Guid sysId = Guid.NewGuid();
				string doc = $"<ScriptDocument><Id>{sysId}</Id><Order>2017-01-09:{i}</Order>{GetScripts()}</ScriptDocument>";
				File.WriteAllText($"{_rootDir}\\Doc_{i}.sdoc", doc);
				_docIds.Add(sysId);
			}

			string GetScripts() {
				return $@"
					<Scripts>
						<Script Id='{Guid.NewGuid()}' Executor='FakeExecutor' Order='2017-01-09'>PRINT 'Hello'</Script>
						<Script Id='{Guid.NewGuid()}' Executor='FakeExecutor' Order='2017-01-09:1'>PRINT 'Hello'</Script>
					</Scripts>";
			}
		}

	}

}
