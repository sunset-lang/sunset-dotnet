# Syntax highlighting

The following code has been written for the [Monaco editor](https://microsoft.github.io/monaco-editor) to allow syntax highlighting specifically for the Sunset Language.

To see it in action, go to [the Monarch page](https://microsoft.github.io/monaco-editor/monarch.html).

> [!NOTE] This needs to be updated to take into account the new [grammar](grammar.md).

```javascript
// Used in https://microsoft.github.io/monaco-editor/monarch.html for code coloring

return {
  defaultToken: "invalid",

  units: [
    "kg",
    "ug",
    "g",
    "T",
    "m",
    "nm",
    "um",
    "mm",
    "km",
    "s",
    "ms",
    "min",
    "hr",
    "day",
    "month",
    "year",
    "rad",
    "deg",
    "Pa",
    "kPa",
    "MPa",
    "GPa",
    "N",
    "kN",
    "MN",
  ],

  operators: [
    "=",
    "*",
    "+",
    "-",
    "/",
  ],

  brackets: [
    { open: "{", close: "}", token: "delimiter.curly" },
    { open: "[", close: "]", token: "delimiter.bracket" },
    { open: "(", close: ")", token: "delimiter.parenthesis" },
    { open: "<", close: ">", token: "delimiter.angle" },
  ],

  tokenizer: {
    root: [
      { include: "@whitespace" },
      { include: "@numbers" },

      // Units
      // TODO: Work out how to pick out only valid units
      [/\[\w*\]/, "tag"],

      // Symbol shorthand
      // TODO: Work out how to allow ^ and {} in symbol names
      // @symbol
      [/@[a-zA-Z]\w*/, "type"],
      // <symbol>
      [/<(\\?\w*\s?)*>/, "type"],

      // Reference shorthand
      // {reference}
      [/{.*}/, "regexp"],

      // Brackets
      [/[<>{}\[\]()]/, "@brackets"],

      // Operators
      [/[=+-/*]/, "tag"],

      // Keywords
      [/[a-zA-Z]\w*/, {
        cases: {
          "@units": "keyword",
          "@default": "identifier",
        },
      }],
    ],

    // Deal with white space, including single and multi-line comments
    whitespace: [
      [/\s+/, "white"],
      [/(^#{2}.*$)/, "comment.doc"],
      [/(^#.*$)/, "comment"],
      [/(^s:.*$)/, "type"],
      [/(^d:.*$)/, "string"],
      [/(^r:.*$)/, "regexp"],
      [/('''.*''')|(""".*""")/, "string"],
    ],

    // Recognize hex, negatives, decimals, imaginaries, longs, and scientific notation
    numbers: [
      [/-?0x([abcdef]|[ABCDEF]|\d)+[lL]?/, "number.hex"],
      [/-?(\d*\.)?\d+([eE][+\-]?\d+)?[jJ]?[lL]?/, "number"],
    ],
  },
};
```

## Examples

An example script to be used for testing can be found below. Refer to the [language description](language-description.md) for more examples.

```
## #### Plate section modulus
# The two "##"s at the beginning of the comment above is used to signal that it will be included in the report.
# The "#### " following it means that a level 4 heading will be added as per standard Markdown.
# This line and the two lines above will not be included in the report as they begin with only a single #. 

## Calculate the **plastic** section modulus of the plate.
# The **plastic** is Markdown for "make 'plastic' bold".

@b = 150 [mm]
    d: Width of the plate.
@t = 10 [mm]
    d: Thickness of the plate.
    
plasticDenominator = 4                      # This variable will not be reported as it does not have a symbol defined

@Z_p {Example reference} = b * t ^ 2 / plasticDenominator
    d: Plastic section modulus.
```
