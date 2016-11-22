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

//TODO(Logan) -> Refactor the code that gets the ScriptDocuments and their scripts.
namespace DbUpgrader.SqlServer {

	public class SqlServerUpgrader : IDbUpgrader {

		private IScriptExecutor _scriptExecutor;
		private IList<Assembly> _successfullyRanAssemblies;

		private const string ROOT_NODE = "ScriptDocument";
		private const string SCRIPT_NODE = "Script";

		public SqlServerUpgrader(string connectionString) {
			_scriptExecutor = new SqlServerExecutor(connectionString);
			_successfullyRanAssemblies = new List<Assembly>();
		}

		public event EventHandler OnBeforeRunStarted = delegate { };
		public event EventHandler OnRunCompleted = delegate { };

		public void Run(IList<Assembly> assemblies) {
			BeforeRunStarted(EventArgs.Empty);
			this.InitializeUpgraderTables();

			for (short i = 0; i < assemblies.Count; i++) {
				this.Run(assemblies[i]);
			}

			RunCompleted(EventArgs.Empty);
		}

		private void Run(Assembly assembly) {
			if (HasAssemblyRanBefore(assembly.FullName)) {
				throw new Exception($"An assembly cannot be run multiple times.\n Assembly: ${assembly.FullName}");
			}

			string[] resources = assembly.GetManifestResourceNames()
				.Where(x => x.Contains(".sql.xml"))
				.ToArray<String>();


			IList<ScriptDocument> documentsToRun = GetDocumentsToRun(assembly);
			for (short i = 0; i < documentsToRun.Count; ++i) {
				IList<Script> scriptsToRun = GetScriptsToRun(assembly, documentsToRun[i]);
				_scriptExecutor.Execute(scriptsToRun);
			}

			_successfullyRanAssemblies.Add(assembly);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool HasAssemblyRanBefore(string assemblyName) {
			return _successfullyRanAssemblies.Any(
				x => x.FullName == assemblyName
			);
		}

		internal IList<Script> GetScriptsToRun(Assembly assembly, ScriptDocument document) {
			Script[] scripts = this.GetScriptsFromResource(assembly, document.ResourceName);
			IList<Guid> scriptsAlreadyRan = _scriptExecutor.GetScriptsAlreadyRanFor(assembly.FullName);

			return scripts.Where(script => !scriptsAlreadyRan.Contains(script.SysId))
				.ToList<Script>();
		}

		internal void InitializeUpgraderTables() {
			Assembly upgraderAssembly = Assembly.GetExecutingAssembly();
			IList<ScriptDocument> documents = GetDocumentsToRun(upgraderAssembly);

			for (short i = 0; i < documents.Count; ++i) {
				IList<Script> scriptsToRun = GetScriptsFromResource(upgraderAssembly, documents[i].ResourceName);
				_scriptExecutor.Execute(scriptsToRun);
			}
			_successfullyRanAssemblies.Add(upgraderAssembly);
		}

		internal Script[] GetScriptsFromResource(Assembly assembly, string resourceName) {
			Script[] scripts = null;
			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
			using (StreamReader reader = new StreamReader(stream)) {
				string xml = reader.ReadToEnd();
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(xml);
				XmlNodeList scriptNodes = xmlDoc.GetElementsByTagName(SCRIPT_NODE);
				scripts = new Script[scriptNodes.Count];

				for (short i = 0; i < scriptNodes.Count; i++) {
					Tuple<DateTime, int> orderValues = ParseOrderAttribute(
						scriptNodes[i].Attributes["Order"].Value
					);
					scripts[i] = new Script() {
						SysId = Guid.Parse(scriptNodes[i].Attributes["Id"].Value),
						SqlScript = scriptNodes[i].InnerText,
						DateCreatedUtc = orderValues.Item1,
						Order = orderValues.Item2,
						AssemblyName = assembly.FullName
					};
				}
			}
			return scripts.OrderBy(x => x.DateCreatedUtc)
				.ThenBy(x => x.Order)
				.ToArray<Script>();
		}

		internal IList<ScriptDocument> GetDocumentsToRun(Assembly assembly) {
			string[] resources = assembly.GetManifestResourceNames()
				.Where(x => x.Contains(".sql.xml"))
				.ToArray<String>();

			ScriptDocument[] documents = new ScriptDocument[resources.Length];
			for (short i = 0; i < resources.Length; ++i) {
				using (Stream stream = assembly.GetManifestResourceStream(resources[i]))
				using (StreamReader reader = new StreamReader(stream)) {
					string xmlStr = reader.ReadToEnd();

					XmlDocument xmlDoc = new XmlDocument();
					xmlDoc.LoadXml(xmlStr);

					Tuple<DateTime, int> orderValues = ParseOrderAttribute(xmlDoc.SelectSingleNode($"{ROOT_NODE}/Order").InnerText);

					documents[i] = new ScriptDocument() {
						SysId = Guid.Parse(xmlDoc.SelectSingleNode($"{ROOT_NODE}/SysId").InnerText),
						DateCreatedUtc = orderValues.Item1,
						Order = orderValues.Item2,
						ResourceName = resources[i]
					};
				}
			}

			return documents;
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
