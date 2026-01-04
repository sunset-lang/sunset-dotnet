# Prototypes

Prototypes define contracts that elements can implement, similar to interfaces in TypeScript or traits in Rust. They enable polymorphism by allowing different elements to be treated uniformly based on shared behaviour.

## Why Use Prototypes?

Prototypes are useful when you have multiple elements that share common characteristics:

- **Polymorphic collections**: Store different element types in the same list
- **Contract enforcement**: Ensure elements provide required outputs
- **Code organisation**: Group related elements by their capabilities
- **Input sharing**: Define common inputs with defaults across multiple elements

## Defining Prototypes

A prototype declaration uses the `prototype` keyword and specifies required inputs and outputs:

```sunset
prototype Shape:
    outputs:
        return Area {m^2}
end
```

### Prototype Structure

| Component | Description | Required |
|-----------|-------------|----------|
| `prototype` | Keyword to start declaration | Yes |
| Name | The prototype's identifier | Yes |
| `inputs:` | Section for input specifications | No |
| `outputs:` | Section for required outputs | No |
| `end` | Ends the declaration | Yes |

### Prototype Inputs

Prototype inputs can have default values. These defaults are inherited by implementing elements:

```sunset
prototype Rectangular:
    inputs:
        Width = 1 {m}
        Length = 2 {m}
    outputs:
        return Area {m^2}
end
```

### Prototype Outputs

Prototype outputs specify required properties but **cannot have expressions**—the implementing element must provide the calculation:

```sunset
prototype Shape:
    outputs:
        return Area {m^2}      // Required output, no expression
        Perimeter {m}          // Another required output
end
```

### Default Return Value

Use the `return` keyword to mark which output is the default return value:

```sunset
prototype Shape:
    outputs:
        return Area {m^2}      // This is the default return
        Perimeter {m}
end
```

When an element implementing this prototype is used in an expression without property access, it returns the `Area` value.

## Implementing Prototypes

Elements implement prototypes using the `as` keyword after the element name:

```sunset
define Square as Shape:
    inputs:
        Width = 1 {m}
    outputs:
        return Area {m^2} = Width ^ 2
end

define Rectangle as Shape:
    inputs:
        Width = 1 {m}
        Length = 2 {m}
    outputs:
        return Area {m^2} = Width * Length
end
```

### Implementation Requirements

When an element implements a prototype:

1. **All prototype outputs must be defined** in the element
2. **Output types must match** the prototype specification
3. **Return must match**: If the prototype marks an output with `return`, the element must also mark it with `return`

### Multiple Prototype Implementation

Elements can implement multiple prototypes by separating them with commas:

```sunset
prototype Shape:
    outputs:
        return Area {m^2}
end

prototype Rectangular:
    outputs:
        Perimeter {m}
end

define Square as Shape, Rectangular:
    inputs:
        Width = 1 {m}
    outputs:
        return Area {m^2} = Width ^ 2
        Perimeter {m} = 4 * Width
end
```

## Prototype Inheritance

Prototypes can extend other prototypes using the `as` keyword:

```sunset
prototype Shape:
    outputs:
        return Area {m^2}
end

prototype Polygon as Shape:
    outputs:
        Sides
end
```

### Inheritance Rules

- Child prototypes **inherit** all inputs and outputs from parent prototypes
- Child prototypes can **add** new inputs and outputs
- Child prototypes **cannot override** parent outputs (this is an error)

```sunset
prototype Shape:
    outputs:
        return Area {m^2}
end

// Valid: Polygon adds Sides output
prototype Polygon as Shape:
    outputs:
        Sides
end

// Error: Cannot override Area from Shape
prototype InvalidPolygon as Shape:
    outputs:
        Area {m^2}         // Error: Area already defined in Shape
end
```

### Multi-Level Inheritance

Prototypes can form inheritance chains:

```sunset
prototype Shape:
    outputs:
        return Area {m^2}
end

prototype Polygon as Shape:
    outputs:
        Sides
end

prototype RegularPolygon as Polygon:
    inputs:
        SideLength = 1 {m}
end
```

An element implementing `RegularPolygon` must provide `Area` (from Shape), `Sides` (from Polygon), and inherits `SideLength` input.

