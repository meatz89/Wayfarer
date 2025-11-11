using Xunit;

namespace Wayfarer.Tests.Project.Validation;

/// <summary>
/// PARSE-TIME validation tests for A-story JSON authoring.
/// These tests validate JSON structure BEFORE runtime.
/// They catch authoring errors, not player experience failures.
///
/// IMPORTANT: These tests check if content will PARSE correctly.
/// They do NOT test if player can actually PLAY the content.
/// See AStoryPlayerExperienceTest for actual gameplay validation.
///
/// Use case: Author writes A4.json with missing sequence, parser catches error.
/// Not use case: Checking if player can complete A4 (that's player experience test).
/// </summary>
public class AStoryValidatorTests
{
    private SceneTemplate CreateAStoryTemplate(int sequence)
    {
        return new SceneTemplate
        {
            Id = $"a{sequence}_test",
            Category = StoryCategory.MainStory,
            MainStorySequence = sequence,
            Tier = 0,
            SituationTemplates = new List<SituationTemplate>
            {
                new SituationTemplate
                {
                    Id = $"sit{sequence}_1",
                    ChoiceTemplates = new List<ChoiceTemplate>
                    {
                        new ChoiceTemplate
                        {
                            Id = $"choice{sequence}_guaranteed",
                            ActionType = ChoiceActionType.Instant,
                            RequirementFormula = null,
                            PathType = ChoicePathType.Fallback
                        }
                    }
                }
            },
            SpawnRules = new SituationSpawnRules
            {
                Pattern = SpawnPattern.Linear,
                InitialSituationId = $"sit{sequence}_1",
                Transitions = new List<SituationTransition>()
            }
        };
    }

    [Fact]
    public void ValidateAStoryChain_CompleteSequence_ReturnsValid()
    {
        // ARRANGE: Complete A1-A3 chain
        var templates = new List<SceneTemplate>
        {
            CreateAStoryTemplate(1),
            CreateAStoryTemplate(2),
            CreateAStoryTemplate(3)
        };

        // ACT
        SceneValidationResult result = SceneTemplateValidator.ValidateAStoryChain(templates);

        // ASSERT
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateAStoryChain_MissingSequence_ReturnsError()
    {
        // ARRANGE: A1, A3 (missing A2)
        var templates = new List<SceneTemplate>
        {
            CreateAStoryTemplate(1),
            CreateAStoryTemplate(3)
        };

        // ACT
        SceneValidationResult result = SceneTemplateValidator.ValidateAStoryChain(templates);

        // ASSERT
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Code == "ACHAIN_001");
    }

