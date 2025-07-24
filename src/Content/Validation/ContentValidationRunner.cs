using System;
using System.IO;

/// <summary>
/// Runner for content validation during build process.
/// Can be integrated into MSBuild or run as a pre-build step.
/// </summary>
public class ContentValidationRunner
{
    /// <summary>
    /// Run content validation and return exit code (0 = success, 1 = failure).
    /// </summary>
    public static int Run(string contentPath = null)
    {
        try
        {
            // Use provided path or default to Content/Templates
            if (string.IsNullOrEmpty(contentPath))
            {
                contentPath = Path.Combine(Directory.GetCurrentDirectory(), "Content", "Templates");
            }

            Console.WriteLine($"Running content validation on: {contentPath}");
            Console.WriteLine(new string('-', 60));

            // Create pipeline with all validators
            ContentValidationPipeline pipeline = new ContentValidationPipeline()
                .AddValidator(new ItemValidator())
                .AddValidator(new NPCValidator())
                .AddValidator(new RouteValidator())
                .AddValidator(new LocationValidator())
                .AddValidator(new LocationSpotValidator())
                .AddValidator(new LetterTemplateValidator())
                .AddValidator(new StandingObligationValidator())
                .AddValidator(new TokenFavorValidator())
                .AddValidator(new NarrativeValidator())
                .AddValidator(new RouteDiscoveryValidator())
                .AddValidator(new ProgressionUnlockValidator());

            // Run validation
            ValidationResult result = pipeline.ValidateContentDirectory(contentPath);

            // Display results
            Console.WriteLine($"\nValidation Results:");
            Console.WriteLine($"  Total Errors: {result.ErrorCount}");
            Console.WriteLine($"  Critical: {result.CriticalCount}");
            Console.WriteLine($"  Warnings: {result.WarningCount}");

            if (result.ErrorCount > 0)
            {
                Console.WriteLine("\nErrors found:");
                foreach (ValidationError error in result.Errors)
                {
                    Console.WriteLine($"  {error}");
                }
            }

            if (!result.IsValid)
            {
                Console.WriteLine("\n❌ Content validation FAILED");
                return 1;
            }

            Console.WriteLine("\n✅ Content validation PASSED");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Validation runner failed: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Validate a single file and display results.
    /// </summary>
    public static int ValidateFile(string filePath)
    {
        try
        {
            Console.WriteLine($"Validating file: {filePath}");
            Console.WriteLine(new string('-', 60));

            ContentValidationPipeline pipeline = new ContentValidationPipeline()
                .AddValidator(new ItemValidator())
                .AddValidator(new NPCValidator())
                .AddValidator(new RouteValidator())
                .AddValidator(new LocationValidator())
                .AddValidator(new LocationSpotValidator())
                .AddValidator(new LetterTemplateValidator())
                .AddValidator(new StandingObligationValidator())
                .AddValidator(new TokenFavorValidator())
                .AddValidator(new NarrativeValidator())
                .AddValidator(new RouteDiscoveryValidator())
                .AddValidator(new ProgressionUnlockValidator());

            ValidationResult result = pipeline.ValidateFile(filePath);

            if (!result.IsValid)
            {
                Console.WriteLine("\nErrors found:");
                foreach (ValidationError error in result.Errors)
                {
                    Console.WriteLine($"  {error}");
                }
                return 1;
            }

            Console.WriteLine("✅ File is valid");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Validation failed: {ex.Message}");
            return 1;
        }
    }
}