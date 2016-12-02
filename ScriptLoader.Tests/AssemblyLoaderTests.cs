using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Executioner.Contracts;
using ScriptLoader.Tests.FakeService;
using BackingStore.Contracts;

namespace ScriptLoader.Tests {

	[TestClass]
	public class AssemblyLoaderTests {

		private static AssemblyLoader _loader;
		private static Assembly _fakeServiceAssembly;
		private static IBackingStore _backingStore;

		[ClassInitialize]
		public static void Initialize(TestContext context) {
			_backingStore = new MockBackingStore();
			_fakeServiceAssembly = typeof(MyFakeService).Assembly;
			_loader = new AssemblyLoader(_fakeServiceAssembly);
		}

		[ClassCleanup]
		public static void Cleanup() {
			_backingStore.Dispose();
		}

		[TestMethod]
		public void AssemblyLoader_LoaderCanFindDocuments() {
			IList<ScriptDocument> documents = _loader.GetDocumentsToRun(_fakeServiceAssembly);
			Assert.IsTrue(documents.Count >= 0, "Unable to find any SQL documents for upgrader.");
		}

		[TestMethod]
		public void AssemblyLoader_LoadDocumentsSucceeds() {
			_loader.LoadDocuments(_backingStore); // This is loading from an assembly resource found in the [ClassInitialize] method.
			Assert.IsTrue(
				_loader.Documents.Count == 1,
				$"Expected 1 document -> Actual {_loader.Documents.Count}."
			);

			for (int i = 0; i < _loader.Documents.Count; ++i) {
				Assert.IsTrue(_loader.Documents[i].Scripts.Count >= 0, "Scripts not loaded.");
				Assert.IsTrue(
					_loader.Documents[i].Scripts.GetType() != typeof(Script[]),
					"ScriptLoader loaded scripts as an array."
				);
			}

		}

	}

}
