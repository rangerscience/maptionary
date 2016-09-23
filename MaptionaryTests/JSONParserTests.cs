using Microsoft.VisualStudio.TestTools.UnitTesting;
using Maptionary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maptionary.Tests {
    [TestClass()]
    public class JSONParserTests {

        // For the most part, the JSON tests are exactly the same as the YAML tests
        // Differences occur in the edge cases tested, but the basic data structures that need to be tested
        //   are the same. (For example, both support arrays, but only YAML has different ways to scope arrays)

        [TestMethod()]
        public void Simplest() {
            string data = @"{""key"": ""value""}";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["key"] == "value");
        }

        [TestMethod()]
        public void SingleQuotes() {
            //Although JSON spec may require double quotes, those are really annoying to type in C#, so we allow single quotes.
            // This feature is meant for hand-written JSON - generated JSON, etc, will use double quotes as required by the spec.
            string data = @"{'key': 'value'}";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["key"] == "value");
        }

        [TestMethod()]
        public void Flat() {
            string data = @"
{
  'key1': 'value1',
  'key2': 'value2',
  'key3': 'value3'
}";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["key1"] == "value1", "Flat key doesn't have correct value: '" + n["key1"] + "'");
            Assert.IsTrue(n["key2"] == "value2");
            Assert.IsTrue(n["key3"] == "value3");
        }

        [TestMethod()]
        public void Flat_SingleLine() {
            string data = @"{ 'key1': 'value1', 'key2': 'value2', 'key3': 'value3' }";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["key1"] == "value1", "Flat key doesn't have correct value: '" + n["key1"] + "'");
            Assert.IsTrue(n["key2"] == "value2");
            Assert.IsTrue(n["key3"] == "value3");
        }

        //TODO: Are quoted numbers strings? Do we care?
        //TODO: Is '.6' legal, or does it need the leading zero?
        [TestMethod()]
        public void NakedNumbers() {
            string data = @"
{
  'key1': 1,
  'key2': 1.23,
  'key3': .6
}";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["key1"] == 1, "Key doesn't have correct numerical value: '" + n["key1"] + "'");
            Assert.IsTrue(n["key2"] == 1.23);
            Assert.IsTrue(n["key3"] == 0.6);
        }

        [TestMethod()]
        public void RepeatedKeys() {
            string data = @"
{
  'key': 'value1',
  'key': 'value2'
}
";

            Node n = Parser.Parse(data);

            //TODO: Desired behavior?
            Assert.IsTrue(n["key"] == "value2", "Repeated key doesn't overwrite prior key");
        }

        [TestMethod()]
        public void Quotations() {
            string data = @"
{ 
  'single': ""double"",
  '""doubles in single""': ""'singles in doubles'"",
  'use:colon': 'use\nnewline',
  'key': ' edging whitespace '
}
";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["single"] = "double");
            Assert.IsTrue(n.ContainsKey("\"doubles in single\""), "Node doesn't contain \"doubles in singles\" key");
            Assert.IsTrue(n["\"doubles in single\""] == "'singles in doubles'", "Doubles in singles key has wrong value: " + n["\"doubles in single\""]);
            Assert.IsTrue(n["use:colon"] = "use\nnewline");
            Assert.IsTrue(n["key"] = " edging whitespace ");
        }

        [TestMethod()]
        public void Object() {
            string data = @"
{ 
  'object': 
  {
    'key1': 'value1',
    'key2': 'value2',
    'key3': 'value3',
  },
  'key': 'value'
}
";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["object"]["key1"] == "value1");
            Assert.IsTrue(n["object"]["key2"] == "value2");
            Assert.IsTrue(n["object"]["key3"] == "value3");
            Assert.IsTrue(n["key"] == "value");
        }

        [TestMethod()]
        public void Object_KRBraces() {
            string data = @"
{ 
  'object': {
    'key1': 'value1',
    'key2': 'value2',
    'key3': 'value3',
  },
  'key': 'value'
}
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
{
  'object1': {
    'key1': 'value1',
    'key2': 'value2',
    'key3': 'value3',
  },
  'object2': {
    'key1': 'value3'
    'key2': 'value4'
    'key3': 'value5'
  },
  'key': 'value',
}
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
            string data = @"{
'object1': {
  'key1': 'value11',
  'innerObject1': {
    'key1': 'value111',
    'key2': 'value112',
    'key3': 'value113'
   },
  'innerObject2': {
    'key1': 'value121',
    'key2': 'value122',
    'key3': 'value123'
  },
  'key2': 'value12',
  'key3': 'value13',
  'innerObject3': {
    'key1': 'value131',
    'key2': 'value132',
    'key3': 'value133',
  }
},
'object2': {
  'key1': 'value21',
  'level2Object1': {
    'key': 'value',
    'level3Object1': {
      'level4Object1': {
        'key': 'value'
       },
      'key': 'value',
      'level4Object2': {
        'key': 'value'
      }
    },
    'level3Object2': {
      'key': 'value'
    }
  },
  'level2Object2': {
    'key': 'value'
  },
  'key2': 'value22',
  'key3': 'value23'
}
'key': 'value'
}";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["object1"]["key1"] == "value11");
            Assert.IsTrue(n["object1"]["key2"] == "value12");
            Assert.IsTrue(n["object1"]["key3"] == "value13");

            Assert.IsTrue(n["object2"]["key1"] == "value21");
            Assert.IsTrue(n["object2"]["key2"] == "value22");
            Assert.IsTrue(n["object2"]["key3"] == "value23");

            Assert.IsTrue(n["key"] == "value");

            Assert.IsTrue(n["object1"]["innerObject1"]["key1"] == "value111");
            Assert.IsTrue(n["object1"]["innerObject1"]["key2"] == "value112");
            Assert.IsTrue(n["object1"]["innerObject1"]["key3"] == "value113");

            Assert.IsTrue(n["object1"]["innerObject2"]["key1"] == "value121");
            Assert.IsTrue(n["object1"]["innerObject2"]["key2"] == "value122");
            Assert.IsTrue(n["object1"]["innerObject2"]["key3"] == "value123");

            Assert.IsTrue(n["object1"]["innerObject3"]["key1"] == "value131");
            Assert.IsTrue(n["object1"]["innerObject3"]["key2"] == "value132");
            Assert.IsTrue(n["object1"]["innerObject3"]["key3"] == "value133");

            Assert.IsTrue(n["object2"]["level2Object1"]["key"] == "value");
            Assert.IsTrue(n["object2"]["level2Object2"]["key"] == "value");
            Assert.IsTrue(n["object2"]["level2Object1"]["level3Object1"]["key"] == "value");
            Assert.IsTrue(n["object2"]["level2Object1"]["level3Object2"]["key"] == "value");
            Assert.IsTrue(n["object2"]["level2Object1"]["level3Object1"]["level4Object1"]["key"] == "value");
            Assert.IsTrue(n["object2"]["level2Object1"]["level3Object1"]["level4Object2"]["key"] == "value");
        }

        [TestMethod()]
        public void BasicArray() {
            string data = @"{
'array': [
  'a',
  'b',
  'c'
],
'key': 'value'
}";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["array"][0] == "a");
            Assert.IsTrue(n["array"][1] == "b");
            Assert.IsTrue(n["array"][2] == "c");
            Assert.IsTrue(n["key"] == "value");
        }

        [TestMethod()]
        public void BasicArray_SingleLine() {
            string data = @"{
'array': [ 'a', 'b', 'c' ],
'key': 'value'
}";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["array"][0] == "a");
            Assert.IsTrue(n["array"][1] == "b");
            Assert.IsTrue(n["array"][2] == "c");
            Assert.IsTrue(n["key"] == "value");
        }

        [TestMethod()]
        public void BasicArray_SplitLine() {
            string data = @"{
'array': [ 'a', 'b',
'c' ],
'key': 'value'
}";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["array"][0] == "a");
            Assert.IsTrue(n["array"][1] == "b");
            Assert.IsTrue(n["array"][2] == "c");
            Assert.IsTrue(n["key"] == "value");
        }

        [TestMethod()]
        public void RootArray() {

            string data = @"[
'a',
'b',
'c'
]";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n[0] == "a");
            Assert.IsTrue(n[1] == "b");
            Assert.IsTrue(n[2] == "c");
        }

        [TestMethod()]
        public void NestedArray() {
            string data = @" {
'array': [
 'a',
  [ 'ba',
    'bb',
    'bc'
  ],
  [ 'ca',
    'cb',
    'cc'
  ],
  'c'
],
'key': 'value'
}";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["array"][0] == "a");

            Assert.IsTrue(n["array"][1][0] == "ba");
            Assert.IsTrue(n["array"][1][1] == "bb");
            Assert.IsTrue(n["array"][1][2] == "bc");

            Assert.IsTrue(n["array"][2][0] == "ca");
            Assert.IsTrue(n["array"][2][1] == "cb");
            Assert.IsTrue(n["array"][2][2] == "cc");

            Assert.IsTrue(n["array"][3] == "c");
            Assert.IsTrue(n["key"] == "value");
        }

        [TestMethod()]
        public void MultipleArrays() {
            string data = @"{
'array1': [
  '1a',
  '1b',
  '1c'
],
'array2': [
  '2a',
  '2b',
  '2c'
],
'key': 'value'
}";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["array1"][0] == "1a");
            Assert.IsTrue(n["array1"][1] == "1b");
            Assert.IsTrue(n["array1"][2] == "1c");

            Assert.IsTrue(n["array2"][0] == "2a");
            Assert.IsTrue(n["array2"][1] == "2b");
            Assert.IsTrue(n["array2"][2] == "2c");

            Assert.IsTrue(n["key"] == "value");
        }

        [TestMethod()]
        public void MixedObjectsAndArray() {
            string data = @"{
'objects':[{
  'object1': {
    'nestedObject': {
      'key': 'value1'
    },
    'array': [
        '1a',
        '1b'
    ],
  'key': 'value'
  },{
  'object2': {
    'nestedObject': {
      'key': 'value2'
    },
    'array': [
      '2a',
      '2b'
    ]
  },
  'key': 'value'
  } , {
  'key': 'value',
  'object3': {
    'array': [
      '3a',
      '3b'
    ],
    'nestedObject': {
      'key': 'value3'
    }
  }
],
'key': 'value'
}";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["objects"].Count == 3);
            Assert.IsTrue(n["key"] == "value");

            Assert.IsTrue(n["objects"][0]["object1"]["nestedObject"]["key"] == "value1");
            Assert.IsTrue(n["objects"][0]["object1"]["array"][0] == "1a");
            Assert.IsTrue(n["objects"][0]["object1"]["array"][1] == "1b");
            Assert.IsTrue(n["objects"][0]["key"] == "value");

            Assert.IsTrue(n["objects"][1]["object2"]["nestedObject"]["key"] == "value2", "Nested key doesn't have correct value: '" + n["objects"][1]["object2"]["nestedObject"]["key"] + "'");
            Assert.IsTrue(n["objects"][1]["object2"]["array"][0] == "2a");
            Assert.IsTrue(n["objects"][1]["object2"]["array"][1] == "2b");
            Assert.IsTrue(n["objects"][1]["key"] == "value");

            Assert.IsTrue(n["objects"][2]["object3"]["nestedObject"]["key"] == "value3");
            Assert.IsTrue(n["objects"][2]["object3"]["array"][0] == "3a");
            Assert.IsTrue(n["objects"][2]["object3"]["array"][1] == "3b");
            Assert.IsTrue(n["objects"][2]["key"] == "value");
        }

        //ABOVE HERE
    }
}