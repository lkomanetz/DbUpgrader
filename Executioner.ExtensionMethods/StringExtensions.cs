using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Executioner.ExtensionMethods {

	public static class StringExtensions {

		public const char SEPARATOR = ':';

		public static Tuple<DateTime, int> ParseOrderXmlAttribute(this string value) {
			string[] values = value.Split(SEPARATOR);

			DateTime date = default(DateTime);
			if (!DateTime.TryParse(values[0], out date)) {
				throw new FormatException("Invalid date value found in XML 'Order' attribute.");
			}

			int order = 0;
			if (values.Length == 2) {
				if (!Int32.TryParse(values[1], out order)) {
					throw new FormatException("Non-numeric value found in XML 'Order' attribute.");
				}
			}

			return Tuple.Create<DateTime, int>(date, order);
		}

	}

}
