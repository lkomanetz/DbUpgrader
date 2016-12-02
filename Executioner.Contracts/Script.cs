using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Executioner.Contracts {

	public class Script : IOrderedItem {

		public Guid SysId { get; set; }
		public Guid DocumentId { get; set; }
		public DateTime DateCreatedUtc { get; set; }
		public int Order { get; set; }
		public string ScriptText { get; set; }
		public string AssemblyName { get; set; }
		public bool IsComplete { get; set; }

		public override int GetHashCode() {
			return this.SysId.GetHashCode();
		}

		public override bool Equals(object obj) {
			var otherScript = obj as Script;
			if (otherScript == null) {
				return false;
			}

			return IsEqual(this, otherScript);
		}

		public static bool operator ==(Script scriptA, Script scriptB) {
			if (Object.ReferenceEquals(scriptA, scriptB)) {
				return true;
			}

			if ((object)scriptA == null || (object)scriptB == null) {
				return false;
			}


			return IsEqual(scriptA, scriptB);
		}

		public static bool operator !=(Script scriptA, Script scriptB) {
			return !(scriptA == scriptB);
		}

		private static bool IsEqual(Script scriptA, Script scriptB) {
			return (
				scriptA.SysId == scriptB.SysId &&
				scriptA.IsComplete == scriptB.IsComplete &&
				scriptA.DateCreatedUtc == scriptB.DateCreatedUtc &&
				scriptA.AssemblyName == scriptB.AssemblyName &&
				scriptA.DocumentId == scriptB.DocumentId &&
				scriptA.Order == scriptB.Order
			);
		}

	}

}
