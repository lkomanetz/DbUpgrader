using BackingStore.Contracts;
using Executioner.Contracts;
using ScriptExecutor.Contracts;
using ScriptLoader.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Executioner.Tests.Classes {

	public class MockScriptExecutor : IScriptExecutor {

		public void Execute(string scriptText) {
			Console.WriteLine($"Executed script: {scriptText}");
		}

	}

}
