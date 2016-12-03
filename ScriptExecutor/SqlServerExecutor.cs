using ScriptExecutor.Contracts;
using Executioner.Contracts;
using BackingStore.Contracts;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptLoader.Contracts;

namespace ScriptExecutor {

	public class SqlServerExecutor : IScriptExecutor {
		private string _connectionString;

		public SqlServerExecutor(string connectionString) {
			_connectionString = connectionString;
		}

		public void Execute(string scriptText) {
			SqlConnection conn = new SqlConnection(_connectionString);
			SqlTransaction transaction = null;

			try {
				conn.Open();
				transaction = conn.BeginTransaction();
				SqlCommand cmd = new SqlCommand(scriptText, conn, transaction);
				cmd.ExecuteNonQuery();
				transaction.Commit();
			}
			catch (SqlException ex) {
				string msg = $"Script execution failed.\n";
				msg += $"Sql: {scriptText}\n";
				msg += $"SqlException Trace: {ex.StackTrace}";
				transaction.Rollback();
				throw new Exception(msg);
			}
			finally {
				transaction.Dispose();
				conn.Close();
				conn.Dispose();
			}
		}

	}

}
