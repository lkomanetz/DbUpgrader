using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptExecutor.Contracts {

	public class ExecutionResult {

		public int ScriptDocumentsCompleted { get; set; }
		public int ScriptsCompleted { get; set; }

		public override int GetHashCode() {
			return ScriptDocumentsCompleted ^ ScriptsCompleted;
		}

		public override bool Equals(object obj)
		{
			ExecutionResult otherResult = obj as ExecutionResult;
			if (otherResult == null) {
				return false;
			}

			return otherResult == this;
		}

		public static bool operator ==(ExecutionResult resultA, ExecutionResult resultB) {
			if (Object.ReferenceEquals(resultA, resultB)) {
				return true;
			}

			if ((object)resultA == null || (object)resultB == null) {
				return false;
			}

			return (
				resultA.ScriptDocumentsCompleted == resultB.ScriptDocumentsCompleted &&
				resultA.ScriptsCompleted == resultB.ScriptsCompleted
			);
		}

		public static bool operator !=(ExecutionResult resultA, ExecutionResult resultB) {
			return !(resultA == resultB);
		}
	}

}
