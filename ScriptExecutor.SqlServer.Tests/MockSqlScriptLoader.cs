﻿using Executioner.Tests.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ScriptExecutor.SqlServer.Tests {

	public class MockSqlScriptLoader : BaseMockLoader {

		public override XmlDocument GetXmlDoc() {
			string xmlStr = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
				<ScriptDocument>
					<Id>ac04f1b3-219a-4a40-8d7d-869dac218cca</Id>
					<Order>2016-06-21</Order>
					<Scripts>
						<Script Id=""278b7ef3-09da-4d1b-a101-390f4e6a5407"" Executor=""SqlServerExecutor"" Order=""2016-06-21""></Script>
						<Script Id=""17a49779-1fec-4624-9da8-3bfde0cd4852"" Executor=""SqlServerExecutor"" Order=""2016-06-22:1""></Script>
						<Script Id=""40b44acb-8bc5-4a8f-a318-2148052b37a5"" Executor=""SqlServerExecutor"" Order=""2016-06-22""></Script>
						<Script Id=""e511aaaa-f546-4e60-9146-7a9c30d1ef9e"" Executor=""SqlServerExecutor"" Order=""2016-06-23""></Script>
					</Scripts>
				</ScriptDocument>";
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xmlStr);

			return doc;
		}

	}

}
