using System;
using Executioner.Contracts;

namespace Executioner.Converters {

	internal static class ScriptConverter {

		internal static Converter<Script, LogEntry> ToLogEntry = (s) => {
				return new LogEntry() {
					SysId = s.SysId,
					IsComplete = s.IsComplete
				};
		};

	}

}