using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using System.Collections.Generic;

namespace Executioner.ExtensionMethods.Tests {

	[TestClass]
	public class IListTests {

		[TestMethod]
		public void IList_FindIndexSucceeds() {
			IList<TestClass> testList = new List<TestClass>();
			Guid testGuid = Guid.NewGuid();

			for (int i = 0; i <= 10; ++i) {
				var item = new TestClass() {
					Number = i,
					FloatNumber = (float)i,
					Text = $"Text{i}",
					Id = Guid.NewGuid()
				};
				testList.Add(item);
			}
			testList[5].Id = testGuid;

			int itemIndex = testList.FindIndex(x => x.Number == 5);
			Assert.IsTrue(itemIndex == 5, $"Expected {5}\nActual {itemIndex}");

			itemIndex = testList.FindIndex(x => x.Text.Equals("Text4"));
			Assert.IsTrue(itemIndex == 4, $"Expected {4}\nActual {itemIndex}");

			itemIndex = testList.FindIndex(x => x.FloatNumber == 6.0);
			Assert.IsTrue(itemIndex == 6, $"Expected {6}\nActual {itemIndex}");

			itemIndex = testList.FindIndex(x => x.Id == testGuid);
			Assert.IsTrue(itemIndex == 5, $"Expected {5}\nActual {itemIndex}");
		}

	}

}
