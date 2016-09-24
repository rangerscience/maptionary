using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Maptionary;

namespace MaptionaryTests {
    [TestClass]
    public class MixedFormatTests {
        [TestMethod]
        public void YAMLContainsJSON_Simplest_Flat() {
            string data = @"data:{'key':'value'}";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["data"]["key"] == "value");
        }

        [TestMethod]
        public void YAMLContainsJSON_Simplest_Expanded() {
            string data = @"
data:
  {
    'key':  'value'
  }
";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["data"]["key"] == "value");
        }

        [TestMethod]
        public void YAMLContainsJSONArray_Simplest_Flat() {
            string data = @"data:['value']";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["data"][0] == "value");
        }

        [TestMethod]
        public void YAMLContainsJSONArray_Simplest_Expanded() {
            string data = @"
data:
  [
   'value'
  ]
";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["data"][0] == "value");
        }

        [TestMethod]
        public void YAMLContainsXML_Simplest_Flat() {
            string data = @"data:<key>value</>";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["data"]["key"] == "value");
        }

        [TestMethod]
        public void YAMLContainsXML_Simplest_Expanded() {
            string data = @"
data:
  <key>value</>
";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["data"]["key"] == "value");
        }

        [TestMethod]
        public void JSONContainsXML_Simplest_Flat() {
            string data = @"{'data':<key>value</>}";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["data"]["key"] == "value");
        }

        [TestMethod]
        public void JSONContainsXML_Simplest_Expanded() {
            string data = @"
{ 
  'data':
    <key>value</>
}
";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["data"]["key"] == "value");
        }

        [TestMethod]
        public void JSONContainsYAML_Simplest_Flat() {
            string data = "{'data':---\nkey: value\n}";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["data"]["key"] == "value");
        }

        [TestMethod]
        public void JSONContainsYAML_Simplest_Expanded() {
            string data = @"
{ 
  'data':
    ---
    key: value
}
";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["data"]["key"] == "value");
        }

        [TestMethod]
        public void XMLContainsJSON_Simplest_Flat() {
            string data = @"<data>{'key':'value'}</>";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["data"]["key"] == "value");
        }

        [TestMethod]
        public void XMLContainsJSON_Simplest_Expanded() {
            string data = @"
<data>
  {
    'key':  'value'
  }
</>
";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["data"]["key"] == "value");
        }

        [TestMethod]
        public void XMLContainsJSONArray_Simplest_Flat() {
            string data = @"<data>['value']</>";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["data"][0] == "value");
        }

        [TestMethod]
        public void XMLContainsJSONArray_Simplest_Expanded() {
            string data = @"
<data>
  [
    'value'
  ]
</>
";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["data"][0] == "value");
        }

        [TestMethod]
        public void XMLContainsYAML_Simplest_Flat() {
            string data = "<data>---\nkey: value\n</>"; //YAML requires newlines. Really can't actually be flat.

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["data"]["key"] == "value");
        }

        [TestMethod]
        public void XMLContainsYAML_Simplest_Expanded() {
            string data = @"
<data>
  ---
  key: value
</>
";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["data"]["key"] == "value");
        }

        [TestMethod] 
        public void YAML_XML_JSON_Simple() {
            string data = @"
data:
  <data>
    {
      'key': 'value'
    }
  </>
";
            Node n = Parser.Parse(data);

            Assert.IsTrue(n["data"]["data"]["key"] == "value");
        }

        [TestMethod]
        public void YAML_JSON_XML_Simple() {
            string data = @"
data:
  {
    'data':
      <key>value</>
  }
";
            Node n = Parser.Parse(data);

            Assert.IsTrue(n["data"]["data"]["key"] == "value");
        }

        [TestMethod]
        public void JSON_YAML_XML_Simple() {
            string data = @"
{
  'data':
    ---
      data:
        <key>value</>
}
";
            Node n = Parser.Parse(data);

            Assert.IsTrue(n["data"]["data"]["key"] == "value");
        }

        [TestMethod]
        public void JSON_XML_YAML_Simple() {
            string data = @"
{
  'data':
    <data>
      ---
      key: value
    </>
}
";
            Node n = Parser.Parse(data);

            Assert.IsTrue(n["data"]["data"]["key"] == "value");
        }

        [TestMethod]
        public void XML_JSON_YAML_Simple() {
            string data = @"
<data>
  {
    'data':
        ---
        key: value
  }
</>
";
            Node n = Parser.Parse(data);

            Assert.IsTrue(n["data"]["data"]["key"] == "value");
        }

        [TestMethod]
        public void XML_YAML_JSON_Simple() {
            string data = @"
<data>
  ---
  {
    'key': 'value'
  }
</>
";

            Node n = Parser.Parse(data);

            Assert.IsTrue(n["data"]["data"]["key"] == "value");
        }
    } //ABOVE THIS
}
