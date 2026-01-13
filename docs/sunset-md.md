# SunMd: Markdown with Sunset Code Blocks

SunMd (`.sunmd`) is a file format that combines Markdown documentation with embedded Sunset calculations. When processed by the Sunset CLI, code blocks are replaced with rendered LaTeX mathematics and SVG diagrams.

## Purpose

SunMd enables engineers to write calculation reports that:
- Mix narrative documentation with live calculations
- Automatically render mathematical expressions in LaTeX
- Embed SVG diagrams generated from Sunset's Diagrams library
- Maintain consistent formatting across all calculations

## File Format

A `.sunmd` file is a standard Markdown file where fenced code blocks with the `sunset` language identifier are processed specially.

### Basic Syntax

````markdown
# My Calculation Report

Some introductory text explaining the calculation.

```sunset
// Sunset code here
W = 10 {kN/m}
L = 6 {m}
```

More text explaining the next step.

```sunset
// Variables from previous blocks are available
M_max = W * L^2 / 8
```
````

### Code Block Behavior

1. **Shared Scope**: Variables declared in one code block are accessible in subsequent blocks within the same document.

2. **LaTeX Output**: Calculations are rendered as aligned LaTeX equations showing:
   - The symbolic expression
   - The expression with values substituted
   - The final result with units

3. **Diagram Output**: Elements implementing `DiagramElement` (from the Diagrams library) are rendered as inline SVG.

## CLI Command

```
sunset render <file.sunmd> [options]
```

### Options

| Option | Description |
|--------|-------------|
| `-o, --output <path>` | Output file path (default: input file with `.md` extension) |
| `--html` | Output as HTML with KaTeX rendering instead of Markdown |
| `--continue` | Continue on errors, showing inline error messages |
| `--sf, --significant-figures <n>` | Number of significant figures for numeric output |

### Examples

```bash
# Render to Markdown
sunset render calculation.sunmd

# Render to HTML with KaTeX
sunset render calculation.sunmd --html -o report.html

# Continue on errors (show inline error messages)
sunset render calculation.sunmd --continue
```

## Example

### Input (`beam-design.sunmd`)

````markdown
# Simply Supported Beam Design

## Loading Parameters

Define the distributed load and span:

```sunset
W = 10 {kN/m}  // Distributed load
L = 6 {m}      // Span length
```

## Maximum Bending Moment

For a simply supported beam with uniformly distributed load:

```sunset
M_max = W * L^2 / 8
```

## Required Section Modulus

Assuming Grade 300 steel:

```sunset
f_y = 300 {MPa}
S_req = M_max / f_y
```
````

### Output (`beam-design.md`)

```markdown
# Simply Supported Beam Design

## Loading Parameters

Define the distributed load and span:

$$
\begin{alignedat}{2}
W &= 10 \text{ kN/m}\\
L &= 6 \text{ m}\\
\end{alignedat}
$$

## Maximum Bending Moment

For a simply supported beam with uniformly distributed load:

$$
\begin{alignedat}{2}
M_{max} &= \frac{W \times L^{2}}{8}\\
&= \frac{10 \text{ kN/m} \times (6 \text{ m})^{2}}{8}\\
&= 45 \text{ kN m}\\
\end{alignedat}
$$

## Required Section Modulus

Assuming Grade 300 steel:

$$
\begin{alignedat}{2}
f_{y} &= 300 \text{ MPa}\\
S_{req} &= \frac{M_{max}}{f_{y}}\\
&= \frac{45 \text{ kN m}}{300 \text{ MPa}}\\
&= 150000 \text{ mm}^{3}\\
\end{alignedat}
$$
```

## Diagrams

When a variable evaluates to an element implementing `DiagramElement`, the SVG output is embedded directly in the document.

### Example with Diagram

````markdown
# Cross Section

```sunset
import Diagrams

section {Diagram} = Diagram(
    ViewportWidth = 200,
    ViewportHeight = 300,
    Scale = 500,
    Elements = [
        Rectangle(
            centre = Point(x = 0.1 {m}, y = 0.15 {m}),
            width = 0.15 {m},
            height = 0.25 {m}
        )
    ]
)
```
````

This renders the diagram as an inline SVG in the output.

## Error Handling

By default, the `render` command fails if any code block contains errors. Use `--continue` to show errors inline:

```markdown
<div class="sunmd-error">
<strong>Error in code block 2:</strong>
<pre>Undefined variable: undefined_var</pre>
</div>
```

## Imports

Standard Sunset imports work within `.sunmd` files:

````markdown
```sunset
import Diagrams
import Diagrams.Geometry
```
````

The StandardLibrary is automatically available for all imports.

## Best Practices

1. **Organize code logically**: Group related calculations in the same code block.

2. **Use comments**: Add `//` comments in code blocks for clarity (they are not rendered in output).

3. **Progressive disclosure**: Define inputs first, then build up to final results.

4. **Meaningful variable names**: Use descriptive names that render well in LaTeX (underscores become subscripts).

## Limitations

- Code blocks must be complete, valid Sunset code
- Circular references between code blocks are not allowed
- The `--html` output requires a browser or viewer that supports KaTeX
