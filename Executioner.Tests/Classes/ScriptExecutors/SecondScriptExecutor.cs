using Executioner.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Executioner.Tests.Classes {
	/// <summary>
	/// <para>This executor is currently only used to test adding multiple
	/// executors.</para>
	/// </summary>
	public class SecondScriptExecutor : IScriptExecutor {
		public bool ScriptExecuted { get; set; }

		public bool Execute(string scriptText) {
			Console.WriteLine($"Executed: {scriptText}");
			return ScriptExecuted;
		}

	}

}
