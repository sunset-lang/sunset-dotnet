# Sunset Language Reference

This is the complete reference guide for the Sunset programming language, a domain-specific language designed for engineering calculations with automatic unit handling and report generation.

## Table of Contents

- [Comments](#comments)
- [Values and Numbers](#values-and-numbers)
- [Units](#units)
- [Strings](#strings)
- [Variables](#variables)
- [Operators](#operators)
- [Conditionals](#conditionals)
- [Elements](#elements)
- [Prototypes](#prototypes)
- [Reporting](#reporting)
- [Collections](#collections)
- [Imports and Packages](#imports-and-packages)
- [Options](#options) *(Not Yet Implemented)*
- [Mathematical Functions](#mathematical-functions)

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

Use `///` for documentation comments that ARE included in generated reports:

```sunset
// This comment is not included in the report
/// This comment IS included in the report
/// You can use **Markdown** formatting here
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

### Non-dimensionalising Units

To remove units from a quantity and obtain a dimensionless numeric value, use the `{/ unit}` syntax:

```sunset
Length = 100 {mm}
NumericValue = Length {/ m}  // Results in 0.1 (dimensionless)

// Can be used inline
Result = (50 {cm}) {/ m}  // Results in 0.5 (dimensionless)
```

| Syntax | Description |
|--------|-------------|
| `quantity {/ unit}` | Divides quantity by unit, returning dimensionless value |

**Rules:**
- Units must be dimensionally compatible (e.g., cannot non-dimensionalise `{m}` with `{s}`)
- Incompatible dimensions result in a compile-time error
- The error does not block execution of other unrelated code

---

## Strings

### Basic Strings

```sunset
message = "This is a string"
```

### Multiline Strings

Use triple quotes `"""` for strings that span multiple lines:

```sunset
description = """
This is a
multiline string
"""
```

**Behaviour:**
- Content between `"""` delimiters is preserved as-is, including newlines
- Leading/trailing whitespace within the string is preserved
- Useful for embedding formatted text, code snippets, or markup (e.g., SVG)

### String Concatenation

Strings can be concatenated using the `+` operator:

```sunset
greeting = "Hello, " + "world!"  // Results in "Hello, world!"
```

Quantities are automatically converted to their display format when concatenated:

```sunset
Length = 100 {mm}
label = "Length: " + Length  // Results in "Length: 100 mm"
```

### String Interpolation

> **Status: Not Yet Implemented**

Expressions can be embedded within strings using `::expression::`:

```sunset
Length = 100 {mm}
message = "The length is ::Length::"  // Results in "The length is 100 mm"

// Complex expressions are supported
summary = "Area: ::Width * Height::"
```

### Joining Lists of Strings

```sunset
words = ["hello", "world"]
sentence = words.join(", ")  // Results in "hello, world"
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
|----------|-------------|-------|
| `is` | Type equality | `Section is Circle` |
| `is Type binding` | Type match with binding | `Section is Circle circ` |

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
  if y < 30 {mm}:
    35 {MPa}
  otherwise:
    40 {MPa}
  end
```

### Rules for Conditionals

- All branches must evaluate to the same type/units
- The `otherwise` branch is required
- Conditions are evaluated sequentially; first true condition wins

### Type Pattern Matching

Use `is` to check element types and optionally bind to a typed variable:

```sunset
prototype Shape:
    outputs:
        return Area {m^2}
end

define Rectangle as Shape:
    inputs:
        Width = 1 {m}
        Length = 2 {m}
    outputs:
        return Area {m^2} = Width * Length
end

define Circle as Shape:
    inputs:
        Radius = 1 {m}
    outputs:
        return Area {m^2} = 3.14159 * Radius ^ 2
end

myShape {Shape} = Rectangle(2 {m}, 3 {m})

// Pattern matching with binding
area {m^2} = rect.Width * rect.Length if myShape is Rectangle rect
           = 3.14159 * circ.Radius ^ 2 if myShape is Circle circ
           = error otherwise
```

| Syntax | Description |
|--------|-------------|
| `expr is Type` | Check if expression matches type |
| `expr is Type binding` | Check type and bind to variable with that type |

**Rules:**
- An `otherwise` branch is always required when using pattern matching
- Binding variables are only in scope within their branch body
- The binding has the matched type, enabling access to type-specific properties

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

### Default Return Value

Elements can be used as inline functions by returning a default value when instantiated without accessing a specific property.

#### Implicit Return

The last variable defined in the element is the default return value:

```sunset
define Multiply:
    inputs:
        Value1 = 12
        Value2 = 5
    outputs:
        Result = Value1 * Value2

Example = Multiply(12, 5)  // Returns 60 (Result is the last defined variable)
```

#### Explicit Return with `return` Keyword

Use `return` to explicitly mark which variable should be returned by default:

```sunset
define Operation:
    inputs:
        Value1 = 12
        Value2 = 5
    outputs:
        return Add = Value1 + Value2
        Multiply = Value1 * Value2

Example = Operation(12, 5)  // Returns 17 (Add is marked with return)
```

| Rule | Description |
|------|-------------|
| Placement | `return` can be used on variables in `inputs` or `outputs` |
| Single use | `return` can only be used **once** per element |
| Empty elements | Instantiating an element with no variables without property access is an error |

### Partial Application (Element Re-instantiation)

Elements are immutable, but can be re-instantiated from an existing instance to create a new, independent copy with modified properties:

```sunset
define Rectangle:
    inputs:
        Length = 1 {m}
        Width = 2 {m}
    outputs:
        Area = Length * Width
end

// Create initial instance
RectangleInstance1 : Rectangle = Rectangle(Length = 2, Width = 4)  // Area = 8

// Re-instantiate from existing instance, changing only Length
RectangleInstance2 : Rectangle = RectangleInstance1(Length = 4)    // Area = 16 (Width = 4 inherited)

// Type annotation is optional when type can be inferred
RectangleInstance3 = RectangleInstance2(Width = 10)                // Area = 40
```

| Feature | Description |
|---------|-------------|
| Immutability | Re-instantiation creates a completely independent copy |
| Property override | Only input properties can be overridden; outputs are re-evaluated |
| Chaining | Re-instantiations can be chained |
| Type inference | Type annotation is optional for simple single instantiation expressions |

---

## Prototypes

Prototypes define contracts that elements can implement, similar to interfaces in other languages. They enable polymorphism by allowing different elements to be treated uniformly based on shared behaviour.

### Defining Prototypes

A prototype declaration specifies required inputs (with optional defaults) and required outputs (without expressions):

```sunset
prototype Shape:
    outputs:
        return Area {m^2}
end
```

| Component | Description |
|-----------|-------------|
| `prototype` | Keyword to start prototype declaration |
| Name | The prototype's identifier |
| `inputs:` | Optional section for input specifications |
| `outputs:` | Section for required output specifications |
| `return` | Marks the default return value |
| `end` | Ends the prototype declaration |

### Prototype Inputs

Prototype inputs can have default values that implementing elements inherit:

```sunset
prototype Rectangular:
    inputs:
        Width = 1 {m}
        Length = 2 {m}
    outputs:
        return Area {m^2}
end
```

### Prototype Outputs

Prototype outputs specify required properties but cannot have expressions—the implementing element must provide the calculation:

```sunset
prototype Shape:
    outputs:
        return Area {m^2}
        Perimeter {m}
end
```

### Implementing Prototypes

Elements implement prototypes using the `as` keyword:

```sunset
define Square as Shape:
    inputs:
        Width = 1 {m}
    outputs:
        return Area {m^2} = Width ^ 2
        Perimeter {m} = 4 * Width
end

define Rectangle as Shape:
    inputs:
        Width = 1 {m}
        Length = 2 {m}
    outputs:
        return Area {m^2} = Width * Length
        Perimeter {m} = 2 * (Width + Length)
end
```

**Error Example:**

If an element doesn't implement all required outputs, a compilation error occurs:

```sunset
define InvalidSquare as Shape:
    inputs:
        Width = 1 {m}
    outputs:
        return Area {m^2} = Width ^ 2
        // Error: Missing required output 'Perimeter' from prototype 'Shape'
end
```

**Requirements:**
- All prototype outputs must be defined in the element
- Output types must match the prototype specification
- If the prototype has a `return` output, the element must mark the same output with `return`

### Multiple Prototype Implementation

Elements can implement multiple prototypes:

```sunset
define Square as Shape, Rectangular:
    inputs:
        Width = 1 {m}
    outputs:
        return Area {m^2} = Width ^ 2
        Perimeter {m} = 4 * Width
end
```

### Prototype Inheritance

Prototypes can extend other prototypes:

```sunset
prototype Shape:
    outputs:
        return Area {m^2}
end

prototype Polygon as Shape:
    outputs:
        Sides
end
```

**Rules:**
- Child prototypes inherit all inputs and outputs from parent prototypes
- Child prototypes can add new inputs and outputs
- Child prototypes **cannot** override parent outputs (this is an error)

### Input Inheritance

When an element implements a prototype with default inputs, those inputs are automatically available:

```sunset
prototype Rectangular:
    inputs:
        Width = 1 {m}
        Length = 2 {m}
    outputs:
        return Area {m^2}
end

define Rectangle as Rectangular:
    outputs:
        return Area {m^2} = Width * Length
end

r = Rectangle()  // Uses inherited defaults: Width = 1, Length = 2, Area = 2 m^2
```

Elements can override inherited inputs:

```sunset
define Square as Rectangular:
    inputs:
        Width = 1 {m}
        Length = Width
    outputs:
        return Area {m^2} = Width * Length
end
```

### Type Annotations

Type annotations use curly braces to specify expected types:

| Syntax | Description |
|--------|-------------|
| `{m}` | Quantity with unit (metres) |
| `{text}` | String/text value |
| `{number}` | Dimensionless numeric value |
| `{Shape}` | Instance of element implementing Shape prototype |
| `{Shape list}` | List of instances implementing Shape prototype |
| `{text list}` | List of string values |
| `{number list}` | List of dimensionless numbers |

```sunset
myShape {Shape} = Square(2)
shapes {Shape list} = [Square(2), Rectangle(2, 3)]
names {text list} = ["Alice", "Bob", "Charlie"]
factors {number list} = [1.0, 1.5, 2.0]
```

### Lists of Prototype Instances

Collect elements implementing the same prototype into a list:

```sunset
Shapes {Shape list} = [Square(2), Rectangle(2, 3)]
```

### Iterating Over Prototype Lists

When iterating over a list of prototype instances:

- `value` resolves to the **default return value** (if the prototype has one)
- `value.instance` provides access to the **full element instance**

```sunset
prototype Shape:
    outputs:
        return Area {m^2}
end

define Square as Shape:
    inputs:
        Width = 1 {m}
    outputs:
        return Area {m^2} = Width ^ 2
end

define Rectangle as Shape:
    inputs:
        Width = 1 {m}
        Length = 2 {m}
    outputs:
        return Area {m^2} = Width * Length
end

Shapes {Shape list} = [Square(2), Rectangle(2, 3)]

// Using default return value (Area)
TotalArea {m^2} = Shapes.sum(value)  // 4 + 6 = 10 m^2

// Accessing element instance properties explicitly
TotalAreaExplicit {m^2} = Shapes.sum(value.instance.Area)  // Same result
```

### Empty Prototypes (Marker Prototypes)

Prototypes with no inputs or outputs are valid and serve as markers:

```sunset
prototype Printable:
end

define Report as Printable:
    inputs:
        Title = "Untitled"
    outputs:
        Content = "Report: " + Title
end
```

---

## Reporting

Sunset generates reports in Markdown format with LaTeX mathematical formatting.

### What Gets Reported

- Variables with symbols defined are included in calculations
- Variables without symbols are excluded from reports
- Documentation comments (`///`) are included as text
- Regular comments (`//`) are excluded

### Report Generation Example

```sunset
/// #### Plate Section Modulus
/// Calculate the **plastic** section modulus of the plate.

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

Collections include lists and dictionaries for storing multiple values.

### Lists

Lists contain zero or more items of the same type:

```sunset
reinforcementDiameters = [12 {mm}, 16 {mm}, 20 {mm}, 24 {mm}]
emptyList = []
```

#### List Methods

| Method | Description | Example |
|--------|-------------|---------|
| `list[index]` | Access by index (0-based) | `list[0]` |
| `list.first()` | Get first element | `list.first()` |
| `list.last()` | Get last element | `list.last()` |
| `list.min()` | Get minimum value | `list.min()` |
| `list.max()` | Get maximum value | `list.max()` |
| `list.average()` | Get average value | `list.average()` |
| `list.foreach(expr)` | Iterate with `value` and `index` | `list.foreach(value * 2)` |
| `list.where(cond)` | Filter elements | `list.where(value > 5)` |
| `list.select(expr)` | Transform elements | `list.select(value * 2)` |

#### Iteration Keywords

In `foreach`, `where`, and `select` expressions:
- `value` - The current element value
- `index` - The current element index (0-based)

```sunset
numbers = [1, 2, 3, 4, 5]

// Get all numbers greater than 2
filtered = numbers.where(value > 2)  // [3, 4, 5]

// Double all numbers
doubled = numbers.select(value * 2)  // [2, 4, 6, 8, 10]

// Chain methods
result = numbers.where(value > 2).select(value * 2).max()  // 10
```

```sunset
items = [12 {mm}, 16 {mm}, 20 {mm}]
first = items[0]           // 12 {mm}
doubled = items.foreach(value * 2)  // [24 {mm}, 32 {mm}, 40 {mm}]
maxItem = items.max()      // 20 {mm}
```

### Dictionaries

Dictionaries are key-value pairs with support for interpolation-based access:

```sunset
windSpeed = ["A2": 45 {m/s}, "B1": 52 {m/s}]
temperatures = [0: 20, 100: 100, 200: 180]
emptyDict = [:]
```

Operations:

| Syntax | Description |
|--------|-------------|
| `dict[key]` | Access by exact key |
| `dict[~key]` | Linear interpolation between keys (numeric keys only) |
| `dict[~key-]` | Find value for largest key ≤ lookup key |
| `dict[~key+]` | Find value for smallest key ≥ lookup key |

```sunset
// Exact key access
temps = [0: 20, 100: 100, 200: 180]
t100 = temps[100]  // 100

// Linear interpolation
stressStrain = [0: 0, 100: 100]
interpolated = stressStrain[~50]  // 50

// Floor/ceiling lookup
table = [0: 10, 100: 100, 200: 180]
belowValue = table[~150-]  // 100 (value at key 100)
aboveValue = table[~150+]  // 180 (value at key 200)
```

See [Functions on Collections](functions-on-collections.md) for more details.

---

## Imports and Packages

Sunset supports importing code from other files and packages to enable code reuse and modular organization.

### Basic Import Syntax

Use the `import` statement to import definitions from other files or packages:

```sunset
import Diagrams.Core
import Diagrams.Geometry
```

### Import Resolution

When you use an import statement, Sunset searches for the module in the following order:

1. **Relative imports** (paths starting with `./` or `../`)
2. **Package imports** (searching package directories)
3. **StandardLibrary fallback** (built-in modules)

### Relative Imports

Import files relative to the current file's location:

```sunset
// Import from current directory
import ./helpers

// Import from parent directory
import ../shared.utils

// Import from multiple levels up
import ../../common.types
```

| Syntax | Description |
|--------|-------------|
| `./module` | Import from current directory |
| `../module` | Import from parent directory |
| `../../module` | Import from grandparent directory |

### Package Imports

Import from installed packages by package name:

```sunset
// Import a module from a package
import PackageName.ModuleName

// Import a specific file from a package
import PackageName.SubModule.FileName
```

### StandardLibrary

Sunset includes a built-in StandardLibrary that provides common units, dimensions, and utility modules. The StandardLibrary is automatically searched when an import cannot be found in other locations.

**Available StandardLibrary modules:**

| Module | Description |
|--------|-------------|
| `StandardLibrary` | Base units and dimensions (automatically loaded) |
| `Diagrams` | Diagram generation library (SVG output) |
| `Diagrams.Core` | Core diagram types and prototypes |
| `Diagrams.Geometry` | Geometric primitives (Point, Line, Plane) |
| `Diagrams.Shapes` | Shape definitions (Rectangle, Circle, etc.) |
| `Diagrams.Operations` | Geometric operations (Intersect, etc.) |
| `Diagrams.Svg` | SVG rendering utilities |

**Note:** The base `StandardLibrary` module (containing units like `m`, `mm`, `kg`, `N`, etc.) is automatically loaded for all Sunset files. You don't need to explicitly import it.

### Import Examples

```sunset
// Import diagram functionality
import Diagrams.Core
import Diagrams.Geometry

// Use imported types
point {Point} = Point(x = 1 {m}, y = 2 {m})
colour {RGBA} = RGBA(R = 255, G = 128, B = 64)
```

### Package Configuration

Packages are configured using a `sunset-package.toml` file in the package root directory. This file defines the package metadata and entry points.

Example `sunset-package.toml`:

```toml
[package]
name = "MyPackage"
version = "1.0.0"

[files]
# Map module names to file paths
main = "src/main.sun"
helpers = "src/helpers.sun"
```

### Import Visibility

When you import a module, all public declarations (elements, prototypes, options, and top-level variables) from that module become available in the importing file.

- **Public declarations**: All top-level declarations are public by default
- **Anonymous variables** (prefixed with `?`): Not exported or visible to importers

---

## Options

Options define a fixed set of valid values for a type:

```sunset
option Size {m}:
    10 {m}
    20 {m}
    30 {m}
end

option DrawingMethods {text}:
    "SVG"
    "Typst"
end

option Scale {number}:
    1
    2
    5
end
```

Options can be used as type annotations:

```sunset
x {Size} = 10 {m}
method {DrawingMethods} = "SVG"
```

See [Options](options.md) for more details.

---

## Mathematical Functions

The following mathematical functions are available:

| Function | Description | Notes |
|----------|-------------|-------|
| `sqrt(x)` | Square root | Returns same dimension as input^0.5 |
| `sin(x)` | Sine | Accepts angle units |
| `cos(x)` | Cosine | Accepts angle units |
| `tan(x)` | Tangent | Accepts angle units |
| `asin(x)` | Inverse sine | Returns dimensionless |
| `acos(x)` | Inverse cosine | Returns dimensionless |
| `atan(x)` | Inverse tangent | Returns dimensionless |

```sunset
// Square root
hypotenuse = sqrt(3^2 + 4^2)  // 5

// Trigonometry with degrees
angle = 45 {deg}
sinValue = sin(angle)  // ~0.707

// Inverse trig functions
ratio = 0.5
angle = asin(ratio)  // Returns dimensionless (radians)
```

---

## Complete Example

```sunset
/// #### Steel Plate Capacity Calculation

/// Calculate the axial capacity of a steel plate section.

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
