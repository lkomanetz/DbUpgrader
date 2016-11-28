using DbUpgrader.Contracts;
using DbUpgrader.DataService.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DbUpgrader.DataService {

	public class FileSystemService : IDataService {

		private const string ROOT_DIRECTORY = "DataRoot";
		private const string FILE_EXTENSION = "txt";
		private Assembly _executingAssembly;

		public FileSystemService(Assembly executingAssmebly) {
			_executingAssembly = executingAssmebly;
			CreateRootDirectoryIfNeeded();
		}

		public void Add(Script script) {
			throw new NotImplementedException();
		}

		public void Add(ScriptDocument document) {
			string path = $@"{Directory.GetCurrentDirectory()}\{ROOT_DIRECTORY}";
			string filePath = $@"{path}\{document.SysId}.{FILE_EXTENSION}";

			if (File.Exists(filePath)) {
				throw new Exception($"File {filePath} already exists.");
			}
			else {
				File.Create(filePath).Dispose();
			}
		}

		public void SetComplete(ScriptDocument document) {
			throw new NotImplementedException();
		}

		public IList<Guid> GetCompletedDocumentIds() {
			IList<Guid> completedDocumentIds = new List<Guid>();
			string path = $@"{Directory.GetCurrentDirectory()}\{ROOT_DIRECTORY}";
			string[] files = Directory.GetFiles(path);

			foreach (string file in files) {
				Guid id = Guid.Empty;
				if (!Guid.TryParse(Path.GetFileNameWithoutExtension(file), out id)) {
					throw new Exception("Unable to parse document Id from file name.");
				}
				completedDocumentIds.Add(id);
			}

			return completedDocumentIds;
		}

		public IList<Guid> GetCompletedScriptsFor(Guid documentId) {
			throw new NotImplementedException();
		}

		public void Clean() {
			string path = $@"{Directory.GetCurrentDirectory()}\{ROOT_DIRECTORY}";
			if (Directory.Exists(path)) {
				Directory.Delete(path, true);
			}
		}

		private void CreateRootDirectoryIfNeeded() {
			string path = $@"{Directory.GetCurrentDirectory()}\{ROOT_DIRECTORY}";
			if (!Directory.Exists(path)) {
				Directory.CreateDirectory(path);
			}
		}

	}

}
