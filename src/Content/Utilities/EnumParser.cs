using System;
using System.Collections.Generic;

/// <summary>
/// Centralized enum parsing utility with consistent error handling and normalization.
/// Eliminates manual enum parsing throughout the codebase.
/// </summary>
public static class EnumParser
{
    /// <summary>
    /// Parse a string value to the specified enum type with normalization.
    /// </summary>
    /// <typeparam name="TEnum">The enum type to parse to</typeparam>
    /// <param name="value">The string value to parse</param>
    /// <param name="result">The parsed enum value</param>
    /// <param name="ignoreCase">Whether to ignore case (default: true)</param>
    /// <param name="normalizeSpaces">Whether to replace spaces with underscores (default: true)</param>
    /// <returns>True if parsing succeeded, false otherwise</returns>
    public static bool TryParse<TEnum>(string value, out TEnum result, bool ignoreCase = true, bool normalizeSpaces = true)
        where TEnum : struct, Enum
    {
        result = default;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        string normalizedValue = value.Trim();

        if (normalizeSpaces)
        {
            normalizedValue = normalizedValue.Replace(" ", "_");
        }

        return Enum.TryParse<TEnum>(normalizedValue, ignoreCase, out result);
    }

    /// <summary>
    /// Parse a string value to the specified enum type with validation.
    /// Throws descriptive exception on failure.
    /// </summary>
    /// <typeparam name="TEnum">The enum type to parse to</typeparam>
    /// <param name="value">The string value to parse</param>
    /// <param name="fieldName">Name of the field being parsed (for error messages)</param>
    /// <param name="ignoreCase">Whether to ignore case (default: true)</param>
    /// <param name="normalizeSpaces">Whether to replace spaces with underscores (default: true)</param>
    /// <returns>The parsed enum value</returns>
    public static TEnum Parse<TEnum>(string value, string fieldName, bool ignoreCase = true, bool normalizeSpaces = true)
        where TEnum : struct, Enum
    {
        if (TryParse<TEnum>(value, out TEnum result, ignoreCase, normalizeSpaces))
        {
            return result;
        }

        string[] validValues = Enum.GetNames(typeof(TEnum));
        throw new ArgumentException(
            $"Invalid value '{value}' for {fieldName}. " +
            $"Valid values are: {string.Join(", ", validValues)}");
    }

    /// <summary>
    /// Parse a list of string values to enum values.
    /// </summary>
    /// <typeparam name="TEnum">The enum type to parse to</typeparam>
    /// <param name="values">The string values to parse</param>
    /// <param name="fieldName">Name of the field being parsed (for error messages)</param>
    /// <param name="ignoreCase">Whether to ignore case (default: true)</param>
    /// <param name="normalizeSpaces">Whether to replace spaces with underscores (default: true)</param>
    /// <returns>List of successfully parsed enum values</returns>
    public static List<TEnum> ParseList<TEnum>(IEnumerable<string> values, string fieldName, bool ignoreCase = true, bool normalizeSpaces = true)
        where TEnum : struct, Enum
    {
        List<TEnum> results = new List<TEnum>();
        List<string> errors = new List<string>();

        foreach (string value in values)
        {
            if (TryParse<TEnum>(value, out TEnum result, ignoreCase, normalizeSpaces))
            {
                results.Add(result);
            }
            else if (!string.IsNullOrWhiteSpace(value))
            {
                errors.Add(value);
            }
        }

        if (errors.Count > 0)
        {
            string[] validValues = Enum.GetNames(typeof(TEnum));
            throw new ArgumentException(
                $"Invalid values for {fieldName}: {string.Join(", ", errors)}. " +
                $"Valid values are: {string.Join(", ", validValues)}");
        }

        return results;
    }

    /// <summary>
    /// Try to parse a list of string values to enum values.
    /// Returns only successfully parsed values without throwing exceptions.
    /// </summary>
    /// <typeparam name="TEnum">The enum type to parse to</typeparam>
    /// <param name="values">The string values to parse</param>
    /// <param name="ignoreCase">Whether to ignore case (default: true)</param>
    /// <param name="normalizeSpaces">Whether to replace spaces with underscores (default: true)</param>
    /// <returns>List of successfully parsed enum values</returns>
    public static List<TEnum> TryParseList<TEnum>(IEnumerable<string> values, bool ignoreCase = true, bool normalizeSpaces = true)
        where TEnum : struct, Enum
    {
        List<TEnum> results = new List<TEnum>();

        foreach (string value in values)
        {
            if (TryParse<TEnum>(value, out TEnum result, ignoreCase, normalizeSpaces))
            {
                results.Add(result);
            }
        }

        return results;
    }
}