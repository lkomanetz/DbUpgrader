using Executioner.Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
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
			string csSource = GenerateSourceString(scriptText);
			SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(csSource);

			CSharpCompilation compilation = CreateCompiler(syntaxTree);
			Assembly dynamicAssembly = Compile(compilation);
			Type program = dynamicAssembly.GetType($"{NAMESPACE_NAME}.{CLASS_NAME}");
			object obj = Activator.CreateInstance(program);
			MethodInfo method = program.GetTypeInfo().GetDeclaredMethod(MAIN_METHOD_NAME);
			method.Invoke(null, null);
			return true;
		}

		private MetadataReference[] GetReferences() {
			string enumerableAssemblyLoc = typeof(Enumerable).GetTypeInfo().Assembly.Location;
			DirectoryInfo coreDir = Directory.GetParent(enumerableAssemblyLoc);
			string coreLoc = coreDir.FullName + Path.DirectorySeparatorChar + "mscorlib.dll";

			IList<MetadataReference> references = new List<MetadataReference>();

			var currentExecutingAssemblies = Assembly.GetEntryAssembly().GetReferencedAssemblies();
			foreach (var currentExecutingAssembly in currentExecutingAssemblies) {
				// An assembly could already be loaded.  Instead of going bang I want to just keep going.
				try {
					Assembly loadedAssembly = Assembly.Load(currentExecutingAssembly);
					references.Add(MetadataReference.CreateFromFile(loadedAssembly.Location));
				}
				catch (FileLoadException) {}
			}

			for (short i = 0; i < this.ReferencedAssemblies.Count; ++i) {
				var reference = MetadataReference.CreateFromFile(this.ReferencedAssemblies[i].Location);
				references.Add(reference);
			}
			references.Add(MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location));
			references.Add(MetadataReference.CreateFromFile(coreLoc));

			return references.ToArray();
		}

		private CSharpCompilation CreateCompiler(SyntaxTree syntaxTree) {
			MetadataReference[] references = GetReferences();
			CSharpCompilation compiler = CSharpCompilation.Create(
				typeof(CSharpExecutor).GetTypeInfo().Assembly.FullName,
				new[] { syntaxTree },
				references,
				new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
			);

			return compiler;
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

					throw new InvalidOperationException(errorMsg);
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