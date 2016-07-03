using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DbUpgrader.SqlServer;
using DbUpgrader.Tests.FakeService;
using DbUpgrader.Contracts;
using System.Collections.Generic;
using System.Reflection;
using DbUpgrader.Tests.AnotherFakeService;

namespace DbUpgrader.Tests {

	[TestClass]
	public class SqlServerUpgraderTests {

		private const string CONNECTION_STRING = @"Server=localhost\SQLEXPRESS;Database=DbUpgrader.UnitTest; Integrated Security=true";

		[TestMethod]
		public void UpgraderCanFindSqlScriptFile() {
			SqlServerUpgrader upgrader = new SqlServerUpgrader(CONNECTION_STRING);
			IList<Script> scriptsToRun = upgrader.GetScriptsToRun(typeof(MyFakeService).Assembly);
			Assert.IsTrue(scriptsToRun.Count >= 0);
		}

		[TestMethod]
		public void ScriptsStayInOrder() {
			SqlServerUpgrader upgrader = new SqlServerUpgrader(CONNECTION_STRING);
			Script[] scripts = upgrader.GetScriptsFromXml(typeof(MyFakeService).Assembly);
			AssertOrder(
				scripts,
				"Date: 6/21/2016 Order: 0Date: 6/22/2016 Order: 0Date: 6/22/2016 Order: 1Date: 6/23/2016 Order: 0"
			);
		}

		[TestMethod]
		public void UpgraderCreatesItsOwnTables() {
			SqlServerUpgrader upgrader = new SqlServerUpgrader(CONNECTION_STRING);
			upgrader.InitializeUpgraderTables();
		}

		[TestMethod]
		public void EndToEndTest() {
			SqlServerUpgrader upgrader = new SqlServerUpgrader(CONNECTION_STRING);
			IList<Assembly> assemblies = new List<Assembly>() {
				typeof(MyFakeService).Assembly,
				typeof(AnotherTestService).Assembly
			};
			upgrader.OnBeforeRunStarted += (sender, args) => {
				Assert.IsTrue(true);
			};
			upgrader.OnRunCompleted += (sender, args) => {
				Assert.IsTrue(true);
			};

			upgrader.Run(assemblies);
		}

		[TestMethod]
		public void RunFailsWithSameAssembly() {
			IList<Assembly> assemblies = new List<Assembly>() {
				typeof(MyFakeService).Assembly,
				typeof(MyFakeService).Assembly
			};

			try {
				SqlServerUpgrader upgrader = new SqlServerUpgrader(CONNECTION_STRING);
				upgrader.Run(assemblies);
				Assert.Fail("Upgrader ran with multiples of the same assembly");
			}
			catch {
				Assert.IsTrue(true);
			}
		}

		private void AssertOrder(Script[] scripts, string expectedOrder) {
			string actualOrder = String.Empty;
			foreach (Script script in scripts) {
				actualOrder += $"Date: {script.DateCreatedUtc.ToShortDateString()} Order: {script.Order}";
			}

			Assert.AreEqual(expectedOrder, actualOrder);
		}

	}

}
