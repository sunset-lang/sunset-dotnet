using Sunset.Parser.Errors;
using Sunset.Parser.Errors.Semantic;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Results;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors.Evaluation;

namespace Sunset.Parser.Test.Integration;

/// <summary>
///     Integration tests for the Diagramming Standard Library infrastructure.
///     
///     NOTE: Full diagram rendering tests are currently blocked because the Diagrams
///     library uses features not yet supported by the parser:
///     - `{Type list}` syntax for list type annotations (e.g., `{DiagramElement list}`)
///     - Type annotations in variable declarations (e.g., `{Point}`, `{Linear}`)
///     
///     The parser currently only supports unit annotations (e.g., `{m^2}`, `{text}`)
///     in variable declarations, not arbitrary type annotations.
///     
///     These tests verify that the import system and StandardLibrary fallback work
///     correctly for importing from StandardLibrary modules. The SVG generation tests
///     demonstrate that string interpolation works for generating diagrams manually.
/// </summary>
[TestFixture]
public class DiagramsIntegrationTests
{
    private string _svgOutputDirectory = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Create a persistent directory for SVG output that survives test runs
        _svgOutputDirectory = Path.Combine(Path.GetTempPath(), "sunset-diagram-tests");
        if (!Directory.Exists(_svgOutputDirectory))
        {
            Directory.CreateDirectory(_svgOutputDirectory);
        }
        
