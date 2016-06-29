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

namespace DbUpgrader.SqlServer {

	public class SqlServerUpgrader : IDbUpgrader {
		private const string SCRIPT_LOCATION = "/Database/";
		private Dictionary<Guid, Script> _scripts; // Dictionary<ScriptId, Script>

		public SqlServerUpgrader(string connectionString) {
			this.ConnectionString = connectionString;
			_scripts = new Dictionary<Guid, Script>();
		}

		public string ConnectionString { get; private set; }

		public void Run(Assembly assembly) {
			string[] resources = assembly.GetManifestResourceNames()
				.Where(x => x.Contains(".sql.xml"))
				.ToArray<String>();

			Script[] scripts = this.LoadScriptsFromXml(assembly);

			for (short i = 0; i < scripts.Length; i++) {
				_scripts.Add(scripts[i].Id, scripts[i]);
			}
		}

		internal Script[] LoadScriptsFromXml(Assembly assembly) {
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
					scripts[i].Id = Guid.Parse(scriptNodes[i].Attributes["Id"].Value);
					scripts[i].SqlScript = scriptNodes[i].InnerText;
					scripts[i].DateCreated = orderValues.Item1;
					scripts[i].Order = orderValues.Item2;
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
