using Xunit;

namespace Wayfarer.Tests.Integration;

/// <summary>
/// Integration tests for parametric scene spawning.
/// Verifies that parameters flow from spawn reward (A2) to spawned scene (A3) to final reward application.
///
/// CRITICAL FLOW:
/// 1. A2 Situation 3 choices set different ContractPayment parameters
/// 2. A3 scene spawns WITH those parameters
/// 3. A3 Situation 5 reward READS those parameters
/// 4. Player receives correct payment amount
///
/// TUTORIAL SPEC:
/// - Choice 1 (Rapport 3+): ContractPayment = 25
/// - Choice 2 (Insurance): ContractPayment = 23
/// - Choice 3 Success: ContractPayment = 27
/// - Choice 3 Failure: ContractPayment = 18
/// - Choice 4 (Standard): ContractPayment = 20
///
/// A3 final situation applies: Base 10 + ContractPayment from parameters
/// </summary>
public class ParametricSpawningTests : IntegrationTestBase
{
    [Fact]
    public void A2Template_NegotiationChoices_HaveContractPaymentParameters()
    {
        // ARRANGE
        GameWorld gameWorld = GetGameWorld();
        // Semantic query by category and sequence instead of hardcoded ID
        SceneTemplate a2Template = gameWorld.SceneTemplates
            .First(st => st.Category == StoryCategory.MainStory && st.MainStorySequence == 2);

        // ACT: Get negotiation situation (Situation 2 - after offer)
        SituationTemplate negotiateSituation = a2Template.SituationTemplates
            .First(sit => sit.Id.Contains("negotiate")); // TODO: Replace with semantic property query

        // ASSERT: All negotiation choices spawn A3 with parameters
        // Semantic query: Get A3 template to compare against
        SceneTemplate a3Template = gameWorld.SceneTemplates.FirstOrDefault(st =>
            st.Category == StoryCategory.MainStory && st.MainStorySequence == 3);
        Assert.NotNull(a3Template); // A3 template must exist

        foreach (ChoiceTemplate choice in negotiateSituation.ChoiceTemplates)
        {
            List<SceneSpawnReward> spawns = GetAllSpawnsFromChoice(choice);
            List<SceneSpawnReward> a3Spawns = spawns
                .Where(s => s.SceneTemplateId == a3Template.Id)
                .ToList();

            Assert.NotEmpty(a3Spawns);

            foreach (SceneSpawnReward spawn in a3Spawns)
            {
                Assert.NotNull(spawn.Parameters);
                Assert.True(spawn.Parameters.ContainsKey("ContractPayment"),
                    $"Choice {choice.Id} missing ContractPayment parameter");

                int payment = int.Parse(spawn.Parameters["ContractPayment"]);
                Assert.InRange(payment, 18, 27); // Valid payment range
            }
        }
    }

    [Fact]
    public async Task A3Scene_ReceivesParameters_FromA2Spawn()
    {
        // ARRANGE
        GameWorld gameWorld = GetGameWorld();
        GameFacade gameFacade = GetGameFacade();
        await gameFacade.StartGameAsync();

        // Navigate to A2 (skip A1 for test speed) - semantic queries by category and sequence
        Scene a1 = gameWorld.Scenes.First(s =>
            s.Template.Category == StoryCategory.MainStory && s.Template.MainStorySequence == 1);
        CompleteSceneWithFallbacks(a1, gameWorld.GetPlayer());

        // ACT: Complete A2 with standard negotiation (ContractPayment = 20)
        Scene a2 = gameWorld.Scenes.First(s =>
            s.Template.Category == StoryCategory.MainStory && s.Template.MainStorySequence == 2);
        ChoiceTemplate standardChoice = a2.CurrentSituation.Template.ChoiceTemplates
            .First(c => c.PathType == ChoicePathType.Fallback);

        await ExecuteChoice(standardChoice, a2, gameWorld.GetPlayer(), gameFacade);

        // ASSERT: A3 spawned with parameters (semantic query by category and sequence)
        Scene a3 = gameWorld.Scenes.FirstOrDefault(s =>
            s.Template.Category == StoryCategory.MainStory && s.Template.MainStorySequence == 3);
        Assert.NotNull(a3);
        Assert.NotNull(a3.Parameters);
        Assert.True(a3.Parameters.ContainsKey("ContractPayment"));
        Assert.Equal("20", a3.Parameters["ContractPayment"]); // Standard = 20
    }

