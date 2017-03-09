using Executioner;
using Executioner.Tests.Classes;
using System;
using System.Linq;
using System.Reflection;
using Xunit;
using System.Collections.Generic;

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

		[Fact]
		public void ScriptExecutor_SqlServer_CDataHandledInSqlScriptSucceeds() {
			string createTableScript = @"
				IF OBJECT_ID('dbo.TestTable', 'U') IS NOT Null
					DROP TABLE dbo.TestTable
				CREATE TABLE dbo.TestTable(
					TestXml XML NOT NULL
				)
			";
			string completedlyCDataWrapped = @"
				<![CDATA[
					INSERT INTO TestTable
					VALUES ('<root><child>Hello</child></root>')
				]]>
			";
			string dropTableScript = "DROP TABLE TestTable";
			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='SqlServerExecutor' Order='2017-03-08'>{createTableScript}</Script>",
				$"<Script Id='{Guid.NewGuid()}' Executor='SqlServerExecutor' Order='2017-03-08:1'>{completedlyCDataWrapped}</Script>",
				$"<Script Id='{Guid.NewGuid()}' Executor='SqlServerExecutor' Order='2017-03-08:2'>{dropTableScript}</Script>"
			};
			Exception ex = Record.Exception(() => {
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
		public void ScriptExecutor_SqlServer_CDataInMiddleOfSqlSucceeds() {
			string sqlScript = @"
				CREATE TABLE dbo.AnotherTest (
					RefId UNIQUEIDENTIFIER NOT NULL
					, TestXml XML NOT NULL
				)
				DECLARE @id AS UNIQUEIDENTIFIER = NEWID();
				DECLARE @xml as XML = <![CDATA['<root><child>Hello</child></root>']]>;

				INSERT INTO dbo.AnotherTest
				VALUES (@id, @xml)

				DROP TABLE dbo.AnotherTest
			";
			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='SqlServerExecutor' Order='2017-03-08'>{sqlScript}</Script>"
			};

			Exception ex = Record.Exception(() => {
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

	}

}