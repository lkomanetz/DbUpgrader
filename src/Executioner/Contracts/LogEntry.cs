using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Executioner {
	namespace Contracts {

		[DataContract]
		internal class LogEntry {

			[DataMember] public Guid SysId { get; set; }
			[DataMember] public bool IsComplete { get; set; }
			[DataMember] public List<LogEntry> Scripts { get; set; }

		}

	}

}