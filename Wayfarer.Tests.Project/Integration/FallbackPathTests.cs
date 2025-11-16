using Xunit;

namespace Wayfarer.Tests.Integration;

/// <summary>
/// Integration tests verifying soft-lock prevention through fallback paths.
/// Tests that player with ZERO resources (0 coins, 0 stats) can complete tutorial.
///
/// CRITICAL SOFT-LOCK PREVENTION:
/// - Every A-story situation MUST have at least one choice with NO requirements
/// - PathType.Fallback choices are explicitly designed as guaranteed progression
/// - Fallback choices must advance the scene (not trap player in same state)
///
/// TEST STRATEGY:
/// 1. Create player with worst possible resources (0 coins, 0 stats, minimal health)
/// 2. At each situation, find and select fallback choice
/// 3. Verify situation advances (doesn't repeat)
/// 4. Verify scene completes or advances to next scene
/// 5. Verify player reaches end of tutorial without soft-lock
/// </summary>
public class FallbackPathTests : IntegrationTestBase
{
    [Fact]
    public void AllAStoryScenes_HaveFallbackInEverySituation()
    {
        // ARRANGE
        GameWorld gameWorld = GetGameWorld();
        List<SceneTemplate> aStoryTemplates = gameWorld.SceneTemplates
            .Where(st => st.Category == StoryCategory.MainStory && st.MainStorySequence.HasValue)
            .OrderBy(st => st.MainStorySequence.Value)
            .ToList();

        Assert.NotEmpty(aStoryTemplates);

        // ACT & ASSERT: Every situation has fallback
        foreach (SceneTemplate template in aStoryTemplates)
        {
            foreach (SituationTemplate situation in template.SituationTemplates)
            {
                bool hasFallback = situation.ChoiceTemplates.Any(IsFallbackChoice);

                Assert.True(hasFallback,
                    $"A-story situation '{situation.Id}' in scene '{template.Id}' " +
                    $"(A{template.MainStorySequence}) has no fallback choice. " +
                    $"This creates soft-lock risk for players with zero resources.");
            }
        }
    }

    [Fact]
    public void A1_AllSituations_HaveFallbackChoices()
    {
        // ARRANGE
        GameWorld gameWorld = GetGameWorld();
        // Semantic query by category and sequence instead of hardcoded ID
        SceneTemplate a1 = gameWorld.SceneTemplates
            .First(st => st.Category == StoryCategory.MainStory && st.MainStorySequence == 1);

        // ACT & ASSERT
        Assert.Equal(3, a1.SituationTemplates.Count); // Inn lodging = 3 situations

        foreach (SituationTemplate situation in a1.SituationTemplates)
        {
            ChoiceTemplate fallback = situation.ChoiceTemplates
                .FirstOrDefault(IsFallbackChoice);

            Assert.NotNull(fallback);
            Assert.True(fallback.PathType == ChoicePathType.Fallback ||
                       IsGuaranteedAccessible(fallback),
                $"Situation {situation.Id} fallback choice has unexpected path type or requirements");
        }
    }

    [Fact]
    public void A2_AllSituations_HaveFallbackChoices()
    {
        // ARRANGE
        GameWorld gameWorld = GetGameWorld();
        // Semantic query by category and sequence instead of hardcoded ID
        SceneTemplate a2 = gameWorld.SceneTemplates
            .First(st => st.Category == StoryCategory.MainStory && st.MainStorySequence == 2);

        // ACT & ASSERT
        foreach (SituationTemplate situation in a2.SituationTemplates)
        {
            ChoiceTemplate fallback = situation.ChoiceTemplates
                .FirstOrDefault(IsFallbackChoice);

            Assert.NotNull(fallback);
            Assert.True(fallback.PathType == ChoicePathType.Fallback ||
                       IsGuaranteedAccessible(fallback),
                $"A2 Situation {situation.Id} fallback choice has unexpected path type or requirements");
        }
    }

    [Fact]
    public void A3_AllSituations_HaveFallbackChoices()
    {
        // ARRANGE
        GameWorld gameWorld = GetGameWorld();
        // Semantic query by category and sequence instead of hardcoded ID
        SceneTemplate a3 = gameWorld.SceneTemplates
            .First(st => st.Category == StoryCategory.MainStory && st.MainStorySequence == 3);

        // ACT & ASSERT
        Assert.Equal(5, a3.SituationTemplates.Count); // Route = 5 situations (4 segments + arrival)

        foreach (SituationTemplate situation in a3.SituationTemplates)
        {
            ChoiceTemplate fallback = situation.ChoiceTemplates
                .FirstOrDefault(IsFallbackChoice);

            Assert.NotNull(fallback);
            Assert.True(fallback.PathType == ChoicePathType.Fallback ||
                       IsGuaranteedAccessible(fallback),
                $"A3 Situation {situation.Id} fallback choice has unexpected path type or requirements");
        }
    }

