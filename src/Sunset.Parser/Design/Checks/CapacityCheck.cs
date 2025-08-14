using System.Text;
using Sunset.Parser.Design.Properties;
using Sunset.Parser.Reporting;
using Sunset.Parser.Units;

namespace Sunset.Parser.Design.Checks;

/// <summary>
///     Abstract base class for checks of a capacity against a demand.
/// </summary>
public class CapacityCheck<T> : ICheck where T : CheckableElementBase<T>
{
    /// <summary>
    ///     Element that the check is related to. Used to extract all demands from the element.
    /// </summary>
    public CheckableElementBase<T>? Element;

    public CapacityCheck(string name,
        PropertyBase capacity,
        Func<IDemand<T>, PropertyBase?> demandGetter,
        List<IDemand<T>> demands)
    {
        Name = name;
        Capacity = capacity;
        DemandGetter = demandGetter;
        Demands = demands;
    }

    public CapacityCheck(string name,
        PropertyBase capacity,
        Func<IDemand<T>, PropertyBase?> demandGetter,
        CheckableElementBase<T> element)
    {
        Name = name;
        Capacity = capacity;
        DemandGetter = demandGetter;
        Demands = element.Demands;
        Element = element;
    }

    /// <summary>
    ///     Capacity of the element that is being checked.
    /// </summary>
    public PropertyBase Capacity { get; }

    /// <summary>
    ///     Function that gets the particular demand quantity from the IDemand
    /// </summary>
    public Func<IDemand<T>, PropertyBase?> DemandGetter { get; }

    /// <summary>
    ///     Demands that are to be checked against the capacity.
    /// </summary>
    public List<IDemand<T>> Demands { get; }

    /// <summary>
    ///     Results for each demand that was checked.
    /// </summary>
    public Dictionary<IDemand<T>, CapacityCheckResult<T>> Results { get; } = [];

    public string Name { get; }

    /// <inheritdoc />
    public bool? Pass { get; } = null;

    /// <summary>
    ///     Checks all the demands in the Demands property.
    /// </summary>
    /// <returns>True if the capacity is greater than the demand for all the demands in the list.</returns>
    public bool Check()
    {
        var pass = true;

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

    /// <inheritdoc />
    public void AddToReport(ReportSection report)
    {
        report.AddItem(this);
    }

    /// <summary>
    ///     Gets all demands from the element provided in the constructor and adds them to be checked.
    /// </summary>
    private void GetAllDemandsFromElement()
    {
        if (Element == null) return;

        Demands.Clear();

        foreach (var demand in Element.Demands) Demands.Add(demand);
    }

    /// <summary>
    ///     Gets all demands of the specified type from the element provided in the constructor and adds them to be checked.
    /// </summary>
    /// <typeparam name="TDemand"></typeparam>
    private void GetAllDemandsFromElement<TDemand>() where TDemand : IDemand<T>
    {
        if (Element == null) return;

        Demands.Clear();

        foreach (var demand in Element.Demands.OfType<TDemand>()) Demands.Add(demand);
    }

    public string ReportSymbols()
    {
        if (Results.Count == 0) return "";

        // Show the symbol of the capacity and the symbol of the demand.
        return Capacity.Symbol + " &> " + (DemandGetter(Demands.First())?.Symbol ?? "Error getting demand symbol");
    }

    public string ReportValues()
    {
        if (Results.Count == 0) return "";

        var builder = new StringBuilder();

        builder.Append(Capacity.Quantity.ToLatexString());

        foreach (var result in Results)
        {
            var demand = DemandGetter(result.Key);

            if (demand == null) continue;
            // Show the value of the capacity and the value of the demand
            if (result.Value.Pass)
                builder.Append(" &> " + demand.Quantity.ToLatexString() +
                               @" \quad\text{ Pass} \\");
            else
                builder.Append(" &< " + demand.Quantity.ToLatexString() +
                               @" \quad\text{ Fail} \");
        }

        return builder.ToString();
    }

    /// <summary>
    ///     Checks a single provided demand.
    /// </summary>
    /// <param name="demand"></param>
    /// <returns></returns>
    public CapacityCheckResult<T> CheckSingle(IDemand<T> demand)
    {
        var result = new CapacityCheckResult<T>(demand);

        var demandQuantity = DemandGetter(demand)?.Quantity;
        if (demandQuantity == null) result.AddMessage("Demand not provided.");

        // By default, return false if the check didn't work.
        if (demandQuantity == null) return result;

        if (!Unit.EqualDimensions(Capacity.Unit, demandQuantity.Unit))
            result.AddMessage(
                $"Capacity and demand must have the same units. Capacity is in units {Capacity.Unit} and demand is in {demandQuantity.Unit}");


        var ratio = (demandQuantity / Capacity.Quantity).Value;

        result.Ratio = ratio;

        Results.Remove(demand);
        Results.TryAdd(demand, result);

        return result;
    }
}