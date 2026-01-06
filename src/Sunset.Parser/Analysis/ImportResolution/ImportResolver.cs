using Sunset.Parser.Errors;
using Sunset.Parser.Errors.Semantic;
using Sunset.Parser.Packages;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Scopes;

namespace Sunset.Parser.Analysis.ImportResolution;

/// <summary>
///     Resolves import declarations to actual packages, modules, files, and declarations.
/// </summary>
public class ImportResolver
{
    /// <summary>
    ///     Flag to control re-export behaviour.
    ///     When true: imports in a file are re-exported to importers of that file.
    ///     When false: only declarations defined in the file are exported.
    /// </summary>
    public const bool ReExportImports = true;

    private readonly PackageRegistry _registry;
    private readonly ErrorLog _log;

    /// <summary>
    ///     Track files currently being processed to detect circular imports.
    /// </summary>
    private readonly HashSet<string> _processingStack = [];

    public ImportResolver(PackageRegistry registry, ErrorLog log)
    {
        _registry = registry;
        _log = log;
    }

    /// <summary>
    ///     Resolves all imports for a file scope.
    /// </summary>
    /// <param name="fileScope">The file scope containing import declarations.</param>
    /// <param name="sourceFilePath">The path of the source file (for relative import resolution).</param>
    /// <param name="imports">The import declarations to resolve.</param>
    /// <returns>The combined import resolution result.</returns>
    public ImportResolutionResult ResolveImportsForFile(
        FileScope fileScope,
        string? sourceFilePath,
        IEnumerable<ImportDeclaration> imports)
    {
        var result = new ImportResolutionResult();

        // Add to processing stack to detect circular imports
        var fileKey = fileScope.FullPath;
        if (_processingStack.Contains(fileKey))
        {
            // Circular import detected - this shouldn't happen at the file level
            // as it's handled per-import, but guard against it
            return result;
        }

        _processingStack.Add(fileKey);

        try
        {
            foreach (var import in imports)
            {
                var importResult = ResolveImport(import, sourceFilePath);

                // Merge results
                foreach (var (name, decl) in importResult.DirectImports)
                {
                    if (!result.DirectImports.ContainsKey(name))
                    {
                        result.DirectImports[name] = decl;
                    }
                    else
                    {
                        // Name collision - track as ambiguous for lazy error reporting at usage time
                        var existingDecl = result.DirectImports[name];
                        if (existingDecl.FullPath != decl.FullPath)
                        {
                            // Different declarations with the same name
                            if (!result.AmbiguousImports.ContainsKey(name))
                            {
                                result.AmbiguousImports[name] = [existingDecl.FullPath];
                            }
                            result.AmbiguousImports[name].Add(decl.FullPath);
                        }
                        // If same declaration (same FullPath), it's just a duplicate import - no action needed
                    }
                }

                foreach (var (name, scope) in importResult.ScopeImports)
                {
                    if (!result.ScopeImports.ContainsKey(name))
                    {
                        result.ScopeImports[name] = scope;
                    }
                }

                if (!importResult.Success)
                {
                    result.Success = false;
                }
            }
        }
        finally
        {
            _processingStack.Remove(fileKey);
        }

        return result;
    }

