using Wayfarer.Content.Catalogues;
using Wayfarer.GameState.Enums;

namespace Wayfarer.GameState;

public abstract class PhysicalServiceType : ServiceType
{
    public override SceneArchetypeDefinition GenerateMultiSituationArc(
        int tier,
        NPC contextNPC,
        Location contextLocation,
        Player contextPlayer)
    {
        string negotiateSitId = $"{Id}_negotiate";
        string accessSitId = $"{Id}_access";
        string serviceSitId = $"{Id}_service";
        string departSitId = $"{Id}_depart";

        SituationArchetype negotiateArchetype = DetermineNegotiationArchetype(contextNPC, contextLocation, contextPlayer);
        NarrativeHints negotiateHints = GenerateNegotiationHints(contextNPC, contextLocation, contextPlayer);
        List<ChoiceTemplate> negotiateChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(negotiateArchetype, negotiateSitId);

        string negotiateName = $"Secure {DisplayName}";
        string accessName = "Enter";
        string serviceName = DisplayName;
        string departName = "Leave";

        SituationTemplate negotiateSituation = new SituationTemplate
        {
            Id = negotiateSitId,
            Name = negotiateName,
            Type = SituationType.Normal,
            NarrativeTemplate = null,
            ChoiceTemplates = negotiateChoices,
            Priority = 100,
            NarrativeHints = negotiateHints,
            RequiredLocationId = contextNPC?.Location?.Id,
            RequiredNpcId = contextNPC?.ID
        };

        SituationTemplate accessSituation = new SituationTemplate
        {
            Id = accessSitId,
            Name = accessName,
            Type = SituationType.Normal,
            NarrativeTemplate = null,
            ChoiceTemplates = new List<ChoiceTemplate>(),
            Priority = 90,
            NarrativeHints = new NarrativeHints
            {
                Tone = "descriptive",
                Theme = "location_access",
                Context = $"{Id}_entry"
            },
            AutoProgressRewards = new ChoiceReward
            {
                TimeSegments = 1
            },
            RequiredLocationId = "generated:private_room",
            RequiredNpcId = null
        };

        SituationTemplate serviceSituation = new SituationTemplate
        {
            Id = serviceSitId,
            Name = serviceName,
            Type = SituationType.Normal,
            NarrativeTemplate = null,
            ChoiceTemplates = new List<ChoiceTemplate>(),
            Priority = 80,
            NarrativeHints = new NarrativeHints
            {
                Tone = "restorative",
                Theme = $"{Id}_experience",
                Context = $"{Id}_provision"
            },
            AutoProgressRewards = GenerateRewards(tier),
            RequiredLocationId = "generated:private_room",
            RequiredNpcId = null
        };

        SituationTemplate departureSituation = new SituationTemplate
        {
            Id = departSitId,
            Name = departName,
            Type = SituationType.Normal,
            NarrativeTemplate = null,
            ChoiceTemplates = new List<ChoiceTemplate>(),
            Priority = 70,
            NarrativeHints = new NarrativeHints
            {
                Tone = "conclusive",
                Theme = "departure",
                Context = $"{Id}_conclusion"
            },
            AutoProgressRewards = new ChoiceReward
            {
                TimeSegments = 1,
                ItemsToRemove = new List<string> { "generated:room_key" },
                LocationsToLock = new List<string> { "generated:private_room" }
            },
            RequiredLocationId = contextNPC?.Location?.Id,
            RequiredNpcId = contextNPC?.ID
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationId = negotiateSitId,
            Transitions = new List<SituationTransition>
            {
                new SituationTransition
                {
                    SourceSituationId = negotiateSitId,
                    DestinationSituationId = accessSitId,
                    Condition = TransitionCondition.Always
                },
                new SituationTransition
                {
                    SourceSituationId = accessSitId,
                    DestinationSituationId = serviceSitId,
                    Condition = TransitionCondition.Always
                },
                new SituationTransition
                {
                    SourceSituationId = serviceSitId,
                    DestinationSituationId = departSitId,
                    Condition = TransitionCondition.Always
                }
            }
        };

        DependentLocationSpec privateRoomSpec = new DependentLocationSpec
        {
            TemplateId = "private_room",
            NamePattern = $"{{npc_name}}'s {{service_type}} Room",
            DescriptionPattern = $"A private room where {{npc_name}} provides {{service_type}} services.",
            VenueIdSource = VenueIdSource.SameAsBase,
            HexPlacement = HexPlacementStrategy.SameVenue,
            Properties = new List<string> { "sleepingSpace", "restful", "indoor", "private" },
            IsLockedInitially = true,
            UnlockItemTemplateId = "room_key",
            CanInvestigate = false
        };

        DependentItemSpec roomKeySpec = new DependentItemSpec
        {
            TemplateId = "room_key",
            NamePattern = "Room Key",
            DescriptionPattern = "A key that unlocks access to {{npc_name}}'s private {{service_type}} room.",
            Categories = new List<ItemCategory> { ItemCategory.Special_Access },
            Weight = 1,
            BuyPrice = 0,
            SellPrice = 0,
            AddToInventoryOnCreation = false,
            SpawnLocationTemplateId = null
        };

        return new SceneArchetypeDefinition
        {
            SituationTemplates = new List<SituationTemplate>
            {
                negotiateSituation,
                accessSituation,
                serviceSituation,
                departureSituation
            },
            SpawnRules = spawnRules,
            DependentLocations = new List<DependentLocationSpec> { privateRoomSpec },
            DependentItems = new List<DependentItemSpec> { roomKeySpec }
        };
    }

    public override SceneArchetypeDefinition GenerateSingleSituation(
        int tier,
        NPC contextNPC,
        Location contextLocation,
        Player contextPlayer)
    {
        string situationId = $"{Id}_situation";

        SituationArchetype archetype = SituationArchetypeCatalog.GetArchetype(Id);
        List<ChoiceTemplate> choices = SituationArchetypeCatalog.GenerateChoiceTemplates(archetype, situationId);

        SituationTemplate situation = new SituationTemplate
        {
            Id = situationId,
            Type = SituationType.Normal,
            NarrativeTemplate = null,
            ChoiceTemplates = choices,
            Priority = 100,
            NarrativeHints = GenerateContextualHints(Id, contextNPC, contextLocation, contextPlayer)
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Standalone,
            InitialSituationId = situationId,
            Transitions = new List<SituationTransition>()
        };

        return new SceneArchetypeDefinition
        {
            SituationTemplates = new List<SituationTemplate> { situation },
            SpawnRules = spawnRules
        };
    }

    protected virtual SituationArchetype DetermineNegotiationArchetype(NPC contextNPC, Location contextLocation, Player contextPlayer)
    {
        if (contextNPC.PersonalityType == PersonalityType.DEVOTED)
        {
            return SituationArchetypeCatalog.GetArchetype("social_maneuvering");
        }

        if (contextNPC.PersonalityType == PersonalityType.MERCANTILE)
        {
            return SituationArchetypeCatalog.GetArchetype("negotiation");
        }

        return SituationArchetypeCatalog.GetArchetype("service_transaction");
    }

    protected virtual NarrativeHints GenerateContextualHints(
        string situationArchetypeId,
        NPC contextNPC,
        Location contextLocation,
        Player contextPlayer)
    {
        return new NarrativeHints
        {
            Tone = "standard",
            Theme = "service",
            Context = situationArchetypeId
        };
    }
}
