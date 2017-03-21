using System;

namespace Executioner{
	namespace Contracts {

		public interface IOrderedItem {

			int Order { get; }
			DateTime DateCreatedUtc { get; }

		}

	}

}