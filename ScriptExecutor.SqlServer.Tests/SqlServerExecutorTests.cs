using Executioner;
using Executioner.Tests.Classes;
using System;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ScriptExecutor.SqlServer.Tests {

	//TODO(Logan) -> Refactor the unit tests so I can dynamically add scripts as needed for a test.
	[TestClass]
	public class SqlServerExecutorTests {

		[TestMethod]
		public void ScriptExecutor_SqlServer_ConnectionSucceeds() {
			string connectionStr = ConfigurationManager.ConnectionStrings["TestConnectionString"].ConnectionString;
			SqlServerExecutor sqlExecutor = new SqlServerExecutor(connectionStr);
			ScriptExecutioner executioner = new ScriptExecutioner(
				new MockSqlScriptLoader(),
				new MemoryStore()
			);
			executioner.Add(sqlExecutor);
			executioner.Run();
		}

	}

}
