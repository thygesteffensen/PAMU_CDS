using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;
using PAMU_CDS;
using PAMU_CDS.Auxiliary;
using Parser;
using Parser.FlowParser;

namespace Test
{
    [TestClass]
    public class XrmMockupCdsTriggerTest
    {
        delegate void FlowRunnerCallback(in string path);

        [TestMethod]
        public void TestTrigger()
        {
            var logger = TestLogger.Create<XrmMockupCdsTrigger>();
            var settings = new CdsFlowSettings {DontExecuteFlows = new[] {"Single_Custom_Action.json"}};

            var flowPaths = new List<string>();

            var flowRunner = new Mock<IFlowRunner>();
            flowRunner.Setup(x => x.InitializeFlowRunner(It.Ref<string>.IsAny))
                .Callback(new FlowRunnerCallback((in string path) => flowPaths.Add(path)));

            var state = new Mock<IState>();
            var orgServiceCtx = new Mock<OrganizationServiceContext>();

            var serviceProvider = new Mock<IServiceProvider>();
            // GetRequiredService<> is an extension, the extension uses this method ;)
            serviceProvider.Setup(x => x.GetService(typeof(IFlowRunner))).Returns(flowRunner.Object);
            serviceProvider.Setup(x => x.GetService(typeof(IState))).Returns(state.Object);
            serviceProvider.Setup(x => x.GetService(typeof(OrganizationServiceContext))).Returns(orgServiceCtx.Object);

            var scope = new Mock<IServiceScope>();
            scope.SetupGet(x => x.ServiceProvider).Returns(serviceProvider.Object);

            var scopeFactory = new Mock<IServiceScopeFactory>();
            scopeFactory.Setup(x => x.CreateScope()).Returns(scope.Object);

            var xrmMockupTrigger = new XrmMockupCdsTrigger(logger, Options.Create(settings), scopeFactory.Object);

            var testFlowPath = Path.GetFullPath(@"TestFlows");
            xrmMockupTrigger.AddFlows(new Uri(testFlowPath + "/Every_CDS_ce_action.json"));
            xrmMockupTrigger.AddFlows(new Uri(testFlowPath + "/Pure_CDS_ce.json"));
            xrmMockupTrigger.AddFlows(new Uri(testFlowPath + "/Single_Custom_Action.json"));

            var orgService = new Mock<IOrganizationService>();

            var entity = new Entity("contact");

            var request = new OrganizationRequest("Create") {Parameters = {["Target"] = entity}};

            xrmMockupTrigger.TriggerExtension(orgService.Object, request, entity, entity, new EntityReference());

            Assert.AreEqual(2, flowPaths.Count, "Only expected to flows to be initialized.");
            Assert.AreEqual(1, flowPaths.Count(x=> x.EndsWith("Every_CDS_ce_action.json")));
            Assert.AreEqual(1, flowPaths.Count(x=> x.EndsWith("Pure_CDS_ce.json")));
        }
    }
}