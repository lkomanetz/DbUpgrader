using Executioner.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Executioner.Sorters {

	public delegate IOrderedEnumerable<T> Sorter<T>(IList<T> collection) where T : IOrderedItem;

	internal static class OrderedItemSorters {

		internal static Sorter<IOrderedItem> SortByDateThenOrder = (collection) => 
			collection
				.OrderBy((i) => i.DateCreatedUtc)
				.ThenBy(x => x.Order);

	}

}