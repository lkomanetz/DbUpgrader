using System;
using System.Text;
using System.IO;
using Executioner.Contracts;
using Xunit;

namespace ScriptLoader.Tests {

	public class ScriptLoaderUtilitiesTests {

		[Theory]
		[InlineData("<![CDATA[PRINT '<E:HELLO/>']]>")] // Namespace and special XML characters
		[InlineData("<![CDATA[UPDATE something SET This = 'Something & Something Else']]>")] // Special characters
		public void ScriptWithXmlNamespaceSucceeds(string scriptText) {
			Exception ex = Record.Exception(() => {
				string xml = GenerateXmlWithNamespace(scriptText);
				byte[] xmlAsBytes = Encoding.ASCII.GetBytes(xml);
				MemoryStream ms = new MemoryStream(xmlAsBytes);
				ScriptDocument doc = ScriptLoaderUtilities.CreateScriptDocument(ms, "Test");
			});

			Assert.Null(ex);
		}

		private string GenerateXmlWithNamespace(string scriptText) {
			return $@"
				<ScriptDocument>
					<Id>{Guid.NewGuid()}</Id>
					<Order>2017-03-22</Order>
					<Scripts>
						<Script Id='{Guid.NewGuid()}' Executor='SqlServerExecutor' Order='2017-03-22'>
							{scriptText}
						</Script>
					</Scripts>
				</ScriptDocument>
			";
		}

	}

}