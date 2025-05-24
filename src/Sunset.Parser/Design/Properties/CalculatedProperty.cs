using Sunset.Parser.Expressions;
using Sunset.Parser.Quantities;

namespace Sunset.Parser.Design;

/// <summary>
///     Calculates a property based on other properties with an ElementPropertiesBase set of properties.
/// </summary>
/// <typeparam name="T">The type of ElementPropertiesBase used for this calculated property.</typeparam>
public class CalculatedProperty<T> : PropertyBase where T : ElementPropertiesBase<T>
{
    /// <summary>
    ///     The calculation function for this property. Must be set in the constructor.
    ///     This function is called automatically to calculate the property if the property has not yet been calculated.
    /// </summary>
    private readonly Func<T, IExpression> _calculationFunction;

    private readonly T _properties;

    private IQuantity? _propertyValue;

    /// <summary>
    ///     Constructs a new calculated property with a calculation method.
    /// </summary>
    /// <param name="properties">The set of properties used to calculate this property.</param>
    /// <param name="variable">The variable used to initiate the property, containing a default value, units and metadata.</param>
    /// <param name="calculationFunction">
    ///     The calculation function for this property. See also
    ///     <seealso cref="_calculationFunction" />
    /// </param>
    public CalculatedProperty(T properties, Func<T, IExpression> calculationFunction)
    {
        _properties = properties;
        _calculationFunction = calculationFunction;
    }

    /// <summary>
    ///     The quantity representing this property. If the quantity has not been calculated, it is calculated prior to
    ///     returning and cached.
    /// </summary>
    public override IQuantity Quantity => _propertyValue ??= Calculate();

    public IQuantity Calculate()
    {
        throw new NotImplementedException();
        //return _calculationFunction(_properties);
    }

    public new CalculatedProperty<T> AssignName(string name)
    {
        base.AssignName(name);
        return this;
    }

    public new CalculatedProperty<T> AssignSymbol(string symbol)
    {
        base.AssignSymbol(symbol);
        return this;
    }

    public new CalculatedProperty<T> AssignDescription(string description)
    {
        base.AssignDescription(description);
        return this;
    }

    public new CalculatedProperty<T> AssignReference(string reference)
    {
        base.AssignReference(reference);
        return this;
    }

    public new CalculatedProperty<T> AssignLabel(string label)
    {
        base.AssignLabel(label);
        return this;
    }
}