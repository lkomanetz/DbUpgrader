using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbUpgrader.Contracts {

	public class Script {
		public Guid SysId { get; set; }
		public DateTime DateCreated { get; set; }
		public int Order { get; set; }
		public string SqlScript { get; set; }
		public string AssemblyName { get; set; }
	}

}
