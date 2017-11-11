using Executioner.Contracts;
using System;

namespace Executioner.Tests.Classes {
	/// <summary>
	/// <para>This executor is currently only used to test adding multiple
	/// executors.</para>
	/// </summary>
	public class SecondScriptExecutor : IScriptExecutor, IMockScriptExecutor {

		public bool ScriptExecuted { get; set; }

		public bool Execute(string scriptText) {
			Console.WriteLine($"Executed Script: {scriptText}");
			return ScriptExecuted;
		}

	}

}