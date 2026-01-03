# AGENTS.md

This file provides guidance to OpenCode when working with code in this repository.

## Build Commands

```bash
# Build entire solution
dotnet build sunset-dotnet.slnx

# Run all tests
dotnet test sunset-dotnet.slnx

# Run tests for a specific project
dotnet test tests/Sunset.Parser.Tests

# Run a single test by name filter
dotnet test tests/Sunset.Parser.Tests --filter "FullyQualifiedName~TestMethodName"

# Run the Blazor docsite locally
dotnet run --project src/Sunset.Docsite

# Run the CLI on a Sunset file
dotnet run --project src/Sunset.CLI -- path/to/file.sun

# Publish docsite for deployment
dotnet publish -c Release -o release src/Sunset.Docsite
```

## Project Architecture

Sunset is a domain-specific language compiler/interpreter for handling quantities with physical units. The codebase follows a **4-stage pipeline**: Lexing → Parsing → Semantic Analysis → Evaluation/Reporting.

### Project Dependencies

```
Sunset.Quantities (no dependencies)
    ↑
Sunset.Parser (uses Quantities)
    ↑
Sunset.Reporting (uses Parser)
    ↑
Sunset.Markdown (uses Parser + Reporting)
    ↑
├── Sunset.Docsite (Blazor UI, uses Parser + Markdown)
└── Sunset.CLI (uses Parser + Markdown)
```

### Core Projects

- **Sunset.Quantities**: Unit system with dimensional analysis. `Unit` stores `Dimension[]` with exponents; `Quantity` wraps value + unit and uses SI base units internally.

- **Sunset.Parser**: Compiler frontend with lexer (`Lexing/Lexer.cs`), parser (`Parsing/Parser.cs`), and 4 sequential analysis passes orchestrated by `Environment.Analyse()`:
  1. Name Resolution (`Analysis/NameResolution/`)
  2. Cycle Detection (`Analysis/ReferenceChecking/`)
  3. Type Checking (`Analysis/TypeChecking/`)
  4. Evaluation (`Visitors/Evaluation/Evaluator.cs`)

- **Sunset.Reporting**: Report generation interfaces (`IReportPrinter`, `PrinterSettings`)

- **Sunset.Markdown**: Markdown output implementation using Markdig

- **Sunset.Docsite**: Blazor WebAssembly app with Monaco editor for live compilation

- **Sunset.CLI**: Command-line interface for running Sunset files

## Key Patterns

### AST and Visitor Pattern

- Expressions implement `IExpression` in `Expressions/`
- Declarations implement `IDeclaration` in `Parsing/Declarations/`
- Analysis passes use visitor pattern with exhaustive pattern matching
- Pass results stored in `PassData` dictionary on nodes (keyed by pass name)

### Scope Hierarchy

- `IScope` → `FileScope` → `ElementDeclaration`
- `Environment` holds `SourceFile` instances and shared `ErrorLog`
- `SourceFile` supports lazy parsing via `Parse()`

### Error Handling

- Errors collected in `ErrorLog` instance, never thrown as exceptions
- `ISemanticError`: NameResolutionError, CircularReferenceError, BinaryUnitMismatchError, DeclaredUnitMismatchError, etc.
- `ISyntaxError`: UnexpectedSymbolError, UnclosedStringError

#### Error Logging Patterns

**Avoid duplicate/cascading errors across passes**: Each analysis pass should only log errors for issues it directly discovers. When an earlier pass has already logged an error, later passes should propagate an error state without logging additional errors.

- **NameResolver** logs `NameResolutionError` for unresolved names
- **TypeChecker** should return `ErrorValueType.Instance` (not `null`) when encountering an unresolved name—this signals an error state without logging a duplicate error
- **Evaluator** should check for error states and skip operations that would fail (e.g., don't call `SetUnits()` on incompatible types)

**Use ErrorValueType for error propagation**: In the TypeChecker, return `ErrorValueType.Instance` instead of `null` when an error was already logged by a previous pass. This prevents cascading errors:

```csharp
// In TypeChecker.Visit(NameExpression)
case null:
    // Name resolution error was already logged by NameResolver
    return ErrorValueType.Instance;

// In TypeChecker.Visit(VariableDeclaration)
if (evaluatedType is ErrorValueType)
{
    // Don't log additional errors—underlying error was already logged
    return evaluatedType;
}
```

**Choose the correct error type**:
- `NameResolutionError`: Variable/symbol cannot be found
- `DeclaredUnitMismatchError`: Declared unit `{m}` doesn't match evaluated unit `{s}`
- `VariableUnitDeclarationError`: Variable has no declared unit (warning case)
- `BinaryUnitMismatchError`: Binary operation has incompatible units (e.g., `5 {m} + 3 {s}`)

**Guard against exceptions in Evaluator**: Before operations that can throw (like `Quantity.SetUnits()`), verify type compatibility:

```csharp
// Check dimensions match before calling SetUnits
if (assignedType != null && evaluatedType != null &&
    Unit.EqualDimensions(assignedType.Unit, evaluatedType.Unit))
{
    quantityResult.Result.SetUnits(assignedType.Unit);
}
```

### Results

- `IResult` hierarchy: `QuantityResult`, `ElementInstanceResult`, `BooleanResult`, `ErrorResult`, `SuccessResult`, `ListResult`

### Built-in Functions

Mathematical functions are implemented in `BuiltIns/Functions/`:
- `sqrt(x)`, `sin(x)`, `cos(x)`, `tan(x)`, `asin(x)`, `acos(x)`, `atan(x)`
- Registry in `BuiltIns/BuiltInFunction.cs`
- Type checking handles angle units; inverse trig functions return dimensionless results

### Collections

- **Lists**: `ListExpression` for `[item1, item2, ...]` syntax
- **Index Access**: `IndexExpression` for `list[index]` syntax
- **Type Checking**: `ListType` ensures element type consistency

## Testing Pattern

```csharp
var source = SourceFile.FromString("x = 35 + 12");
var env = new Environment(source);
env.Analyse();
// Assert on results via PassData, DefaultValue, ErrorLog, etc.
```

## Development Notes

- .NET 9.0 with nullable reference types enabled
- Analysis passes are pure functions—don't mutate AST during analysis
- Use base SI units internally; convert only for display
- Expression operators perform automatic simplification at construction
