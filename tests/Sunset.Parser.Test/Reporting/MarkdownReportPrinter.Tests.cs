using Sunset.Parser.Reporting;
using Sunset.Parser.Units;

namespace Sunset.Parser.Test.Reporting;

[TestFixture]
public class MarkdownReportPrinterTests
{
    private ReportSection AxialSectionCapacity()
    {
        var report = new ReportSection("Axial section capacity");

        report.AddText("Calculate the axial section capacity of a steel plate.");

        var yield = new Variable(350, DefinedUnits.Megapascal, "f_y", "Yield strength",
            "The yield strength of the steel plate.");
        var width = new Variable(100, DefinedUnits.Millimetre, "b", "Width", "The width of the steel plate.");
        var thickness = new Variable(10, DefinedUnits.Millimetre, "t", "Thickness",
            "The thickness of the steel plate.");
        var area = new Variable(width * thickness).AssignSymbol("A").Report(report);
        area.AssignName("Area");
        area.AssignDescription("The cross-sectional area of the steel plate.");
        var capacityFactor = new Variable(0.9, DefinedUnits.Dimensionless, "\\phi", "Capacity factor", "",
            "AS 4100-1998 Clause 2.1.5");

        var capacity = new Variable(capacityFactor * yield * area).AssignSymbol("\\phi N_s").Report(report);
        capacity.AssignName("Axial capacity");
        capacity.AssignDescription("The axial capacity of the steel plate.");
        capacity.AssignReference("AS 4100-1998 Clause 4.3.1");

        return report;
    }

    private ReportSection BendingSectionCapacity()
    {
        var report = new ReportSection("Bending section capacity");

        report.AddText("Calculate the bending section capacity of a steel plate.");

        var yield = new Variable(350, DefinedUnits.Megapascal, "f_y", "Yield strength",
            "The yield strength of the steel plate.");
        var width = new Variable(100, DefinedUnits.Millimetre, "b", "Width", "The width of the steel plate.");
        var thickness = new Variable(10, DefinedUnits.Millimetre, "t", "Thickness",
            "The thickness of the steel plate.");
        var sectionModulus = new Variable(width * thickness.Pow(2) / 4).AssignSymbol("Z_p").Report(report);
        sectionModulus.AssignDescription("The plastic section modulus of the plate.");
        var capacityFactor = new Variable(0.9, DefinedUnits.Dimensionless, "\\phi", "Capacity Factor", "",
            "AS 4100-1998 Clause 2.1.5");

        var capacity = new Variable(capacityFactor * yield * sectionModulus);
        capacity.AssignSymbol("\\phi M_s").Report(report);
        capacity.AssignName("Bending capacity");
        capacity.AssignDescription("The bending capacity of the steel plate.");
        capacity.AssignReference("AS 4100-1998 Clause 5.2.4");

        return report;
    }


    [Test]
    public void PrintReport_AxialSectionCapacity_ShouldReportCorrectly()
    {
        var report = AxialSectionCapacity();
        var printer = new MarkdownReportPrinter();
        var printedReport = printer.PrintReport(report);

        Console.WriteLine(printedReport);

        var expected = """
                       # 1 Axial section capacity<a id="1"></a>
                       Calculate the axial section capacity of a steel plate.
                       $$
                       \begin{alignedat}{2}
                       A &= b t \\
                       &= 100 \text{ mm} \times 10 \text{ mm} \\
                       &= 1,000 \text{ mm}^{2} \\\\
                       \phi N_s &= \phi f_y A &\quad\text{(AS 4100-1998 Clause 4.3.1)} \\
                       &= 0.9 \times 350 \text{ MPa} \times 1,000 \text{ mm}^{2} \\
                       &= 315 \text{ kN} \\
                       \end{alignedat}
                       $$
                       """;
        Assert.That(Northrop.Common.TestHelpers.TestHelpers.NormalizeString(printedReport),
            Is.EqualTo(Northrop.Common.TestHelpers.TestHelpers.NormalizeString(expected)));
    }

    [Test]
    public void PrintReport_BendingSectionCapacity_ShouldReportCorrectly()
    {
        var report = BendingSectionCapacity();
        var printer = new MarkdownReportPrinter();
        var printedReport = printer.PrintReport(report);

        Console.WriteLine(printedReport);

        var expected = """
                       # 1 Bending section capacity<a id="1"></a>
                       Calculate the bending section capacity of a steel plate.
                       $$
                       \begin{alignedat}{2}
                       Z_p &= \frac{b t^{2}}{4} \\
                       &= \frac{100 \text{ mm} \times 10 \text{ mm}^{2}}{4} \\
                       &= 2,500 \text{ mm}^{3} \\\\
                       \phi M_s &= \phi f_y Z_p &\quad\text{(AS 4100-1998 Clause 5.2.4)} \\
                       &= 0.9 \times 350 \text{ MPa} \times 2,500 \text{ mm}^{3} \\
                       &= 787.5 \text{ N m} \\
                       \end{alignedat}
                       $$
                       """;
        Assert.That(Northrop.Common.TestHelpers.TestHelpers.NormalizeString(printedReport),
            Is.EqualTo(Northrop.Common.TestHelpers.TestHelpers.NormalizeString(expected)));
    }

