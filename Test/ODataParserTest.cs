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
            var selectQuery = "$expand=primarycontactid($select=contactid,fullname)";

            var oDataParser = new OdataParser();

            var oData = oDataParser.Get(selectQuery);

            Assert.AreEqual("expand", oData.Name);
            Assert.AreEqual(1, oData.Values.Length);
            Assert.AreEqual("primarycontactid", oData.Values[0].OptionName);
            Assert.AreEqual(1, oData.Values[0].Parameters.Length);
            Assert.AreEqual("select", oData.Values[0].Parameters[0].Name);
            Assert.AreEqual(2, oData.Values[0].Parameters[0].Properties.Length);
            Assert.AreEqual("contactid", oData.Values[0].Parameters[0].Properties[0]);
            Assert.AreEqual("fullname", oData.Values[0].Parameters[0].Properties[1]);
        }

        [TestMethod]
        public void TestOdataParser2()
        {
            var selectQuery = "$select=primarycontactid,option2";

            var oDataParser = new OdataParser();

            var oData = oDataParser.Get(selectQuery);

            Assert.AreEqual("select", oData.Name);
            Assert.AreEqual(2, oData.Values.Length);
            Assert.AreEqual("primarycontactid", oData.Values[0].OptionName);
            Assert.AreEqual("option2", oData.Values[1].OptionName);
        }
        
        [TestMethod]
        public void TestOdataParser3()
        {
            var selectQuery = "dca_contact_dca_socialsecurityno";

            var oDataParser = new OdataParser();

            var oData = oDataParser.Get(selectQuery);

            Assert.AreEqual("select", oData.Name);
            Assert.AreEqual(2, oData.Values.Length);
            Assert.AreEqual("primarycontactid", oData.Values[0].OptionName);
            Assert.AreEqual("option2", oData.Values[1].OptionName);
        }
        
    }
}