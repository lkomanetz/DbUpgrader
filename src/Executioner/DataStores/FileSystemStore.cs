using System;
using System.Collections.Generic;
using System.Linq;
using Executioner.Contracts;
using Executioner.Serializers;
using Executioner.Converters;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace Executioner {

	public class FileSystemStore : IDataStore {

		private readonly string _fileExt;
		private readonly string _rootDir;
		private readonly JsonSerializer _serializer;

		public FileSystemStore(string rootDirectory) {
			_fileExt = ".txt";
			_rootDir = rootDirectory;
			_serializer = new JsonSerializer(typeof(LogEntry));

			if (!Directory.Exists(_rootDir))
				Directory.CreateDirectory(_rootDir);
		}

		public void CreateLogFile(Guid documentId) {
			string fileLocation = $@"{_rootDir}\{documentId}{_fileExt}";
			if (!File.Exists(fileLocation)) File.Create(fileLocation).Close();
		}

		public void Add(Script script) {
			LogEntry entry = ScriptConverter.ToLogEntry(script);
			AppendToLog(entry, script.DocumentId);
		}

		public IList<Guid> GetCompletedScriptIdsFor(Guid documentId) {
			string fileLocation = $@"{_rootDir}\{documentId}{_fileExt}";
			string[] lines = File.ReadAllLines(fileLocation);
			IList<LogEntry> entries = new List<LogEntry>();

			foreach (string line in lines) entries.Add(ScriptConverter.FromEntryText(line));
			return entries.Distinct()
				.Select(e => e.SysId)
				.ToList();
		}

		public void Clean() {
			if (Directory.Exists(_rootDir))
				Directory.Delete(_rootDir, true);
		}

		private void AppendToLog(LogEntry entry, Guid docId) {
			string fileLocation = $@"{_rootDir}\{docId}{_fileExt}";
			if (!File.Exists(fileLocation)) throw new FileNotFoundException(fileLocation);

			using (StreamWriter writer = File.AppendText(fileLocation)) {
				writer.WriteLine(entry.ToString());
			} 
		}

	}

}