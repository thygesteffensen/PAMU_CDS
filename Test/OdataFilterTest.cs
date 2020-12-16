using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk.Query;
using PAMU_CDS.Auxiliary;

namespace Test
{
    [TestClass]
    public class OdataFilterTest
    {
        [TestMethod]
        public void TestOneCondition()
        {
            var input = "fullname eq 'Hej'";
            
            var parser = new OdataFilter();

            var filter = parser.OdataToFilterExpression(input);
            var cond = filter.Conditions.First();
            
            Assert.AreEqual(ConditionOperator.Equal, cond.Operator);
            Assert.AreEqual("fullname", cond.AttributeName);
            Assert.AreEqual("Hej", cond.Values.First());
        }
        
        [TestMethod]
        public void TestAndCondition()
        {
            var input = "fullname eq 'Hej' and firstname eq 'Hej' or lastname ne 'Hej'";
            // var input = "fullname eq 'Hej' and firstname eq 'Hej'";
            
            var parser = new OdataFilter();

            var filter = parser.OdataToFilterExpression(input);
            
            Assert.AreEqual(LogicalOperator.And ,filter.FilterOperator);
            Assert.AreEqual(2, filter.Conditions.Count);
            
            var condition1 = filter.Conditions.First();
            
            Assert.AreEqual(ConditionOperator.Equal, condition1.Operator);
            Assert.AreEqual("fullname", condition1.AttributeName);
            Assert.AreEqual("Hej", condition1.Values.First());
            
            var condition2 = filter.Conditions.First();
            
            Assert.AreEqual(ConditionOperator.Equal, condition2.Operator);
            Assert.AreEqual("firstname", condition2.AttributeName);
            Assert.AreEqual("Hej", condition2.Values.First());
        }
    }
}