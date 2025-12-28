# Language Fundamentals

This page describes the core concepts of the **Sunset Language**, a simplified programming language used for engineering calculations.

> **Note:** This page overlaps with [Getting Started](getting-started.md). For a quick introduction, see Getting Started. For the complete syntax reference, see [Reference](reference.md).

## Features

- Automatic handling of units and dimensional analysis
- Reporting in Markdown with LaTeX mathematical formatting
- Reusable elements for encapsulating calculations
- Conditional expressions for branching logic

## Comments

Comments start with `//` and cause the remainder of a line to be ignored.

```sunset
// This is a comment
x = 35 {mm}
y = 50 {N} // This is also a comment
```

For report generation, use `#` (excluded from reports) and `##` (included in reports).

## Units

Units are enclosed in curly brackets `{unit}`:

- `{m}` for metres
- `{mm}` for millimetres
- `{s}` for seconds
- `{MPa}` for megapascals

Derived units use operators: `*`, `.`, space for multiplication; `/` for division; `^` for exponents.

```sunset
area {mm^2} = length * width
velocity {m/s} = distance / time
```

## Variables

Variables are defined by assigning an expression to a name:

```sunset
length = 35 {mm}
area <A> = 100 {mm} * 30 {mm}
@f_y = 250 {MPa}
```

### Metadata

- `s:` - Symbol (LaTeX)
- `d:` - Description
- `r:` - Reference
- `l:` - Label

```sunset
bendingCapacity = 1500 {kNm}
    s: \phi M_{sx}
    d: The bending section capacity
    r: AS4100 Cl. 4.3.2
```

### Shorthand Syntax

- `<symbol>` - Symbol shorthand: `width <b> = 150 {mm}`
- `@name` - Symbol-as-name: `@f_y = 250 {MPa}`
- `"description"` - Description shorthand: `capacity = 250 {MPa} "Yield strength"`
- `{reference}` - Reference shorthand: `@Z_p {AS4100} = b * t^2 / 4`

## Calculations

Use arithmetic operators to form expressions:

- `+` for addition
- `-` for subtraction
- `*` for multiplication
- `/` for division
- `^` for exponents

```sunset
width <b> = 150 {mm}
thickness <t> = 10 {mm}
area <A> = width * thickness
```

> **Note:** Mathematical functions (`sqrt`, `sin`, `cos`, `tan`, etc.) are planned but not yet implemented.

## Elements

Elements are reusable groups of inputs and outputs:

```sunset
define Square:
    inputs:
        Width <w> {mm} = 100 {mm}
        Length <l> {mm} = 200 {mm}
    outputs:
        Area <A> {mm^2} = Width * Length
end

mySquare = Square(Width = 150 {mm})
result = mySquare.Area
```

See [Elements](elements.md) for more details.

## Conditionals

Variables can have conditional values:

```sunset
x = 15
y = 12 if x > 10
  = 3 otherwise
```

See [Conditionals](conditionals.md) for more details.

## Implementation Status

### Implemented

- [x] Variables and expressions
- [x] Elements (define, inputs, outputs, end)
- [x] Basic conditionals (if/otherwise)
- [x] Units and dimensional analysis
- [x] Metadata (symbol, description, reference)
- [x] Reporting (Markdown output)

### Planned

- [ ] Mathematical functions (sqrt, sin, cos, tan, etc.)
- [ ] Arrays/Lists
- [ ] Collection functions
- [ ] Dictionaries
- [ ] Options type
