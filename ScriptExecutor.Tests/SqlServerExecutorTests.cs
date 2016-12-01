using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScriptExecutor;
using ScriptLoader.Contracts;
using BackingStore.Contracts;

namespace ScriptExecutor.Tests {

	[TestClass]
	public class SqlServerExecutorTests {
		private static SqlServerExecutor _executor;
		private static IScriptLoader _loader;
		private static IBackingStore _backingStore;

		[ClassInitialize]
		public static void Initialize(TestContext context) {
			_executor = new SqlServerExecutor("testString", _loader, _backingStore);
		}

		[TestMethod]
		public void SqlServer_ExecuteUpdatesCompletionProperty() {
		}

	}

}
