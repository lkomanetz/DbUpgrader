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
			throw new NotImplementedException();
		}

		public void Add(ScriptDocument document) {
			string fileLoc = $@"{_rootDir}\{document.SysId}{ScriptLoaderConstants.FILE_EXTENSION}";
			if (File.Exists(fileLoc)) {
				return;
			}

			Directory.CreateDirectory(_rootDir);
			FileStream stream = File.Create(fileLoc);
			stream.Dispose();

			using (StreamWriter sw = new StreamWriter(fileLoc)) {
				_serializer.Serialize(sw, document);
			}
		}

		public void Clean() {
			throw new NotImplementedException();
		}

		public bool Delete(Script script) {
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

		public IList<Guid> GetCompletedDocumentIds() {
			throw new NotImplementedException();
		}

		public IList<Guid> GetCompletedScriptIdsFor(Guid documentId) {
			throw new NotImplementedException();
		}

		public IList<ScriptDocument> GetDocuments(GetDocumentsRequest request = null) {
			throw new NotImplementedException();
		}

		public IList<Script> GetScriptsFor(Guid documentId) {
			string fileLoc = $@"{_rootDir}\{documentId}{ScriptLoaderConstants.FILE_EXTENSION}";
			if (!File.Exists(fileLoc)) {
				return null;
			}

			ScriptDocument doc = null;
			using (StreamReader reader = new StreamReader(fileLoc)) {
				doc = (ScriptDocument)_serializer.Deserialize(reader);
			}

			if (doc == null) {
				return null;
			}

			return Sort(doc.Scripts);
		}

		public void Update(Script script) {
			throw new NotImplementedException();
		}

		public void Update(ScriptDocument document) {
			throw new NotImplementedException();
		}

		private IList<Script> Sort(IList<Script> scripts) {
			return scripts
				.OrderBy(script => script.DateCreatedUtc)
				.ThenBy(script => script.Order)
				.ToList();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool AreComplete(IList<Script> scripts) {
			return scripts.All(script => script.IsComplete);
		}

	}

}