## Input Inheritance

When an element implements a prototype with default inputs, those inputs are automatically available in the element:

```sunset
prototype Rectangular:
    inputs:
        Width = 1 {m}
        Length = 2 {m}
    outputs:
        return Area {m^2}
end

define Rectangle as Rectangular:
    // Width and Length are inherited from Rectangular with their defaults
    outputs:
        return Area {m^2} = Width * Length
end

r = Rectangle()  // Uses inherited defaults: Width=1, Length=2, Area=2
```

### Overriding Inherited Inputs

Elements can override inherited inputs by redefining them:

```sunset
define Square as Rectangular:
    inputs:
        Width = 1 {m}       // Override with custom default
        Length = Width      // Override to always equal Width
    outputs:
        return Area {m^2} = Width * Length
end

s = Square(Width = 3)  // Width=3, Length=3, Area=9
```

## Type Annotations

Type annotations use curly braces to specify expected types for variables:

| Syntax | Description |
|--------|-------------|
| `{m}` | Quantity with unit (metres) |
| `{Shape}` | Instance of element implementing Shape prototype |
| `{Shape list}` | List of instances implementing Shape prototype |

### Single Instance Annotation

```sunset
myShape {Shape} = Square(2)           // Must implement Shape
myRect {Rectangular} = Rectangle()    // Must implement Rectangular
```

### List Annotation

```sunset
shapes {Shape list} = [Square(2), Rectangle(2, 3)]
```

## Polymorphic Lists

One of the most powerful uses of prototypes is collecting different element types in a single list:

```sunset
prototype Shape:
    outputs:
        return Area {m^2}
end

define Square as Shape:
    inputs:
        Width = 1 {m}
    outputs:
        return Area {m^2} = Width ^ 2
end

define Circle as Shape:
    inputs:
        Radius = 1 {m}
    outputs:
        return Area {m^2} = 3.14159 * Radius ^ 2
end

define Triangle as Shape:
    inputs:
        Base = 1 {m}
        Height = 1 {m}
    outputs:
        return Area {m^2} = 0.5 * Base * Height
end

// All shapes in one list
AllShapes {Shape list} = [
    Square(2),
    Circle(1),
    Triangle(3, 4)
]
```

## Iterating Over Prototype Lists

When iterating over a list of prototype instances, Sunset provides special keywords:

| Keyword | Description |
|---------|-------------|
| `value` | The **default return value** of the current element |
| `value.instance` | The **full element instance** for property access |
| `index` | The current index (0-based) |

### Using `value` for Default Return

When the prototype has a `return` output, `value` gives you the return value directly:

```sunset
Shapes {Shape list} = [Square(2), Rectangle(2, 3)]

// Sum of all areas (uses default return)
TotalArea {m^2} = Shapes.sum(value)  // 4 + 6 = 10 m^2

// Filter shapes with area > 5
LargeShapes = Shapes.where(value > 5 {m^2})  // [Rectangle(2, 3)]
```

### Using `value.instance` for Property Access

When you need to access specific properties of the element instance:

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

Rects {Shape list} = [Rectangle(2, 3), Rectangle(4, 5)]

// Access Width property of each rectangle
Widths = Rects.select(value.instance.Width)  // [2, 4]

// Sum areas explicitly
TotalArea {m^2} = Rects.sum(value.instance.Area)  // 6 + 20 = 26 m^2
```

### Why Two Access Patterns?

The distinction between `value` and `value.instance` exists because:

1. **`value`** provides quick access to the most important result (the default return)
2. **`value.instance`** provides full access when you need other properties

This makes common operations concise while still allowing complete flexibility.

## Pattern Matching on Prototypes

When working with polymorphic elements, you often need to perform different calculations based on the concrete element type. Pattern matching with the `is` keyword enables type-safe access to element-specific properties.

### The Problem: Property Access Restrictions

When a variable is typed with a prototype (e.g., `{Shape}`), you can only access properties defined in that prototype:

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

myShape {Shape} = Rectangle(2 {m}, 3 {m})

// This works - Area is defined in Shape
area = myShape.Area  // 6 m^2

// This is a compile error - Width is not defined in Shape
// width = myShape.Width
```

