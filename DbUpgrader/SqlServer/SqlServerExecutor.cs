using DbUpgrader.Contracts;
using DbUpgrader.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbUpgrader.SqlServer {

	public class SqlServerExecutor : IScriptExecutor {
		public void Execute(Script script) {

		}

		public void Execute(Script[] scripts) {
			for (short i = 0; i < scripts.Length; i++) {
				Execute(scripts[i]);
			}
		}

	}

}
