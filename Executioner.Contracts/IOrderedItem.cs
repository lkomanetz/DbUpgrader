using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Executioner.Contracts {

	public interface IOrderedItem {

		int Order { get; }
		DateTime DateCreatedUtc { get; }

	}

}
