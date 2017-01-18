using Executioner.Contracts;
using Executioner.Contracts.ScriptExecutor;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Executioner {

	public class CSharpExecutor : IScriptExecutor {

		public CSharpExecutor() {
			this.UsingStatements = new List<string>();
			this.ReferencedAssemblies = new List<Assembly>();
		}

		public IList<string> UsingStatements { get; set; }
		public IList<Assembly> ReferencedAssemblies { get; set; }

		public bool Execute(string scriptText) {
			CSharpCodeProvider provider = new CSharpCodeProvider();
			CompilerParameters parameters = CreateParameters();

			string csSource = GenerateSourceString(scriptText);
			
			CompilerResults results = provider.CompileAssemblyFromSource(parameters, csSource);
			CheckForErrors(results);

			Assembly assembly = results.CompiledAssembly;
			Type program = assembly.GetType(
				$"{ScriptExecutorConstants.CS_NAMESPACE_NAME}.{ScriptExecutorConstants.CS_CLASS_NAME}"
			);

			MethodInfo method = program.GetMethod(ScriptExecutorConstants.CS_MAIN_METHOD);
			method.Invoke(null, null);
			return true;
		}

		private void CheckForErrors(CompilerResults results) {
			if (results.Errors.HasErrors) {
				StringBuilder sb = new StringBuilder();
				
				foreach (CompilerError error in results.Errors) {
					sb.AppendLine($"Error ({error.ErrorNumber}): {error.ErrorText}");
				}

				throw new InvalidOperationException(sb.ToString());
			}	
		}

		private string GenerateSourceString(string scriptText) {
			SanitizeUsingStatements();

			StringBuilder sb = new StringBuilder();
			sb.AppendLine("using System;");

			foreach (string statement in this.UsingStatements) {
				sb.AppendLine($"using {statement};");
			}

			sb.AppendLine($"namespace {ScriptExecutorConstants.CS_NAMESPACE_NAME} {{");
			sb.AppendLine($"public class {ScriptExecutorConstants.CS_CLASS_NAME} {{");
			sb.AppendLine($"public static void {ScriptExecutorConstants.CS_MAIN_METHOD}(){{");
			sb.AppendLine(scriptText);
			sb.AppendLine("}}}");

			return sb.ToString();
		}

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
