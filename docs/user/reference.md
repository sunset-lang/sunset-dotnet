# Sunset Language Reference

This is the complete reference guide for the Sunset programming language, a domain-specific language designed for engineering calculations with automatic unit handling and report generation.

## Table of Contents

- [Comments](#comments)
- [Values and Numbers](#values-and-numbers)
- [Units](#units)
- [Variables](#variables)
- [Operators](#operators)
- [Conditionals](#conditionals)
- [Elements](#elements)
- [Reporting](#reporting)
- [Collections](#collections) *(Not Yet Implemented)*
- [Options](#options) *(Not Yet Implemented)*
- [Mathematical Functions](#mathematical-functions) *(Not Yet Implemented)*

---

## Comments

Sunset supports two types of comments:

### Regular Comments

Use `//` for comments that are ignored by both the interpreter and report generation:

```sunset
// This is a comment
x = 35 {mm}  // This is also a comment
```

### Documentation Comments

Use `#` for comments not included in reports, and `##` for comments that ARE included in generated reports:

```sunset
# This comment is not included in the report
## This comment IS included in the report
## You can use **Markdown** formatting here
```

---

## Values and Numbers

### Basic Numbers

```sunset
40          // Unitless integer
12.5        // Decimal number
345,850.598 // Commas are ignored
```

### Scientific Notation

```sunset
12e5        // Equal to 1,200,000
12e-2       // Equal to 0.12
3.5E3       // Equal to 3,500
```

### Strings

```sunset
"This is a string"
"""
This is a
multiline string
"""
```

### Boolean Values

```sunset
result = true
isValid = false
```

### Error Values

The `error` keyword represents a calculation error that propagates through dependent calculations:

```sunset
x = error       // Explicit error
y = 12
z = x + y       // Also becomes an error due to x
```

---

## Units

Units are enclosed in curly braces `{unit}` and follow the International System of Units (SI).

### Base Units

| Symbol | Unit |
|--------|------|
| `m` | metre |
| `mm` | millimetre |
| `s` | second |
| `kg` | kilogram |
| `N` | Newton |
| `Pa`, `kPa`, `MPa`, `GPa` | Pascal (and multiples) |
| `kN` | kilonewton |

### Unit Expressions

Units can be combined using operators:

- `*`, `.`, or space for multiplication
- `/` for division
- `^` for exponents

```sunset
area {mm^2}           // Square millimetres
velocity {m/s}        // Metres per second
force {kg m/s^2}      // Equivalent to Newtons
density {kg/m^3}      // Kilograms per cubic metre
```

### Equivalent Unit Expressions

```sunset
{mm^2}      // These are all
{mm*mm}     // equivalent ways
{mm mm}     // to express
{mm.mm}     // square millimetres
```

### Parentheses in Units

```sunset
{kg m/s^2}      // kg * m / s^2
{kg (m/s)^2}    // kg * m^2 / s^2  (different!)
```

### Unit Assignment and Reassignment

Assign units to a value:

```sunset
length = 35 {mm}
force = 10 {kN}
```

Reassign units (useful for empirical formulae):

```sunset
// Convert to unitless, then back to desired units
compressiveStrength = 32 {MPa}
// Strip units, apply formula, reassign units:
flexuralStrength = 0.6 * (compressiveStrength {MPa}) {MPa}
```

---

## Variables

### Basic Variable Declaration

Variable names must start with a letter or underscore, and can contain letters, numbers, and underscores:

```sunset
// Valid names
length = 35 {mm}
_duration = 2.5 {s}
my_variable_2 = 100

// Invalid names
3length = 35 {mm}       // Cannot start with number
my variable = 68 {s}    // Cannot contain spaces
```

### Variable with Expected Units

You can specify expected units for a variable, which enables unit conversion and type checking:

```sunset
length {mm} = 30 {mm}
width {mm} = 0.4 {m}     // Automatically converts to 400 mm
area {mm^2} = length * width
```

### Variable Metadata

Metadata can be added on indented lines following a variable declaration:

| Prefix | Purpose | Example |
|--------|---------|---------|
| `s:` | Symbol (LaTeX) | `s: \phi M_{sx}` |
| `d:` | Description | `d: The bending capacity` |
| `r:` | Reference | `r: AS4100 Cl. 4.3.2` |
| `l:` | Label (for UI) | `l: Bending Capacity` |

```sunset
bendingCapacity = 1500 {kNm}
    s: \phi M_{sx}
    d: The bending section capacity of the plate in the x axis.
    r: AS4100 Cl. 4.3.2
```

### Symbol Shorthand

Use angle brackets `<symbol>` as shorthand for symbol definition:

```sunset
// These are equivalent:
width = 150 {mm}
    s: b

width <b> = 150 {mm}
```

Symbols can be used as variable references in calculations:

```sunset
width <b> = 150 {mm}
thickness <t> = 10 {mm}
area <A> = b * t          // Can use symbols b and t
```

### Symbol-as-Name Shorthand

Use `@` prefix when the symbol should also be the variable name:

```sunset
@b = 150 {mm}             // Both name and symbol are "b"
@f_y = 250 {MPa}          // Both name and symbol are "f_y"
```

### Description Shorthand

Use double quotes for inline description:

```sunset
capacity = 250 {MPa} "The yield strength of steel."
```

### Reference Shorthand

Use curly braces with text for inline reference:

```sunset
@Z_p {AS4100 Cl. 5.2.1} = b * t^2 / 4
```

### Anonymous Variables

Use `?` prefix for temporary variables not included in reports or accessible outside their scope:

```sunset
?numerator = x / 35
?denominator = y / 40
Result <r> = numerator / denominator
```

---

## Operators

### Arithmetic Operators

| Operator | Description | Example |
|----------|-------------|---------|
| `+` | Addition | `x + y` |
| `-` | Subtraction | `x - y` |
| `*` | Multiplication | `x * y` |
| `/` | Division | `x / y` |
| `^` | Exponentiation | `x ^ 2` |
| `-` (unary) | Negation | `-x` |

### Comparison Operators

| Operator | Description | Example |
|----------|-------------|---------|
| `==` | Equal to | `x == y` |
| `!=` | Not equal to | `x != y` |
| `<` | Less than | `x < y` |
| `<=` | Less than or equal | `x <= y` |
| `>` | Greater than | `x > y` |
| `>=` | Greater than or equal | `x >= y` |

### Type Comparison Operators

| Operator | Description | Example |
|----------|-------------|---------|
| `is` | Type equality | `Section is Circle` |
| `is not` | Type inequality | `Section is not Circle` |

### Operator Precedence

From highest to lowest:
1. Parentheses `()`
2. Exponentiation `^`
3. Unary minus `-`
4. Multiplication `*`, Division `/`
5. Addition `+`, Subtraction `-`
6. Comparisons `<`, `<=`, `>`, `>=`, `==`, `!=`

---

## Conditionals

### Single-Line If Expression

```sunset
x = 15
y = 12 if x > 10
  = 3 otherwise
```

### Multi-Branch If Expression

```sunset
x = 30
y = 10 if x < 12
  = 15 if x >= 30
  = 20 otherwise
```

### Block If Expression

```sunset
@x =
  if y < 20 {mm}:
    A + B * C
  else if y < 30 {mm}:
    35 {MPa}
  else:
    40 {MPa}
  end
```

### Rules for Conditionals

- All branches must evaluate to the same type/units
- The `otherwise` or `else` branch is required
- Conditions are evaluated sequentially; first true condition wins

---

## Elements

Elements are reusable groups of inputs and calculations, similar to functions or classes.

### Defining Elements

```sunset
define Square:
    inputs:
        Width <w> {mm} = 100 {mm}
        Length <l> {mm} = 200 {mm}
    outputs:
        Area <A> {mm^2} = Width * Length
end
```

### Instantiating Elements

```sunset
// With default values
mySquare = Square()

// With all parameters (positional)
mySquare = Square(150 {mm}, 300 {mm})

// With named parameters
mySquare = Square(Width = 200 {mm}, Length = 350 {mm})

// Mixed (named parameters for specific overrides)
mySquare = Square(Width = 200 {mm})
```

### Accessing Element Properties

Use the dot `.` operator to access properties of an element instance:

```sunset
SquareInstance = Square(Width = 200 {mm}, Length = 350 {mm})
Result = SquareInstance.Area
```

### Nested Elements

Elements can contain other elements as inputs:

```sunset
define Section:
    inputs:
        Width <w> = 10 {mm}
        Depth <d> = 100 {mm}
    outputs:
        Area <A> = Width * Depth
end

define IsotropicMaterial:
    inputs:
        YieldStrength <f_y> = 300 {MPa}
        Density <rho> = 7800 {kg/m^3}
end

define Beam:
    inputs:
        Section = Section()
        Material = IsotropicMaterial()
    outputs:
        AxialCapacity <N> = Section.Area * Material.YieldStrength
end
```

### Element Inheritance

> **Note:** Element inheritance is partially implemented. Basic syntax exists but advanced features may not work.

Elements can inherit from parent elements:

```sunset
define Circle:
    inputs:
        Diameter <phi> = 100 {mm}
    outputs:
        Area = (3.14159 * Diameter^2) / 4
end

define Reinforcement(Circle):
    inputs:
        Diameter = parent
    outputs:
        Area = parent
end
```

The `parent` keyword indicates that the property is inherited unchanged from the parent element.

---

## Reporting

Sunset generates reports in Markdown format with LaTeX mathematical formatting.

### What Gets Reported

- Variables with symbols defined are included in calculations
- Variables without symbols are excluded from reports
- Documentation comments (`##`) are included as text
- Regular comments (`#` or `//`) are excluded

### Report Generation Example

```sunset
## #### Plate Section Modulus
## Calculate the **plastic** section modulus of the plate.

@b = 150 {mm}
    d: Width of the plate.
@t = 10 {mm}
    d: Thickness of the plate.

plasticDenominator = 4  // Not reported (no symbol)

@Z_p {Example reference} = b * t^2 / plasticDenominator
    d: Plastic section modulus.
```

This generates a report with:
- A heading "Plate Section Modulus"
- The text about plastic section modulus
- LaTeX-formatted equations showing the calculations
- A "Where" section listing all variables with descriptions

---

## Collections

> **Status: Not Yet Implemented**
>
> The following collection features are documented for future implementation but are not currently functional.

### Lists

Lists contain zero or more items of the same type:

```sunset
reinforcementDiameters = [12 {mm}, 16 {mm}, 20 {mm}, 24 {mm}]
```

Planned operations:
- `list[index]` - Access by index
- `list.first()` - Get first element
- `list.last()` - Get last element
- `list.foreach(expression)` - Iterate with `value` and `index` keywords
- `list.min()`, `list.max()`, `list.average()` - Reducers
- `list.where(condition)` - Filtering
- `list.select(expression)` - Mapping

### Dictionaries

Dictionaries are key-value pairs:

```sunset
windSpeed = ["A2": 45 {m/s}, "B1": 52 {m/s}]
```

Planned operations:
- `dict[key]` - Access by key
- `dict[~key]` - Linear interpolation between keys
- `dict[~key-]` - Find value just below key
- `dict[~key+]` - Find value just above key

---

## Options

> **Status: Not Yet Implemented**
>
> Options are planned for future implementation.

Options define a fixed set of choices:

```sunset
BoltTypes = Options(
    4.6/S: "Grade 4.6, snug tight",
    8.8/S: "Grade 8.8, snug tight",
    8.8/TB: "Grade 8.8, tensioned bearing",
    8.8/TF: "Grade 8.8, tensioned friction"
)
```

---

## Mathematical Functions

> **Status: Not Yet Implemented**
>
> The following mathematical functions are documented for future implementation.

| Function | Description |
|----------|-------------|
| `sqrt(x)` | Square root |
| `sin(x)` | Sine |
| `cos(x)` | Cosine |
| `tan(x)` | Tangent |
| `asin(x)` | Inverse sine |
| `acos(x)` | Inverse cosine |
| `atan(x)` | Inverse tangent |

---

## Complete Example

```sunset
## #### Steel Plate Capacity Calculation

## Calculate the axial capacity of a steel plate section.

// Define material properties
@f_y = 250 {MPa}
    d: Yield strength of steel.
    r: AS4100 Table 2.1

// Define plate dimensions
@b = 150 {mm}
    d: Width of the plate.

@t = 10 {mm}
    d: Thickness of the plate.

// Calculate section properties
@A {mm^2} = b * t
    d: Cross-sectional area of the plate.

// Calculate capacity
capacityFactor = 0.9  // Not reported (no symbol)

@phi_N {kN} = capacityFactor * f_y * A
    d: Design axial capacity of the plate.
    r: AS4100 Cl. 6.2.1
```
