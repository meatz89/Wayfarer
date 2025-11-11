using System.Text.Json;

/// <summary>
/// Pipeline for validating content at build time.
/// Ensures all JSON content meets schema requirements before runtime.
/// </summary>
public class ContentValidationPipeline
{
    private readonly List<IContentValidator> _validators;
    private readonly List<ValidationError> _errors;

    public ContentValidationPipeline()
    {
        _validators = new List<IContentValidator>();
        _errors = new List<ValidationError>();
    }

    /// <summary>
    /// Add a validator to the pipeline.
    /// </summary>
    public ContentValidationPipeline AddValidator(IContentValidator validator)
    {
        _validators.Add(validator);
        return this;
    }

    /// <summary>
    /// Validate all content files in the specified directory.
    /// </summary>
    public ValidationResult ValidateContentDirectory(string contentPath)
    {
        _errors.Clear();

        if (!Directory.Exists(contentPath))
        {
            _errors.Add(new ValidationError(
                "ContentDirectory",
                $"Content directory not found: {contentPath}",
                ValidationSeverity.Critical));
            return new ValidationResult(_errors);
        }

        // Validate all JSON files
        string[] jsonFiles = Directory.GetFiles(contentPath, "*.json", SearchOption.AllDirectories);

        foreach (string file in jsonFiles)
        {
            ValidateFile(file);
        }

        return new ValidationResult(_errors);
    }

    /// <summary>
    /// Validate a single content file.
    /// </summary>
    public ValidationResult ValidateFile(string filePath)
    {
        List<ValidationError> localErrors = new List<ValidationError>();

        string content = File.ReadAllText(filePath);
        string fileName = Path.GetFileName(filePath);

        // Validate JSON syntax - let JsonException propagate
        using (JsonDocument doc = JsonDocument.Parse(content))
        {
        }

        // Run all validators
        foreach (IContentValidator validator in _validators)
        {
            if (validator.CanValidate(fileName))
            {
                IEnumerable<ValidationError> validationErrors = validator.Validate(content, fileName);
                localErrors.AddRange(validationErrors);
            }
        }

        _errors.AddRange(localErrors);
        return new ValidationResult(localErrors);
    }

    /// <summary>
    /// Get all validation errors from the last run.
    /// </summary>
    public IReadOnlyList<ValidationError> GetErrors()
    {
        return _errors.AsReadOnly();
    }

    /// <summary>
    /// Clear all validation errors.
    /// </summary>
    public void ClearErrors()
    {
        _errors.Clear();
    }
}

/// <summary>
/// Result of content validation.
/// </summary>
public class ValidationResult
{
    public IReadOnlyList<ValidationError> Errors { get; }
    public bool IsValid => !Errors.Any(e => e.Severity == ValidationSeverity.Critical);

    public int ErrorCount => Errors.Count;

    public int WarningCount => Errors.Count(e => e.Severity == ValidationSeverity.Warning);

    public int CriticalCount => Errors.Count(e => e.Severity == ValidationSeverity.Critical);

    public ValidationResult()
    {

    }

    public ValidationResult(IEnumerable<ValidationError> errors)
    {
        Errors = errors.ToList().AsReadOnly();
    }

    // Static factory methods for TimeModel compatibility
    public static ValidationResult Success()
    {
        return new ValidationResult(new List<ValidationError>());
    }

    public static ValidationResult Failure(string message)
    {
        return new ValidationResult(new List<ValidationError>
    {
        new ValidationError("TimeModel", message, ValidationSeverity.Critical)
    });
    }

    public static ValidationResult Warning(string message)
    {
        return new ValidationResult(new List<ValidationError>
    {
        new ValidationError("TimeModel", message, ValidationSeverity.Warning)
    });
    }

    public void ThrowIfInvalid()
    {
        if (!IsValid)
        {
            IEnumerable<ValidationError> criticalErrors = Errors.Where(e => e.Severity == ValidationSeverity.Critical);
            throw new ContentValidationException(
                $"Content validation failed with {CriticalCount} critical errors",
                criticalErrors);
        }
    }
}

/// <summary>
/// Represents a validation error.
/// </summary>
public class ValidationError
{
    public string Source { get; }
    public string Message { get; }
    public ValidationSeverity Severity { get; }

    public ValidationError(string source, string message, ValidationSeverity severity)
    {
        Source = source;
        Message = message;
        Severity = severity;
    }

    public override string ToString()
    {
        return $"[{Severity}] {Source}: {Message}";
    }
}

/// <summary>
/// Severity levels for validation errors.
/// </summary>
public enum ValidationSeverity
{
    Info,
    Warning,
    Critical
}

/// <summary>
/// Exception thrown when content validation fails.
/// </summary>
public class ContentValidationException : Exception
{
    public IEnumerable<ValidationError> Errors { get; }

    public ContentValidationException(string message, IEnumerable<ValidationError> errors)
        : base(message)
    {
        Errors = errors;
    }
}