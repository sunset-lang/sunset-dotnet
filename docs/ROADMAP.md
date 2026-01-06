# Sunset Language Roadmap

This document tracks features that are documented but not yet implemented in the Sunset language.

## Implementation Status Legend

- ✅ Implemented
- 🔶 Partially Implemented
- ⬜ Not Started

---

## Priority 1: Core Language Features

### Mathematical Functions
**Status:** ✅ Implemented

All core mathematical functions have been implemented in the `src/Sunset.Parser/BuiltIns/` directory.

| Function | Description | Status |
|----------|-------------|--------|
| `sqrt(x)` | Square root | ✅ |
| `sin(x)` | Sine | ✅ |
| `cos(x)` | Cosine | ✅ |
| `tan(x)` | Tangent | ✅ |
| `asin(x)` | Inverse sine | ✅ |
| `acos(x)` | Inverse cosine | ✅ |
| `atan(x)` | Inverse tangent | ✅ |

**Implementation Details:**
- Built-in functions are registered in `src/Sunset.Parser/BuiltIns/BuiltInFunction.cs`
- Each function has its own implementation file in `src/Sunset.Parser/BuiltIns/Functions/`
- Type checking validates argument types and handles angle units appropriately
- Trig functions accept angle inputs; inverse trig functions return dimensionless results

---

### Logical Operators
**Status:** 🔶 Partially Implemented

| Operator | Description | Status |
|----------|-------------|--------|
| `and` | Logical AND | ⬜ |
| `or` | Logical OR | ⬜ |
| `not` | Logical NOT (unary) | 🔶 (only works with `is not`) |

**Implementation Notes:**
- Add `And`, `Or` token types to `TokenType.cs`
- Add to `TokenDefinitions.cs` keyword mapping
- Handle in Parser for binary expressions
- Implement boolean evaluation in `Evaluator.cs`

---

## Priority 2: Collection Types

### Lists/Arrays - Basic
**Status:** ✅ Implemented

| Feature | Syntax | Status |
|---------|--------|--------|
| List literal | `[item1, item2, item3]` | ✅ |
| Index access | `list[index]` | ✅ |
| First element | `list.first()` | ✅ |
| Last element | `list.last()` | ✅ |

**Implementation Details:**
- `ListExpression` class in `src/Sunset.Parser/Expressions/ListExpression.cs`
- `IndexExpression` class in `src/Sunset.Parser/Expressions/IndexExpression.cs`
- `ListType` for type checking in `src/Sunset.Parser/Results/Types/`
- List methods in `src/Sunset.Parser/BuiltIns/ListMethods/`
- Error handling in `src/Sunset.Parser/Errors/Semantic/ListErrors.cs`

---

### Lists/Arrays - Advanced
**Status:** ✅ Implemented

| Feature | Syntax | Status |
|---------|--------|--------|
| Minimum | `list.min()` | ✅ |
| Maximum | `list.max()` | ✅ |
| Average | `list.average()` | ✅ |
| Iteration | `list.foreach(expression)` | ✅ |
| Filter | `list.where(condition)` | ✅ |
| Map | `list.select(expression)` | ✅ |

**Implementation Notes:**
- List method infrastructure implemented in `src/Sunset.Parser/BuiltIns/ListMethods/`
- All methods preserve units when operating on lists with units
- Proper error handling for empty lists and non-list targets
- `value` and `index` keywords available in foreach/where/select expressions
- Methods can be chained: `list.where(value > 5).select(value * 2).max()`

---

### Dictionaries
**Status:** 🔶 Partially Implemented

| Feature | Syntax | Status |
|---------|--------|--------|
| Dictionary literal | `["key1": value1, "key2": value2]` | ✅ |
| Empty dictionary | `[:]` | ✅ |
| Key access | `dict[key]` | ✅ |
| Linear interpolation | `dict[~key]` | ✅ |
| Find below key | `dict[~key-]` | ✅ |
| Find above key | `dict[~key+]` | ✅ |
| Iteration | `dict.foreach(expression)` | ⬜ |

