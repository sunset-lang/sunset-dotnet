# Syntax highlighting

The following code has been written for the [Monaco editor](https://microsoft.github.io/monaco-editor) to allow syntax highlighting specifically for the Sunset Language.

To see it in action, go to [the Monarch page](https://microsoft.github.io/monaco-editor/monarch.html).

> [!NOTE] This needs to be updated to take into account the new [grammar](grammar.md).

```javascript
  // Used in https://microsoft.github.io/monaco-editor/monarch.html for syntax highlighting

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
      "^",
    ],

    brackets: [
      { open: "{", close: "}", token: "delimiter.curly" },
      { open: "[", close: "]", token: "delimiter.bracket" },
      { open: "(", close: ")", token: "delimiter.parenthesis" },
      { open: "<", close: ">", token: "delimiter.angle" },
    ],

    keywords: [
      "define",
      "end",
      "inputs",
      "outputs",
      "if",
      "otherwise",
    ],

    tokenizer: {
      root: [
        { include: "@whitespace" },
        { include: "@numbers" },

        // Symbols <symbol_{subscript}^{superscript}>
        // <symbol>
        [/<(\{?\\?\w\s*\^?\}?)*>/, "type"],

        // Unit enclosure
        [/[{}]/, "keyword"],

        // Brackets
        [/[<>{}\[\]()]/, "@brackets"],

        // Operators
        [/[=+-/*^]/, "operators"],

        // Colon
        [/[:]/, "keyword"],

        // Keywords
        [/[a-zA-Z]\w*/, {
          cases: {
            "@units": "keyword",
            "@keywords": "keyword",
            "@default": "identifier",
          },
        }],
      ],

      // Deal with white space, including single and multi-line comments
      whitespace: [
        [/\s+/, "white"],
        [/^\/{2}(?!\/).*$/, "comment"],
        [/(^\/{3}.*$)/, "comment.doc"],
        [/(^d:.*$)/, "comment.doc"],
        [/(^r:.*$)/, "regexp"],
        [/('.*')|(".*")/, "string"],
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
// A steel plate with rectangular cross section
define Plate:
  inputs:
    Width <w> = 120 {mm}
      d: Width of the plate
    Thickness <t> = 20 {mm}
      d: Thickness of the plate
  outputs:
    Area <A> = Width * Thickness
      d: Area of the plate
    SecondMoment <Z> = Width * Thickness ^ 2 / 6
      d: Second moment of area of the plate
end

phiBending <\phi> = 0.9
  d: Safety factor for bending moments

PlateInstance <plate> = Plate(
                      Width = 100 {mm},
                      Thickness = 32 {mm}
                      )

YieldStress <f_y> {MPa} = 300 {MPa} if PlateInstance.Thickness <= 32 {mm}
                        = 280 {MPa} if PlateInstance.Thickness > 32 {mm}
                        = 200 {MPa} otherwise
                  
TensionCapacity <\phi N_t> {kN} = phi * PlateInstance.Area * YieldStress

MomentCapacity <\phi M_s> {kN} = phi * PlateInstance.SecondMoment * YieldStress
```
