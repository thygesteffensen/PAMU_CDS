using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Moq;
using Newtonsoft.Json.Linq;
using PAMU_CDS.Actions;
using PAMU_CDS.Auxiliary;
using Parser.ExpressionParser;
using Parser.FlowParser.ActionExecutors;

namespace Test.UnitTest
{
    [TestClass]
    public class GetRecordActionTest
    {
        [TestMethod]
        public async Task TestGetRecordNoOdata()
        {
            var loggerMock = new Mock<ILogger<GetItemAction>>();

            RetrieveRequest retrieveRequest = null;
            var guid = Guid.NewGuid();

            var orgServiceMock = new Mock<IOrganizationService>();

            orgServiceMock.Setup(x => x.Execute(It.IsAny<RetrieveRequest>()))
                .Callback<OrganizationRequest>(request => { retrieveRequest = (RetrieveRequest) request; })
                .Returns(new RetrieveResponse
                {
                    Results = new ParameterCollection
                        {{nameof(Entity), new Entity {LogicalName = "contact", Id = guid}}}
                });

            var expressionEngineMock = new Mock<IExpressionEngine>();
            expressionEngineMock.Setup(x => x.Parse(It.IsAny<string>())).Returns<string>(input => input);
            expressionEngineMock.Setup(x => x.ParseToValueContainer(It.IsAny<string>())).Returns<string>((input) => new ValueContainer(input));

            var fa = new OrganizationServiceContext {OrganizationService = orgServiceMock.Object};
            
            var createActionExecutor =
                new GetItemAction(expressionEngineMock.Object, fa,
                    loggerMock.Object);

            var actionDescription =
                "{\"type\":\"OpenApiConnection\"," +
                "\"inputs\":" +
                "{\"host\":{\"connectionName\":\"shared_commondataserviceforapps\",\"operationId\":\"GetItem\",\"apiId\":\"/providers/Microsoft.PowerApps/apis/shared_commondataserviceforapps\"}," +
                $"\"parameters\":{{\"entityName\":\"contacts\",\"recordId\":\"{guid}\"}}," +
                "\"authentication\":\"@parameters('$authentication')\"}}";
            createActionExecutor.InitializeActionExecutor("GetContact", JToken.Parse(actionDescription));

            var response = await createActionExecutor.Execute();


            orgServiceMock.Verify(x => x.Execute(It.IsAny<RetrieveRequest>()));

            Assert.IsNotNull(retrieveRequest);
            Assert.AreEqual("contact", retrieveRequest.Target.LogicalName);
            Assert.AreEqual(guid, retrieveRequest.Target.Id);
            Assert.IsTrue(retrieveRequest.ColumnSet.AllColumns);
            Assert.IsNull(retrieveRequest.RelatedEntitiesQuery);
            
            
            Assert.IsNotNull(response.ActionOutput);

            Assert.AreEqual(ActionStatus.Succeeded, response.ActionStatus);
            Assert.AreEqual(true, response.ContinueExecution);
            Assert.AreEqual(null, response.NextAction);
        }

        [TestMethod]
        public async Task TestGetRecordSelect()
        {
            var guid = Guid.NewGuid();
            var columns = new[] {"firstname", "lastname", "address1_name"};

            RetrieveRequest retrieveRequest = null;

            var loggerMock = new Mock<ILogger<GetItemAction>>();

            var orgServiceMock = new Mock<IOrganizationService>();

            orgServiceMock.Setup(x => x.Execute(It.IsAny<RetrieveRequest>()))
                .Callback<OrganizationRequest>(request =>
                {
                    retrieveRequest = (RetrieveRequest) request;
                })
                .Returns(new RetrieveResponse
                {
                    Results = new ParameterCollection
                    {
                        {
                            nameof(Entity), new Entity
                            {
                                LogicalName = "contact",
                                Id = guid,
                                Attributes = new AttributeCollection
                                {
                                    {"firstname", "John"},
                                    {"address1_name", "123 Main St Anytown"},
                                    {"address1_line1", "123 Main St"}
                                }
                            }
                        }
                    }
                });

            var expressionEngineMock = new Mock<IExpressionEngine>();
            expressionEngineMock.Setup(x => x.Parse(It.IsAny<string>())).Returns<string>(input => input);
            expressionEngineMock.Setup(x => x.ParseToValueContainer(It.IsAny<string>())).Returns<string>((input) => new ValueContainer(input));

            var fa = new OrganizationServiceContext {OrganizationService = orgServiceMock.Object};
            
            var createActionExecutor =
                new GetItemAction(expressionEngineMock.Object, fa,
                    loggerMock.Object);

            var actionDescription =
                "{\"type\":\"OpenApiConnection\"," +
                "\"inputs\":" +
                "{\"host\":{\"connectionName\":\"shared_commondataserviceforapps\",\"operationId\":\"GetItem\",\"apiId\":\"/providers/Microsoft.PowerApps/apis/shared_commondataserviceforapps\"}," +
                $"\"parameters\":{{\"entityName\":\"contacts\",\"recordId\":\"{guid}\",\"$select\":\"{string.Join(",", columns)}\"}}," +
                "\"authentication\":\"@parameters('$authentication')\"}}";
            createActionExecutor.InitializeActionExecutor("GetContact", JToken.Parse(actionDescription));

            var response = await createActionExecutor.Execute();
            
            Assert.IsNotNull(retrieveRequest);
            Assert.AreEqual("contact", retrieveRequest.Target.LogicalName);
            Assert.AreEqual(guid, retrieveRequest.Target.Id);
            Assert.IsFalse(retrieveRequest.ColumnSet.AllColumns);
            Assert.IsNull(retrieveRequest.RelatedEntitiesQuery);

            var cols = retrieveRequest.ColumnSet;

            Assert.AreEqual(3, cols.Columns.Count);
            Assert.IsTrue(cols.Columns.Contains(columns[0]));
            Assert.IsTrue(cols.Columns.Contains(columns[1]));
            Assert.IsTrue(cols.Columns.Contains(columns[2]));
            
            

            Assert.IsNotNull(response.ActionOutput);
            var body = response.ActionOutput["body"].GetValue<Dictionary<string, ValueContainer>>();

            Assert.IsTrue(body.ContainsKey("firstname"));
            Assert.IsTrue(body.ContainsKey("address1_name"));
            

            Assert.AreEqual(ActionStatus.Succeeded, response.ActionStatus);
            Assert.AreEqual(true, response.ContinueExecution);
            Assert.AreEqual(null, response.NextAction);
        }
    }
}