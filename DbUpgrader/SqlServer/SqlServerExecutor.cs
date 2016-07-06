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
			SqlConnection conn = new SqlConnection(_connectionString);
			SqlTransaction transaction = null;

			try {
				conn.Open();
				transaction = conn.BeginTransaction();
				SqlCommand cmd = new SqlCommand(script.SqlScript, conn, transaction);
				cmd.ExecuteNonQuery();
				transaction.Commit();
				LogScriptAsRan(script.SysId, script.AssemblyName);
			}
			catch (SqlException ex) {
				string msg = $"Script execution failed.\nScript Id: {script.SysId}\n";
				msg += $"Assembly: {script.AssemblyName}\n";
				msg += $"Sql: {script.SqlScript}\n";
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

		public void Execute(IList<Script> scripts) {
			for (short i = 0; i < scripts.Count; i++) {
				Execute(scripts[i]);
			}
		}

		public IList<Guid> GetScriptsAlreadyRanFor(string assemblyName) {
			List<Guid> scriptIds = new List<Guid>();
			SqlConnection conn = new SqlConnection(_connectionString);

			try {
				string cmdString = "SELECT ScriptId FROM [Upgrader].[ExecutedScripts] WHERE [AssemblyName] = @assemblyName";
				SqlCommand cmd = new SqlCommand(cmdString, conn);
				cmd.Parameters.AddWithValue("@assemblyName", assemblyName);

				conn.Open();
				SqlDataReader reader = cmd.ExecuteReader();

				while (reader.Read()) {
					scriptIds.Add(reader.GetGuid(0));
				}
			}
			catch {
				return scriptIds;
			}
			finally {
				conn.Close();
				conn.Dispose();
			}

			return scriptIds;
		}

		private void LogScriptAsRan(Guid scriptId, string assemblyName) {
			/*
			 * I'm not providing the names of the columns because when this first runs DateExecutedUtc
			 * starts out as being named DateExecuted.  This will cause the insert to fail.
			 */
			string cmdString = $@"
				INSERT INTO [Upgrader].[ExecutedScripts]
				VALUES (
					@sysId
					, @scriptId
					, @dateExecutedUtc
					, @assemblyName
				)";

			using (SqlConnection conn = new SqlConnection(_connectionString)) {
				conn.Open();
				using (SqlTransaction transaction = conn.BeginTransaction()) {
					SqlCommand cmd = new SqlCommand(cmdString, conn, transaction);
					cmd.Parameters.AddWithValue("@sysId", Guid.NewGuid());
					cmd.Parameters.AddWithValue("@scriptId", scriptId);
					cmd.Parameters.AddWithValue("@dateExecutedUtc", DateTime.UtcNow);
					cmd.Parameters.AddWithValue("@assemblyName", assemblyName);

					cmd.ExecuteNonQuery();
					transaction.Commit();
				}
			}
		}

	}

}
