namespace Sunset.Quantities

/// <summary>
/// Represents a dimension
/// </summary>
type Dimension = { Power: int; Factor: double }

module Say =
    let hello name = printfn "Hello %s" name
