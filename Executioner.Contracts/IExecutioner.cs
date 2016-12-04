using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptExecutor.Contracts;

namespace Executioner.Contracts {
	public interface IExecutioner {

		IList<ScriptDocument> ScriptDocuments { get; }
		ExecutionResult Run(ExecutionRequest request = null);
		void Add(IScriptExecutor executor);
		void Add(IScriptExecutor[] executors);

	}
}
