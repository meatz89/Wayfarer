using System;
using System.IO;
using Wayfarer.Content.Validation.Validators;

namespace Wayfarer.Content.Validation
{
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
                var pipeline = new ContentValidationPipeline()
                    .AddValidator(new ItemValidator())
                    .AddValidator(new NPCValidator())
                    .AddValidator(new RouteValidator());
                
                // Run validation
                var result = pipeline.ValidateContentDirectory(contentPath);
                
                // Display results
                Console.WriteLine($"\nValidation Results:");
                Console.WriteLine($"  Total Errors: {result.ErrorCount}");
                Console.WriteLine($"  Critical: {result.CriticalCount}");
                Console.WriteLine($"  Warnings: {result.WarningCount}");
                
                if (result.ErrorCount > 0)
                {
                    Console.WriteLine("\nErrors found:");
                    foreach (var error in result.Errors)
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
                
                var pipeline = new ContentValidationPipeline()
                    .AddValidator(new ItemValidator())
                    .AddValidator(new NPCValidator())
                    .AddValidator(new RouteValidator());
                
                var result = pipeline.ValidateFile(filePath);
                
                if (!result.IsValid)
                {
                    Console.WriteLine("\nErrors found:");
                    foreach (var error in result.Errors)
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
}