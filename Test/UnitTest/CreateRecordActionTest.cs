﻿using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Moq;
using Newtonsoft.Json.Linq;
using PAMU_CDS.Actions;
using PAMU_CDS.Auxiliary;
using Parser.ExpressionParser;
using Parser.FlowParser.ActionExecutors;

namespace Test.UnitTest
{
    [TestClass]
    public class CreateRecordActionTest
    {
        [TestMethod]
        public async Task TestCreateRecord()
        {
            var guid = Guid.NewGuid();

            Entity createEntity = null;

            var orgServiceMock = new Mock<IOrganizationService>();

            orgServiceMock.Setup(x => x.Create(It.IsAny<Entity>()))
                .Callback<Entity>(entity => { createEntity = entity; })
                .Returns(guid);
            
            orgServiceMock.Setup(x => x.Retrieve(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<ColumnSet>()))
                .Returns(new Entity("contact", guid));

            var expressionEngineMock = new Mock<IExpressionEngine>();
            expressionEngineMock.Setup(x => x.Parse(It.IsAny<string>())).Returns<string>((input) => input);
            expressionEngineMock.Setup(x => x.ParseToValueContainer(It.IsAny<string>())).Returns<string>((input) => new ValueContainer(input));

            var fa = new OrganizationServiceContext {OrganizationService = orgServiceMock.Object};

            var createActionExecutor = new CreateRecordAction(expressionEngineMock.Object, fa);

            var actionDescription =
                "{\"type\":\"OpenApiConnection\"," +
                "\"inputs\":" +
                "{\"host\":{\"connectionName\":\"shared_commondataserviceforapps\",\"operationId\":\"CreateRecord\",\"apiId\":\"/providers/Microsoft.PowerApps/apis/shared_commondataserviceforapps\"}," +
                "\"parameters\":{\"entityName\":\"accounts\",\"item/name\":\"John Doe\"}," +
                "\"authentication\":\"@parameters('$authentication')\"}}";
            createActionExecutor.InitializeActionExecutor("CreateContact", JToken.Parse(actionDescription));

            var response = await createActionExecutor.Execute();

            Assert.IsNotNull(createEntity);
            Assert.AreEqual("John Doe", createEntity["name"]);
            
            Assert.AreEqual(ActionStatus.Succeeded, response.ActionStatus);
            Assert.AreEqual(true, response.ContinueExecution);
            Assert.AreEqual(null, response.NextAction);
        }
    }
}