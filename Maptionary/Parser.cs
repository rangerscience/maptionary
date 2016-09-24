using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maptionary {
    public class Parser {
        public static Node Parse(string data) {
            int i = 0;
            Node root = new Node();

            //Skip leading newlines
            while (i < data.Length && (data[i] == '\n' || data[i] == '\r')) {
                i++;
            }


            //Peak at the first non-whitespace
            int _i = i;
            while(_i < data.Length && data[_i] == ' ') {
                _i++;
            }

            switch(data[_i]) {

                case '<':
                    //XML
                    XML(ref data, ref i, ref root);
                    break;

                case '{':
                case '[':
                    // JSON control characters
                    JSON(ref data, ref i, ref root);
                    break;
                
                case '-':
                    //YAML. Either the start of an array, or the start of the document signifier (---)
                default:
                    // Naked character. That means YAML!
                    YAML(ref data, ref i, ref root);
                    break;
            }

            return root;
        }

        // Avoid allocations of strings wherever possible, so "cache" the keystrings:
        const string colon = ":";
        const string dash = "-";
        const string newline = "\n";
        const string whitespace = " ";

        const string startCurly = "{";
        const string endCurly = "}";
        const string startBracket = "[";
        const string endBracket = "]";
        const string comma = ",";

        const string openCarrot = "<";
        const string closeCarrot = ">";
        const string endObject = "</";

        //*********** YAML *************
        static void ReadNextYAMLToken(ref string data, ref int i, out string token) {
            if (i >= data.Length) {
                //End of string? End of line.
                token = newline;
                return;
            }

            char c = data[i];
            int _i = i; // Used for measuring whitespace and string token sizes
            switch (c) {                
                case ':':
                    token = colon;
                    break;
                case '-':
                    token = dash;
                    break;
                case '\r':
                case '\n':
                    token = newline;
                    break;
                case '\'':
                case '"':
                    //This token will continue until the next quotation mark, no matter what - excepting end of data string!
                    char sym = data[i]; // Same behavior for each
                    _i++; //Advance past this current quotation mark
                    while (_i < data.Length && data[_i] != sym) {
                        _i++;
                    }
                    _i++; //Advance past the final quotation mark
                    token = data.Substring(i, _i - i);
                    break;
                case ' ':
                    while (_i < data.Length && data[_i] == ' ') {
                        _i++;
                    }
                    token = data.Substring(i, _i - i); // TODO: This allocates an unnecessary string, but... might not be a decent method that avoids this allocation
                    break;
                default:
                    // Only newlines and colons break the symbol - and the end of the data string!
                    while (_i < data.Length &&
                        data[_i] != '\n' &&
                        data[_i] != '\r' &&
                        data[_i] != ':') {
                        _i++;
                    }
                    token = data.Substring(i, _i - i);
                    break;
            }
        }

        static void YAML(ref string data, ref int i, ref Node root) {

            // We need our own reference to the current node, since we'll move it around, but need the root ref to stay the same
            Node n = root;

            // Re-use the variable, to avoid string allocations and resulting GC
            string token = null;

            // TODO: Explain why we want to set priorToken to newline
            string priorToken = "\n";

            int indentLevel = 0;
            Node[] levels = new Node[100]; // Handle 50 levels deep inherently.
            levels[0] = root;

            while (i < data.Length) {

                //Peak at the next symbol, in case it involves switching to a different format.
                if (data[i] == '{' || data[i] == '[') {
                    //JSON!
                    JSON(ref data, ref i, ref n);
                } else if (data[i] == '<') {
                    //XML!
                    XML(ref data, ref i, ref n);
                }

                ReadNextYAMLToken(ref data, ref i, out token);
                i += token.Length; // Advance our counter past the token

                // Colon, dash, newline, whitespace, string - these are the possible tokens
                // (Remember that numbers are treated as strings)

                //TODO: ===?
                if (token == colon) {
                    //TODO: Error checking! If there's no priorToken, our presumptive logic doesn't work.
                    // Presuming correct YAML, the priorToken is, logically, a key
                    n[priorToken] = new Node();
                    n[priorToken].parent = n;
                    n = n[priorToken];

                    priorToken = colon;
                } else if (token == dash) {
                    //TODO: Nodes should know they're an array
                    // A dash is basically "fill in this key with an array index, then a colon"

                    //Arrays have an annoying edge case where there's a non-indented array appearing to be at "root" level - aka, "\n- value"
                    //We have consider a case where there's multiple such arrays as values in the root node - "array1:\n- 1a\narray2:\n- 2a"
                    //We can tell if we're a NEW node, because the recent hit of a ":" has left us with an empty `n`.
                    if (priorToken == newline && n.Keys.Count() == 0) {
                        //Our situation is otherwise fine, but we need to set the level[1] node to ourselves, so we can return correctly after de-nesting
                        levels[1] = n;
                        indentLevel = 0; //Anything else in this node will have zero whitespace scoping
                                         //Next, this node starts getting filled as an array.
                                         //(Realize that the current "n" is the value node for whatever key recently preceded the colon that preceded the newline that's in priorToken)

                        //We can well if we're RETURNING to such a node, simply because we're in a "\n-" situation.
                    } else if (priorToken == newline) {
                        n = levels[1];
                        indentLevel = 0; //Anything else in this node will have zero whitespace scoping
                    }


                    string key = n.Keys.Count().ToString(); // 0 keys gets key '0', 1 key gets key '1' (so, say, '0' and '1')
                    n[key] = new Node();
                    n[key].parent = n;
                    n = n[key];

                    // So, we check to see if we're a "\n-" situation, AND this is a new node (presumable, freshly created by handling the ':')
                    // TODO: Numerical "ContainsKey"


                    priorToken = dash;
                } else if (token == newline) {
                    if (priorToken == dash) {
                        // Ignore this newline, we're in an array object, and it's allowed to start on the next line
                        //} else if (priorToken == colon) {
                        // Beginning a new object, ignore this newline
                    } else {
                        // Whatever the result of this is, it'll be handled by the next token.
                        priorToken = newline;
                    }

                } else if (token[0] == ' ' && token.Trim().Length == 0) { // Although there are better methods, we need to be compatible with the Unity version of C#

                    if (priorToken == colon || priorToken == dash) {
                        // Ignore this whitespace, we're between a colon or dash and the start of the object

                    } else if (priorToken == newline) {
                        int _indentLevel = token.Length;

                        // Treat arrays as 1 deeper than they are (not a full indent, which would be 2, but 1)
                        // This nicely handles both indented and non-indented arrays
                        string nextToken;
                        int _i = i;
                        ReadNextYAMLToken(ref data, ref _i, out nextToken);
                        if (nextToken == dash) {
                            _indentLevel += 1;
                        }

                        if (_indentLevel < indentLevel) {
                            // End of an object
                            priorToken = whitespace;
                            n = levels[_indentLevel];
                            indentLevel = _indentLevel;
                        } else if (_indentLevel > indentLevel) {
                            // Entering an object
                            levels[_indentLevel] = n;
                            indentLevel = _indentLevel;
                            priorToken = whitespace;
                        } else {
                            // Continuing an object
                            priorToken = whitespace;
                            n = levels[_indentLevel];
                        }
                    }
                } else {
                    // Handle an edge case - although we might want to do this in the read token, then we don't know the correct token size :P
                    if (token[0] == '"' || token[0] == '\'') {
                        token = token.Substring(1, token.Length - 2);
                    }

                    if (priorToken == colon) {
                        // Logically, this is leaf node, and the current token is the value
                        n.leaf = token;
                        if (n.parent != null) {
                            n = n.parent;
                            //TODO: Indent level?
                        }
                    } else if (priorToken == dash) {
                        // Logically, this is leaf node, and the current token is the value... unless the next token is a colon.

                        string nextToken;
                        int _i = i;
                        ReadNextYAMLToken(ref data, ref _i, out nextToken);
                        if (nextToken == colon) {
                            //Mostly let things play out normally, but since we skipped handling the whitespace, we need to pretent to do that here.
                            // Anything else inside this array element will be at the current indentLevel, plus 2
                            int _indentLevel = indentLevel + 2;
                            levels[_indentLevel] = n;
                            indentLevel = _indentLevel;
                        } else {
                            n.leaf = token;
                            if (n.parent != null) {
                                n = n.parent;
                                //TODO: Indent level?
                            }
                        }
                    } else if (priorToken == whitespace) {
                        //Nothing special
                    } else if (priorToken == newline) {
                        // We're on a new key for the root object (since there's no whitespace after the newline, we know we're at the root level)
                        n = levels[0];
                        indentLevel = 0;
                    }

                    priorToken = token;
                }
            }
        }


        //*********** JSON *************
        static void ReadNextJSONToken(ref string data, ref int i, out string token) {
            if (i >= data.Length) {
                //End of string? End of line.
                token = newline;
                return;
            }

            char c = data[i];
            int _i = i; // Used for measuring whitespace and string token sizes

            switch (c) {
                case ':':
                    token = colon;
                    break;

                case '{':
                    token = startCurly;
                    break;
                case '}':
                    token = endCurly;
                    break;

                case '[':
                    token = startBracket;
                    break;
                case ']':
                    token = endBracket;
                    break;

                case ',':
                    token = comma;
                    break;

                case '\r':
                case '\n':
                    token = newline;
                    break;

                case '\'':
                case '"':
                    //This token will continue until the next quotation mark, no matter what - excepting end of data string!
                    char sym = data[i]; // Same behavior for each
                    _i++; //Advance past this current quotation mark
                    while (_i < data.Length && data[_i] != sym) {
                        _i++;
                    }
                    _i++; //Advance past the final quotation mark
                    token = data.Substring(i, _i - i);
                    break;

                case ' ':
                    //Although JSON ignores whitespace, it still needs to know how far to advance i
                    while (_i < data.Length && data[_i] == ' ') {
                        _i++;
                    }
                    token = data.Substring(i, _i - i); // TODO: This allocates an unnecessary string, but... might not decent method that avoids this allocation
                    break;

                default:
                    // There are some JSON data types that aren't quoted (like numbers), but they all get terminated by commas, curlies, and brackets.
                    // TODO Verify ^^
                    while (_i < data.Length &&
                        data[_i] != ',' &&
                        data[_i] != '{' && data[_i] != '}' &&
                        data[_i] != '[' && data[_i] != ']') {
                        _i++;
                    }
                    token = data.Substring(i, _i - i);
                    break;
            }
        }

        //TODO: Copypasta from YAML parsing
        static void JSON(ref string data, ref int i, ref Node root) {
            // Re-use the variable, to avoid string allocations and resulting GC
            string token = null;
            string priorToken = null;
            Node n = root;

            while (i < data.Length) {

                ReadNextJSONToken(ref data, ref i, out token);
                i += token.Length; // Advance our counter past the token

                if (token == startCurly || token == startBracket) {

                    //Check if this is actually the start of an object (or array) as an element of an array.
                    if (n.isArray) {
                        Node _n = new Node();
                        _n.parent = n;
                        _n.leaf = token;
                        string key = n.Count().ToString();
                        n[key] = _n;

                        //...and recurse into the new node
                        n = _n;
                    }

                    //Check if this is the start of an array node
                    // (note that this is not an 'else' with the above, in order to handle arrays of arrays)
                    if (token == startBracket) {
                        //Note that we've entered an array node
                        n.isArray = true;
                    }

                    //Otherwise, there's really nothing special to do, just note that the next symbol isn't a leaf node value
                    priorToken = token;
                } else if (token == endCurly || token == endBracket) {
                    //End of node, go up one.
                    //TODO: Error handling
                    if (n.parent != null) {
                        n = n.parent;
                    }
                } else if (token == colon) {
                    n[priorToken] = new Node();
                    n[priorToken].parent = n;
                    n = n[priorToken];

                    priorToken = colon;
                } else if (token == comma) {
                    //So far, we're well handled by everything else...
                } else if (
                    token[0] == ' ' && token.Trim().Length == 0 ||
                    token == newline
                    ) {
                    //Whitespace! Ignore it.
                } else {
                    //This is an edge case for YAML, but the command situation for JSON. Still, do need to trim the quotes off:
                    if (token[0] == '"' || token[0] == '\'') {
                        token = token.Substring(1, token.Length - 2);
                    }

                    if (priorToken == colon) {
                        // Logically, this is leaf node, and the current token is the value
                        n.leaf = token;
                        if (n.parent != null) {
                            n = n.parent;
                            //TODO: Indent level?
                        }
                    } else if (n.isArray) {
                        // Leaf node in an array
                        Node _n = new Node();
                        _n.parent = n;
                        _n.leaf = token;
                        string key = n.Count().ToString();
                        n[key] = _n;
                    } else {
                        //Nothing special
                    }

                    priorToken = token;
                }
            }
        }

        //*********** XML *************
        static void ReadNextXMLToken(ref string data, ref int i, out string token) {
            if (i >= data.Length) {
                //End of string? End of line.
                token = newline;
                return;
            }

            char c = data[i];
            int _i = i; // Used for measuring whitespace and string token sizes
            switch (c) {
                case '<':
                    if( (i+1) < data.Length && data[i+1] == '/' ) {
                        //Read to next >
                        token = endObject;
                    } else {
                        token = openCarrot;
                    }
                    break;
                case '>':
                    token = closeCarrot;
                    break;

                case '\'':
                case '"':
                    //This token will continue until the next quotation mark, no matter what - excepting end of data string!
                    char sym = data[i]; // Same behavior for each
                    _i++; //Advance past this current quotation mark
                    while (_i < data.Length && data[_i] != sym) {
                        _i++;
                    }
                    _i++; //Advance past the final quotation mark
                    token = data.Substring(i, _i - i);
                    break;

                default:
                    //I think only carrots terminate these tokens
                    while (_i < data.Length &&
                        data[_i] != '<' &&
                        data[_i] != '>'
                        ) {
                        _i++;
                    }
                    token = data.Substring(i, _i - i);
                    break;
            }
        }


        static void XML(ref string data, ref int i, ref Node root) {
            // Re-use the variables, to avoid string allocations and resulting GC
            string token = null;
            string priorToken = null;
            Node n = root;
            Node _n = new Node(); //This is a junk node, but to avoid compiler errors, need to create it

            while (i < data.Length) {

                ReadNextXMLToken(ref data, ref i, out token);
                i += token.Length; // Advance our counter past the token

                if (token == openCarrot) {
                    //We're going to have a new node.
                    _n = new Node();
                    _n.parent = n;
                    priorToken = token;
                } else if (token == closeCarrot) {
                    //  Now we're in the node, unless we just finished ending an object.
                    if (priorToken == endObject) {
                        //Meh. Nothing to do, but remember we hit a close carrot.
                        priorToken = token;
                    } else {
                        n = _n;
                        priorToken = token;
                    }
                } else if (token == endObject) {
                    n = n.parent;
                    priorToken = token;
                } else {
                    //Edge case!
                    // TODO: What about <key>"value"</> ? Should we still strip quotes?
                    if (token[0] == '"' || token[0] == '\'') {
                        token = token.Substring(1, token.Length - 2);
                    }

                    if (priorToken == openCarrot) {
                        //Key value! Stick the new node in with this key.
                        n[token] = _n;
                    } else if (priorToken == closeCarrot) {
                        // Value for the node!
                        n.leaf = token;
                    } else if (priorToken == endObject) {
                        //Closing tag. Notionally, matches an opening tag, but eh. Just ignore it.
                    }
                }
            }
        }
    }
}
