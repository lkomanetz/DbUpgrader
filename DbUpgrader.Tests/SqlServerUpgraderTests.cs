using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DbUpgrader.SqlServer;
using DbUpgrader.Tests.FakeService;
using DbUpgrader.Contracts;

namespace DbUpgrader.Tests {

	[TestClass]
	public class SqlServerUpgraderTests {

		private const string CONNECTION_STRING = "Test_Connection";

		[TestMethod]
		public void UpgraderCanFindSqlScriptFile() {
			SqlServerUpgrader upgrader = new SqlServerUpgrader(CONNECTION_STRING);
			upgrader.Run(typeof(MyFakeService).Assembly);
		}

		[TestMethod]
		public void ScriptsStayInOrder() {
			SqlServerUpgrader upgrader = new SqlServerUpgrader(CONNECTION_STRING);
			Script[] scripts = upgrader.GetScriptsFromXml(typeof(MyFakeService).Assembly);
			AssertOrder(
				scripts,
				"Date: 6/22/2016 Order: 0Date: 6/22/2016 Order: 1Date: 6/23/2016 Order: 0"
			);
		}

		private void AssertOrder(Script[] scripts, string expectedOrder) {
			string actualOrder = String.Empty;
			foreach (Script script in scripts) {
				actualOrder += $"Date: {script.DateCreated.ToShortDateString()} Order: {script.Order}";
			}

			Assert.AreEqual(expectedOrder, actualOrder);
		}

	}

}
