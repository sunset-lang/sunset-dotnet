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
| `is not` | Types are not equivalent |

### Logical Operators

> **Note:** Logical operators are defined in the lexer but may have limited implementation.

| Operator | Description |
|----------|-------------|
| `and` | Logical AND |
| `or` | Logical OR |
| `not` | Logical NOT |

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

For more complex conditionals, use block syntax with `if`, `else if`, `else`, and `end`:

```sunset
@x =
  if y < 20 {mm}:
    A + B * C
  else if y < 30 {mm}:
    35 {MPa} "Description for this branch" {Reference}
  else:
    40 {MPa}
      d: Default description
      r: Default reference
  end
```

### Block Syntax Rules

- Each branch ends with a colon `:`
- The expression follows on the same line or indented on subsequent lines
- Use `else if` for additional conditions
- End with `else:` for the default case
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
    else:
      40 {MPa}
  end
```

The comparison can include combinations like `> 20 {mm} or < 10 {mm}`.

## Conditional Rules

1. **Type Consistency**: All branch expressions must evaluate to the same type/units
2. **Required Else**: Every conditional must have an `otherwise` or `else` branch
3. **Sequential Evaluation**: Conditions are checked in order; first true condition wins

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
        else:
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

> **Status: Not Yet Implemented**
>
> The following describes planned functionality.

When an [`Option`](options.md) is provided as the variable in a comparison `if` statement, the `else` branch may be omitted if all options are covered:

```sunset
options BoltTypes = ["4.6/S", "8.8/S", "8.8/TB", "8.8/TF"]

boltCapacity =
  if BoltTypes:
    is "4.6/S":
      100 {kN}
    is "8.8/S":
      150 {kN}
    is "8.8/TB":
      180 {kN}
    is "8.8/TF":
      200 {kN}
  end
```

This ensures all option values are explicitly handled, preventing errors when new options are added.
