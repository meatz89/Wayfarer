using Xunit;

namespace Wayfarer.Tests.Project;

/// <summary>
/// Integration tests for tutorial inn_lodging scene.
/// Tests PLAYER EXPERIENCE: Can player actually play through tutorial?
/// REWRITTEN from scratch for hierarchical placement architecture.
/// </summary>
public class TutorialInnLodgingIntegrationTest
{
    [Fact]
    public void Tutorial_LoadsSceneTemplates()
    {
        // ARRANGE & ACT
        GameWorld gameWorld = GameWorldInitializer.CreateGameWorld("Content/Core");

        // ASSERT: Tutorial scene templates exist (semantic query by starter and presentation mode)
        SceneTemplate tutorialLodging = gameWorld.SceneTemplates
            .FirstOrDefault(st => st.IsStarter && st.PresentationMode == PresentationMode.Modal);

        Assert.NotNull(tutorialLodging);
        Assert.NotEmpty(tutorialLodging.SituationTemplates);
        Assert.True(tutorialLodging.IsStarter);
        Assert.Equal(PresentationMode.Modal, tutorialLodging.PresentationMode);
    }

    [Fact]
    public void Tutorial_SituationTemplates_HaveLocationFilters()
    {
        // ARRANGE & ACT
        GameWorld gameWorld = GameWorldInitializer.CreateGameWorld("Content/Core");
        // Semantic query by starter and presentation mode instead of hardcoded ID
        SceneTemplate tutorialLodging = gameWorld.SceneTemplates
            .First(st => st.IsStarter && st.PresentationMode == PresentationMode.Modal);

        // ASSERT: Situations use PlacementFilter (NEW architecture)
        foreach (SituationTemplate situation in tutorialLodging.SituationTemplates)
        {
            // Hierarchical placement: LocationFilter or inherit from scene BaseLocationFilter
            bool hasLocationFilter =
                situation.LocationFilter != null ||
                tutorialLodging.BaseLocationFilter != null;

            Assert.True(hasLocationFilter,
                $"Situation {situation.Id} missing location filter (hierarchical placement)");
        }
    }

    [Fact]
    public void Tutorial_FirstSituation_HasMultipleChoicePaths()
    {
        // ARRANGE & ACT
        GameWorld gameWorld = GameWorldInitializer.CreateGameWorld("Content/Core");
        // Semantic query by starter and presentation mode instead of hardcoded ID
        SceneTemplate tutorialLodging = gameWorld.SceneTemplates
            .First(st => st.IsStarter && st.PresentationMode == PresentationMode.Modal);

        SituationTemplate firstSituation = tutorialLodging.SituationTemplates.First();

        // ASSERT: Multiple paths for player choice
        Assert.NotEmpty(firstSituation.ChoiceTemplates);
        Assert.True(firstSituation.ChoiceTemplates.Count > 1,
            "First situation should have multiple player choices");

        // Should have at least one always-available choice
        bool hasUnblockedChoice = firstSituation.ChoiceTemplates.Any(c =>
            c.RequirementFormula == null ||
            c.RequirementFormula.OrPaths == null ||
            !c.RequirementFormula.OrPaths.Any());

        Assert.True(hasUnblockedChoice,
            "First situation must have at least one accessible choice (prevent soft-lock)");
    }
}
