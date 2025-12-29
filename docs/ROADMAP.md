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

#### Implementation Plan

##### Step 1: Create Built-in Function Infrastructure

**1.1 Add BuiltInFunction enum and registry**
- Create `src/Sunset.Parser/BuiltIns/BuiltInFunction.cs`
- Define enum with: `Sqrt`, `Sin`, `Cos`, `Tan`, `Asin`, `Acos`, `Atan`
- Create `BuiltInFunctions` static class with:
  - `IsBuiltIn(string name)` - check if name is a built-in function
  - `Get(string name)` - return the BuiltInFunction enum value
  - `GetArgumentCount(BuiltInFunction func)` - return expected arg count (1 for all math functions)

**1.2 Add BuiltInFunctionType result type**
- Create `src/Sunset.Parser/Results/Types/BuiltInFunctionType.cs`
- Holds reference to which built-in function this represents
- Used during type checking to verify argument types

##### Step 2: Update Name Resolution

**File:** `src/Sunset.Parser/Analysis/NameResolution/NameResolver.cs`

**2.1 Modify `Visit(CallExpression)` method**
- Before checking for element declarations, check if target is a `NameExpression`
- If the name matches a built-in function:
  - Mark the expression with PassData indicating it's a built-in call
  - Skip element resolution logic
  - Still resolve argument expressions

**2.2 Add helper method**
```csharp
private bool TryResolveBuiltInFunction(CallExpression dest, IScope scope)
{
    if (dest.Target is not NameExpression nameExpr) return false;
    if (!BuiltInFunctions.IsBuiltIn(nameExpr.Name)) return false;

    dest.SetPassData("BuiltInFunction", BuiltInFunctions.Get(nameExpr.Name));
    return true;
}
```

##### Step 3: Update Type Checking

**File:** `src/Sunset.Parser/Analysis/TypeChecking/TypeChecker.cs`

**3.1 Add built-in function type checking**
- In `Visit(CallExpression)`, check for built-in function PassData
- For math functions:
  - Verify exactly 1 argument
  - For `sqrt`: argument can have any unit (result unit = sqrt of input unit)
  - For trig functions (`sin`, `cos`, `tan`): argument should be dimensionless (angles)
  - For inverse trig (`asin`, `acos`, `atan`): argument must be dimensionless, result is dimensionless (radians)

**3.2 Return appropriate result type**
- `sqrt(x)`: If x has unit U², result has unit U; otherwise dimensionless
- Trig functions: Always return dimensionless

##### Step 4: Update Evaluator

**File:** `src/Sunset.Parser/Visitors/Evaluation/Evaluator.cs`

**4.1 Modify `Visit(CallExpression)` to handle built-in functions**
```csharp
private IResult Visit(CallExpression dest, IScope currentScope)
{
    // Check for built-in function
    if (dest.TryGetPassData<BuiltInFunction>("BuiltInFunction", out var builtIn))
    {
        return EvaluateBuiltInFunction(builtIn, dest, currentScope);
    }

    // Existing element instantiation logic...
}
```

**4.2 Implement `EvaluateBuiltInFunction` method**
```csharp
private IResult EvaluateBuiltInFunction(BuiltInFunction func, CallExpression call, IScope scope)
{
    var argResult = Visit(call.Arguments[0].Expression, scope);
    if (argResult is not QuantityResult quantityResult)
    {
        return new ErrorResult("Math functions require numeric arguments");
    }

    var value = quantityResult.Result.BaseValue;
    var resultValue = func switch
    {
        BuiltInFunction.Sqrt => Math.Sqrt(value),
        BuiltInFunction.Sin => Math.Sin(value),
        BuiltInFunction.Cos => Math.Cos(value),
        BuiltInFunction.Tan => Math.Tan(value),
        BuiltInFunction.Asin => Math.Asin(value),
        BuiltInFunction.Acos => Math.Acos(value),
        BuiltInFunction.Atan => Math.Atan(value),
        _ => throw new NotImplementedException($"Built-in function {func} not implemented")
    };

    // Handle unit for sqrt, dimensionless for others
    var resultUnit = func == BuiltInFunction.Sqrt
        ? GetSqrtResultUnit(quantityResult.Result.Unit)
        : DefinedUnits.Dimensionless;

    return new QuantityResult(new Quantity(resultValue, resultUnit));
}
```

##### Step 5: Add Tests

**File:** `tests/Sunset.Parser.Tests/Integration/MathFunctions.Tests.cs`

**5.1 Test cases to implement:**
- `sqrt(4)` → `2`
- `sqrt(9 m²)` → `3 m` (unit handling)
- `sin(0)` → `0`
- `cos(0)` → `1`
- `tan(0)` → `0`
- `asin(0)` → `0`
- `acos(1)` → `0`
- `atan(0)` → `0`
- Test with variables: `x = 16` then `sqrt(x)` → `4`
- Test in expressions: `2 * sqrt(9)` → `6`
- Error cases: wrong argument count, non-numeric arguments

##### Step 6: Update Documentation

- Update this ROADMAP.md to mark functions as ✅ Implemented
- Add examples to language documentation if it exists

#### Files to Modify

| File | Changes |
|------|---------|
| `src/Sunset.Parser/BuiltIns/BuiltInFunction.cs` | **NEW** - Built-in function enum and registry |
| `src/Sunset.Parser/Results/Types/BuiltInFunctionType.cs` | **NEW** - Type for built-in functions |
| `src/Sunset.Parser/Analysis/NameResolution/NameResolver.cs` | Handle built-in function resolution |
| `src/Sunset.Parser/Analysis/TypeChecking/TypeChecker.cs` | Type check built-in function calls |
| `src/Sunset.Parser/Visitors/Evaluation/Evaluator.cs` | Evaluate built-in functions |
| `tests/Sunset.Parser.Tests/Integration/MathFunctions.Tests.cs` | **NEW** - Test suite |

#### Design Decisions

1. **No new tokens needed**: Math function names remain identifiers; they're recognized during name resolution, not lexing
2. **PassData pattern**: Use existing PassData mechanism to mark CallExpressions as built-in calls
3. **Unit handling for sqrt**: Special case - if input has unit U², result has unit U
4. **Trig functions expect radians**: Standard mathematical convention; angles should be dimensionless
5. **Extensible design**: The BuiltInFunction enum and switch pattern makes adding more functions easy

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
**Status:** ⬜ Not Started

| Feature | Syntax | Status |
|---------|--------|--------|
| List literal | `[item1, item2, item3]` | ⬜ |
| Index access | `list[index]` | ⬜ |
| First element | `list.first()` | ⬜ |
| Last element | `list.last()` | ⬜ |

**Implementation Notes:**
- Add `ListResult` type
- Implement list literal parsing
- Add `CollectionAccess` expression type

---

### Lists/Arrays - Advanced
**Status:** ⬜ Not Started

| Feature | Syntax | Status |
|---------|--------|--------|
| Iteration | `list.foreach(expression)` | ⬜ |
| Minimum | `list.min()` | ⬜ |
| Maximum | `list.max()` | ⬜ |
| Average | `list.average()` | ⬜ |
| Filter | `list.where(condition)` | ⬜ |
| Map | `list.select(expression)` | ⬜ |

**Implementation Notes:**
- Implement method call syntax for list operations
- Add `value` and `index` keywords for foreach expressions

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
| Lists - Basic | 4 | 0 | 0 | 4 |
| Lists - Advanced | 6 | 0 | 0 | 6 |
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
