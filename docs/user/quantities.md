# Quantities and calculations

Quantities are used to describe certain physical quantities, which may have both a value and a unit of measurement. For example, the quantity 150 kg has a value of 150 and a unit of kilograms. The behaviour of these quantities is described in the [`IQuantity`](../../api/Northrop.Common.Sunset.Quantities.IQuantity.yml) interface and mostly implemented in the [`Quantity`](../../api/Northrop.Common.Sunset.Quantities.Quantity.yml) class.

To create a new quantity, simply provide a value and unit of measurement to the constructor.

```csharp
var mass = new Quantity(150, Unit.Kilograms);
```

## Calculations

The `Quantity` class contains a number of mathematical functions (in the `Quantity.Operators.cs` file) and the operator overloads, such that the following arithmetic operations will automatically convert the units and calculate resulting units when standard operators are used.

```csharp
var mass = new Quantity(150, Unit.Kilograms);
```

> [!NOTE]
> Ongoing work required to document the Sunset framework.
