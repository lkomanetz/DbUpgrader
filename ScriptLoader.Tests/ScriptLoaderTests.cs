using Executioner;
using Executioner.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ScriptLoader.Tests {

	[TestClass]
	public class ScriptLoaderTests {

		private static IScriptLoader _loader;
		private static string _rootDir;

		[ClassInitialize]
		public static void Initialize(TestContext context) {
			_rootDir = "C:\\ScriptLoaderTests";
			_loader = new FileSystemLoader(_rootDir);
		}

		[TestCleanup]
		public void Clean() {
			if (Directory.Exists(_rootDir)) {
				Directory.Delete(_rootDir, true);
			}
		}

		[TestMethod]
		public void ScriptLoader_UndeclaredNamespaceInScriptTextSucceeds() {
			try
			{
				// This xml for script text is not allowed because the XML document it stores it in doesn't know of
				// the 'xs' namespace (there is no xmlns sections declaring it) so it's considered malformed XML.
				SetupDocument($"<xs:TestElement>Hello</xs:TestElement>"); 
				_loader.LoadDocuments();
			}
			catch (XmlException) {
				Assert.IsTrue(false, "Script loader failed to load script with undeclared namespace.");
			}
		}

		/*
		 * I'm not using a [TestInitialize] method for this since I want to be able to dictate what the script text
		 * is for each test.  This method should be called for each script loader test.  The Clean() method will make
		 * sure the directory is removed before the next test.
		 */
		private void SetupDocument(string scriptText) {
			Directory.CreateDirectory(_rootDir);
			string scriptStr = $"<Scripts><Script Id='{Guid.NewGuid()}' Executor='Test' Order='2017-01-27'>{scriptText}</Script></Scripts>";
			string doc = $"<ScriptDocument><Id>{Guid.NewGuid()}</Id><Order>2017-01-27</Order>{scriptStr}</ScriptDocument>";

			File.WriteAllText($"{_rootDir}\\Test_Doc.sdoc", doc);
		}
	}

}
