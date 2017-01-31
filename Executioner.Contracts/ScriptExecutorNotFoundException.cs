using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Executioner.Contracts {
	
	public class ScriptExecutorNotFoundException : Exception {

		public ScriptExecutorNotFoundException() :
			base()
		{ }

		public ScriptExecutorNotFoundException(string executorName) :
			base($"'{executorName}' not found.")
		{ }

		public ScriptExecutorNotFoundException(SerializationInfo info, StreamingContext context) :
			base(info, context)
		{ }

	}

}