    [Fact]
    public void ValidateAStoryChain_DoesNotStartAt1_ReturnsError()
    {
        // ARRANGE: A2, A3 (missing A1)
        var templates = new List<SceneTemplate>
        {
            CreateAStoryTemplate(2),
            CreateAStoryTemplate(3)
        };

        // ACT
        SceneValidationResult result = SceneTemplateValidator.ValidateAStoryChain(templates);

        // ASSERT
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Code == "ACHAIN_003");
    }

    [Fact]
    public void ValidateAStoryChain_DuplicateSequence_ReturnsError()
    {
        // ARRANGE: Two A2s
        var templates = new List<SceneTemplate>
        {
            CreateAStoryTemplate(1),
            CreateAStoryTemplate(2),
            CreateAStoryTemplate(2) // Duplicate
        };

        // ACT
        SceneValidationResult result = SceneTemplateValidator.ValidateAStoryChain(templates);

        // ASSERT
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Code == "ACHAIN_002");
    }

    [Fact]
    public void ValidateAStoryChain_NoAStoryScenes_ReturnsValid()
    {
        // ARRANGE: No A-story scenes (empty or only B/C stories)
        var templates = new List<SceneTemplate>();

        // ACT
        SceneValidationResult result = SceneTemplateValidator.ValidateAStoryChain(templates);

        // ASSERT: No A-story = valid (allows gradual authoring)
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateAStoryChain_FlexibleLength_AcceptsAnyComplete()
    {
        // ARRANGE: Test various valid lengths
        var testCases = new[]
        {
            new[] { 1, 2, 3 },           // 3 scenes
            new[] { 1, 2, 3, 4, 5 },     // 5 scenes
            new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, // 10 scenes
            new[] { 1 }                   // Single scene (edge case)
        };

        foreach (var sequences in testCases)
        {
            var templates = sequences.Select(CreateAStoryTemplate).ToList();

            // ACT
            SceneValidationResult result = SceneTemplateValidator.ValidateAStoryChain(templates);

            // ASSERT
            Assert.True(result.IsValid,
                $"Should accept complete sequence: {string.Join(", ", sequences)}");
        }
    }

    [Fact]
    public void Validate_MainStoryWithoutSequence_ReturnsError()
    {
        // ARRANGE: MainStory category but no MainStorySequence
        var template = new SceneTemplate
        {
            Id = "a1_test",
            Category = StoryCategory.MainStory,
            MainStorySequence = null,
            Tier = 0,
            SituationTemplates = new List<SituationTemplate>
            {
                new SituationTemplate
                {
                    Id = "sit1_1",
                    ChoiceTemplates = new List<ChoiceTemplate>
                    {
                        new ChoiceTemplate
                        {
                            Id = "choice1_guaranteed",
                            ActionType = ChoiceActionType.Instant,
                            RequirementFormula = null,
                            PathType = ChoicePathType.Fallback
                        }
                    }
                }
            },
            SpawnRules = new SituationSpawnRules
            {
                Pattern = SpawnPattern.Linear,
                InitialSituationId = "sit1_1",
                Transitions = new List<SituationTransition>()
            }
        };

        // ACT
        SceneValidationResult result = SceneTemplateValidator.Validate(template);

        // ASSERT
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Code == "ASTORY_001");
    }

    [Fact]
    public void Validate_AStorySituationWithoutGuaranteedPath_ReturnsError()
    {
        // ARRANGE: A-story situation with only gated choices (no guaranteed path)
        var template = new SceneTemplate
        {
            Id = "a1_test",
            Category = StoryCategory.MainStory,
            MainStorySequence = 1,
            Tier = 0,
            SituationTemplates = new List<SituationTemplate>
            {
                new SituationTemplate
                {
                    Id = "sit1_1",
                    ChoiceTemplates = new List<ChoiceTemplate>
                    {
                        new ChoiceTemplate
                        {
                            Id = "gated_choice",
                            ActionType = ChoiceActionType.Instant,
                            RequirementFormula = new CompoundRequirement
                            {
                                OrPaths = new List<OrPath>
                                {
                                    new OrPath
                                    {
                                        NumericRequirements = new List<NumericRequirement>
                                        {
                                            new NumericRequirement()
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            },
            SpawnRules = new SituationSpawnRules
            {
                Pattern = SpawnPattern.Linear,
                InitialSituationId = "sit1_1",
                Transitions = new List<SituationTransition>()
            }
        };

        // ACT
        SceneValidationResult result = SceneTemplateValidator.Validate(template);

        // ASSERT
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Code == "ASTORY_004");
    }

    [Fact]
    public void Validate_AStoryWithGuaranteedChallenge_IsValid()
    {
        // ARRANGE: Challenge choice that spawns scenes on both success AND failure
        var template = new SceneTemplate
        {
            Id = "a1_test",
            Category = StoryCategory.MainStory,
            MainStorySequence = 1,
            Tier = 0,
            SituationTemplates = new List<SituationTemplate>
            {
                new SituationTemplate
                {
                    Id = "sit1_1",
                    ChoiceTemplates = new List<ChoiceTemplate>
                    {
                        new ChoiceTemplate
                        {
                            Id = "challenge_guaranteed",
                            ActionType = ChoiceActionType.StartChallenge,
                            RequirementFormula = null,
                            RewardTemplate = new ChoiceReward
                            {
                                ScenesToSpawn = new List<SceneSpawnReward>
                                {
                                    new SceneSpawnReward { SceneTemplateId = "next_scene" }
                                }
                            },
                            OnFailureReward = new ChoiceReward
                            {
                                ScenesToSpawn = new List<SceneSpawnReward>
                                {
                                    new SceneSpawnReward { SceneTemplateId = "next_scene" }
                                }
                            }
                        }
                    }
                }
            },
            SpawnRules = new SituationSpawnRules
            {
                Pattern = SpawnPattern.Linear,
                InitialSituationId = "sit1_1",
                Transitions = new List<SituationTransition>()
            }
        };

        // ACT
        SceneValidationResult result = SceneTemplateValidator.Validate(template);

        // ASSERT: Challenge with both success/failure spawns = guaranteed progression
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_NonAStoryWithoutSequence_IsValid()
    {
        // ARRANGE: SideStory without MainStorySequence (correct)
        var template = new SceneTemplate
        {
            Id = "b1_test",
            Category = StoryCategory.SideStory,
            MainStorySequence = null,
            Tier = 0,
            SituationTemplates = new List<SituationTemplate>
            {
                new SituationTemplate
                {
                    Id = "sit1_1",
                    ChoiceTemplates = new List<ChoiceTemplate>
                    {
                        new ChoiceTemplate
                        {
                            Id = "choice1_guaranteed",
                            ActionType = ChoiceActionType.Instant,
                            RequirementFormula = null,
                            PathType = ChoicePathType.Fallback
                        }
                    }
                }
            },
            SpawnRules = new SituationSpawnRules
            {
                Pattern = SpawnPattern.Linear,
                InitialSituationId = "sit1_1",
                Transitions = new List<SituationTransition>()
            }
        };

        // ACT
        SceneValidationResult result = SceneTemplateValidator.Validate(template);

        // ASSERT: B/C stories don't need MainStorySequence
        Assert.True(result.IsValid);
    }
}
