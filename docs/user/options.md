# Options

Options define a fixed set of valid values that a variable can have. They create a sum type where only specific values are allowed, providing compile-time validation and exhaustive pattern matching capabilities.

Options are useful for representing categorical data like material grades, drawing methods, size constraints, or any scenario where inputs should be restricted to a known set of values.

## Defining Options

Options are defined using the `option` keyword, followed by the option name, an optional type annotation, and the list of allowed values.

### Quantity Options

Options with physical units specify the unit type after the option name:

```sunset
option Size {m}:
    10 {m}
    20 {m}
    30 {m}
end
```

All values must have dimensions compatible with the declared unit type.

### Text Options

Use `{text}` for string-based options:

```sunset
option DrawingMethods {text}:
    "SVG"
    "Typst"
end
```

### Numeric Options

Use `{number}` for dimensionless numeric options:

```sunset
option Scale {number}:
    1
    2
    5
    10
end
```

### Type Inference

If the type annotation is omitted, it is inferred from the first value:

```sunset
option Sizes:
    10 {m}
    20 {m}
end
// Inferred as {m} from first value
```

## Using Options

Options are used as type annotations on variables, similar to how unit annotations or element types are used:

```sunset
option Size {m}:
    10 {m}
    20 {m}
end

define Rectangle:
    inputs:
        Width {Size} = 10 {m}
        Length {Size} = 20 {m}
    outputs:
        Area = Width * Length
end
```

When a variable is annotated with an option type, only values that match one of the defined options are allowed.

## Validation

### Compile-Time Validation

Invalid option values cause compilation errors when the value is a literal or constant expression:

```sunset
option Size {m}:
    10 {m}
    20 {m}
end

// Error: 15 {m} is not a valid Size option
x {Size} = 15 {m}

// Error: Invalid option values in element instantiation
RectangleInstance = Rectangle(15 {m}, 27 {m})
```

### Runtime Validation

Computed values that cannot be verified at compile time are validated at runtime:

```sunset
x = someCalculation()
// Type-compatible, validated at runtime
y {Size} = x
```

## Options in Conditionals

Options work with both value equality (`==`) and variant checking (`is`):

```sunset
option Size {m}:
    10 {m}
    20 {m}
    30 {m}
end

x {Size} = 10 {m}

result = 100 if x == 10 {m}
       = 200 if x is 20 {m}
       = 300 otherwise
```

### Exhaustive Matching

When all option values are explicitly covered in a conditional, the `otherwise` branch can be omitted:

```sunset
option Size {m}:
    10 {m}
    20 {m}
    30 {m}
end

x {Size} = 10 {m}

area = x * 2 if x is 10 {m}
     = x * 3 if x is 20 {m}
     = x * 4 if x is 30 {m}
// No otherwise needed - all Size options are covered
```

If a new option value is added later, the compiler will produce an error on any conditionals that don't handle the new value, ensuring all cases are always covered.

## Practical Examples

### Drawing Methods

```sunset
option DrawingMethods {text}:
    "SVG"
    "Typst"
end

prototype DiagramElement:
    inputs:
        Method {DrawingMethods} = "SVG"
        Scale {m} = 1 {m}
end

define Point as DiagramElement:
    inputs:
        x = 0 {m}
        y = 0 {m}
    outputs:
        return Draw 
            = "<circle cx=\"{x}\" cy=\"{y}\" />" if Method is "SVG"
            = "#circle(({x}, {y}))" if Method is "Typst"
end
```

### Material Grades

```sunset
option BoltGrade {text}:
    "4.6/S"
    "8.8/S"
    "8.8/TB"
    "8.8/TF"
end

define BoltedConnection:
    inputs:
        Grade {BoltGrade} = "8.8/S"
        Diameter {mm} = 20 {mm}
    outputs:
        Strength {MPa} = 240 {MPa} if Grade is "4.6/S"
                       = 640 {MPa} if Grade is "8.8/S"
                       = 640 {MPa} if Grade is "8.8/TB"
                       = 640 {MPa} if Grade is "8.8/TF"
end
```

## Benefits

1. **Type Safety**: Only valid option values can be assigned to option-typed variables
2. **Compile-Time Validation**: Invalid literal values are caught before runtime
3. **Exhaustive Matching**: Compiler ensures all option cases are handled in conditionals
4. **Self-Documenting**: Option definitions clearly specify all valid values
5. **UI Generation**: Options can be rendered as dropdowns in generated interfaces
6. **Refactoring Safety**: Adding new option values forces handling in all conditional expressions
