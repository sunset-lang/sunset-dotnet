namespace Sunset.Parser.Reporting;

public class PrinterSettings
{
    public static PrinterSettings Default { get; } = new();

    // Quantity reporting
    public bool CondenseAtAssignedSymbols { get; set; } = true;
    public bool ShowSymbolsInCalculations { get; set; } = false;
    public bool ShowValuesInCalculations { get; set; } = true;
    public bool ShowQuantityDescriptionsAfterCalculations { get; set; } = false;

    // Units
    public bool AutoSimplifyUnits { get; set; } = true;
    public bool ScientificUnitsOnly { get; set; } = false;

    // Numbers
    public RoundingOption RoundingOption { get; set; } = RoundingOption.Auto;
    public int SignificantFigures { get; set; } = 4;
    public int DecimalPlaces { get; set; } = 3;

    // Heading options
    public HeadingNumberingOption HeadingNumberingOption { get; set; } = HeadingNumberingOption.Numeric;
    public bool PrintTableOfContents { get; set; } = false;
}

public enum RoundingOption
{
    None,
    Auto,
    Engineering,
    SignificantFigures,
    FixedDecimal,
    Scientific
}

public enum HeadingNumberingOption
{
    None,
    Numeric,
    Alphanumeric
}