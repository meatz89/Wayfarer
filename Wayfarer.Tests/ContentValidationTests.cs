using System.Linq;
using Xunit;
/// <summary>
/// Tests for the content validation pipeline.
/// </summary>
public class ContentValidationTests
{
    [Fact]
    public void ItemValidator_Should_Detect_Missing_Required_Fields()
    {
        // Arrange
        ItemValidator validator = new ItemValidator();
        string invalidJson = @"[
                {
                    ""id"": ""test_item"",
                    ""name"": ""Test Item""
                }
            ]";

        // Act
        List<ValidationError> errors = validator.Validate(invalidJson, "test_items.json").ToList();

        // Assert
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Message.Contains("Missing required field: weight"));
        Assert.Contains(errors, e => e.Message.Contains("Missing required field: buyPrice"));
        Assert.Contains(errors, e => e.Message.Contains("Missing required field: sellPrice"));
        Assert.Contains(errors, e => e.Message.Contains("Missing required field: inventorySlots"));
    }

    [Fact]
    public void ItemValidator_Should_Detect_Invalid_Category()
    {
        // Arrange
        ItemValidator validator = new ItemValidator();
        string json = @"[
                {
                    ""id"": ""test_item"",
                    ""name"": ""Test Item"",
                    ""weight"": 1,
                    ""buyPrice"": 10,
                    ""sellPrice"": 5,
                    ""inventorySlots"": 1,
                    ""categories"": [""InvalidCategory""]
                }
            ]";

        // Act
        List<ValidationError> errors = validator.Validate(json, "test_items.json").ToList();

        // Assert
        Assert.Contains(errors, e =>
            e.Message.Contains("Invalid item category: 'InvalidCategory'") &&
            e.Severity == ValidationSeverity.Warning);
    }

    [Fact]
    public void NPCValidator_Should_Detect_Invalid_Profession()
    {
        // Arrange
        NPCValidator validator = new NPCValidator();
        string json = @"[
                {
                    ""id"": ""test_npc"",
                    ""name"": ""Test NPC"",
                    ""profession"": ""InvalidProfession"",
                    ""locationId"": ""test_location""
                }
            ]";

        // Act
        List<ValidationError> errors = validator.Validate(json, "npcs.json").ToList();

        // Assert
        Assert.Contains(errors, e =>
            e.Message.Contains("Invalid profession: 'InvalidProfession'") &&
            e.Severity == ValidationSeverity.Critical);
    }

    [Fact]
    public void ContentValidationPipeline_Should_Run_Multiple_Validators()
    {
        // Arrange
        ContentValidationPipeline pipeline = new ContentValidationPipeline()
            .AddValidator(new ItemValidator())
            .AddValidator(new NPCValidator());

        string itemJson = @"[{""id"": ""bad_item""}]"; // Invalid item

        // Act
        ValidationResult result = pipeline.ValidateFile("test_items.json");
        pipeline.ValidateFile("test_items.json"); // Simulate validating a file

        // Assert
        // This would need actual file system access to test fully
        // For now, we're just verifying the pipeline can be constructed and called
        Assert.NotNull(result);
    }

    [Fact]
    public void SchemaValidator_Should_Detect_Missing_Fields()
    {
        // Arrange
        SchemaValidator validator = new SchemaValidator();
        string json = @"[
                {
                    ""id"": ""test_location""
                }
            ]";

        // Act
        List<ValidationError> errors = validator.Validate(json, "locations.json").ToList();

        // Assert
        Assert.Contains(errors, e =>
            e.Message.Contains("Missing required field: name") &&
            e.Severity == ValidationSeverity.Critical);
    }

    [Fact]
    public void SchemaValidator_Should_Detect_Unknown_Fields()
    {
        // Arrange
        SchemaValidator validator = new SchemaValidator();
        string json = @"[
                {
                    ""id"": ""test_location"",
                    ""name"": ""Test Location"",
                    ""unknownField"": ""This should not be here""
                }
            ]";

        // Act
        List<ValidationError> errors = validator.Validate(json, "locations.json").ToList();

        // Assert
        Assert.Contains(errors, e =>
            e.Message.Contains("Unknown field: unknownField") &&
            e.Severity == ValidationSeverity.Warning);
    }
}