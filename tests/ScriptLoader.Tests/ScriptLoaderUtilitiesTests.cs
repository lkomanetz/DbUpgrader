using System;
using System.Text;
using System.IO;
using Executioner.Contracts;
using Xunit;

namespace ScriptLoader.Tests {

	public class ScriptLoaderUtilitiesTests {

		[Fact]
		public void ScriptWithXmlNamespaceSucceeds() {
			Exception ex = Record.Exception(() => {
				string xml = GenerateXmlWithNamespace();
				byte[] xmlAsBytes = Encoding.ASCII.GetBytes(xml);
				MemoryStream ms = new MemoryStream(xmlAsBytes);
				ScriptDocument doc = ScriptLoaderUtilities.CreateScriptDocument(ms, "Test");
			});

			Assert.Null(ex);
		}

		private string GenerateXmlWithNamespace() {
			return $@"
				<ScriptDocument>
					<Id>{Guid.NewGuid()}</Id>
					<Order>2017-03-22</Order>
					<Scripts>
						<Script Id='{Guid.NewGuid()}' Executor='SqlServerExecutor' Order='2017-03-22'>
							<![CDATA[PRINT '<E:HELLO/>']]>
						</Script>
					</Scripts>
				</ScriptDocument>
			";
		}

	}

}