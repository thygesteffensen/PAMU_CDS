using System;
using System.Collections.Generic;
using System.IO;
using DG.Tools.XrmMockup;
using IXrmMockupExtension;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using PAMU_CDS;
using Parser;

namespace Test
{
    [TestClass]
    public class TestBase
    {
        private static readonly string TestFlowPath = Path.GetFullPath(@"TestFlows");

        
        protected IOrganizationService OrgAdminUiService;
        protected IOrganizationService OrgAdminService;
        protected static XrmMockup365 Crm;
        protected static XrmMockupCdsTrigger _pamuCds;

        public TestBase()
        {
            OrgAdminUiService =
                Crm.GetAdminService(new MockupServiceSettings(true, false, MockupServiceSettings.Role.UI));
            OrgAdminService = Crm.GetAdminService();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Crm.ResetEnvironment();
        }


        [AssemblyInitialize]
        public static void InitializeServices(TestContext context)
        {
            var services = new ServiceCollection();
            services.AddFlowRunner();
            services.AddPamuCds();
            
            services.AddFlowActionByName<ManualActionExecutor>("Post_a_notification_using_non_existing_provider");

            var sp = services.BuildServiceProvider();

            _pamuCds = sp.GetRequiredService<XrmMockupCdsTrigger>();
            
            _pamuCds.AddFlows(new Uri(TestFlowPath));
            
            InitializeMockup(context);
        }

        public static void InitializeMockup(TestContext context)
        {
            var path = Directory.GetCurrentDirectory();
            Crm = XrmMockup365.GetInstance(new XrmMockupSettings
            {
                BasePluginTypes = new Type[] { },
                CodeActivityInstanceTypes = new Type[] { },
                EnableProxyTypes = true,
                IncludeAllWorkflows = true,
                MockUpExtensions = new List<IMockUpExtension> {_pamuCds}
            });
        }
    }
}