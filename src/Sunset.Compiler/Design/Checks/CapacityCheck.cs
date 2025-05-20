using System.Text;
using Sunset.Compiler.Quantities;
using Sunset.Compiler.Reporting;
using Sunset.Compiler.Units;

namespace Sunset.Compiler.Design;

/// <summary>
/// Abstract base class for checks of a capacity against a demand.
/// </summary>
public class CapacityCheck<T> : ICheck where T : CheckableElementBase<T>
{
    /// <summary>
    /// Abstract base class for checks of a capacity against a demand.
    /// </summary>
    public CapacityCheck(string name,
        PropertyBase capacity,
        Func<IDemand<T>, IQuantity?> demandGetter,
        List<IDemand<T>> demands)
    {
        Name = name;
        Capacity = capacity;
        DemandGetter = demandGetter;
        Demands = demands;
    }

    public CapacityCheck(string name,
        PropertyBase capacity,
        Func<IDemand<T>, IQuantity?> demandGetter,
        CheckableElementBase<T> element)
    {
        Name = name;
        Capacity = capacity;
        DemandGetter = demandGetter;
        Demands = element.Demands;
        Element = element;
    }

    /// <summary>
    /// Element that the check is related to. Used to extract all demands from the element.
    /// </summary>
    public CheckableElementBase<T>? Element;

    // TODO: Consider this being just an implementation of ICheck such that the same capacity can be calculated once and shared across
    // multiple different checks.
    public string Name { get; }

    /// <summary>
    /// Capacity of the element that is being checked.
    /// </summary>
    public PropertyBase Capacity { get; }

    /// <summary>
    /// Function that gets the particular demand quantity from the IDemand
    /// </summary>
    public Func<IDemand<T>, IQuantity?> DemandGetter { get; }

    /// <inheritdoc />
    public bool? Pass { get; } = null;

    /// <summary>
    /// Demands that are to be checked against the capacity.
    /// </summary>
    public List<IDemand<T>> Demands { get; }

    /// <summary>
    /// Results for each demand that was checked.
    /// </summary>
    public Dictionary<IDemand<T>, CapacityCheckResult<T>> Results { get; } = [];

    /// <summary>
    /// Gets all demands from the element provided in the constructor and adds them to be checked.
    /// </summary>
    private void GetAllDemandsFromElement()
    {
        if (Element == null) return;

        Demands.Clear();

        foreach (var demand in Element.Demands)
        {
            Demands.Add(demand);
        }
    }

    /// <summary>
    /// Gets all demands of the specified type from the element provided in the constructor and adds them to be checked.
    /// </summary>
    /// <typeparam name="TDemand"></typeparam>
    private void GetAllDemandsFromElement<TDemand>() where TDemand : IDemand<T>
    {
        if (Element == null) return;

        Demands.Clear();

        foreach (var demand in Element.Demands.OfType<TDemand>())
        {
            Demands.Add(demand);
        }
    }

    /// <summary>
    /// Checks all the demands in the Demands property.
    /// </summary>
    /// <returns>True if the capacity is greater than the demand for all the demands in the list.</returns>
    public bool Check()
    {
        bool pass = true;

        foreach (var demand in Demands)
        {
            var result = CheckSingle(demand);

            pass &= result.Pass;
        }

        return pass;
    }

    public string Report()
    {
        var builder = new StringBuilder();

        builder.AppendLine($"**{Name}**");
        builder.AppendLine("$$ \n \\begin{align*}");
        builder.AppendLine(ReportSymbols() + @"\\");
        builder.AppendLine(ReportValues());
        builder.AppendLine("\\end{align*} \n $$");

        return builder.ToString();
    }

    public string ReportSymbols()
    {
        if (Results.Count == 0) return "";

        // Show the symbol of the capacity and the symbol of the demand.
        return Capacity.Symbol + " &> " + (DemandGetter(Demands.First())?.Symbol ?? "Error getting demand symbol");
    }

    public string ReportValues()
    {
        if (Results.Count == 0)
        {
            return "";
        }

        var builder = new StringBuilder();

        builder.Append(Capacity.ValueToLatexString());

        foreach (var result in Results)
        {
            var demand = DemandGetter(result.Key);

            if (demand == null) continue;
            // Show the value of the capacity and the value of the demand
            if (result.Value.Pass)
            {
                builder.Append(" &> " + demand.ValueToLatexString() +
                               @" \quad\text{ Pass} \\");
            }
            else
            {
                builder.Append(" &< " + demand.ValueToLatexString() +
                               @" \quad\text{ Fail} \");
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Checks a single provided demand.
    /// </summary>
    /// <param name="demand"></param>
    /// <returns></returns>
    public CapacityCheckResult<T> CheckSingle(IDemand<T> demand)
    {
        var result = new CapacityCheckResult<T>(demand);
        var message = new StringBuilder();

        var demandQuantity = DemandGetter(demand);
        if (demandQuantity == null)
        {
            result.AddMessage("Demand not provided.");
        }

        // By default, return false if the check didn't work.
        if (demandQuantity == null) return result;

        if (!Unit.EqualDimensions(Capacity, demandQuantity))
        {
            result.AddMessage(
                $"Capacity and demand must have the same units. Capacity is in units {Capacity.Unit} and demand is in {demandQuantity.Unit}");
        }


        var ratio = (demandQuantity / Capacity.ToQuantity()).Value;

        result.Ratio = ratio;
        
        Results.Remove(demand);
        Results.TryAdd(demand, result);

        return result;
    }

    /// <inheritdoc />
    public ReportSection? DefaultReport { get; set; }

    // TODO: Add Report Builder pattern functions

    /// <inheritdoc />
    public void AddToReport(ReportSection report)
    {
        report.AddItem(this);
    }

    /// <inheritdoc />
    public void AddToReport()
    {
        DefaultReport?.AddItem(this);
    }
}