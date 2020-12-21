using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Parser.FlowParser.ActionExecutors;

namespace Test
{
    internal static class TestSingleCustomActionStatic
    {
        public static bool TestSingleCustomActionHaveRun;
    }
    
    [TestClass]
    public class TestSingleCustomAction : TestBase
    {
        [TestMethod]
        public void TestCreateContact()
        {
            var entity = new Entity("contact");

            OrgAdminService.Create(entity);
            
            Assert.IsTrue(TestSingleCustomActionStatic.TestSingleCustomActionHaveRun);
        }
    }
    
    public class ManualActionExecutor : ActionExecutorBase
    {
        protected override void ProcessJson()
        {
            
        }

        public override Task<ActionResult> Execute()
        {
            TestSingleCustomActionStatic.TestSingleCustomActionHaveRun = true;
            
            return Task.FromResult(new ActionResult());
        }
    }
}