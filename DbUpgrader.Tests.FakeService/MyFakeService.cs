using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DbUpgrader;

namespace DbUpgrader.Tests.FakeService {

	public class MyFakeService {

		public void Run() {
			SqlServerUpgrader upgrader = new SqlServerUpgrader("connectionString");
			upgrader.Run(Assembly.GetExecutingAssembly());
		}

	}

}
