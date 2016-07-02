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

		private IScriptExecutor _scriptExecutor;
		private IList<Assembly> _successfullyRanAssemblies;

		public SqlServerUpgrader(string connectionString) {
			this.ConnectionString = connectionString;
			_scriptExecutor = new SqlServerExecutor(connectionString);
			_successfullyRanAssemblies = new List<Assembly>();
		}

		public string ConnectionString { get; private set; }

		public event EventHandler OnBeforeRunStarted = delegate { };
		public event EventHandler OnRunCompleted = delegate { };

		public void Run(IList<Assembly> assemblies) {
			BeforeRunStarted(EventArgs.Empty);
			for (short i = 0; i < assemblies.Count; i++) {
				this.Run(assemblies[i]);
			}
			RunCompleted(EventArgs.Empty);
		}

		private void Run(Assembly assembly) {
			this.InitializeUpgraderTables();
			if (HasAssemblyRanBefore(assembly.FullName)) {
				throw new Exception($"An assembly cannot be run multiple times.\n Assembly: ${assembly.FullName}");
			}

			string[] resources = assembly.GetManifestResourceNames()
				.Where(x => x.Contains(".sql.xml"))
				.ToArray<String>();

			IList<Script> scriptsToRun = GetScriptsToRun(assembly);
			_scriptExecutor.Execute(scriptsToRun);

			_successfullyRanAssemblies.Add(assembly);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool HasAssemblyRanBefore(string assemblyName) {
			return _successfullyRanAssemblies.Any(
				x => x.FullName == assemblyName
			);
		}

		internal IList<Script> GetScriptsToRun(Assembly assembly) {
			Script[] scripts = this.GetScriptsFromXml(assembly);
			IList<Guid> scriptsAlreadyRan = _scriptExecutor.GetScriptsAlreadyRanFor(assembly.FullName);

			return scripts.Where(script => !scriptsAlreadyRan.Contains(script.SysId))
				.ToList<Script>();
		}

		internal void InitializeUpgraderTables() {
			Assembly upgraderAssembly = Assembly.GetExecutingAssembly();
			IList<Script> scriptsToRun = GetScriptsToRun(upgraderAssembly);
			_scriptExecutor.Execute(scriptsToRun);
			_successfullyRanAssemblies.Add(upgraderAssembly);
		}

		internal Script[] GetScriptsFromXml(Assembly assembly) {
			string[] resources = assembly.GetManifestResourceNames()
				.Where(x => x.Contains(".sql.xml"))
				.ToArray<String>();

			if (resources.Length > 1) {
				throw new Exception($"Only one /Database/<script> file allowed. Found: {resources.Length}");
			}
			Script[] scripts = null;

			using (Stream stream = assembly.GetManifestResourceStream(resources[0]))
			using (StreamReader reader = new StreamReader(stream)) {
				string xml = reader.ReadToEnd();
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(xml);
				XmlNodeList scriptNodes = xmlDoc.GetElementsByTagName("script");
				scripts = new Script[scriptNodes.Count];

				for (short i = 0; i < scriptNodes.Count; i++) {
					Tuple<DateTime, int> orderValues = ParseOrderAttribute(
						scriptNodes[i].Attributes["Order"].Value
					);
					scripts[i] = new Script();
					scripts[i].SysId = Guid.Parse(scriptNodes[i].Attributes["Id"].Value);
					scripts[i].SqlScript = scriptNodes[i].InnerText;
					scripts[i].DateCreatedUtc = orderValues.Item1;
					scripts[i].Order = orderValues.Item2;
					scripts[i].AssemblyName = assembly.FullName;
				}
			}
			return scripts.OrderBy(x => x.DateCreatedUtc)
				.ThenBy(x => x.Order)
				.ToArray<Script>();
		}

		private Tuple<DateTime, int> ParseOrderAttribute(string value) {
			string pattern = @"(\d{2,4}-\d{1,2}-\d{1,2}):*(\d{1,3})*";
			Match match = Regex.Match(value, pattern);

			if (!match.Success) {
				return null;
			}

			DateTime date = DateTime.Parse(match.Groups[1].Value);
			string orderStr = match.Groups[2].Value;
			int order = String.IsNullOrEmpty(orderStr) ? Int32.Parse("0") : Int32.Parse(orderStr);

			return Tuple.Create<DateTime, int>(date, order);
		}

		private void BeforeRunStarted(EventArgs e) { OnBeforeRunStarted(this, e); }
		private void RunCompleted(EventArgs e) { OnRunCompleted(this, e); }
	}

}
