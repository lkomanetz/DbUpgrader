using Executioner.Contracts;
using System;

namespace Executioner {

	public class ScriptExecutedEventArgs : EventArgs {

		public ScriptExecutedEventArgs(Script script) {
			this.Script = script;
		}

		public Script Script { get; private set; }

	}

}