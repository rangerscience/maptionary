using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maptionary
{
    public class Node : Dictionary<string, Node>
    {
        public string leaf = null;
        public Node parent = null;
        public bool isArray = false; //TODO: Access controls? 
        //TODO: Also, features for using this, although right now it's just used for JSON parsing

        public Node this[string key] {
            get {
                //Safely get the value at the key, so we don't throw errors if the key doesn't exist.
                Node result = new Node();
                TryGetValue(key, out result);

                //Turn non-existent keys into empty nodes on access, to support chain assignment of new keys
                // aka - n[1][2] = 3
                if(result == null) {
                    Node n = new Node();
                    this[key] = n;
                    return n;
                } else {
                    return result;
                }
            }
            set {
                if(ContainsKey(key)) {
                    this.Remove(key);
                }
                this.Add(key, value);
            }
        }
        //TODO: Copypasta. Is there a better way?
        public Node this[int key] {
            get { 
                return this[key.ToString()];
            }
            set {
                this[key.ToString()] = value;
            }
        }
        public Node this[float key] {
            get {
                return this[key.ToString()];
            }
            set {
                this[key.ToString()] = value;
            }
        }
        public Node this[double key] {
            get {
                return this[key.ToString()];
            }
            set {
                this[key.ToString()] = value;
            }
        }

        public static implicit operator bool(Node n)
        {
            if(n.leaf != null) {
                return true;
            }
            foreach(Node _n in n.Values) {
                //If any node in the structure has a leaf value, the structure is truthy.
                if(_n) {
                    return true;
                }
            }

            return false;
        }

        public static implicit operator Node(string s) {
            Node n = new Node();
            n.leaf = s;
            return n;
        }

        public static implicit operator string(Node n) {
            if(n.leaf != null) {
                return n.leaf;
            }
            return null;
        }

        //Convert the number to a string, then a node
        public static implicit operator Node(int n) {
            return (Node) n.ToString();
        }
        public static implicit operator Node(float n) {
            return (Node)n.ToString();
        }
        public static implicit operator Node(double n) {
            return (Node)n.ToString();
        }

        //Convert the node to a number
        //TODO: Better default value?
        //TODO: Copy pasta?
        //TODO: Keep numbers as numbers, internally? (maybe using "val" instead of "string"?)
        public static implicit operator int(Node n) {
            //Node must be a leaf node
            int num;
            if (n.leaf != null) {
                if (int.TryParse(n.leaf, out num)) {
                    return num;
                }
            }
            return 0;
        }
        public static implicit operator double(Node n) {
            //Node must be a leaf node
            double num;
            if (n.leaf != null) {
                if (double.TryParse(n.leaf, out num)) {
                    return num;
                }
            }
            return 0;
        }
        public static implicit operator float(Node n) {
            //Node must be a leaf node
            float num;
            if (n.leaf != null) {
                if (float.TryParse(n.leaf, out num)) {
                    return num;
                }
            }
            return 0;
        }

        public string ToYAML() {
            return "---" + _ToYAML("");
        }

        string _ToYAML(string level) {
            if (leaf != null) {
                return " " + EscapeYAML(this.leaf);
            } else if (isArray) {
                string s = "";
                foreach (KeyValuePair<String, Node> entry in this) {
                    s += "\n" + level + "-" + entry.Value._ToYAML(level + "  ");
                }
                return s;
            } else {
                string s = "";
                foreach(KeyValuePair<String, Node> entry in this) {
                    s += "\n" + level + EscapeYAML(entry.Key) + ":" + entry.Value._ToYAML(level + "  ");
                }
                return s;
            }
        }

        public static string EscapeYAML(string s) {
            if(
                s.Contains(':') ||
                s.Contains('\n') ||
                s.Contains('\r') ||
                s[0] == ' ' || s[s.Length-1] == ' ') {
                return "\"" + s + "\"";
            }
            return s;
        }

        //TODO: Test serialization of empty nodes!
        public string ToJSON() {
            return _ToJSON("").TrimStart('\n'); //Remove the leading newline
        }

        string _ToJSON(string level) {
            if(leaf != null) {
                return " " + EscapeJSON(leaf);
            } else {
                string s = "\n" + level + "{";
                foreach (KeyValuePair<String, Node> entry in this) {
                    s += "\n  " + level + EscapeJSON(entry.Key) + ":" + entry.Value._ToJSON(level + "    ") + ",";
                }
                if (s.Length > 1) {

                    //Remove the trailing comma (aka, the last character), then close the object
                    return s.Substring(0, s.Length - 1) + "\n" + level + "}";
                } else {
                    //Empty object
                    return "{}";
                }
            }
        }

        //Being real lazy and ALSO using this to wrap the necessary quotes around the keys and values
        public static string EscapeJSON(string s) {
            return "\"" + s + "\"";
        }
    }
}