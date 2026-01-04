using Sunset.Parser.BuiltIns;
using Sunset.Parser.BuiltIns.ListMethods;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Visitors;

namespace Sunset.Parser.Analysis.NameResolution;

public static class NameResolverExtensions
{
    private const string PassDataKey = "NameResolver";

    public static IDeclaration? GetResolvedDeclaration(this IVisitable dest)
    {
        return dest.GetPassData<NamePassData>(PassDataKey).ResolvedDeclaration;
    }

    public static void SetResolvedDeclaration(this IVisitable dest, IDeclaration declaration)
    {
        dest.GetPassData<NamePassData>(PassDataKey).ResolvedDeclaration = declaration;
    }

    /// <summary>
    /// Gets the built-in function associated with this call expression, if any.
    /// </summary>
    public static IBuiltInFunction? GetBuiltInFunction(this IVisitable dest)
    {
        return dest.GetPassData<NamePassData>(PassDataKey).BuiltInFunction;
    }

    /// <summary>
    /// Sets the built-in function for this call expression.
    /// </summary>
    public static void SetBuiltInFunction(this IVisitable dest, IBuiltInFunction function)
    {
        dest.GetPassData<NamePassData>(PassDataKey).BuiltInFunction = function;
    }

    /// <summary>
    /// Checks if this call expression is a built-in function call.
    /// </summary>
    public static bool IsBuiltInFunctionCall(this IVisitable dest)
    {
        return dest.GetPassData<NamePassData>(PassDataKey).BuiltInFunction != null;
    }

    /// <summary>
    /// Gets the list method associated with this call expression, if any.
    /// </summary>
    public static IListMethod? GetListMethod(this IVisitable dest)
    {
        return dest.GetPassData<NamePassData>(PassDataKey).ListMethod;
    }

    /// <summary>
    /// Sets the list method for this call expression.
    /// </summary>
    public static void SetListMethod(this IVisitable dest, IListMethod method)
    {
        dest.GetPassData<NamePassData>(PassDataKey).ListMethod = method;
    }

    /// <summary>
    /// Checks if this call expression is a list method call.
    /// </summary>
    public static bool IsListMethodCall(this IVisitable dest)
    {
        return dest.GetPassData<NamePassData>(PassDataKey).ListMethod != null;
    }

    /// <summary>
    /// Gets the source instance for re-instantiation, if any.
    /// </summary>
    public static VariableDeclaration? GetSourceInstance(this IVisitable dest)
    {
        return dest.GetPassData<NamePassData>(PassDataKey).SourceInstance;
    }

    /// <summary>
    /// Sets the source instance for re-instantiation.
    /// </summary>
    public static void SetSourceInstance(this IVisitable dest, VariableDeclaration sourceInstance)
    {
        dest.GetPassData<NamePassData>(PassDataKey).SourceInstance = sourceInstance;
    }

    /// <summary>
    /// Checks if this call expression is a re-instantiation (partial application).
    /// </summary>
    public static bool IsReinstantiation(this IVisitable dest)
    {
        return dest.GetPassData<NamePassData>(PassDataKey).SourceInstance != null;
    }
}