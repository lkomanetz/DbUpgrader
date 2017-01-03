using Executioner;
using Executioner.Tests.Classes;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScriptExecutor.CSharp.Tests.Classes;
using ScriptExecutor.CSharp.Tests.TestClasses;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace ScriptExecutor.CSharp.Tests {

	[TestClass]
	public class CSharpExecutorTests {

		[TestMethod]
		public void CSharp_NoExceptionsThrown() {
			string code = @"Console.WriteLine(""Hello world!"");";

			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='CSharpExecutor' Order='2016-12-28'>{code}</Script>"
			};

			ScriptExecutioner executioner = CreateExecutioner(
				scripts: scripts,
				usingStatements: null,
				referencedAssemblies: null
			);
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

			ScriptExecutioner executioner = CreateExecutioner(
				scripts: scripts,
				usingStatements: null,
				referencedAssemblies: null
			);

			executioner.Run();
		}

		[TestMethod]
		public void CSharp_CallToStaticMethodInSameAssembly_Succeeds() {
			string code = @"SameAssemblyClass.StaticMethod();";
			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='CSharpExecutor' Order='2016-12-29'>{code}</Script>"
			};

			string[] usingStatements = new string[] {
				"using ScriptExecutor.CSharp.Tests.Classes"
			};

			var referencedAssemblies = new List<Assembly>() {
				typeof(SameAssemblyClass).Assembly
			};

			var executioner = CreateExecutioner(scripts, usingStatements, referencedAssemblies);
			executioner.Run();
		}

		[TestMethod]
		public void CSharp_CallToStaticMethodInDifferentAssembly_Succeeds() {
			string code = @"TestClass.StaticMethod();";
			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='CSharpExecutor' Order='2016-12-29'>{code}</Script>"
			};

			string[] usingStatements = new string[] {
				"using ScriptExecutor.CSharp.Tests.TestClasses;"
			};

			var referencedAssemblies = new List<Assembly>() {
				typeof(TestClass).Assembly
			};

			var executioner = CreateExecutioner(scripts, usingStatements, referencedAssemblies);
			executioner.Run();
		}

		[TestMethod]
		public void CSharp_CallToStaticMethodsInMultipleAssemblies_Succeeds() {
			string code = @"TestClass.StaticMethod(); SameAssemblyClass.StaticMethod();";
			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='CSharpExecutor' Order='2016-12-29'>{code}</Script>"
			};

			string[] usingStatements = new string[] {
				"ScriptExecutor.CSharp.Tests.Classes",
				"ScriptExecutor.CSharp.Tests.TestClasses"
			};

			var referencedAssemblies = new List<Assembly>() {
				typeof(TestClass).Assembly,
				typeof(SameAssemblyClass).Assembly,
			};

			var executioner = CreateExecutioner(scripts, usingStatements, referencedAssemblies);
			executioner.Run();
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException), "No compile time error with missing using statements.")]
		public void CSharp_CallToStaticMethodWithoutUsingStatements_Fails() {
			string code = @"TestClass.StaticMethod();";
			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='CSharpExecutor' Order='2016-12-29'>{code}</Script>"
			};

			var referencedAssemblies = new List<Assembly>() {
				typeof(TestClass).Assembly
			};

			var executioner = CreateExecutioner(scripts, null, referencedAssemblies);
			executioner.Run();
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException), "No compile time error with missing referenced assemblies.")]
		public void CSharp_CallToStaticMethodWithoutAssemblies_Fails() {
			string code = @"TestClass.StaticMethod();";
			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='CSharpExecutor' Order='2016-12-29'>{code}</Script>"
			};
			string[] usingStatements = new string[] {
				"ScriptExecutor.CSharp.Tests.TestClasses"
			};

			var executioner = CreateExecutioner(scripts, usingStatements, null);
			executioner.Run();
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException), "No compile time error with missing using and referenced assemblies")]
		public void CSharp_MissingUsingAndReferencedAssemblies_Fails() {
			string code = @"TestClass.StaticMethod();";
			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='CSharpExecutor' Order='2016-12-29'>{code}</Script>"
			};

			var executioner = CreateExecutioner(scripts, null, null);
			executioner.Run();

		}

		private ScriptExecutioner CreateExecutioner(
			string[] scripts,
			string[] usingStatements,
			IList<Assembly> referencedAssemblies
		) {
			usingStatements = usingStatements ?? new string[0];
			referencedAssemblies = referencedAssemblies ?? new List<Assembly>();

			ScriptExecutioner executioner = new ScriptExecutioner(
				new BaseMockLoader(scripts),
				new MockLogger()
			);

			var executor = (CSharpExecutor)executioner.ScriptExecutors
				.Where(x => x.GetType() == typeof(CSharpExecutor))
				.Single();

			executor.ReferencedAssemblies = referencedAssemblies;
			foreach (string statement in usingStatements) {
				executor.UsingStatements.Add(statement);
			}

			executioner.Add(executor);

			return executioner;
		}
	}

}
