using System.Text;
using Sunset.Parser.Quantities;
using Sunset.Parser.Reporting;

namespace Sunset.Parser.Design;

/// <summary>
///     Checks a property of an element to see if it is within a specified range.
/// </summary>
public class RangeCheck : ICheck
{
    private bool? _pass;

    /// <summary>
    ///     Constructs a new RangeCheck for a given property and between two (optional) values.
    /// </summary>
    /// <param name="property">The property to be checked.</param>
    /// <param name="min">
    ///     Minimum value that the property must be greater than or equal to. If null, there is no bottom range
    ///     to the check.
    /// </param>
    /// <param name="max">
    ///     Maximum value that the property must be less than or equal to. If null, there is no top range to the
    ///     check.
    /// </param>
    public RangeCheck(PropertyBase property, Quantity? min, Quantity? max)
    {
        Name = property.Name;

        Property = property;
        Min = min;
        Max = max;
    }

    /// <summary>
    ///     Constructs a new RangeCheck for a given property and between two (optional) values.
    /// </summary>
    /// <param name="name">Name to be used for the check.</param>
    /// <param name="property">The property to be checked against.</param>
    /// <param name="min">
    ///     Minimum value that the property must be greater than or equal to. If null, there is no bottom range
    ///     to the check.
    /// </param>
    /// <param name="max">
    ///     Maximum value that the property must be less than or equal to. If null, there is no top range to the
    ///     check.
    /// </param>
    public RangeCheck(string name, PropertyBase property, Quantity? min, Quantity? max)
    {
        Name = name;

        Property = property;
        Min = min;
        Max = max;
    }

    /// <summary>
    ///     Property of an element that is being checked.
    /// </summary>
    public PropertyBase Property { get; }

    /// <summary>
    ///     Minimum value that the property must be greater than or equal to. If null, there is no bottom range to the check.
    /// </summary>
    public Quantity? Min { get; }

    /// <summary>
    ///     Maximum value that the property must be less than or equal to. If null, there is no top range to the check.
    /// </summary>
    public Quantity? Max { get; }

    public ReportSection? DefaultReport { get; set; }
    public string Name { get; }

    /// <inheritdoc />
    public bool? Pass => _pass;

    /// <summary>
    ///     Checks whether the property is within the specified range. If either the Min or Max properties are null, they
    ///     are not considered in the range.
    /// </summary>
    /// <returns>Returns true if the property is within the range and false if it is not.</returns>
    public bool Check()
    {
        if (Min != null)
            if (Property.Quantity < Min)
            {
                _pass = false;
                return _pass.Value;
            }

        if (Max != null)
            if (Property.Quantity > Max)
            {
                _pass = false;
                return _pass.Value;
            }

        _pass = true;
        return _pass.Value;
    }

    public string Report()
    {
        var builder = new StringBuilder();

        builder.AppendLine($"**{Name}**");
        builder.AppendLine("$$ \n \\begin{align*}");
        builder.Append(ReportValues());
        builder.AppendLine($"\\quad\\text{{{ReportMessage()}}}");
        builder.AppendLine("\\end{align*} \n $$");

        return builder.ToString();
    }

    public void AddToReport(ReportSection report)
    {
        report.AddItem(this);
    }


    public string ReportValues()
    {
        var result = "";

        // If the check fails, show the offending failure
        // Property < Min
        if (Min != null)
            if (Property.Quantity < Min)
                return Property.Quantity.ToLatexString() + " &< " + Min.ToLatexString();

        // Property > Max
        if (Max != null)
            if (Property.Quantity > Max)
                return Property.Quantity.ToLatexString() + " &> " + Max.ToLatexString();

        // If the check passes, just show that the values work
        // Min < Property < Max
        if (Min != null && Max != null)
        {
            result = Min.ToLatexString() + " <= ";
            result += "&" + Property.Quantity.ToLatexString();
            result += " <= " + Max.ToLatexString();

            return result;
        }

        // Property > Min
        if (Min != null)
        {
            result = $"{Property.Quantity.ToLatexString()} &> {Min.ToLatexString()}";
            return result;
        }

        // Property < Max
        if (Max != null)
        {
            result = $"{Property.Quantity.ToLatexString()} &< {Max.ToLatexString()}";
            return result;
        }

        return result;
    }

    public string ReportMessage()
    {
        if (Pass == null) return "Fail";
        return Pass.Value ? "Pass" : "Fail";
    }

    public void AddToReport()
    {
        DefaultReport?.AddItem(this);
    }
}