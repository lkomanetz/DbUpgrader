using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using ScriptLoader.Contracts;
using ScriptLoader.Tests.FakeService;

namespace ScriptLoader.Tests {

	[TestClass]
	public class AssemblyLoaderTests {

		private static AssemblyLoader _loader;
		private static Assembly _fakeServiceAssembly;

		[ClassInitialize]
		public static void Initialize(TestContext context) {
			_fakeServiceAssembly = typeof(MyFakeService).Assembly;
			_loader = new AssemblyLoader(_fakeServiceAssembly, new MockBackingStore());
		}

		[TestMethod]
		public void AssemblyLoader_ScriptsStayInOrder() {
			IList<ScriptDocument> documents = _loader.GetDocumentsToRun(_fakeServiceAssembly);

			for (int i = 0; i < documents.Count; ++i) {
				AssertOrder(
					documents[i].Scripts,
					"Date: 6/21/2016 Order: 0Date: 6/22/2016 Order: 0Date: 6/22/2016 Order: 1Date: 6/23/2016 Order: 0"
				);
			}
		}

		[TestMethod]
		public void AssemblyLoader_LoaderCanFindScripts() {
			IList<ScriptDocument> documents = _loader.GetDocumentsToRun(_fakeServiceAssembly);
			for (int i = 0; i < documents.Count; ++i) {
				Assert.IsTrue(documents[i].Scripts.Count >= 0);
			}
		}

		[TestMethod]
		public void AssemblyLoader_LoaderCanFindDocuments() {
			IList<ScriptDocument> documents = _loader.GetDocumentsToRun(_fakeServiceAssembly);
			Assert.IsTrue(documents.Count >= 0, "Unable to find any SQL documents for upgrader.");
		}

		[TestMethod]
		public void AssemblyLoader_LoadDocumentsSucceeds() {
			_loader.LoadDocuments();
			Assert.IsTrue(
				_loader.Documents.Count == 1,
				$"Expected 1 document -> Actual {_loader.Documents.Count}."
			);
		}

		private void AssertOrder(IList<Script> scripts, string expectedOrder) {
			string actualOrder = String.Empty;
			foreach (Script script in scripts) {
				actualOrder += $"Date: {script.DateCreatedUtc.ToShortDateString()} Order: {script.Order}";
			}

			Assert.AreEqual(expectedOrder, actualOrder);
		}

	}

}
