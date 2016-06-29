using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbUpgrader.Contracts.Interfaces {

	public interface IScriptExecutor {

		void Execute(Script script);
		void Execute(Script[] scripts);

	}

}
