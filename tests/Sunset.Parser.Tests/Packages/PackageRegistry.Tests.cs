using Sunset.Parser.Errors;
using Sunset.Parser.Packages;

namespace Sunset.Parser.Test.Packages;

[TestFixture]
public class PackageRegistryTests
{
    private string _testDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "sunset-registry-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    private string CreateTestPackage(string packageName, string? tomlContent = null)
    {
        var packageDir = Path.Combine(_testDirectory, packageName);
        Directory.CreateDirectory(packageDir);
        
        var content = tomlContent ?? $"""
            [package]
            version = "1.0.0"
            description = "Test package {packageName}"
            """;
        
        File.WriteAllText(Path.Combine(packageDir, PackageConfigLoader.PackageFileName), content);
        return packageDir;
    }

    [Test]
    public void DefaultPackagePath_IsUserFolder()
    {
        var expectedPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".sunset",
            "packages");

        Assert.That(PackageRegistry.DefaultPackagePath, Is.EqualTo(expectedPath));
    }

    [Test]
    public void Constructor_WithNoSearchPaths_UsesDefault()
    {
        var log = new ErrorLog();
        var registry = new PackageRegistry(log);

        Assert.That(registry.SearchPaths, Contains.Item(PackageRegistry.DefaultPackagePath));
    }

    [Test]
    public void AddSearchPath_AddsToSearchPaths()
    {
        var log = new ErrorLog();
        var registry = new PackageRegistry(log);
        
        registry.AddSearchPath(_testDirectory);

        Assert.That(registry.SearchPaths, Contains.Item(_testDirectory));
    }

    [Test]
    public void ResolvePackage_WithExistingPackage_ReturnsConfig()
    {
        CreateTestPackage("mypackage");
        
        var log = new ErrorLog();
        var registry = new PackageRegistry(log);
        registry.AddSearchPath(_testDirectory);

        var config = registry.ResolvePackage("mypackage");

        Assert.That(config, Is.Not.Null);
        Assert.That(config!.Name, Is.EqualTo("mypackage"));
    }

    [Test]
    public void ResolvePackage_WithNonExistentPackage_ReturnsNull()
    {
        var log = new ErrorLog();
        var registry = new PackageRegistry(log);
        registry.AddSearchPath(_testDirectory);

        var config = registry.ResolvePackage("nonexistent");

        Assert.That(config, Is.Null);
    }

    [Test]
    public void ResolvePackage_SearchesAllPaths()
    {
        // Create two search paths
        var searchPath1 = Path.Combine(_testDirectory, "path1");
        var searchPath2 = Path.Combine(_testDirectory, "path2");
        Directory.CreateDirectory(searchPath1);
        Directory.CreateDirectory(searchPath2);

        // Create a package in the second path only
        var packageDir = Path.Combine(searchPath2, "hidden-package");
        Directory.CreateDirectory(packageDir);
        File.WriteAllText(Path.Combine(packageDir, PackageConfigLoader.PackageFileName), """
            [package]
            version = "1.0.0"
            """);

        var log = new ErrorLog();
        var registry = new PackageRegistry(log);
        registry.AddSearchPath(searchPath1);
        registry.AddSearchPath(searchPath2);

        var config = registry.ResolvePackage("hidden-package");

        Assert.That(config, Is.Not.Null);
    }

    [Test]
    public void ResolvePackage_CachesResult()
    {
        CreateTestPackage("cached-pkg");
        
        var log = new ErrorLog();
        var registry = new PackageRegistry(log);
        registry.AddSearchPath(_testDirectory);

        // First call
        var config1 = registry.ResolvePackage("cached-pkg");
        // Second call should return cached result
        var config2 = registry.ResolvePackage("cached-pkg");

        Assert.That(config1, Is.SameAs(config2));
    }

    [Test]
    public void ResolveRelativePackage_FromCurrentDirectory()
    {
        // Create a package in a subdirectory
        var basePath = Path.Combine(_testDirectory, "project");
        Directory.CreateDirectory(basePath);
        
        var localPackage = Path.Combine(_testDirectory, "local");
        Directory.CreateDirectory(localPackage);
        File.WriteAllText(Path.Combine(localPackage, PackageConfigLoader.PackageFileName), """
            [package]
            version = "1.0.0"
            """);

        var log = new ErrorLog();
        var registry = new PackageRegistry(log);

        // Resolve ./local from project directory (depth 0 = ./)
        var config = registry.ResolveRelativePackage(basePath, 0, "local");

        Assert.That(config, Is.Not.Null);
        Assert.That(config!.Name, Is.EqualTo("local"));
    }

    [Test]
    public void ResolveRelativePackage_FromParentDirectory()
    {
        // Create a package in the test directory
        CreateTestPackage("shared");
        
        // Create a subdirectory for the "current" file location
        var subDir = Path.Combine(_testDirectory, "subproject", "module");
        Directory.CreateDirectory(subDir);

        var log = new ErrorLog();
        var registry = new PackageRegistry(log);

        // Resolve ../shared from subproject/module (depth 1 = ../)
        var config = registry.ResolveRelativePackage(subDir, 1, "shared");

        // Should find it in _testDirectory (parent of subproject)
        // Actually parent of subDir is "subproject", parent of that is _testDirectory
        // So depth 1 from subDir goes to subproject, depth 2 goes to _testDirectory
        // Let me fix this - depth should mean "how many levels up from parent"
        // ./  = current dir = depth 0
        // ../ = parent dir  = depth 1
        // ../../ = grandparent = depth 2
        
        Assert.That(config, Is.Not.Null);
        Assert.That(config!.Name, Is.EqualTo("shared"));
    }
}
