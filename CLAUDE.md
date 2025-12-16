# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

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
Sunset.Docsite (Blazor UI, uses Parser + Markdown)
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

- Errors collected in `ErrorLog.Log` static instance, never thrown
- `ISemanticError`: TypeResolutionError, NameResolutionError, CircularReferenceError
- `ISyntaxError`: UnexpectedSymbolError, UnclosedStringError

### Results

- `IResult` hierarchy: `QuantityResult`, `ElementInstanceResult`, `BooleanResult`, `ErrorResult`, `SuccessResult`

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
