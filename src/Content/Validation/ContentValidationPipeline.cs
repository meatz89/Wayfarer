using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Wayfarer.Content.Validation
{
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
            var jsonFiles = Directory.GetFiles(contentPath, "*.json", SearchOption.AllDirectories);
            
            foreach (var file in jsonFiles)
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
            var localErrors = new List<ValidationError>();
            
            try
            {
                var content = File.ReadAllText(filePath);
                var fileName = Path.GetFileName(filePath);
                
                // First check if it's valid JSON
                try
                {
                    using var doc = JsonDocument.Parse(content);
                }
                catch (JsonException ex)
                {
                    localErrors.Add(new ValidationError(
                        fileName,
                        $"Invalid JSON: {ex.Message}",
                        ValidationSeverity.Critical));
                    _errors.AddRange(localErrors);
                    return new ValidationResult(localErrors);
                }
                
                // Run all validators
                foreach (var validator in _validators)
                {
                    if (validator.CanValidate(fileName))
                    {
                        var validationErrors = validator.Validate(content, fileName);
                        localErrors.AddRange(validationErrors);
                    }
                }
            }
            catch (Exception ex)
            {
                localErrors.Add(new ValidationError(
                    filePath,
                    $"Failed to validate file: {ex.Message}",
                    ValidationSeverity.Critical));
            }
            
            _errors.AddRange(localErrors);
            return new ValidationResult(localErrors);
        }
        
        /// <summary>
        /// Get all validation errors from the last run.
        /// </summary>
        public IReadOnlyList<ValidationError> GetErrors() => _errors.AsReadOnly();
        
        /// <summary>
        /// Clear all validation errors.
        /// </summary>
        public void ClearErrors() => _errors.Clear();
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
        
        public ValidationResult(IEnumerable<ValidationError> errors)
        {
            Errors = errors.ToList().AsReadOnly();
        }
        
        public void ThrowIfInvalid()
        {
            if (!IsValid)
            {
                var criticalErrors = Errors.Where(e => e.Severity == ValidationSeverity.Critical);
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
        
        public override string ToString() => $"[{Severity}] {Source}: {Message}";
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
}