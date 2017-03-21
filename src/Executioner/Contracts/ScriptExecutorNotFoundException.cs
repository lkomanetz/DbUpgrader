using System;

namespace Executioner {
	namespace Contracts {

		public class ScriptExecutorNotFoundException : Exception {

			public ScriptExecutorNotFoundException() :
				base()
			{ }

			public ScriptExecutorNotFoundException(string executorName) :
				base($"'{executorName}' not found.")
			{ }

		}

	}

}