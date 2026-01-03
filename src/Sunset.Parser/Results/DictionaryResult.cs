using System.Diagnostics;
using Sunset.Parser.Expressions;
using Sunset.Quantities.Quantities;

namespace Sunset.Parser.Results;

/// <summary>
/// Represents a single entry in a dictionary result.
/// </summary>
public class DictionaryEntryResult(IResult key, IResult value)
{
    /// <summary>
    /// The key of this entry.
    /// </summary>
    public IResult Key { get; } = key;

    /// <summary>
    /// The value of this entry.
    /// </summary>
    public IResult Value { get; } = value;

    public override bool Equals(object? obj)
    {
        return obj is DictionaryEntryResult other &&
               Key.Equals(other.Key) &&
               Value.Equals(other.Value);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Key, Value);
    }
}

/// <summary>
/// Wrapper around a dictionary of key-value pairs that is returned from evaluating a dictionary expression.
/// </summary>
[DebuggerDisplay("Dictionary[{Entries.Count}]")]
public class DictionaryResult(List<DictionaryEntryResult> entries) : IResult
{
    /// <summary>
    /// The key-value entries contained in the dictionary.
    /// </summary>
    public List<DictionaryEntryResult> Entries { get; } = entries;

    /// <summary>
    /// Gets the number of entries in the dictionary.
    /// </summary>
    public int Count => Entries.Count;

    /// <summary>
    /// Tries to get a value by its key using direct lookup.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <returns>The value if found, null otherwise.</returns>
    public IResult? TryGetValue(IResult key)
    {
        foreach (var entry in Entries)
        {
            if (entry.Key.Equals(key))
            {
                return entry.Value;
            }
        }
        return null;
    }

    /// <summary>
    /// Performs interpolation or nearest-key lookup for numeric keys.
    /// </summary>
    /// <param name="lookupKey">The numeric key to look up.</param>
    /// <param name="mode">The interpolation mode.</param>
    /// <returns>The interpolated or found value, or null if the operation fails.</returns>
    public IResult? Interpolate(double lookupKey, CollectionAccessMode mode)
    {
        if (Entries.Count == 0)
            return null;

        // Extract numeric keys and sort them
        var numericEntries = new List<(double Key, IResult Value)>();
        foreach (var entry in Entries)
        {
            if (entry.Key is QuantityResult qr)
            {
                numericEntries.Add((qr.Result.BaseValue, entry.Value));
            }
            else
            {
                return null; // Non-numeric keys cannot be interpolated
            }
        }

        numericEntries.Sort((a, b) => a.Key.CompareTo(b.Key));

        return mode switch
        {
            CollectionAccessMode.Interpolate => LinearInterpolate(numericEntries, lookupKey),
            CollectionAccessMode.InterpolateBelow => FindBelow(numericEntries, lookupKey),
            CollectionAccessMode.InterpolateAbove => FindAbove(numericEntries, lookupKey),
            _ => null
        };
    }

    private static IResult? LinearInterpolate(List<(double Key, IResult Value)> sortedEntries, double lookupKey)
    {
        if (sortedEntries.Count == 0)
            return null;

        // Exact match check
        foreach (var entry in sortedEntries)
        {
            if (Math.Abs(entry.Key - lookupKey) < double.Epsilon)
                return entry.Value;
        }

        // Check bounds
        if (lookupKey < sortedEntries[0].Key || lookupKey > sortedEntries[^1].Key)
            return null; // Out of range

        // Find the two surrounding entries
        for (int i = 0; i < sortedEntries.Count - 1; i++)
        {
            var (k1, v1) = sortedEntries[i];
            var (k2, v2) = sortedEntries[i + 1];

            if (lookupKey >= k1 && lookupKey <= k2)
            {
                // Both values must be quantities for interpolation
                if (v1 is not QuantityResult qr1 || v2 is not QuantityResult qr2)
                    return null;

                // Linear interpolation: v = v1 + (v2 - v1) * (x - k1) / (k2 - k1)
                var t = (lookupKey - k1) / (k2 - k1);
                var interpolatedValue = qr1.Result.BaseValue + (qr2.Result.BaseValue - qr1.Result.BaseValue) * t;

                // Use the unit from the first value
                return new QuantityResult(new Quantity(interpolatedValue, qr1.Result.Unit));
            }
        }

        return null;
    }

    private static IResult? FindBelow(List<(double Key, IResult Value)> sortedEntries, double lookupKey)
    {
        if (sortedEntries.Count == 0)
            return null;

        // Find largest key <= lookupKey
        IResult? result = null;
        foreach (var entry in sortedEntries)
        {
            if (entry.Key <= lookupKey)
                result = entry.Value;
            else
                break;
        }

        return result;
    }

    private static IResult? FindAbove(List<(double Key, IResult Value)> sortedEntries, double lookupKey)
    {
        if (sortedEntries.Count == 0)
            return null;

        // Find smallest key >= lookupKey
        foreach (var entry in sortedEntries)
        {
            if (entry.Key >= lookupKey)
                return entry.Value;
        }

        return null;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not DictionaryResult other) return false;
        if (Entries.Count != other.Entries.Count) return false;

        for (int i = 0; i < Entries.Count; i++)
        {
            if (!Entries[i].Equals(other.Entries[i])) return false;
        }
        return true;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var entry in Entries)
        {
            hash.Add(entry);
        }
        return hash.ToHashCode();
    }
}