**Implementation Details:**
- `DictionaryExpression` class in `src/Sunset.Parser/Expressions/DictionaryExpression.cs`
- `DictionaryEntry` class in `src/Sunset.Parser/Expressions/DictionaryEntry.cs`
- `DictionaryType` for type checking in `src/Sunset.Parser/Results/Types/IResultType.cs`
- `DictionaryResult` for evaluation in `src/Sunset.Parser/Results/DictionaryResult.cs`
- `CollectionAccessMode` enum for interpolation modes in `src/Sunset.Parser/Expressions/CollectionAccessMode.cs`
- Error handling in `src/Sunset.Parser/Errors/Semantic/DictionaryErrors.cs`
- Interpolation logic supports linear interpolation and floor/ceiling lookups

---

## Priority 3: Unit Operations

### Non-dimensionalising Units
**Status:** ✅ Implemented

Allows removing units from a quantity by dividing by a specified unit, returning a dimensionless numeric value.

| Feature | Syntax | Status |
|---------|--------|--------|
| Unit removal | `quantity {/ unit}` | ✅ |

**Syntax:**
```sunset
// Remove units from a quantity
Length = 100 {mm}
NumericValue = Length {/ m}  // Results in 0.1 (dimensionless)

// Can be used inline
Result = (50 {cm}) {/ m}  // Results in 0.5 (dimensionless)
```

**Behavior:**
- The slash (`/`) signals division by the specified unit to make the value unitless
- Returns a dimensionless numeric value
- **Compile-time error** if the units are not dimensionally compatible (e.g., trying to non-dimensionalise `{m}` with `{s}`)
- Error should not block execution of other unrelated code in the AST

**Implementation Details:**
- `NonDimensionalizingExpression` class in `src/Sunset.Parser/Expressions/NonDimensionalizingExpression.cs`
- Parser detects `{/` pattern in `Parser.Rules.cs` and creates `NonDimensionalizingExpression`
- `DimensionalIncompatibilityError` in `src/Sunset.Parser/Errors/Semantic/DimensionalIncompatibilityError.cs`
- TypeChecker validates dimensional compatibility
- Evaluator converts the value to the target unit and returns a dimensionless result

---

## Priority 4: String Operations

### String Concatenation
**Status:** ✅ Implemented

| Feature | Syntax | Status |
|---------|--------|--------|
| String + String | `"hello " + "world"` | ✅ |
| String + Quantity | `"Length: " + 100 {mm}` | ✅ |
| Quantity + String | `100 {mm} + " long"` | ✅ |

**Behavior:**
- Concatenating two strings produces a combined string
- Concatenating a string with a quantity uses the display format with units (e.g., `"100 mm"`)
- Dimensionless quantities omit the unit in the formatted output

**Implementation Details:**
- Type checking in `TypeChecker.cs` handles string+string, string+quantity, and quantity+string cases
- Evaluation in `Evaluator.cs` performs concatenation with `FormatQuantity()` helper
- `StringResult` equality implemented for proper value comparison
- `StringType` added as a valid type that doesn't require unit declarations

---

### String Interpolation
**Status:** ✅ Implemented

| Feature | Syntax | Status |
|---------|--------|--------|
| Interpolation | `"The value is ::expression::"` | ✅ |
| Escape sequence | `"\::not interpolation\::"` | ✅ |
| Multiline support | `"""text ::expr:: more"""` | ✅ |
| Boolean formatting | `"::true::"` → `"True"` | ✅ |

**Syntax:**
```sunset
x = 100 {mm}
message = "The length is ::x::"  // Results in "The length is 100 mm"

// Complex expressions supported
summary = "Area: ::Width * Height::"

// Escape literal colons with backslash
ratio = "ratio 1\::2"  // Results in "ratio 1::2"

// Property access in interpolation
result = "area: ::sq.Area::"
```

