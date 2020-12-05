﻿using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;
using Newtonsoft.Json.Linq;
using PAMU_CDS.Actions;
using Parser.ExpressionParser;
using Parser.FlowParser.ActionExecutors;

namespace Test.UnitTest
{
    [TestClass]
    public class DeleteRecordActionTest
    {
        [TestMethod]
        public async Task TestDeleteAction()
        {
            var guid = Guid.NewGuid();

            var orgServiceMock = new Mock<IOrganizationService>();
            orgServiceMock.Setup(x => x.Delete(It.IsAny<string>(), It.IsAny<Guid>()))
                .Callback<string,Guid>((entityName, id) =>
                {
                    Assert.AreEqual("account", entityName);
                    Assert.AreEqual(guid, id);
                });

            var expressionEngineMock = new Mock<IExpressionEngine>();
            expressionEngineMock.Setup(x => x.Parse(It.IsAny<string>())).Returns<string>(input => input);
            expressionEngineMock.Setup(x => x.Parse("@triggerOutputs()?['body/contactid']")).Returns(guid.ToString());

            var deleteActionExecutor =
                new DeleteRecordAction(expressionEngineMock.Object, orgServiceMock.Object);

            var actionDescription =
                "{\"type\":\"OpenApiConnection\"," +
                "\"inputs\":{" +
                "\"host\":{\"connectionName\":\"shared_commondataserviceforapps_1\",\"operationId\":\"DeleteRecord\",\"apiId\":\"/providers/Microsoft.PowerApps/apis/shared_commondataserviceforapps\"}," +
                $"\"parameters\":{{\"entityName\":\"accounts\",\"recordId\":\"{guid}\"}},\"authentication\":\"@parameters('$authentication')\"}}}}";
            deleteActionExecutor.InitializeActionExecutor("DeleteContact", JToken.Parse(actionDescription));

            var response = await deleteActionExecutor.Execute();

            Assert.AreEqual(ActionStatus.Succeeded, response.ActionStatus);
            Assert.AreEqual(true, response.ContinueExecution);
            Assert.AreEqual(null, response.NextAction);
        }
    }
}