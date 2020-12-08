﻿using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Moq;
using Newtonsoft.Json.Linq;
using PAMU_CDS.Actions;
using Parser.ExpressionParser;
using Parser.FlowParser.ActionExecutors;

namespace Test.UnitTest
{
    [TestClass]
    public class AssociateEntitiesActionTest
    {
        [TestMethod]
        public async Task TestAssociateAction()
        {
            AssociateRequest associateRequest = null;
            var accountId = Guid.NewGuid();
            var parentAccountId = Guid.NewGuid();
            var relationship = new Relationship("account_parent_account");

            var orgServiceMock = new Mock<IOrganizationService>();
            orgServiceMock.Setup(x => x.Execute(It.IsAny<AssociateRequest>()))
                .Callback<OrganizationRequest>(request => { associateRequest = (AssociateRequest) request; });

            var expressionEngineMock = new Mock<IExpressionEngine>();
            expressionEngineMock.Setup(x => x.Parse(It.IsAny<string>())).Returns<string>(input => input);

            var associateActionExecutor =
                new AssociateEntitiesAction(expressionEngineMock.Object, orgServiceMock.Object);

            var actionDescription =
                "{\"type\":\"OpenApiConnection\"," +
                "\"inputs\":{" +
                "\"host\":{\"connectionName\":\"shared_commondataserviceforapps_1\",\"operationId\":\"AssociateEntities\",\"apiId\":\"/providers/Microsoft.PowerApps/apis/shared_commondataserviceforapps\"}," +
                $"\"parameters\":{{\"entityName\":\"accounts\",\"recordId\":\"{accountId}\",\"associationEntityRelationship\":\"{relationship.SchemaName}\"," +
                $"\"item/@odata.id\":\"https://contoso.crm4.dynamics.com/api/data/v9.1/accounts({parentAccountId})\"}}," +
                "\"authentication\":\"@parameters('$authentication')\"}}";
            associateActionExecutor.InitializeActionExecutor("DeleteContact", JToken.Parse(actionDescription));

            var response = await associateActionExecutor.Execute();

            Assert.AreEqual(ActionStatus.Succeeded, response.ActionStatus);
            Assert.AreEqual(true, response.ContinueExecution);
            Assert.AreEqual(null, response.NextAction);
            
            orgServiceMock.Verify(x=> x.Execute(It.IsAny<AssociateRequest>()));
            
            Assert.AreEqual(relationship.SchemaName, associateRequest.Relationship.SchemaName);
            Assert.AreEqual(accountId, associateRequest.Target.Id);
            Assert.AreEqual("account", associateRequest.Target.LogicalName);
            Assert.AreEqual(1, associateRequest.RelatedEntities.Count);
            Assert.AreEqual(parentAccountId, associateRequest.RelatedEntities[0].Id);
            Assert.AreEqual("account", associateRequest.RelatedEntities[0].LogicalName);
        }
    }
}