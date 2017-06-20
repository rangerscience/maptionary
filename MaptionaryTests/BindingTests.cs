using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Maptionary;
using System.IO;

namespace MaptionaryTests {
    [TestClass]
    public class BindingTests {
        [TestMethod()]
        public void BindNodeToFile() {
            Node n = new Node();

            string filePath = "binding_test1.yml";
            System.IO.File.WriteAllText(filePath, "key: value");

            Bind b = new Bind(filePath);
            
            Assert.IsTrue(b.data["key"] == "value", "Bound node did not read from file");

            System.IO.File.WriteAllText(filePath, "key: updated");
            
            System.Threading.Thread.Sleep(2000);

            //TODO: Wait until update...?
            
            Assert.IsTrue(b.data["key"] == "updated", "Bound node did not update from file:\n" + b.data.ToYAML());
        }

        /*
        [TestMethod()]
        public void BindNodeToHTTP() {
            Node n = new Node();
            TestServer s = new TestServer();
            // TODO: s.Run();
            
            Assert.IsTrue(n.Bind("http://127.0.0.1:5000/binding_test1.yml"), "Could not bind node to HTTP");

            Assert.IsTrue(n["key"] == "value", "Bound node did not read from HTTP");

            Assert.IsTrue(s.update("binding_test1.yml", "key: updated"));

            //TODO: Wait until update...?

            Assert.IsTrue(n["key"] == "value2", "Bound node did not update from HTTP");
        }

        [TestMethod()]
        public void BindNodeToWS() {
            Node n = new Node();
            TestServer s = new TestServer();
            // TODO: s.Run();

            Assert.IsTrue(n.Bind("ws://127.0.0.1:5000/binding_test1.yml"), "Could not bind node to WS");

            Assert.IsTrue(n["key"] == "value", "Bound node did not read from HTTP");

            Assert.IsTrue(s.update("binding_test1.yml", "key: updated"));

            Assert.IsTrue(n["key"] == "value2", "Bound node did not update from HTTP");
        }

        [TestMethod()]
        public void BindFunctionToNode() {
            Node n = new Node();
            string filePath = "binding_test1.yml";
            System.IO.File.WriteAllText(filePath, "key: value");

            string output = "";

            Assert.IsTrue(
                n.Bind((Node _n) => {
                    output = _n["key"];
                }),
                "Could not bind function to node"
            );

            Assert.IsTrue(n.Bind(filePath), "Could not bind node to file");

            Assert.IsTrue(output == "value", "Bound function did not run on file load");

            System.IO.File.WriteAllText(filePath, "key: updated");

            //TODO: Wait until update...?

            Assert.IsTrue(output == "value2", "Bound function did not run on node update");
        }
        */
    }
}
