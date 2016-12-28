using Executioner;
using Executioner.Tests.Classes;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ScriptExecutor.CSharp.Tests {

	[TestClass]
	public class CSharpExecutorTests {

		[TestMethod]
		public void CSharp_NoExceptionsThrown() {
			string code = @"Console.WriteLine(""Hello world!"");";

			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='CSharpExecutor' Order='2016-12-28'>{code}</Script>"
			};

			ScriptExecutioner executioner = CreateExecutor(scripts);
			executioner.Run();
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException), "Compile time error not found.")]
		public void CSharp_CompilerErrorsFound() {
			// This is missing the semi-colon
			string code = @"Console.WriteLine(""Hello world!"")";

			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='CSharpExecutor' Order='2016-12-28'>{code}</Script>"
			};

			ScriptExecutioner executioner = CreateExecutor(scripts);
			executioner.Run();
		}

		private ScriptExecutioner CreateExecutor(string[] scripts) {
			ScriptExecutioner executioner = new ScriptExecutioner(
				new BaseMockLoader(scripts),
				new MockLogger()
			);
			executioner.Add(new CSharpExecutor());

			return executioner;
		}
	}

}
