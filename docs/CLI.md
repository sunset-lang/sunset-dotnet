# Sunset CLI

The Sunset command-line interface (CLI) is a tool for compiling, analyzing, and executing scripts written in the Sunset language—a domain-specific language for calculations involving physical quantities and units.

## Installation

### From .NET Tool

```bash
dotnet tool install --global Sunset.CLI
```

### From Source

```bash
git clone https://github.com/example/sunset-dotnet.git
cd sunset-dotnet
dotnet build
dotnet pack src/Sunset.CLI -o ./artifacts
dotnet tool install --global --add-source ./artifacts Sunset.CLI
```

After installation, the `sunset` command will be available in your terminal.

## Commands

### sunset run

Executes a Sunset script and displays the results.

```bash
sunset run <FILE> [options]
```

#### Arguments

| Argument | Description |
|----------|-------------|
| `<FILE>` | Path to the Sunset source file (`.sun` or `.sunset`) |

#### Options

| Option | Description |
|--------|-------------|
| `-o, --output <FILE>` | Write output to a file instead of stdout |
| `-f, --format <FORMAT>` | Output format: `text` (default), `markdown`, `html`, `json` |
| `--no-color` | Disable colored output |
| `-v, --verbose` | Show detailed evaluation steps |
| `-q, --quiet` | Suppress all output except errors |
| `--sf, --significant-figures <N>` | Number of significant figures for results (default: 4) |
| `--dp, --decimal-places <N>` | Number of decimal places for results |
| `--si-units` | Display results in SI base units only |
| `--simplify-units` | Automatically simplify derived units |

#### Examples

```bash
# Run a script and display results
sunset run calculations.sun

# Run with markdown output
sunset run calculations.sun --format markdown

# Run and save output to file
sunset run calculations.sun -o results.md -f markdown

# Run with 6 significant figures
sunset run calculations.sun --sf 6

# Run with verbose output showing evaluation steps
sunset run calculations.sun --verbose
```

### sunset check

Analyzes a Sunset script for errors without executing it. Useful for validation and IDE integration.

```bash
sunset check <FILE> [options]
```

#### Arguments

| Argument | Description |
|----------|-------------|
| `<FILE>` | Path to the Sunset source file |

#### Options

| Option | Description |
|--------|-------------|
| `--warnings-as-errors` | Treat warnings as errors (exit with non-zero code) |
| `-f, --format <FORMAT>` | Output format: `text` (default), `json`, `sarif` |
| `--no-color` | Disable colored output |

#### Examples

```bash
# Check a file for errors
sunset check calculations.sun

# Check with JSON output (useful for tooling)
sunset check calculations.sun --format json

# Strict mode: fail on warnings
sunset check calculations.sun --warnings-as-errors
```

#### Exit Codes

| Code | Meaning |
|------|---------|
| 0 | No errors found |
| 1 | Syntax or semantic errors found |
| 2 | Warnings found (only with `--warnings-as-errors`) |

### sunset build

Compiles one or more Sunset source files into a report document.

```bash
sunset build <FILES...> [options]
```

#### Arguments

| Argument | Description |
|----------|-------------|
| `<FILES...>` | One or more Sunset source files or glob patterns |

#### Options

| Option | Description |
|--------|-------------|
| `-o, --output <FILE>` | Output file path (required) |
| `-f, --format <FORMAT>` | Output format: `markdown` (default), `html`, `pdf`, `latex` |
| `--title <TITLE>` | Document title |
| `--toc` | Include table of contents |
| `--number-headings` | Number section headings |
| `--show-symbols` | Show symbolic expressions in calculations |
| `--show-values` | Show numeric values in calculation steps |
| `--sf, --significant-figures <N>` | Number of significant figures (default: 4) |
| `--dp, --decimal-places <N>` | Number of decimal places |
| `--si-units` | Use SI base units only |
| `--simplify-units` | Automatically simplify derived units |

#### Examples

```bash
# Build a markdown report
sunset build *.sun -o report.md

# Build an HTML report with table of contents
sunset build calculations.sun -o report.html -f html --toc

# Build with symbolic expressions shown
sunset build design.sun -o design-report.md --show-symbols --show-values

# Build from multiple files
sunset build foundations.sun walls.sun roof.sun -o structural-report.md --title "Structural Calculations"
```

### sunset new

Creates a new Sunset module or file from a template.

```bash
sunset new <TEMPLATE> [NAME] [options]
```

#### Templates

| Template | Description |
|----------|-------------|
| `file` | Creates a new Sunset source file |
| `module` | Creates a new Sunset module directory |

