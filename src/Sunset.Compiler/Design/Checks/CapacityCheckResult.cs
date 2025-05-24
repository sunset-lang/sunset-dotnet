using System.Text;

namespace Northrop.Common.Sunset.Design;

public struct CapacityCheckResult<T>(IDemand<T> demand) where T : IElement
{
    /// <summary>
    /// True if the element capacity is greater than the demand. False otherwise.
    /// </summary>
    public bool Pass => Ratio < 1;


    /// <summary>
    /// 0 for fail, 1 when the demand is exactly equal to the element's capacity
    /// </summary>
    public double? Ratio { get; internal set; } = null;

    /// <summary>
    /// An optional message to pass back with the result.
    /// </summary>
    public string Message => _messageBuilder.ToString();

    private readonly StringBuilder _messageBuilder = new();

    /// <summary>
    /// The demand resulting in this check result.
    /// </summary>
    public IDemand<T> Demand = demand;

    /// <summary>
    /// Add a message to the result. Appends a new line to the Message property.
    /// </summary>
    /// <param name="message">Message to add.</param>
    public void AddMessage(string message)
    {
        _messageBuilder.AppendLine(message);
    }
}