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
            var input = "fullname eq 'Hej' and firstname eq 'Hej'";

            var parser = new OdataFilter();

            var filter = parser.OdataToFilterExpression(input);

            Assert.AreEqual(LogicalOperator.And, filter.FilterOperator);
            Assert.AreEqual(2, filter.Conditions.Count);

            var condition1 = filter.Conditions.First();

            Assert.AreEqual(ConditionOperator.Equal, condition1.Operator);
            Assert.AreEqual("firstname", condition1.AttributeName);
            Assert.AreEqual("Hej", condition1.Values.First());

            var condition2 = filter.Conditions.Last();

            Assert.AreEqual(ConditionOperator.Equal, condition2.Operator);
            Assert.AreEqual("fullname", condition2.AttributeName);
            Assert.AreEqual("Hej", condition2.Values.First());
        }

        [TestMethod]
        public void TestMs()
// https://docs.microsoft.com/en-us/dotnet/api/microsoft.xrm.sdk.messages.retrievemultiplerequest?view=dynamics-general-ce-9
        {
            var parser = new OdataFilter();

            var filterExpression =
                parser.OdataToFilterExpression("(FirstName eq 'Joe' or FirstName eq 'John') and City eq 'Redmond'");

            Assert.AreEqual(1, filterExpression.Conditions.Count);
            Assert.AreEqual(LogicalOperator.And, filterExpression.FilterOperator);
            Assert.AreEqual("City", filterExpression.Conditions.First().AttributeName);
            Assert.AreEqual("Redmond", filterExpression.Conditions.First().Values.First());

            Assert.AreEqual(1, filterExpression.Filters.Count);
            var innerFilter = filterExpression.Filters.First();
            Assert.AreEqual(2, innerFilter.Conditions.Count);
            Assert.AreEqual(LogicalOperator.Or, innerFilter.FilterOperator);
            Assert.AreEqual("FirstName", innerFilter.Conditions.First().AttributeName);
            Assert.AreEqual("FirstName", innerFilter.Conditions.Last().AttributeName);
        }

        [TestMethod]
        public void TestIntegerValues()
        {
            var parser = new OdataFilter();

            var filterExpression =
                parser.OdataToFilterExpression("age eq 32");

            Assert.AreEqual(1, filterExpression.Conditions.Count);
            Assert.AreEqual(LogicalOperator.And, filterExpression.FilterOperator);
            Assert.AreEqual("age", filterExpression.Conditions.First().AttributeName);
            Assert.AreEqual(32, filterExpression.Conditions.First().Values.First());
        }
        
        [TestMethod]
        public void TestDecimalValues()
        {
            var parser = new OdataFilter();

            var filterExpression =
                parser.OdataToFilterExpression("height eq 1.88");

            Assert.AreEqual(1, filterExpression.Conditions.Count);
            Assert.AreEqual(LogicalOperator.And, filterExpression.FilterOperator);
            Assert.AreEqual("height", filterExpression.Conditions.First().AttributeName);
            Assert.AreEqual(1.88m, filterExpression.Conditions.First().Values.First());
        }
        
        [TestMethod]
        public void TestBoolValues()
        {
            var parser = new OdataFilter();

            var filterExpression =
                parser.OdataToFilterExpression("enrolled eq true");

            Assert.AreEqual(1, filterExpression.Conditions.Count);
            Assert.AreEqual(LogicalOperator.And, filterExpression.FilterOperator);
            Assert.AreEqual("enrolled", filterExpression.Conditions.First().AttributeName);
            Assert.AreEqual(true, filterExpression.Conditions.First().Values.First());
        }
        
        [TestMethod]
        public void TestNullValues()
        {
            var parser = new OdataFilter();

            var filterExpression =
                parser.OdataToFilterExpression("degree eq null");

            Assert.AreEqual(1, filterExpression.Conditions.Count);
            Assert.AreEqual(LogicalOperator.And, filterExpression.FilterOperator);
            Assert.AreEqual("degree", filterExpression.Conditions.First().AttributeName);
            Assert.AreEqual(ConditionOperator.Null, filterExpression.Conditions.First().Operator);
            Assert.AreEqual(0, filterExpression.Conditions.First().Values.Count);
        }
    }
}