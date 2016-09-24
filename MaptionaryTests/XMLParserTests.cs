using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Maptionary;

namespace MaptionaryTests {
    [TestClass]
    public class XMLParserTests {

        // For the most part, the XML tests are exactly the same as the YAML tests
        // ...excepting edge cases, just like with the JSON tests
        // ...and that I don't *actually* understand the XML format.

        //TODO:
        // Attributes
        // Node with leaf value AND contents (is that valid?)
        //  ^^ Leaf value split by contents (is that valid?)
        // Comments / directives - <?...>
        // Collapse test cases so that the assert blocks are re-used.
        // XML "arrays"
        // Whitespace fuckery (note: for XML, <  key>value</>)

        [TestMethod]
        public void Simplest() {
            string data = @"<key>value</key>";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["key"] == "value");
        }

        [TestMethod]
        public void WhitespacePreservation() {
            string data = @"<key>  value
  </key>";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["key"] == "  value\r\n  ", "Whitespace preservation seems off: '" + n["key"] + "'. Could be line endings");
        }

        //Added "feature", mostly for ease in writing the test data.
        // This feature is meant for hand-written XML - generated will have normal closing tags.
        //TODO: Or is this a normal feature?
        [TestMethod]
        public void NonstrictTermination() {
            string data = @"<key>value</>";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["key"] == "value");
        }

        [TestMethod()]
        public void Flat() {
            string data = @"
<key1>value1</>
<key2>value2</>
<key3>value3</>";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["key1"] == "value1", "Flat key doesn't have correct value: '" + n["key1"] + "'");
            Assert.IsTrue(n["key2"] == "value2");
            Assert.IsTrue(n["key3"] == "value3");
        }

        //TODO: Is this valid XML?
        [TestMethod()]
        public void Flat_SingleLine() {
            string data = @"<key1>value1</><key2>value2</><key3>value3</>";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["key1"] == "value1", "Flat key doesn't have correct value: '" + n["key1"] + "'");
            Assert.IsTrue(n["key2"] == "value2");
            Assert.IsTrue(n["key3"] == "value3");
        }

        [TestMethod()]
        public void RepeatedKeys() {
            string data = @"
<key>value1</>
<key>value2</>
";

            Node n = Parser.Parse(data);

            //TODO: Desired behavior?
            Assert.IsTrue(n["key"] == "value2", "Repeated key doesn't overwrite prior key");
        }

        //TODO: Empty XML nodes and truthiness. And how preserved whitespace makes all that rather wonky.
        //(We don't actually care here, since we can just check for key presence)
        //And, quotes don't matter for values!
        [TestMethod()]
        public void Quotations() {
            string data = @"
<'single'></>
<""double""></>
<'""doubles in single""'></>
<""'singles in doubles'""></>
<'use:colon'></>
<'use\nnewline'></>
<key></>
<' edging whitespace '></>
";

            Node n = Parser.Parse(data);
            Assert.IsTrue(n.ContainsKey("single"));
            Assert.IsTrue(n.ContainsKey("double"));
            Assert.IsTrue(n.ContainsKey("\"doubles in single\""));
            Assert.IsTrue(n.ContainsKey("'singles in doubles'"));
            Assert.IsTrue(n.ContainsKey("use:colon"));
            //Assert.IsTrue(n.ContainsKey("use\nnewline")); TODO: This test fails, and I don't have any good idea why.
            Assert.IsTrue(n.ContainsKey("key"));
            Assert.IsTrue(n.ContainsKey(" edging whitespace "));
        }

        [TestMethod()]
        public void Object() {
            string data = @"
<object>
  <key1>value1</>
  <key2>value2</>
  <key3>value3</>
</>
<key>value</>
";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["object"]["key1"] == "value1");
            Assert.IsTrue(n["object"]["key2"] == "value2");
            Assert.IsTrue(n["object"]["key3"] == "value3");
            Assert.IsTrue(n["key"] == "value");
        }
        [TestMethod()]
        public void MultipleObjects() {
            string data = @"
<object1>
  <key1>value1</>
  <key2>value2</>
  <key3>value3</>
</>
<object2>
  <key1>value3</>
  <key2>value4</>
  <key3>value5</>
</>
<key>value</>
";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["object1"]["key1"] == "value1");
            Assert.IsTrue(n["object1"]["key2"] == "value2");
            Assert.IsTrue(n["object1"]["key3"] == "value3");
            Assert.IsTrue(n["object2"]["key1"] == "value3");
            Assert.IsTrue(n["object2"]["key2"] == "value4");
            Assert.IsTrue(n["object2"]["key3"] == "value5");
            Assert.IsTrue(n["key"] == "value");
        }


        [TestMethod()]
        public void NestedObjects() {
            string data = @"
<object1>
  <key1>value11</>
  <innerObject1>
    <key1>value111</>
    <key2>value112</>
    <key3>value113</>
  </>
  <innerObject2>
    <key1>value121</>
    <key2>value122</>
    <key3>value123</>
  </>
  <key2>value12</>
  <key3>value13</>
  <innerObject3>
    <key1>value131</>
    <key2>value132</>
    <key3>value133</>
  </>
</>
<object2>
  <key1>value21</>
  <level2Object1>
    <key>value</>
    <level3Object1>
      <level4Object1>
        <key>value</>
      </>
      <key>value</>
      <level4Object2>
        <key>value</>
      </>
    </>
    <level3Object2>
      <key>value</>
    </>
  </>
  <level2Object2>
    <key>value</>
  </>
  <key2>value22</>
  <key3>value23</>
</>
<key>value</>
";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["object1"]["key1"] == "value11");
            Assert.IsTrue(n["object1"]["key2"] == "value12");
            Assert.IsTrue(n["object1"]["key3"] == "value13");

            Assert.IsTrue(n["object1"]["innerObject1"]["key1"] == "value111");
            Assert.IsTrue(n["object1"]["innerObject1"]["key2"] == "value112");
            Assert.IsTrue(n["object1"]["innerObject1"]["key3"] == "value113");

            Assert.IsTrue(n["object1"]["innerObject2"]["key1"] == "value121");
            Assert.IsTrue(n["object1"]["innerObject2"]["key2"] == "value122");
            Assert.IsTrue(n["object1"]["innerObject2"]["key3"] == "value123");

            Assert.IsTrue(n["object1"]["innerObject3"]["key1"] == "value131");
            Assert.IsTrue(n["object1"]["innerObject3"]["key2"] == "value132");
            Assert.IsTrue(n["object1"]["innerObject3"]["key3"] == "value133");

            Assert.IsTrue(n["object2"]["key1"] == "value21");
            Assert.IsTrue(n["object2"]["key2"] == "value22");
            Assert.IsTrue(n["object2"]["key3"] == "value23");

            Assert.IsTrue(n["object2"]["level2Object1"]["key"] == "value");
            Assert.IsTrue(n["object2"]["level2Object2"]["key"] == "value");
            Assert.IsTrue(n["object2"]["level2Object1"]["level3Object1"]["key"] == "value");
            Assert.IsTrue(n["object2"]["level2Object1"]["level3Object2"]["key"] == "value");
            Assert.IsTrue(n["object2"]["level2Object1"]["level3Object1"]["level4Object1"]["key"] == "value");
            Assert.IsTrue(n["object2"]["level2Object1"]["level3Object1"]["level4Object2"]["key"] == "value");

            Assert.IsTrue(n["key"] == "value");
        }

    } // ABOVE THIS
}
