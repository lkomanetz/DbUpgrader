using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DbUpgrader.SqlServer;
using DbUpgrader.Tests.FakeService;
using DbUpgrader.Contracts;
using DbUpgrader.Contracts.Interfaces;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using DbUpgrader.Tests.AnotherFakeService;
using DbUpgrader.DataService.Contracts;
using DbUpgrader.DataService;

namespace DbUpgrader.Tests.SqlServer {

	[TestClass]
	public class SqlServerUpgraderTests {

		private static IDataService _dataService;
		private static SqlServerUpgrader _upgrader;
		private static IDbCleaner _cleaner;

		[ClassInitialize]
		public static void Initialize(TestContext context) {
			string connectionString = ConfigurationManager.ConnectionStrings["UnitTestConnectionString"].ConnectionString;
			_dataService = new InMemoryService();
			_cleaner = new TestSqlServerCleaner(connectionString);
			_upgrader = new SqlServerUpgrader(connectionString, _dataService);
			_cleaner.Clean();
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
			/*
			 * I'm only doing the Assert.IsTrue(true) because I want to make sure that both
			 * OnBeforeRunStarted and OnRunCompleted events are executed and nothing more.  This might
			 * change later on where I want to assert something with the event arguments that gets 
			 * passed into the event invocation.
			 */
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

	}

}
