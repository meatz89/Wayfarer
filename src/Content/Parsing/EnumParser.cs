/// <summary>
/// Centralized enum parsing utility with consistent error handling and normalization.
/// Eliminates manual enum parsing throughout the codebase.
/// </summary>
public static class EnumParser
{
    /// <summary>
    /// Parse a string value to the specified enum type with normalization.
    /// HIGHLANDER: ONE way to parse enums - always case-insensitive, always normalize spaces.
    /// </summary>
    /// <typeparam name="TEnum">The enum type to parse to</typeparam>
    /// <param name="value">The string value to parse</param>
    /// <param name="result">The parsed enum value</param>
    /// <returns>True if parsing succeeded, false otherwise</returns>
    public static bool TryParse<TEnum>(string value, out TEnum result)
        where TEnum : struct, Enum
    {
        result = default;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        string normalizedValue = value.Trim().Replace(" ", "_");

        return Enum.TryParse<TEnum>(normalizedValue, ignoreCase: true, out result);
    }

    /// <summary>
    /// Parse a string value to the specified enum type with validation.
    /// Throws descriptive exception on failure.
    /// HIGHLANDER: ONE way to parse enums - always case-insensitive, always normalize spaces.
    /// </summary>
    /// <typeparam name="TEnum">The enum type to parse to</typeparam>
    /// <param name="value">The string value to parse</param>
    /// <param name="fieldName">Name of the field being parsed (for error messages)</param>
    /// <returns>The parsed enum value</returns>
    public static TEnum Parse<TEnum>(string value, string fieldName)
        where TEnum : struct, Enum
    {
        if (TryParse<TEnum>(value, out TEnum result))
        {
            return result;
        }

        string[] validValues = Enum.GetNames(typeof(TEnum));
        throw new ArgumentException(
            $"Invalid value '{value}' for {fieldName}. " +
            $"Valid values are: {string.Join(", ", validValues)}");
    }

}