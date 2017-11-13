using Executioner.Contracts;
using Executioner.ExtensionMethods;
using Executioner.Sorters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Executioner {

	public class FileSystemLoader : IScriptLoader {

		private string _rootDir;
		private Sorter<IOrderedItem> _sorter;

		public FileSystemLoader(Sorter<IOrderedItem> sorter) {
			_rootDir = String.Empty;
			_sorter = sorter;
			this.Documents = new List<ScriptDocument>();
		}

		public FileSystemLoader(
			string rootDirectory,
			Sorter<IOrderedItem> sorter
		) : this(sorter) {
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
			IList<IOrderedItem> docs = new List<IOrderedItem>();
			foreach (string file in files) {
				using (Stream stream = new FileStream(file, FileMode.Open)) {
					docs.Add(ScriptLoaderUtilities.CreateScriptDocument(stream, file));
				}
			}

			if (this.Documents.Count == 0) {
				throw new FileNotFoundException($"Script Documents not found in '{_rootDir}'.");
			}

			this.Documents = _sorter.Invoke(docs).Select(x => (ScriptDocument)x).ToList();
			// this.Documents = this.Documents.SortOrderedItems();
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
