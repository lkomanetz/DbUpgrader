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
using Microsoft.CodeAnalysis.Scripting;

namespace Executioner {

	public class CSharpExecutor : IScriptExecutor {

		public CSharpExecutor() {
			this.UsingStatements = new List<string>();
			this.ReferencedAssemblies = new List<Assembly>();
		}

		public IList<string> UsingStatements { get; set; }
		public IList<Assembly> ReferencedAssemblies { get; set; }

		public bool Execute(string scriptText) {
			SanitizeUsingStatements();
			MetadataReference[] references = GetReferences();
			var csScript = CSharpScript.Create(
				scriptText,
				ScriptOptions.Default.AddReferences(references).AddImports(this.UsingStatements)
			);
			Compile(csScript);
			csScript.RunAsync().Wait();

			return true;
		}

		private void Compile(Script<object> csScript) {
			Compilation compilation = csScript.GetCompilation();
			EmitResult result = null;
			using (var ms = new MemoryStream()) {
				result = compilation.Emit(ms);
				if (!result.Success) {
					IEnumerable<Diagnostic> failures = result.Diagnostics
						.Where(x => x.IsWarningAsError || x.Severity == DiagnosticSeverity.Error);
					
					string errorMsg = String.Empty;
					foreach (Diagnostic failure in failures) {
						errorMsg += $"{failure.Id}: {failure.GetMessage()}\n";
					}

					throw new InvalidOperationException(errorMsg);
				}
			}
		}

		private MetadataReference[] GetReferences() {
			var dd = typeof(Enumerable).GetTypeInfo().Assembly.Location;
			var coreDir = Directory.GetParent(dd);

			IList<MetadataReference> references = new List<MetadataReference>();
			for (short i = 0; i < this.ReferencedAssemblies.Count; ++i) {
				var reference = MetadataReference.CreateFromFile(this.ReferencedAssemblies[i].Location);
				references.Add(reference);
			}

			string mscorLibLoc = coreDir.FullName + Path.DirectorySeparatorChar + "mscorlib.dll";
			string sysRuntimeLoc = coreDir.FullName + Path.DirectorySeparatorChar + "System.Runtime.dll";

			references.Add(MetadataReference.CreateFromFile(mscorLibLoc));
			references.Add(MetadataReference.CreateFromFile(sysRuntimeLoc));
			references.Add(MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location));

			return references.ToArray();
		}

		private void SanitizeUsingStatements() {
			this.UsingStatements.Add("using System;");

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