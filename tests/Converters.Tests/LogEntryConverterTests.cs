using Executioner;
using Executioner.Converters;
using Executioner.Contracts;
using System;
using System.Collections.Generic;
using Xunit;

namespace Converters.Tests {

    public class LogEntryConverterTests {

        [Fact]
        public void ConvertsScriptDocumentToLogEntry() {
            ScriptDocument doc = new ScriptDocument() {
                SysId = Guid.NewGuid(),
                IsComplete = true,
                Scripts = new List<Script>() {
                    new Script() {
                        IsComplete = true,
                        SysId = Guid.NewGuid()
                    }
                }
            };

            LogEntry entry = LogEntryConverters.FromScriptDocument(doc);
            Assert.True(AreEqual(entry, doc));
        }

        [Fact]
        public void ConvertsScriptToLogEntry() {
            Script script = new Script() {
                SysId = Guid.NewGuid(),
                IsComplete = false,
                ScriptText = String.Empty,
                ExecutorName = String.Empty,
            };

            LogEntry entry = LogEntryConverters.FromScript(script);
            Assert.True(AreEqual(entry, script));
        }

        private bool AreEqual<T>(LogEntry entry, T item) {
            if (item is Script) {
                var s = item as Script;
                return (entry.SysId == s.SysId && entry.IsComplete == s.IsComplete && entry.Scripts == null);
            }
            else if (item is ScriptDocument) {
                var d = item as ScriptDocument;
                return AreEqual(entry, d);
            }
            else
                return false;

            bool AreEqual(LogEntry logEntry, ScriptDocument doc) {
                bool isEqual = (
                    logEntry.SysId == doc.SysId &&
                    logEntry.IsComplete == doc.IsComplete &&
                    (logEntry.Scripts != null && logEntry.Scripts.Count == doc.Scripts.Count)
                );

                if (!isEqual)
                    return false;

                for (short i = 0; i < logEntry.Scripts.Count; ++i) {
                    isEqual = (
                        logEntry.Scripts != null &&
                        logEntry.Scripts[i].SysId == doc.Scripts[i].SysId &&
                        logEntry.Scripts[i].IsComplete == doc.Scripts[i].IsComplete
                    );

                    if (!isEqual)
                        return false;
                }

                return isEqual;
            }
        }

    }

}