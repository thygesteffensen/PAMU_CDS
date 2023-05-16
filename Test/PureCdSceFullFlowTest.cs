using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Collections.Generic;
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

        [TestMethod]
        public void TestLookupFilter()
        {
            var school = new Entity("demo_school");
            school.Id = OrgAdminService.Create(school);

            var science = new Entity("demo_course")
            {
                Attributes =
                {
                    new KeyValuePair<string, object>("demo_school",school.ToEntityReference())
                }
            };
            science.Id = OrgAdminService.Create(science);

            var child = new Entity("demo_child")
            {
                Attributes =
                {
                    new KeyValuePair<string, object>("demo_school",school.ToEntityReference())
                }
            };
            child.Id = OrgAdminService.Create(child);

            var enrollment = new Entity("demo_enrollment")
            {
                Attributes =
                {
                    new KeyValuePair<string, object>("demo_school",school.ToEntityReference()),
                    new KeyValuePair<string, object>("demo_child",child.ToEntityReference()),
                    new KeyValuePair<string, object>("demo_course",science.ToEntityReference())
                }
            };
            enrollment.Id = OrgAdminService.Create(enrollment);

            var retrieved = OrgAdminService.Retrieve("demo_enrollment", enrollment.Id, new ColumnSet(true));
            Assert.AreEqual(345630002, retrieved.GetAttributeValue<OptionSetValue>("statuscode").Value);

        }
    }
}