    [Test]
    public void PrintReport_CombinedAxialAndBending_ShouldReportCorrectly()
    {
        var axialReport = AxialSectionCapacity();
        var bendingReport = BendingSectionCapacity();
        var combinedReport = new ReportSection("Combined section capacity");
        combinedReport.AddSubsection(axialReport);
        combinedReport.AddSubsection(bendingReport);

        var settings = new PrinterSettings()
        {
            PrintTableOfContents = true,
            ShowQuantityDescriptionsAfterCalculations = true,
        };
        var printer = new MarkdownReportPrinter(settings);
        var printedReport = printer.PrintReport(combinedReport);

        Console.WriteLine(printedReport);

        var expected = """
                       # Table of contents
                       - [1 Combined section capacity](#1)
                         - [1.1 Axial section capacity](#1.1)
                         - [1.2 Bending section capacity](#1.2)
                       # 1 Combined section capacity<a id="1"></a>
                       ## 1.1 Axial section capacity<a id="1.1"></a>
                       Calculate the axial section capacity of a steel plate.
                       $$
                       \begin{alignedat}{2}
                       A &= b t \\
                       &= 100 \text{ mm} \times 10 \text{ mm} \\
                       &= 1,000 \text{ mm}^{2} \\\\
                       \phi N_s &= \phi f_y A &\quad\text{(AS 4100-1998 Clause 4.3.1)} \\
                       &= 0.9 \times 350 \text{ MPa} \times 1,000 \text{ mm}^{2} \\
                       &= 315 \text{ kN} \\
                       \end{alignedat}
                       $$
                       Where: 
                       - $b$ The width of the steel plate.
                       - $t$ The thickness of the steel plate.
                       - $A$ The cross-sectional area of the steel plate.
                       - $\phi$ (AS 4100-1998 Clause 2.1.5)
                       - $f_y$ The yield strength of the steel plate.
                       - $\phi N_s$ The axial capacity of the steel plate. (AS 4100-1998 Clause 4.3.1)
                       ## 1.2 Bending section capacity<a id="1.2"></a>
                       Calculate the bending section capacity of a steel plate.
                       $$
                       \begin{alignedat}{2}
                       Z_p &= \frac{b t^{2}}{4} \\
                       &= \frac{100 \text{ mm} \times 10 \text{ mm}^{2}}{4} \\
                       &= 2,500 \text{ mm}^{3} \\\\
                       \phi M_s &= \phi f_y Z_p &\quad\text{(AS 4100-1998 Clause 5.2.4)} \\
                       &= 0.9 \times 350 \text{ MPa} \times 2,500 \text{ mm}^{3} \\
                       &= 787.5 \text{ N m} \\
                       \end{alignedat}
                       $$
                       Where: 
                       - $b$ The width of the steel plate.
                       - $t$ The thickness of the steel plate.
                       - $Z_p$ The plastic section modulus of the plate.
                       - $\phi$ (AS 4100-1998 Clause 2.1.5)
                       - $f_y$ The yield strength of the steel plate.
                       - $\phi M_s$ The bending capacity of the steel plate. (AS 4100-1998 Clause 5.2.4)
                       """;
        Assert.That(Northrop.Common.TestHelpers.TestHelpers.NormalizeString(printedReport),
            Is.EqualTo(Northrop.Common.TestHelpers.TestHelpers.NormalizeString(expected)));
    }

    [Test]
    public void PrintTableOfContents_CombinedAxialAndBending_ShouldReportCorrectly()
    {
        var axialReport = AxialSectionCapacity();
        var bendingReport = BendingSectionCapacity();
        var combinedReport = new ReportSection("Combined section capacity");
        combinedReport.AddSubsection(axialReport);
        combinedReport.AddSubsection(bendingReport);

        var settings = new PrinterSettings()
        {
            PrintTableOfContents = true,
        };
        var printer = new MarkdownReportPrinter(settings);
        var printedTableOfContents = printer.PrintTableOfContents(combinedReport);

        Console.WriteLine(printedTableOfContents);

        var expected = """
                       # Table of contents
                       - [1 Combined section capacity](#1)
                         - [1.1 Axial section capacity](#1.1)
                         - [1.2 Bending section capacity](#1.2)
                       """;

        Assert.That(Northrop.Common.TestHelpers.TestHelpers.NormalizeString(printedTableOfContents),
            Is.EqualTo(Northrop.Common.TestHelpers.TestHelpers.NormalizeString(expected)));
    }
}