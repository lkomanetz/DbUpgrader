using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptLoader.Contracts;

namespace ScriptExecutor.Contracts {

	public interface IScriptExecutor {

		IList<Guid> CompletedDocumentIds { get; }

		void Execute();

	}

}
