using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Executioner.Contracts;
using Executioner.ExtensionMethods;
using System.Xml.Serialization;
using System.IO;

namespace Executioner {

	public class FileSystemLogger : ILogger {

		private string _fileExt;
		private string _rootDir;
		private XmlSerializer _serializer;

		public FileSystemLogger(string rootDirectory) {
			_fileExt = ".xml";
			_rootDir = rootDirectory;
			_serializer = new XmlSerializer(typeof(ScriptDocument));

			if (!Directory.Exists(_rootDir)) {
				Directory.CreateDirectory(_rootDir);
			}
		}

		public void Add(Script script) {
			ScriptDocument doc = Deserialize(script.DocumentId);
			if (doc == null) {
				throw new FileNotFoundException($"Document Id '{script.DocumentId}' not found.");
			}

			if (doc.Scripts.Any(x => x.SysId == script.SysId)) {
				return;
			}

			doc.Scripts.Add(script);
			doc.Scripts = (List<Script>)doc.Scripts.SortOrderedItems();
			doc.IsComplete = doc.Scripts.AreComplete();

			Serialize(doc);
		}

		public void Add(ScriptDocument document) {
			ScriptDocument existingDoc = Deserialize(document.SysId);
			if (existingDoc == null) {
				Serialize(document);
			}
		}

		public IList<Guid> GetCompletedDocumentIds() {
			IEnumerable<string> filePaths = Directory.EnumerateFiles(_rootDir);
			if (filePaths.Count() == 0) {
				return new List<Guid>();
			}

			IList<Guid> completedDocIds = new List<Guid>();
			foreach (string path in filePaths) {
				string fileName = Path.GetFileNameWithoutExtension(path);
				Guid sysId = Guid.Empty;
				if (!Guid.TryParse(fileName, out sysId)) {
					throw new FormatException($"{fileName} is incorrect GUID format.");
				}
				ScriptDocument doc = Deserialize(sysId);
				if (doc.IsComplete) {
					completedDocIds.Add(doc.SysId);
				}
			}

			return completedDocIds;
		}

		public IList<Guid> GetCompletedScriptIdsFor(Guid documentId) {
			ScriptDocument doc = Deserialize(documentId);
			if (doc == null) {
				throw new FileNotFoundException($"Document Id '{documentId}' not found.");
			}

			return doc.Scripts
				.Where(x => x.IsComplete)
				.Select(x => x.SysId)
				.ToList();
		}

		public void Update(Script script) {
			ScriptDocument doc = Deserialize(script.DocumentId);
			if (doc == null) {
				throw new FileNotFoundException($"Document Id '{script.DocumentId}' not found.");
			}
			int index = doc.Scripts.FindIndex(x => x.SysId == script.SysId);

			doc.Scripts[index] = script;
			doc.IsComplete = doc.Scripts.AreComplete();
			Serialize(doc);
		}

		public void Clean() {
			if (Directory.Exists(_rootDir)) {
				Directory.Delete(_rootDir, true);
			}
		}

		private ScriptDocument Deserialize(Guid docId) {
			string fileLoc = $@"{_rootDir}\{docId}{_fileExt}";
			if (!File.Exists(fileLoc)) {
				return null;
			}

			ScriptDocument doc = null;
			using (StreamReader reader = new StreamReader(fileLoc)) {
				doc = (ScriptDocument)_serializer.Deserialize(reader);
			}

			return doc;
		}

		private void Serialize(ScriptDocument doc) {
			string fileLoc = $@"{_rootDir}\{doc.SysId}{_fileExt}";

			if (!Directory.Exists(_rootDir)) {
				Directory.CreateDirectory(_rootDir);
			}

			FileStream fs = File.Create(fileLoc);
			fs.Dispose();

			using (StreamWriter sw = new StreamWriter(fileLoc, false)) {
				_serializer.Serialize(sw, doc);
			}
		}

	}

}