### The Solution: Pattern Matching with `is`

Use pattern matching to check the concrete type and bind the element to a variable with that type:

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
        Diameter = 2 {m}
    outputs:
        return Area {m^2} = 3.14159 * (Diameter / 2) ^ 2
end

define AreaCalculator:
    inputs:
        ShapeToCalculate {Shape} = Rectangle()
    outputs:
        // Use pattern matching to access type-specific properties
        return Area {m^2} = rect.Width * rect.Length if ShapeToCalculate is Rectangle rect
                         = 3.14159 * (circ.Diameter / 2) ^ 2 if ShapeToCalculate is Circle circ
                         = error otherwise
end

RectangleInstance {Rectangle} = Rectangle(2 {m}, 3 {m})
CircleInstance {Circle} = Circle(4 {m})

RectangleArea {m^2} = AreaCalculator(RectangleInstance)  // 6 m^2
CircleArea {m^2} = AreaCalculator(CircleInstance)        // ~12.57 m^2
```

### Pattern Matching Syntax

| Syntax | Description |
|--------|-------------|
| `expr is Type` | Check if expression matches type |
| `expr is Type binding` | Check type and bind to variable |

The binding variable:
- Is only in scope within its branch body
- Has the matched type, enabling access to type-specific properties
- Can use any valid identifier name (convention: use descriptive names like `rect`, `circ`)

### When to Use Pattern Matching

Pattern matching is useful when:

1. **Polymorphic calculations**: Different element types require different formulas
2. **Type-specific property access**: You need properties not in the shared prototype
3. **Runtime type dispatch**: The concrete type is only known at runtime

## Empty Prototypes (Markers)

Prototypes with no inputs or outputs are valid and serve as markers or tags:

```sunset
prototype Printable:
end

prototype Exportable:
end

define Report as Printable, Exportable:
    inputs:
        Title = "Untitled"
    outputs:
        Content = "Report: " + Title
end
```

Marker prototypes are useful for:
- Categorising elements
- Enabling type-based filtering
- Future extensibility

## Complete Example

Here's a comprehensive example showing prototypes in action:

```sunset
/// # Shape Calculations
/// Calculate total area of various shapes.

// Define the Shape prototype
prototype Shape:
    outputs:
        return Area {m^2}
end

// Define specific shapes
define Square as Shape:
    inputs:
        Width = 1 {m}
    outputs:
        return Area {m^2} = Width ^ 2
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

// Create instances
Shapes {Shape list} = [
    Square(Width = 2 {m}),
    Rectangle(Width = 3 {m}, Length = 4 {m}),
    Circle(Radius = 1.5 {m})
]

// Calculate totals
TotalArea <A_total> {m^2} = Shapes.sum(value)
    d: Total area of all shapes

AverageArea <A_avg> {m^2} = Shapes.average(value)
    d: Average area per shape

LargestArea <A_max> {m^2} = Shapes.max(value)
    d: Area of the largest shape
```

## Error Cases

### Missing Required Output

```sunset
prototype Shape:
    outputs:
        return Area {m^2}
        Perimeter {m}
end

// Error: Missing Perimeter output
define InvalidSquare as Shape:
    inputs:
        Width = 1 {m}
    outputs:
        return Area {m^2} = Width ^ 2
        // Perimeter is missing!
end
```

### Output Type Mismatch

```sunset
prototype Shape:
    outputs:
        return Area {m^2}
end

// Error: Area has wrong units
define InvalidSquare as Shape:
    inputs:
        Width = 1 {m}
    outputs:
        return Area {m} = Width  // Should be {m^2}
end
```

### Return Mismatch

```sunset
prototype Shape:
    outputs:
        return Area {m^2}
end

// Error: Wrong output marked as return
define InvalidSquare as Shape:
    inputs:
        Width = 1 {m}
    outputs:
        Area {m^2} = Width ^ 2      // Not marked as return
        return Other = 0             // Wrong return
end
```

### Prototype Output Override

```sunset
prototype Shape:
    outputs:
        return Area {m^2}
end

// Error: Cannot redefine Area in child prototype
prototype InvalidPolygon as Shape:
    outputs:
        Area {m^2}  // Error: Already defined in Shape
        Sides
end
```
