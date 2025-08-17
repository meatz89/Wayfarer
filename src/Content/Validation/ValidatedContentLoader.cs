using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

/// <summary>
/// Loads and validates content through the validation pipeline.
/// Replaces direct JSON loading with validated loading.
/// </summary>
public class ValidatedContentLoader
{
    private readonly ContentValidationPipeline _pipeline;
    private readonly JsonSerializerOptions _jsonOptions;

    public ValidatedContentLoader()
    {
        _pipeline = new ContentValidationPipeline()
            .AddValidator(new ItemValidator())
            .AddValidator(new NPCValidator())
            .AddValidator(new RouteValidator())
            .AddValidator(new LocationValidator())
            .AddValidator(new LocationSpotValidator())
            .AddValidator(new LetterTemplateValidator())
            .AddValidator(new StandingObligationValidator())
            .AddValidator(new NarrativeValidator())
            .AddValidator(new RouteDiscoveryValidator())
            .AddValidator(new ProgressionUnlockValidator())
            .AddValidator(new GameConfigValidator());

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
    }

    /// <summary>
    /// Load and validate JSON content from a file.
    /// Throws ContentValidationException if validation fails.
    /// </summary>
    public T LoadValidatedContent<T>(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Content file not found: {filePath}");
        }

        // Read file content
        string content = File.ReadAllText(filePath);
        string fileName = Path.GetFileName(filePath);

        // Validate content
        ValidationResult result = _pipeline.ValidateFile(filePath);
        result.ThrowIfInvalid();

        // Deserialize if valid
        try
        {
            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }
        catch (JsonException ex)
        {
            throw new ContentValidationException(
                $"Failed to deserialize {fileName} after validation",
                new[] { new ValidationError(fileName, ex.Message, ValidationSeverity.Critical) });
        }
    }

    /// <summary>
    /// Load content with a custom deserializer for complex types.
    /// </summary>
    public T LoadValidatedContentWithParser<T>(string filePath, Func<string, T> parser)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Content file not found: {filePath}");
        }

        // Read and validate
        string content = File.ReadAllText(filePath);
        string fileName = Path.GetFileName(filePath);

        ValidationResult result = _pipeline.ValidateFile(filePath);
        result.ThrowIfInvalid();

        // Parse with custom parser
        try
        {
            return parser(content);
        }
        catch (Exception ex)
        {
            throw new ContentValidationException(
                $"Failed to parse {fileName} after validation",
                new[] { new ValidationError(fileName, ex.Message, ValidationSeverity.Critical) });
        }
    }
}