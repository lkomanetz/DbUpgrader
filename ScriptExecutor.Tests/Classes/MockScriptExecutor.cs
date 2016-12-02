using BackingStore.Contracts;
using Executioner.Contracts;
using ScriptExecutor.Contracts;
using ScriptLoader.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptExecutor.Tests.Classes {

	public class MockScriptExecutor : BaseScriptExecutor {

		public MockScriptExecutor(IScriptLoader scriptLoader, IBackingStore backingStore) :
			base(scriptLoader, backingStore) {
		}

		public override void Execute() {
			IList<ScriptDocument> docsToExecute = _backingStore.GetDocuments();
			for (short i = 0; i < docsToExecute.Count; ++i) {
				for (short j = 0; j < docsToExecute[i].Scripts.Count; ++j) {
					Execute(docsToExecute[i].Scripts[j]);
				}
				docsToExecute[i].IsComplete = true;
				_backingStore.Update(docsToExecute[i]);
			}
		}

		private void Execute(Script script) {
			script.IsComplete = true;
			_backingStore.Update(script);
		}

	}

}
