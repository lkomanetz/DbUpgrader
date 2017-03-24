using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Executioner {
	namespace Contracts {

		[XmlRoot]
		public class ScriptDocument : IOrderedItem {
			
			private string _orderString;

			public ScriptDocument() {
				this.Scripts = new List<Script>();
			}

			[XmlElement("Id", Order = 0)]
			public Guid SysId { get; set; }

			[XmlElement("Order", Order = 1)]
			public string OrderString
			{
				get { return _orderString; }
				set {
					_orderString = value;
					this.DateCreatedUtc = ScriptLoaderUtilities.ParseOrderXmlAttribute(_orderString).Item1;
					this.Order = ScriptLoaderUtilities.ParseOrderXmlAttribute(_orderString).Item2;
				}

			}

			[XmlArray("Scripts", Order = 2)]
			public List<Script> Scripts { get; set; }

			[XmlIgnore] public DateTime DateCreatedUtc { get; set; }
			[XmlIgnore] public int Order { get; set; }
			[XmlIgnore] public string ResourceName { get; set; }
			[XmlIgnore] public bool IsComplete { get; set; }

		}

	}

}