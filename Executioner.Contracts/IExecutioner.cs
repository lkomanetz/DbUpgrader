using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Executioner.Contracts {

	public interface IExecutioner {

		IList<ScriptDocument> ScriptDocuments { get; }
		ExecutionResult Run(ExecutionRequest request = null);

	}

}