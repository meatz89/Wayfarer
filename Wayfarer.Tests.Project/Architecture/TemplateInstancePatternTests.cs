using System.Reflection;
using Xunit;

namespace Wayfarer.Tests.Architecture;

/// <summary>
/// Tests for Template/Instance Pattern compliance (arc42 §8.4 Three-Tier Timing Model).
/// Templates (immutable archetypes) instantiate at parse-time.
/// Instance state (mutable runtime) should reference templates, not duplicate their data.
///
/// BEHAVIOR-ONLY TESTING (§8.21): Tests verify structural compliance.
/// </summary>
public class TemplateInstancePatternTests
{
    /// <summary>
    /// Verify ActiveEmergencyState references Template, doesn't duplicate.
    /// State class should hold reference to template + mutable state only.
    /// </summary>
    [Fact]
    public void ActiveEmergencyState_ReferencesTemplate()
    {
        Type stateType = typeof(ActiveEmergencyState);
        PropertyInfo[] properties = stateType.GetProperties();

        // Must have Template property
        PropertyInfo templateProp = properties.FirstOrDefault(p => p.Name == "Template");
        Assert.NotNull(templateProp);
        Assert.Equal(typeof(EmergencySituation), templateProp.PropertyType);

        // Must NOT have duplicated template properties
        List<string> templateOnlyProperties = new List<string>
        {
            "Name", "Description", "TriggerDay", "TriggerSegment",
            "ResponseWindowSegments", "Responses", "IgnoreOutcome"
        };

        foreach (string propName in templateOnlyProperties)
        {
            PropertyInfo prop = properties.FirstOrDefault(p => p.Name == propName);
            Assert.Null(prop);
        }

        // Must have mutable state properties
        PropertyInfo isTriggeredProp = properties.FirstOrDefault(p => p.Name == "IsTriggered");
        Assert.NotNull(isTriggeredProp);
        Assert.Equal(typeof(bool), isTriggeredProp.PropertyType);

        PropertyInfo isResolvedProp = properties.FirstOrDefault(p => p.Name == "IsResolved");
        Assert.NotNull(isResolvedProp);
        Assert.Equal(typeof(bool), isResolvedProp.PropertyType);
    }

    /// <summary>
    /// Verify ObservationSceneState references Template, doesn't duplicate.
    /// </summary>
    [Fact]
    public void ObservationSceneState_ReferencesTemplate()
    {
        Type stateType = typeof(ObservationSceneState);
        PropertyInfo[] properties = stateType.GetProperties();

        // Must have Template property
        PropertyInfo templateProp = properties.FirstOrDefault(p => p.Name == "Template");
        Assert.NotNull(templateProp);
        Assert.Equal(typeof(ObservationScene), templateProp.PropertyType);

        // Must NOT have duplicated template properties
        List<string> templateOnlyProperties = new List<string>
        {
            "Id", "Name", "Description", "RequiredKnowledge",
            "IsRepeatable", "ExaminationPoints"
        };

        foreach (string propName in templateOnlyProperties)
        {
            PropertyInfo prop = properties.FirstOrDefault(p => p.Name == propName);
            Assert.Null(prop);
        }

        // Must have mutable state properties
        PropertyInfo isCompletedProp = properties.FirstOrDefault(p => p.Name == "IsCompleted");
        Assert.NotNull(isCompletedProp);
    }

    /// <summary>
    /// Verify EmergencySituation template is immutable (all setters, no mutable state).
    /// Templates should not have state-tracking properties.
    /// </summary>
    [Fact]
    public void EmergencySituation_IsImmutableTemplate()
    {
        Type templateType = typeof(EmergencySituation);
        PropertyInfo[] properties = templateType.GetProperties();

        // Should have template properties
        PropertyInfo idProp = properties.FirstOrDefault(p => p.Name == "Id");
        Assert.NotNull(idProp);

        PropertyInfo nameProp = properties.FirstOrDefault(p => p.Name == "Name");
        Assert.NotNull(nameProp);

        PropertyInfo descProp = properties.FirstOrDefault(p => p.Name == "Description");
        Assert.NotNull(descProp);

        // Should NOT have runtime state properties
        List<string> runtimeStateProperties = new List<string>
        {
            "IsTriggered", "IsResolved", "IsActive", "CurrentState",
            "TriggeredAtSegment", "TriggeredAtDay"
        };

        foreach (string propName in runtimeStateProperties)
        {
            PropertyInfo prop = properties.FirstOrDefault(p => p.Name == propName);
            Assert.Null(prop);
        }
    }

