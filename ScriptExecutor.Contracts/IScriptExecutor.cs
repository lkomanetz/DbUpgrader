using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptExecutor.Contracts {

	public interface IScriptExecutor {

		void Execute(string scriptText);

	}

}
