using Executioner.ExtensionMethods;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Executioner.ExtensionMethods.Tests {

	[TestClass]
	public class IListTests {

		[TestMethod]
		public void IList_FindIndexSucceeds() {
			IList<TestClass> testList = CreateList();

			Guid testGuid = Guid.NewGuid();
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

		[TestMethod]
		public void IList_ForEachMethodSucceeds() {
			IList<TestClass> testList = CreateList();
			testList.ForEach(x => {
				x.Boolean = true;
			});

			Assert.IsFalse(testList.Any(x => !x.Boolean), "ForEach did not change all false booleans to true.");

			testList.ForEach(x => x.Boolean = false);
			Assert.IsFalse(testList.Any(x => x.Boolean), "ForEach did not change all true booleans to false.");
		}

		[TestMethod]
		public void IList_SortOnOrderSucceeds() {
			DateTime now = DateTime.UtcNow;

			IList<OrderedItem> items = new List<OrderedItem>() {
				new OrderedItem() { Id = 0, DateCreatedUtc = now, Order = 1 },
				new OrderedItem() { Id = 1, DateCreatedUtc = now, Order = 3 },
				new OrderedItem() { Id = 2, DateCreatedUtc = now, Order = 0 },
				new OrderedItem() { Id = 3, DateCreatedUtc = now, Order = 2 }
			};
			IList<OrderedItem> sortedList = items.SortOrderedItems();

			AssertOrder(
				new int[] { 2, 0, 3, 1 },
				sortedList.Select(x => x.Id).ToArray()
			);
		}

		[TestMethod]
		public void IList_SortOnDateCreatedUtcSucceeds() {
			DateTime now = DateTime.UtcNow;
			IList<OrderedItem> items = new List<OrderedItem>() {
				new OrderedItem() { Id = 0, DateCreatedUtc = now },
				new OrderedItem() { Id = 1, DateCreatedUtc = now.AddDays(-1) },
				new OrderedItem() { Id = 2, DateCreatedUtc = now.AddDays(1) }
			};

			IList<OrderedItem> sortedList = items.SortOrderedItems();
			AssertOrder(
				new int[] { 1, 0, 2 },
				sortedList.Select(x => x.Id).ToArray()
			);
		}

		[TestMethod]
		public void IList_SortOnDateAndOrderSucceeds() {
			DateTime now = DateTime.UtcNow;
			IList<OrderedItem> items = new List<OrderedItem>() {
				new OrderedItem() { Id = 0, DateCreatedUtc = now, Order = 0 },
				new OrderedItem() { Id = 1, DateCreatedUtc = now, Order = 1 },
				new OrderedItem() { Id = 2, DateCreatedUtc = now.AddSeconds(-1), Order = 1 },
				new OrderedItem() { Id = 3, DateCreatedUtc = now.AddSeconds(-1), Order = 0 },
				new OrderedItem() { Id = 4, DateCreatedUtc = now.AddDays(1), Order = 0 }
			};

			IList<OrderedItem> sortedList = items.SortOrderedItems();
			AssertOrder(
				new int[] { 3, 2, 0, 1, 4 },
				sortedList.Select(x => x.Id).ToArray()
			);
		}

		[TestMethod]
		[ExpectedException(typeof(Exception), "Sorting did not throw expected exception.")]
		public void IList_SortFailsWithDuplicateOrder() {
			DateTime now = DateTime.UtcNow;
			IList<OrderedItem> items = new List<OrderedItem>() {
				new OrderedItem() { Id = 0, DateCreatedUtc = now, Order = 0 },
				new OrderedItem() { Id = 1, DateCreatedUtc = now, Order = 0 },
				new OrderedItem() { Id = 2, DateCreatedUtc = now, Order = 1 }
			};

			IList<OrderedItem> sortedList = items.SortOrderedItems();
		}

		private void AssertOrder(int[] expectedOrder, int[] actualOrder) {
			Assert.IsTrue(
				expectedOrder.Length == actualOrder.Length,
				$"Actual order length {actualOrder.Length} != Expected order lenght {expectedOrder.Length}."
			);

			for (int i = 0; i < expectedOrder.Length; ++i) {
				Assert.IsTrue(actualOrder[i] == expectedOrder[i], $"{actualOrder[i]} != {expectedOrder[i]}.");
			}
		}

		private IList<TestClass> CreateList() {
			IList<TestClass> testList = new List<TestClass>();

			for (int i = 0; i <= 10; ++i) {
				var item = new TestClass() {
					Number = i,
					FloatNumber = (float)i,
					Text = $"Text{i}",
					Id = Guid.NewGuid()
				};
				testList.Add(item);
			}

			return testList;
		}
	}

}
