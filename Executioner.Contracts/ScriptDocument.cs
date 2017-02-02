using System;
using System.Collections.Generic;

namespace Executioner.Contracts {

	public class ScriptDocument : IOrderedItem {
		
		public ScriptDocument() {
			this.Scripts = new List<Script>();
		}

		public Guid SysId { get; set; }
		public DateTime DateCreatedUtc { get; set; }
		public int Order { get; set; }
		public List<Script> Scripts { get; set; }
		public string ResourceName { get; set; }
		public bool IsComplete { get; set; }

	}

}