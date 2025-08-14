# Types of variables

A variable can take on any of the following types:

- Constants
- Expressions
- Elements
- Conditionals
- Lists
- Dictionaries

## Anonymous

Temporary variables are defined using the character ? at the beginning of the line. These variables are not accessible
to be used in external calculations, but are useful for providing intermediate calculation steps.

In the below example, the `numerator` and `denominator` variables are used to calculate the result but are defined as
anonymous variables and are not reported or included in any future calculations. They cannot be accessed from outside
the element that they are defined in, if they are defined in an element.

```
?numerator = x / 35
?denominator = y / 40
Result <r> = numerator / denominator
```

## Constants

A constant can be a single value or an expression that evaluates to a number of constants.

```
# Single value constant
YieldStrength <f_y> = 250 {MPa} "The yield strength of steel."

# Expression containing multiple constants
Area <A> = 100 {mm} * 30 {mm} "Cross-sectional area of plate."

# Expression that evaluates to a constant
AxialCapacity <N> = f_y * A "Axial capacity of plate."
```

## Expressions

Expressions are described in the [Fundamentals](fundamentals.md) section.

## Elements

### As variables in calculations

Elements can be instantiated as variables

The variables within an element can be accessed with the `.` modifier.

### As variables in other elements

Elements can also be used as variables in other elements.

If we wanted to calculate the elastic capacity of a section, we might define some elements as below:

```
Section:
    inputs:
        Width <w> = 10 {mm}
        Depth <d> = 100 {mm}
    
    Area <A> = w * d
    @I_xx = w * d^3 / 12

IsotropicMaterial:
    inputs:
        YieldStrength <f_y> = 300 {MPa}
        Density <\rho> = 7800 {kg / m^3}
        
Beam:
    inputs:
        Section = Section()
        Material = IsotropicMaterial(YieldStrength: 250 {MPa}) 
    
    AxialCapacity <N> = Section.Area * Material.YieldStrength
    BendingCapacity <M> = Section.I_xx * Material.YieldStrength
    Weight <w> = Section.Area * Material.Density
```

Note that for elements used as variables, they do not need to define a symbol as there is no straightforward way of
printing them to the screen.

> [!NOTE] Consider whether they should be provided with a symbol or some kind of name for the purpose of reporting and
> UI generation.

## Conditionals

Variables can take on different values depending on whether certain conditions are met in the code. See [Conditionals](conditionals.md) for more information.

## Lists

Lists contain zero or more variables within them. They are defined using square brackets `[` and `]`, with items separated with commas.

All items within a list must be of the same type.

They are expressed as:

```sunset
reinforcementDiameters = [12 {mm}, 16 {mm}, 20 {mm}, 24 {mm}, 28 {mm}, 32 {mm}, 36 {mm}, 40 {mm}]
```

Items within a list can be accessed using square brackets notation `list[index : integer]`, and the first and last elements can be accessed using the `.first()` and `.last()` functions respectively.

See [the other collection functions](functions-on-collections.md) for more information on how to iterate over lists.

## Dictionaries

Dictionaries are lists of key-value pairs. They are also defined with square brackets, with each item defined as a `key : value` pair and separated with commas.

All keys must be of the same type and all values must be of the same type, but they do not have to match.

```sunset
windSpeed = ["A2" : 45 {m/s}, "B1" : 52 {m/s}]
```

Items within a dictionary can be accessed similar to those within a list.

See [the other collection functions](functions-on-collections.md) for more information on how to iterate over dictionaries.

Other features of dictionaries:

- Linear interpolation between keys: `dict[~key]`
- Finding the value just below the key: `dict[~key-]`
- Finding the value just above the key: `dict[~key+]`
- Goal seek style iteration **syntax to be confirmed**
