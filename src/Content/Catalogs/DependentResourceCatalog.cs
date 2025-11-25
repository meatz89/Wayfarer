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
///
/// All specs use generic, descriptive names until AI narrative system implemented.
/// When AI narrative comes online, it will regenerate ALL text with full entity context.
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
    /// All specs use generic, descriptive names until AI narrative system implemented.
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
            Name = "Private Room",
            Description = "A private room for lodging.",
            VenueIdSource = VenueIdSource.SameAsBase,
            HexPlacement = HexPlacementStrategy.SameVenue,
            Properties = new List<string> { "sleepingSpace", "restful", "indoor", "private" },
            IsLockedInitially = true,
            UnlockItemTemplateId = "room_key",
            CanInvestigate = false,
            // FAIL-FAST: ALL categorical dimensions REQUIRED (no defaults)
            Privacy = "Private",      // Private room for lodging
            Safety = "Safe",          // Inn rooms are safe spaces
            Activity = "Quiet",       // Bedroom environment
            Purpose = "Dwelling"      // Sleeping/resting space
        };

        DependentItemSpec itemSpec = new DependentItemSpec
        {
            TemplateId = "room_key",
            Name = "Room Key",
            Description = "A key that unlocks access to a private room.",
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
            Name = "Bath Chamber",
            Description = "A private bathing chamber.",
            VenueIdSource = VenueIdSource.SameAsBase,
            HexPlacement = HexPlacementStrategy.SameVenue,
            Properties = new List<string> { "restful", "indoor", "private", "water" },
            IsLockedInitially = true,
            UnlockItemTemplateId = "bath_token",
            CanInvestigate = false,
            // FAIL-FAST: ALL categorical dimensions REQUIRED (no defaults)
            Privacy = "Private",      // Private bathing area
            Safety = "Safe",          // Safe environment
            Activity = "Quiet",       // Restful activity
            Purpose = "Dwelling"      // Personal care space
        };

        DependentItemSpec itemSpec = new DependentItemSpec
        {
            TemplateId = "bath_token",
            Name = "Bath Token",
            Description = "A token granting access to a private bathing chamber.",
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
            Name = "Training Grounds",
            Description = "A private training area.",
            VenueIdSource = VenueIdSource.SameAsBase,
            HexPlacement = HexPlacementStrategy.SameVenue,
            Properties = new List<string> { "outdoor", "private" },
            IsLockedInitially = true,
            UnlockItemTemplateId = "training_pass",
            CanInvestigate = false,
            // FAIL-FAST: ALL categorical dimensions REQUIRED (no defaults)
            Privacy = "Private",      // Private training facility
            Safety = "Neutral",       // Training has inherent risk
            Activity = "Busy",        // Physical training activity
            Purpose = "Work"          // Training/education purpose
        };

        DependentItemSpec itemSpec = new DependentItemSpec
        {
            TemplateId = "training_pass",
            Name = "Training Pass",
            Description = "A pass granting access to a training area.",
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
            Name = "Treatment Room",
            Description = "A private medical treatment room.",
            VenueIdSource = VenueIdSource.SameAsBase,
            HexPlacement = HexPlacementStrategy.SameVenue,
            Properties = new List<string> { "restful", "indoor", "private", "quiet" },
            IsLockedInitially = true,
            UnlockItemTemplateId = "treatment_receipt",
            CanInvestigate = false,
            // FAIL-FAST: ALL categorical dimensions REQUIRED (no defaults)
            Privacy = "Private",      // Private medical room
            Safety = "Safe",          // Healing environment
            Activity = "Quiet",       // Restful recovery
            Purpose = "Dwelling"      // Personal care/recovery space
        };

        DependentItemSpec itemSpec = new DependentItemSpec
        {
            TemplateId = "treatment_receipt",
            Name = "Treatment Receipt",
            Description = "A receipt granting access to a medical treatment room.",
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
            Name = "Workshop",
            Description = "A private workshop for crafting.",
            VenueIdSource = VenueIdSource.SameAsBase,
            HexPlacement = HexPlacementStrategy.SameVenue,
            Properties = new List<string> { "indoor", "private" },
            IsLockedInitially = true,
            UnlockItemTemplateId = "workshop_permit",
            CanInvestigate = false,
            // FAIL-FAST: ALL categorical dimensions REQUIRED (no defaults)
            Privacy = "Private",      // Private workspace
            Safety = "Neutral",       // Tools/hazards present
            Activity = "Busy",        // Active crafting work
            Purpose = "Work"          // Manufacturing/crafting purpose
        };

        DependentItemSpec itemSpec = new DependentItemSpec
        {
            TemplateId = "workshop_permit",
            Name = "Workshop Permit",
            Description = "A permit granting access to a private workshop.",
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
            Name = "Study Room",
            Description = "A quiet study room.",
            VenueIdSource = VenueIdSource.SameAsBase,
            HexPlacement = HexPlacementStrategy.SameVenue,
            Properties = new List<string> { "indoor", "private", "quiet", "restful" },
            IsLockedInitially = true,
            UnlockItemTemplateId = "library_pass",
            CanInvestigate = false,
            // FAIL-FAST: ALL categorical dimensions REQUIRED (no defaults)
            Privacy = "Private",      // Private study space
            Safety = "Safe",          // Safe reading environment
            Activity = "Quiet",       // Quiet intellectual work
            Purpose = "Education"     // Learning/study purpose
        };

        DependentItemSpec itemSpec = new DependentItemSpec
        {
            TemplateId = "library_pass",
            Name = "Library Pass",
            Description = "A pass granting access to a private study room.",
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
