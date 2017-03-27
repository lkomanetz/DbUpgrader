using System;
using System.Collections.Generic;
using System.Linq;
using Executioner.Contracts;
using Executioner.Serializers;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace Executioner {

	public class FileSystemLogger : ILogger {

		private string _fileExt;
		private string _rootDir;
		private JsonSerializer _serializer;

		public FileSystemLogger(string rootDirectory) {
			_fileExt = ".json";
			_rootDir = rootDirectory;
			_serializer = new JsonSerializer(typeof(LogEntry));

			if (!Directory.Exists(_rootDir)) {
				Directory.CreateDirectory(_rootDir);
			}
		}

		public void Add(Script script) {
			LogEntry entry = Deserialize(script.DocumentId);

			if (entry == null) {
				throw new FileNotFoundException($"Document Id '{script.DocumentId}' not found.");
			}

			if (entry.Scripts.Any(x => x.SysId == script.SysId)) {
				return;
			}

			entry.Scripts.Add(new LogEntry() {
				SysId = script.SysId,
				IsComplete = script.IsComplete,
			});
			entry.IsComplete = entry.Scripts.All(x => x.IsComplete);

			Serialize(entry);
		}

		public void Add(ScriptDocument document) {
			LogEntry entry = Deserialize(document.SysId);
			if (entry == null) {
				Serialize(LogEntry.ToLogEntry(document));
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

				LogEntry entry = Deserialize(sysId);
				if (entry.IsComplete) {
					completedDocIds.Add(entry.SysId);
				}
			}

			return completedDocIds;
		}

		public IList<Guid> GetCompletedScriptIdsFor(Guid documentId) {
			LogEntry entry = Deserialize(documentId);
			if (entry == null) {
				return null;
			}

			return entry.Scripts
				.Where(scriptEntry => scriptEntry.IsComplete)
				.Select(scriptEntry => scriptEntry.SysId)
				.ToList();
		}

		public void Update(Script script) {
			LogEntry entry = Deserialize(script.DocumentId);
			if (entry == null) {
				throw new FileNotFoundException($"Document Id '{script.DocumentId}' not found.");
			}

			int index = entry.Scripts.FindIndex(x => x.SysId == script.SysId);

			entry.Scripts[index] = new LogEntry() { SysId = script.SysId, IsComplete = script.IsComplete };
			entry.IsComplete = entry.Scripts.All(x => x.IsComplete);
			Serialize(entry);
		}

		public void Clean() {
			if (Directory.Exists(_rootDir)) {
				Directory.Delete(_rootDir, true);
			}
		}

		private LogEntry Deserialize(Guid docId) {
			string fileLoc = $@"{_rootDir}\{docId}{_fileExt}";
			if (!File.Exists(fileLoc)) {
				return null;
			}

			LogEntry entry = null;
			string jsonData = File.ReadAllText(fileLoc);
			entry = _serializer.Deserialize<LogEntry>(jsonData);

			return entry;
		}

		private void Serialize(LogEntry entry) {
			string fileLoc = $@"{_rootDir}\{entry.SysId}{_fileExt}";

			if (!Directory.Exists(_rootDir)) {
				Directory.CreateDirectory(_rootDir);
			}

			FileStream fs = File.Create(fileLoc);
			fs.Dispose();

			using (FileStream writer = new FileStream(fileLoc, FileMode.Create, FileAccess.Write)) {
				string jsonData = _serializer.Serialize(entry);
				byte[] dataAsBytes = Encoding.UTF8.GetBytes(jsonData);
				writer.Write(dataAsBytes, 0, dataAsBytes.Length);
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
			[DataMember] public List<LogEntry> Scripts { get; set; }

			public static LogEntry ToLogEntry(ScriptDocument doc) {
				LogEntry entry = new LogEntry() {
					SysId = doc.SysId,
					IsComplete = doc.IsComplete
				};

				entry.Scripts = new List<LogEntry>();
				for (short i = 0; i < doc.Scripts.Count; ++i) {
					entry.Scripts.Add(
						new LogEntry() {
							SysId = doc.Scripts[i].SysId,
							IsComplete = doc.Scripts[i].IsComplete
						}
					);
				}

				return entry;
			}

		}

	}

}