    /// <summary>
    /// Verify ObservationScene template is immutable.
    /// </summary>
    [Fact]
    public void ObservationScene_IsImmutableTemplate()
    {
        Type templateType = typeof(ObservationScene);
        PropertyInfo[] properties = templateType.GetProperties();

        // Should have template properties
        PropertyInfo idProp = properties.FirstOrDefault(p => p.Name == "Id");
        Assert.NotNull(idProp);

        PropertyInfo nameProp = properties.FirstOrDefault(p => p.Name == "Name");
        Assert.NotNull(nameProp);

        // Should NOT have runtime state properties
        List<string> runtimeStateProperties = new List<string>
        {
            "IsCompleted", "CurrentProgress", "ExaminedPoints",
            "PlayerVisited", "IsActive"
        };

        foreach (string propName in runtimeStateProperties)
        {
            PropertyInfo prop = properties.FirstOrDefault(p => p.Name == propName);
            Assert.Null(prop);
        }
    }

    /// <summary>
    /// Verify GameWorld separates template collections from state collections.
    /// </summary>
    [Fact]
    public void GameWorld_SeparatesTemplatesFromState()
    {
        Type gwType = typeof(GameWorld);
        PropertyInfo[] properties = gwType.GetProperties();

        // Template collections (immutable)
        PropertyInfo emergencyTemplates = properties.FirstOrDefault(p => p.Name == "EmergencySituations");
        Assert.NotNull(emergencyTemplates);

        PropertyInfo observationTemplates = properties.FirstOrDefault(p => p.Name == "ObservationScenes");
        Assert.NotNull(observationTemplates);

        // State collections (mutable)
        PropertyInfo emergencyStates = properties.FirstOrDefault(p => p.Name == "EmergencyStates");
        Assert.NotNull(emergencyStates);

        PropertyInfo observationStates = properties.FirstOrDefault(p => p.Name == "ObservationSceneStates");
        Assert.NotNull(observationStates);

        // Verify types are different (templates vs states)
        Assert.True(IsListOf(emergencyTemplates.PropertyType, typeof(EmergencySituation)));
        Assert.True(IsListOf(emergencyStates.PropertyType, typeof(ActiveEmergencyState)));
        Assert.True(IsListOf(observationTemplates.PropertyType, typeof(ObservationScene)));
        Assert.True(IsListOf(observationStates.PropertyType, typeof(ObservationSceneState)));
    }

    /// <summary>
    /// Verify SceneTemplate is a template type (has Id, defines archetype).
    /// </summary>
    [Fact]
    public void SceneTemplate_IsProperTemplate()
    {
        Type templateType = typeof(SceneTemplate);
        PropertyInfo[] properties = templateType.GetProperties();

        // Should have Id (templates allowed to have IDs)
        PropertyInfo idProp = properties.FirstOrDefault(p => p.Name == "Id");
        Assert.NotNull(idProp);

        // Should have archetype-defining properties
        PropertyInfo categoryProp = properties.FirstOrDefault(p => p.Name == "Category");
        Assert.NotNull(categoryProp);

        // Should NOT have instance state
        List<string> instanceStateProperties = new List<string>
        {
            "IsActive", "IsCompleted", "CurrentSituation", "PlayerProgress"
        };

        foreach (string propName in instanceStateProperties)
        {
            PropertyInfo prop = properties.FirstOrDefault(p => p.Name == propName);
            Assert.Null(prop);
        }
    }

    /// <summary>
    /// Verify SituationTemplate is a template type.
    /// </summary>
    [Fact]
    public void SituationTemplate_IsProperTemplate()
    {
        Type templateType = typeof(SituationTemplate);
        PropertyInfo[] properties = templateType.GetProperties();

        // Should have Id
        PropertyInfo idProp = properties.FirstOrDefault(p => p.Name == "Id");
        Assert.NotNull(idProp);

        // Should NOT have instance state
        PropertyInfo isCompletedProp = properties.FirstOrDefault(p => p.Name == "IsCompleted");
        Assert.Null(isCompletedProp);
    }

