using System;
using System.Text.Json;

/// <summary>
/// Base class for content validators providing common helper methods.
/// </summary>
public abstract class BaseValidator : IContentValidator
{
    public abstract bool CanValidate(string fileName);
    public abstract IEnumerable<ValidationError> Validate(string content, string fileName);

    /// <summary>
    /// Tries to get a property from a JsonElement using case-insensitive comparison.
    /// </summary>
    protected bool TryGetPropertyCaseInsensitive(JsonElement element, string propertyName, out JsonElement value)
    {
        // First try exact match for performance
        if (element.TryGetProperty(propertyName, out value))
            return true;
            
        // Try case-insensitive search
        foreach (JsonProperty prop in element.EnumerateObject())
        {
            if (string.Equals(prop.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                value = prop.Value;
                return true;
            }
        }
        
        value = default;
        return false;
    }

    /// <summary>
    /// Gets a string property value using case-insensitive comparison.
    /// </summary>
    protected string GetStringProperty(JsonElement element, string propertyName)
    {
        if (TryGetPropertyCaseInsensitive(element, propertyName, out JsonElement property) &&
            property.ValueKind == JsonValueKind.String)
        {
            return property.GetString();
        }
        return null;
    }

    /// <summary>
    /// Validates a numeric field with optional min/max constraints.
    /// </summary>
    protected void ValidateNumericField(JsonElement item, string fieldName, string itemId,
        string fileName, List<ValidationError> errors, int? min = null, int? max = null)
    {
        if (TryGetPropertyCaseInsensitive(item, fieldName, out JsonElement field))
        {
            if (field.ValueKind != JsonValueKind.Number || !field.TryGetInt32(out int value))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{itemId}",
                    $"Field '{fieldName}' must be a valid integer",
                    ValidationSeverity.Critical));
            }
            else
            {
                if (min.HasValue && value < min.Value)
                {
                    errors.Add(new ValidationError(
                        $"{fileName}:{itemId}",
                        $"Field '{fieldName}' value {value} is below minimum {min.Value}",
                        ValidationSeverity.Critical));
                }
                if (max.HasValue && value > max.Value)
                {
                    errors.Add(new ValidationError(
                        $"{fileName}:{itemId}",
                        $"Field '{fieldName}' value {value} is above maximum {max.Value}",
                        ValidationSeverity.Critical));
                }
            }
        }
    }
}