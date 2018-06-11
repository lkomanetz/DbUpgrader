using Executioner.Contracts;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace Executioner {

	internal class ExecutorCreator {
		private static IDictionary<string, Type> _executorsCreated;

		static ExecutorCreator() => _executorsCreated = new Dictionary<string, Type>();	

		internal static IScriptExecutor Create(string executorName) {
			if (_executorsCreated.TryGetValue(executorName, out Type foundType)) {
				return (IScriptExecutor)Activator.CreateInstance(foundType);
			}

			Type objectType = GetClassType(executorName);
			if (objectType == null) {
				throw new Exception($"Unable to find C# type for executor '{executorName}'");
			}

			IScriptExecutor executor = (IScriptExecutor)Activator.CreateInstance(objectType);
			_executorsCreated.Add(executorName, objectType);
			return executor;
		}

		/*
		 * In order to get the type of the executor name passed in, I ended up having to go through all assemblies
		 * in the current application domain.  I wanted to keep the ScriptDocument simple without having to add more
		 * things to it in order to load the type through reflection.  This is more of a brute force method to find
		 * the type with just a name.
		 */
		private static Type GetClassType(string executorName) {
			Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
			Type objectType = null;

			foreach (Assembly assembly in assemblies) {
				Type[] executorTypes = FindExecutorsIn(assembly);

				objectType = executorTypes
					.Where(x => x.Name.Equals(executorName))
					.SingleOrDefault();
				
				if (objectType != null) break;
			}

			return objectType;

			Type[] FindExecutorsIn(Assembly assembly) {
				string interfaceName = typeof(IScriptExecutor).Name;
				return assembly.GetTypes()
					.Where(t => t.GetInterfaces().Any(i => i.Name.Equals(interfaceName)))
					.ToArray<Type>();
			}
		}

	}

}