using Wayfarer.GameState;
using Wayfarer.GameState.Enums;

namespace Wayfarer.Content.Catalogues;

/// <summary>
/// ⚠️ PARSE-TIME ONLY CATALOGUE ⚠️
///
/// Procedurally generates dependent location and item specifications based on
/// service activity type. Enables reusable scene archetypes to work across
/// multiple service domains without hardcoding resource specs.
///
/// CATALOGUE PATTERN COMPLIANCE:
/// - Called ONLY from SceneArchetypeCatalog at PARSE TIME
/// - NEVER called from facades, managers, or runtime code
/// - NEVER imported except in catalog classes
/// - Generates specs that SceneInstantiator uses at scene finalization
/// - Translation happens ONCE at game initialization
///
/// ARCHITECTURE:
/// SceneArchetype specifies activity type → DependentResourceCatalog generates specs
/// → SceneInstantiator uses specs to generate actual LocationDTOs/ItemDTOs
/// → Runtime queries GameWorld (pre-populated), NO catalogue calls
///
/// Generated specs include:
/// - DependentLocationSpec: Access-controlled location with activity-appropriate properties
/// - DependentItemSpec: Access item (key, token, pass) that unlocks location
/// </summary>
public static class DependentResourceCatalog
{
    /// <summary>
    /// Result container for dependent resource generation
    /// Contains both location and item specs for activity
    /// </summary>
    public class DependentResources
    {
        public DependentLocationSpec LocationSpec { get; set; }
        public DependentItemSpec ItemSpec { get; set; }
    }

    /// <summary>
    /// Generate dependent location and item specs for service activity type
    ///
    /// Returns complete resource pair:
    /// - Location: Private space with activity-appropriate properties
    /// - Item: Access token that unlocks location
    ///
    /// All specs use placeholder patterns for NPC name replacement at finalization.
    /// </summary>
    public static DependentResources GenerateForActivity(ServiceActivityType activityType)
    {
        return activityType switch
        {
            ServiceActivityType.Lodging => GenerateLodgingResources(),
            ServiceActivityType.Bathing => GenerateBathingResources(),
            ServiceActivityType.Training => GenerateTrainingResources(),
            ServiceActivityType.Healing => GenerateHealingResources(),
            ServiceActivityType.Crafting => GenerateCraftingResources(),
            ServiceActivityType.Studying => GenerateStudyingResources(),
            _ => throw new InvalidDataException($"Unknown ServiceActivityType: {activityType}")
        };
    }

    private static DependentResources GenerateLodgingResources()
    {
        DependentLocationSpec locationSpec = new DependentLocationSpec
        {
            TemplateId = "private_room",
            NamePattern = "{NPCName}'s Lodging Room",
            DescriptionPattern = "A private room where {NPCName} provides lodging services.",
            VenueIdSource = VenueIdSource.SameAsBase,
            HexPlacement = HexPlacementStrategy.SameVenue,
            Properties = new List<string> { "sleepingSpace", "restful", "indoor", "private" },
            IsLockedInitially = true,
            UnlockItemTemplateId = "room_key",
            CanInvestigate = false
        };

        DependentItemSpec itemSpec = new DependentItemSpec
        {
            TemplateId = "room_key",
            NamePattern = "Room Key",
            DescriptionPattern = "A key that unlocks access to {NPCName}'s private lodging room.",
            Categories = new List<ItemCategory> { ItemCategory.Special_Access },
            Weight = 1,
            BuyPrice = 0,
            SellPrice = 0,
            AddToInventoryOnCreation = false,
            SpawnLocationTemplateId = null
        };

        return new DependentResources
        {
            LocationSpec = locationSpec,
            ItemSpec = itemSpec
        };
    }

    private static DependentResources GenerateBathingResources()
    {
        DependentLocationSpec locationSpec = new DependentLocationSpec
        {
            TemplateId = "bath_chamber",
            NamePattern = "{NPCName}'s Bathing Chamber",
            DescriptionPattern = "A private bathing chamber maintained by {NPCName}.",
            VenueIdSource = VenueIdSource.SameAsBase,
            HexPlacement = HexPlacementStrategy.SameVenue,
            Properties = new List<string> { "restful", "indoor", "private", "water" },
            IsLockedInitially = true,
            UnlockItemTemplateId = "bath_token",
            CanInvestigate = false
        };

        DependentItemSpec itemSpec = new DependentItemSpec
        {
            TemplateId = "bath_token",
            NamePattern = "Bath Token",
            DescriptionPattern = "A token granting access to {NPCName}'s private bathing facilities.",
            Categories = new List<ItemCategory> { ItemCategory.Special_Access },
            Weight = 1,
            BuyPrice = 0,
            SellPrice = 0,
            AddToInventoryOnCreation = false,
            SpawnLocationTemplateId = null
        };

        return new DependentResources
        {
            LocationSpec = locationSpec,
            ItemSpec = itemSpec
        };
    }

