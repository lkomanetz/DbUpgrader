using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Executioner.NetFramework.Tests {

	public class DotNetFrameworkTests {

		[Fact]
		public void ScriptExecutionerRunsInFullNetFramework() {
			string[] scripts = new string[] {
				$"<Script Id='{Guid.NewGuid()}' Executor='SqlServerExecutor' Order='2016-06-21'></Script>"
			};

			ScriptExecutioner executioner = new ScriptExecutioner(
				new BaseMockLoader(scripts),
				new MockDataStore()
			);

		}

	}

}
