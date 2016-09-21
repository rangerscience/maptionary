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
    }
}