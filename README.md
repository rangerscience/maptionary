# Maptionary
C# library for parsing JSON, XML, and YAML into a super convenient to use data structure. Built for rapid prototyping against flexible or unfamilar data.

Does basics of YAML, JSON and XML - objects and arrays. Doesn't handle the "advanced" stuff: tags, attributes, references, multiline strings, etc. Maybe later!

You may also find the `Node` data structure useful independent of the parsing and serialization capabilities of Maptionary.

`using Maptionary;` is the correct directive.

## Maptionary.Node
This is a C# datastructure designed to mimic the behaviour of Ruby, Javascript and Python hashes. That is, it's really unsafe - I mean convenient! 
It's literally a `Dictionary<String, Node>` with some handy operating overloading for massive convenience. 
Mostly, you don't need to worry about node existence, pretty much ever, or what "type" the node's value is.

Check it out:

```C#
Node n = new Node();

//Empty nodes are "falsey"
if(n) {
  //You'll never get here!
}

//and you don't have to worry about whether a key exists or not to use it:
if(n["key"]) {
  // You'll never get here! Non-existent nodes are falsey
}
n["key"] = "value"
n["key"] = "value2"

//...even if it's deeper in the tree:
if(! n["new key"]["new nested key"]["inception!"]) {
  // Since it doesn't exist yet,  n["new key"]["new nested key"]["inception!"] is "falsey"
  
  // But none of the nodes in the chain need to exist for you to assign to them:
  n["new key"]["new nested key"]["inception!"] = "value"
}
/* NOTE: Super important! 
 * Non-existent nodes that get referenced (like in the above examples) are caused to 'exist' (but by empty, and thus falsey)
 * and so n.ContainsKey("other key") will be true, even if ((bool) n["other key"]) is false.
 * This is a side effect of how assigning to nested non-existent keys works.
 */

// Node with content (whether that's deeper nodes, or "leaf" values) are "truthey"
if(n["key"] && n["new key"]) {
 // You'll get here!
}

// Nodes also know how to do numbers, both for keys and values:
n[1.23] = 0.67;
n[6] = n[1.23] + 1;

// and won't throw errors if you use them wrong:
n["key"] = "value";
n[2] = n["key"] + 1;
// Currently, nonexistent / empty nodes, and nodes that aren't numbers, evaluate to 0, but that's subject to change if a better idea comes along.

// Nodes can output to YAML, XML and JSON:
n.ToYAML();
n.ToJSON();
n.ToXML();
// XML and JSON will come out "pretty-printed". There's currently no way to get compacted XML or JSON from Maptionary.

// Note that if you manually assign a node like it's an array:
n["array"][0] = 0;
n["array"][1] = 1;
n["array"][2] = 2;

//you need to tell Maptionary it's an array:
n["array"].isArray = true;

//in order to serialize out into an array
n["array"].ToJSON() == "[
  0,
  1,
  2
]";

//Otherwise you'll get an object with numerical keys:
n["array"].ToYAML() == @"---
0: 0
1: 1
2: 2";

// Finally, it's really easy to iterate over the contents of a node:
foreach(KeyValuePair<string, Node> _n in n) {
  //...
}
```

For a really detailed look at what happens in a given situation, check the test cases in NodeTests.

## Maptionary.Parse

Maptionary's `Parse` function is trivial to use, decently fast, and can handle just about any combination of (basic) JSON, XML and YAML you throw at it

Usage is simple, pass in the XML/JSON/YAML string, and you get out a `Node`:
```C#
Node n = Maptionary.Parse("key: value");
```
You don't even need to worry about which formats are present in the string; Maptionary will handle that for you!

The basics work in each format:
```YAML
objects:
- object1:
    nestedObject:
      key: value1
    array:
      - 1a
      - 1b
  key: value
```

```Javascript
{
  "objects":
    [
      {
        'object1': {
          'nestedObject': {
            'key': "value1"
          },
          'array': [
            '1a',
            '1b'
          ]
        },
        'key': 'value'
      },
    ],
}
```
Note 1: For ease of use when typing JSON into C# code, Maptionary supports both single and double quotes in JSON.

Note 2: It also doesn't care about commas at all, /except/ for naked array elements: `[1, 2, 3]` is valid. `[1 2 3]` isn't.

```XML
<object1>
  <key1>value11</key1>
  <innerObject1>
    <key1>value111</>
    <key2>value112</>
    <key3>value113</>
  </>
</>
<key>value</>
```
Note 1: Maptionary lets you close XML tags with a generic closing tag - "</>"

Note 2: Since XML doesn't have "basic" arrays (you have to use the advanced features of XML to make it work), you can't do arrays in XML with Maptionary

Note 3: My understanding of XML is not the greatest. I may have gotten bits wrong.


You can even mix and match XML, JSON, and YAML!

Here's an example with YAML inside JSON inside XML:
```
<data>
  ---
  data:  
    {
      'key': 'value'
    }
</>
```

Finally, when in doubt, wrap it in quotes:
```YAML
---
'http://github.com': "That's a nice pinata!"
```

For a much more detailed idea of what's possible, check the test cases.

## Future?

* YAML references
* Deserialization into C# objects
* Ways to validate a node structure against a data schema
* XML attributes
* "Strict" parsing - throw errors when data strings aren't formated correctly
* XML comments
* Timestamps!
