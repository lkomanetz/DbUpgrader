using Executioner.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Executioner.ExtensionMethods
{
	public static class IListExtensions {

		public static int FindIndex<T>(this IList<T> source, Func<T, bool> match) {
			for (int i = 0; i < source.Count; ++i) {
				if (match(source[i])) {
					return i;
				}
			}

			return -1;
		}

		public static void ForEach<T>(this IList<T> source, Action<T> action) {
			for (int i = 0; i < source.Count; ++i) {
				action(source[i]);
			}
		}

		public static IList<T> SortOrderedItems<T>(this IList<T> source) where T : IOrderedItem {
			return source.OrderBy(x => x.DateCreatedUtc)
					.ThenBy(x => x.Order)
					.ToList();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool AreComplete(this IList<Script> scripts) {
			return scripts.All(x => x.IsComplete);
		}

	}

}
