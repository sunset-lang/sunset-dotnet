namespace Sunset.TestHelpers;

public static class TestHelpers
{
    public static string NormalizeString(string input)
    {
        return input.Replace("\r\n", "\n").Trim();
    }
}