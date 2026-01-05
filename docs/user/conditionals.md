# Conditionals

Conditions are expressions that evaluate to `true` or `false`. They can be used to assign different values to variables based on criteria.

## Comparison Operators

### Value Comparisons

| Operator | Description |
|----------|-------------|
| `==` | Equal to |
| `!=` | Not equal to |
| `<` | Less than |
| `<=` | Less than or equal to |
| `>` | Greater than |
| `>=` | Greater than or equal to |

### Type Comparisons

| Operator | Description |
|----------|-------------|
| `is` | Types are equivalent |

### Logical Operators

> **Note:** Logical operators `and` and `or` are not yet implemented.

| Operator | Description | Status |
|----------|-------------|--------|
| `and` | Logical AND | Not yet implemented |
| `or` | Logical OR | Not yet implemented |

## Single-Line If Expressions

The simplest and most commonly used form of conditional:

```sunset
x = 15
y = 12 if x > 10
  = 3 otherwise
```

## Multi-Branch If Expressions

Multiple conditions can be chained:

```sunset
x = 30
y = 10 if x < 12
  = 15 if x >= 30
  = 20 otherwise
```

Conditions are evaluated sequentially. The first condition that evaluates to `true` determines the value.

## Block If Expressions

For more complex conditionals, use block syntax with `if`, additional `if` branches, `otherwise`, and `end`:

```sunset
@x =
  if y < 20 {mm}:
    A + B * C
  if y < 30 {mm}:
    35 {MPa} "Description for this branch" {Reference}
  otherwise:
    40 {MPa}
      d: Default description
      r: Default reference
  end
```

### Block Syntax Rules

- Each branch ends with a colon `:`
- The expression follows on the same line or indented on subsequent lines
- Use additional `if` statements for more conditions
- End with `otherwise:` for the default case
- Close with `end`

## Comparison Form

An alternative syntax when comparing a single variable against multiple values:

```sunset
@x =
  if y:
    > 20 {mm}:
      A + B * C
    < 30 {mm}:
      35 {MPa}
    otherwise:
      40 {MPa}
  end
```

The comparison can include combinations like `> 20 {mm} or < 10 {mm}`.

## Conditional Rules

1. **Type Consistency**: All branch expressions must evaluate to the same type/units
2. **Required Otherwise**: Every conditional must have an `otherwise` branch
3. **Sequential Evaluation**: Conditions are checked in order; first true condition wins

## Type Pattern Matching

Type pattern matching allows you to check the concrete type of an element and optionally bind it to a variable with the specific type. This is particularly useful when working with prototypes and polymorphic elements.

### Basic Type Check

Use `is` to check if an element implements a specific prototype or is a specific element type:

```sunset
prototype Shape:
    outputs:
        return Area {m^2}
end

define Circle as Shape:
    inputs:
        Radius = 1 {m}
    outputs:
        return Area {m^2} = 3.14159 * Radius ^ 2
end

myShape {Shape} = Circle(2 {m})

result = "circle" if myShape is Circle
       = "other" otherwise
```

### Pattern Matching with Binding

When you need to access properties specific to a concrete element type, use pattern matching with a binding variable:

```sunset
prototype Shape:
    outputs:
        return Area {m^2}
end

define Rectangle as Shape:
    inputs:
        Width = 1 {m}
        Length = 2 {m}
    outputs:
        return Area {m^2} = Width * Length
end

define Circle as Shape:
    inputs:
        Radius = 1 {m}
    outputs:
        return Area {m^2} = 3.14159 * Radius ^ 2
end

define AreaCalculator:
    inputs:
        ShapeToCalculate {Shape} = Rectangle()
    outputs:
        return Area {m^2} = rect.Width * rect.Length if ShapeToCalculate is Rectangle rect
                         = 3.14159 * circ.Radius ^ 2 if ShapeToCalculate is Circle circ
                         = error otherwise
end

RectangleArea {m^2} = AreaCalculator(Rectangle(2 {m}, 3 {m}))  // 6 m^2
CircleArea {m^2} = AreaCalculator(Circle(2 {m}))               // ~12.57 m^2
```

The binding variable (e.g., `rect`, `circ`) is only in scope within its branch body and has the specific element type, allowing access to element-specific properties.

### Property Access Restrictions

When a variable is typed with a prototype annotation (e.g., `{Shape}`), you can only access properties defined in that prototype. To access properties specific to a concrete element type, you must use pattern matching:

```sunset
define Calculator:
    inputs:
        Shape {Shape} = Rectangle()
    outputs:
        // Error: Shape is typed as {Shape}, which doesn't have Width
        // InvalidWidth = Shape.Width
        
        // Correct: Use pattern matching to access Rectangle-specific properties
        return Width {m} = rect.Width if Shape is Rectangle rect
                        = 0 {m} otherwise
end
```

### Pattern Matching Rules

1. **Required Otherwise**: When using type pattern matching, an `otherwise` branch is always required
2. **Sequential Evaluation**: Patterns are checked in order; the first matching pattern wins
3. **Binding Scope**: Binding variables are only available within their branch body
4. **Type Safety**: The binding variable has the matched type, enabling access to type-specific properties

> **Future Enhancement:** Combining type patterns with boolean conditions using `and`/`or` operators is planned for a future release. For example:
> ```sunset
> = value if x is Rectangle rect and rect.Width > 0 {m}
> ```

## Examples

### Basic Comparison

```sunset
x = 15
result = 100 if x > 10
       = 50 otherwise
```

### Multiple Conditions with Units

```sunset
windPressure <p> {kPa} = 1.5 {kPa} if height > 50 {m}
                       = 1.0 {kPa} if height > 25 {m}
                       = 0.8 {kPa} otherwise
```

## Multi-Variable If Statements

> **Status: Partially Implemented**
>
> Basic support exists but advanced features may not be available.

Conditional statements can span multiple variables when complex calculations require different approaches:

```sunset
define Beam:
    inputs:
        Length = 5000 {mm}
        IsSimplySupported = true
    outputs:
        if IsSimplySupported:
            MomentCoefficient = 8
            ShearCoefficient = 2
        otherwise:
            MomentCoefficient = 12
            ShearCoefficient = 2.5
        end

        MaxMoment = Load * Length^2 / MomentCoefficient
        MaxShear = Load * Length / ShearCoefficient
end
```

### Multi-Variable Rules

- All non-anonymous variables must be defined in each branch
- Variables must have the same units/types across all branches
- Use `end` to close the conditional block

## Conditions on Options

When an [`Option`](options.md)-typed variable is used in a conditional, you can use both value equality (`==`) and variant checking (`is`):

```sunset
option BoltGrade {text}:
    "4.6/S"
    "8.8/S"
    "8.8/TB"
    "8.8/TF"
end

grade {BoltGrade} = "8.8/S"

boltCapacity = 100 {kN} if grade == "4.6/S"
             = 150 {kN} if grade is "8.8/S"
             = 180 {kN} if grade is "8.8/TB"
             = 200 {kN} if grade is "8.8/TF"
```

### Exhaustive Matching

When all option values are explicitly covered in a conditional, the `otherwise` branch can be omitted:

```sunset
option Size {m}:
    10 {m}
    20 {m}
end

x {Size} = 10 {m}

// No otherwise needed - all Size options are covered
result = 1 if x is 10 {m}
       = 2 if x is 20 {m}
```

This ensures all option values are explicitly handled. If a new option value is added later, the compiler will produce an error on any conditionals that don't handle it.
