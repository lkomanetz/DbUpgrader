using DbUpgrader.Contracts;
using DbUpgrader.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml;

namespace DbUpgrader.SqlServer {

	public class SqlServerUpgrader : IDbUpgrader {

		private IScriptLoader _scriptLoader;
		private IScriptExecutor _scriptExecutor;
		private IList<Assembly> _successfullyRanAssemblies;

		private const string ROOT_NODE = "ScriptDocument";
		private const string SCRIPT_NODE = "Script";

		public SqlServerUpgrader(string connectionString) {
			_scriptLoader = new ScriptLoader();
			_scriptExecutor = new SqlServerExecutor(connectionString);
			_successfullyRanAssemblies = new List<Assembly>();
		}

		public event EventHandler OnBeforeRunStarted = delegate { };
		public event EventHandler OnRunCompleted = delegate { };

		public void Run(IList<Assembly> assemblies) {
			BeforeRunStarted(EventArgs.Empty);
			this.InitializeUpgraderTables();

			for (short i = 0; i < assemblies.Count; i++) {
				if (HasAssemblyRanBefore(assemblies[i].FullName)) {
					throw new Exception($"An assembly cannot be run multiple times.\n Assembly: ${assemblies[i].FullName}");
				}
				_scriptLoader.LoadDocuments(assemblies[i]);
				this.Run(_scriptLoader.Documents, assemblies[i]);
				_successfullyRanAssemblies.Add(assemblies[i]);
			}

			RunCompleted(EventArgs.Empty);
		}

		//TODO(Logan) -> I need to refactor how the code determines which scripts to run and how to pass the assembly.
		private void Run(IList<ScriptDocument> documents, Assembly assembly) {
			for (short i = 0; i < documents.Count; ++i) {
				IList<Script> scriptsToRun = FindScriptsThatNeedToRun(documents[i].Scripts, assembly);
				_scriptExecutor.Execute(scriptsToRun);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool HasAssemblyRanBefore(string assemblyName) {
			return _successfullyRanAssemblies.Any(
				x => x.FullName == assemblyName
			);
		}

		internal void InitializeUpgraderTables() {
			Assembly upgraderAssembly = Assembly.GetExecutingAssembly();

			IScriptLoader scriptLoader = new ScriptLoader();
			scriptLoader.LoadDocuments(upgraderAssembly);
			IList<ScriptDocument> documents = scriptLoader.Documents;

			for (short i = 0; i < documents.Count; ++i) {
				IList<Script> scriptsToRun = FindScriptsThatNeedToRun(documents[i].Scripts, upgraderAssembly);
				_scriptExecutor.Execute(scriptsToRun);
			}

			_successfullyRanAssemblies.Add(upgraderAssembly);
		}

		private IList<Script> FindScriptsThatNeedToRun(IList<Script> scriptsToRun, Assembly assembly) {
			IList<Guid> scriptsAlreadyRan = _scriptExecutor.GetScriptsAlreadyRanFor(assembly.FullName);
			foreach (Guid scriptId in scriptsAlreadyRan) {
				Script scriptToRemove = scriptsToRun.Where(x => x.SysId == scriptId).SingleOrDefault();
				if (scriptToRemove != null) {
					scriptsToRun.Remove(scriptToRemove);
				}
			}

			return scriptsToRun.OrderBy(x => x.DateCreatedUtc)
				.ThenBy(x => x.Order)
				.ToList();
		}

		private void BeforeRunStarted(EventArgs e) { OnBeforeRunStarted(this, e); }
		private void RunCompleted(EventArgs e) { OnRunCompleted(this, e); }
	}

}