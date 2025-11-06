using Wayfarer.Content;
using Wayfarer.Content.Catalogues;
using Wayfarer.Content.Generators;
using Wayfarer.GameState;
using Wayfarer.GameState.Enums;
using Xunit;

namespace Wayfarer.Tests.Project;

/// <summary>
/// Comprehensive test validating the "service_with_location_access" scene archetype
/// Tests the complete flow: Generation → Dependent Resources → Marker Resolution
/// </summary>
public class LodgingSceneFlowTest
{
    [Fact]
    public void ServiceWithLocationAccess_GeneratesCorrectStructure()
    {
        GenerationContext context = new GenerationContext
        {
            Tier = 1,
            NpcPersonality = PersonalityType.MERCANTILE,
            NpcLocationId = "test_inn",
            NpcId = "test_innkeeper",
            NpcName = "Martha",
            PlayerCoins = 50
        };

        SceneArchetypeDefinition result = SceneArchetypeCatalog.Generate(
            "service_with_location_access",
            context.Tier,
            context);

        Assert.NotNull(result);
        Assert.Equal(4, result.SituationTemplates.Count);

        SituationTemplate negotiate = result.SituationTemplates[0];
        SituationTemplate access = result.SituationTemplates[1];
        SituationTemplate service = result.SituationTemplates[2];
        SituationTemplate depart = result.SituationTemplates[3];

        Assert.Equal("secure_lodging_negotiate", negotiate.Id);
        Assert.Equal("secure_lodging_access", access.Id);
        Assert.Equal("secure_lodging_service", service.Id);
        Assert.Equal("secure_lodging_depart", depart.Id);

        Assert.NotEmpty(negotiate.ChoiceTemplates);
        Assert.Empty(access.ChoiceTemplates);
        Assert.Empty(service.ChoiceTemplates);
        Assert.Empty(depart.ChoiceTemplates);

        Assert.NotNull(access.AutoProgressRewards);
        Assert.NotNull(service.AutoProgressRewards);
        Assert.NotNull(depart.AutoProgressRewards);

        Assert.Equal("generated:private_room", access.RequiredLocationId);
        Assert.Equal("generated:private_room", service.RequiredLocationId);
    }

    [Fact]
    public void ServiceWithLocationAccess_CreatesDependentResources()
    {
        GenerationContext context = new GenerationContext
        {
            Tier = 1,
            NpcPersonality = PersonalityType.MERCANTILE,
            NpcLocationId = "test_inn",
            NpcId = "test_innkeeper",
            NpcName = "Martha",
            PlayerCoins = 50
        };

        SceneArchetypeDefinition result = SceneArchetypeCatalog.Generate(
            "service_with_location_access",
            context.Tier,
            context);

        Assert.Single(result.DependentLocations);
        DependentLocationSpec locationSpec = result.DependentLocations[0];

        Assert.Equal("private_room", locationSpec.TemplateId);
        Assert.Contains("{NPCName}", locationSpec.NamePattern);
        Assert.Equal(VenueIdSource.SameAsBase, locationSpec.VenueIdSource);
        Assert.Equal(HexPlacementStrategy.SameVenue, locationSpec.HexPlacement);
        Assert.True(locationSpec.IsLockedInitially);
        Assert.Equal("room_key", locationSpec.UnlockItemTemplateId);
        Assert.Contains("sleepingSpace", locationSpec.Properties);

        Assert.Single(result.DependentItems);
        DependentItemSpec itemSpec = result.DependentItems[0];

        Assert.Equal("room_key", itemSpec.TemplateId);
        Assert.Contains("{NPCName}", itemSpec.DescriptionPattern);
    }

    [Fact]
    public void ServiceWithLocationAccess_LinearSpawnPattern()
    {
        GenerationContext context = new GenerationContext
        {
            Tier = 1,
            NpcPersonality = PersonalityType.MERCANTILE,
            NpcLocationId = "test_inn",
            NpcId = "test_innkeeper",
            NpcName = "Martha",
            PlayerCoins = 50
        };

        SceneArchetypeDefinition result = SceneArchetypeCatalog.Generate(
            "service_with_location_access",
            context.Tier,
            context);

        Assert.NotNull(result.SpawnRules);
        Assert.Equal(SpawnPattern.Linear, result.SpawnRules.Pattern);
        Assert.Equal("secure_lodging_negotiate", result.SpawnRules.InitialSituationId);
        Assert.Equal(3, result.SpawnRules.Transitions.Count);

        SituationTransition t1 = result.SpawnRules.Transitions[0];
        Assert.Equal("secure_lodging_negotiate", t1.SourceSituationId);
        Assert.Equal("secure_lodging_access", t1.DestinationSituationId);
        Assert.Equal(TransitionCondition.Always, t1.Condition);

        SituationTransition t2 = result.SpawnRules.Transitions[1];
        Assert.Equal("secure_lodging_access", t2.SourceSituationId);
        Assert.Equal("secure_lodging_service", t2.DestinationSituationId);

        SituationTransition t3 = result.SpawnRules.Transitions[2];
        Assert.Equal("secure_lodging_service", t3.SourceSituationId);
        Assert.Equal("secure_lodging_depart", t3.DestinationSituationId);
    }

