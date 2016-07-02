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
			using (SqlConnection conn = new SqlConnection(_connectionString)) {
				conn.Open();
				using (SqlTransaction transaction = conn.BeginTransaction()) {
					SqlCommand cmd = new SqlCommand(script.SqlScript, conn, transaction);
					cmd.ExecuteNonQuery();
					transaction.Commit();
					LogScriptAsRan(script.SysId, script.AssemblyName);
				}
			}
		}

		public void Execute(IList<Script> scripts) {
			for (short i = 0; i < scripts.Count; i++) {
				Execute(scripts[i]);
			}
		}

		public IList<Guid> GetScriptsAlreadyRanFor(string assemblyName) {
			List<Guid> scriptIds = new List<Guid>();
			using (SqlConnection conn = new SqlConnection(_connectionString)) {
				string cmdString = "SELECT ScriptId FROM [Upgrader].[ExecutedScripts] WHERE [AssemblyName] = @assemblyName";
				SqlCommand cmd = new SqlCommand(cmdString, conn);
				cmd.Parameters.AddWithValue("@assemblyName", assemblyName);

				conn.Open();
				SqlDataReader reader = cmd.ExecuteReader();
				while (reader.Read()) {
					scriptIds.Add(reader.GetGuid(0));
				}
			}

			return scriptIds;
		}

		private void LogScriptAsRan(Guid scriptId, string assemblyName)
		{
			string cmdString = $@"
				INSERT INTO [Upgrader].[ExecutedScripts] (
					SysId
					, ScriptId
					, DateExecuted
					, AssemblyName
				)
				VALUES (
					@sysId
					, @scriptId
					, @dateExecuted
					, @assemblyName
				)";

			using (SqlConnection conn = new SqlConnection(_connectionString)) {
				conn.Open();
				using (SqlTransaction transaction = conn.BeginTransaction()) {
					SqlCommand cmd = new SqlCommand(cmdString, conn, transaction);
					cmd.Parameters.AddWithValue("@sysId", Guid.NewGuid());
					cmd.Parameters.AddWithValue("@scriptId", scriptId);
					cmd.Parameters.AddWithValue("@dateExecuted", DateTime.UtcNow);
					cmd.Parameters.AddWithValue("@assemblyName", assemblyName);

					cmd.ExecuteNonQuery();
					transaction.Commit();
				}
			}

		}

	}

}
