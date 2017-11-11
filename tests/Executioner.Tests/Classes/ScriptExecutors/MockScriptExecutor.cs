using Executioner.Contracts;
using System;

namespace Executioner.Tests.Classes {

	public class MockScriptExecutor : IScriptExecutor, IMockScriptExecutor {

		public bool ScriptExecuted { get; set; }

		public bool Execute(string scriptText) {
			Console.WriteLine($"Executed script: {scriptText}");
			return ScriptExecuted;
		}

	}

}