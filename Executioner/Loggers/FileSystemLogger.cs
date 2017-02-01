using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Executioner.Contracts;
using Executioner.ExtensionMethods;
using System.IO;
using System.Runtime.Serialization.Json;

namespace Executioner {

	public class FileSystemLogger : ILogger {

		private string _fileExt;
		private string _rootDir;
		private DataContractJsonSerializer _serializer;

		public FileSystemLogger(string rootDirectory) {
			_fileExt = ".json";
			_rootDir = rootDirectory;
			_serializer = new DataContractJsonSerializer(typeof(LogEntry));

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
				return null;
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

			LogEntry entry = null;
			using (FileStream reader = new FileStream(fileLoc, FileMode.Open)) {
				entry = (LogEntry)_serializer.ReadObject(reader);
			}

			return entry.ToScriptDocument();
		}

		private void Serialize(ScriptDocument doc) {
			LogEntry entry = LogEntry.ToLogEntry(doc);
			string fileLoc = $@"{_rootDir}\{doc.SysId}{_fileExt}";

			if (!Directory.Exists(_rootDir)) {
				Directory.CreateDirectory(_rootDir);
			}

			FileStream fs = File.Create(fileLoc);
			fs.Dispose();

			using (FileStream writer = new FileStream(fileLoc, FileMode.Create, FileAccess.Write)) {
				_serializer.WriteObject(writer, entry);	
			}
		}

		/*
		 * This class exists to reduce log size.  The reason it's private is because it is not meant to be known
		 * in any other part of the solution except for in the FileSystemLogger class. This keeps the interface the
		 * same but allowed me to change implementation detail without the rest of the solution having to change anything.
		 */
		[DataContract]
		private class LogEntry {

			[DataMember] public Guid SysId { get; set; }
			[DataMember] public bool IsComplete { get; set; }
			[DataMember] public LogEntry[] Scripts { get; set; }

			public ScriptDocument ToScriptDocument() {
				ScriptDocument doc = new ScriptDocument() {
					SysId = this.SysId,
					IsComplete = this.IsComplete
				};

				if (this.Scripts != null) {
					foreach (LogEntry script in this.Scripts) {
						doc.Scripts.Add(new Script() {
							SysId = script.SysId,
							IsComplete = script.IsComplete
						});
					}
				}

				return doc;
			}

			public static LogEntry ToLogEntry(ScriptDocument doc) {
				LogEntry entry = new LogEntry() {
					SysId = doc.SysId,
					IsComplete = doc.IsComplete
				};

				entry.Scripts = new LogEntry[doc.Scripts.Count];
				for (short i = 0; i < doc.Scripts.Count; ++i) {
					entry.Scripts[i] = new LogEntry() {
						SysId = doc.Scripts[i].SysId,
						IsComplete = doc.Scripts[i].IsComplete
					};
				}

				return entry;
			}

		}

	}

}
