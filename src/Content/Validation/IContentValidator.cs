/// <summary>
/// Interface for content validators that can be added to the validation pipeline.
/// </summary>
public interface IContentValidator
{
/// <summary>
/// Determines if this validator can handle the given file.
/// </summary>
/// <param name="fileName">Name of the file to validate</param>
/// <returns>True if this validator should process the file</returns>
bool CanValidate(string fileName);

/// <summary>
/// Validate the content and return any errors found.
/// </summary>
/// <param name="content">The file content to validate</param>
/// <param name="fileName">Name of the file being validated</param>
/// <returns>List of validation errors (empty if valid)</returns>
IEnumerable<ValidationError> Validate(string content, string fileName);
}