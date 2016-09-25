using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Maptionary;

namespace MaptionaryTests {
    [TestClass]
    public class Serialization {
        [TestMethod()]
        public void Simple() {
            Node n = new Node();

            n["key"] = "value";
            string data = @"---
key: value".Replace("\r", "");

            Assert.AreEqual(data, n.ToYAML());
        }

        [TestMethod()]
        public void Flat() {
            Node n = new Node();

            n["key1"] = "value1";
            n["key2"] = "value2";
            n["key3"] = "value3";
            string data = @"---
key1: value1
key2: value2
key3: value3".Replace("\r", "");

            Assert.AreEqual(data, n.ToYAML());
        }

        [TestMethod()]
        public void QuotesAndEscapes() {
            Node n = new Node();

            n["'singles'"] = "\"doubles\"";
            n["use:colon"] = "use\nnewline";
            n["key"] = " edging whitespace ";
            string data = @"---
""singles"": ""doubles""
""use:colon"": ""use\nnewline""
key: "" edging whitespace """.Replace("\r", "");

            Assert.AreEqual(data, n.ToYAML());
        }

        [TestMethod()]
        public void Object() {
            Node n = new Node();

            n["object"]["key1"] = "value1";
            n["object"]["key2"] = "value2";
            n["object"]["key3"] = "value3";
            n["key"] = "value";

            string data = @"
object:
  key1: value1
  key2: value2
  key3: value3
key: value".Replace("\r", "");

            Assert.AreEqual(data, n.ToYAML());
        }

        [TestMethod()]
        public void MultipleObjects() {
            Node n = new Node();

            n["object1"]["key1"] = "value1";
            n["object1"]["key2"] = "value2";
            n["object1"]["key3"] = "value3";
            n["object2"]["key1"] = "value4";
            n["object2"]["key2"] = "value5";
            n["object2"]["key3"] = "value6";
            n["key"] = "value";

            string data = @"
object1:
  key1: value1
  key2: value2
  key3: value3
object2:
  key1: value3
  key2: value4
  key3: value5
key: value".Replace("\r", "");

            Assert.AreEqual(data, n.ToYAML());
        }

        [TestMethod()]
        public void BasicArray() {
            Node n = new Node();

            n["array"][0] = "a";
            n["array"][1] = "b";
            n["array"][2] = "c";
            n["key"] = "value";

            string data = @"
array:
- a
- b
- c
key: value".Replace("\r", "");

            Assert.AreEqual(data, n.ToYAML());
        }

        [TestMethod()]
        public void MultipleArrays() {
            Node n = new Node();

            n["array1"][0] = "1a";
            n["array1"][1] = "1b";
            n["array1"][2] = "1c";
            n["array2"][0] = "2a";
            n["array2"][1] = "2b";
            n["array2"][2] = "2c";
            n["key"] = "value";

            string data = @"
array1:
- 1a
- 1b
- 1c
array2:
- 2a
- 2b
- 2c
key: value".Replace("\r", "");

            Assert.AreEqual(data, n.ToYAML());
        }

        [TestMethod()]
        public void MixedObjectsAndArray() {
            Node n = new Node();

            n["objects"][0]["object1"]["nestedObject"]["key"] = "value1";
            n["objects"][0]["object1"]["nestedObject"]["array"][0] = "1a";
            n["objects"][0]["object1"]["nestedObject"]["array"][1] = "1b";
            n["objects"][0]["object1"]["key"] = "value";
            
            n["objects"][1]["object2"]["nestedObject"]["key"] = "value2";
            n["objects"][1]["object2"]["nestedObject"]["array"][0] = "2a";
            n["objects"][1]["object2"]["nestedObject"]["array"][1] = "2b";
            n["objects"][1]["object2"]["key"] = "value";

            n["objects"][2]["object3"]["key"] = "value";
            n["objects"][2]["object3"]["nestedObject"]["array"][0] = "1a";
            n["objects"][2]["object3"]["nestedObject"]["array"][1] = "1b";
            n["objects"][2]["object3"]["nestedObject"]["key"] = "value1";

            string data = @"
objects:
- object1:
    nestedObject:
      key: value1
    array:
    - 1a
    - 1b
  key: value
- object2:
    nestedObject:
      key: value2
    array:
    - 2a
    - 2b
  key: value
- key: value
  object3:
    array:
    - 3a
    - 3b
    nestedObject:
      key: value3
key: value";

            Assert.AreEqual(data, n.ToYAML());
        }

    } // ABOVE THIS
}
