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
**Status:** ⬜ Not Started

| Feature | Syntax | Status |
|---------|--------|--------|
| String + String | `"hello " + "world"` | ⬜ |
| String + Quantity | `"Length: " + 100 {mm}` | ⬜ |

**Behavior:**
- Concatenating two strings produces a combined string
- Concatenating a string with a quantity uses the display format with units (e.g., `"100 mm"`)

**Implementation Notes:**
- Extend binary expression handling for `+` operator with string operands
- Add `StringResult` type if not already present
- Implement quantity-to-string conversion using existing display formatting

---

### String Interpolation
**Status:** ⬜ Not Started

| Feature | Syntax | Status |
|---------|--------|--------|
| Interpolation | `"Depth {expression}"` | ⬜ |

**Syntax:**
```sunset
Length = 100 {mm}
Message = "The length is {Length}"  // Results in "The length is 100 mm"

// Inline expressions
Summary = "Area: {Width * Height}"
```

**Behavior:**
- Expressions within `{...}` inside a string are evaluated and converted to their display format
- Quantities include their units in the interpolated output

**Implementation Notes:**
- Modify lexer to handle interpolation tokens within strings
- Add `InterpolatedStringExpression` to parse interpolated segments
- Evaluate each segment and concatenate results

---

### List Join Method
**Status:** ⬜ Not Started

| Feature | Syntax | Status |
|---------|--------|--------|
| Join strings | `list.join(separator)` | ⬜ |

**Syntax:**
```sunset
Words = ["hello", "world"]
Sentence = Words.join(", ")  // Results in "hello, world"
```

**Behavior:**
- Joins a list of strings using the specified separator
- Returns a single concatenated string

**Implementation Notes:**
- Add `JoinMethod` to `src/Sunset.Parser/BuiltIns/ListMethods/`
- Type check that the list contains strings and separator is a string
- Implement in evaluator using standard string join logic

---

## Priority 5: Functional Programming

### Default Return Value
**Status:** ⬜ Not Started

Allows elements to be used as inline functions by returning a default value when instantiated without property access.

| Feature | Description | Status |
|---------|-------------|--------|
| Implicit return | Last defined value is default | ⬜ |
| `return` keyword | Explicit default value marker | ⬜ |

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

**Implementation Notes:**
- Add `return` keyword to lexer (`TokenType.Return`)
- Modify `ElementDeclaration` to track the default return variable
- Add validation for single `return` usage per element
- Add validation for empty element instantiation
- Modify element instantiation evaluation to return default value when no property is accessed

---

### Partial Application (Element Re-instantiation)
**Status:** ⬜ Not Started

Allows creating new element instances based on existing instances, preserving unchanged input values.

| Feature | Description | Status |
|---------|-------------|--------|
| Re-instantiation | `existingInstance(property = value)` | ⬜ |
| Property inheritance | Unchanged properties copied from source | ⬜ |
| Type inference | Type inferred from source instance | ⬜ |

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

**Implementation Notes:**
- Modify `CallExpression` handling to detect when callee is an element instance vs. element definition
- Implement instance cloning with property override logic
- Add type inference for re-instantiation expressions
- **TODO:** Verify whether type inference for simple instantiation expressions is already implemented

---

## Priority 6: Type System Extensions

### Options Type
**Status:** ⬜ Not Started

| Feature | Description | Status |
|---------|-------------|--------|
| Options definition | `Options(key1: "desc1", key2: "desc2")` | ⬜ |
| Option value selection | Select from defined options | ⬜ |
| Exhaustive matching | Omit `else` when all options covered | ⬜ |

**Implementation Notes:**
- Add Options as a special element or type
- Track option values for exhaustive matching
- Integrate with conditional type checking

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
| String Operations | 4 | 0 | 0 | 4 |
| Functional Programming | 5 | 0 | 0 | 5 |
| Dictionaries | 7 | 6 | 0 | 1 |
| Options | 3 | 0 | 0 | 3 |
| Element Inheritance | 5 | 1 | 0 | 4 |
| Anonymous Elements | 2 | 0 | 0 | 2 |
| Element Groups | 2 | 0 | 0 | 2 |
| **Total** | **49** | **25** | **1** | **23** |

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
