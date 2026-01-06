using Sunset.Parser.Scopes;
using Environment = Sunset.Parser.Scopes.Environment;

namespace Sunset.Parser.Test.Analysis;

[TestFixture]
public class TypeAnnotationTests
{
    private Environment CreateAndAnalyse(string code)
    {
        var sourceFile = SourceFile.FromString(code);
        var environment = new Environment(sourceFile);
        environment.Analyse();
        return environment;
    }

    #region Valid Element Type Annotations

    [Test]
    public void TypeAnnotation_ElementType_IsValid()
    {
        var code = """
            define Point:
                inputs:
                    x = 0 {m}
                    y = 0 {m}
            end
            pt {Point} = Point(x = 5 {m}, y = 10 {m})
            """;
        var env = CreateAndAnalyse(code);
        Assert.That(env.Log.Errors.Count(), Is.EqualTo(0),
            $"Unexpected errors: {string.Join(", ", env.Log.Errors.Select(e => e.Message))}");
    }

    [Test]
    public void TypeAnnotation_PrototypeType_IsValid()
    {
        var code = """
            prototype Shape:
            end
            define Circle as Shape:
                inputs:
                    radius = 1 {m}
            end
            shape {Shape} = Circle(radius = 5 {m})
            """;
        var env = CreateAndAnalyse(code);
        Assert.That(env.Log.Errors.Count(), Is.EqualTo(0));
    }

    #endregion

    #region Invalid Mixed Type Annotations

    [Test]
    public void TypeAnnotation_ElementTypeWithUnit_IsError()
    {
        var code = """
            define Point:
                inputs:
                    x = 0 {m}
            end
            pt {Point m} = Point(x = 5 {m})
            """;
        var env = CreateAndAnalyse(code);
        Assert.That(env.Log.Errors.Count(), Is.GreaterThan(0));
        // Check for appropriate error message
        var error = env.Log.Errors.First();
        Assert.That(error.Message, Does.Contain("Point").And.Contain("type").And.Contain("unit"));
    }

    [Test]
    public void TypeAnnotation_ElementTypeWithPower_IsError()
    {
        var code = """
            define Point:
                inputs:
                    x = 0 {m}
            end
            pt {Point^2} = Point(x = 5 {m})
            """;
        var env = CreateAndAnalyse(code);
        Assert.That(env.Log.Errors.Count(), Is.GreaterThan(0));
    }

    [Test]
    public void TypeAnnotation_ElementTypeMultiplied_IsError()
    {
        var code = """
            define Point:
                inputs: x = 0 {m}
            end
            define Line:
                inputs: length = 0 {m}
            end
            invalid {Point * Line} = Point()
            """;
        var env = CreateAndAnalyse(code);
        Assert.That(env.Log.Errors.Count(), Is.GreaterThan(0));
    }

    [Test]
    public void TypeAnnotation_UnitWithElementType_IsError()
    {
        var code = """
            define Point:
                inputs: x = 0 {m}
            end
            invalid {m * Point} = Point()
            """;
        var env = CreateAndAnalyse(code);
        Assert.That(env.Log.Errors.Count(), Is.GreaterThan(0));
    }

    #endregion

    #region Valid Unit Type Annotations (regression tests)

    [Test]
    public void TypeAnnotation_SimpleUnit_IsValid()
    {
        var code = "x {m} = 5 {m}";
        var env = CreateAndAnalyse(code);
        Assert.That(env.Log.Errors.Count(), Is.EqualTo(0));
    }

    [Test]
    public void TypeAnnotation_CompoundUnit_IsValid()
    {
        var code = "force {kg*m/s^2} = 10 {N}";
        var env = CreateAndAnalyse(code);
        Assert.That(env.Log.Errors.Count(), Is.EqualTo(0));
    }

    [Test]
    public void TypeAnnotation_UnitPower_IsValid()
    {
        var code = "area {m^2} = 25 {m^2}";
        var env = CreateAndAnalyse(code);
        Assert.That(env.Log.Errors.Count(), Is.EqualTo(0));
    }

    #endregion

    #region List Type Annotations

    [Test]
    public void TypeAnnotation_ElementList_IsValid()
    {
        var code = """
            define Point:
                inputs:
                    x = 0 {m}
                    y = 0 {m}
            end
            points {Point list} = [Point(x = 1 {m}, y = 2 {m}), Point(x = 3 {m}, y = 4 {m})]
            """;
        var env = CreateAndAnalyse(code);
        Assert.That(env.Log.Errors.Count(), Is.EqualTo(0),
            $"Unexpected errors: {string.Join(", ", env.Log.Errors.Select(e => e.Message))}");
    }

    [Test]
    public void TypeAnnotation_PrototypeList_IsValid()
    {
        var code = """
            prototype Shape:
            end
            define Circle as Shape:
                inputs: radius = 1 {m}
            end
            define Square as Shape:
                inputs: side = 1 {m}
            end
            shapes {Shape list} = [Circle(radius = 2 {m}), Square(side = 3 {m})]
            """;
        var env = CreateAndAnalyse(code);
        Assert.That(env.Log.Errors.Count(), Is.EqualTo(0),
            $"Unexpected errors: {string.Join(", ", env.Log.Errors.Select(e => e.Message))}");
    }

    [Test]
    public void TypeAnnotation_NumberList_IsValid()
    {
        var code = "numbers {number list} = [1, 2, 3, 4, 5]";
        var env = CreateAndAnalyse(code);
        Assert.That(env.Log.Errors.Count(), Is.EqualTo(0),
            $"Unexpected errors: {string.Join(", ", env.Log.Errors.Select(e => e.Message))}");
    }

    [Test]
    public void TypeAnnotation_TextList_IsValid()
    {
        var code = """
            names {text list} = ["Alice", "Bob", "Charlie"]
            """;
        var env = CreateAndAnalyse(code);
        Assert.That(env.Log.Errors.Count(), Is.EqualTo(0),
            $"Unexpected errors: {string.Join(", ", env.Log.Errors.Select(e => e.Message))}");
    }

    [Test]
    public void TypeAnnotation_UnitList_IsValid()
    {
        var code = "lengths {m list} = [1 {m}, 2 {m}, 3 {m}]";
        var env = CreateAndAnalyse(code);
        Assert.That(env.Log.Errors.Count(), Is.EqualTo(0),
            $"Unexpected errors: {string.Join(", ", env.Log.Errors.Select(e => e.Message))}");
    }

    [Test]
    public void TypeAnnotation_EmptyList_WithTypeAnnotation_IsValid()
    {
        var code = """
            define Point:
                inputs: x = 0 {m}
            end
            emptyPoints {Point list} = []
            """;
        var env = CreateAndAnalyse(code);
        Assert.That(env.Log.Errors.Count(), Is.EqualTo(0),
            $"Unexpected errors: {string.Join(", ", env.Log.Errors.Select(e => e.Message))}");
    }

    #endregion
}