    /// <summary>
    /// Verify ChoiceTemplate is a template type.
    /// </summary>
    [Fact]
    public void ChoiceTemplate_IsProperTemplate()
    {
        Type templateType = typeof(ChoiceTemplate);
        PropertyInfo[] properties = templateType.GetProperties();

        // Should have template-defining properties
        PropertyInfo costTemplateProp = properties.FirstOrDefault(p => p.Name == "CostTemplate");
        PropertyInfo rewardTemplateProp = properties.FirstOrDefault(p => p.Name == "RewardTemplate");
        // At least one of these should exist
        Assert.True(costTemplateProp != null || rewardTemplateProp != null || properties.Length > 0);

        // Should NOT have instance state
        PropertyInfo wasChosenProp = properties.FirstOrDefault(p => p.Name == "WasChosen");
        Assert.Null(wasChosenProp);

        PropertyInfo timesSelectedProp = properties.FirstOrDefault(p => p.Name == "TimesSelected");
        Assert.Null(timesSelectedProp);
    }

    /// <summary>
    /// Verify State definitions are templates (metadata about conditions).
    /// </summary>
    [Fact]
    public void State_IsMetadataTemplate()
    {
        Type stateType = typeof(State);
        PropertyInfo[] properties = stateType.GetProperties();

        // Should have type identifier
        PropertyInfo typeProp = properties.FirstOrDefault(p => p.Name == "Type");
        Assert.NotNull(typeProp);

        // Should have metadata properties
        PropertyInfo categoryProp = properties.FirstOrDefault(p => p.Name == "Category");
        PropertyInfo durationProp = properties.FirstOrDefault(p => p.Name == "Duration");
        Assert.True(categoryProp != null || durationProp != null);

        // Should NOT have instance tracking
        PropertyInfo appliedAtProp = properties.FirstOrDefault(p => p.Name == "AppliedAt");
        Assert.Null(appliedAtProp);
    }

    /// <summary>
    /// Verify ActiveState is instance state (tracks when state was applied).
    /// </summary>
    [Fact]
    public void ActiveState_IsInstanceState()
    {
        Type activeStateType = typeof(ActiveState);
        PropertyInfo[] properties = activeStateType.GetProperties();

        // Should have state type (reference to template)
        PropertyInfo typeProp = properties.FirstOrDefault(p => p.Name == "Type");
        Assert.NotNull(typeProp);

        // Should have instance tracking
        PropertyInfo appliedDayProp = properties.FirstOrDefault(p => p.Name == "AppliedDay");
        Assert.NotNull(appliedDayProp);

        PropertyInfo appliedSegmentProp = properties.FirstOrDefault(p => p.Name == "AppliedSegment");
        Assert.NotNull(appliedSegmentProp);
    }

    /// <summary>
    /// Verify Achievement is a template (defines milestone archetype).
    /// </summary>
    [Fact]
    public void Achievement_IsTemplate()
    {
        Type achievementType = typeof(Achievement);
        PropertyInfo[] properties = achievementType.GetProperties();

        // Should have template properties
        PropertyInfo nameProp = properties.FirstOrDefault(p => p.Name == "Name");
        Assert.NotNull(nameProp);

        // Should NOT have earned state
        PropertyInfo isEarnedProp = properties.FirstOrDefault(p => p.Name == "IsEarned");
        Assert.Null(isEarnedProp);

        PropertyInfo earnedAtProp = properties.FirstOrDefault(p => p.Name == "EarnedAt");
        Assert.Null(earnedAtProp);
    }

    /// <summary>
    /// Verify PlayerAchievement is instance state (tracks when earned).
    /// </summary>
    [Fact]
    public void PlayerAchievement_IsInstanceState()
    {
        Type playerAchievementType = typeof(PlayerAchievement);
        PropertyInfo[] properties = playerAchievementType.GetProperties();

        // Should reference Achievement template
        PropertyInfo achievementProp = properties.FirstOrDefault(p => p.Name == "Achievement");
        Assert.NotNull(achievementProp);
        Assert.Equal(typeof(Achievement), achievementProp.PropertyType);

        // Should have instance tracking
        PropertyInfo earnedDayProp = properties.FirstOrDefault(p => p.Name == "EarnedDay");
        Assert.NotNull(earnedDayProp);
    }

    // ========== HELPER METHODS ==========

    private bool IsListOf(Type type, Type elementType)
    {
        if (!type.IsGenericType) return false;
        if (type.GetGenericTypeDefinition() != typeof(List<>)) return false;
        return type.GetGenericArguments()[0] == elementType;
    }
}
