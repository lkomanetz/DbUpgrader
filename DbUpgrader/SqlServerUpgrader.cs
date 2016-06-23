using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbUpgrader.Contracts.Interfaces;
using DbUpgrader.Contracts;
using System.Reflection;

namespace DbUpgrader {

	public class SqlServerUpgrader : IDbUpgrader {

		public SqlServerUpgrader(string connectionString) {
			this.ConnectionString = connectionString;
		}

		public string ConnectionString { get; private set; }

		public void Run(Assembly assembly) {
			Console.WriteLine(assembly.Location);
			/*
			 * IDbUpgrader upgrader = new SqlServerUpgrader('connectionString');
			 * Assembly assem = typeof(MyFakeService).Assembly;
			 * 
			 * upgrader.Run(assem);
			 */
		}

	}

}
