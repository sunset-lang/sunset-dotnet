using Sunset.Compiler.Quantities;

namespace Sunset.Compiler.Design;

/// <summary>
/// Calculates a property based on other properties with an ElementPropertiesBase set of properties.
/// </summary>
/// <typeparam name="T">The type of ElementPropertiesBase used for this calculated property.</typeparam>
public class CalculatedProperty<T> : PropertyBase where T : ElementPropertiesBase<T>
{
    private readonly T _properties;

    private Quantity? _propertyValue;

    /// <summary>
    /// The quantity representing this property. If the quantity has not been calculated, it is calculated prior to returning and cached.
    /// </summary>
    public override Quantity PropertyValue
    {
        get
        {
            if (_propertyValue == null)
            {
                _propertyValue = CalculationFunction(_properties);
            }

            return _propertyValue;
        }
    }

    /// <summary>
    /// Constructs a new calculated property with a calculation method.
    /// </summary>
    /// <param name="properties">The set of properties used to calculate this property.</param>
    /// <param name="calculationFunction">The calculation function for this property. See also <seealso cref="CalculationFunction"/></param>
    public CalculatedProperty(T properties, Func<T, Quantity> calculationFunction)
    {
        _properties = properties;
        CalculationFunction = calculationFunction;
    }


    /// <summary>
    /// The calculation function for this property. Must be set in the constructor.
    /// This function is called automatically to calculate the property if the property has not yet been calculated.
    /// </summary>
    public Func<T, Quantity> CalculationFunction { get; }

    public Quantity Calculate()
    {
        var recalculatedProperty = CalculationFunction(_properties);
        PropertyValue.Set(recalculatedProperty.Value, recalculatedProperty.Unit);
        return PropertyValue;
    }
}