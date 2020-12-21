using Microsoft.VisualStudio.TestTools.UnitTesting;
using PAMU_CDS;

namespace Test
{
    [TestClass]
    public class CalculatorTest
    {

        [TestMethod]
        public void Test()
        {
            var calc = new Calculator();
            
            Assert.AreEqual(10, calc.Calculate("2+2*4"));
            Assert.AreEqual(8, calc.Calculate("2*4"));
            Assert.AreEqual(8, calc.Calculate("2*2*2"));
            Assert.AreEqual(10, calc.Calculate("2*4+2"));
            Assert.AreEqual(18, calc.Calculate("2*2*4+2"));
            Assert.AreEqual(28, calc.Calculate("2*2*4+2+10"));
        }
    }
}