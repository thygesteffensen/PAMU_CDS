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
            const string selectQuery = "$expand=primarycontactid($filter=endswith(subject,'1'))";

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
        public void TestOdataParser2()
        {
            const string selectQuery = "dca_contact_dca_socialsecurityno";

            var oDataParser = new OdataParser();

            var oData = oDataParser.Get(selectQuery);

            Assert.AreEqual(1, oData.Length);
        }
        
        [TestMethod]
        public void TestOdataParser3()
        {
            const string selectQuery = "relationship_1,relationship_2($select=name)";

            var oDataParser = new OdataParser();

            var oData = oDataParser.Get(selectQuery);

            Assert.AreEqual(1, oData.Length);
        }
        
    }
}