using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;

namespace Executioner {

	public class AppDomain {

		public static AppDomain CurrentDomain { get; private set; }

		static AppDomain() {
			CurrentDomain = new AppDomain();
		}

		public Assembly[] GetAssemblies() {
			IList<Assembly> assemblies = new List<Assembly>();
			var dependencies = DependencyContext.Default.RuntimeLibraries;

			foreach (RuntimeLibrary library in dependencies) {
				try {
					var assembly = Assembly.Load(new AssemblyName(library.Name));
					assemblies.Add(assembly);
				}
				catch (FileNotFoundException) {
					// If an assembly isn't found it should be fine.
				}
			}

			return assemblies.ToArray();
		}

	}

}