    /// <summary>
    ///     Resolves a single import declaration.
    /// </summary>
    private ImportResolutionResult ResolveImport(ImportDeclaration import, string? sourceFilePath)
    {
        var result = new ImportResolutionResult();

        if (import.PathSegments.Count == 0)
        {
            result.Success = false;
            return result;
        }

        // Resolve the package first
        PackageConfig? packageConfig;

        if (import.IsRelative)
        {
            // Relative import - resolve from source file location
            if (string.IsNullOrEmpty(sourceFilePath))
            {
                _log.Error(new PackageNotFoundError(
                    import.PathSegments[0].ToString(),
                    import.ImportToken));
                result.Success = false;
                return result;
            }

            var basePath = Path.GetDirectoryName(sourceFilePath) ?? sourceFilePath;
            packageConfig = _registry.ResolveRelativePackage(
                basePath,
                import.RelativeDepth,
                import.PathSegments[0].ToString());
        }
        else
        {
            // Absolute import - search registry
            packageConfig = _registry.ResolvePackage(import.PathSegments[0].ToString());
        }

        if (packageConfig == null)
        {
            _log.Error(new PackageNotFoundError(
                import.PathSegments[0].ToString(),
                import.ImportToken));
            result.Success = false;
            return result;
        }

        // Create and initialize the package
        var package = new Package(packageConfig, _log);

        // Navigate the path to find the target
        IScope currentScope = package;
        var pathIndex = 1; // Start after the package name

        while (pathIndex < import.PathSegments.Count)
        {
            var segment = import.PathSegments[pathIndex].ToString();

            // Try to get the next scope
            IScope? nextScope = null;

            if (currentScope is Package pkg)
            {
                nextScope = pkg.GetChildScope(segment);
            }
            else if (currentScope is Module mod)
            {
                nextScope = mod.GetChildScope(segment);
            }
            else if (currentScope is FileScope fileScope)
            {
                // We've reached a file - remaining segments are identifier references
                break;
            }

            if (nextScope == null)
            {
                // Check if it's a file or identifier we're looking for
                if (currentScope is Package pkgForFile)
                {
                    var fileScope = pkgForFile.GetChildScope(segment) as FileScope;
                    if (fileScope != null)
                    {
                        currentScope = fileScope;
                        pathIndex++;
                        continue;
                    }
                }
                else if (currentScope is Module modForFile)
                {
                    var fileScope = modForFile.GetChildScope(segment) as FileScope;
                    if (fileScope != null)
                    {
                        currentScope = fileScope;
                        pathIndex++;
                        continue;
                    }
                }

                // Not found
                _log.Error(new ModuleNotFoundError(
                    segment,
                    currentScope.FullPath,
                    import.PathSegments[pathIndex]));
                result.Success = false;
                return result;
            }

            currentScope = nextScope;
            pathIndex++;
        }

        // Now we need to determine what to import based on where we ended up

        if (import.SpecificIdentifiers != null && import.SpecificIdentifiers.Count > 0)
        {
            // Import specific identifiers from a file
            if (currentScope is not FileScope fileScope)
            {
                _log.Error(new FileNotFoundInModuleError(
                    "identifiers",
                    currentScope.FullPath,
                    import.ImportToken));
                result.Success = false;
                return result;
            }

            foreach (var identifierToken in import.SpecificIdentifiers)
            {
                var identifier = identifierToken.ToString();
                var decl = fileScope.TryGetExportedDeclaration(identifier);

                if (decl == null)
                {
                    // Check if it's private
                    var privateDecl = fileScope.TryGetDeclaration(identifier);
                    if (privateDecl != null)
                    {
                        _log.Error(new PrivateIdentifierImportError(
                            identifier,
                            fileScope.FullPath,
                            identifierToken));
                    }
                    else
                    {
                        _log.Error(new IdentifierNotFoundInFileError(
                            identifier,
                            fileScope.FullPath,
                            identifierToken));
                    }
                    result.Success = false;
                    continue;
                }

                result.DirectImports[identifier] = decl;
            }
        }
        else if (currentScope is FileScope targetFile)
        {
            // Import all exported declarations from the file
            foreach (var (name, decl) in targetFile.ExportedDeclarations)
            {
                result.DirectImports[name] = decl;
            }

            // Handle re-exports if enabled
            if (ReExportImports)
            {
                // TODO: Recursively resolve imports in the target file and include them
                // This requires tracking to avoid infinite recursion
            }
        }
        else if (currentScope is Module || currentScope is Package)
        {
            // Import the scope itself (requires qualification for access)
            result.ScopeImports[currentScope.Name] = currentScope;
        }

        return result;
    }
}
