using System;

namespace Executioner.Tests {

	public class Program {
		
		public static void Main(string[] args) {
			ExecutionerTests tests = new ExecutionerTests();
			tests.CreateExecutorsFromScriptsSucceeds();
		}

	}

}