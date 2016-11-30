using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ScriptLoader.Contracts {

	public interface IScriptLoader {
		
		IList<ScriptDocument> Documents { get; }
		//TODO(Logan) -> Figure out how to add IBackingStore dependency to LoadDocuments()
		void LoadDocuments();

	}

}
