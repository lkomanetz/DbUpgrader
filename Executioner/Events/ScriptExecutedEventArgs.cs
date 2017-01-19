using Executioner.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Executioner {

	public class ScriptExecutedEventArgs : EventArgs {

		public ScriptExecutedEventArgs(Script script) {
			this.Script = script;
		}

		public Script Script { get; private set; }

	}

}
