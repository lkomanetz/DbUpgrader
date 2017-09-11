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
				for (short i = 0; i < doc.Scripts.Count; ++i) {
					entry.Scripts.Add(
						new LogEntry() {
							SysId = doc.Scripts[i].SysId,
							IsComplete = doc.Scripts[i].IsComplete
						}
					);
				}

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