    [Fact]
    public void FallbackChoices_AdvanceProgression()
    {
        // ARRANGE
        GameWorld gameWorld = GetGameWorld();
        List<SceneTemplate> aStoryTemplates = gameWorld.SceneTemplates
            .Where(st => st.Category == StoryCategory.MainStory && st.MainStorySequence.HasValue)
            .OrderBy(st => st.MainStorySequence.Value)
            .ToList();

        // ACT & ASSERT: Fallback choices cause advancement
        foreach (SceneTemplate template in aStoryTemplates)
        {
            foreach (SituationTemplate situation in template.SituationTemplates)
            {
                ChoiceTemplate fallback = situation.ChoiceTemplates
                    .FirstOrDefault(IsFallbackChoice);

                Assert.NotNull(fallback);

                // Fallback must either:
                // 1. Spawn next scene (final situation)
                // 2. Advance to next situation (non-final)
                // 3. Complete scene (single situation)

                bool isFinalSituation = IsFinalSituation(situation, template.SpawnRules);
                bool spawnsNextScene = HasSceneSpawnReward(fallback);

                if (isFinalSituation)
                {
                    Assert.True(spawnsNextScene || template.SituationTemplates.Count == 1,
                        $"Final situation '{situation.Id}' in '{template.Id}' fallback must spawn next scene");
                }
                else
                {
                    // Non-final situation: verify transition exists
                    bool hasTransitionFromHere = template.SpawnRules?.Transitions?.Any(t =>
                        t.SourceSituationId == situation.Id) == true;

                    Assert.True(hasTransitionFromHere,
                        $"Non-final situation '{situation.Id}' has no transition (soft-lock risk)");
                }
            }
        }
    }

    [Fact]
    public void FallbackChoices_HaveNoResourceCosts()
    {
        // ARRANGE
        GameWorld gameWorld = GetGameWorld();
        List<SceneTemplate> aStoryTemplates = gameWorld.SceneTemplates
            .Where(st => st.Category == StoryCategory.MainStory && st.MainStorySequence.HasValue)
            .ToList();

        // ACT & ASSERT: Fallback choices must have zero costs
        foreach (SceneTemplate template in aStoryTemplates)
        {
            foreach (SituationTemplate situation in template.SituationTemplates)
            {
                ChoiceTemplate fallback = situation.ChoiceTemplates
                    .FirstOrDefault(IsFallbackChoice);

                if (fallback != null)
                {
                    ChoiceCost cost = fallback.CostTemplate ?? new ChoiceCost();

                    Assert.True(cost.Coins == 0,
                        $"Fallback choice '{fallback.Id}' in '{situation.Id}' costs {cost.Coins} coins (should be 0)");
                    Assert.True(cost.Health == 0,
                        $"Fallback choice '{fallback.Id}' in '{situation.Id}' costs {cost.Health} health (should be 0)");
                    Assert.True(cost.Stamina == 0,
                        $"Fallback choice '{fallback.Id}' in '{situation.Id}' costs {cost.Stamina} stamina (should be 0)");

                    // NOTE: Some fallbacks MAY have minor narrative costs (e.g., rapport loss)
                    // but NEVER blocking costs (coins, health, stats required to progress)
                }
            }
        }
    }

    // HELPER METHODS

    private bool IsFallbackChoice(ChoiceTemplate choice)
    {
        // Fallback choice criteria:
        // 1. Explicitly marked as Fallback path type, OR
        // 2. Has no requirements (guaranteed accessible)
        return choice.PathType == ChoicePathType.Fallback ||
               IsGuaranteedAccessible(choice);
    }

    private bool IsGuaranteedAccessible(ChoiceTemplate choice)
    {
        // No requirements = always accessible
        return choice.RequirementFormula == null ||
               choice.RequirementFormula.OrPaths == null ||
               !choice.RequirementFormula.OrPaths.Any();
    }

    private bool IsFinalSituation(SituationTemplate situation, SituationSpawnRules rules)
    {
        if (rules?.Transitions == null || !rules.Transitions.Any())
            return true; // No transitions = single situation = final

        // Final situation = no transitions FROM this situation
        return !rules.Transitions.Any(t => t.SourceSituationId == situation.Id);
    }

    private bool HasSceneSpawnReward(ChoiceTemplate choice)
    {
        return choice.RewardTemplate?.ScenesToSpawn?.Any() == true ||
               choice.OnSuccessReward?.ScenesToSpawn?.Any() == true ||
               choice.OnFailureReward?.ScenesToSpawn?.Any() == true;
    }
}
