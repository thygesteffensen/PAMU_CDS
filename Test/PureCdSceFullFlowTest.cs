using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Test
{
    [TestClass]
    public class PureCdSceFullFlowTest : TestBase
    {
        [TestMethod]
        public void TestCreateContact()
        {
            // Black box test
            var contact = new Entity("contact");
            
            contact.Id = OrgAdminService.Create(contact);
            
            var retrievedContact = OrgAdminService.Retrieve(contact.LogicalName, contact.Id, new ColumnSet("jobtitle"));

            Assert.IsNotNull(retrievedContact);
            Assert.AreEqual("Technical Supervisor", retrievedContact.Attributes["jobtitle"]);
        }
    }
}