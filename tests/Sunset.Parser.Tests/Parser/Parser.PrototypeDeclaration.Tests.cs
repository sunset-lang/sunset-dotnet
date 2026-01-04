using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Scopes;

namespace Sunset.Parser.Test.Parser;

[TestFixture]
public class ParserPrototypeDeclarationTests
{
    [Test]
    public void GetPrototypeDeclaration_EmptyPrototype_CorrectDeclaration()
    {
        var parser = new Parsing.Parser(SourceFile.FromString("""
            prototype Empty:
            end
            """));

        var declarations = parser.Parse(new FileScope("$", null));
        var prototype = declarations.OfType<PrototypeDeclaration>().FirstOrDefault();

        Assert.That(prototype, Is.Not.Null);
        Assert.That(prototype!.Name, Is.EqualTo("Empty"));
        Assert.That(prototype.ChildDeclarations, Is.Empty);
    }

    [Test]
    public void GetPrototypeDeclaration_WithOutputs_CorrectDeclaration()
    {
        var parser = new Parsing.Parser(SourceFile.FromString("""
            prototype Shape:
                outputs:
                    return Area {m^2}
            end
            """));

        var declarations = parser.Parse(new FileScope("$", null));
        var prototype = declarations.OfType<PrototypeDeclaration>().FirstOrDefault();

        Assert.That(prototype, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(prototype!.Name, Is.EqualTo("Shape"));
            Assert.That(prototype.ChildDeclarations.ContainsKey("Area"), Is.True);
            Assert.That(prototype.ExplicitDefaultReturn, Is.Not.Null);
            Assert.That(prototype.ExplicitDefaultReturn!.Name, Is.EqualTo("Area"));
        });
    }

    [Test]
    public void GetPrototypeDeclaration_WithInputs_CorrectDeclaration()
    {
        var parser = new Parsing.Parser(SourceFile.FromString("""
            prototype Rectangular:
                inputs:
                    Width = 1 {m}
                    Length = 2 {m}
            end
            """));

        var declarations = parser.Parse(new FileScope("$", null));
        var prototype = declarations.OfType<PrototypeDeclaration>().FirstOrDefault();

        Assert.That(prototype, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(prototype!.Name, Is.EqualTo("Rectangular"));
            Assert.That(prototype.ChildDeclarations.ContainsKey("Width"), Is.True);
            Assert.That(prototype.ChildDeclarations.ContainsKey("Length"), Is.True);
        });
    }

    [Test]
    public void GetPrototypeDeclaration_WithInputsAndOutputs_CorrectDeclaration()
    {
        var parser = new Parsing.Parser(SourceFile.FromString("""
            prototype Rectangular:
                inputs:
                    Width = 1 {m}
                    Length = 2 {m}
                outputs:
                    return Area {m^2}
            end
            """));

        var declarations = parser.Parse(new FileScope("$", null));
        var prototype = declarations.OfType<PrototypeDeclaration>().FirstOrDefault();

        Assert.That(prototype, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(prototype!.Name, Is.EqualTo("Rectangular"));
            Assert.That(prototype.ChildDeclarations.ContainsKey("Width"), Is.True);
            Assert.That(prototype.ChildDeclarations.ContainsKey("Length"), Is.True);
            Assert.That(prototype.ChildDeclarations.ContainsKey("Area"), Is.True);
            Assert.That(prototype.ExplicitDefaultReturn!.Name, Is.EqualTo("Area"));
        });
    }

    [Test]
    public void GetPrototypeDeclaration_WithInheritance_CorrectDeclaration()
    {
        var parser = new Parsing.Parser(SourceFile.FromString("""
            prototype Polygon as Shape:
                outputs:
                    Sides
            end
            """));

        var declarations = parser.Parse(new FileScope("$", null));
        var prototype = declarations.OfType<PrototypeDeclaration>().FirstOrDefault();

        Assert.That(prototype, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(prototype!.Name, Is.EqualTo("Polygon"));
            Assert.That(prototype.BasePrototypeTokens, Is.Not.Null);
            Assert.That(prototype.BasePrototypeTokens!.Count, Is.EqualTo(1));
            Assert.That(prototype.BasePrototypeTokens[0].ToString(), Is.EqualTo("Shape"));
            Assert.That(prototype.ChildDeclarations.ContainsKey("Sides"), Is.True);
        });
    }

    [Test]
    public void GetPrototypeDeclaration_WithMultipleInheritance_CorrectDeclaration()
    {
        var parser = new Parsing.Parser(SourceFile.FromString("""
            prototype ColoredShape as Shape, Colored:
                outputs:
                    Description
            end
            """));

        var declarations = parser.Parse(new FileScope("$", null));
        var prototype = declarations.OfType<PrototypeDeclaration>().FirstOrDefault();

        Assert.That(prototype, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(prototype!.Name, Is.EqualTo("ColoredShape"));
            Assert.That(prototype.BasePrototypeTokens, Is.Not.Null);
            Assert.That(prototype.BasePrototypeTokens!.Count, Is.EqualTo(2));
            Assert.That(prototype.BasePrototypeTokens[0].ToString(), Is.EqualTo("Shape"));
            Assert.That(prototype.BasePrototypeTokens[1].ToString(), Is.EqualTo("Colored"));
        });
    }

    [Test]
    public void GetElementDeclaration_WithPrototype_CorrectDeclaration()
    {
        var parser = new Parsing.Parser(SourceFile.FromString("""
            define Square as Shape:
                inputs:
                    Width = 1 {m}
                outputs:
                    return Area {m^2} = Width ^ 2
            end
            """));

        var declarations = parser.Parse(new FileScope("$", null));
        var element = declarations.OfType<ElementDeclaration>().FirstOrDefault();

        Assert.That(element, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(element!.Name, Is.EqualTo("Square"));
            Assert.That(element.PrototypeNameTokens, Is.Not.Null);
            Assert.That(element.PrototypeNameTokens!.Count, Is.EqualTo(1));
            Assert.That(element.PrototypeNameTokens[0].ToString(), Is.EqualTo("Shape"));
        });
    }

    [Test]
    public void GetElementDeclaration_WithMultiplePrototypes_CorrectDeclaration()
    {
        var parser = new Parsing.Parser(SourceFile.FromString("""
            define Square as Shape, Rectangular:
                inputs:
                    Width = 1 {m}
                outputs:
                    return Area {m^2} = Width ^ 2
                    Perimeter {m} = 4 * Width
            end
            """));

        var declarations = parser.Parse(new FileScope("$", null));
        var element = declarations.OfType<ElementDeclaration>().FirstOrDefault();

        Assert.That(element, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(element!.Name, Is.EqualTo("Square"));
            Assert.That(element.PrototypeNameTokens, Is.Not.Null);
            Assert.That(element.PrototypeNameTokens!.Count, Is.EqualTo(2));
            Assert.That(element.PrototypeNameTokens[0].ToString(), Is.EqualTo("Shape"));
            Assert.That(element.PrototypeNameTokens[1].ToString(), Is.EqualTo("Rectangular"));
        });
    }

    [Test]
    public void GetPrototypeDeclaration_MultipleOutputsWithReturn_CorrectDeclaration()
    {
        var parser = new Parsing.Parser(SourceFile.FromString("""
            prototype Shape:
                outputs:
                    return Area {m^2}
                    Perimeter {m}
            end
            """));

        var declarations = parser.Parse(new FileScope("$", null));
        var prototype = declarations.OfType<PrototypeDeclaration>().FirstOrDefault();

        Assert.That(prototype, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(prototype!.ChildDeclarations.ContainsKey("Area"), Is.True);
            Assert.That(prototype.ChildDeclarations.ContainsKey("Perimeter"), Is.True);
            Assert.That(prototype.ExplicitDefaultReturn!.Name, Is.EqualTo("Area"));
        });
    }

    [Test]
    public void Parse_PrototypeAndElement_BothParsed()
    {
        var parser = new Parsing.Parser(SourceFile.FromString("""
            prototype Shape:
                outputs:
                    return Area {m^2}
            end

            define Square as Shape:
                inputs:
                    Width = 1 {m}
                outputs:
                    return Area {m^2} = Width ^ 2
            end
            """));

        var declarations = parser.Parse(new FileScope("$", null));
        
        var prototype = declarations.OfType<PrototypeDeclaration>().FirstOrDefault();
        var element = declarations.OfType<ElementDeclaration>().FirstOrDefault();

        Assert.That(prototype, Is.Not.Null);
        Assert.That(element, Is.Not.Null);
        Assert.That(prototype!.Name, Is.EqualTo("Shape"));
        Assert.That(element!.Name, Is.EqualTo("Square"));
        Assert.That(element.PrototypeNameTokens![0].ToString(), Is.EqualTo("Shape"));
    }
}