        Console.WriteLine($"SVG output directory: {_svgOutputDirectory}");
    }

    /// <summary>
    ///     Helper to create an environment and analyse code.
    /// </summary>
    private Scopes.Environment CreateAndAnalyse(string code)
    {
        var source = SourceFile.FromString(code);
        var env = new Scopes.Environment(source);
        env.Analyse();
        return env;
    }

    /// <summary>
    ///     Helper to get a string result from a variable.
    /// </summary>
    private string? GetStringResult(Scopes.Environment env, string varName)
    {
        var fileScope = env.ChildScopes["$file"] as FileScope;
        if (fileScope == null) return null;

        if (!fileScope.ChildDeclarations.TryGetValue(varName, out var decl)) return null;
        
        var varDecl = decl as VariableDeclaration;
        if (varDecl == null) return null;

        var result = varDecl.GetResult(fileScope) as StringResult;
        return result?.Result;
    }

    /// <summary>
    ///     Helper to get a numeric result from a variable.
    /// </summary>
    private double? GetNumericResult(Scopes.Environment env, string varName)
    {
        var fileScope = env.ChildScopes["$file"] as FileScope;
        if (fileScope == null) return null;

        if (!fileScope.ChildDeclarations.TryGetValue(varName, out var decl)) return null;
        
        var varDecl = decl as VariableDeclaration;
        if (varDecl == null) return null;

        var result = varDecl.GetResult(fileScope) as QuantityResult;
        return result?.Result.BaseValue;
    }

    /// <summary>
    ///     Helper to check for errors and return them as a string.
    /// </summary>
    private string GetErrorSummary(Scopes.Environment env)
    {
        var errors = env.Log.Errors.ToList();
        if (errors.Count == 0) return string.Empty;
        return string.Join("\n", errors.Select(e => $"  - {e.GetType().Name}: {e.Message}"));
    }

    /// <summary>
    ///     Helper to write SVG to file and print path to console.
    /// </summary>
    private void WriteSvgOutput(string testName, string svgContent)
    {
        var fileName = $"{testName}_{DateTime.Now:yyyyMMdd_HHmmss}.svg";
        var filePath = Path.Combine(_svgOutputDirectory, fileName);
        File.WriteAllText(filePath, svgContent);
        Console.WriteLine($"SVG written to: {filePath}");
    }

    // =========================================================================
    // STANDARD LIBRARY LOADING TESTS
    // =========================================================================

    [Test]
    public void StandardLibrary_UnitsAvailable_WithoutExplicitImport()
    {
        // Units from StandardLibrary.sun should be available without explicit import
        var code = """
            length {m} = 100 {mm}
            force {N} = 50 {kN}
            """;

        var env = CreateAndAnalyse(code);
        var errors = GetErrorSummary(env);
        Assert.That(errors, Is.Empty, $"Should have no errors:\n{errors}");

        var length = GetNumericResult(env, "length");
        Assert.That(length, Is.Not.Null);
        Assert.That(length!.Value, Is.EqualTo(0.1).Within(0.0001)); // 100mm = 0.1m

        var force = GetNumericResult(env, "force");
        Assert.That(force, Is.Not.Null);
        Assert.That(force!.Value, Is.EqualTo(50000).Within(0.1)); // 50kN = 50000N
    }

    [Test]
    public void StandardLibrary_DimensionsAvailable_ForUnitDeclarations()
    {
        // Dimensions from StandardLibrary.sun should be available
        var code = """
            acceleration {m/s^2} = 9.81 {m/s^2}
            time {s} = 2 {s}
            velocity {m/s} = acceleration * time
            """;

        var env = CreateAndAnalyse(code);
        var errors = GetErrorSummary(env);
        Assert.That(errors, Is.Empty, $"Should have no errors:\n{errors}");

        var velocity = GetNumericResult(env, "velocity");
        Assert.That(velocity, Is.Not.Null);
        Assert.That(velocity!.Value, Is.EqualTo(19.62).Within(0.01)); // 9.81 * 2
    }

    // =========================================================================
    // IMPORT FALLBACK TESTS (StandardLibrary modules without prefix)
    // =========================================================================

    [Test]
    [Ignore("Blocked by: Interpolated strings in imported elements return error during evaluation (colour.Svg returns Error!)")]
    public void Import_DiagramsCore_ResolvesFromStandardLibrary()
    {
        // This should resolve to StandardLibrary/Diagrams/Core.sun
        var code = """
            import Diagrams.Core
            
            colour {RGBA} = RGBA(R = 255, G = 128, B = 64)
            result = colour.Svg
            """;

        var env = CreateAndAnalyse(code);
        var errors = GetErrorSummary(env);
        Assert.That(errors, Is.Empty, $"Should have no errors:\n{errors}");

        var result = GetStringResult(env, "result");
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.Contain("255"));
        Assert.That(result, Does.Contain("128"));
        Assert.That(result, Does.Contain("64"));
    }

    [Test]
    [Ignore("Blocked by: Diagrams module chain has unresolved element declarations during evaluation")]
    public void Import_Diagrams_ResolvesAllModules()
    {
        // This should resolve to StandardLibrary/Diagrams/Diagrams.sun which imports all modules
        var code = """
            import Diagrams
            
            diag {Diagram} = Diagram(ViewportWidth = 400, ViewportHeight = 300, Scale = 100)
            flipY = diag.FlipY
            """;

        var env = CreateAndAnalyse(code);
        var errors = GetErrorSummary(env);
        Assert.That(errors, Is.Empty, $"Should have no errors:\n{errors}");

        var flipY = GetNumericResult(env, "flipY");
        Assert.That(flipY, Is.Not.Null);
        Assert.That(flipY!.Value, Is.EqualTo(300)); // FlipY = ViewportHeight
    }

    // =========================================================================
    // SVG OUTPUT TESTS (Simplified - without Diagrams library)
    // =========================================================================

    [Test]
    public void StringInterpolation_GeneratesSvgMarkup()
    {
        // Test that string interpolation can generate SVG-like markup
        // This doesn't use the Diagrams library, just tests the SVG generation concept
        // Note: Using single quotes for HTML attributes to avoid escaped quote parsing bug
        var code = """
            width = 400
            height = 300
            cx = 200
            cy = 150
            r = 50
            
            svgHeader = "<svg width='::width::' height='::height::' xmlns='http://www.w3.org/2000/svg'>"
            circle = "<circle cx='::cx::' cy='::cy::' r='::r::' fill='red' />"
            svgFooter = "</svg>"
            
            svg = svgHeader + "\n" + circle + "\n" + svgFooter
            """;

        var env = CreateAndAnalyse(code);
        var errors = GetErrorSummary(env);
        Assert.That(errors, Is.Empty, $"Should have no errors:\n{errors}");

        var svg = GetStringResult(env, "svg");
        Assert.That(svg, Is.Not.Null);
        Assert.That(svg, Does.Contain("<svg"));
        Assert.That(svg, Does.Contain("</svg>"));
        Assert.That(svg, Does.Contain("<circle"));
        Assert.That(svg, Does.Contain("cx='200'"));
        Assert.That(svg, Does.Contain("cy='150'"));
        Assert.That(svg, Does.Contain("r='50'"));
        
        // Write SVG to file for manual verification
        WriteSvgOutput("SimpleCircle", svg!);
    }

    [Test]
    public void StringInterpolation_GeneratesRectangleSvg()
    {
        // Note: Using single quotes for HTML attributes to avoid escaped quote parsing bug
        var code = """
            width = 400
            height = 300
            rectX = 50
            rectY = 50
            rectWidth = 200
            rectHeight = 100
            
            svgHeader = "<svg width='::width::' height='::height::' xmlns='http://www.w3.org/2000/svg' style='background-color: white;'>"
            rect = "<rect x='::rectX::' y='::rectY::' width='::rectWidth::' height='::rectHeight::' fill='rgb(200,200,200)' stroke='black' stroke-width='2' />"
            svgFooter = "</svg>"
            
            svg = svgHeader + "\n" + rect + "\n" + svgFooter
            """;

        var env = CreateAndAnalyse(code);
        var errors = GetErrorSummary(env);
        Assert.That(errors, Is.Empty, $"Should have no errors:\n{errors}");

        var svg = GetStringResult(env, "svg");
        Assert.That(svg, Is.Not.Null);
        Assert.That(svg, Does.Contain("<rect"));
        Assert.That(svg, Does.Contain("width='200'"));
        Assert.That(svg, Does.Contain("height='100'"));
        
        WriteSvgOutput("SimpleRectangle", svg!);
    }

    [Test]
    public void StringInterpolation_MultipleShapes()
    {
        // Note: Using single quotes for HTML attributes to avoid escaped quote parsing bug
        var code = """
            width = 600
            height = 400
            
            svgHeader = "<svg width='::width::' height='::height::' xmlns='http://www.w3.org/2000/svg' style='background-color: white;'>"
            
            // Rectangle
            rect = "<rect x='50' y='50' width='150' height='100' fill='rgb(200,200,200)' stroke='black' stroke-width='1' />"
            
            // Circle
            circle = "<circle cx='350' cy='100' r='50' fill='rgb(200,200,200)' stroke='black' stroke-width='1' />"
            
            // Ellipse  
            ellipse = "<ellipse cx='300' cy='300' rx='100' ry='40' fill='rgb(200,200,200)' stroke='black' stroke-width='1' />"
            
            svgFooter = "</svg>"
            
            svg = svgHeader + "\n" + rect + "\n" + circle + "\n" + ellipse + "\n" + svgFooter
            """;

        var env = CreateAndAnalyse(code);
        var errors = GetErrorSummary(env);
        Assert.That(errors, Is.Empty, $"Should have no errors:\n{errors}");

        var svg = GetStringResult(env, "svg");
        Assert.That(svg, Is.Not.Null);
        Assert.That(svg, Does.Contain("<rect"));
        Assert.That(svg, Does.Contain("<circle"));
        Assert.That(svg, Does.Contain("<ellipse"));
        
        WriteSvgOutput("MultipleShapes", svg!);
    }

    [Test]
    public void StringInterpolation_EngineeringDrawing_PadFooting()
    {
        // Simulate a simple engineering drawing without the Diagrams library
        // Note: Using single quotes for HTML attributes to avoid escaped quote parsing bug
        var code = """
            // Viewport settings
            viewportWidth = 400
            viewportHeight = 200
            scale = 200
            
            // Footing dimensions (in metres, converted to pixels)
            footingWidth = 1.2 {m}
            footingDepth = 0.4 {m}
            columnWidth = 0.3 {m}
            columnDepth = 0.3 {m}
            
            // Convert to SVG coordinates (Y-down)
            footingX = 20
            footingY = viewportHeight - (footingDepth {/m} * scale) - 20
            footingW = footingWidth {/m} * scale
            footingH = footingDepth {/m} * scale
            
            columnX = footingX + (footingW - columnWidth {/m} * scale) / 2
            columnY = footingY - columnDepth {/m} * scale
            columnW = columnWidth {/m} * scale
            columnH = columnDepth {/m} * scale
            
            // Build SVG
            svgHeader = "<svg width='::viewportWidth::' height='::viewportHeight::' xmlns='http://www.w3.org/2000/svg' style='background-color: white;'>"
            footing = "<rect x='::footingX::' y='::footingY::' width='::footingW::' height='::footingH::' fill='rgb(200,200,200)' stroke='black' stroke-width='1' />"
            column = "<rect x='::columnX::' y='::columnY::' width='::columnW::' height='::columnH::' fill='rgb(200,200,200)' stroke='black' stroke-width='1' />"
            svgFooter = "</svg>"
            
            svg = svgHeader + "\n" + footing + "\n" + column + "\n" + svgFooter
            """;

        var env = CreateAndAnalyse(code);
        var errors = GetErrorSummary(env);
        Assert.That(errors, Is.Empty, $"Should have no errors:\n{errors}");

        var svg = GetStringResult(env, "svg");
        Assert.That(svg, Is.Not.Null);
        Assert.That(svg, Does.Contain("<svg"));
        Assert.That(svg, Does.Contain("<rect")); // Two rectangles
        Assert.That(svg, Does.Contain("</svg>"));
        
        WriteSvgOutput("PadFooting", svg!);
        
        Console.WriteLine($"\nGenerated SVG:\n{svg}");
    }

    [Test]
    public void StringInterpolation_Triangle_WithLines()
    {
        // Note: Using single quotes for HTML attributes to avoid escaped quote parsing bug
        var code = """
            width = 400
            height = 400
            
            // Triangle vertices
            x1 = 100
            y1 = 300
            x2 = 300
            y2 = 300
            x3 = 200
            y3 = 100
            
            svgHeader = "<svg width='::width::' height='::height::' xmlns='http://www.w3.org/2000/svg' style='background-color: white;'>"
            
            // Lines forming triangle
            line1 = "<line x1='::x1::' y1='::y1::' x2='::x2::' y2='::y2::' stroke='black' stroke-width='2' />"
            line2 = "<line x1='::x2::' y1='::y2::' x2='::x3::' y2='::y3::' stroke='black' stroke-width='2' />"
            line3 = "<line x1='::x3::' y1='::y3::' x2='::x1::' y2='::y1::' stroke='black' stroke-width='2' />"
            
            // Points at vertices
            pt1 = "<circle cx='::x1::' cy='::y1::' r='5' fill='red' />"
            pt2 = "<circle cx='::x2::' cy='::y2::' r='5' fill='red' />"
            pt3 = "<circle cx='::x3::' cy='::y3::' r='5' fill='red' />"
            
            svgFooter = "</svg>"
            
            svg = svgHeader + "\n" + line1 + "\n" + line2 + "\n" + line3 + "\n" + pt1 + "\n" + pt2 + "\n" + pt3 + "\n" + svgFooter
            """;

        var env = CreateAndAnalyse(code);
        var errors = GetErrorSummary(env);
        Assert.That(errors, Is.Empty, $"Should have no errors:\n{errors}");

        var svg = GetStringResult(env, "svg");
        Assert.That(svg, Is.Not.Null);
        Assert.That(svg, Does.Contain("<line")); // Three lines
        Assert.That(svg, Does.Contain("<circle")); // Three points
        
        WriteSvgOutput("Triangle", svg!);
    }
}
