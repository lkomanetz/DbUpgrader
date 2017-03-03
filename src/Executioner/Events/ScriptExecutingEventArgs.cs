using Executioner.Contracts;
using System;

namespace Executioner {

	public class ScriptExecutingEventArgs : EventArgs {

		public ScriptExecutingEventArgs(Script script) {
			this.Script = script;
		}

		public Script Script { get; private set; }
	}

}
