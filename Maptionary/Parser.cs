﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maptionary {
    public class Parser {
        public static Node Parse(string data) {
            int i = 0;
            return YAML(ref data, ref i);
        }

        // Avoid allocations of strings wherever possible, so "cache" the keystrings:
        const string colon = ":";
        const string dash = "-";
        const string newline = "\n";

        static void ReadNextYAMLToken(ref string data, ref int i, out string token) {
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
                    token = data.Substring(i, _i - i); // TODO: This allocates an unnecessary string, but... might not decent method that avoids this allocation
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

        static Node YAML(ref string data, ref int i) {

            // Re-use the variable, to avoid string allocations and resulting GC
            string token = null;
            string priorToken = null;
            Node n = new Node();
            Node root = n;

            while (i < data.Length) {

                ReadNextYAMLToken(ref data, ref i, out token);
                i += token.Length; // Advance our counter past the token

                // Colon, dash, newline, whitespace, string - these are the possible tokens
                // (Remember that numbers are treated as strings)

                if (token == colon) {
                    //TODO: Error checking! If there's no priorToken, our presumptive logic doesn't work.
                    // Presuming correct YAML, the priorToken is, logically, a key
                    n[priorToken] = new Node();
                    n[priorToken].parent = n;
                    n = n[priorToken];

                    priorToken = token;
                } else if (token == dash) {

                } else if (token == newline) {
                    if (priorToken == dash) {
                        // Ignore this newline, we're in an array object, and it's allowed to start on the next line
                    } else { 
                        // Whatever the result of this is, it'll be handled by the next token.
                        priorToken = token;
                    }
                } else if (token.Trim().Length == 0) { // Although there are better methods, we need to be compatible with the Unity version of C#
                    if(priorToken == colon || priorToken == dash) {
                        // Ignore this whitespace, we're between a colon or dash and the start of the object
                    }
                } else {
                    // Handle an edge case - although we might want to do this in the read token, then we don't know the correct token size :P
                    if (token[0] == '"' || token[0] == '\'') { 
                        token = token.Substring(1, token.Length - 2);
                    }

                    if (priorToken == colon) {
                        // Logically, this is leaf node, and the current token is the value
                        n.leaf = token;
                    } else if (priorToken == newline) {
                        // We're on a new key for the root object (since there's no whitespace after the newline, we know we're at the root level)
                        // Since our current node is, logically, a value node, go back up one on the tree.
                        // But, since our data string can start with newlines (and have double newlines), don't go above root.
                        if(n != root) { 
                            n = n.parent;
                        }
                    }


                    priorToken = token;
                }
            }

            return root;
        }
    }
}
