using Executioner.Contracts;
using Executioner.Contracts.ScriptExecutor;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Executioner {

	public class CSharpExecutor : IScriptExecutor {

		public void Execute(string scriptText) {
			CSharpCodeProvider provider = new CSharpCodeProvider();
			CompilerParameters parameters = new CompilerParameters() {
				GenerateInMemory = true,
				GenerateExecutable = false
			};

			string csSource = GenerateSourceString(scriptText);
			CompilerResults results = provider.CompileAssemblyFromSource(parameters, csSource);
			CheckForErrors(results);

			Assembly assembly = results.CompiledAssembly;
			Type program = assembly.GetType(
				$"{ScriptExecutorConstants.CS_NAMESPACE_NAME}.{ScriptExecutorConstants.CS_CLASS_NAME}"
			);

			MethodInfo method = program.GetMethod(ScriptExecutorConstants.CS_MAIN_METHOD);
			method.Invoke(null, null);
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
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("using System;");
			sb.AppendLine($"namespace {ScriptExecutorConstants.CS_NAMESPACE_NAME} {{");
			sb.AppendLine($"public class {ScriptExecutorConstants.CS_CLASS_NAME} {{");
			sb.AppendLine($"public static void {ScriptExecutorConstants.CS_MAIN_METHOD}(){{");
			sb.AppendLine(scriptText);
			sb.AppendLine("}}}");

			return sb.ToString();
		}
	}

}
