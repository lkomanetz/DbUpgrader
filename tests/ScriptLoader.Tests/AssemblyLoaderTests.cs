using Executioner;
using Executioner.Contracts;
using ScriptLoader.Tests.FakeService;
using ScriptLoader.Tests.AnotherFakeService;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace ScriptLoader.Tests {

	public class AssemblyLoaderTests {

		private static AssemblyLoader _loader;
		private static Assembly _fakeServiceAssembly;

		public AssemblyLoaderTests() {
			_fakeServiceAssembly = typeof(MyFakeService).GetTypeInfo().Assembly;
			_loader = new AssemblyLoader(new List<Assembly>() { _fakeServiceAssembly });
		}

		[Fact]
		public void AssemblyLoader_LoaderCanFindDocuments() {
			_loader.LoadDocuments();
			Assert.True(_loader.Documents.Count >= 0, "Unable to find any SQL documents for upgrader.");
		}

		[Fact]
		public void AssemblyLoader_LoadDocumentsSucceeds() {
			_loader.LoadDocuments(); // This is loading from an assembly resource found in the [ClassInitialize] method.
			Assert.True(
				_loader.Documents.Count == 1,
				$"Expected 1 document -> Actual {_loader.Documents.Count}."
			);

			for (int i = 0; i < _loader.Documents.Count; ++i) {
				Assert.True(_loader.Documents[i].Scripts.Count >= 0, "Scripts not loaded.");
				Assert.True(
					_loader.Documents[i].Scripts.GetType() != typeof(Script[]),
					"ScriptLoader loaded scripts as an array."
				);
			}
		}

		[Fact]
		public void AssemblyLoader_MultipleAssembliesSucceeds() {
			int expectedDocumentCount = 2;
			IList<Assembly> assemblies = new List<Assembly>() {
				typeof(MyFakeService).GetTypeInfo().Assembly,
				typeof(Service).GetTypeInfo().Assembly
			};

			AssemblyLoader loader = new AssemblyLoader(assemblies);
			loader.LoadDocuments();

			Assert.True(
				loader.Documents.Count == expectedDocumentCount,
				$"Expected 2 documents -> Actual {loader.Documents.Count}."
			);

			for (int i = 0; i < loader.Documents.Count; ++i) {
				Assert.True(loader.Documents[i].Scripts.Count >= 0, "Scripts not loaded.");
			}
		}

		[Fact]
		public void AssemblyLoader_DocumentsAreOnlyLoadedOnce() {
			IList<Assembly> assemblies = new List<Assembly>() { typeof(MyFakeService).GetTypeInfo().Assembly };
			AssemblyLoader loader = new AssemblyLoader(assemblies);
			loader.LoadDocuments();
			loader.LoadDocuments();

			Assert.True(
				loader.Documents.Count == 1,
				$"Assembly loader loaded {loader.Documents.Count} documents.  Expeted one."
			);
		}

	}

}
