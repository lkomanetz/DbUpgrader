using Executioner.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Executioner.ExtensionMethods.Tests {

	public class OrderedItem : IOrderedItem {

		public int Id { get; set; }
		public DateTime DateCreatedUtc { get; set; }
		public int Order { get; set; }

	}

}
