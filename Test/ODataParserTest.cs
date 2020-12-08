using Microsoft.VisualStudio.TestTools.UnitTesting;
using PAMU_CDS;

namespace Test
{
    [TestClass]
    public class ODataParserTest
    {
        [TestMethod]
        public void TestOdataParser()
        {
            const string selectQuery = "primarycontactid($select=contactid,fullname)";

            var oDataParser = new OdataParser();

            var oData = oDataParser.Get(selectQuery);

            Assert.AreEqual(1, oData.Length);
            Assert.AreEqual("primarycontactid", oData[0].Option);
            Assert.AreEqual(1, oData[0].Parameters.Length);
            Assert.AreEqual("select", oData[0].Parameters[0].Name);
            Assert.AreEqual(2, oData[0].Parameters[0].Properties.Length);
            Assert.AreEqual("contactid", oData[0].Parameters[0].Properties[0]);
            Assert.AreEqual("fullname", oData[0].Parameters[0].Properties[1]);
        }
        
        [TestMethod]
        public void TestOdataParser1()
        {
            const string selectQuery = "primarycontactid($select=contactid,fullname;$orderby=createdon asc;$filter=endswith(subject,'1'))";

            var oDataParser = new OdataParser();

            var oData = oDataParser.Get(selectQuery);

            Assert.AreEqual(1, oData.Length);
            Assert.AreEqual("primarycontactid", oData[0].Option);
            Assert.AreEqual(3, oData[0].Parameters.Length);
            
            Assert.AreEqual("select", oData[0].Parameters[0].Name);
            Assert.AreEqual(2, oData[0].Parameters[0].Properties.Length);
            Assert.AreEqual("contactid", oData[0].Parameters[0].Properties[0]);
            Assert.AreEqual("fullname", oData[0].Parameters[0].Properties[1]);
            
            Assert.AreEqual("orderby", oData[0].Parameters[1].Name);
            Assert.AreEqual(1, oData[0].Parameters[1].Properties.Length);
            Assert.AreEqual("createdon asc", oData[0].Parameters[1].Properties[0]);
            
            Assert.AreEqual("filter", oData[0].Parameters[2].Name);
            Assert.AreEqual(1, oData[0].Parameters[2].Properties.Length);
            Assert.AreEqual("endswith(subject,'1')", oData[0].Parameters[2].Properties[0]);
        }
        
        [TestMethod]
        public void TestOdataParser2()
        {
            const string selectQuery = "some_relationship";

            var oDataParser = new OdataParser();

            var oData = oDataParser.Get(selectQuery);

            Assert.AreEqual(1, oData.Length);
            Assert.AreEqual("some_relationship", oData[0].Option);

        }
        
        [TestMethod]
        public void TestOdataParser3()
        {
            const string selectQuery = "relationship_1,relationship_2($select=name)";

            var oDataParser = new OdataParser();

            var oData = oDataParser.Get(selectQuery);

            Assert.AreEqual(2, oData.Length);
            
            Assert.AreEqual("relationship_1", oData[0].Option);
            
            Assert.AreEqual("relationship_2", oData[1].Option);
            Assert.AreEqual(1, oData[1].Parameters.Length);
            Assert.AreEqual("select", oData[1].Parameters[0].Name);
            Assert.AreEqual(1, oData[1].Parameters[0].Properties.Length);
            Assert.AreEqual("name", oData[1].Parameters[0].Properties[0]);
        }
        
    }
}