namespace Wayfarer.GameState.Extensions;

/// <summary>
/// Extension methods for string manipulation
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Repeat a character n times
    /// </summary>
    public static string Repeat(this char c, int count)
    {
        if (count <= 0) return string.Empty;
        return new string(c, count);
    }
}