**Behavior:**
- Expressions within `::...::` inside strings are evaluated and converted to text
- Quantities are formatted with their display units (e.g., `"100 mm"`)
- Dimensionless quantities show only the numeric value
- Booleans are formatted as `"True"` or `"False"`
- Escape `\::` produces literal `::` in output
- Empty interpolation `::::` reports an error
- Unclosed interpolation `::expr` (without closing `::`) reports an error
- Strings inside interpolation are not allowed (reports `StringInInterpolationError`)

**Implementation Details:**
- `InterpolatedStringToken` in `src/Sunset.Parser/Lexing/Tokens/InterpolatedStringToken.cs` with segment data
- `InterpolatedStringExpression` in `src/Sunset.Parser/Expressions/InterpolatedStringExpression.cs`
- `TextSegment` and `ExpressionSegment` in `src/Sunset.Parser/Expressions/StringSegment.cs`
- Lexer detects `::` in strings and creates `InterpolatedStringToken` with parsed segments
- Parser creates sub-parser for each expression segment via `ParseExpressionSegment()`
- All analysis passes (NameResolver, ReferenceChecker, TypeChecker) visit embedded expressions
- Evaluator concatenates text segments with formatted expression results
- Error types: `UnclosedInterpolationError`, `EmptyInterpolationError`, `StringInInterpolationError`

---

### List Join Method
**Status:** ✅ Implemented

| Feature | Syntax | Status |
|---------|--------|--------|
| Join strings | `list.join(separator)` | ✅ |

**Syntax:**
```sunset
Words = ["hello", "world"]
Sentence = Words.join(", ")  // Results in "hello, world"
```

**Behavior:**
- Joins a list of strings using the specified separator
- Returns a single concatenated string
- Empty lists return an empty string
- Single-element lists return the element without separator

**Implementation Details:**
- `JoinMethod` in `src/Sunset.Parser/BuiltIns/ListMethods/JoinMethod.cs`
- `IListMethodWithStringArgument` interface for methods taking string arguments
- Type checking validates list elements and separator are strings

---

## Priority 5: Functional Programming

### Default Return Value
**Status:** ✅ Implemented

Allows elements to be used as inline functions by returning a default value when instantiated without property access.

| Feature | Description | Status |
|---------|-------------|--------|
| Implicit return | Last defined value is default | ✅ |
| `return` keyword | Explicit default value marker | ✅ |

**Syntax:**
```sunset
// Implicit return: last defined value (Result) is returned by default
define Multiply:
    inputs:
        Value1 = 12
        Value2 = 5
    calculations:
        Result = Value1 * Value2

Example = Multiply(12, 5)  // Returns 60 (Result is the default return)
```

```sunset
// Explicit return: use `return` keyword to mark the default value
define Operation:
    inputs:
        Value1 = 12
        Value2 = 5
    calculations:
        return Add = Value1 + Value2
        Multiply = Value1 * Value2

Example = Operation(12, 5)  // Returns 17 (Add is marked with return)
```

**Behavior:**
- When an element is instantiated in an expression without accessing a property, it returns its default value
- **Implicit return**: The last variable defined in the element (in `calculations` or `inputs`) is the default
- **Explicit return**: The `return` keyword marks which variable is the default return value
- `return` can be used on variables in either `inputs` or `calculations`
- **Error** if `return` is used more than once in an element definition
- **Error** if an element with no variables is instantiated without property access

**Implementation Details:**
- `return` keyword added to lexer (`TokenType.Return`) in `TokenDefinitions.cs`
- `ElementDeclaration` tracks `ExplicitDefaultReturn` and `DefaultReturnVariable` properties
- `TypeChecker` validates single `return` usage per element (`MultipleReturnError`)
- `Evaluator.ResolveDefaultReturnValue()` extracts default value when element instance is used in expressions
- Default return value is resolved when element instance is used in binary expressions (e.g., `MyElement(x = 5) * 2`)

---

### Partial Application (Element Re-instantiation)
**Status:** ✅ Implemented

Allows creating new element instances based on existing instances, preserving unchanged input values.

| Feature | Description | Status |
|---------|-------------|--------|
| Re-instantiation | `existingInstance(property = value)` | ✅ |
| Property inheritance | Unchanged properties copied from source | ✅ |
| Type inference | Type inferred from source instance | ✅ |

