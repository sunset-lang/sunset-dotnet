using Sunset.Parser.Expressions;
using Sunset.Parser.Parsing.Constants;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Scopes;

namespace Sunset.Parser.Test.Parser;

[TestFixture]
public class ParserOptionDeclarationTests
{
    [Test]
    public void Parse_QuantityOption_CorrectDeclaration()
    {
        var parser = new Parsing.Parser(SourceFile.FromString("""
            option Size {m}:
                10 {m}
                20 {m}
            end
            """));
        var declarations = parser.Parse(new FileScope("$", null));

        Assert.That(declarations, Has.Count.EqualTo(1));
        var option = declarations[0] as OptionDeclaration;
        Assert.That(option, Is.Not.Null);
        Assert.That(option!.Name, Is.EqualTo("Size"));
        Assert.That(option.TypeAnnotation, Is.Not.Null);
        Assert.That(option.Values, Has.Count.EqualTo(2));
    }

    [Test]
    public void Parse_TextOption_CorrectDeclaration()
    {
        var parser = new Parsing.Parser(SourceFile.FromString("""
            option Methods {text}:
                "SVG"
                "Typst"
            end
            """));
        var declarations = parser.Parse(new FileScope("$", null));

        Assert.That(declarations, Has.Count.EqualTo(1));
        var option = declarations[0] as OptionDeclaration;
        Assert.That(option, Is.Not.Null);
        Assert.That(option!.Name, Is.EqualTo("Methods"));
        Assert.That(option.TypeAnnotation, Is.Not.Null);
        Assert.That(option.TypeAnnotation, Is.TypeOf<NameExpression>());
        Assert.That(option.Values, Has.Count.EqualTo(2));
        
        // Verify the values are string expressions
        Assert.That(option.Values[0], Is.TypeOf<StringConstant>());
        Assert.That(option.Values[1], Is.TypeOf<StringConstant>());
    }

    [Test]
    public void Parse_NumberOption_CorrectDeclaration()
    {
        var parser = new Parsing.Parser(SourceFile.FromString("""
            option Scale {number}:
                1
                2
                5
            end
            """));
        var declarations = parser.Parse(new FileScope("$", null));

        Assert.That(declarations, Has.Count.EqualTo(1));
        var option = declarations[0] as OptionDeclaration;
        Assert.That(option, Is.Not.Null);
        Assert.That(option!.Name, Is.EqualTo("Scale"));
        Assert.That(option.TypeAnnotation, Is.Not.Null);
        Assert.That(option.TypeAnnotation, Is.TypeOf<NameExpression>());
        Assert.That(option.Values, Has.Count.EqualTo(3));
    }

    [Test]
    public void Parse_InferredTypeOption_NoTypeAnnotation()
    {
        var parser = new Parsing.Parser(SourceFile.FromString("""
            option Size:
                10 {m}
                20 {m}
            end
            """));
        var declarations = parser.Parse(new FileScope("$", null));

        Assert.That(declarations, Has.Count.EqualTo(1));
        var option = declarations[0] as OptionDeclaration;
        Assert.That(option, Is.Not.Null);
        Assert.That(option!.Name, Is.EqualTo("Size"));
        Assert.That(option.TypeAnnotation, Is.Null);
        Assert.That(option.Values, Has.Count.EqualTo(2));
    }

    [Test]
    public void Parse_OptionWithSingleValue_CorrectDeclaration()
    {
        var parser = new Parsing.Parser(SourceFile.FromString("""
            option SingleValue {m}:
                42 {m}
            end
            """));
        var declarations = parser.Parse(new FileScope("$", null));

        var option = declarations[0] as OptionDeclaration;
        Assert.That(option, Is.Not.Null);
        Assert.That(option!.Values, Has.Count.EqualTo(1));
    }

    [Test]
    public void Parse_OptionWithComplexUnit_CorrectDeclaration()
    {
        var parser = new Parsing.Parser(SourceFile.FromString("""
            option Velocity {m/s}:
                10 {m/s}
                20 {m/s}
            end
            """));
        var declarations = parser.Parse(new FileScope("$", null));

        var option = declarations[0] as OptionDeclaration;
        Assert.That(option, Is.Not.Null);
        Assert.That(option!.Name, Is.EqualTo("Velocity"));
        Assert.That(option.TypeAnnotation, Is.Not.Null);
    }

    [Test]
    public void Parse_MultipleOptions_CorrectDeclarations()
    {
        var parser = new Parsing.Parser(SourceFile.FromString("""
            option Size {m}:
                10 {m}
                20 {m}
            end
            
            option Methods {text}:
                "SVG"
                "Typst"
            end
            """));
        var declarations = parser.Parse(new FileScope("$", null));

        Assert.That(declarations, Has.Count.EqualTo(2));
        Assert.That(declarations[0], Is.TypeOf<OptionDeclaration>());
        Assert.That(declarations[1], Is.TypeOf<OptionDeclaration>());
        
        Assert.That((declarations[0] as OptionDeclaration)!.Name, Is.EqualTo("Size"));
        Assert.That((declarations[1] as OptionDeclaration)!.Name, Is.EqualTo("Methods"));
    }

    [Test]
    public void Parse_OptionFollowedByVariable_BothParsed()
    {
        var parser = new Parsing.Parser(SourceFile.FromString("""
            option Size {m}:
                10 {m}
                20 {m}
            end
            
            x = 5
            """));
        var declarations = parser.Parse(new FileScope("$", null));

        Assert.That(declarations, Has.Count.EqualTo(2));
        Assert.That(declarations[0], Is.TypeOf<OptionDeclaration>());
        Assert.That(declarations[1], Is.TypeOf<VariableDeclaration>());
    }
}
