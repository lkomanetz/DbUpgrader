using Executioner;
using Executioner.Tests.Classes;
using System;
using ScriptExecutor.CSharp.Tests.Classes;
using ScriptExecutor.CSharp.Tests.TestClasses;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using System.Threading;

namespace ScriptExecutor.CSharp.Tests {

	public class CSharpExecutorTests {

		[Fact]
		public void CSharp_NoExceptionsThrown() {
			Exception ex = Record.Exception(() => {
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
			});
			Assert.Null(ex);
		}

		[Fact]
		public void CSharp_CompilerErrorsFound() {
			Exception ex = Record.Exception(() => {
				//Console is spelled wrong.
				string code = @"Consoe.WriteLine(""Hello world!"")";

				string[] scripts = new string[] {
					$"<Script Id='{Guid.NewGuid()}' Executor='CSharpExecutor' Order='2016-12-28'>{code}</Script>"
				};

				ScriptExecutioner executioner = CreateExecutioner(
					scripts: scripts,
					usingStatements: null,
					referencedAssemblies: null
				);

				executioner.Run();
			});
			Assert.NotNull(ex);
			Assert.True(ex is InvalidOperationException, $"Exception is {ex.GetType().Name}");
		}

		[Fact]
		public void CSharp_CallToStaticMethodInSameAssembly_Succeeds() {
			string code = @"SameAssemblyClass.StaticMethod();";
			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='CSharpExecutor' Order='2016-12-29'>{code}</Script>"
			};

			string[] usingStatements = new string[] {
				"using ScriptExecutor.CSharp.Tests.Classes"
			};

			var referencedAssemblies = new List<Assembly>() {
				typeof(SameAssemblyClass).GetTypeInfo().Assembly
			};

			var executioner = CreateExecutioner(scripts, usingStatements, referencedAssemblies);
			executioner.Run();
		}

		[Fact]
		public void CSharp_CallToStaticMethodInDifferentAssembly_Succeeds() {
			string code = @"TestClass.StaticMethod();";
			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='CSharpExecutor' Order='2016-12-29'>{code}</Script>"
			};

			string[] usingStatements = new string[] {
				"using ScriptExecutor.CSharp.Tests.TestClasses;"
			};

			var referencedAssemblies = new List<Assembly>() {
				typeof(TestClass).GetTypeInfo().Assembly
			};

			var executioner = CreateExecutioner(scripts, usingStatements, referencedAssemblies);
			executioner.Run();
		}

		[Fact]
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
				typeof(TestClass).GetTypeInfo().Assembly,
				typeof(SameAssemblyClass).GetTypeInfo().Assembly,
			};

			var executioner = CreateExecutioner(scripts, usingStatements, referencedAssemblies);
			executioner.Run();
		}

		[Fact]
		public void CSharp_CallToStaticMethodWithoutUsingStatements_Fails() {
			Exception ex = Record.Exception(() => {
				string code = @"TestClass.StaticMethod();";
				string[] scripts = new string[] {
					$"<Script Id='{Guid.NewGuid()}' Executor='CSharpExecutor' Order='2016-12-29'>{code}</Script>"
				};

				var referencedAssemblies = new List<Assembly>() {
					typeof(TestClass).GetTypeInfo().Assembly
				};

				var executioner = CreateExecutioner(scripts, null, referencedAssemblies);
				executioner.Run();
			});
			Assert.NotNull(ex);
			Assert.True(ex is InvalidOperationException, $"Exception is {ex.GetType().Name}.");
		}

		[Fact]
		public void CSharp_CallToStaticMethodWithoutAssemblies_Fails() {
			Exception ex = Record.Exception(() => {
				string code = @"TestClass.StaticMethod();";
				string[] scripts = new string[] {
					$"<Script Id='{Guid.NewGuid()}' Executor='CSharpExecutor' Order='2016-12-29'>{code}</Script>"
				};
				string[] usingStatements = new string[] {
					"ScriptExecutor.CSharp.Tests.TestClasses"
				};

				var executioner = CreateExecutioner(scripts, usingStatements, null);
				executioner.Run();
			});
			Assert.NotNull(ex);
			Assert.True(ex is InvalidOperationException, $"Exception is {ex.GetType().Name}.");
		}

		[Fact]
		public void CSharp_MissingUsingAndReferencedAssemblies_Fails() {
			Exception ex = Record.Exception(() => {
				string code = @"TestClass.StaticMethod();";
				string[] scripts = new string[] {
					$"<Script Id='{Guid.NewGuid()}' Executor='CSharpExecutor' Order='2016-12-29'>{code}</Script>"
				};

				var executioner = CreateExecutioner(scripts, null, null);
				executioner.Run();
			});
			Assert.NotNull(ex);
			Assert.True(ex is InvalidOperationException, $"Exception is {ex.GetType().Name}.");
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
				new MockDataStore()
			);

			var executor = (CSharpExecutor)executioner.ScriptExecutors
				.Where(x => x.GetType() == typeof(CSharpExecutor))
				.Single();

			executor.ReferencedAssemblies = referencedAssemblies;
			foreach (string statement in usingStatements) {
				executor.UsingStatements.Add(statement);
			}

			return executioner;
		}

	}

}