**Syntax:**
```sunset
define Rectangle:
    inputs:
        Length = 1 {m}
        Width = 2 {m}
    calculations:
        Area = Length * Width
end

RectangleInstance1 : Rectangle = Rectangle(Length = 2, Width = 4)  // Area = 8
RectangleInstance2 : Rectangle = RectangleInstance1(Length = 4)    // Area = 16 (Width = 4 inherited)
RectangleInstance3 = RectangleInstance2(Width = 10)                // Area = 40, type inferred
```

**Behavior:**
- Re-instantiating from an existing instance creates a **completely independent copy** (enforces immutability)
- Only input properties can be overridden; calculations are re-evaluated
- Re-instantiations can be chained
- Type annotation is optional when the expression is a simple single instantiation (type is inferred from the source instance)

**Implementation Details:**
- `NameResolver.Visit(CallExpression)` detects when callee is an element instance vs. element definition
- `NamePassData.SourceInstance` stores the source variable for re-instantiation
- `Evaluator.EvaluateReinstantiation()` clones instances with property overrides
- Instance cloning is immutable - creates independent copies with new values for overridden properties
- Calculations in the new instance are re-evaluated with the new input values

---

## Priority 6: Type System Extensions

### Options Type
**Status:** 🔶 In Progress

| Feature | Description | Status |
|---------|-------------|--------|
| Options definition | `option Name {type}: values... end` | 🔶 |
| Option type annotation | `{OptionName}` as type annotation | 🔶 |
| Compile-time validation | Validate literal values against options | 🔶 |
| `text` keyword | Type annotation for string options | 🔶 |
| `number` keyword | Type annotation for dimensionless options | 🔶 |
| Exhaustive matching | Omit `otherwise` when all options covered | ⬜ |

**Implementation Notes:**
- Options create a sum type with fixed valid values
- `{text}` and `{number}` keywords for built-in type annotations
- Type inference from first value if annotation omitted
- Compile-time validation for literals, runtime for computed values

---

## Priority 7: Element System

### Element Inheritance
**Status:** 🔶 Partially Implemented

| Feature | Description | Status |
|---------|-------------|--------|
| Inheritance syntax | `define Child(Parent):` | ✅ Parsed |
| `parent` keyword | Inherit property unchanged | ⬜ |
| Property overriding | Redefine parent properties | ⬜ |
| Inheritance validation | Ensure all parent properties declared | ⬜ |
| Type compatibility | Child usable where parent expected | ⬜ |

**Implementation Notes:**
- Add parent element reference to `ElementDeclaration`
- Implement `parent` keyword in `NameResolver`
- Add inheritance chain validation
- Update type checking for element compatibility

---

### Anonymous Elements
**Status:** ⬜ Not Started

| Feature | Description | Status |
|---------|-------------|--------|
| Dot notation | `result.subvalue = expression` | ⬜ |
| Dynamic creation | Create nested element-like structures | ⬜ |

---

### Element Groups
**Status:** ⬜ Not Started

| Feature | Description | Status |
|---------|-------------|--------|
| Group definition | `group GroupName = [Element1, Element2]` | ⬜ |
| Type constraints | Use groups as input type constraints | ⬜ |

---

## Recent Bug Fixes

The following bugs have been fixed:

| Issue | Description | Status |
|-------|-------------|--------|
| #45 | `if` expression printing value expression for constant values | ✅ Fixed (PR #80) |
| #58 | Multiple errors for name resolution errors | ✅ Fixed |
| #63 | Unit type checking in if statement conditions | ✅ Fixed |
| #71 | Large values truncate/overflow | ✅ Fixed (PR #81) |

---

## Summary

