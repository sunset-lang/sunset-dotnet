namespace Northrop.Common.TestHelpers;

public static class TestHelpers
{
    public static string NormalizeString(string input) => input.Replace("\r\n", "\n").Trim();
}