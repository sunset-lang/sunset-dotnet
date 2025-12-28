# Options

> **Status: Not Yet Implemented**
>
> The Options type is planned for future implementation but is not currently functional. This document describes the intended functionality.

Options are special types that define a fixed set of valid choices. They are useful for representing categorical data like material grades, connection types, or load cases.

## Defining Options

Options are defined with a name, a set of keys, and their descriptions:

```sunset
BoltTypes = Options(
    4.6/S: "Grade 4.6, snug tight",
    8.8/S: "Grade 8.8, snug tight",
    8.8/TB: "Grade 8.8, tensioned bearing",
    8.8/TF: "Grade 8.8, tensioned friction"
)
```

## Using Options

Once defined, options can be used as input types in elements:

```sunset
define BoltedConnection:
    inputs:
        BoltGrade = BoltTypes.8.8/S  // Default to Grade 8.8 snug tight
        BoltDiameter = 20 {mm}
    outputs:
        Capacity = calculateCapacity(BoltGrade, BoltDiameter)
end
```

## Options in Conditionals

Options work seamlessly with conditional statements. When all option values are covered, the `else` branch can be omitted:

```sunset
boltStrength =
  if BoltGrade:
    is 4.6/S:
      240 {MPa}
    is 8.8/S:
      640 {MPa}
    is 8.8/TB:
      640 {MPa}
    is 8.8/TF:
      640 {MPa}
  end
```

This exhaustive matching ensures that adding a new option value will cause a compile-time error if not handled, preventing runtime issues.

## Benefits of Options

1. **Type Safety**: Only valid option values can be used
2. **Documentation**: Descriptions provide context for each choice
3. **Exhaustive Matching**: Compiler ensures all cases are handled in conditionals
4. **UI Generation**: Options can be rendered as dropdowns in generated interfaces
