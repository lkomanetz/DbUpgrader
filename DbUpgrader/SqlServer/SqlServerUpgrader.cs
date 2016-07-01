using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbUpgrader.Contracts.Interfaces;
using DbUpgrader.Contracts;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

namespace DbUpgrader.SqlServer {

	public class SqlServerUpgrader : IDbUpgrader {

		private Dictionary<Guid, Script> _scriptDictionary; // Dictionary<ScriptId, Script>
		private IScriptExecutor _scriptExecutor;
		private IList<Assembly> _successfullyRanAssemblies;

		public SqlServerUpgrader(string connectionString) {
			this.ConnectionString = connectionString;
			_scriptDictionary = new Dictionary<Guid, Script>();
			_scriptExecutor = new SqlServerExecutor(connectionString);
		}

		public string ConnectionString { get; private set; }

		public void Run(Assembly assembly)
		{
			this.InitializeUpgraderTables();
			if (HasAssemblyRanBefore(assembly))
			{
				throw new Exception($"An assembly cannot be run multiple times.\n Assembly: ${assembly.FullName}");
			}

			string[] resources = assembly.GetManifestResourceNames()
				.Where(x => x.Contains(".sql.xml"))
				.ToArray<String>();

			LoadScripts(assembly);
			IList<Script> scriptsToRun = GetScriptsToRun(assembly);
			_scriptExecutor.Execute(scriptsToRun);

			_successfullyRanAssemblies.Add(assembly);
		}

		private void LoadScripts(Assembly assembly) {
			Script[] scripts = this.GetScriptsFromXml(assembly);
			for (short i = 0; i < scripts.Length; i++) {
				_scriptDictionary.Add(scripts[i].SysId, scripts[i]);
			}
		}

		public void Run(IList<Assembly> assemblies) {
			for (short i = 0; i < assemblies.Count; i++) {
				this.Run(assemblies[i]);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool HasAssemblyRanBefore(Assembly assembly) {
			return _successfullyRanAssemblies.Any(
				x => x.FullName == assembly.FullName
			);
		}

		private IList<Script> GetScriptsToRun(Assembly assembly) {
			IList<Guid> scriptsAlreadyRan = _scriptExecutor.GetScriptsAlreadyRanFor(assembly.FullName);

			return _scriptDictionary.Select(x => x.Value)
				.Where(script => !scriptsAlreadyRan.Contains(script.SysId))
				.ToList<Script>();
		}

		internal void InitializeUpgraderTables() {
			Assembly upgraderAssembly = Assembly.GetExecutingAssembly();
			Script[] upgraderScripts = this.GetScriptsFromXml(upgraderAssembly);
			_scriptExecutor.Execute(upgraderScripts);
		}

		internal Script[] GetScriptsFromXml(Assembly assembly) {
			string[] resources = assembly.GetManifestResourceNames()
				.Where(x => x.Contains(".sql.xml"))
				.ToArray<String>();
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
					scripts[i].DateCreated = orderValues.Item1;
					scripts[i].Order = orderValues.Item2;
					scripts[i].AssemblyName = assembly.FullName;
				}
			}
			return scripts.OrderBy(x => x.DateCreated)
				.ThenBy(x => x.Order)
				.ToArray<Script>();
		}

		internal Tuple<DateTime, int> ParseOrderAttribute(string value) {
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

	}

}
