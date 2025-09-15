using System;
using System.Collections.Generic;

/// <summary>
/// Exception thrown when package loading encounters missing dependencies or validation errors
/// </summary>
public class PackageLoadException : Exception
{
    /// <summary>
    /// The package that failed to load
    /// </summary>
    public string PackageId { get; set; }

    /// <summary>
    /// The type of entity that has missing dependencies (e.g., "NPCRequest", "Route", "Exchange")
    /// </summary>
    public string EntityType { get; set; }

    /// <summary>
    /// The specific missing dependency (e.g., card ID, NPC ID, location ID)
    /// </summary>
    public string MissingDependency { get; set; }

    /// <summary>
    /// The entity that was trying to reference the missing dependency
    /// </summary>
    public string ReferencingEntity { get; set; }

    /// <summary>
    /// Collection of all validation errors found
    /// </summary>
    public List<string> ValidationErrors { get; set; } = new List<string>();

    /// <summary>
    /// Suggestions for fixing the error
    /// </summary>
    public List<string> Suggestions { get; set; } = new List<string>();

    public PackageLoadException(string message) : base(message)
    {
    }

    public PackageLoadException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Create a detailed exception for a missing dependency
    /// </summary>
    public static PackageLoadException CreateMissingDependency(
        string packageId,
        string entityType,
        string referencingEntity,
        string missingDependency,
        string suggestion = null)
    {
        var exception = new PackageLoadException(
            $"[{packageId}] {entityType} '{referencingEntity}' references missing {missingDependency}")
        {
            PackageId = packageId,
            EntityType = entityType,
            ReferencingEntity = referencingEntity,
            MissingDependency = missingDependency
        };

        if (!string.IsNullOrEmpty(suggestion))
        {
            exception.Suggestions.Add(suggestion);
        }

        return exception;
    }

    /// <summary>
    /// Create an exception for multiple validation errors
    /// </summary>
    public static PackageLoadException MultipleValidationErrors(
        string packageId,
        List<string> errors)
    {
        var message = $"Package '{packageId}' failed validation with {errors.Count} error(s):\n" +
                     string.Join("\n  - ", errors);

        return new PackageLoadException(message)
        {
            PackageId = packageId,
            ValidationErrors = errors
        };
    }

    /// <summary>
    /// Get a formatted error report
    /// </summary>
    public string GetDetailedReport()
    {
        var report = new System.Text.StringBuilder();
        report.AppendLine("=== PACKAGE LOAD ERROR ===");

        if (!string.IsNullOrEmpty(PackageId))
            report.AppendLine($"Package: {PackageId}");

        if (!string.IsNullOrEmpty(EntityType))
            report.AppendLine($"Entity Type: {EntityType}");

        if (!string.IsNullOrEmpty(ReferencingEntity))
            report.AppendLine($"Referencing Entity: {ReferencingEntity}");

        if (!string.IsNullOrEmpty(MissingDependency))
            report.AppendLine($"Missing Dependency: {MissingDependency}");

        if (ValidationErrors.Count > 0)
        {
            report.AppendLine($"\nValidation Errors ({ValidationErrors.Count}):");
            foreach (var error in ValidationErrors)
            {
                report.AppendLine($"  - {error}");
            }
        }

        if (Suggestions.Count > 0)
        {
            report.AppendLine("\nSuggestions:");
            foreach (var suggestion in Suggestions)
            {
                report.AppendLine($"  → {suggestion}");
            }
        }

        report.AppendLine("========================");
        return report.ToString();
    }
}