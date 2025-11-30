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
            Tier = 1,
            SituationTemplates = new List<SituationTemplate>
            {
                new() { Id = "sit1", LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation } },
                new() { Id = "sit2", LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation } }
            },
            SpawnRules = new SituationSpawnRules
            {
                Pattern = SpawnPattern.Linear,
                InitialSituationId = "sit1",
                Transitions = new List<SituationTransition>
                {
                    new()
                    {
                        SourceSituationId = "sit1",
                        DestinationSituationId = "sit2",
                        Condition = TransitionCondition.Always
                    }
                }
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
            Tier = 1,
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
            Tier = 1,
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
            Tier = 1,
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
            Tier = 1,
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

    [Fact]
    public void Validate_InvalidTransitionSource_ReturnsError()
    {
        var template = CreateValidTemplate();
        template.SpawnRules.Transitions.Add(new SituationTransition
        {
            SourceSituationId = "invalid_source",
            DestinationSituationId = "sit2"
        });

        SceneValidationResult result = SceneTemplateValidator.Validate(template);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Code == "DEP_001");
    }

    [Fact]
    public void Validate_InvalidTransitionDestination_ReturnsError()
    {
        var template = CreateValidTemplate();
        template.SpawnRules.Transitions.Add(new SituationTransition
        {
            SourceSituationId = "sit1",
            DestinationSituationId = "invalid_destination"
        });

        SceneValidationResult result = SceneTemplateValidator.Validate(template);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Code == "DEP_002");
    }

    [Fact]
    public void Validate_InvalidInitialSituation_ReturnsError()
    {
        var template = CreateValidTemplate();
        template.SpawnRules.InitialSituationId = "invalid_initial";

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
            Tier = 1,
            SituationTemplates = new List<SituationTemplate>
            {
                new() { Id = null },
                new() { Id = "sit2" }
            },
            SpawnRules = new SituationSpawnRules
            {
                Pattern = SpawnPattern.Linear,
                InitialSituationId = "invalid",
                Transitions = new List<SituationTransition>
                {
                    new()
                    {
                        SourceSituationId = "sit1",
                        DestinationSituationId = "sit2",
                        Condition = TransitionCondition.Always
                    }
                }
            }
        };

        SceneValidationResult result = SceneTemplateValidator.Validate(template);

        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 3);
    }
}
