using Executioner.Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.CodeAnalysis.Emit;
using System.Runtime.Loader;

namespace Executioner {

	public class CSharpExecutor : IScriptExecutor {

		private static string NAMESPACE_NAME = "CsExecutor";
		private static string CLASS_NAME = "ExecutorProgram";
		private static string MAIN_METHOD_NAME = "Execute";

		public CSharpExecutor() {
			this.UsingStatements = new List<string>();
			this.ReferencedAssemblies = new List<Assembly>();
		}

		public IList<string> UsingStatements { get; set; }
		public IList<Assembly> ReferencedAssemblies { get; set; }

		public bool Execute(string scriptText) {
			/*
			CSharpCodeProvider provider = new CSharpCodeProvider();
			CompilerParameters parameters = CreateParameters();
			 */

			string csSource = GenerateSourceString(scriptText);
			SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(csSource);

			MetadataReference[] references = GetReferences();
			CSharpCompilation compilation = CSharpCompilation.Create(
				typeof(CSharpExecutor).GetTypeInfo().Assembly.FullName,
				new[] { syntaxTree },
				references,
				new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
			);
			
			Assembly dynamicAssembly = Compile(compilation);
			Type program = dynamicAssembly.GetType($"{NAMESPACE_NAME}.{CLASS_NAME}");
			object obj = Activator.CreateInstance(program);
			MethodInfo method = program.GetTypeInfo().GetDeclaredMethod(MAIN_METHOD_NAME);
			// method.Invoke()
			/*
			CompilerResults results = provider.CompileAssemblyFromSource(parameters, csSource);
			CheckForErrors(results);

			Assembly assembly = results.CompiledAssembly;
			TypeInfo program = assembly.GetType(
				$"{NAMESPACE_NAME}.{CLASS_NAME}"
			)
			.GetTypeInfo();

			MethodInfo method = program.GetMethod(MAIN_METHOD_NAME);
			method.Invoke(null, null);
			*/
			return true;
		}

		/*
		private void CheckForErrors(CompilerResults results) {
			if (results.Errors.HasErrors) {
				StringBuilder sb = new StringBuilder();
				
				foreach (CompilerError error in results.Errors) {
					sb.AppendLine($"Error ({error.ErrorNumber}): {error.ErrorText}");
				}

				throw new InvalidOperationException(sb.ToString());
			}	
		}
		 */

		private MetadataReference[] GetReferences() {
			return new MetadataReference[] {
				MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
				MetadataReference.CreateFromFile(typeof(Enumerable).GetTypeInfo().Assembly.Location)
			};
		}

		private Assembly Compile(CSharpCompilation compilation) {
			using (MemoryStream ms = new MemoryStream()) {
				EmitResult result = compilation.Emit(ms);

				if (!result.Success) {
					IEnumerable<Diagnostic> failures = result.Diagnostics
						.Where(x => x.IsWarningAsError || x.Severity == DiagnosticSeverity.Error);
					
					string errorMsg = String.Empty;
					foreach (Diagnostic failure in failures) {
						errorMsg += $"{failure.Id}: {failure.GetMessage()}\n";
					}

					throw new Exception(errorMsg);
				}

				ms.Seek(0, SeekOrigin.Begin);
				return AssemblyLoadContext.Default.LoadFromStream(ms);
			}
		}

		private string GenerateSourceString(string scriptText) {
			SanitizeUsingStatements();

			StringBuilder sb = new StringBuilder();
			sb.AppendLine("using System;");

			foreach (string statement in this.UsingStatements) {
				sb.AppendLine($"using {statement};");
			}

			sb.AppendLine($"namespace {NAMESPACE_NAME} {{");
			sb.AppendLine($"public class {CLASS_NAME} {{");
			sb.AppendLine($"public static void {MAIN_METHOD_NAME}(){{");
			sb.AppendLine(scriptText);
			sb.AppendLine("}}}");
			return sb.ToString();
		}

		/*
		private CompilerParameters CreateParameters() {
			CompilerParameters parameters = new CompilerParameters() {
				GenerateInMemory = true,
				GenerateExecutable = false
			};
			foreach (Assembly assembly in this.ReferencedAssemblies) {
				parameters.ReferencedAssemblies.Add(assembly.Location);
			}
			return parameters;
		}
		 */

		private void SanitizeUsingStatements() {
			for (short i = 0; i < this.UsingStatements.Count; ++i) {
				this.UsingStatements[i] = this.UsingStatements[i].Replace(";", "");
				this.UsingStatements[i] = Regex.Replace(
					this.UsingStatements[i],
					@"(using\s*)",
					""
				);
			}
		}

	}

}