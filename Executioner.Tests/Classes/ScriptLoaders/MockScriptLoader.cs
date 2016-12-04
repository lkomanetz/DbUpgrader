using ScriptLoader.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackingStore.Contracts;
using Executioner.Contracts;
using Executioner.ExtensionMethods;
using System.Xml;

namespace Executioner.Tests.Classes {

	public class MockScriptLoader : BaseMockLoader {

		public override XmlDocument GetXmlDoc() {
			string xmlStr = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
				<ScriptDocument>
					<Id>ac04f1b3-219a-4a40-8d7d-869dac218cca</Id>
					<Order>2016-06-21</Order>
					<Scripts>
						<Script Id=""278b7ef3-09da-4d1b-a101-390f4e6a5407"" Executor=""MockScriptExecutor"" Order=""2016-06-21"">
							CREATE TABLE [FirstTable] (
								Value1 NVARCHAR(25)
								, Value2 NVARCHAR(25)
							)
							CREATE TABLE [AnotherTable] (
								ValueA NVARCHAR(25)
								, ValueB NVARCHAR(25)
							)
							CREATE TABLE [ThirdTable] (
								Value9 NVARCHAR(25)
								, Value8 NVARCHAR(25)
							)
						</Script>
						<Script Id=""17a49779-1fec-4624-9da8-3bfde0cd4852"" Executor=""MockScriptExecutor"" Order=""2016-06-22:1"">
							INSERT INTO AnotherTable (ValueA, ValueB)
							VALUES ('TestA', 'TestB')
						</Script>
						<Script Id=""40b44acb-8bc5-4a8f-a318-2148052b37a5"" Executor=""MockScriptExecutor"" Order=""2016-06-22"">
							INSERT INTO FirstTable (Value1, Value2)
							VALUES ('Test1', 'Test2')
						</Script>
						<Script Id=""e511aaaa-f546-4e60-9146-7a9c30d1ef9e"" Executor=""MockScriptExecutor"" Order=""2016-06-23"">
							INSERT INTO ThirdTable (Value9, Value8)
							VALUES ('Test9', 'Test8')
						</Script>
					</Scripts>
				</ScriptDocument>";
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xmlStr);

			return doc;
		}

	}

}
