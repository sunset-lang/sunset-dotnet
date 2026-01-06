using Sunset.Parser.Lexing.Tokens;

namespace Sunset.Parser.Errors.Semantic;

/// <summary>
///     Error when a package cannot be found in any search path.
/// </summary>
public class PackageNotFoundError(string packageName, IToken token) : ISemanticError
{
    public string PackageName { get; } = packageName;
    public string Message => $"Package '{PackageName}' not found";
    public Dictionary<Language, string> Translations { get; } = new();
    public IToken? StartToken => token;
    public IToken? EndToken => token;
}

/// <summary>
///     Error when a module cannot be found within a package.
/// </summary>
public class ModuleNotFoundError(string moduleName, string packageName, IToken token) : ISemanticError
{
    public string ModuleName { get; } = moduleName;
    public string PackageName { get; } = packageName;
    public string Message => $"Module '{ModuleName}' not found in package '{PackageName}'";
    public Dictionary<Language, string> Translations { get; } = new();
    public IToken? StartToken => token;
    public IToken? EndToken => token;
}

/// <summary>
///     Error when a file cannot be found within a module or package.
/// </summary>
public class FileNotFoundInModuleError(string fileName, string modulePath, IToken token) : ISemanticError
{
    public string FileName { get; } = fileName;
    public string ModulePath { get; } = modulePath;
    public string Message => $"File '{FileName}' not found in '{ModulePath}'";
    public Dictionary<Language, string> Translations { get; } = new();
    public IToken? StartToken => token;
    public IToken? EndToken => token;
}

/// <summary>
///     Error when an identifier cannot be found in a file.
/// </summary>
public class IdentifierNotFoundInFileError(string identifier, string filePath, IToken token) : ISemanticError
{
    public string Identifier { get; } = identifier;
    public string FilePath { get; } = filePath;
    public string Message => $"Identifier '{Identifier}' not found in '{FilePath}'";
    public Dictionary<Language, string> Translations { get; } = new();
    public IToken? StartToken => token;
    public IToken? EndToken => token;
}

/// <summary>
///     Error when a circular import is detected.
/// </summary>
public class CircularImportError(string currentFile, List<string> importChain, IToken token) : ISemanticError
{
    public string CurrentFile { get; } = currentFile;
    public List<string> ImportChain { get; } = importChain;
    public string Message => $"Circular import detected: {string.Join(" -> ", ImportChain)} -> {CurrentFile}";
    public Dictionary<Language, string> Translations { get; } = new();
    public IToken? StartToken => token;
    public IToken? EndToken => token;
}

/// <summary>
///     Error when an identifier is ambiguous due to multiple imports.
/// </summary>
public class AmbiguousIdentifierError(string identifier, List<string> sources, IToken token) : ISemanticError
{
    public string Identifier { get; } = identifier;
    public List<string> Sources { get; } = sources;
    public string Message => $"Ambiguous identifier '{Identifier}'. Found in: {string.Join(", ", Sources)}";
    public Dictionary<Language, string> Translations { get; } = new();
    public IToken? StartToken => token;
    public IToken? EndToken => token;
}

/// <summary>
///     Error when attempting to import a private identifier.
/// </summary>
public class PrivateIdentifierImportError(string identifier, string filePath, IToken token) : ISemanticError
{
    public string Identifier { get; } = identifier;
    public string FilePath { get; } = filePath;
    public string Message => $"Cannot import private identifier '{Identifier}' from '{FilePath}'";
    public Dictionary<Language, string> Translations { get; } = new();
    public IToken? StartToken => token;
    public IToken? EndToken => token;
}
