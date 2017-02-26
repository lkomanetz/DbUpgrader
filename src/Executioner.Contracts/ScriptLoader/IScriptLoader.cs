using System;
using System.Collections.Generic;

namespace Executioner {
	namespace Contracts {

		public interface IScriptLoader {
			
			IList<ScriptDocument> Documents { get; }
			void LoadDocuments();

		}

	}

}