using Executioner.Contracts;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.Runtime.CompilerServices;

namespace Executioner.BackingStore {

	public class FileSystemStore : IBackingStore {

		private string _rootDir;
		private XmlSerializer _serializer;

		public FileSystemStore(string rootDir) {
			_rootDir = rootDir;
			_serializer = new XmlSerializer(typeof(ScriptDocument));
		}

		public void Add(Script script) {
			ScriptDocument doc = Deserialize(script.DocumentId);
			if (doc == null) {
				throw new FileNotFoundException($"Document Id '{script.DocumentId}' not found.");
			}

			doc.Scripts.Add(script);
			doc.Scripts = (List<Script>)Sort(doc.Scripts);
			doc.IsComplete = AreComplete(doc.Scripts);

			Serialize(doc);
		}

		public void Add(ScriptDocument document) {
			Serialize(document);
		}

		public void Clean() {
			if (Directory.Exists(_rootDir)) {
				Directory.Delete(_rootDir, true);
			}
		}

		public bool Delete(Script script) {
			ScriptDocument doc = Deserialize(script.DocumentId);
			if (doc == null) {
				throw new FileNotFoundException($"Document Id '{script.DocumentId}' not found.");
			}
			bool scriptRemoved = doc.Scripts.Remove(
				doc.Scripts.Where(x => x.SysId == script.SysId).SingleOrDefault()
			);

			if (scriptRemoved) {
				doc.Scripts = (List<Script>)Sort(doc.Scripts);
				Serialize(doc);
			}

			return scriptRemoved;
		}

		public bool Delete(ScriptDocument document) {
			string fileLoc = $@"{_rootDir}\{document.SysId}{ScriptLoaderConstants.FILE_EXTENSION}";
			bool deleteCompleted = false;
			try {
				if (File.Exists(fileLoc)) {
					File.Delete(fileLoc);
				}
				deleteCompleted = true;
			}
			catch {
				deleteCompleted = false;	
			}

			return deleteCompleted;
		}

		public void Dispose() {
			_serializer = null;
			_rootDir = null;
		}

		public IList<Guid> GetCompletedDocumentIds() {
			IEnumerable<string> filePaths = Directory.EnumerateFiles(_rootDir);
			if (filePaths.Count() == 0) {
				return null;
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

		public IList<ScriptDocument> GetDocuments(GetDocumentsRequest request = null) {
			request = request ?? new GetDocumentsRequest() { IncludeCompletedDocuments = true };
			IEnumerable<string> filePaths = Directory.EnumerateFiles(_rootDir);
			if (filePaths.Count() == 0) {
				return null;
			}

			List<ScriptDocument> documents = new List<ScriptDocument>();
			foreach (string path in filePaths) {
				string fileName = Path.GetFileNameWithoutExtension(path);
				Guid sysId = Guid.Empty;
				if (!Guid.TryParse(fileName, out sysId)) {
					throw new FormatException($"{fileName} is incorrect GUID format.");
				}
				documents.Add(Deserialize(sysId));
			}

			if (request.IncludeCompletedDocuments) {
				documents = documents.Where(x => x.IsComplete || !x.IsComplete).ToList();
			}
			else {
				documents = documents.Where(x => !x.IsComplete).ToList();
			}

			return Sort(documents);
		}

		public IList<Script> GetScriptsFor(Guid documentId) {
			ScriptDocument doc = Deserialize(documentId);

			if (doc == null) {
				return null;
			}

			return Sort(doc.Scripts);
		}

		public void Update(Script script) {
			ScriptDocument doc = Deserialize(script.DocumentId);
			if (doc == null) {
				throw new FileNotFoundException($"Document Id '{script.DocumentId}' not found.");
			}
			int index = doc.Scripts.FindIndex(x => x.SysId == script.SysId);

			doc.Scripts[index] = script;
			doc.IsComplete = AreComplete(doc.Scripts);
			Serialize(doc);
		}

		public void Update(ScriptDocument document) {
			string fileLoc = $@"{_rootDir}\{document.SysId}{ScriptLoaderConstants.FILE_EXTENSION}";
			if (!File.Exists(fileLoc)) {
				throw new FileNotFoundException($"Document Id '{document.SysId}' not found.");
			}

			Serialize(document);
		}

		private ScriptDocument Deserialize(Guid docId) {
			string fileLoc = $@"{_rootDir}\{docId}{ScriptLoaderConstants.FILE_EXTENSION}";
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
			string fileLoc = $@"{_rootDir}\{doc.SysId}{ScriptLoaderConstants.FILE_EXTENSION}";

			if (!Directory.Exists(_rootDir)) {
				Directory.CreateDirectory(_rootDir);
			}

			FileStream fs = File.Create(fileLoc);
			fs.Dispose();

			using (StreamWriter sw = new StreamWriter(fileLoc, false)) {
				_serializer.Serialize(sw, doc);
			}
		}

		private IList<Script> Sort(IList<Script> scripts) {
			return scripts
				.OrderBy(script => script.DateCreatedUtc)
				.ThenBy(script => script.Order)
				.ToList();
		}

		private IList<ScriptDocument> Sort(IList<ScriptDocument> documents) {
			return documents
				.OrderBy(doc => doc.DateCreatedUtc)
				.ThenBy(doc => doc.Order)
				.ToList();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool AreComplete(IList<Script> scripts) {
			return scripts.All(script => script.IsComplete);
		}

	}

}
