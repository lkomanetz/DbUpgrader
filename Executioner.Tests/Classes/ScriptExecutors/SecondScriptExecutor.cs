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

		public void Execute(string scriptText) {
			Console.WriteLine($"Executed: {scriptText}");
		}

	}

}
