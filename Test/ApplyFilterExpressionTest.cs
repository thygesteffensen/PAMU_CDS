using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using PAMU_CDS.Auxiliary;

namespace Test
{
    [TestClass]
    public class ApplyFilterExpressionTest
    {
        [TestMethod]
        public void TestFilterExpressionEqual()
        {
            var entity = new Entity {Attributes = {["firstname"] = "John"}};
            var filterExpression = new FilterExpression {FilterOperator = LogicalOperator.And};
            filterExpression.Conditions.Add(new ConditionExpression("firstname", ConditionOperator.Equal, "John"));

            var result = ApplyFilterExpression.ApplyFilterExpressionToEntity(entity, filterExpression);

            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void TestFilterExpressionNotEqual()
        {
            var entity = new Entity {Attributes = {["firstname"] = "Jane"}};
            var filterExpression = new FilterExpression {FilterOperator = LogicalOperator.And};
            filterExpression.Conditions.Add(new ConditionExpression("firstname", ConditionOperator.NotEqual, "John"));

            var result = ApplyFilterExpression.ApplyFilterExpressionToEntity(entity, filterExpression);

            Assert.AreEqual(true, result);
        }


        [TestMethod]
        public void TestFilterExpressionCombined()
        {
            var entity = new Entity {Attributes = {["firstname"] = "Jane", ["lastname"] = "Doe"}};
            var filterExpression = new FilterExpression {FilterOperator = LogicalOperator.And};
            filterExpression.Conditions.Add(new ConditionExpression("firstname", ConditionOperator.NotEqual, "John"));
            filterExpression.Conditions.Add(new ConditionExpression("lastname", ConditionOperator.Equal, "Doe"));

            var result = ApplyFilterExpression.ApplyFilterExpressionToEntity(entity, filterExpression);

            Assert.AreEqual(true, result);
        }


        [TestMethod]
        public void TestFilterExpressionNonExistingProperty()
        {
            var entity = new Entity();
            var filterExpression = new FilterExpression {FilterOperator = LogicalOperator.And};
            filterExpression.Conditions.Add(new ConditionExpression("firstname", ConditionOperator.Equal, "John"));

            var result = ApplyFilterExpression.ApplyFilterExpressionToEntity(entity, filterExpression);

            Assert.AreEqual(false, result);
        }
    }
}