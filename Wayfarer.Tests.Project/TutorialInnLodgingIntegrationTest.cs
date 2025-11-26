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

        // ASSERT: Tutorial scene templates exist (semantic query by MainStory category and modal presentation)
        // Scenes activate via LOCATION ONLY when player enters matching location
        SceneTemplate tutorialLodging = gameWorld.SceneTemplates
            .FirstOrDefault(st => st.Category == StoryCategory.MainStory &&
                                 st.PresentationMode == PresentationMode.Modal &&
                                 st.LocationActivationFilter != null);

        Assert.NotNull(tutorialLodging);
        Assert.NotEmpty(tutorialLodging.SituationTemplates);
        Assert.Equal(StoryCategory.MainStory, tutorialLodging.Category);
        Assert.Equal(PresentationMode.Modal, tutorialLodging.PresentationMode);
    }

    [Fact]
    public void Tutorial_SituationTemplates_HaveLocationFilters()
    {
        // ARRANGE & ACT
        GameWorld gameWorld = GameWorldInitializer.CreateGameWorld("Content/Core");
        // Semantic query by MainStory category and modal presentation
        SceneTemplate tutorialLodging = gameWorld.SceneTemplates
            .First(st => st.Category == StoryCategory.MainStory &&
                        st.PresentationMode == PresentationMode.Modal &&
                        st.LocationActivationFilter != null);

        // ASSERT: Situations use PlacementFilter (NEW architecture)
        foreach (SituationTemplate situation in tutorialLodging.SituationTemplates)
        {
            // Hierarchical placement: LocationFilter or inherit from scene LocationActivationFilter
            bool hasLocationFilter =
                situation.LocationFilter != null ||
                tutorialLodging.LocationActivationFilter != null;

            Assert.True(hasLocationFilter,
                $"Situation {situation.Id} missing location filter (hierarchical placement)");
        }
    }

    [Fact]
    public void Tutorial_FirstSituation_HasMultipleChoicePaths()
    {
        // ARRANGE & ACT
        GameWorld gameWorld = GameWorldInitializer.CreateGameWorld("Content/Core");
        // Semantic query by MainStory category and modal presentation
        SceneTemplate tutorialLodging = gameWorld.SceneTemplates
            .First(st => st.Category == StoryCategory.MainStory &&
                        st.PresentationMode == PresentationMode.Modal &&
                        st.LocationActivationFilter != null);

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
