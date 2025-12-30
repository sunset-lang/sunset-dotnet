![image](docs/assets/logo.svg)

This repository contains the .NET version of the Sunset compiler and runtime.

## Project Structure

The solution is organized into the following projects:

### Core Libraries

- **Sunset.Quantities**: Unit system with dimensional analysis. Handles quantities with physical units using SI base units internally.
- **Sunset.Parser**: Compiler frontend including lexer, parser, and 4-stage analysis pipeline:
  1. Name Resolution
  2. Cycle Detection
  3. Type Checking
  4. Evaluation
- **Sunset.Reporting**: Report generation interfaces for outputting calculation results.
- **Sunset.Markdown**: Markdown/LaTeX output implementation using Markdig.

### Applications

- **Sunset.Docsite**: Blazor WebAssembly app with Monaco editor for live compilation.
- **Sunset.CLI**: Command-line interface for running Sunset files.

### Tests

- **Sunset.Quantities.Tests**: Unit tests for the quantities library.
- **Sunset.Parser.Tests**: Unit tests for lexer, parser, and analysis passes.
- **Sunset.Markdown.Tests**: Integration tests for report generation.

## Getting Started

### Dependencies

- .NET 9.0+

### Build & Run

```bash
# Build entire solution
dotnet build sunset-dotnet.slnx

# Run all tests
dotnet test sunset-dotnet.slnx

# Run the Blazor docsite locally
dotnet run --project src/Sunset.Docsite

# Run the CLI
dotnet run --project src/Sunset.CLI -- <file.sun>
```

## Documentation

This repository also contains the [documentation site](https://sunset-lang.github.io/sunset-docs) for the Sunset Language.

See [docs/ROADMAP.md](docs/ROADMAP.md) for current implementation status and planned features.

## Unit Terminology

The below is based on the SI system of units.

- **Base units**: Units that apply to only one dimension, e.g. metres, kilograms. All base units are coherent.
- **Coherent units**: Units where there are no multipliers applied to the dimensions, e.g. metres, kilograms, pascals.
- **Named/defined units**: Units that have been assigned a special name, e.g. metres, millimetres, pascals, kilopascals.
- **Derived units**: Units that apply to more than one dimension, e.g. kilopascals, newtons.
