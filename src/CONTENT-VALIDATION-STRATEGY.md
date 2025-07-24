# Content Validation Strategy for Wayfarer

## Overview

This document describes the content validation approach used in Wayfarer, including lessons learned from implementation.

## Key Design Principles

### 1. Case-Insensitive Property Matching

**Problem**: JSON files use camelCase (e.g., `baseCoinCost`) while C# DTOs use PascalCase (e.g., `BaseCoinCost`).

**Solution**: All validators inherit from `BaseValidator` which provides case-insensitive property matching:

```csharp
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
```

### 2. Validation Severity Levels

- **Critical**: Game-breaking errors that prevent proper initialization
- **Warning**: Issues that may cause gameplay problems but don't prevent startup
- **Info**: Suggestions for content improvement

### 3. Validator Organization

```
Content/Validation/
├── Validators/
│   ├── BaseValidator.cs           # Common helper methods
│   ├── RouteValidator.cs          # Validates routes.json
│   ├── NPCValidator.cs            # Validates npcs.json
│   ├── LocationValidator.cs       # Validates locations.json
│   ├── ItemValidator.cs           # Validates items.json
│   └── LetterTemplateValidator.cs # Validates letter_templates.json
├── ContentValidationPipeline.cs   # Orchestrates validation
└── IContentValidator.cs           # Interface for validators
```

## Common Validation Patterns

### 1. Required Field Validation

```csharp
private readonly HashSet<string> _requiredFields = new HashSet<string>
{
    "id", "name", "origin", "destination"
};

foreach (string field in _requiredFields)
{
    if (!TryGetPropertyCaseInsensitive(element, field, out _))
    {
        errors.Add(new ValidationError(
            $"{fileName}:{itemId}",
            $"Missing required field: {field}",
            ValidationSeverity.Critical));
    }
}
```

### 2. Enum Validation

```csharp
if (TryGetPropertyCaseInsensitive(element, "method", out JsonElement method) &&
    method.ValueKind == JsonValueKind.String)
{
    string? methodStr = method.GetString();
    if (!string.IsNullOrEmpty(methodStr) &&
        !EnumParser.TryParse<TravelMethods>(methodStr, out _))
    {
        errors.Add(new ValidationError(
            $"{fileName}:{itemId}",
            $"Invalid travel method: '{methodStr}'",
            ValidationSeverity.Critical));
    }
}
```

### 3. Numeric Range Validation

```csharp
ValidateNumericField(element, "travelTimeHours", itemId, fileName, errors, min: 1);
ValidateNumericField(element, "baseCoinCost", itemId, fileName, errors, min: 0);
```

## Lessons Learned

### 1. JSON Serialization Mismatches

**Issue**: Default System.Text.Json serialization uses camelCase for JSON but PascalCase for C# properties.

**Solutions**:
1. Use case-insensitive property matching in validators
2. Configure JsonSerializerOptions with PropertyNamingPolicy.CamelCase
3. Use [JsonPropertyName] attributes on DTOs

### 2. Validation Performance

**Issue**: Case-insensitive property matching can be slow for large JSON files.

**Solution**: Always try exact match first before falling back to case-insensitive search.

### 3. Error Reporting

**Issue**: Validation errors need to be actionable for content creators.

**Best Practices**:
- Include file name and item ID in error messages
- Provide specific field names that are missing or invalid
- Suggest valid values for enums
- Group errors by severity

### 4. Content Loading vs Validation

**Key Insight**: Separate validation from loading. The content pipeline:
1. Validates JSON structure and required fields
2. Loads content with ValidatedContentLoader
3. Creates dummy entities for missing references (Phase 6)
4. Logs all issues for E2E test detection

## Integration with E2E Testing

The E2E test detects validation errors through:
1. Console output parsing for warning counts
2. Detection of `content_validation_errors.log` file
3. Verification that dummy entities were created

This ensures content issues are caught during CI/CD while still allowing the game to run locally for development.

## Future Improvements

1. **Schema Generation**: Auto-generate JSON schemas from DTOs
2. **Content Editor**: Build a tool that validates content as it's edited
3. **Validation Caching**: Cache validation results based on file timestamps
4. **Custom Validation Rules**: Allow game designers to add business logic validations