    private static DependentResources GenerateTrainingResources()
    {
        DependentLocationSpec locationSpec = new DependentLocationSpec
        {
            TemplateId = "training_yard",
            NamePattern = "{NPCName}'s Training Grounds",
            DescriptionPattern = "A private training area supervised by {NPCName}.",
            VenueIdSource = VenueIdSource.SameAsBase,
            HexPlacement = HexPlacementStrategy.SameVenue,
            Properties = new List<string> { "outdoor", "private" },
            IsLockedInitially = true,
            UnlockItemTemplateId = "training_pass",
            CanInvestigate = false
        };

        DependentItemSpec itemSpec = new DependentItemSpec
        {
            TemplateId = "training_pass",
            NamePattern = "Training Pass",
            DescriptionPattern = "A pass granting access to {NPCName}'s training grounds.",
            Categories = new List<ItemCategory> { ItemCategory.Special_Access },
            Weight = 1,
            BuyPrice = 0,
            SellPrice = 0,
            AddToInventoryOnCreation = false,
            SpawnLocationTemplateId = null
        };

        return new DependentResources
        {
            LocationSpec = locationSpec,
            ItemSpec = itemSpec
        };
    }

    private static DependentResources GenerateHealingResources()
    {
        DependentLocationSpec locationSpec = new DependentLocationSpec
        {
            TemplateId = "treatment_room",
            NamePattern = "{NPCName}'s Treatment Room",
            DescriptionPattern = "A private medical chamber where {NPCName} provides healing services.",
            VenueIdSource = VenueIdSource.SameAsBase,
            HexPlacement = HexPlacementStrategy.SameVenue,
            Properties = new List<string> { "restful", "indoor", "private", "quiet" },
            IsLockedInitially = true,
            UnlockItemTemplateId = "treatment_receipt",
            CanInvestigate = false
        };

        DependentItemSpec itemSpec = new DependentItemSpec
        {
            TemplateId = "treatment_receipt",
            NamePattern = "Treatment Receipt",
            DescriptionPattern = "A receipt granting access to {NPCName}'s medical treatment room.",
            Categories = new List<ItemCategory> { ItemCategory.Special_Access },
            Weight = 1,
            BuyPrice = 0,
            SellPrice = 0,
            AddToInventoryOnCreation = false,
            SpawnLocationTemplateId = null
        };

        return new DependentResources
        {
            LocationSpec = locationSpec,
            ItemSpec = itemSpec
        };
    }

    private static DependentResources GenerateCraftingResources()
    {
        DependentLocationSpec locationSpec = new DependentLocationSpec
        {
            TemplateId = "workshop",
            NamePattern = "{NPCName}'s Workshop",
            DescriptionPattern = "A private workshop where {NPCName} allows crafting work.",
            VenueIdSource = VenueIdSource.SameAsBase,
            HexPlacement = HexPlacementStrategy.SameVenue,
            Properties = new List<string> { "indoor", "private" },
            IsLockedInitially = true,
            UnlockItemTemplateId = "workshop_permit",
            CanInvestigate = false
        };

        DependentItemSpec itemSpec = new DependentItemSpec
        {
            TemplateId = "workshop_permit",
            NamePattern = "Workshop Permit",
            DescriptionPattern = "A permit granting access to {NPCName}'s private workshop.",
            Categories = new List<ItemCategory> { ItemCategory.Special_Access },
            Weight = 1,
            BuyPrice = 0,
            SellPrice = 0,
            AddToInventoryOnCreation = false,
            SpawnLocationTemplateId = null
        };

        return new DependentResources
        {
            LocationSpec = locationSpec,
            ItemSpec = itemSpec
        };
    }

    private static DependentResources GenerateStudyingResources()
    {
        DependentLocationSpec locationSpec = new DependentLocationSpec
        {
            TemplateId = "study_room",
            NamePattern = "{NPCName}'s Study Room",
            DescriptionPattern = "A quiet study room maintained by {NPCName}.",
            VenueIdSource = VenueIdSource.SameAsBase,
            HexPlacement = HexPlacementStrategy.SameVenue,
            Properties = new List<string> { "indoor", "private", "quiet", "restful" },
            IsLockedInitially = true,
            UnlockItemTemplateId = "library_pass",
            CanInvestigate = false
        };

        DependentItemSpec itemSpec = new DependentItemSpec
        {
            TemplateId = "library_pass",
            NamePattern = "Library Pass",
            DescriptionPattern = "A pass granting access to {NPCName}'s private study room.",
            Categories = new List<ItemCategory> { ItemCategory.Special_Access },
            Weight = 1,
            BuyPrice = 0,
            SellPrice = 0,
            AddToInventoryOnCreation = false,
            SpawnLocationTemplateId = null
        };

        return new DependentResources
        {
            LocationSpec = locationSpec,
            ItemSpec = itemSpec
        };
    }
}
