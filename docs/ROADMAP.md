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
**Status:** ⬜ Not Started

| Feature | Syntax | Status |
|---------|--------|--------|
| Dictionary literal | `["key1": value1, "key2": value2]` | ⬜ |
| Key access | `dict[key]` | ⬜ |
| Linear interpolation | `dict[~key]` | ⬜ |
| Find below key | `dict[~key-]` | ⬜ |
| Find above key | `dict[~key+]` | ⬜ |
| Iteration | `dict.foreach(expression)` | ⬜ |

**Implementation Notes:**
- Add `DictionaryResult` type
- Implement dictionary literal parsing (differentiate from list by `:` separator)
- Add interpolation operators to lexer

---

## Priority 3: Type System Extensions

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

## Priority 4: Element System

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
| Dictionaries | 6 | 0 | 0 | 6 |
| Options | 3 | 0 | 0 | 3 |
| Element Inheritance | 5 | 1 | 0 | 4 |
| Anonymous Elements | 2 | 0 | 0 | 2 |
| Element Groups | 2 | 0 | 0 | 2 |
| **Total** | **38** | **18** | **1** | **19** |

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
