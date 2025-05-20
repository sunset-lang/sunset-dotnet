using System.ComponentModel;
using System.Runtime.CompilerServices;
using Sunset.Compiler.Quantities;
using Sunset.Compiler.Units;

namespace Sunset.Compiler.Design;

public sealed class InputProperty : PropertyBase, INotifyPropertyChanged
{
    private Quantity _propertyValue;

    public InputProperty(Quantity value)
    {
        _propertyValue = value;
    }

    public InputProperty(string name, Quantity value, List<NamedUnit> validUnits)
    {
        value.Name = name;
        _propertyValue = value;
        ValidUnits = validUnits;
    }

    public List<NamedUnit> ValidUnits { get; }

    /// <summary>
    /// Quantity representing the value of the property.
    /// </summary>
    public override Quantity PropertyValue => _propertyValue;

    /// <summary>
    /// Sets the value of this property to a new Quantity value and unit. Can only be set to a quantity with the same dimensions as the current quantity.
    /// Does not change any of the other properties of quantity such as the symbol or name.
    /// </summary>
    /// <param name="quantity">Quantity to replace the previous property value.</param>
    /// <exception cref="ArgumentException">Thrown if the unit dimensions do not match.</exception>
    public void Set(Quantity quantity)
    {
        if (!Unit.EqualDimensions(quantity, _propertyValue)) throw new ArgumentException("Dimensions do not match");

        _propertyValue = quantity;
        OnPropertyChanged(nameof(PropertyValue));
    }

    /// <summary>
    /// Sets the value of this property to a new value. The unit of the value is not changed on this property.
    /// Does not perform any unit conversions.
    /// </summary>
    /// <param name="value">New value of the property.</param>
    public void Set(double value)
    {
        if (Math.Abs(value - _propertyValue.Value) < 1e-12) return;

        _propertyValue.Set(value);
        OnPropertyChanged(nameof(Value));
    }

    /// <summary>
    /// Sets the units of this property to a new unit. Can only be set to a unit with the same dimensions as the current unit.
    /// Performs an automatic unit conversion for the value of the property.
    /// </summary>
    /// <param name="unit">New unit of the property.</param>
    public void Set(Unit unit)
    {
        if (unit == _propertyValue.Unit) return;

        _propertyValue.Set(unit);
        OnPropertyChanged(nameof(PropertyValue));
    }

    /// <summary>
    /// Sets the value and unit of this property to a new value and unit. The unit can only be set to a unit with the same dimensions as the current unit.
    /// Does not perform any unit conversions.
    /// </summary>
    /// <param name="value">New value of the property.</param>
    /// <param name="unit">New unit of the property.</param>
    public void Set(double value, Unit unit)
    {
        if (Math.Abs(value - _propertyValue.Value) < 1e-12 && unit == _propertyValue.Unit) return;

        _propertyValue.Set(value, unit);
        OnPropertyChanged(nameof(PropertyValue));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}