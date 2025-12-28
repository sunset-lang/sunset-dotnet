# Sunset Language Roadmap

This document tracks features that are documented but not yet implemented in the Sunset language.

## Implementation Status Legend

- ✅ Implemented
- 🔶 Partially Implemented
- ⬜ Not Started

---

## Priority 1: Core Language Features

### Mathematical Functions
**Status:** ⬜ Not Started

| Function | Description | Status |
|----------|-------------|--------|
| `sqrt(x)` | Square root | ⬜ |
| `sin(x)` | Sine | ⬜ |
| `cos(x)` | Cosine | ⬜ |
| `tan(x)` | Tangent | ⬜ |
| `asin(x)` | Inverse sine | ⬜ |
| `acos(x)` | Inverse cosine | ⬜ |
| `atan(x)` | Inverse tangent | ⬜ |

**Implementation Notes:**
- Add token types for function names
- Add CallExpression handling for built-in functions
- Implement evaluation in `Evaluator.cs`
- Handle unit stripping for trig functions (they expect unitless input)

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

### Lists/Arrays
**Status:** ⬜ Not Started

| Feature | Syntax | Status |
|---------|--------|--------|
| List literal | `[item1, item2, item3]` | ⬜ |
| Index access | `list[index]` | ⬜ |
| First element | `list.first()` | ⬜ |
| Last element | `list.last()` | ⬜ |
| Iteration | `list.foreach(expression)` | ⬜ |
| Minimum | `list.min()` | ⬜ |
| Maximum | `list.max()` | ⬜ |
| Average | `list.average()` | ⬜ |
| Filter | `list.where(condition)` | ⬜ |
| Map | `list.select(expression)` | ⬜ |

**Implementation Notes:**
- Add `ListResult` type
- Implement list literal parsing
- Add `CollectionAccess` expression type
- Implement method call syntax for list operations

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

## Summary

| Category | Total | ✅ | 🔶 | ⬜ |
|----------|-------|-----|-----|-----|
| Math Functions | 7 | 0 | 0 | 7 |
| Logical Operators | 3 | 0 | 1 | 2 |
| Lists | 10 | 0 | 0 | 10 |
| Dictionaries | 6 | 0 | 0 | 6 |
| Options | 3 | 0 | 0 | 3 |
| Element Inheritance | 5 | 1 | 0 | 4 |
| Anonymous Elements | 2 | 0 | 0 | 2 |
| Element Groups | 2 | 0 | 0 | 2 |
| **Total** | **38** | **1** | **1** | **36** |

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
