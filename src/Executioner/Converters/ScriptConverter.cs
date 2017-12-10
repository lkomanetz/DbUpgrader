using System;
using Executioner.Contracts;

namespace Executioner.Converters {

	internal static class ScriptConverter {

		internal static Converter<Script, LogEntry> ToLogEntry => (s) => 
			new LogEntry(s.SysId, DateTime.UtcNow);

		internal static Converter<string, LogEntry> FromEntryText => (entryText) => {
			string[] vals = entryText.Split("->");
			return new LogEntry(
				Guid.Parse(vals[0]),
				DateTime.Parse(vals[1])
			);
		};

	}

}