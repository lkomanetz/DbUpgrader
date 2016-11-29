using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptLoader.Contracts {
	public class ScriptDocument {

		public Guid SysId { get; set; }
		public DateTime DateCreatedUtc { get; set; }
		public int Order { get; set; }
		public IList<Script> Scripts { get; set; }
		public string ResourceName { get; set; }
		public bool IsComplete { get; set; }

	}
}
