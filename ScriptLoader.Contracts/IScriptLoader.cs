using BackingStore.Contracts;
using Executioner.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ScriptLoader.Contracts {

	public interface IScriptLoader {
		
		IList<ScriptDocument> Documents { get; }
		void LoadDocuments(IBackingStore backingStore);

	}

}
