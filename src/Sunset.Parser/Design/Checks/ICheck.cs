using Sunset.Parser.Reporting;

namespace Sunset.Parser.Design;

/// <summary>
///     Interface for a design check that is performed on an IElement, which compares an ICapacity value to one or more
///     IDemand values.
/// </summary>
/// <typeparam name="T">The element type that this check applies to.</typeparam>
/// Different check types: PropertyCheck, DemandCheck, CheckGroup
/// PropertyCheckBase - checks whether a property of an element is within a certain range.
public interface ICheck : IReportItem
{
    public string Name { get; }

    public bool? Pass { get; }

    public bool Check();

    public string Report();
}