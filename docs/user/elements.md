# Elements

Elements are reusable groups of expressions that encapsulate inputs and outputs. They are similar to functions or classes in other programming languages.

## Defining Elements

An element definition consists of:
- The `define` keyword
- A name
- An `inputs:` section with input variables and their default values
- An `outputs:` section with calculated expressions
- The `end` keyword

### Exported Outputs

When an output variable has the same name as the element, it becomes an "exported output". This means that when the element is instantiated and assigned to a variable, it evaluates directly to that output value:

```sunset
define Power:
    inputs:
        Exponent = 2
        Value = 4
    outputs:
        Power = Value ^ Exponent
end

// Evaluates directly to 25 (5^2)
x = Power(Value = 5, Exponent = 2)
```

This is useful for elements that compute a single primary result while still allowing access to intermediate calculations if needed.

### Basic Element Definition

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

## Using Elements as Functions

Elements can be used as inline functions by returning a default value when instantiated without accessing a specific property.

### Default Return Value

When an element is instantiated in an expression without accessing a property, it returns its **default value**. The default value is determined by:

1. **Implicit return**: The last variable defined in the element (in `outputs` or `inputs`) is the default
2. **Explicit return**: Use the `return` keyword to mark which variable is the default return value

#### Implicit Return

```sunset
define Multiply:
    inputs:
        Value1 = 12
        Value2 = 5
    outputs:
        Result = Value1 * Value2

Example = Multiply(12, 5)  // Returns 60 (Result is the last defined variable)
```

#### Explicit Return with `return` Keyword

Use `return` to explicitly mark which variable should be returned by default:

```sunset
define Operation:
    inputs:
        Value1 = 12
        Value2 = 5
    outputs:
        return Add = Value1 + Value2
        Multiply = Value1 * Value2

Example = Operation(12, 5)  // Returns 17 (Add is marked with return)
```

**Rules:**
- `return` can be used on variables in either `inputs` or `outputs`
- `return` can only be used **once** per element definition—using it multiple times is an error
- If an element with no variables is instantiated without property access, it is an error

### Partial Application (Element Re-instantiation)

Elements are immutable, but can be re-instantiated from an existing instance. This creates a new, completely independent copy with the same input properties, but allows specific properties to be changed.

```sunset
define Rectangle:
    inputs:
        Length = 1 {m}
        Width = 2 {m}
    outputs:
        Area = Length * Width
end

// Create an initial instance
RectangleInstance1 : Rectangle = Rectangle(Length = 2, Width = 4)  // Area = 8

// Re-instantiate from existing instance, changing only Length
RectangleInstance2 : Rectangle = RectangleInstance1(Length = 4)    // Area = 16 (Width = 4 inherited)

// Type annotation is optional when type can be inferred
RectangleInstance3 = RectangleInstance2(Width = 10)                // Area = 40
```

**Behaviour:**
- Re-instantiating from an existing instance creates a **completely independent copy** (enforces immutability)
- Only input properties can be overridden; outputs are re-evaluated based on the new inputs
- Re-instantiations can be chained
- Type annotation is optional when the expression is a simple single instantiation (type is inferred from the source instance)

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

