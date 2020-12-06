using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Moq;
using Newtonsoft.Json.Linq;
using PAMU_CDS.Actions;
using Parser;
using Parser.ExpressionParser;
using Parser.FlowParser.ActionExecutors;

namespace Test.UnitTest
{
    [TestClass]
    public class GetRecordActionTest
    {
        [TestMethod]
        public async Task TestGetRecord()
        {
            var guid = Guid.NewGuid();

            var orgServiceMock = new Mock<IOrganizationService>();
            orgServiceMock.Setup(x => x.Create(It.IsAny<Entity>()))
                .Returns(guid)
                .Callback<Entity>((entity) =>
                {
                    Assert.AreEqual("John Doe", entity["name"]);
                });
            orgServiceMock.Setup(x => x.Retrieve(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<ColumnSet>()))
                .Returns(new Entity("contact", guid));
            
            var expressionEngineMock = new Mock<IExpressionEngine>();
            expressionEngineMock.Setup(x => x.Parse(It.IsAny<string>())).Returns<string>((input) => input);

            var stateMock = new Mock<IState>();
            stateMock.Setup(x => x.AddOutputs(It.IsAny<string>(), It.IsAny<ValueContainer>()))
                .Callback<string,ValueContainer>((actionName, valueContainer) =>
                {
                    Assert.AreEqual("CreateContact", actionName);
                });

            var createActionExecutor =
                new CreateRecordAction(expressionEngineMock.Object, orgServiceMock.Object, stateMock.Object);

            var actionDescription = 
                "{\"type\":\"OpenApiConnection\"," +
                "\"inputs\":" +
                "{\"host\":{\"connectionName\":\"shared_commondataserviceforapps\",\"operationId\":\"CreateRecord\",\"apiId\":\"/providers/Microsoft.PowerApps/apis/shared_commondataserviceforapps\"}," +
                "\"parameters\":{\"entityName\":\"accounts\",\"item/name\":\"John Doe\"}," +
                "\"authentication\":\"@parameters('$authentication')\"}}";
            createActionExecutor.InitializeActionExecutor("CreateContact", JToken.Parse(actionDescription));

            var response = await createActionExecutor.Execute();
            
            Assert.AreEqual(ActionStatus.Succeeded, response.ActionStatus);
            Assert.AreEqual(true, response.ContinueExecution);
            Assert.AreEqual(null, response.NextAction);
        }
    }
}