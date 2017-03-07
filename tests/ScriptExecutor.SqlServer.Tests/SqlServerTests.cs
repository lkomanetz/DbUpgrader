using Executioner;
using Executioner.Tests.Classes;
using System;
using System.Linq;
using Xunit;

namespace ScriptExecutor.SqlServer.Tests {

	public class SqlServerExecutorTests {
		private static string connectionString = "Server=localHost; Integrated Security=true";

		[Fact]
		public void ScriptExecutor_SqlServer_ConnectionSucceeds() {
			Exception ex = Record.Exception(() => {
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
			});
			Assert.Null(ex);
		}

		[Fact]
		public void ScriptExecutor_SqlServer_ExceptionThrownOnEmptyScript() {
			Exception ex = Record.Exception(() => {
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
			});
			Assert.NotNull(ex);
			Assert.True(ex is InvalidOperationException, "Empty SQL script attempted to run.");
		}

	}

}