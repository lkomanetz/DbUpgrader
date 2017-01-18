using Executioner.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Executioner.Tests.Classes {

	public class MockScriptExecutor : IScriptExecutor {

		public bool ScriptExecuted { get; set; }

		public bool Execute(string scriptText) {
			Console.WriteLine($"Executed script: {scriptText}");
			return ScriptExecuted;
		}

	}

}
