# Types of Variables

A variable can take on any of the following types:

- Constants
- Expressions
- Elements
- Conditionals
- Lists *(Not Yet Implemented)*
- Dictionaries *(Not Yet Implemented)*

## Anonymous Variables

Temporary variables are defined using the character `?` at the beginning of the line. These variables are not accessible to external calculations and are useful for providing intermediate calculation steps without cluttering reports.

In the below example, the `numerator` and `denominator` variables are used to calculate the result but are defined as anonymous variables. They are not reported or accessible from outside the element they are defined in.

```sunset
?numerator = x / 35
?denominator = y / 40
Result <r> = numerator / denominator
```

## Constants

A constant can be a single value or an expression that evaluates to a number.

```sunset
// Single value constant
YieldStrength <f_y> = 250 {MPa}
    d: The yield strength of steel.

// Expression containing multiple constants
Area <A> = 100 {mm} * 30 {mm}
    d: Cross-sectional area of plate.

// Expression that evaluates to a constant
AxialCapacity <N> = f_y * A
    d: Axial capacity of plate.
```

## Expressions

Expressions combine variables, constants, and operators to produce calculated values.

```sunset
x = 35 + 12
y = x * 2
z = x - y
```

Expressions can include:
- Arithmetic operators: `+`, `-`, `*`, `/`, `^`
- Parentheses for grouping: `(a + b) * c`
- Variable references
- Unit assignments

See [Getting Started](getting-started.md) for more details on calculations.

## Elements

### As Variables in Calculations

Elements can be instantiated and used as variables. Access their properties with the `.` operator:

```sunset
define Square:
    inputs:
        Width <w> {mm} = 100 {mm}
        Length <l> {mm} = 200 {mm}
    outputs:
        Area <A> {mm^2} = Width * Length
end

mySquare = Square(Width = 150 {mm})
result = mySquare.Area
```

### As Variables in Other Elements

Elements can be composed within other elements:

```sunset
define Section:
    inputs:
        Width <w> = 10 {mm}
        Depth <d> = 100 {mm}
    outputs:
        Area <A> = Width * Depth
        @I_xx = Width * Depth^3 / 12
end

define IsotropicMaterial:
    inputs:
        YieldStrength <f_y> = 300 {MPa}
        Density <rho> = 7800 {kg/m^3}
end

define Beam:
    inputs:
        Section = Section()
        Material = IsotropicMaterial()
    outputs:
        AxialCapacity <N> = Section.Area * Material.YieldStrength
        BendingCapacity <M> = Section.I_xx * Material.YieldStrength
        Weight <w> = Section.Area * Material.Density
end
```

Note that for elements used as variables, they do not need to define a symbol as there is no straightforward way of printing them to the screen.

## Conditionals

Variables can take on different values depending on conditions. See [Conditionals](conditionals.md) for more information.

```sunset
x = 15
y = 12 if x > 10
  = 3 otherwise
```

## Lists

> **Status: Not Yet Implemented**
>
> List functionality is planned but not currently available.

Lists contain zero or more variables of the same type. They are defined using square brackets `[` and `]`, with items separated by commas:

```sunset
reinforcementDiameters = [12 {mm}, 16 {mm}, 20 {mm}, 24 {mm}, 28 {mm}, 32 {mm}]
```

Planned operations:
- `list[index]` - Access by index
- `list.first()` - Get first element
- `list.last()` - Get last element

See [Functions on Collections](functions-on-collections.md) for more information on planned collection operations.

## Dictionaries

> **Status: Not Yet Implemented**
>
> Dictionary functionality is planned but not currently available.

Dictionaries are lists of key-value pairs. They are defined with square brackets, with each item as a `key: value` pair separated by commas:

```sunset
windSpeed = ["A2": 45 {m/s}, "B1": 52 {m/s}]
```

All keys must be of the same type and all values must be of the same type, but keys and values do not have to match types.

Planned operations:
- `dict[key]` - Access by key
- `dict[~key]` - Linear interpolation between keys
- `dict[~key-]` - Find value just below key
- `dict[~key+]` - Find value just above key

See [Functions on Collections](functions-on-collections.md) for more information on planned dictionary operations.
