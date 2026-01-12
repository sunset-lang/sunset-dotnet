using Sunset.Parser.Analysis.NameResolution;
using Sunset.Parser.Errors;
using Sunset.Parser.Errors.Semantic;
using Sunset.Parser.Packages;
using Sunset.Parser.Parsing.Declarations;
using Sunset.Parser.Scopes;
using Sunset.Parser.Visitors;

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

    /// <summary>
    ///     Pass data key to track if a file has been analyzed.
    /// </summary>
    private const string AnalyzedPassDataKey = "ImportResolver.Analyzed";

    private readonly PackageRegistry _registry;
    private readonly ErrorLog _log;
    private readonly Package? _standardLibraryPackage;

    /// <summary>
    ///     Track files currently being processed to detect circular imports.
    /// </summary>
    private readonly HashSet<string> _processingStack = [];

    /// <summary>
    ///     Track files currently being analyzed to detect circular dependencies.
    /// </summary>
    private readonly HashSet<string> _analyzingStack = [];

    public ImportResolver(PackageRegistry registry, ErrorLog log, Package? standardLibraryPackage = null)
    {
        _registry = registry;
        _log = log;
        _standardLibraryPackage = standardLibraryPackage;
    }

    /// <summary>
    ///     Ensures a file scope has been analyzed (name resolution has run).
    ///     This is necessary for imported files from StandardLibrary which are parsed but not analyzed.
    ///     Only applies to StandardLibrary files - external package files are analyzed as part of the
    ///     main analysis pass.
    /// </summary>
    private void EnsureFileAnalyzed(FileScope fileScope)
    {
        // Only analyze files from the StandardLibrary package.
        // External package files are analyzed as part of the main Environment.Analyse() pass
        // where they have proper access to units and other dependencies.
        if (!IsStandardLibraryFile(fileScope))
        {
            return;
        }

        // Check if already analyzed
        if (fileScope.PassData.ContainsKey(AnalyzedPassDataKey))
        {
            return;
        }

        // Check for circular analysis dependency
        var fileKey = fileScope.FullPath;
        if (_analyzingStack.Contains(fileKey))
        {
            // Circular dependency - file is already being analyzed
            return;
        }

        _analyzingStack.Add(fileKey);

        try
        {
            // First, resolve imports for this file (if not already done)
            if (!fileScope.PassData.ContainsKey(nameof(ImportPassData)))
            {
                var imports = fileScope.ChildDeclarations.Values
                    .OfType<ImportDeclaration>()
                    .ToList();

                if (imports.Count > 0)
                {
                    var importResult = ResolveImportsForFile(fileScope, null, imports);
                    fileScope.PassData[nameof(ImportPassData)] = new ImportPassData
                    {
                        ResolvedImports = importResult
                    };
                }
            }

            // Ensure all imported StandardLibrary files are analyzed first
            // This populates their ImportPassData before we run name resolution
            if (fileScope.PassData.TryGetValue(nameof(ImportPassData), out var passDataObj) &&
                passDataObj is ImportPassData importPassData)
            {
                foreach (var (_, decl) in importPassData.ResolvedImports.DirectImports)
                {
                    // Find the containing FileScope for this declaration
                    var declScope = decl as IScope ?? decl.ParentScope;
                    while (declScope != null && declScope is not FileScope)
                    {
                        declScope = declScope.ParentScope;
                    }

                    if (declScope is FileScope importedFile && IsStandardLibraryFile(importedFile))
                    {
                        EnsureFileAnalyzed(importedFile);
                    }
                }
            }

            // Run name resolution on the file scope
            var nameResolver = new NameResolver(_log);
            nameResolver.Visit(fileScope);

            // Mark as analyzed
            fileScope.PassData[AnalyzedPassDataKey] = new AnalyzedPassData();
        }
        finally
        {
            _analyzingStack.Remove(fileKey);
        }
    }

    /// <summary>
    ///     Checks if a file scope belongs to the StandardLibrary package.
    /// </summary>
    private bool IsStandardLibraryFile(FileScope fileScope)
    {
        if (_standardLibraryPackage == null)
        {
            return false;
        }

        // Walk up the parent scope chain to find the package
        IScope? current = fileScope.ParentScope;
        while (current != null)
        {
            if (current == _standardLibraryPackage)
            {
                return true;
            }
            current = current.ParentScope;
        }

        return false;
    }

    /// <summary>
    ///     Pass data marker indicating a file has been analyzed.
    /// </summary>
    private class AnalyzedPassData : IPassData { }

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
            // Try to resolve as a StandardLibrary module (e.g., "import Diagrams.X" -> "StandardLibrary/Diagrams/X")
            var standardLibraryResult = TryResolveFromStandardLibrary(import);
            if (standardLibraryResult != null)
            {
                return standardLibraryResult;
            }

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

            // Ensure the file has been analyzed before accessing its declarations
            EnsureFileAnalyzed(fileScope);

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
            // Ensure the file has been analyzed before accessing its declarations
            EnsureFileAnalyzed(targetFile);

            // Import all exported declarations from the file
            foreach (var (name, decl) in targetFile.ExportedDeclarations)
            {
                result.DirectImports[name] = decl;
            }

            // Handle re-exports if enabled
            if (ReExportImports)
            {
                // Check if this file has already been processed for imports
                if (!targetFile.PassData.ContainsKey(nameof(ImportPassData)))
                {
                    // Get import declarations from the target file
                    var targetImports = targetFile.ChildDeclarations.Values
                        .OfType<ImportDeclaration>()
                        .ToList();
                    
                    if (targetImports.Count > 0)
                    {
                        // Recursively resolve imports for the target file
                        // The processing stack in ResolveImportsForFile will prevent circular imports
                        var targetResult = ResolveImportsForFile(
                            targetFile,
                            null, // We don't have the file path for standard library files
                            targetImports);
                        
                        // Store the result in the target file's pass data
                        targetFile.PassData[nameof(ImportPassData)] = new ImportPassData 
                        { 
                            ResolvedImports = targetResult 
                        };
                    }
                }
                
                // Re-export the target file's resolved imports
                if (targetFile.PassData.TryGetValue(nameof(ImportPassData), out var importData) &&
                    importData is ImportPassData targetImportData)
                {
                    foreach (var (name, decl) in targetImportData.ResolvedImports.DirectImports)
                    {
                        if (!result.DirectImports.ContainsKey(name))
                        {
                            result.DirectImports[name] = decl;
                        }
                    }
                }
            }
        }
        else if (currentScope is Module || currentScope is Package)
        {
            // Import the scope itself (requires qualification for access)
            result.ScopeImports[currentScope.Name] = currentScope;
        }

        return result;
    }

    /// <summary>
    ///     Tries to resolve an import from the StandardLibrary package.
    ///     This allows "import Diagrams.Geometry" to resolve to "StandardLibrary/Diagrams/Geometry.sun".
    /// </summary>
    private ImportResolutionResult? TryResolveFromStandardLibrary(ImportDeclaration import)
    {
        if (_standardLibraryPackage == null || import.IsRelative)
        {
            return null;
        }

        // Try to navigate from StandardLibrary root using the import path segments
        // e.g., "import Diagrams.Geometry" tries to find StandardLibrary/Diagrams/Geometry.sun
        IScope currentScope = _standardLibraryPackage;

        for (var i = 0; i < import.PathSegments.Count; i++)
        {
            var segment = import.PathSegments[i].ToString();
            IScope? nextScope = null;

            if (currentScope is Package pkg)
            {
                nextScope = pkg.GetChildScope(segment);
            }
            else if (currentScope is Module mod)
            {
                nextScope = mod.GetChildScope(segment);
            }
            else if (currentScope is FileScope)
            {
                // We've reached a file - remaining segments are identifier references
                break;
            }

            if (nextScope == null)
            {
                // Not found in StandardLibrary
                return null;
            }

            currentScope = nextScope;
        }

        // If we ended up at a Module, check for an "index file" (file with same name as module)
        // e.g., import Diagrams -> look for Diagrams/Diagrams.sun
        if (currentScope is Module indexModule)
        {
            var indexFile = indexModule.GetChildScope(indexModule.Name);
            if (indexFile is FileScope indexFileScope)
            {
                currentScope = indexFileScope;
            }
        }

        // Build result based on what we found
        var result = new ImportResolutionResult();

        if (import.SpecificIdentifiers != null && import.SpecificIdentifiers.Count > 0)
        {
            // Import specific identifiers from a file
            if (currentScope is not FileScope fileScope)
            {
                return null;
            }

            // Ensure the file has been analyzed before accessing its declarations
            EnsureFileAnalyzed(fileScope);

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
            // Ensure the file has been analyzed before accessing its declarations
            EnsureFileAnalyzed(targetFile);

            // Import all exported declarations from the file
            foreach (var (name, decl) in targetFile.ExportedDeclarations)
            {
                result.DirectImports[name] = decl;
            }

            // Handle re-exports if enabled
            if (ReExportImports)
            {
                // Check if this file has already been processed for imports
                if (!targetFile.PassData.ContainsKey(nameof(ImportPassData)))
                {
                    // Get import declarations from the target file
                    var targetImports = targetFile.ChildDeclarations.Values
                        .OfType<ImportDeclaration>()
                        .ToList();
                    
                    if (targetImports.Count > 0)
                    {
                        // Recursively resolve imports for the target file
                        var targetResult = ResolveImportsForFile(
                            targetFile,
                            null,
                            targetImports);
                        
                        // Store the result in the target file's pass data
                        targetFile.PassData[nameof(ImportPassData)] = new ImportPassData 
                        { 
                            ResolvedImports = targetResult 
                        };
                    }
                }
                
                // Re-export the target file's resolved imports
                if (targetFile.PassData.TryGetValue(nameof(ImportPassData), out var importData) &&
                    importData is ImportPassData targetImportData)
                {
                    foreach (var (name, decl) in targetImportData.ResolvedImports.DirectImports)
                    {
                        if (!result.DirectImports.ContainsKey(name))
                        {
                            result.DirectImports[name] = decl;
                        }
                    }
                }
            }
        }
        else if (currentScope is Module || currentScope is Package)
        {
            // Import the scope itself (requires qualification for access)
            result.ScopeImports[currentScope.Name] = currentScope;
        }
        else
        {
            return null;
        }

        return result;
    }
}
