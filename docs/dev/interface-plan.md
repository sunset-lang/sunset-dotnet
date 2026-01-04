# Prototypes Implementation Plan

This document outlines the implementation plan for adding prototypes (interfaces/traits) to Sunset.

## Overview

Prototypes define contracts that elements can implement, enabling polymorphism and type contracts.

### Key Decisions
- **Keyword**: `prototype`
- **Implements syntax**: `define Square as Shape, Comparable:`
- **Type annotations**: `{Shape}` for objects, `{Shape list}` for lists
- **Prototype inheritance**: Allowed via `prototype A as B:`
- **Input inheritance**: Prototype inputs inherited automatically (can be overridden)
- **Output override in child prototypes**: Error (only new inputs/outputs allowed)
- **Nominal typing**: Must explicitly use `as PrototypeName`
- **Instance access**: `value.instance.Area` for accessing element properties when `value` resolves to default return
- **Empty prototypes**: Valid (marker prototypes)
- **Name clashes**: Error when prototype/element/variable/unit/dimension share names

## Example Syntax

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
TotalArea {m^2} = Shapes.sum(value)
TotalAreaExplicit {m^2} = Shapes.sum(value.instance.Area)
```

## Implementation Phases

### Phase 1: Documentation (Specification)
- Update `docs/user/reference.md` with Prototypes section
- Update `docs/user/elements.md` with "Implementing Prototypes" section
- Update `docs/user/toc.yml` to add prototypes entry
- Create `docs/user/prototypes.md` with full tutorial

### Phase 2: Lexer
- Add tokens: `Prototype`, `As`, `List`, `Instance`
- Add keyword mappings

### Phase 3: Parser & AST
- Create `PrototypeDeclaration.cs`
- Create `PrototypeOutputDeclaration.cs`
- Create `TypeAnnotation.cs`
- Create `InstanceConstant.cs`
- Update `ElementDeclaration.cs`
- Update `VariableDeclaration.cs`
- Update `Parser.cs` and `Parser.Rules.cs`

### Phase 4: Error Types
- Create syntax errors for prototypes
- Create semantic errors for prototypes
- Create name clash error

### Phase 5: Type System
- Add `PrototypeType` class
- Update `AreCompatible()` for prototype compatibility

### Phase 6: Name Resolution
- Add visitor methods for prototype declarations
- Add name clash detection
- Resolve base prototypes and implemented prototypes

### Phase 7: Reference Checking
- Add visitor methods for prototype declarations
- Implement inheritance cycle detection

### Phase 8: Type Checking
- Add visitor methods for prototype declarations
- Implement conformance checking
- Add `_iterationElementType` tracking for `instance` access

### Phase 9: Evaluation
- Add visitor methods for prototype declarations
- Implement prototype input inheritance
- Track `_iterationInstanceValue` for `value.instance` access

### Phase 10: Integration Tests
- End-to-end tests for complete examples

### Phase 11: List Method Updates
- Update list methods to support `value.instance`

## File Summary

### New Files
| File | Purpose |
|------|---------|
| `docs/user/prototypes.md` | Dedicated prototype documentation |
| `src/Sunset.Parser/Parsing/Declarations/PrototypeDeclaration.cs` | Prototype AST node |
| `src/Sunset.Parser/Parsing/Declarations/PrototypeOutputDeclaration.cs` | Prototype output AST node |
| `src/Sunset.Parser/Expressions/TypeAnnotation.cs` | Type annotation expression |
| `src/Sunset.Parser/Parsing/Constants/InstanceConstant.cs` | Instance keyword constant |
| `src/Sunset.Parser/Errors/Syntax/PrototypeSyntaxErrors.cs` | Syntax errors |
| `src/Sunset.Parser/Errors/Semantic/PrototypeErrors.cs` | Semantic errors |
| `src/Sunset.Parser/Errors/Semantic/NameClashError.cs` | Name clash error |

### Modified Files
| File | Changes |
|------|---------|
| `docs/user/reference.md` | Add Prototypes section |
| `docs/user/elements.md` | Add "Implementing Prototypes" section |
| `docs/user/toc.yml` | Add prototypes.md entry |
| `src/Sunset.Parser/Lexing/Tokens/TokenType.cs` | Add 4 token types |
| `src/Sunset.Parser/Lexing/Tokens/TokenDefinitions.cs` | Add 4 keywords |
| `src/Sunset.Parser/Parsing/Declarations/ElementDeclaration.cs` | Add prototype properties |
| `src/Sunset.Parser/Parsing/Declarations/VariableDeclaration.cs` | Add TypeAnnotation |
| `src/Sunset.Parser/Parsing/Parser.cs` | Add prototype parsing |
| `src/Sunset.Parser/Parsing/Parser.Rules.cs` | Add instance rule |
| `src/Sunset.Parser/Results/Types/IResultType.cs` | Add PrototypeType |
| `src/Sunset.Parser/Scopes/SourceFile.cs` | Handle duplicate names |
| `src/Sunset.Parser/Analysis/NameResolution/NameResolver.cs` | Prototype resolution |
| `src/Sunset.Parser/Analysis/ReferenceChecking/ReferenceChecker.cs` | Cycle detection |
| `src/Sunset.Parser/Analysis/TypeChecking/TypeChecker.cs` | Prototype type checking |
| `src/Sunset.Parser/Visitors/Evaluation/Evaluator.cs` | Prototype evaluation |
