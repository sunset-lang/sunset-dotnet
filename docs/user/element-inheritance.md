# Element Inheritance

> **Note:** Element inheritance is partially implemented. Basic syntax is supported but some advanced features may not be available.

Elements can inherit from other elements, copying the behaviour (inputs and outputs) of that element and allowing it to be extended.

For example, a reinforcement bar can be considered as having inherited from a circle.

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

The element inheriting from another must explicitly inherit all properties of the parent element, either:
- Using the `parent` keyword to inherit unchanged, or
- Overriding by re-defining the property

All inputs and outputs must be included in the child element for readability. Any properties not explicitly inherited will throw an error. This encourages full consideration of inherited properties.

## Inheritance Rules

1. Use `define ChildElement(ParentElement):` syntax to declare inheritance
2. All parent inputs must be listed in the child's `inputs:` section
3. All parent outputs must be listed in the child's `outputs:` section
4. Use `parent` keyword to inherit a property unchanged
5. Provide a new expression to override a property

## Example: Overriding Properties

```sunset
define Rectangle:
    inputs:
        Width = 100 {mm}
        Height = 50 {mm}
    outputs:
        Area = Width * Height
        Perimeter = 2 * (Width + Height)
end

define Square(Rectangle):
    inputs:
        Width = parent
        Height = Width  // Override to force height = width
    outputs:
        Area = parent
        Perimeter = parent
end
```

## Multiple Inheritance

> **Status: Not Supported**
>
> Multiple inheritance was considered but has been abandoned to simplify the language. Interface-like behaviour may be considered for future implementation if required.

## Element Groups

> **Status: Planned**
>
> Inherited elements will be able to be grouped into categories to allow input definitions to accept a particular group of element types.

```sunset
// Future syntax (not yet implemented)
group Shape = [Circle, Square, Rectangle]

define Container:
    inputs:
        Content: Shape = Circle()  // Accepts any Shape
    outputs:
        ContentArea = Content.Area
end
```
