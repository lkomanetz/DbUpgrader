using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DbUpgrader.Contracts.Interfaces
{
	public interface IDbUpgrader
	{
		void Run(Assembly assembly);
	}

}
