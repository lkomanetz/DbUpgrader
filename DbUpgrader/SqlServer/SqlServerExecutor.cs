using DbUpgrader.Contracts;
using DbUpgrader.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbUpgrader.SqlServer {

	public class SqlServerExecutor : IScriptExecutor {
		private string _connectionString;

		public SqlServerExecutor(string connectionString) {
			_connectionString = connectionString;
		}

		public void Execute(Script script) {
			using (SqlConnection conn = new SqlConnection(_connectionString))
			using (SqlTransaction transaction = conn.BeginTransaction())  {
				SqlCommand cmd = new SqlCommand(script.SqlScript, conn, transaction);
				cmd.ExecuteNonQuery();
				transaction.Commit();
			}
		}

		public void Execute(Script[] scripts) {
			for (short i = 0; i < scripts.Length; i++) {
				Execute(scripts[i]);
			}
		}

	}

}
