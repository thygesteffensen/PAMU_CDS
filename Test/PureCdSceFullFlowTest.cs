using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Linq;

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

        [TestMethod]
        public void TestCreateAccountLookupBackFromContact()
        {
            var account = new Entity("account");

            account.Id = OrgAdminService.Create(account);

            var retrievedContact = OrgAdminService.RetrieveMultiple(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet(true)
            }).Entities.FirstOrDefault();

            Assert.IsNotNull(retrievedContact);
            Assert.AreEqual(account.Id, retrievedContact.GetAttributeValue<EntityReference>("parentcustomerid")?.Id);

        }
    }
}