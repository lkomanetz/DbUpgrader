using System;
using System.Collections.Generic;
using System.Linq;
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

	}
}
