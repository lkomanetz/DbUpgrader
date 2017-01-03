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
			catch (InvalidOperationException ex) {
				transaction.Rollback();
				throw ex;
			}
			catch (SqlException ex) {
				transaction.Rollback();
				throw ex;
			}
			finally {
				transaction.Dispose();
				conn.Close();
				conn.Dispose();
			}
		}

	}

}
