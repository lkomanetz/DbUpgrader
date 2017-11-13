using Executioner.Contracts;
using System.Collections.Generic;

namespace Executioner.Converters {

	internal static class ScriptDocumentConverter {

		internal static Converter<ScriptDocument, LogEntry> ToLogEntry = (sDoc) => {
			LogEntry entry = new LogEntry() {
				SysId = sDoc.SysId,
				IsComplete = sDoc.IsComplete
			};

			entry.Scripts = new List<LogEntry>();
			for (short i = 0; i < sDoc.Scripts.Count; ++i) 
				entry.Scripts.Add(ScriptConverter.ToLogEntry(sDoc.Scripts[i]));

			return entry;
		};

	}

}