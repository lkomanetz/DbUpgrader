using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DbUpgrader;
using DbUpgrader.Tests.FakeService;

namespace DbUpgrader.Tests
{
	[TestClass]
	public class UnitTest1
	{
		private const string CONNECTION_STRING = "Test_Connection";

		[TestMethod]
		public void TestMethod1()
		{
			SqlServerUpgrader upgrader = new SqlServerUpgrader(CONNECTION_STRING);
			upgrader.Run(typeof(MyFakeService).Assembly);
		}
	}
}
