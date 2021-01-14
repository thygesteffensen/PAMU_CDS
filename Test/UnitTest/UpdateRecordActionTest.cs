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
    public class UpdateRecordActionTest
    {
        private Entity _entity;

        [TestMethod]
        public async Task TestUpdateRecord()
        {
            var guid = Guid.NewGuid();

            OrganizationRequest updateRequest = null;
            string outputActionName = null;

            var orgServiceMock = new Mock<IOrganizationService>();

            orgServiceMock.Setup(x => x.Execute(It.IsAny<OrganizationRequest>()))
                .Returns<OrganizationRequest>(request => new OrganizationResponse())
                .Callback<OrganizationRequest>(request => { updateRequest = request; });

            orgServiceMock.Setup(x => x.Retrieve(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<ColumnSet>()))
                .Returns(new Entity("account", guid));

            var expressionEngineMock = new Mock<IExpressionEngine>();
            expressionEngineMock.Setup(x => x.Parse(It.IsAny<string>())).Returns<string>((input) => input);
            expressionEngineMock.Setup(x => x.ParseToValueContainer(It.IsAny<string>())).Returns<string>((input) => new ValueContainer(input));

            var stateMock = new Mock<IState>();
            stateMock.Setup(x => x.AddOutputs(It.IsAny<string>(), It.IsAny<ValueContainer>()))
                .Callback<string, ValueContainer>((actionName, valueContainer) =>
                {
                    outputActionName = actionName;
                });

            var updateActionExecutor =
                new UpdateRecordAction(expressionEngineMock.Object, orgServiceMock.Object, stateMock.Object);

            var actionDescription =
                "{\"type\":\"OpenApiConnection\"," +
                "\"inputs\":{" +
                "\"host\":{\"connectionName\":\"shared_commondataserviceforapps\",\"operationId\":\"UpdateRecord\",\"apiId\":\"/providers/Microsoft.PowerApps/apis/shared_commondataserviceforapps\"}," +
                $"\"parameters\":{{\"entityName\":\"accounts\",\"recordId\":\"{guid}\"," +
                "\"item/name\":\"Another name\",\"item/address1_city\":\"Springfield\",\"item/address1_line1\":\"Evergreen Terrace 472\"}," +
                "\"authentication\":\"@parameters('$authentication')\"}}";
            updateActionExecutor.InitializeActionExecutor("UpdateAccount", JToken.Parse(actionDescription));

            var response = await updateActionExecutor.Execute();

            Assert.AreEqual(1, updateRequest.Parameters.Count);
            Assert.IsTrue(updateRequest.Parameters.ContainsKey("Target"));

            _entity = (Entity) updateRequest.Parameters["Target"];

            Assert.AreEqual(3, _entity.Attributes.Count);

            Assert.IsTrue(_entity.Attributes.ContainsKey("name"));
            Assert.IsTrue(_entity.Attributes.ContainsKey("address1_city"));
            Assert.IsTrue(_entity.Attributes.ContainsKey("address1_line1"));
            Assert.AreEqual("Update", updateRequest.RequestName);
            
            
            Assert.IsNotNull(outputActionName);
            Assert.AreEqual("UpdateAccount", outputActionName);

            Assert.AreEqual(ActionStatus.Succeeded, response.ActionStatus);
            Assert.AreEqual(true, response.ContinueExecution);
            Assert.AreEqual(null, response.NextAction);
        }
    }
}