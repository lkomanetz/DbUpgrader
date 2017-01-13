using Executioner.Contracts;
using System;
using System.Collections.Generic;
using System.IO;

namespace Executioner {

	public class FileSystemLoader : IScriptLoader {
		private string _rootDir;

		public FileSystemLoader() {
			_rootDir = String.Empty;
		}

		public FileSystemLoader(string rootDirectory) {
			_rootDir = rootDirectory;
		}

		public IList<ScriptDocument> Documents { get; internal set; }

		public string Location {
			get { return _rootDir; }
			set { _rootDir = value; }
		}

		public void LoadDocuments() {
			if (this.Documents == null) {
				this.Documents = new List<ScriptDocument>();
			}

			IEnumerable<string> files = Directory.EnumerateFiles(_rootDir);
			foreach (string file in files) {
				using (Stream stream = new FileStream(file, FileMode.Open)) {
					this.Documents.Add(ScriptLoaderUtilities.CreateScriptDocument(stream, file));
				}
			}
		}

	}

}
