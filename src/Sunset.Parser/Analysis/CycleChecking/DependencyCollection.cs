using System.Collections;
using Sunset.Parser.Abstractions;

namespace Sunset.Parser.Analysis.CycleChecking;

public class DependencyCollection : IEnumerable
{
    private readonly HashSet<IDeclaration> _dependencies;

    public bool IsEmpty => _dependencies.Count == 0;

    public DependencyCollection()
    {
        _dependencies = [];
    }

    public DependencyCollection(IDeclaration dependency)
    {
        _dependencies = [dependency];
    }

    private DependencyCollection(IEnumerable<IDeclaration> dependencies)
    {
        _dependencies = [..dependencies];
    }

    /// <summary>
    /// Create a shallow copy of the collection.
    /// </summary>
    public DependencyCollection Clone()
    {
        // Return a copy of itself
        return new DependencyCollection(_dependencies);
    }

    /// <summary>
    /// Add the results of another DependencyCollection to this one.
    /// </summary>
    /// <returns>The original DependencyCollection with the contents of the other DependencyCollection included.</returns>
    public DependencyCollection Join(DependencyCollection? other)
    {
        if (other != null) _dependencies.UnionWith(other._dependencies);
        return this;
    }

    public DependencyCollection Join(IDeclaration? dependency)
    {
        if (dependency != null) _dependencies.Add(dependency);
        return this;
    }

    public IEnumerator GetEnumerator()
    {
        return _dependencies.GetEnumerator();
    }

    public string[] GetPaths()
    {
        return _dependencies.Select(d => d.FullPath).ToArray();
    }

    /// <summary>
    ///  Finds the declaration of a dependency by the name declared. Returns null if one does not exist.
    /// </summary>
    public IDeclaration? FindByName(string name)
    {
        return _dependencies.FirstOrDefault(d => d.Name == name);
    }
}