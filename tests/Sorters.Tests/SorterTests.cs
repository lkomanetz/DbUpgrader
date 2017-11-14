using Executioner.Contracts;
using Executioner.Sorters;
using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;

namespace Sorters.Tests {

    public class SorterTests {
        private DateTime _now;

        public SorterTests() => _now = DateTime.UtcNow;

        [Fact]
        public void UnorderedListIsOrderedByOrderProperty() {
            IList<SimpleOrderedItem> items = new List<SimpleOrderedItem>() {
                new SimpleOrderedItem('a', 1, _now.AddDays(1)),
                new SimpleOrderedItem('b', 0, _now),
                new SimpleOrderedItem('c', 3, _now.AddDays(3)),
                new SimpleOrderedItem('d', 2, _now.AddDays(2))
            };

            Sorter<SimpleOrderedItem> sorter = (collection) => collection.OrderBy(item => item.Order);

            var orderedItems = new MockLoader(items, sorter).SortedItems;
            AssertOrder("badc", orderedItems);
        }

        [Fact]
        public void UnorderedListIsOrderedByDateThenOrderProperty() {
            IList<SimpleOrderedItem> items = new List<SimpleOrderedItem>() {
                new SimpleOrderedItem('a', 1, _now),
                new SimpleOrderedItem('b', 0, _now),
                new SimpleOrderedItem('c', 0, _now.AddMonths(1)),
                new SimpleOrderedItem('d', 0, _now.AddDays(2))
            };

            Sorter<SimpleOrderedItem> sorter = (collection) =>
                collection.OrderBy(item => item.DateCreatedUtc).ThenBy(item => item.Order);
            
            var orderedItems = new MockLoader(items, sorter).SortedItems;
            AssertOrder("badc", orderedItems);
        }

        private void AssertOrder(string expectedOrder, IOrderedEnumerable<SimpleOrderedItem> collection) {
            char[] orderArray = expectedOrder.ToArray();
            Assert.True(
                orderArray.Length == collection.Count(),
                "Count mismatch between expectedOrder and collection"
            );

            for (short i = 0; i < orderArray.Length; ++i) {
                SimpleOrderedItem item = collection.ElementAt(i);
                Assert.True(item.Id == orderArray[i], $"Expected Id '{orderArray[i]}' but was '{item.Id}'.");
            }
        }
    }

    internal class MockLoader {

        private IOrderedEnumerable<SimpleOrderedItem> _sortedItems;

        public MockLoader(IList<SimpleOrderedItem> items, Sorter<SimpleOrderedItem> sorter) {
            _sortedItems = sorter.Invoke(items);
        }

        public IOrderedEnumerable<SimpleOrderedItem> SortedItems => _sortedItems;

    }

    internal class SimpleOrderedItem : IOrderedItem {

        public SimpleOrderedItem(char id, int order, DateTime createdOnUtc) {
            this.Id = id;
            this.Order = order;
            this.DateCreatedUtc = createdOnUtc;
        }

        public char Id { get; }
        public int Order { get; }
        public DateTime DateCreatedUtc { get; }

    }

}