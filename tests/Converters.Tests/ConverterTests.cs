using Executioner;
using Executioner.Converters;
using Executioner.Contracts;
using System;
using System.Collections.Generic;
using Xunit;

namespace Converters.Tests {

    public class ConverterTests {

        [Fact]
        public void ConvertsScriptToLogEntry() {
            Script script = new Script() {
                SysId = Guid.NewGuid(),
                IsComplete = false,
                ScriptText = String.Empty,
                ExecutorName = String.Empty,
            };

            LogEntry entry = ScriptConverter.ToLogEntry(script);
            Assert.True(AreEqual(entry, script));
        }

        private bool AreEqual<T>(LogEntry entry, T item) {
            if (item is Script) {
                var s = item as Script;
                return (entry.SysId == s.SysId);
            }
            else
                return false;
        }

    }

}