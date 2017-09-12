using Executioner.Contracts;
using System;
using System.Collections.Generic;

namespace Executioner {
	namespace Converters {

		internal delegate TOutput Converter<TInput, TOutput>(TInput input);

		internal static class LogEntryConverters {

			internal static Converter<ScriptDocument, LogEntry> FromScriptDocument = (doc) => {
				LogEntry entry = new LogEntry() {
					SysId = doc.SysId,
					IsComplete = doc.IsComplete
				};

				entry.Scripts = new List<LogEntry>();
				for (short i = 0; i < doc.Scripts.Count; ++i) 
					entry.Scripts.Add(LogEntryConverters.FromScript(doc.Scripts[i]));

				return entry;
			};

			internal static Converter<Script, LogEntry> FromScript = (script) => {
				return new LogEntry() {
					SysId = script.SysId,
					IsComplete = script.IsComplete
				};
			};

		}

	}

}