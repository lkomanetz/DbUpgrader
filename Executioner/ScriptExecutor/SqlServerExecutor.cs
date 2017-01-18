using Executioner.Contracts;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Executioner {

	public class SqlServerExecutor : IScriptExecutor {
		private string _connectionString;

		public SqlServerExecutor() {
			_connectionString = String.Empty;
		}

		public SqlServerExecutor(string connectionString) {
			_connectionString = connectionString;
		}

		public string ConnectionString {
			get { return _connectionString; }
			set { _connectionString = value; }
		}

		public bool Execute(string scriptText) {
			bool scriptExecuted = false;
			using (SqlConnection conn = new SqlConnection(_connectionString)) {
				conn.Open();
				using (SqlTransaction tx = conn.BeginTransaction()) {
					SqlCommand cmd = new SqlCommand(scriptText, conn, tx);
					cmd.ExecuteNonQuery();
					tx.Commit();
					scriptExecuted = true;
				}
			}

			return scriptExecuted;
		}

	}

}
