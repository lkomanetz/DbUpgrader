using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DbUpgrader.SqlServer;
using DbUpgrader.Tests.FakeService;
using DbUpgrader.Contracts;
using DbUpgrader.Contracts.Interfaces;
using System.Collections.Generic;
using System.Reflection;
using DbUpgrader.Tests.AnotherFakeService;

namespace DbUpgrader.Tests {

	[TestClass]
	public class SqlServerUpgraderTests {

		private const string CONNECTION_STRING = @"Server=localhost\SQLEXPRESS;Database=DbUpgrader.UnitTest; Integrated Security=true";
		private static SqlServerUpgrader _upgrader;
		private static IDbCleaner _cleaner;

		[ClassInitialize]
		public static void Initialize(TestContext context) {
			_cleaner = new TestSqlServerCleaner(CONNECTION_STRING);
			_upgrader = new SqlServerUpgrader(CONNECTION_STRING);
			_cleaner.Clean();
		}

		[TestMethod]
		public void UpgraderCanFindSqlScriptFile() {
			IList<Script> scriptsToRun = _upgrader.GetScriptsToRun(typeof(MyFakeService).Assembly);
			Assert.IsTrue(scriptsToRun.Count >= 0);
		}

		[TestMethod]
		public void ScriptsStayInOrder() {
			Script[] scripts = _upgrader.GetScriptsFromXml(typeof(MyFakeService).Assembly);
			AssertOrder(
				scripts,
				"Date: 6/21/2016 Order: 0Date: 6/22/2016 Order: 0Date: 6/22/2016 Order: 1Date: 6/23/2016 Order: 0"
			);
		}

		[TestMethod]
		public void UpgraderCreatesItsOwnTables() {
			_upgrader.InitializeUpgraderTables();
		}

		[TestMethod]
		public void EndToEndTest() {
			IList<Assembly> assemblies = new List<Assembly>() {
				typeof(MyFakeService).Assembly,
				typeof(AnotherTestService).Assembly
			};
			_upgrader.OnBeforeRunStarted += (sender, args) => {
				Assert.IsTrue(true);
			};
			_upgrader.OnRunCompleted += (sender, args) => {
				Assert.IsTrue(true);
			};

			_upgrader.Run(assemblies);
		}

		[TestMethod]
		public void RunFailsWithSameAssembly() {
			IList<Assembly> assemblies = new List<Assembly>() {
				typeof(MyFakeService).Assembly,
				typeof(MyFakeService).Assembly
			};

			try {
				_upgrader.Run(assemblies);
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
			/*
			 * DbUpgrader upgrader = new DbUpgrader(connectionString, DbEngine.SqlServer);
			 * DbUpgrader mySqlUpgrader = new DbUpgrader(connectionString, DbEngine.MySql);
			 *
			 * public DbUpgrader(string connectionString, DbEngine engine) {
			 *	switch (engine) {
			 *		case DbEngine.SqlServer:
			 *			_upgrader = new SqlServerUpgrader(connectionString);
			 *			break;
			 *		case DbEngine.MySql:
			 *			_upgrader = new MySqlUpgrader(connectionString);
			 *			break;	
			 *	}
			 * }
			 *
			 * public void Run(IList<Assembly> assemblies) {
			 *		_upgrader.Run(assemblies);
			 * }
			 */
		}

	}

}
