using DbUpgrader.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using DbUpgrader.DataService.Contracts;
using DbUpgrader.Contracts;

namespace DbUpgrader.DataService.Tests {

	public class MockUpgrader : IDbUpgrader {
		private IScriptLoader _scriptLoader;
		private IDataService _dataService;
		private IList<Assembly> _successfullyRanAssemblies;

		public MockUpgrader(IDataService dataService, IScriptLoader scriptLoader) {
			_dataService = dataService;
			_scriptLoader = scriptLoader;
			_successfullyRanAssemblies = new List<Assembly>();
		}

		public void Run(IList<Assembly> assemblies) {
			for (short i = 0; i < assemblies.Count; ++i) {
				if (_successfullyRanAssemblies.Any(x => x.FullName == assemblies[i].FullName)) {
					throw new Exception($"Assembly '{assemblies[i].FullName}' has already ran.");
				}
				_scriptLoader.LoadDocuments(assemblies[i]);

				foreach (var doc in _scriptLoader.Documents) {
					IList<Guid> completedScripts = _dataService.GetCompletedScriptsFor(doc.SysId);
					IList<Script> scriptsToRun = FindScriptsThatNeedToRun(doc.Scripts, completedScripts);
				}
				_successfullyRanAssemblies.Add(assemblies[i]);	
			}
		}

		public void Run(ScriptDocument document) {
			document.IsComplete = true;
			_dataService.Update(document);
		}

		private IList<Script> FindScriptsThatNeedToRun(IList<Script> scriptsToRun, IList<Guid> scriptsAlreadyRan) {
			foreach (var scriptId in scriptsAlreadyRan) {
				Script scriptToRemove = scriptsToRun.Where(x => x.SysId == scriptId).SingleOrDefault();
				if (scriptToRemove != null) {
					scriptsToRun.Remove(scriptToRemove);
				}
			}

			return scriptsToRun.OrderBy(x => x.DateCreatedUtc)
				.ThenBy(x => x.Order)
				.ToList();
		}
	}

}
