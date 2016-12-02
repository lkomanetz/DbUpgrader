using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackingStore.Contracts;
using ScriptExecutor.Contracts;
using ScriptLoader.Contracts;
using Executioner.Contracts;

namespace ScriptExecutor {

	public abstract class BaseScriptExecutor : IScriptExecutor {
		protected IBackingStore _backingStore;
		protected IScriptLoader _scriptLoader;

		public BaseScriptExecutor(IScriptLoader scriptLoader, IBackingStore backingStore) {
			_backingStore = backingStore;
			_scriptLoader = scriptLoader;

			_scriptLoader.LoadDocuments(_backingStore);
		}

		public IList<Guid> CompletedDocumentIds { get { return _backingStore.GetCompletedDocumentIds(); } }
		public IList<ScriptDocument> ScriptDocuments { get { return _backingStore.GetDocuments(); } }

		public abstract void Execute();
	}

}
