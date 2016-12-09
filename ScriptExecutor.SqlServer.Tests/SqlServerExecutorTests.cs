using Executioner;
using Executioner.Tests.Classes;
using System;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ScriptExecutor.SqlServer.Tests {

	[TestClass]
	public class SqlServerExecutorTests {
		private static string connectionString;

		[ClassInitialize]
		public static void Initialize(TestContext context) {
			connectionString = ConfigurationManager.ConnectionStrings["TestConnectionString"].ConnectionString;
		}

		[TestMethod]
		[DeploymentItem("App.config")]
		public void ScriptExecutor_SqlServer_ConnectionSucceeds() {
			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='SqlServerExecutor' Order='2016-06-22:1'>print 'Hello'</Script>"
			};
			SqlServerExecutor sqlExecutor = new SqlServerExecutor(connectionString);
			ScriptExecutioner executioner = new ScriptExecutioner(
				new BaseMockLoader(scripts),
				new MemoryStore()
			);
			executioner.Add(sqlExecutor);
			executioner.Run();
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException), "Empty SQL script attempted to run.")]
		public void ScriptExecutor_SqlServer_ExceptionThrownOnEmptyScript() {
			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='SqlServerExecutor' Order='2016-06-22'></Script>"
			};

			ScriptExecutioner executioner = new ScriptExecutioner(
				new BaseMockLoader(scripts),
				new MemoryStore()
			);

			executioner.Add(new SqlServerExecutor(connectionString));
			executioner.Run();
		}
	}

}
