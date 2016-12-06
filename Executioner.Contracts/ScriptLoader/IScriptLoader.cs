using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Executioner.Contracts {

	public interface IScriptLoader {
		
		IList<ScriptDocument> Documents { get; }
		void LoadDocuments(IBackingStore backingStore);

	}

}