    [Fact]
    public async Task A3Completion_AppliesParameterPayment_ToPlayer()
    {
        // ARRANGE
        GameWorld gameWorld = GetGameWorld();
        GameFacade gameFacade = GetGameFacade();
        await gameFacade.StartGameAsync();

        int initialCoins = gameWorld.GetPlayer().Coins;

        // Complete A1 and A2 (semantic queries by category and sequence)
        Scene a1 = gameWorld.Scenes.First(s =>
            s.Template.Category == StoryCategory.MainStory && s.Template.MainStorySequence == 1);
        CompleteSceneWithFallbacks(a1, gameWorld.GetPlayer());

        Scene a2 = gameWorld.Scenes.First(s =>
            s.Template.Category == StoryCategory.MainStory && s.Template.MainStorySequence == 2);
        CompleteSceneWithFallbacks(a2, gameWorld.GetPlayer());

        int coinsAfterA2 = gameWorld.GetPlayer().Coins;

        // ACT: Complete A3 (all situations) - semantic query by category and sequence
        Scene a3 = gameWorld.Scenes.First(s =>
            s.Template.Category == StoryCategory.MainStory && s.Template.MainStorySequence == 3);
        CompleteSceneWithFallbacks(a3, gameWorld.GetPlayer());

        // ASSERT: Player received ContractPayment + base reward
        int coinsAfterA3 = gameWorld.GetPlayer().Coins;
        int gainedFromA3 = coinsAfterA3 - coinsAfterA2;

        // A3 completion = 10 (base) + ContractPayment (20 for standard)
        Assert.True(gainedFromA3 >= 28, // 10 + 18 minimum
            $"Expected minimum 28 coins from A3 (10 base + 18 min payment), got {gainedFromA3}");
        Assert.True(gainedFromA3 <= 37, // 10 + 27 maximum
            $"Expected maximum 37 coins from A3 (10 base + 27 max payment), got {gainedFromA3}");
    }

    // HELPER METHODS

    private List<SceneSpawnReward> GetAllSpawnsFromChoice(ChoiceTemplate choice)
    {
        List<SceneSpawnReward> spawns = new List<SceneSpawnReward>();

        if (choice.RewardTemplate?.ScenesToSpawn != null)
            spawns.AddRange(choice.RewardTemplate.ScenesToSpawn);

        if (choice.OnSuccessReward?.ScenesToSpawn != null)
            spawns.AddRange(choice.OnSuccessReward.ScenesToSpawn);

        if (choice.OnFailureReward?.ScenesToSpawn != null)
            spawns.AddRange(choice.OnFailureReward.ScenesToSpawn);

        return spawns;
    }

    private void CompleteSceneWithFallbacks(Scene scene, Player player)
    {
        while (scene.State == SceneState.Active)
        {
            ChoiceTemplate fallback = scene.CurrentSituation.Template.ChoiceTemplates
                .FirstOrDefault(c => c.PathType == ChoicePathType.Fallback ||
                                    (c.RequirementFormula == null ||
                                     c.RequirementFormula.OrPaths == null ||
                                     !c.RequirementFormula.OrPaths.Any()));

            if (fallback == null)
                break;

            // Execute fallback synchronously
            Task.Run(async () => await ExecuteChoice(fallback, scene, player, GetGameFacade())).Wait();
        }
    }

    private async Task ExecuteChoice(ChoiceTemplate choice, Scene scene, Player player, GameFacade gameFacade)
    {
        // This is a placeholder - actual implementation depends on GameFacade API
        // Real implementation would call something like: gameFacade.ExecuteChoiceAsync(choice.Id, scene.Id)
        throw new NotImplementedException("ExecuteChoice needs GameFacade API");
    }
}
