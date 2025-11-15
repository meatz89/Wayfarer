using Xunit;

namespace Wayfarer.Tests;

/// <summary>
/// Integration tests simulating ACTUAL PLAYER EXPERIENCE.
/// Tests PLAYABILITY: Can player actually progress through A-Story?
/// REWRITTEN from scratch for hierarchical placement architecture.
///
/// Philosophy: If player gets soft-locked, test FAILS.
/// </summary>
public class AStoryPlayerExperienceTest : IntegrationTestBase
{
    [Fact]
    public async Task PlayerStartsGame_CanImmediatelyPlayA1()
    {
        // ARRANGE & ACT: Player starts new game
        GameWorld gameWorld = GetGameWorld();
        GameFacade gameFacade = GetGameFacade();
        await gameFacade.StartGameAsync();

        // ASSERT: Player can see and interact with A1
        Scene a1 = gameWorld.Scenes
            .Where(s => s.State == SceneState.Active)
            .FirstOrDefault(s => s.Category == StoryCategory.MainStory && s.MainStorySequence == 1);

        Assert.NotNull(a1); // A1 spawned and is Active

        Assert.NotNull(a1.CurrentSituation); // A1 has current situation

        Assert.NotEmpty(a1.CurrentSituation.Template.ChoiceTemplates); // Situation has choices

        // Player must have at least ONE accessible choice (prevent soft-lock)
        bool hasAccessibleChoice = a1.CurrentSituation.Template.ChoiceTemplates.Any(choice =>
            choice.RequirementFormula == null ||
            choice.RequirementFormula.OrPaths == null ||
            !choice.RequirementFormula.OrPaths.Any());

        Assert.True(hasAccessibleChoice,
            "SOFT LOCK: Player cannot select any choices in A1");
    }

    [Fact]
    public void A1SceneTemplate_Exists()
    {
        // ARRANGE & ACT
        GameWorld gameWorld = GetGameWorld();

        // ASSERT: A1 template exists in game content
        SceneTemplate a1Template = gameWorld.SceneTemplates
            .FirstOrDefault(st => st.Id == "a1_secure_lodging");

        Assert.NotNull(a1Template);
        Assert.NotEmpty(a1Template.SituationTemplates);
        Assert.True(a1Template.IsStarter);
    }

    [Fact]
    public void A2SceneTemplate_Exists()
    {
        // ARRANGE & ACT
        GameWorld gameWorld = GetGameWorld();

        // ASSERT: A2 template exists (spawned after A1)
        SceneTemplate a2Template = gameWorld.SceneTemplates
            .FirstOrDefault(st => st.Id == "a2_morning");

        Assert.NotNull(a2Template);
        Assert.NotEmpty(a2Template.SituationTemplates);
    }

    [Fact]
    public void A3SceneTemplate_Exists()
    {
        // ARRANGE & ACT
        GameWorld gameWorld = GetGameWorld();

        // ASSERT: A3 template exists (spawned after A2)
        SceneTemplate a3Template = gameWorld.SceneTemplates
            .FirstOrDefault(st => st.Id == "a3_route_travel");

        Assert.NotNull(a3Template);
        Assert.NotEmpty(a3Template.SituationTemplates);
    }

    [Fact]
    public void AllAStoryTemplates_HaveSituationsWithChoices()
    {
        // ARRANGE & ACT
        GameWorld gameWorld = GetGameWorld();
        List<SceneTemplate> aStoryTemplates = gameWorld.SceneTemplates
            .Where(st => st.Id.StartsWith("a") && char.IsDigit(st.Id[1]))
            .ToList();

        // ASSERT: Every A-Story template has situations with choices
        Assert.NotEmpty(aStoryTemplates);

        foreach (SceneTemplate template in aStoryTemplates)
        {
            Assert.NotEmpty(template.SituationTemplates);

            foreach (SituationTemplate situation in template.SituationTemplates)
            {
                Assert.NotEmpty(situation.ChoiceTemplates);
            }
        }
    }
}
