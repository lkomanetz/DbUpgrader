using System;
using System.Collections.Generic;

namespace Executioner {
	namespace Contracts {

		public interface IExecutioner {

			IList<ScriptDocument> ScriptDocuments { get; }
			ExecutionResult Run(ExecutionRequest request = null);

		}

	}

}