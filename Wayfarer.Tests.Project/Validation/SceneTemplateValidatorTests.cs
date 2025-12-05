using Xunit;

namespace Wayfarer.Tests.Project.Validation;

public class SceneTemplateValidatorTests
{
    private SceneTemplate CreateValidTemplate()
    {
        return new SceneTemplate
        {
            Id = "test_scene",
            SceneArchetype = SceneArchetypeType.InnLodging,
            SituationTemplates = new List<SituationTemplate>
            {
                new() { Id = "sit1", LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation } },
                new() { Id = "sit2", LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation } }
            },
            SpawnRules = new SituationSpawnRules
            {
                Pattern = SpawnPattern.Linear,
                InitialSituationTemplateId = "sit1"
                // HIGHLANDER: Flow control through Consequence.NextSituationTemplateId (arc42 §8.30)
            }
        };
    }

    [Fact]
    public void Validate_ValidTemplate_ReturnsSuccess()
    {
        var template = CreateValidTemplate();

        SceneValidationResult result = SceneTemplateValidator.Validate(template);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_MissingId_ReturnsError()
    {
        var template = new SceneTemplate
        {
            Id = null,
            SituationTemplates = CreateValidTemplate().SituationTemplates,
            SpawnRules = CreateValidTemplate().SpawnRules
        };

        SceneValidationResult result = SceneTemplateValidator.Validate(template);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Code == "STRUCT_001");
    }

    [Fact]
    public void Validate_NoSituations_ReturnsError()
    {
        var template = new SceneTemplate
        {
            Id = "test_scene",
            SituationTemplates = new List<SituationTemplate>(),
            SpawnRules = CreateValidTemplate().SpawnRules
        };

        SceneValidationResult result = SceneTemplateValidator.Validate(template);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Code == "STRUCT_002");
    }

    [Fact]
    public void Validate_SituationMissingId_ReturnsError()
    {
        var template = new SceneTemplate
        {
            Id = "test_scene",
            SituationTemplates = new List<SituationTemplate>
            {
                new() { Id = null },
                new() { Id = "sit2" }
            },
            SpawnRules = CreateValidTemplate().SpawnRules
        };

        SceneValidationResult result = SceneTemplateValidator.Validate(template);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Code == "STRUCT_003");
    }

    [Fact]
    public void Validate_TooManyChoices_ReturnsError()
    {
        var template = new SceneTemplate
        {
            Id = "test_scene",
            SituationTemplates = new List<SituationTemplate>
            {
                new()
                {
                    Id = "sit1",
                    ChoiceTemplates = new List<ChoiceTemplate>
                    {
                        new(), new(), new(), new(), new()  // 5 choices
                    }
                },
                new() { Id = "sit2" }
            },
            SpawnRules = CreateValidTemplate().SpawnRules
        };

        SceneValidationResult result = SceneTemplateValidator.Validate(template);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Code == "STRUCT_004");
    }

    // HIGHLANDER: Transition validation tests REMOVED (arc42 §8.30)
    // Flow control now through Consequence.NextSituationTemplateId and IsTerminal
    // Invalid flow references validated at runtime via Scene.AdvanceToNextSituation()

    [Fact]
    public void Validate_InvalidInitialSituation_ReturnsError()
    {
        var template = CreateValidTemplate();
        template.SpawnRules.InitialSituationTemplateId = "invalid_initial";

        SceneValidationResult result = SceneTemplateValidator.Validate(template);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Code == "DEP_003");
    }

    [Fact]
    public void Validate_MultipleErrors_ReturnsAllErrors()
    {
        var template = new SceneTemplate
        {
            Id = null,
            SituationTemplates = new List<SituationTemplate>
            {
                new() { Id = null },
                new() { Id = "sit2" }
            },
            SpawnRules = new SituationSpawnRules
            {
                Pattern = SpawnPattern.Linear,
                InitialSituationTemplateId = "invalid"
                // HIGHLANDER: Flow control through Consequence.NextSituationTemplateId (arc42 §8.30)
            }
        };

        SceneValidationResult result = SceneTemplateValidator.Validate(template);

        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 3);
    }
}
