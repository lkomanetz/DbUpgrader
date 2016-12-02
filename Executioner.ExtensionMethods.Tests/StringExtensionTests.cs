using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Executioner.ExtensionMethods.Tests {

	[TestClass]
	public class StringExtensionTests {

		[TestMethod]
		public void StringExtension_OrderAttributeParseSucceeds() {
			string orderStr = "2016-12-01";
			Tuple<DateTime, int> orderValues = orderStr.ParseOrderXmlAttribute();
			Assert.IsTrue(orderValues.Item1 == new DateTime(2016, 12, 1), "ParseOrderXmlAttribute failed to parse date.");
			Assert.IsTrue(orderValues.Item2 == 0, "ParseOrderXmlAttribute failed to parse order.");

			orderStr += ":1";
			orderValues = orderStr.ParseOrderXmlAttribute();
			Assert.IsTrue(orderValues.Item2 == 1, "ParseOrderXmlAttribute failed to parse order.");
		}

		[TestMethod]
		[ExpectedException(typeof(FormatException), "FormatException was not thrown on incorrect XML Order attribute")]
		public void StringExtension_OrderAttributeParseDateTimeThrowsException() {
			string orderStr = "hello";
			orderStr.ParseOrderXmlAttribute();
		}

		[TestMethod]
		[ExpectedException(typeof(FormatException), "FormatException was not thrown on incorrect XML Order attribute")]
		public void StringExtension_OrderAttributeParseOrderThrowsException() {
			string orderStr = "2016-12-01:f";
			orderStr.ParseOrderXmlAttribute();
		}

	}

}
