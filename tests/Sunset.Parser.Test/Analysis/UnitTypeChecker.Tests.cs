using Sunset.Parser.Analysis;
using Sunset.Parser.Expressions;

namespace Sunset.Parser.Test.Analysis;

[TestFixture]
public class UnitTypeCheckerTests
{
    private readonly UnitTypeChecker _typeChecker = new();

    public VariableDeclaration GetVariableDeclaration(string input)
    {
        var parser = new Parsing.Parser(input);
        var declaration = parser.SyntaxTree.FirstOrDefault();
        if (declaration is null)
        {
            throw new Exception("Expression not parsed.");
        }

        if (declaration is VariableDeclaration variableDeclaration)
        {
            return variableDeclaration;
        }

        throw new Exception("Expression not a variable declaration.");
    }

    [Test]
    public void Visit_VariableDeclaration_WithSimpleValidUnits_CorrectUnits()
    {
        var declaration = GetVariableDeclaration("area <A> {mm^2} = 100 {mm} * 200 {mm} + 400 {mm^2}");

        var unit = _typeChecker.Visit(declaration);
        if (unit is null)
        {
            Assert.Fail("Unit type checker did not return a unit.");
            return;
        }

        Assert.That(unit.ToString(), Is.EqualTo("mm^2"));
    }

    [Test]
    public void Visit_VariableDeclaration_WithSimpleInvalidUnits_CreatesError()
    {
        var declaration = GetVariableDeclaration("area <A> {mm^2} = 100 {mm} * 200 {mm} + 400 {mm}");

        var unit = _typeChecker.Visit(declaration);
        Assert.That(unit, Is.Null);
    }

    [Test]
    public void Visit_VariableDeclaration_WithComplexValidUnits_CorrectUnits()
    {
        var declaration = GetVariableDeclaration("area <A> {kN} = 100 {kg} * 200 {m} / (400 {s})^2");

        var unit = _typeChecker.Visit(declaration);
        if (unit is null)
        {
            Assert.Fail("Unit type checker did not return a unit.");
            return;
        }

        Assert.That(unit.ToString(), Is.EqualTo("kN"));
    }

    [Test]
    public void Visit_VariableDeclaration_WithComplexInvalidUnits_CreatesError()
    {
        var declaration = GetVariableDeclaration("area <A> {kN} = 100 {kg} * 200 {m} / (400 {s})^3");

        var unit = _typeChecker.Visit(declaration);
        Assert.That(unit, Is.Null);
    }
}