﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Executioner.Contracts;
using ScriptLoader.Tests.FakeService;
using Executioner;
using ScriptLoader.Tests.AnotherFakeService;

namespace ScriptLoader.Tests {

	[TestClass]
	public class AssemblyLoaderTests {

		private static AssemblyLoader _loader;
		private static Assembly _fakeServiceAssembly;

		[ClassInitialize]
		public static void Initialize(TestContext context) {
			_fakeServiceAssembly = typeof(MyFakeService).Assembly;
			_loader = new AssemblyLoader(new List<Assembly>() { _fakeServiceAssembly });
		}

		[TestMethod]
		public void AssemblyLoader_LoaderCanFindDocuments() {
			IList<ScriptDocument> documents = _loader.GetDocumentsToRun(new List<Assembly>() { _fakeServiceAssembly });
			Assert.IsTrue(documents.Count >= 0, "Unable to find any SQL documents for upgrader.");
		}

		[TestMethod]
		public void AssemblyLoader_LoadDocumentsSucceeds() {
			_loader.LoadDocuments(); // This is loading from an assembly resource found in the [ClassInitialize] method.
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

		[TestMethod]
		public void AssemblyLoader_MultipleAssembliesSucceeds() {
			IList<Assembly> assemblies = new List<Assembly>() {
				typeof(MyFakeService).Assembly,
				typeof(Service).Assembly
			};

			AssemblyLoader loader = new AssemblyLoader(assemblies);
			loader.LoadDocuments();

			Assert.IsTrue(
				loader.Documents.Count == 2,
				$"Expected 2 documents -> Actual {loader.Documents.Count}."
			);

			for (int i = 0; i < loader.Documents.Count; ++i) {
				Assert.IsTrue(loader.Documents[i].Scripts.Count >= 0, "Scripts not loaded.");
			}
		}

		[TestMethod]
		public void AssemblyLoader_DocumentsAreOnlyLoadedOnce() {
			IList<Assembly> assemblies = new List<Assembly>() { typeof(MyFakeService).Assembly };
			AssemblyLoader loader = new AssemblyLoader(assemblies);
			loader.LoadDocuments();
			loader.LoadDocuments();

			Assert.IsTrue(
				loader.Documents.Count == 1,
				$"Assembly loader loaded {loader.Documents.Count} documents.  Expeted one."
			);
		}

	}

}
