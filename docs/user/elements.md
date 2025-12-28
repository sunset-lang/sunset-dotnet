# Elements

Elements are reusable groups of expressions that encapsulate inputs and outputs. They are similar to functions or classes in other programming languages.

## Defining Elements

An element definition consists of:
- The `define` keyword
- A name
- An `inputs:` section with input variables and their default values
- An `outputs:` section with calculated expressions
- The `end` keyword

```sunset
define PadFooting:
    inputs:
        Width <w> = 1200 {mm}
            d: Width of the footing
        Length <l> = 1600 {mm}
            d: Length of the footing
        Depth <d> = 800 {mm}
            d: Depth of the footing
    outputs:
        BearingArea <A_bearing> = Width * Length
            d: Bearing area of the footing on the ground
        Volume <V> = Width * Length * Depth
            d: Volume of the footing
end
```

The tabs/indentation are included for readability but are not strictly required.

## Instantiating Elements

Elements can be instantiated in several ways:

```sunset
// With default values only
PadFootingDefault = PadFooting()

// With all parameters (positional)
PadFootingAll = PadFooting(1400 {mm}, 2400 {mm}, 900 {mm})

// With named parameters (remaining use defaults)
PadFootingNamed = PadFooting(Width = 1500 {mm})
```

The default value of an input variable must be a constant or an element instantiated with constant parameters.

## Accessing Element Properties

The variables within an element can be accessed with the `.` operator:

```sunset
define Square:
    inputs:
        Width <w> {mm} = 100 {mm}
        Length <l> {mm} = 200 {mm}
    outputs:
        Area <A> {mm^2} = Width * Length
end

SquareInstance = Square(Width = 200 {mm}, Length = 350 {mm})
Result {mm^2} = SquareInstance.Area
```

## Elements as Variables in Other Elements

Elements can be used as input variables in other elements, allowing for composition:

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

Note that for elements used as variables, they do not need to define a symbol as there is no straightforward way of printing them in reports.

## Element Inheritance

> **Note:** Element inheritance is partially implemented. Basic syntax is supported but some advanced features may not be available.

Elements can inherit from other elements, copying the behaviour (inputs and outputs) of the parent element and allowing extension:

```sunset
define Circle:
    inputs:
        Diameter <phi> = 100 {mm}
            d: Diameter of the circle
    outputs:
        Area = (3.14159 * Diameter^2) / 4
            d: Area of the circle
end

define Reinforcement(Circle):
    inputs:
        Diameter = parent
            d: Diameter of the reinforcing bar
    outputs:
        Area = parent
end
```

The element inheriting from another must explicitly inherit all properties of the parent element using the `parent` keyword, or override them by re-defining them.

### Inheritance Rules

- All inputs and outputs must be included in the child element
- Use `parent` to inherit a property unchanged
- Override properties by providing a new definition
- Any properties not explicitly included will throw an error

> **Note:** Multiple inheritance is not supported. Interface-like behaviour may be considered for future implementation.

## Conditional Execution in Elements

> **Status: Design Phase**
>
> The following describes planned functionality that is not yet implemented.

Elements may need to conditionally execute different calculations based on input values. This can be achieved using `if` expressions within the element outputs:

```sunset
define Column:
    inputs:
        Slenderness = 20
    outputs:
        Behaviour =
            if Slenderness > 20:
                "Slender"
            otherwise:
                "Stocky"
            end
end
```

For more complex branching behaviour where entire calculation blocks differ, consider using separate elements with a common interface.

### Branching Element Behaviour

> **Status: Design Phase**
>
> The following describes design considerations for future implementation.

To conditionally execute calculations within an element, `branch` elements may be created. This would allow elements to dynamically recast themselves to a child element based on certain parameters.

**Use cases:**
- Slender vs. stocky concrete columns with different calculation approaches
- CHS vs. UB shear behaviour in steel beams
- Different connection types with varying capacity calculations

**Possible syntax using `match`:**

```sunset
define Beam:
    inputs:
        Section = Section()
    outputs:
        match Section:
            is CHSSection:
                // Calculations for CHS beam
                ShearCapacity = CHSShearCalculation
            is UBSection:
                // Calculations for UB beam
                ShearCapacity = UBShearCalculation
        end
end
```

This is equivalent to multiple `if` statements but with compiler validation that all public calculations are defined in each branch.

**Design considerations:**
- Relationship between `branch` elements and inherited elements
- Whether to allow dynamic element recasting (e.g., `this = SlenderColumn if Slenderness > 20`)
- Type checking requirements across branches
- Potential use of single inheritance with multiple interfaces (similar to C#)

## Anonymous Elements

> **Status: Not Yet Implemented**
>
> Anonymous elements are planned for grouping variables with dynamically generated inputs.

Anonymous elements would allow creation of nested element-like structures using the `.` operator:

```sunset
result.subvalue = calculation
```

This feature is under development.
