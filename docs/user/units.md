# Units

This page describes how units work in the Sunset framework. These units are based on the [International System of Units](https://en.wikipedia.org/wiki/International_System_of_Units) as closely as they can be followed, with some additional dimensions added for convenience.

## Terminology

### Dimensions

Dimensions describe the different physical dimensions that a unit can possess. These include the standard SI dimensions:

- Time
- Length
- Mass
- Electric current
- Thermodynamic temperature
- Amount of a substance
- Luminous intensity

It also includes a number of non-standard dimensions for convenience, such as:

- Angle, included to allow for angles to be expressed with a symbol despite being dimensionless.

The dimensions are enumerated in the [`DimensionName`](../../api/Northrop.Common.Sunset.Units.DimensionName.yml) enumeration. The dimension information specific to a particular unit is stored in an array of [`Dimension`](../../api/Northrop.Common.Sunset.Units.Dimension.yml) objects, with length equal to the number of dimensions in `DimensionName`.

### Unit types

There are four different types of units within the framework. These are:

- [`Unit`](../../api/Northrop.Common.Sunset.Units.Unit.yml)
- [`NamedUnit`](../../api/Northrop.Common.Sunset.Units.NamedUnit.yml) which inherits `Unit`
- [`NamedUnitMultiple`](../../api/Northrop.Common.Sunset.Units.NamedUnitMultiple.yml) which inherits `NamedUnit`
- [`BaseUnit`](../../api/Northrop.Common.Sunset.Units.NamedUnit.yml) which inherits `NamedUnit`

`Unit` describes any general purpose unit that does not necessarily have a strict physical meaning. For example, this could be a unit such as $kg m^2 s^{-1} K$, which has no immediate physical meaning but must be representable as a unit. This class implements most of the behaviour of the units, and is divided into a number of different partial classes.

`NamedUnit` describes a unit that has a specific **single** name, and is used to describe what are common units. This includes Newtons. The names for all named units including multiples and base units is in the [`UnitName`](../../api/Northrop.Common.Sunset.Units.UnitName.yml) enumeration.

`NamedUnitMultiple` is a specific type of `NamedUnit`, where the unit has a name but is a multiple of what would be considered the _coherent_ named unit. For example, a Newton is a `NamedUnit` but a Kilonewton is a `NamedUnitMultiple`.

`BaseUnit` is a type of named unit where it describes only a single dimension. For example, a Metre is a `BaseUnit` as it describes a unit in only one Dimension (Length).

## Usage

Generally, units are created through mathematical operations on quantities that are assigned simple base or named units initially. This is described in the next section on [calculations](./quantities.md).

All of the named units enumerated in `UnitName` are described in the `Unit.BaseUnits.cs` file as static instances, such that the following will retrieve the metre base unit.

```csharp
var unit = Unit.Metre;
```
