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
    }
}
