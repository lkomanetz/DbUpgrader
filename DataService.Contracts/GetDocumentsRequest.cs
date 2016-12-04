using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackingStore.Contracts {

	public class GetDocumentsRequest {

		//TODO(Logan) -> Change from nullable boolean to an enumeration.
		public bool? IsComplete { get; set; }

	}

}
