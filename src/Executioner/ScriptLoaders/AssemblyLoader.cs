using Executioner.Contracts;
using Executioner.Sorters;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using Executioner.ExtensionMethods;

namespace Executioner {

	public class AssemblyLoader : IScriptLoader {
		private IList<Assembly> _assemblies;
		private Sorter<ScriptDocument> _sorter;

		public AssemblyLoader(IList<Assembly> assemblies, Sorter<ScriptDocument> sorter) {
			_assemblies = assemblies;
			_sorter = sorter;
		}

		public IList<ScriptDocument> Documents { get; internal set; }

		public void LoadDocuments() {
			this.Documents = GetDocumentsToRun(_assemblies);
		}

		internal ScriptDocument[] GetDocumentsToRun(IList<Assembly> assemblies) {
			IList<ScriptDocument> documents = new List<ScriptDocument>();
			foreach (Assembly assembly in assemblies) {
				string[] resources = assembly.GetManifestResourceNames()
					.Where(x => x.Contains(ScriptLoaderConstants.FILE_EXTENSION))
					.ToArray<string>();

				for (short i = 0; i < resources.Length; ++i) {
					using (Stream stream = assembly.GetManifestResourceStream(resources[i])) {
						documents.Add(ScriptLoaderUtilities.CreateScriptDocument(stream, resources[i]));
					}
				}
			}

			return _sorter(documents).ToArray();
		}

	}

}
