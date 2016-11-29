using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptLoader.Contracts {

	public class Script {
		public Guid SysId { get; set; }
		public Guid DocumentId { get; set; }
		public DateTime DateCreatedUtc { get; set; }
		public int Order { get; set; }
		public string ScriptText { get; set; }
		public string AssemblyName { get; set; }
		public bool IsComplete { get; set; }
	}

}
