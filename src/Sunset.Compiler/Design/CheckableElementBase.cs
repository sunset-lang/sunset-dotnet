using Sunset.Compiler.Quantities;
using Sunset.Compiler.Reporting;

namespace Sunset.Compiler.Design;

/// <summary>
/// Interface for an element that contains a number of capacities, demands and checks.
/// Uses the curiously recurring template pattern to enforce the type of the element on the demands.
/// </summary>
public abstract class CheckableElementBase<T> : IElement
    where T : CheckableElementBase<T>
{
    /// <summary>
    /// The Checks that are to be performed on the element.
    /// </summary>
    public List<ICheck> Checks { get; } = [];

    /// <summary>
    /// The demands that are to be used to check the element.
    /// </summary>
    public List<IDemand<T>> Demands { get; } = [];

    public void AddDemand(IDemand<T> demand)
    {
        Demands.Add(demand);
    }

    protected void AddCheck(ICheck check)
    {
        Checks.Add(check);
    }

    protected void AddRangeCheck(string name, PropertyBase property, IQuantity? min, IQuantity? max)
    {
        var check = new RangeCheck(property, min, max);
        AddCheck(check);
    }

    protected void AddCapacityCheck<TDemand>(string name, PropertyBase capacity, Func<TDemand, IQuantity?> demandGetter)
        where TDemand : IDemand<T>
    {
        Func<IDemand<T>, IQuantity?> demandGetterWrapper = demand => demandGetter((TDemand)demand);

        var check = new CapacityCheck<T>(name, capacity, demandGetterWrapper, this);
        AddCheck(check);
    }

    /// <summary>
    /// Performs all the checks that are registered for the element.
    /// </summary>
    /// <returns>Returns true if all the checks pass, returns false if any of the checks fail.</returns>
    public bool CheckAll()
    {
        var pass = true;

        foreach (ICheck check in Checks)
        {
            pass &= check.Check();
        }

        return pass;
    }

    /// <summary>
    /// Function to initialise all the checks for the element by using the AddCheck functions.
    /// </summary>
    public abstract void InitializeChecks();

    /// <summary>
    /// Updates the CheckableElement by performing all the checks.
    /// </summary>
    public void Update()
    {
        CheckAll();
    }

    public abstract ReportSection Report();

    public ReportSection ReportChecks()
    {
        var report = new ReportSection("Checks");

        foreach (var check in Checks)
        {
            report.AddItem(check);
        }

        return report;
    }
}