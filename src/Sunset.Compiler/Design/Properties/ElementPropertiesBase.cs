using System.ComponentModel;
using System.Runtime.CompilerServices;
using Sunset.Compiler.Quantities;
using Sunset.Compiler.Reporting;
using Sunset.Compiler.Units;

namespace Sunset.Compiler.Design;

public abstract class ElementPropertiesBase<T> where T : ElementPropertiesBase<T>
{
    private readonly List<CalculatedProperty<T>> _calculatedProperties = [];
    private readonly List<InputProperty> _inputProperties = [];

    public List<CalculatedProperty<T>> CalculatedProperties => _calculatedProperties;
    public List<InputProperty> InputProperties => _inputProperties;

    /// <summary>
    /// Adds a new calculated property to the set of properties. Uses the provided calculation function to automatically calculate the property.
    /// Does not calculate the property immediately, but waits for the input properties to change.
    /// Calculation can be triggered by calling the Calculate method on the property on the CalculateAllProperties method on the set of properties.
    /// </summary>
    /// <param name="calculation">Method used to calculate the property based on the other properties within the set.</param>
    /// <returns>A reference to the CalculatedProperty that has been registered in the property set.</returns>
    public CalculatedProperty<T> AddCalculatedProperty(Func<T, Quantity> calculation)
    {
        var calculatedProperty = new CalculatedProperty<T>((T)this, calculation);

        _calculatedProperties.Add(calculatedProperty);

        return calculatedProperty;
    }

    /// <summary>
    /// Adds a new input property to the set of properties. Registers the property to an event handler that fires whenever
    /// the value of the property changes. This event handler will trigger the recalculation of all calculated properties.
    /// </summary>
    /// <param name="name">Name of the property. Used to set the Quantity name.</param>
    /// <param name="value">The quantity used to set up the input property.</param>
    /// <returns>A reference to the InputProperty that has been registered in the property set.</returns>
    public InputProperty AddInputProperty(string name, IQuantity value, List<NamedUnit> validUnits)
    {
        var inputProperty = new InputProperty(name, value.ToQuantity(), validUnits);

        inputProperty.PropertyChanged += OnInputPropertyChanged;
        _inputProperties.Add(inputProperty);

        return inputProperty;
    }

    /// <summary>
    /// Calculates or recalculates all the calculated properties in the set of properties.
    /// This is called automatically whenever the input properties change.
    /// </summary>
    public void CalculateAllProperties()
    {
        foreach (var property in _calculatedProperties)
        {
            property.Calculate();
        }

        OnPropertyChanged(nameof(CalculatedProperties));
    }

    private void OnInputPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(InputProperties));
        CalculateAllProperties();
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public ReportSection ReportInputProperties()
    {
        var report = new ReportSection("Inputs");

        foreach (var property in InputProperties)
        {
            // TODO: report.AddItem(property) doesn't work the same way, although it should. Investigate this and create an issue.
            property.Report(report);
        }

        return report;
    }

    public ReportSection ReportCalculatedProperties()
    {
        var report = new ReportSection("Calculations");

        foreach (var property in CalculatedProperties)
        {
            property.Report(report);
        }

        return report;
    }
    
}