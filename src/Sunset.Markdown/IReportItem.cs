using Sunset.Parser.Parsing.Declarations;
using Sunset.Quantities.Quantities;

namespace Sunset.Markdown;

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