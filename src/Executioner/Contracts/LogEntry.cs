using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace Executioner {
	namespace Contracts {

		internal class LogEntry {

			private const string DATE_FORMAT = "yyyy-MM-dd HH:mm:ss.fff";
 
			private readonly Guid _sysId;
			private readonly DateTime _timeCompletedUtc;

			public LogEntry(Guid sysId, DateTime timeCompletedUtc) {
				_sysId = sysId;
				_timeCompletedUtc = timeCompletedUtc;
			}

			public Guid SysId => _sysId;
			public DateTime TimeCompletedUtc => _timeCompletedUtc;

			public override string ToString() =>
				$"{_sysId} -> {_timeCompletedUtc.ToString(DATE_FORMAT, CultureInfo.InvariantCulture)}";

			public override int GetHashCode() => _sysId.GetHashCode();

			public override bool Equals(object obj) {
				if (ReferenceEquals(this, obj)) return true;
				if (obj == null || this.GetType() != obj.GetType()) return false;

				LogEntry entry = (LogEntry)obj;
				return entry.SysId == this.SysId;
			}

			public static bool operator !=(LogEntry entryA, LogEntry entryB) =>
				!(entryA == entryB);

			public static bool operator ==(LogEntry entryA, LogEntry entryB) {
				if (ReferenceEquals(entryA, entryB)) return true;
				else return entryA.SysId == entryB.SysId;
			}

		}

	}

}