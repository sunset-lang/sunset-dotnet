using Sunset.Quantities.Quantities;

namespace Sunset.Parser.Visitors.Evaluation;

public class DefaultQuantityPassData : IPassData
{
    public IQuantity? DefaultQuantity { get; set; }
}