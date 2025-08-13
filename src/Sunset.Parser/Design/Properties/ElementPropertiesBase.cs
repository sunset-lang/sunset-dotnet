using System.ComponentModel;
using System.Runtime.CompilerServices;
using Sunset.Parser.Reporting;

namespace Sunset.Parser.Design.Properties;

public abstract class ElementPropertiesBase<T> where T : ElementPropertiesBase<T>
{
    public List<CalculatedProperty<T>> CalculatedProperties { get; } = [];

    public List<InputProperty> InputProperties { get; } = [];

    /// <summary>
    ///     Adds a new calculated property to the set of properties. Uses the provided calculation function to automatically
    ///     calculate the property.
    ///     Does not calculate the property immediately, but waits for the input properties to change.
    ///     Calculation can be triggered by calling the Calculate method on the property on the CalculateAllProperties method
    ///     on the set of properties.
    /// </summary>
    /// <param name="calculatedProperty">Calculated property to be added to the Element</param>
    public void AddCalculatedProperty(CalculatedProperty<T> calculatedProperty)
    {
        CalculatedProperties.Add(calculatedProperty);
    }

    /// <summary>
    ///     Adds a new input property to the set of properties. Registers the property to an event handler that fires whenever
    ///     the value of the property changes. This event handler will trigger the recalculation of all calculated properties.
    /// </summary>
    /// <param name="inputProperty">Property to be added to this Element</param>
    /// <returns>A reference to the InputProperty that has been registered in the property set.</returns>
    public void AddInputProperty(InputProperty inputProperty)
    {
        inputProperty.PropertyChanged += OnInputPropertyChanged;
        InputProperties.Add(inputProperty);
    }

    /// <summary>
    ///     Calculates or recalculates all the calculated properties in the set of properties.
    ///     This is called automatically whenever the input properties change.
    /// </summary>
    public void CalculateAllProperties()
    {
        foreach (var property in CalculatedProperties) property.Calculate();

        OnPropertyChanged(nameof(CalculatedProperties));
    }

    private void OnInputPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(InputProperties));
        CalculateAllProperties();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public ReportSection ReportInputProperties()
    {
        var report = new ReportSection("Inputs");

        foreach (var property in InputProperties) report.AddItem(property);

        return report;
    }

    public ReportSection ReportCalculatedProperties()
    {
        var report = new ReportSection("Calculations");

        foreach (var property in CalculatedProperties) report.AddItem(property);

        return report;
    }
}