| Category | Total | ✅ | 🔶 | ⬜ |
|----------|-------|-----|-----|-----|
| Math Functions | 7 | 7 | 0 | 0 |
| Logical Operators | 3 | 0 | 1 | 2 |
| Lists - Basic | 4 | 4 | 0 | 0 |
| Lists - Advanced | 6 | 6 | 0 | 0 |
| Unit Operations | 1 | 1 | 0 | 0 |
| String Operations | 4 | 4 | 0 | 0 |
| Functional Programming | 5 | 5 | 0 | 0 |
| Dictionaries | 7 | 6 | 0 | 1 |
| Options | 3 | 0 | 0 | 3 |
| Element Inheritance | 5 | 1 | 0 | 4 |
| Anonymous Elements | 2 | 0 | 0 | 2 |
| Element Groups | 2 | 0 | 0 | 2 |
| Module System | 8 | 8 | 0 | 0 |
| Diagramming Library | 1 | 1 | 0 | 0 |
| **Total** | **58** | **43** | **1** | **14** |

---

## Priority 8: Module System

### Import Statement
**Status:** ✅ Implemented

| Feature | Syntax | Status |
|---------|--------|--------|
| Import declaration | `import module.path` | ✅ |
| Relative imports | `import ./local.module` | ✅ |
| Parent directory imports | `import ../shared.utils` | ✅ |
| Package imports | `import PackageName` | ✅ |
| Module imports | `import PackageName.Module` | ✅ |
| File imports | `import PackageName.Module.File` | ✅ |
| Specific identifier imports | `import Package.File.[Ident1, Ident2]` | ✅ |
| StandardLibrary fallback | `import Diagrams.Geometry` | ✅ |

**Syntax:**
```sunset
// Import from standard library modules (resolves to StandardLibrary/Diagrams/...)
import Diagrams.Core
import Diagrams.Geometry

// Relative imports
import ./local.helpers      // Current directory
import ../shared.utils      // Parent directory
import ../../common.types   // Two levels up

// Import specific identifiers
import Diagrams.Geometry.[Point, Line]
```

**Behavior:**
- Imports make all exported declarations from the imported file available in the current scope
- Module paths use dot notation (e.g., `Diagrams.Core` resolves to `Diagrams/Core.sun`)
- StandardLibrary modules can be imported without the `StandardLibrary.` prefix
- Relative imports use `./` (current directory) or `../` (parent directory) notation
- Circular imports detected and reported as errors
- Case-insensitive package resolution for cross-platform compatibility

**Implementation Details:**
- `Import` token type in `TokenType.cs` and `TokenDefinitions.cs`
- `ImportDeclaration` class in `Parsing/Declarations/ImportDeclaration.cs`
- `ImportResolver` in `Analysis/ImportResolution/ImportResolver.cs` resolves imports
- `PackageRegistry` in `Packages/PackageRegistry.cs` manages package discovery
- `Package` and `Module` classes in `Packages/` represent package structure
- `Environment` loads StandardLibrary and passes it to ImportResolver for fallback resolution
- Import pass data stored in `ImportPassData` on FileScope

---

## Priority 9: Diagramming Standard Library

### Standard Library: Diagrams
**Status:** ✅ Implemented

A standard library for generating diagrams as SVG output alongside LaTeX and Markdown reports.

**Prerequisites:**
- ✅ String Interpolation (Priority 4)
- ✅ Import Statement (Priority 8)

**File Structure:**
```
src/Sunset.Parser/StandardLibrary/
├── sunset-package.toml             # Package manifest
├── StandardLibrary.sun             # Entry point - units/dimensions
└── Diagrams/
    ├── Diagrams.sun                # Main entry point (imports all modules)
    ├── Core.sun                    # RGBA, styles, prototypes, Diagram
    ├── Geometry.sun                # Point, Line, Planes
    ├── Shapes.sun                  # Rectangle, Circle, Ellipse, Paths
    ├── Operations.sun              # Intersect
    └── Svg.sun                     # SVG renderer
```

**Core Types:**

| Type | Description |
|------|-------------|
| `RGBA` | Color definition (R, G, B, A components) |
| `BasicColours` | Predefined colours (Black, White, Red, Green, Blue, Grey, LightGrey) |
| `DiagramFormat` | Output format option ("SVG", "Typst") |
| `DrawStyle` | Base styling prototype (stroke, fill, width) |
| `DiagramElement` | Base prototype for drawable elements |
| `DiagramDefinition` | Prototype for user diagrams (provides `_diagram` context) |
| `Diagram` | Container for elements with viewport, scale, and format |
| `Linear` | Prototype for line-like geometry (for intersections) |
| `Plane` | Prototype for infinite plane-like geometry (extends Linear) |

