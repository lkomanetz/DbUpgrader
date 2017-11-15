using System;

namespace Executioner {
	namespace Contracts {

		public class ExecutionRequest { 

			public bool ExecuteAllScripts { get; set; }
			public Func<Script, bool> ExecuteScriptsBetween { get; set; }

		}

	}

}