#### Options

| Option | Description |
|--------|-------------|
| `-o, --output <PATH>` | Output path (default: current directory) |
| `--force` | Overwrite existing files |

#### Examples

```bash
# Create a new sunset file
sunset new file calculations

# Create a new module
sunset new module my-calculations

# Create in specific directory
sunset new file beam-design -o ./structural/
```

### sunset watch

Watches for file changes and automatically re-runs analysis.

```bash
sunset watch <FILE> [options]
```

#### Options

| Option | Description |
|--------|-------------|
| `--run` | Execute and display results on each change |
| `--check` | Only check for errors on each change (default) |
| `-o, --output <FILE>` | Write output to file on each change |
| `-f, --format <FORMAT>` | Output format for results |

#### Examples

```bash
# Watch and check for errors
sunset watch calculations.sun

# Watch and run, outputting results
sunset watch calculations.sun --run

# Watch and build output on changes
sunset watch calculations.sun --run -o results.md -f markdown
```

### sunset repl

Starts an interactive Read-Eval-Print Loop for experimenting with Sunset expressions.

```bash
sunset repl [options]
```

#### Options

| Option | Description |
|--------|-------------|
| `-l, --load <FILE>` | Load a file into the REPL session |
| `--sf, --significant-figures <N>` | Number of significant figures |
| `--si-units` | Use SI base units only |

#### Examples

```bash
# Start interactive session
sunset repl

# Start with a file pre-loaded
sunset repl --load constants.sun
```

#### REPL Commands

Within the REPL, special commands are prefixed with `:`:

| Command | Description |
|---------|-------------|
| `:help` | Show help information |
| `:load <file>` | Load a source file |
| `:clear` | Clear all defined variables |
| `:vars` | List all defined variables |
| `:quit` or `:q` | Exit the REPL |

## Global Options

These options are available for all commands:

| Option | Description |
|--------|-------------|
| `-h, --help` | Show help information |
| `--version` | Show version information |
| `--no-color` | Disable colored output |
| `-v, --verbosity <LEVEL>` | Set verbosity: `quiet`, `minimal`, `normal`, `detailed`, `diagnostic` |

## Configuration

### Module File (sunset.toml)

Modules can include a `sunset.toml` configuration file:

```toml
[module]
name = "my-calculations"
version = "1.0.0"
description = "Engineering calculation module"

[output]
format = "markdown"
significant_figures = 4
simplify_units = true
show_symbols = true

[build]
sources = ["src/**/*.sun"]
output = "dist/report.md"
title = "Engineering Calculations"
```

### Environment Variables

| Variable | Description |
|----------|-------------|
| `SUNSET_NO_COLOR` | Set to `1` to disable colored output |
| `SUNSET_VERBOSITY` | Default verbosity level |

## Error Output

Errors and warnings are displayed with source context:

```
error[E001]: Unit mismatch in binary operation
  --> calculations.sun:15:12
   |
15 |     force = mass + velocity
   |             ^^^^^^^^^^^^^^^ cannot add {kg} and {m/s}
   |
   = help: Ensure both operands have compatible units
```

### Error Codes

| Code | Category |
|------|----------|
| E001-E099 | Syntax errors |
| E100-E199 | Name resolution errors |
| E200-E299 | Type/unit errors |
| E300-E399 | Evaluation errors |

## Exit Codes

| Code | Meaning |
|------|---------|
| 0 | Success |
| 1 | Compilation/analysis errors |
| 2 | Invalid arguments or options |
| 3 | File not found or I/O error |
| 130 | Interrupted (Ctrl+C) |

## Examples

### Basic Calculation Script

```bash
# calculations.sun
g = 9.81 {m/s^2}       // gravitational acceleration
mass = 75 {kg}         // mass
weight = mass * g      // calculated weight in Newtons
```

```bash
$ sunset run calculations.sun
g = 9.81 m/s²
mass = 75 kg
weight = 735.75 N
```

### Structural Engineering Report

```bash
$ sunset build beam-design.sun column-design.sun \
    -o structural-report.html \
    -f html \
    --title "Structural Design Calculations" \
    --toc \
    --number-headings \
    --show-symbols
```

### CI/CD Integration

```bash
# In a CI pipeline - fail on any issues
sunset check calculations.sun --warnings-as-errors --format sarif > results.sarif
```

## See Also

- [Sunset Language Reference](./LANGUAGE.md)
- [Unit System Documentation](./UNITS.md)
- [API Documentation](./API.md)
