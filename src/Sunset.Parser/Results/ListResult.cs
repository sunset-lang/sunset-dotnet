using System.Diagnostics;

namespace Sunset.Parser.Results;

/// <summary>
/// Wrapper around a list of results that is returned from evaluating a list expression.
/// </summary>
[DebuggerDisplay("List[{Elements.Count}]")]
public class ListResult(List<IResult> elements) : IResult
{
    /// <summary>
    /// The elements contained in the list.
    /// </summary>
    public List<IResult> Elements { get; } = elements;

    /// <summary>
    /// Gets the number of elements in the list.
    /// </summary>
    public int Count => Elements.Count;

    /// <summary>
    /// Gets the element at the specified index.
    /// </summary>
    public IResult this[int index] => Elements[index];

    public override bool Equals(object? obj)
    {
        if (obj is not ListResult other) return false;
        if (Elements.Count != other.Elements.Count) return false;

        for (int i = 0; i < Elements.Count; i++)
        {
            if (!Elements[i].Equals(other.Elements[i])) return false;
        }
        return true;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var element in Elements)
        {
            hash.Add(element);
        }
        return hash.ToHashCode();
    }
}