    [Fact]
    public void ServiceWithLocationAccess_NegotiateRewardsUnlockRoom()
    {
        GenerationContext context = new GenerationContext
        {
            Tier = 1,
            NpcPersonality = PersonalityType.MERCANTILE,
            NpcLocationId = "test_inn",
            NpcId = "test_innkeeper",
            NpcName = "Martha",
            PlayerCoins = 50
        };

        SceneArchetypeDefinition result = SceneArchetypeCatalog.Generate(
            "service_with_location_access",
            context.Tier,
            context);

        SituationTemplate negotiate = result.SituationTemplates[0];
        Assert.NotEmpty(negotiate.ChoiceTemplates);

        ChoiceTemplate firstChoice = negotiate.ChoiceTemplates[0];
        Assert.NotNull(firstChoice.RewardTemplate);
        Assert.NotEmpty(firstChoice.RewardTemplate.LocationsToUnlock);
        Assert.NotEmpty(firstChoice.RewardTemplate.ItemIds);

        Assert.Contains("generated:private_room", firstChoice.RewardTemplate.LocationsToUnlock);
        Assert.Contains("generated:room_key", firstChoice.RewardTemplate.ItemIds);
    }

    [Fact]
    public void ServiceWithLocationAccess_DepartCleansUpResources()
    {
        GenerationContext context = new GenerationContext
        {
            Tier = 1,
            NpcPersonality = PersonalityType.MERCANTILE,
            NpcLocationId = "test_inn",
            NpcId = "test_innkeeper",
            NpcName = "Martha",
            PlayerCoins = 50
        };

        SceneArchetypeDefinition result = SceneArchetypeCatalog.Generate(
            "service_with_location_access",
            context.Tier,
            context);

        SituationTemplate depart = result.SituationTemplates[3];
        Assert.NotNull(depart.AutoProgressRewards);
        Assert.NotEmpty(depart.AutoProgressRewards.ItemsToRemove);
        Assert.NotEmpty(depart.AutoProgressRewards.LocationsToLock);

        Assert.Contains("generated:room_key", depart.AutoProgressRewards.ItemsToRemove);
        Assert.Contains("generated:private_room", depart.AutoProgressRewards.LocationsToLock);
    }

    [Theory]
    [InlineData(PersonalityType.MERCANTILE)]
    [InlineData(PersonalityType.DEVOTED)]
    [InlineData(PersonalityType.CUNNING)]
    [InlineData(PersonalityType.PROUD)]
    [InlineData(PersonalityType.STEADFAST)]
    public void ServiceWithLocationAccess_AllPersonalitiesGenerateValidStructure(PersonalityType personality)
    {
        GenerationContext context = new GenerationContext
        {
            Tier = 1,
            NpcPersonality = personality,
            NpcLocationId = "test_inn",
            NpcId = "test_innkeeper",
            NpcName = "Martha",
            PlayerCoins = 50
        };

        SceneArchetypeDefinition result = SceneArchetypeCatalog.Generate(
            "service_with_location_access",
            context.Tier,
            context);

        Assert.Equal(4, result.SituationTemplates.Count);
        Assert.Single(result.DependentLocations);
        Assert.Single(result.DependentItems);
        Assert.NotEmpty(result.SituationTemplates[0].ChoiceTemplates);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void ServiceWithLocationAccess_AllTiersGenerateValidStructure(int tier)
    {
        GenerationContext context = new GenerationContext
        {
            Tier = tier,
            NpcPersonality = PersonalityType.MERCANTILE,
            NpcLocationId = "test_inn",
            NpcId = "test_innkeeper",
            NpcName = "Martha",
            PlayerCoins = 50
        };

        SceneArchetypeDefinition result = SceneArchetypeCatalog.Generate(
            "service_with_location_access",
            context.Tier,
            context);

        Assert.Equal(4, result.SituationTemplates.Count);
        Assert.Equal(SpawnPattern.Linear, result.SpawnRules.Pattern);
        Assert.Equal(3, result.SpawnRules.Transitions.Count);
    }

    [Fact]
    public void ServiceWithLocationAccess_Deterministic()
    {
        GenerationContext context = new GenerationContext
        {
            Tier = 2,
            NpcPersonality = PersonalityType.MERCANTILE,
            NpcLocationId = "test_inn",
            NpcId = "test_innkeeper",
            NpcName = "Martha",
            PlayerCoins = 25
        };

        SceneArchetypeDefinition result1 = SceneArchetypeCatalog.Generate(
            "service_with_location_access",
            context.Tier,
            context);

        SceneArchetypeDefinition result2 = SceneArchetypeCatalog.Generate(
            "service_with_location_access",
            context.Tier,
            context);

        Assert.Equal(result1.SituationTemplates.Count, result2.SituationTemplates.Count);
        Assert.Equal(result1.SituationTemplates[0].Id, result2.SituationTemplates[0].Id);
        Assert.Equal(result1.SituationTemplates[0].ChoiceTemplates.Count, result2.SituationTemplates[0].ChoiceTemplates.Count);
        Assert.Equal(result1.DependentLocations[0].TemplateId, result2.DependentLocations[0].TemplateId);
    }
}
