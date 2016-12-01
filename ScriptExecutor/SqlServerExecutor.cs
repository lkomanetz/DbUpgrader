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
		private IBackingStore _backingStore;
		private IScriptLoader _scriptLoader;
		private IList<Guid> _completedDocs;

		public SqlServerExecutor(string connectionString, IScriptLoader scriptLoader, IBackingStore backingStore) {
			_backingStore = backingStore;
			_connectionString = connectionString;
			_scriptLoader = scriptLoader;
			_completedDocs = new List<Guid>();
		}

		public IList<Guid> CompletedDocuments { get { return _backingStore.GetCompletedDocumentIds(); } }

		private void Execute(Script script) {
			SqlConnection conn = new SqlConnection(_connectionString);
			SqlTransaction transaction = null;

			try {
				conn.Open();
				transaction = conn.BeginTransaction();
				SqlCommand cmd = new SqlCommand(script.ScriptText, conn, transaction);
				cmd.ExecuteNonQuery();
				transaction.Commit();
			}
			catch (SqlException ex) {
				string msg = $"Script execution failed.\nScript Id: {script.SysId}\n";
				msg += $"Assembly: {script.AssemblyName}\n";
				msg += $"Sql: {script.ScriptText}\n";
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

		private void Execute(IList<Script> scripts) {
			for (short i = 0; i < scripts.Count; i++) {
				Execute(scripts[i]);
				scripts[i].IsComplete = true;
				_backingStore.Update(scripts[i]);
			}
		}

		public void Execute() {
			IList<ScriptDocument> docsToExecute = _backingStore.GetDocuments();
			for (short i = 0; i < docsToExecute.Count; ++i) {
				Execute(docsToExecute[i].Scripts);

				docsToExecute[i].IsComplete = true;
				_completedDocs.Add(docsToExecute[i].SysId);
				_backingStore.Update(docsToExecute[i]);
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

	}

}
