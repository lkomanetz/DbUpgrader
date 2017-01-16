using Executioner.Contracts;
using System;
using System.Collections.Generic;
using System.IO;

namespace Executioner {

	public class FileSystemLoader : IScriptLoader {
		private string _rootDir;

		public FileSystemLoader() {
			_rootDir = String.Empty;
			this.Documents = new List<ScriptDocument>();
		}

		public FileSystemLoader(string rootDirectory) :
			this() {
			_rootDir = rootDirectory;
			CreateRootDirectory();
		}

		public IList<ScriptDocument> Documents { get; internal set; }

		public string Location {
			get { return _rootDir; }
			set { _rootDir = value; }
		}

		public void LoadDocuments() {
			CreateRootDirectory();

			IEnumerable<string> files = Directory.EnumerateFiles(_rootDir);
			foreach (string file in files) {
				using (Stream stream = new FileStream(file, FileMode.Open)) {
					this.Documents.Add(ScriptLoaderUtilities.CreateScriptDocument(stream, file));
				}
			}

			if (this.Documents.Count == 0) {
				throw new FileNotFoundException($"Script Documents not found in '{_rootDir}'.");
			}
		}

		private void CreateRootDirectory() {
			if (String.IsNullOrEmpty(_rootDir)) {
				return;	
			}

			if (!Directory.Exists(_rootDir)) {
				Directory.CreateDirectory(_rootDir);
			}
		}

	}

}
