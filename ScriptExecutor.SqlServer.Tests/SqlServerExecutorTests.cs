using Executioner;
using Executioner.Tests.Classes;
using System;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace ScriptExecutor.SqlServer.Tests {

	[TestClass]
	public class SqlServerExecutorTests {
		private static string connectionString;

		[ClassInitialize]
		public static void Initialize(TestContext context) {
			connectionString = ConfigurationManager.ConnectionStrings["TestConnectionString"].ConnectionString;
		}

		[TestMethod]
		public void ScriptExecutor_SqlServer_ConnectionSucceeds() {
			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='SqlServerExecutor' Order='2016-06-22:1'>print 'Hello'</Script>"
			};
			ScriptExecutioner executioner = new ScriptExecutioner(
				new BaseMockLoader(scripts),
				new MockLogger()
			);
			var executor = (SqlServerExecutor)executioner.ScriptExecutors
				.Where(x => x.GetType() == typeof(SqlServerExecutor))
				.Single();
			executor.ConnectionString = connectionString;

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
				new MockLogger()
			);

			var executor = (SqlServerExecutor)executioner.ScriptExecutors
				.Where(x => x.GetType() == typeof(SqlServerExecutor))
				.Single();
			executor.ConnectionString = connectionString;

			executioner.Run();
		}

	}

}