**Geometry Primitives:**

| Element | Description |
|---------|-------------|
| `Point` | 2D point with x, y coordinates |
| `Line` | Finite line segment between two points (implements `Linear`) |
| `PlaneHorizontal` | Infinite horizontal line at y offset (implements `Plane`) |
| `PlaneVertical` | Infinite vertical line at x offset (implements `Plane`) |
| `PlaneRotated` | Infinite line through point at angle (implements `Plane`) |

**Shapes:**

| Element | Description |
|---------|-------------|
| `Rectangle` | Rectangle with centre, width, height |
| `Circle` | Circle with centre, radius |
| `Ellipse` | Ellipse with centre, radiusX, radiusY |
| `OpenPath` | Open polyline through points |
| `FilledPath` | Closed polygon with fill |

**Operations:**

| Function | Description |
|----------|-------------|
| `Intersect(L1, L2)` | Compute intersection of two Linear elements (validates segment bounds for Line) |

**Renderers:**

| Element | Description |
|---------|-------------|
| `DrawSVG` | Generate complete SVG document from Diagram |

**Design Decisions:**
- Coordinate system: Engineering convention (Y-up), flipped in SVG output
- Scale: Dimensionless pixels-per-metre factor
- Parallel line intersection: Returns `error`
- Line segment intersection outside bounds: Returns `error`
- Diagram reference: Passed as `_diagram` input to elements (or inherited via `DiagramDefinition` prototype)
- Output format: Controlled via `Diagram.Format` with pattern matching in Draw functions

**Example Usage (using DiagramDefinition prototype):**
```sunset
import Diagrams

define PadFooting_Elevation as DiagramDefinition:
    inputs:
        Width = 1.2 {m}
        Depth = 2.4 {m}
        // ViewportWidth, ViewportHeight, Scale, Format inherited from DiagramDefinition
    outputs:
        // _diagram is automatically provided by DiagramDefinition prototype
        
        Left {PlaneVertical} = PlaneVertical(x = 0 {m}, _diagram = _diagram)
        Right {PlaneVertical} = PlaneVertical(x = Width, _diagram = _diagram)
        Bottom {PlaneHorizontal} = PlaneHorizontal(y = 0 {m}, _diagram = _diagram)
        Top {PlaneHorizontal} = PlaneHorizontal(y = Depth, _diagram = _diagram)
        
        Corners {Point list} = [
            Intersect(Left, Bottom, _diagram = _diagram).Result,
            Intersect(Left, Top, _diagram = _diagram).Result,
            Intersect(Right, Top, _diagram = _diagram).Result,
            Intersect(Right, Bottom, _diagram = _diagram).Result
        ]
        
        return Draw {Diagram} = _diagram(
            Elements = [FilledPath(points = Corners, _diagram = _diagram)]
        )
end
```

---

## Key Files for Implementation

| File | Purpose |
|------|---------|
| `src/Sunset.Parser/Lexing/Tokens/TokenType.cs` | Add new token types |
| `src/Sunset.Parser/Lexing/Tokens/TokenDefinitions.cs` | Add keyword mappings |
| `src/Sunset.Parser/Parsing/Parser.cs` | Parse new syntax |
| `src/Sunset.Parser/Expressions/` | Add new expression types |
| `src/Sunset.Parser/Results/` | Add new result types |
| `src/Sunset.Parser/Analysis/TypeChecking/TypeChecker.cs` | Add type rules |
| `src/Sunset.Parser/Visitors/Evaluation/Evaluator.cs` | Implement evaluation |
| `src/Sunset.Parser/BuiltIns/` | Built-in function implementations |
| `src/Sunset.Parser/StandardLibrary/` | Standard library `.sun` files |
