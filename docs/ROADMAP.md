# Sunset Language Roadmap

This document tracks features that are documented but not yet implemented in the Sunset language.

## Implementation Status Legend

- 🔶 Partially Implemented
- ⬜ Not Started

---

## Priority 1: Core Language Features

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

### Dictionaries
**Status:** 🔶 Partially Implemented

| Feature | Syntax | Status |
|---------|--------|--------|
| Iteration | `dict.foreach(expression)` | ⬜ |

**Implementation Notes:**
- Add `foreach` method support to dictionary types
- Implement iteration over key-value pairs in `Evaluator.cs`

---

## Priority 3: Type System Extensions

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

## Priority 4: Element System

### Element Inheritance
**Status:** 🔶 Partially Implemented

| Feature | Description | Status |
|---------|-------------|--------|
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

## Priority 5: SunMd Format

### SunMd Document Rendering
**Status:** 🔶 Partially Implemented

A new file format (`.sunmd`) that combines Markdown with Sunset code blocks. Code blocks are replaced with LaTeX mathematics and SVG diagrams.

| Feature | Description | Status |
|---------|-------------|--------|
| Code block parsing | Extract `sunset` fenced code blocks from Markdown | 🔶 |
| Shared scope | Variables accessible across code blocks | 🔶 (see known issues) |
| LaTeX rendering | Convert calculations to `$$...$$` blocks | 🔶 |
| Diagram detection | Detect `DiagramElement` instances | 🔶 |
| SVG embedding | Inline SVG for diagram outputs | 🔶 |
| CLI `render` command | `sunset render file.sunmd` | 🔶 |
| HTML output | KaTeX rendering with `--html` flag | 🔶 |
| Error handling | `--continue` flag for inline errors | 🔶 |
| String conditionals | Render string conditional expressions | ⬜ |
| String variables | Render string literals and concatenated strings | ⬜ |
| sqrt function | Render `sqrt()` as LaTeX `\sqrt{}` | ⬜ |
| Symbol subscript braces | Auto-wrap multi-char subscripts (e.g. `Z_ex` → `Z_{ex}`) | ⬜ |
| Preserve declared units | Render in declared unit without simplification (e.g. `{kN/m}` stays as kN/m) | ⬜ |

**Known Issues:**
- **Multi-block incremental analysis bug:** Documents with multiple `sunset` code blocks fail due to `Environment.Analyse()` re-analyzing all scopes when each new block is added, causing type-checking errors to cascade. Workaround: use a single code block per document. Fix requires implementing true incremental analysis that only processes newly added scopes.
- **String conditionals not supported:** Variables with string conditional expressions (e.g. `Result = "OK" if x < 1 = "Not OK" otherwise`) throw `NotImplementedException` during rendering.
- **String variables not supported:** Any string variable (including simple literals and SVG markup) throws `NotImplementedException` during rendering. This blocks inline diagram generation via string interpolation.
- **sqrt function rendering:** The `sqrt()` function renders as "Error!" in LaTeX output.

**Implementation Notes:**
- Uses MarkDig for Markdown parsing
- Reuses `MarkdownVariablePrinter` for LaTeX generation
- Detects `DiagramElement` via prototype chain traversal
- See `docs/sunset-md.md` for full specification

**Key Files:**
- `src/Sunset.Markdown/SunMd/SunMdProcessor.cs` - Main processor
- `src/Sunset.Markdown/SunMd/DiagramDetector.cs` - Diagram detection
- `src/Sunset.CLI/Commands/RenderCommand.cs` - CLI command

---

## Priority 6: Standard Library

### Units
**Status:** 🔶 Partially Implemented

| Feature | Description | Status |
|---------|-------------|--------|
| Percentage unit | Add `{percent}` unit to display dimensionless values as percentages (similar to degrees/radians) | ⬜ |

**Implementation Notes:**
- Percentage should work like angle units where the underlying value is dimensionless but displayed with a `%` symbol
- `0.5 {percent}` should display as `50%`
- Conversion: `value {percent}` = `value * 100` for display

---

## Summary

| Category | Total | 🔶 | ⬜ |
|----------|-------|-----|-----|
| Logical Operators | 3 | 1 | 2 |
| Dictionaries | 1 | 0 | 1 |
| Options | 6 | 5 | 1 |
| Element Inheritance | 4 | 0 | 4 |
| Anonymous Elements | 2 | 0 | 2 |
| Element Groups | 2 | 0 | 2 |
| SunMd Format | 13 | 8 | 5 |
| Standard Library | 1 | 0 | 1 |
| **Total** | **32** | **14** | **18** |

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
