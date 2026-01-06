using Sunset.Parser.Errors;
using Sunset.Parser.Packages;

namespace Sunset.Parser.Test.Packages;

[TestFixture]
public class PackageConfigLoaderTests
{
    private string _testDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "sunset-test-" + Guid.NewGuid().ToString("N"));
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

    private string CreateTestPackage(string packageName, string tomlContent)
    {
        var packageDir = Path.Combine(_testDirectory, packageName);
        Directory.CreateDirectory(packageDir);
        File.WriteAllText(Path.Combine(packageDir, PackageConfigLoader.PackageFileName), tomlContent);
        return packageDir;
    }

    [Test]
    public void IsPackageDirectory_WithValidPackage_ReturnsTrue()
    {
        var packageDir = CreateTestPackage("mypackage", """
            [package]
            version = "1.0.0"
            """);

        Assert.That(PackageConfigLoader.IsPackageDirectory(packageDir), Is.True);
    }

    [Test]
    public void IsPackageDirectory_WithoutToml_ReturnsFalse()
    {
        var packageDir = Path.Combine(_testDirectory, "nopackage");
        Directory.CreateDirectory(packageDir);

        Assert.That(PackageConfigLoader.IsPackageDirectory(packageDir), Is.False);
    }

    [Test]
    public void IsPackageDirectory_WithNonExistentDirectory_ReturnsFalse()
    {
        var nonExistentDir = Path.Combine(_testDirectory, "nonexistent");

        Assert.That(PackageConfigLoader.IsPackageDirectory(nonExistentDir), Is.False);
    }

    [Test]
    public void LoadFromDirectory_WithValidToml_ReturnsConfig()
    {
        var packageDir = CreateTestPackage("testpkg", """
            [package]
            version = "2.1.0"
            description = "A test package"
            """);

        var log = new ErrorLog();
        var config = PackageConfigLoader.LoadFromDirectory(packageDir, log);

        Assert.That(config, Is.Not.Null);
        Assert.That(config!.Name, Is.EqualTo("testpkg"));
        Assert.That(config.Version, Is.EqualTo("2.1.0"));
        Assert.That(config.Description, Is.EqualTo("A test package"));
        Assert.That(config.RootPath, Is.EqualTo(packageDir));
    }

    [Test]
    public void LoadFromDirectory_WithMinimalToml_ReturnsConfigWithDefaults()
    {
        var packageDir = CreateTestPackage("minimal", """
            [package]
            version = "0.1.0"
            """);

        var log = new ErrorLog();
        var config = PackageConfigLoader.LoadFromDirectory(packageDir, log);

        Assert.That(config, Is.Not.Null);
        Assert.That(config!.Name, Is.EqualTo("minimal"));
        Assert.That(config.Version, Is.EqualTo("0.1.0"));
        Assert.That(config.Description, Is.Null);
    }

    [Test]
    public void LoadFromDirectory_WithoutToml_ReturnsNull()
    {
        var packageDir = Path.Combine(_testDirectory, "noconfig");
        Directory.CreateDirectory(packageDir);

        var log = new ErrorLog();
        var config = PackageConfigLoader.LoadFromDirectory(packageDir, log);

        Assert.That(config, Is.Null);
    }

    [Test]
    public void LoadFromDirectory_WithInvalidToml_ReturnsNullAndLogsError()
    {
        var packageDir = CreateTestPackage("invalid", """
            this is not valid toml {{{
            """);

        var log = new ErrorLog();
        var config = PackageConfigLoader.LoadFromDirectory(packageDir, log);

        Assert.That(config, Is.Null);
        Assert.That(log.ErrorMessages.Any(), Is.True);
    }

    [Test]
    public void LoadFromDirectory_NameIsFolderName_NotFromToml()
    {
        // Even if TOML had a name field (which it doesn't in our schema),
        // the package name should always be the folder name
        var packageDir = CreateTestPackage("folder-name", """
            [package]
            version = "1.0.0"
            """);

        var log = new ErrorLog();
        var config = PackageConfigLoader.LoadFromDirectory(packageDir, log);

        Assert.That(config, Is.Not.Null);
        Assert.That(config!.Name, Is.EqualTo("folder-name"));
    }
}
