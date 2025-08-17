using Sunset.Parser.Abstractions;
using Sunset.Parser.Quantities;

namespace Sunset.Parser.Reporting;

public interface IReportItem
{
}

public class QuantityReport(IQuantity quantity) : IReportItem
{
    public readonly IQuantity Quantity = quantity;
}

public class VariableReportItem(IVariable variable) : IReportItem
{
    public readonly IVariable Variable = variable;
}