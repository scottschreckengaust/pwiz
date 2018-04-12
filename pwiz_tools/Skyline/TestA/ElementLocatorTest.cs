using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using pwiz.Skyline.Model.ElementLocators;
using pwiz.SkylineTestUtil;

namespace pwiz.SkylineTestA
{
    [TestClass]
    public class ElementLocatorTest : AbstractUnitTest
    {
        [TestMethod]
        public void TestElementLocators()
        {
            var locatorProtein = new ElementLocator("A0JP43", null).ChangeType("MoleculeGroup");
            VerifyElementLocator(locatorProtein);
            var locatorPeptide = new ElementLocator("ELVIS", null).ChangeParent(locatorProtein).ChangeType("Molecule");
            VerifyElementLocator(locatorPeptide);
            var docKey = new ElementLocator("test", new[]
            {
                new KeyValuePair<string, string>("attr1", null),
                new KeyValuePair<string, string>("attr1", "attr2Value"),
                new KeyValuePair<string, string>("attr3&%", "att3Value*\"")
            }).ChangeParent(locatorPeptide);
            VerifyElementLocator(docKey);
        }

        [TestMethod]
        public void TestElementLocatorQuote()
        {
            Assert.AreEqual("a", ElementLocator.QuoteIfSpecial("a"));
            Assert.AreEqual("\"/\"", ElementLocator.QuoteIfSpecial("/"));
            Assert.AreEqual("\"?\"", ElementLocator.QuoteIfSpecial("?"));
            Assert.AreEqual("\"&\"", ElementLocator.QuoteIfSpecial("&"));
            Assert.AreEqual("\"=\"", ElementLocator.QuoteIfSpecial("="));
            Assert.AreEqual("\"\"\"\"", ElementLocator.QuoteIfSpecial("\""));
        }

        private void VerifyElementLocator(ElementLocator objectReference)
        {
            string str = objectReference.ToString();
            var docKeyRoundTrip = ElementLocator.Parse(str);
            Assert.AreEqual(objectReference, docKeyRoundTrip);
            Assert.AreEqual(str, docKeyRoundTrip.ToString());
        }
    }
}
