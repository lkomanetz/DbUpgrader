using DbUpgrader.Contracts.Interfaces;
using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbUpgrader.Tests.SqlServer {

	public class TestSqlServerCleaner : IDbCleaner {
		private string _connectionString;
		private string _sqlStatement;

		public TestSqlServerCleaner(string connectionString) {
			_connectionString = connectionString;
			_sqlStatement = GenerateSqlStatement();
		}

		public void Clean() {
			SqlConnection conn = new SqlConnection(_connectionString);
			SqlTransaction transaction = null;

			try {
				conn.Open();
				transaction = conn.BeginTransaction();
				SqlCommand cmd = new SqlCommand(_sqlStatement, conn, transaction);
				cmd.ExecuteNonQuery();
				transaction.Commit();
			}
			catch (Exception err) {
				transaction?.Rollback();
				throw err;
			}
			finally {
				transaction?.Dispose();
				conn?.Close();
				conn?.Dispose();
			}
		}

		private string GenerateSqlStatement() {
			return @"
				IF OBJECT_ID('dbo.TableA') IS NOT NULL
					DROP TABLE dbo.TableA
				IF OBJECT_ID('dbo.TableB') IS NOT NULL
					DROP TABLE dbo.TableB
				IF OBJECT_ID('Upgrader.ExecutedScripts') IS NOT NULL
					DROP TABLE Upgrader.ExecutedScripts
				IF OBJECT_ID('dbo.FirstTable') IS NOT NULL
					DROP TABLE dbo.FirstTable
				IF OBJECT_ID('dbo.AnotherTable') IS NOT NULL
					DROP TABLE dbo.AnotherTable
				IF OBJECT_ID('dbo.ThirdTable') IS NOT NULL
					DROP TABLE dbo.ThirdTable
			";
		}
	}

}
