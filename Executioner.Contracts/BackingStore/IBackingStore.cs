﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Executioner.Contracts {

	public interface IBackingStore : IDisposable {

		IList<ScriptDocument> GetDocuments(GetDocumentsRequest request = null);
		IList<Guid> GetCompletedDocumentIds();
		IList<Script> GetScriptsFor(Guid documentId);
		IList<Guid> GetCompletedScriptIdsFor(Guid documentId);
		void Add(ScriptDocument document);
		void Add(Script script);
		void Update(ScriptDocument document);
		void Update(Script script);
		bool Delete(ScriptDocument document);
		bool Delete(Script script);
		void Clean();

	}

}
