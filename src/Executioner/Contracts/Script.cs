using System;
using System.Xml.Serialization;

namespace Executioner {
	namespace Contracts {

		[XmlRoot]
		public class Script : IOrderedItem {
			private string _orderString;

			[XmlAttribute("Id")] public Guid SysId { get; set; }
			[XmlAttribute("Executor")] public string ExecutorName { get; set; }
			[XmlIgnore] public Guid DocumentId { get; set; }
			[XmlIgnore] public DateTime DateCreatedUtc { get; set; }
			[XmlIgnore] public int Order { get; set; }
			[XmlIgnore] public bool IsComplete { get; set; }
			[XmlText] public string ScriptText { get; set; }

			[XmlAttribute("Order")]
			public string OrderString {
				get { return _orderString; }
				set {
					_orderString = value;
					this.DateCreatedUtc = ScriptLoaderUtilities.ParseOrderXmlAttribute(_orderString).Item1;
					this.Order = ScriptLoaderUtilities.ParseOrderXmlAttribute(_orderString).Item2;
				}
			}

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
					scriptA.ExecutorName == scriptB.ExecutorName &&
					scriptA.DocumentId == scriptB.DocumentId &&
					scriptA.Order == scriptB.Order
				);
			}

		}

	}
}