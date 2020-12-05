using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using PAMU_CDS.Auxiliary;
using Parser.ExpressionParser;

namespace Test
{
    [TestClass]
    public class EntityExtensionTest : TestBase
    {
        [TestMethod]
        public void TestEntityToValueContainer()
        {
            var contact = new Entity("contact");
            var contactGuid = Guid.NewGuid();
            contact.Attributes["contactid"] = contactGuid;
            contact.Attributes["fullname"] = "John Doe";
            contact.Attributes["age"] = 31;
            contact.Attributes["statecode"] = new OptionSetValue(1);
            var accountGuid = Guid.NewGuid();
            contact.Attributes["account"] = new EntityReference("account", accountGuid);

            var contactValueContainer = contact.ToValueContainer()["body"];
            var contactValueContainerAsDict = contactValueContainer.GetValue<Dictionary<string, ValueContainer>>();

            Assert.AreEqual(10, contactValueContainerAsDict.Keys.Count,
                "Number of values in value container does not match.");

            Assert.IsTrue(
                contactValueContainerAsDict.ContainsKey(
                    "_account_value@Microsoft.Dynamics.CRM.associatednavigationproperty"));
        }

        [TestMethod]
        public void TestCreateEntityFromParameters()
        {
            var entity = new Entity();
            var entityAsValueContainer =
                new ValueContainer(new Dictionary<string, ValueContainer>
                {
                    {"item", new ValueContainer(new Dictionary<string, ValueContainer>())}
                })
                {
                    ["entityName"] = new ValueContainer("contact"),
                    ["recordId"] = new ValueContainer(Guid.NewGuid().ToString())
                };

            var items = entityAsValueContainer["item"];
            items["jobtitle"] = new ValueContainer("Wrapping Division Grade 3");
            items["fullname"] = new ValueContainer("Bryony Shelfley");
            items["employee"] = new ValueContainer("North Pole Giftwrap Battalion");

            entity.CreateEntityFromParameters(entityAsValueContainer);

            Assert.AreEqual("Bryony Shelfley", entity.Attributes["fullname"]);
        }
    }
}