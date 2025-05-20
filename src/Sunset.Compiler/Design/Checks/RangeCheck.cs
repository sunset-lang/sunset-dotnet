using System.Text;
using Sunset.Compiler.Quantities;
using Sunset.Compiler.Reporting;

namespace Sunset.Compiler.Design;

/// <summary>
/// Checks a property of an element to see if it is within a specified range.
/// </summary>
public class RangeCheck : ICheck
{
    public string Name { get; }

    private bool? _pass = null;

    /// <inheritdoc />
    public bool? Pass => _pass;

    /// <summary>
    /// Property of an element that is being checked.
    /// </summary>
    public PropertyBase Property { get; }

    /// <summary>
    /// Minimum value that the property must be greater than or equal to. If null, there is no bottom range to the check.
    /// </summary>
    public IQuantity? Min { get; }

    /// <summary>
    /// Maximum value that the property must be less than or equal to. If null, there is no top range to the check.
    /// </summary>
    public IQuantity? Max { get; }

    /// <summary>
    /// Constructs a new RangeCheck for a given property and between two (optional) values.
    /// </summary>
    /// <param name="property">The property to be checked.</param>
    /// <param name="min">Minimum value that the property must be greater than or equal to. If null, there is no bottom range to the check.</param>
    /// <param name="max">Maximum value that the property must be less than or equal to. If null, there is no top range to the check.</param>
    public RangeCheck(PropertyBase property, IQuantity? min, IQuantity? max)
    {
        Name = property.Name;

        Property = property;
        Min = min;
        Max = max;
    }

    /// <summary>
    /// Constructs a new RangeCheck for a given property and between two (optional) values.
    /// </summary>
    /// <param name="name">Name to be used for the check.</param>
    /// <param name="property">The property to be checked against.</param>
    /// <param name="min">Minimum value that the property must be greater than or equal to. If null, there is no bottom range to the check.</param>
    /// <param name="max">Maximum value that the property must be less than or equal to. If null, there is no top range to the check.</param>
    public RangeCheck(string name, PropertyBase property, IQuantity? min, IQuantity? max)
    {
        Name = name;

        Property = property;
        Min = min;
        Max = max;
    }

    /// <summary>
    /// Checks whether the property is within the specified range. If either the Min or Max properties are null, they
    /// are not considered in the range.
    /// </summary>
    /// <returns>Returns true if the property is within the range and false if it is not.</returns>
    public bool Check()
    {
        if (Min != null)
        {
            if (Property.PropertyValue < Min.ToQuantity())
            {
                _pass = false;
                return _pass.Value;
            }
        }

        if (Max != null)
        {
            if (Property.PropertyValue > Max.ToQuantity())
            {
                _pass = false;
                return _pass.Value;
            }
        }

        _pass = true;
        return _pass.Value;
    }

    public string Report()
    {
        var builder = new StringBuilder();

        builder.AppendLine($"**{Name}**");
        builder.AppendLine("$$ \n \\begin{align*}");
        builder.AppendLine($@"{ReportSymbols()}\\");
        builder.Append(ReportValues());
        builder.AppendLine($"\\quad\\text{{{ReportMessage()}}}");
        builder.AppendLine("\\end{align*} \n $$");

        return builder.ToString();
    }

    public string ReportSymbols()
    {
        var result = "";

        if (Min != null)
        {
            result += (Min.Symbol ?? Min.ValueToLatexString()) + " <= ";
        }

        result += "&" + Property.Symbol;

        if (Max != null)
        {
            result += " <= " + (Max.Symbol ?? Max.ValueToLatexString());
        }

        return result;
    }

    public string ReportValues()
    {
        var result = "";

        // If the check fails, show the offending failure
        // Property < Min
        if (Min != null)
        {
            if (Property.PropertyValue < Min.ToQuantity())
            {
                return Property.ValueToLatexString() + " &< " + Min.ValueToLatexString();
            }
        }

        // Property > Max
        if (Max != null)
        {
            if (Property.PropertyValue > Max.ToQuantity())
            {
                return Property.ValueToLatexString() + " &> " + Max.ValueToLatexString();
            }
        }

        // If the check passes, just show that the values work
        // Min < Property < Max
        if (Min != null && Max != null)
        {
            result = Min.ValueToLatexString() + " <= ";
            result += "&" + Property.ValueToLatexString();
            result += " <= " + Max.ValueToLatexString();

            return result;
        }

        // Property > Min
        if (Min != null)
        {
            result = $"{Property.ValueToLatexString()} &> {Min.ValueToLatexString()}";
            return result;
        }

        // Property < Max
        if (Max != null)
        {
            result = $"{Property.ValueToLatexString()} &< {Max.ValueToLatexString()}";
            return result;
        }

        return result;
    }

    public string ReportMessage()
    {
        if (Pass == null) return "Fail";
        return Pass.Value ? "Pass" : "Fail";
    }

    public ReportSection? DefaultReport { get; set; }

    public void AddToReport(ReportSection report)
    {
        report.AddItem(this);
    }

    public void AddToReport()
    {
        DefaultReport?.AddItem(this);
    }
}