using Sunset.Parser.Errors;
using Sunset.Parser.Packages;
using Sunset.Parser.Scopes;

namespace Sunset.Parser.Test.Packages;

[TestFixture]
public class PackageTests
{
    private string _testDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "sunset-package-test-" + Guid.NewGuid().ToString("N"));
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

    private Package CreateTestPackage(string packageName, Action<string> setupAction)
    {
        var packageDir = Path.Combine(_testDirectory, packageName);
        Directory.CreateDirectory(packageDir);
        
        // Create package config
        File.WriteAllText(Path.Combine(packageDir, PackageConfigLoader.PackageFileName), $"""
            [package]
            version = "1.0.0"
            description = "Test package {packageName}"
            """);

        // Run custom setup
        setupAction(packageDir);

        var log = new ErrorLog();
        var config = PackageConfigLoader.LoadFromDirectory(packageDir, log)!;
        return new Package(config, log);
    }

    [Test]
    public void Package_Initialize_LoadsRootFiles()
    {
        var package = CreateTestPackage("testpkg", dir =>
        {
            File.WriteAllText(Path.Combine(dir, "main.sun"), "x = 10");
            File.WriteAllText(Path.Combine(dir, "utils.sun"), "y = 20");
        });

        package.Initialize();

        Assert.That(package.RootFiles, Has.Count.EqualTo(2));
        Assert.That(package.RootFiles.ContainsKey("main"), Is.True);
        Assert.That(package.RootFiles.ContainsKey("utils"), Is.True);
    }

    [Test]
    public void Package_Initialize_LoadsModules()
    {
        var package = CreateTestPackage("testpkg", dir =>
        {
            var moduleDir = Path.Combine(dir, "geometry");
            Directory.CreateDirectory(moduleDir);
            File.WriteAllText(Path.Combine(moduleDir, "shapes.sun"), "z = 30");
        });

        package.Initialize();

        Assert.That(package.Modules, Has.Count.EqualTo(1));
        Assert.That(package.Modules.ContainsKey("geometry"), Is.True);
    }

    [Test]
    public void Package_TryGetDeclaration_FindsModule()
    {
        var package = CreateTestPackage("testpkg", dir =>
        {
            var moduleDir = Path.Combine(dir, "core");
            Directory.CreateDirectory(moduleDir);
        });

        var result = package.TryGetDeclaration("core");

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<Module>());
    }

    [Test]
    public void Package_TryGetDeclaration_FindsRootFile()
    {
        var package = CreateTestPackage("testpkg", dir =>
        {
            File.WriteAllText(Path.Combine(dir, "helpers.sun"), "x = 10");
        });

        var result = package.TryGetDeclaration("helpers");

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<FileScope>());
    }

    [Test]
    public void Package_GetChildScope_ReturnsModuleOrFile()
    {
        var package = CreateTestPackage("testpkg", dir =>
        {
            File.WriteAllText(Path.Combine(dir, "main.sun"), "x = 10");
            var moduleDir = Path.Combine(dir, "utils");
            Directory.CreateDirectory(moduleDir);
        });

        var fileScope = package.GetChildScope("main");
        var moduleScope = package.GetChildScope("utils");

        Assert.That(fileScope, Is.InstanceOf<FileScope>());
        Assert.That(moduleScope, Is.InstanceOf<Module>());
    }

    [Test]
    public void Package_SkipsNestedPackages()
    {
        var package = CreateTestPackage("testpkg", dir =>
        {
            var nestedDir = Path.Combine(dir, "nested");
            Directory.CreateDirectory(nestedDir);
            // Make it a package
            File.WriteAllText(Path.Combine(nestedDir, PackageConfigLoader.PackageFileName), """
                [package]
                version = "0.1.0"
                """);
        });

        package.Initialize();

        Assert.That(package.Modules, Has.Count.EqualTo(0));
    }
}

[TestFixture]
public class ModuleTests
{
    private string _testDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "sunset-module-test-" + Guid.NewGuid().ToString("N"));
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

    private (Package Package, Module Module) CreateTestModule(Action<string, string> setupAction)
    {
        var packageDir = Path.Combine(_testDirectory, "testpkg");
        var moduleDir = Path.Combine(packageDir, "mymodule");
        Directory.CreateDirectory(packageDir);
        Directory.CreateDirectory(moduleDir);

        // Create package config
        File.WriteAllText(Path.Combine(packageDir, PackageConfigLoader.PackageFileName), """
            [package]
            version = "1.0.0"
            """);

        // Run custom setup
        setupAction(packageDir, moduleDir);

        var log = new ErrorLog();
        var config = PackageConfigLoader.LoadFromDirectory(packageDir, log)!;
        var package = new Package(config, log);
        var module = new Module("mymodule", moduleDir, package, null, log)
        {
            ParentScope = package
        };

        return (package, module);
    }

    [Test]
    public void Module_Initialize_LoadsFiles()
    {
        var (_, module) = CreateTestModule((_, moduleDir) =>
        {
            File.WriteAllText(Path.Combine(moduleDir, "shapes.sun"), "x = 10");
            File.WriteAllText(Path.Combine(moduleDir, "utils.sun"), "y = 20");
        });

        module.Initialize();

        Assert.That(module.Files, Has.Count.EqualTo(2));
        Assert.That(module.Files.ContainsKey("shapes"), Is.True);
        Assert.That(module.Files.ContainsKey("utils"), Is.True);
    }

    [Test]
    public void Module_Initialize_LoadsSubmodules()
    {
        var (_, module) = CreateTestModule((_, moduleDir) =>
        {
            var subDir = Path.Combine(moduleDir, "advanced");
            Directory.CreateDirectory(subDir);
            File.WriteAllText(Path.Combine(subDir, "complex.sun"), "z = 30");
        });

        module.Initialize();

        Assert.That(module.Submodules, Has.Count.EqualTo(1));
        Assert.That(module.Submodules.ContainsKey("advanced"), Is.True);
    }

    [Test]
    public void Module_TryGetDeclaration_FindsSubmodule()
    {
        var (_, module) = CreateTestModule((_, moduleDir) =>
        {
            var subDir = Path.Combine(moduleDir, "core");
            Directory.CreateDirectory(subDir);
        });

        var result = module.TryGetDeclaration("core");

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<Module>());
    }

    [Test]
    public void Module_TryGetDeclaration_FindsFile()
    {
        var (_, module) = CreateTestModule((_, moduleDir) =>
        {
            File.WriteAllText(Path.Combine(moduleDir, "helpers.sun"), "x = 10");
        });

        var result = module.TryGetDeclaration("helpers");

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<FileScope>());
    }

    [Test]
    public void Module_FullPath_IncludesPackageName()
    {
        var (package, module) = CreateTestModule((_, _) => { });

        Assert.That(module.FullPath, Is.EqualTo("testpkg.mymodule"));
    }
}
