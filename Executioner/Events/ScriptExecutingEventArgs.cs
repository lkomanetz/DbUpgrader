using Executioner.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Executioner {

	public class ScriptExecutingEventArgs : EventArgs {

		public ScriptExecutingEventArgs(Script script) {
			this.Script = script;
		}

		public Script Script { get; private set; }
	}

}
