using Microsoft.VisualStudio.TestTools.UnitTesting;
using Maptionary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maptionary.Tests {
    [TestClass()]
    public class NodeTests {
        [TestMethod()]
        public void SingleLevel() {
            Node n = new Node();

            Assert.IsFalse(n["key"], "Unassigned key isn't falsy");

            n["key"] = "value";

            Assert.IsTrue(n["key"], "Assigned key isn't truthy");
            Assert.AreEqual((string)n["key"], "value", "Assigned key doesn't have correct value: " + n["key"]);
            //Check the intended use case.
            Assert.IsTrue(n["key"] == "value", "Assigned key doesn't have correct value: " + n["key"]);
        }

        [TestMethod()]
        public void MultiLevel() {
            Node n = new Node();

            Assert.IsFalse(n["key"]["key"], "Unnassigned nested key isn't falsy");

            n["key"]["key"] = "value";

            Assert.IsTrue(n["key"]["key"], "Assigned nested key isn't truthy");
            Assert.AreEqual((string)n["key"]["key"], "value", "Assigned nested key doesn't have correct value: " + n["key"]["key"]);
            //Check the intended use case.
            Assert.IsTrue(n["key"]["key"] == "value", "Assigned nested key doesn't have correct value: " + n["key"]["key"]);
        }

        [TestMethod()]
        public void Multinode() {
            Node n = new Node();
            Node n1 = new Node();

            n["key"] = n1;

            Assert.AreSame(n["key"], n1, "Node assigned to key is the same");
            Assert.IsFalse(n["key"], "Key assigned with empty node isn't falsy");
            Assert.IsFalse(n["key"]["key"], "Unnassigned key through nested node isn't falsy");

            n1["key"] = "value";

            Assert.IsTrue(n["key"], "Key assigned with non-empty node isn't truthy");
            Assert.AreEqual((string)n["key"]["key"], "value", "Key assigned through nested node doesn't have correct value: " + n["key"]["key"]);
            //Check the intended use case.
            Assert.IsTrue(n["key"]["key"] == "value", "Key assigned through nested node doesn't have correct value: " + n["key"]["key"]);

            n1["recursion"] = n;
            // Compiles / runs, which is a test in an of itself.
            // TODO: Test for this in serialization
            Assert.AreSame(n["key"]["recursion"], n, "Recursive structure doesn't cause an error");
            Assert.AreEqual((string)n["key"]["recursion"]["key"]["key"], "value", "Key accessed through recursive structure doesn't have correct value: " + n["key"]["recursion"]["key"]["key"]);
        }

        [TestMethod()]
        public void StringValueConversion() {
            Node n = new Node();

            n["key"] = "value";

            Assert.AreEqual((string)n["key"], "value", "Assigned key doesn't have correct value: " + n["key"]);

            //TODO: More string tests. Are there other ways to do / be a string?
        }

        [TestMethod()]
        public void StringKeyConversion() {
            // Test covered by all other tests. (Can you think of other ways / kinds of string to pass in as a key?)
        }

        [TestMethod()]
        public void NumericalValueConversion() {
            Node n = new Node();

            n["integer string"] = "1";
            n["float string"] = "1.23";
            n["float"] = 1.23f;
            n["integer"] = 1;
            n["double"] = 1.23d;
            n["NaN"] = "NaN";

            Assert.AreEqual((string)n["integer string"], "1", "Integer string not equal to integer string: " + ((int)n["integer string"]));
            Assert.IsTrue(n["integer string"] == "1", "Integer string not equal to integer string: " + ((int)n["integer string"]));

            Assert.AreEqual((int)n["integer string"], 1, "Integer string not equal to integer: " + ((int)n["integer string"]));
            Assert.IsTrue(n["integer string"] == 1, "Integer string not equal to integer: " + ((int)n["integer string"]));

            Assert.AreEqual((string)n["float string"], "1.23", "Float string not equal to float string: " + ((int)n["float string"]));
            Assert.IsTrue(n["float string"] == "1.23", "Float string not equal to float string: " + ((int)n["float string"]));

            Assert.AreEqual((float)n["float string"], 1.23f, "Float string not equal to float: " + ((int)n["float string"]));
            Assert.IsTrue(n["float string"] == 1.23f, "Float string not equal to float: " + ((int)n["float string"]));

            Assert.AreEqual((string)n["integer"], "1", "Integer not equal to integer string");
            Assert.IsTrue(n["integer"] == "1", "Integer not equal to integer string");

            Assert.AreEqual((int)n["integer"], 1, "Integer not equal to integer");
            Assert.IsTrue(n["integer"] == 1, "Integer not equal to integer");

            Assert.AreEqual((string)n["float"], "1.23", "Float not equal to float string: " + n["float"]);
            Assert.IsTrue(n["float"] == "1.23", "Float not equal to float string");

            Assert.AreEqual((float)n["float"], 1.23f, "Float not equal to float");
            Assert.IsTrue(n["float"] == 1.23f, "Float not equal to float");

            Assert.AreEqual((double)n["float"], 1.23d, "Float not equal to double");
            Assert.IsTrue(n["float"] == 1.23d, "Float not equal to double");

            Assert.IsTrue(n["float"] == 1.23, "Float not equal to simple decimal");

            Assert.IsFalse(n["NaN"] == 1, "NaN shouldn't equal 1");
            Assert.IsFalse(n["NaN"] == 1d, "NaN shouldn't equal 1d");
            Assert.IsFalse(n["NaN"] == 1f, "NaN shouldn't equal 1f");
        }

        [TestMethod()]
        public void NumericalVKeyConversion() {
            Node n = new Node();

            n[1] = "1";
            n[1.23] = "1.23";
            n[1.23f] = "1.23";
            n[1.23d] = "1.23";

            Assert.AreEqual((string)n[1], "1", "Integer key not equal to integer string");
            Assert.AreEqual((string)n[1.23], "1.23", "Simple double key not equal to simple double string");
            Assert.AreEqual((string)n[1.23f], "1.23", "Float key not equal to simple double string");
            Assert.AreEqual((string)n[1.23d], "1.23", "Double key not equal to simple double string");
        }
    }
    
    //TODO: Test safe parsing of malformed YAML
    [TestClass()]
    public class YAMLParseTests {
        [TestMethod()]
        public void Simplest() {
            string data = @"key: value";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["key"] == "value");
        }

        [TestMethod()]
        public void RepeatedKeys() {
            string data = @"
key: value1
key: value2
";

            Node n = Parser.Parse(data);

            //TODO: Desired behavior?
            Assert.IsTrue(n["key"] == "value2", "Repeated key doesn't overwrite prior key");
        }

        [TestMethod()]
        public void Flat() {
            string data = @"
key1: value1
key2: value2
key3: value3
";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["key1"] == "value1");
            Assert.IsTrue(n["key2"] == "value2");
            Assert.IsTrue(n["key3"] == "value3");
        }

        [TestMethod()]
        public void Quotations() {
            string data = @"
'single': ""double""
'""doubles in single""': ""'singles in doubles'""
'use:colon': 'use\nnewline'
key: ' edging whitespace '
";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["single"] = "double");
            Assert.IsTrue(n["\"doubles in single\""] == "'singles in doubles'");
            Assert.IsTrue(n["use:colon"] = "use\nnewline");
            Assert.IsTrue(n["key"] = " edging whitespace ");
        }

        [TestMethod()]
        public void Object() {
            string data = @"
object:
  key1: value1
  key2: value2
  key3: value3
key: value
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
object1:
  key1: value1
  key2: value2
  key3: value3
object2:
  key1: value1
  key2: value2
  key3: value3
key: value
";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["object1"]["key1"] == "value1");
            Assert.IsTrue(n["object1"]["key2"] == "value2");
            Assert.IsTrue(n["object1"]["key3"] == "value3");
            Assert.IsTrue(n["object2"]["key1"] == "value1");
            Assert.IsTrue(n["object2"]["key2"] == "value2");
            Assert.IsTrue(n["object2"]["key3"] == "value3");
            Assert.IsTrue(n["key"] == "value");
        }

        [TestMethod()]
        public void NestedObjects() {
            string data = @"
object1:
  key1: value1
  innerObject1:
    key1: value1
    key2: value2
    key3: value3
  innerObject2:
    key1: value1
    key2: value2
    key3: value3
  key2: value2
  key3: value3
  innerObject3:
    key1: value1
    key2: value2
    key3: value3
object2:
  key1: value1
  level2Object1:
    key: value
    level3Object1:
      level4Object1:
        key: value
      key: value
      level4Object2:
        key: value
    level3Object2:
      key: value
  level2Object2:
    key: value
  key2: value2
  key3: value3
key: value
";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["object1"]["key1"] == "value1");
            Assert.IsTrue(n["object1"]["key2"] == "value2");
            Assert.IsTrue(n["object1"]["key3"] == "value3");

            Assert.IsTrue(n["object2"]["key1"] == "value1");
            Assert.IsTrue(n["object2"]["key2"] == "value2");
            Assert.IsTrue(n["object2"]["key3"] == "value3");

            Assert.IsTrue(n["key"] == "value");

            Assert.IsTrue(n["object1"]["innerObject1"]["key1"] == "value1");
            Assert.IsTrue(n["object1"]["innerObject1"]["key2"] == "value2");
            Assert.IsTrue(n["object1"]["innerObject1"]["key3"] == "value3");

            Assert.IsTrue(n["object1"]["innerObject2"]["key1"] == "value1");
            Assert.IsTrue(n["object1"]["innerObject2"]["key2"] == "value2");
            Assert.IsTrue(n["object1"]["innerObject2"]["key3"] == "value3");

            Assert.IsTrue(n["object1"]["innerObject3"]["key1"] == "value1");
            Assert.IsTrue(n["object1"]["innerObject3"]["key2"] == "value2");
            Assert.IsTrue(n["object1"]["innerObject3"]["key3"] == "value3");

            Assert.IsTrue(n["object1"]["level2Object1"]["key"] == "value");
            Assert.IsTrue(n["object1"]["level2Object2"]["key"] == "value");
            Assert.IsTrue(n["object1"]["level2Object1"]["level3Object1"]["key"] == "value");
            Assert.IsTrue(n["object1"]["level2Object1"]["level3Object2"]["key"] == "value");
            Assert.IsTrue(n["object1"]["level2Object1"]["level3Object1"]["level4Object1"]["key"] == "value");
            Assert.IsTrue(n["object1"]["level2Object1"]["level3Object1"]["level4Object2"]["key"] == "value");


            data = @"
object:
  level2Object:
    level3Object:
      level4Object:
        key: value
";
            n = Parser.Parse(data);

            Assert.IsTrue(n["object1"]["level2Object"]["level3Object"]["level4Object"]["key"] == "value");
        }

        [TestMethod()]
        public void BasicArray() {
            string data = @"
array:
- 0
- 1
- 2
key: value";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n[0] == 0);
            Assert.IsTrue(n[1] == 1);
            Assert.IsTrue(n[2] == 2);
            Assert.IsTrue(n["key"] == "value");

            data = @"
array:
- 0
- 1
- 2";

            n = Parser.Parse(data);

            Assert.IsTrue(n[0] == 0);
            Assert.IsTrue(n[1] == 1);
            Assert.IsTrue(n[2] == 2);
        }

        [TestMethod()]
        public void BasicIndentedArray() {
            string data = @"
array:
  - 0
  - 1
  - 2
key: value";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n[0] == 0);
            Assert.IsTrue(n[1] == 1);
            Assert.IsTrue(n[2] == 2);
            Assert.IsTrue(n["key"] == "value");

            data = @"
array:
  - 0
  - 1
  - 2";

            n = Parser.Parse(data);

            Assert.IsTrue(n[0] == 0);
            Assert.IsTrue(n[1] == 1);
            Assert.IsTrue(n[2] == 2);
        }


        [TestMethod()]
        public void NestedArray() {
            string data = @"
array:
- 0
- - 0
  - 1
  - 2
-   - 0
    - 1
    - 2
- 3
key: value";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n[0] == 0);
            Assert.IsTrue(n[3] == 3);
            Assert.IsTrue(n["key"] == "value");

            Assert.IsTrue(n[1][0] == 0);
            Assert.IsTrue(n[1][1] == 1);
            Assert.IsTrue(n[1][2] == 2);

            Assert.IsTrue(n[2][0] == 0);
            Assert.IsTrue(n[2][1] == 1);
            Assert.IsTrue(n[2][2] == 2);

            data = @"
array:
- 0
- - 0
  - 1
  - 2
-   - 0
    - 1
    - 2";

            n = Parser.Parse(data);

            Assert.IsTrue(n[0] == 0);

            Assert.IsTrue(n[1][0] == 0);
            Assert.IsTrue(n[1][1] == 1);
            Assert.IsTrue(n[1][2] == 2);

            Assert.IsTrue(n[2][0] == 0);
            Assert.IsTrue(n[2][1] == 1);
            Assert.IsTrue(n[2][2] == 2);

            data = @"
array:
- 0
- - 0
  - 1
  - - 0
    - 1
- - 0
  - 1";

            n = Parser.Parse(data);

            Assert.IsTrue(n[0] == 0);

            Assert.IsTrue(n[1][0] == 0);
            Assert.IsTrue(n[1][1] == 1);

            Assert.IsTrue(n[2][0] == 0);
            Assert.IsTrue(n[2][1] == 1);

            Assert.IsTrue(n[1][2][0] == 0);
            Assert.IsTrue(n[1][2][1] == 1);

            data = @"
array:
- 0
- - 0
  - - 0
    - 1";

            Assert.IsTrue(n[1][1][1] == 1);
        }

        [TestMethod()]
        public void MixedObjectsAndArray() {
            string data = @"
objects:
- object:
    nestedObject:
      key: value
    array:
    - 0
    - 1
- object:
    array:
    - 0
    - 1
    nestedObject:
      key: value
- object:
    array:
      - 0
      - 1
    nestedObject:
      key: value
key: value";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["objects"].Count == 4);
            Assert.IsTrue(n["key"] == "value");

            Assert.IsTrue(n["objects"][0]["object"]["nestedObject"]["key"] == "value");
            Assert.IsTrue(n["objects"][0]["object"]["array"][0] == 0);
            Assert.IsTrue(n["objects"][0]["object"]["array"][1] == 1);

            Assert.IsTrue(n["objects"][1]["object"]["nestedObject"]["key"] == "value");
            Assert.IsTrue(n["objects"][1]["object"]["array"][0] == 0);
            Assert.IsTrue(n["objects"][1]["object"]["array"][1] == 1);

            Assert.IsTrue(n["objects"][2]["object"]["nestedObject"]["key"] == "value");
            Assert.IsTrue(n["objects"][2]["object"]["array"][0] == 0);
            Assert.IsTrue(n["objects"][2]["object"]["array"][1] == 1);

            data = @"
objects:
- object:
    array:
    - 0";

            n = Parser.Parse(data);
            Assert.IsTrue(n["objects"][0]["object"]["array"][0] == 0);
        }
    }
}