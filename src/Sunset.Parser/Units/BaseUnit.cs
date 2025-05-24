namespace Sunset.Parser.Units;

public class BaseUnit : NamedUnit
{
    public BaseUnit(DimensionName dimensionName, UnitName unitName, string prefixSymbol, string baseUnitSymbol)
        : base(unitName, prefixSymbol, baseUnitSymbol)
    {
        PrimaryDimension = dimensionName;
        UnitDimensions[(int)dimensionName].Power = 1;

        Symbol = prefixSymbol + baseUnitSymbol;
    }

    public DimensionName PrimaryDimension { get; set; }
}