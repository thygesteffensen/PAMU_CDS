using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Moq;
using Newtonsoft.Json.Linq;
using PAMU_CDS.Auxiliary;
using PAMU_CDS.Actions;
using Parser;
using Parser.ExpressionParser;
using Parser.FlowParser.ActionExecutors;

namespace Test.UnitTest
{
    [TestClass]
    public class ListRecordsActionTest
    {
        [TestMethod]
        public async Task TestGetRecordNoOdata()
        {
            var loggerMock = new Mock<ILogger<ListRecordsAction>>();

            RetrieveMultipleRequest retrieveRequest = null;
            ValueContainer outputValueContainer = null;

            var orgServiceMock = new Mock<IOrganizationService>();

            orgServiceMock.Setup(x => x.Execute(It.IsAny<RetrieveMultipleRequest>()))
                .Callback<OrganizationRequest>(request => { retrieveRequest = (RetrieveMultipleRequest) request; })
                .Returns(new RetrieveMultipleResponse
                {
                    Results = new ParameterCollection
                    {
                        {
                            "EntityCollection",
                            new EntityCollection {Entities = {new Entity("contact"), new Entity("contact")}}
                        }
                    }
                });


            var expressionEngineMock = new Mock<IExpressionEngine>();
            expressionEngineMock.Setup(x => x.Parse(It.IsAny<string>())).Returns<string>(input => input);
            expressionEngineMock.Setup(x => x.ParseToValueContainer(It.IsAny<string>())).Returns<string>((input) => new ValueContainer(input));
            
            var stateMock = new Mock<IState>();
            stateMock.Setup(x => x.AddOutputs(It.IsAny<string>(), It.IsAny<ValueContainer>()))
                .Callback<string, ValueContainer>((actionName, valueContainer) =>
                {
                    outputValueContainer = valueContainer;
                });

            var fa = new OrganizationServiceContext {OrganizationService = orgServiceMock.Object};
            
            var createActionExecutor =
                new ListRecordsAction(expressionEngineMock.Object, fa, stateMock.Object,
                    loggerMock.Object);
            
            var actionDescription =
                "{\"type\":\"OpenApiConnection\"," +
                "\"inputs\":" +
                "{\"host\":{\"connectionName\":\"shared_commondataserviceforapps\",\"operationId\":\"ListRecords\",\"apiId\":\"/providers/Microsoft.PowerApps/apis/shared_commondataserviceforapps\"},"
                +
                $"\"parameters\":{{\"entityName\":\"contacts\"}}," +
                "\"authentication\":\"@parameters('$authentication')\"}}";
            createActionExecutor.InitializeActionExecutor("GetContacts", JToken.Parse(actionDescription));

            var response = await createActionExecutor.Execute();

            orgServiceMock.Verify(x => x.Execute(It.IsAny<RetrieveMultipleRequest>()));

            Assert.IsNotNull(retrieveRequest);
            Assert.AreEqual(typeof(QueryExpression), retrieveRequest.Query.GetType());

            var query = (QueryExpression) retrieveRequest.Query;
            Assert.AreEqual("contact", query.EntityName);
            Assert.IsNotNull(outputValueContainer);
            Assert.AreEqual(ActionStatus.Succeeded, response.ActionStatus);
            Assert.AreEqual(true, response.ContinueExecution);
            Assert.AreEqual(null, response.NextAction);
        }

        [TestMethod]
        public async Task TestListRecordsOdata()
        {
            var loggerMock = new Mock<ILogger<ListRecordsAction>>();

            RetrieveMultipleRequest retrieveRequest = null;
            ValueContainer outputValueContainer = null;

            var orgServiceMock = new Mock<IOrganizationService>();

            orgServiceMock.Setup(x => x.Execute(It.IsAny<RetrieveMultipleRequest>()))
                .Callback<OrganizationRequest>(request => { retrieveRequest = (RetrieveMultipleRequest) request; })
                .Returns(new RetrieveMultipleResponse
                {
                    Results = new ParameterCollection
                    {
                        {
                            "EntityCollection",
                            new EntityCollection {Entities = {new Entity("contact"), new Entity("contact")}}
                        }
                    }
                });

            var expressionEngineMock = new Mock<IExpressionEngine>();
            expressionEngineMock.Setup(x => x.Parse(It.IsAny<string>())).Returns<string>(input => input);
            expressionEngineMock.Setup(x => x.ParseToValueContainer(It.IsAny<string>())).Returns<string>((input) => new ValueContainer(input, true));

            var stateMock = new Mock<IState>();
            stateMock.Setup(x => x.AddOutputs(It.IsAny<string>(), It.IsAny<ValueContainer>()))
                .Callback<string, ValueContainer>((actionName, valueContainer) =>
                {
                    outputValueContainer = valueContainer;
                });

            var fa = new OrganizationServiceContext {OrganizationService = orgServiceMock.Object};
            
            var listRecordsActionExecutor =
                new ListRecordsAction(expressionEngineMock.Object, fa, stateMock.Object,
                    loggerMock.Object);
            var actionDescription =
                "{\"type\":\"OpenApiConnection\"," +
                "\"inputs\":" +
                "{\"host\":{\"connectionName\":\"shared_commondataserviceforapps\",\"operationId\":\"ListRecords\",\"apiId\":\"/providers/Microsoft.PowerApps/apis/shared_commondataserviceforapps\"},"
                +
                "\"parameters\":{\"entityName\":\"contacts\"," +
                "\"$select\":\"fullname,firstname,lastname\"," +
                "\"$filter\":\"age lt 25 and age gt 15\"," +
                "\"$orderby\":\"age desc\"," +
                "\"$top\":\"25\"" +
                "}," +
                "\"authentication\":\"@parameters('$authentication')\"}}";
            listRecordsActionExecutor.InitializeActionExecutor("GetContacts", JToken.Parse(actionDescription));

            var response = await listRecordsActionExecutor.Execute();

            orgServiceMock.Verify(x => x.Execute(It.IsAny<RetrieveMultipleRequest>()));

            Assert.IsNotNull(retrieveRequest);
            Assert.AreEqual(typeof(QueryExpression), retrieveRequest.Query.GetType());

            var query = (QueryExpression) retrieveRequest.Query;
            Assert.AreEqual("contact", query.EntityName);
            Assert.IsNotNull(outputValueContainer);

            Assert.AreEqual(false, query.ColumnSet.AllColumns);
            Assert.IsTrue(query.ColumnSet.Columns.Contains("fullname"));
            Assert.IsTrue(query.ColumnSet.Columns.Contains("firstname"));
            Assert.IsTrue(query.ColumnSet.Columns.Contains("lastname"));

            Assert.AreEqual(25, query.TopCount);

            Assert.AreEqual("age", query.Orders.First().AttributeName);
            Assert.AreEqual(OrderType.Descending, query.Orders.First().OrderType);

            Assert.IsNotNull(query.Criteria);
            
            Assert.AreEqual(ActionStatus.Succeeded, response.ActionStatus);
            Assert.AreEqual(true, response.ContinueExecution);
            Assert.AreEqual(null, response.NextAction);
        }
    }
}