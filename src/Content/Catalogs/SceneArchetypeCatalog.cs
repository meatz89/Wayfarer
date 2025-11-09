using Wayfarer.GameState;
using Wayfarer.GameState.Enums;
using Wayfarer.Content.Generators;

namespace Wayfarer.Content.Catalogues;

/// <summary>
/// ⚠️ PARSE-TIME ONLY CATALOGUE ⚠️
///
/// Generates complete multi-situation Scene arcs from archetype IDs.
/// Creates entity-agnostic scene structures reusable across different NPCs/Locations.
///
/// CATALOGUE PATTERN COMPLIANCE:
/// - Called ONLY from SceneTemplateParser at PARSE TIME
/// - NEVER called from facades, managers, or runtime code
/// - NEVER imported except in parser classes
/// - Generates SceneArchetypeDefinition that parser embeds in SceneTemplate
/// - Translation happens ONCE at game initialization
///
/// ARCHITECTURE:
/// Parser reads sceneArchetypeId from JSON → Calls GetSceneArchetype() → Receives SceneArchetypeDefinition
/// → Parser embeds SituationTemplates and SpawnRules into SceneTemplate
/// → For each SituationTemplate with archetypeId: Parser calls SituationArchetypeCatalog to generate ChoiceTemplates
/// → Runtime queries GameWorld.SceneTemplates (pre-populated), NO catalogue calls
///
/// SCENE ARCHETYPE LIBRARY (3 patterns):
/// 1. service_with_location_access - 4-situation arc: negotiate → access → service → depart
///    Used for: lodging, bathing, healing, storage, training
///
/// 2. transaction_sequence - 3-situation arc: browse → negotiate → complete
///    Used for: shopping, selling, trading
///
/// 3. gatekeeper_sequence - 2-situation arc: confront → pass
///    Used for: checkpoints, restricted areas, authority challenges
///
/// Each archetype generates:
/// - Multiple SituationTemplates (with archetypeId for SituationArchetypeCatalog)
/// - SituationSpawnRules (transitions between situations)
/// - Purely categorical (tier-based scaling, no entity context)
/// </summary>
public static class SceneArchetypeCatalog
{
    /// <summary>
    /// UNIFIED GENERATION ENTRY POINT (HIGHLANDER: ONE way to generate ALL archetypes)
    ///
    /// Generate scene archetype definition by ID with GenerationContext for categorical property reading
    /// Called at parse time to generate context-aware multi-situation structure
    /// Reads GenerationContext properties (NPC personality, Location properties, Player state)
    /// Throws InvalidDataException on unknown archetype ID (fail fast)
    ///
    /// ALL scene generation goes through this ONE method.
    /// </summary>
    public static SceneArchetypeDefinition Generate(
        string archetypeId,
        int tier,
        GenerationContext context)
    {
        return archetypeId?.ToLowerInvariant() switch
        {
            "service_with_location_access" => GenerateServiceWithLocationAccess(tier, context),
            "service_simplified" => GenerateServiceSimplified(tier, context),
            "transaction_sequence" => GenerateTransactionSequence(tier, context),
            "gatekeeper_sequence" => GenerateGatekeeperSequence(tier, context),
            "consequence_reflection" => GenerateConsequenceReflection(tier, context),
            "inn_crisis_escalation" => GenerateInnCrisisEscalation(tier, context),

            // SINGLE-SITUATION SCENE ARCHETYPES (Standalone pattern, one situation)
            "single_negotiation" => GenerateSingleSituationScene("negotiation", tier, context, "negotiation", "diplomatic"),
            "single_confrontation" => GenerateSingleSituationScene("confrontation", tier, context, "authority_challenge", "tense"),
            "single_investigation" => GenerateSingleSituationScene("investigation", tier, context, "discovery", "analytical"),
            "single_social_maneuvering" => GenerateSingleSituationScene("social_maneuvering", tier, context, "social_navigation", "subtle"),
            "single_crisis" => GenerateSingleSituationScene("crisis", tier, context, "emergency", "urgent"),
            "single_service_transaction" => GenerateSingleSituationScene("service_transaction", tier, context, "service_request", "transactional"),
            "single_access_control" => GenerateSingleSituationScene("access_control", tier, context, "restricted_access", "authoritative"),

            _ => throw new InvalidDataException($"Unknown scene archetype ID: '{archetypeId}'. Valid archetypes: service_with_location_access, service_simplified, transaction_sequence, gatekeeper_sequence, consequence_reflection, inn_crisis_escalation, single_negotiation, single_confrontation, single_investigation, single_social_maneuvering, single_crisis, single_service_transaction, single_access_control")
        };
    }

    /// <summary>
    /// SERVICE_WITH_LOCATION_ACCESS archetype
    ///
    /// When Used: Services requiring private space (lodging, bathing, healing, storage, training)
    /// Situation Count: 4
    /// Pattern: Linear (negotiate → access → service → depart)
    ///
    /// Situation 1 - Negotiate: Player arranges service
    ///   - Archetype: social_maneuvering (DEVOTED) or service_transaction (MERCANTILE)
    ///   - Choices: 4 (stat-gated, money, challenge, fallback)
    ///   - Rewards: Unlock room, grant key on successful negotiation
    ///
    /// Situation 2 - Access: Inspect and prepare private space
    ///   - Archetype: entering_private_space
    ///   - Choices: 4 (stat-gated, money, challenge, fallback)
    ///   - Action: Assess room quality and prepare for rest
    ///
    /// Situation 3 - Service: Receive service benefit
    ///   - Archetype: rest_preparation
    ///   - Choices: 4 (stat-gated, money, challenge, fallback)
    ///   - Rewards: Tier-scaled health/stamina restoration + time advancement
    ///
    /// Situation 4 - Depart: Organize departure and cleanup
    ///   - Archetype: departing_private_space
    ///   - Choices: 4 (stat-gated, money, challenge, fallback)
    ///   - Action: Pack belongings, return key, prepare for journey
    ///   - Rewards: Remove key, lock location, return to base
    ///
    /// Creates dependent resources:
    ///   - generated:private_room (unlocked by room_key)
    ///   - generated:room_key (removed on departure)
    /// </summary>
    private static SceneArchetypeDefinition GenerateServiceWithLocationAccess(int tier, GenerationContext context)
    {
        string serviceId = "secure_lodging";
        string negotiateSitId = $"{serviceId}_negotiate";
        string accessSitId = $"{serviceId}_access";
        string serviceSitId = $"{serviceId}_service";
        string departSitId = $"{serviceId}_depart";

        SituationArchetype negotiateArchetype = SituationArchetypeCatalog.GetArchetype("service_negotiation");
        NarrativeHints negotiateHints = GenerateServiceNegotiationHints(context, serviceId);
        List<ChoiceTemplate> negotiateChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(negotiateArchetype, negotiateSitId, context);

        // Add unlock rewards to negotiate choices (route by PathType, NOT ID string matching)
        // InstantSuccess: Unlock room and grant key immediately
        // Challenge: Unlock room and grant key on success only
        // Fallback: No rewards (player leaves without service)
        List<ChoiceTemplate> enrichedChoices = new List<ChoiceTemplate>();
        foreach (ChoiceTemplate choice in negotiateChoices)
        {
            switch (choice.PathType)
            {
                case ChoicePathType.InstantSuccess:
                    // Instant success paths: unlock room and grant key immediately
                    enrichedChoices.Add(new ChoiceTemplate
                    {
                        Id = choice.Id,
                        PathType = choice.PathType,
                        ActionTextTemplate = choice.ActionTextTemplate,
                        RequirementFormula = choice.RequirementFormula,
                        CostTemplate = choice.CostTemplate,
                        RewardTemplate = new ChoiceReward
                        {
                            LocationsToUnlock = new List<string> { "generated:private_room" },
                            ItemIds = new List<string> { "generated:room_key" }
                        },
                        ActionType = choice.ActionType,
                        ChallengeId = choice.ChallengeId,
                        ChallengeType = choice.ChallengeType,
                        NavigationPayload = choice.NavigationPayload
                    });
                    break;

                case ChoicePathType.Challenge:
                    // Challenge path: unlock room and grant key on success only
                    enrichedChoices.Add(new ChoiceTemplate
                    {
                        Id = choice.Id,
                        PathType = choice.PathType,
                        ActionTextTemplate = choice.ActionTextTemplate,
                        RequirementFormula = choice.RequirementFormula,
                        CostTemplate = choice.CostTemplate,
                        RewardTemplate = choice.RewardTemplate,
                        OnSuccessReward = new ChoiceReward
                        {
                            LocationsToUnlock = new List<string> { "generated:private_room" },
                            ItemIds = new List<string> { "generated:room_key" }
                        },
                        ActionType = choice.ActionType,
                        ChallengeId = choice.ChallengeId,
                        ChallengeType = choice.ChallengeType,
                        NavigationPayload = choice.NavigationPayload
                    });
                    break;

                case ChoicePathType.Fallback:
                default:
                    // Fallback path: no rewards (player leaves without securing lodging)
                    enrichedChoices.Add(choice);
                    break;
            }
        }

        string negotiateName = "Secure Lodging";
        string accessName = "Enter";
        string serviceName = "Rest";
        string departName = "Leave";

        SituationTemplate negotiateSituation = new SituationTemplate
        {
            Id = negotiateSitId,
            Name = negotiateName,
            Type = SituationType.Normal,
            NarrativeTemplate = null,
            ChoiceTemplates = enrichedChoices,
            Priority = 100,
            NarrativeHints = negotiateHints,
            RequiredLocationId = context.NpcLocationId,
            RequiredNpcId = context.NpcId
        };

        SituationArchetype accessArchetype = SituationArchetypeCatalog.GetArchetype("entering_private_space");
        List<ChoiceTemplate> accessChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(accessArchetype, accessSitId);

        SituationTemplate accessSituation = new SituationTemplate
        {
            Id = accessSitId,
            Name = accessName,
            Type = SituationType.Normal,
            NarrativeTemplate = null,
            ChoiceTemplates = accessChoices,
            Priority = 90,
            NarrativeHints = new NarrativeHints
            {
                Tone = "descriptive",
                Theme = "location_access",
                Context = $"{serviceId}_entry"
            },
            RequiredLocationId = "generated:private_room",
            RequiredNpcId = null
        };

        SituationArchetype serviceArchetype = SituationArchetypeCatalog.GetArchetype("service_execution_rest");
        List<ChoiceTemplate> enrichedServiceChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(serviceArchetype, serviceSitId, context);

        SituationTemplate serviceSituation = new SituationTemplate
        {
            Id = serviceSitId,
            Name = serviceName,
            Type = SituationType.Normal,
            NarrativeTemplate = null,
            ChoiceTemplates = enrichedServiceChoices,
            Priority = 80,
            NarrativeHints = new NarrativeHints
            {
                Tone = "restorative",
                Theme = $"{serviceId}_experience",
                Context = $"{serviceId}_provision"
            },
            RequiredLocationId = "generated:private_room",
            RequiredNpcId = null
        };

        SituationArchetype departArchetype = SituationArchetypeCatalog.GetArchetype("service_departure");
        List<ChoiceTemplate> departChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(departArchetype, departSitId, context);

        // Enrich choices with departure cleanup (return key, lock room)
        // Merge archetype rewards (buffs) with service-specific cleanup
        List<ChoiceTemplate> enrichedDepartChoices = new List<ChoiceTemplate>();
        foreach (ChoiceTemplate choice in departChoices)
        {
            ChoiceReward mergedReward = new ChoiceReward
            {
                ItemsToRemove = new List<string> { "generated:room_key" },
                LocationsToLock = new List<string> { "generated:private_room" },
                StateApplications = choice.RewardTemplate?.StateApplications ?? new List<StateApplication>()
            };

            enrichedDepartChoices.Add(new ChoiceTemplate
            {
                Id = choice.Id,
                ActionTextTemplate = choice.ActionTextTemplate,
                RequirementFormula = choice.RequirementFormula,
                CostTemplate = choice.CostTemplate,
                RewardTemplate = mergedReward,
                ActionType = choice.ActionType,
                ChallengeId = choice.ChallengeId,
                ChallengeType = choice.ChallengeType,
                NavigationPayload = choice.NavigationPayload
            });
        }

        SituationTemplate departureSituation = new SituationTemplate
        {
            Id = departSitId,
            Name = departName,
            Type = SituationType.Normal,
            NarrativeTemplate = null,
            ChoiceTemplates = enrichedDepartChoices,
            Priority = 70,
            NarrativeHints = new NarrativeHints
            {
                Tone = "conclusive",
                Theme = "departure",
                Context = $"{serviceId}_conclusion"
            },
            RequiredLocationId = context.NpcLocationId,
            RequiredNpcId = context.NpcId
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
            NamePattern = "{NPCName}'s Lodging Room",
            DescriptionPattern = "A private room where {NPCName} provides lodging services.",
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
            DescriptionPattern = "A key that unlocks access to {NPCName}'s private lodging room.",
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

    /// <summary>
    /// SERVICE_SIMPLIFIED archetype
    ///
    /// When Used: Simplified 3-situation service flow (tutorial, straightforward service transactions)
    /// Situation Count: 3
    /// Pattern: Linear (negotiate → service → depart)
    ///
    /// Situation 1 - Negotiate: Player arranges service access
    ///   - Archetype: service_negotiation (context-aware scaling)
    ///   - Choices: 4 (stat-gated, money, challenge, fallback)
    ///   - Rewards: Unlock room, grant key on successful negotiation
    ///
    /// Situation 2 - Service: Receive service benefit
    ///   - Archetype: service_execution_rest (context-aware scaling)
    ///   - Choices: 4 (balanced, physical, mental, special)
    ///   - Rewards: Comfort-scaled restoration + time advancement to next morning
    ///
    /// Situation 3 - Depart: Leave and cleanup
    ///   - Archetype: service_departure (context-aware scaling)
    ///   - Choices: 2 (immediate, careful)
    ///   - Rewards: Remove key, lock room, optional service-type buff
    ///
    /// Creates dependent resources:
    ///   - generated:private_room (unlocked by room_key)
    ///   - generated:room_key (removed on departure)
    ///
    /// Difference from service_with_location_access:
    ///   - Skips "Access" situation (inspection/preparation)
    ///   - 3 situations instead of 4
    ///   - More streamlined for tutorial/simple service flows
    /// </summary>
    private static SceneArchetypeDefinition GenerateServiceSimplified(int tier, GenerationContext context)
    {
        string serviceId = "service_simplified";
        string negotiateSitId = $"{serviceId}_negotiate";
        string serviceSitId = $"{serviceId}_service";
        string departSitId = $"{serviceId}_depart";

        // SITUATION 1: NEGOTIATE (context-aware scaling)
        SituationArchetype negotiateArchetype = SituationArchetypeCatalog.GetArchetype("service_negotiation");
        NarrativeHints negotiateHints = GenerateServiceNegotiationHints(context, serviceId);
        List<ChoiceTemplate> negotiateChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(negotiateArchetype, negotiateSitId, context);

        // Enrich with service-specific rewards (route by PathType, NOT ID string matching)
        // InstantSuccess: Unlock room and grant key immediately
        // Challenge: Unlock room and grant key on success only
        // Fallback: No rewards (player leaves without service)
        List<ChoiceTemplate> enrichedNegotiateChoices = new List<ChoiceTemplate>();
        foreach (ChoiceTemplate choice in negotiateChoices)
        {
            switch (choice.PathType)
            {
                case ChoicePathType.InstantSuccess:
                    // Instant success paths: unlock room and grant key immediately
                    enrichedNegotiateChoices.Add(new ChoiceTemplate
                    {
                        Id = choice.Id,
                        PathType = choice.PathType,
                        ActionTextTemplate = choice.ActionTextTemplate,
                        RequirementFormula = choice.RequirementFormula,
                        CostTemplate = choice.CostTemplate,
                        RewardTemplate = new ChoiceReward
                        {
                            LocationsToUnlock = new List<string> { "generated:private_room" },
                            ItemIds = new List<string> { "generated:room_key" }
                        },
                        ActionType = choice.ActionType,
                        ChallengeId = choice.ChallengeId,
                        ChallengeType = choice.ChallengeType,
                        NavigationPayload = choice.NavigationPayload
                    });
                    break;

                case ChoicePathType.Challenge:
                    // Challenge path: unlock room and grant key on success only
                    enrichedNegotiateChoices.Add(new ChoiceTemplate
                    {
                        Id = choice.Id,
                        PathType = choice.PathType,
                        ActionTextTemplate = choice.ActionTextTemplate,
                        RequirementFormula = choice.RequirementFormula,
                        CostTemplate = choice.CostTemplate,
                        RewardTemplate = choice.RewardTemplate,
                        OnSuccessReward = new ChoiceReward
                        {
                            LocationsToUnlock = new List<string> { "generated:private_room" },
                            ItemIds = new List<string> { "generated:room_key" }
                        },
                        ActionType = choice.ActionType,
                        ChallengeId = choice.ChallengeId,
                        ChallengeType = choice.ChallengeType,
                        NavigationPayload = choice.NavigationPayload
                    });
                    break;

                case ChoicePathType.Fallback:
                default:
                    // Fallback path: no rewards (player leaves without service)
                    enrichedNegotiateChoices.Add(choice);
                    break;
            }
        }

        SituationTemplate negotiateSituation = new SituationTemplate
        {
            Id = negotiateSitId,
            Name = "Secure Lodging",
            Type = SituationType.Normal,
            NarrativeTemplate = null,
            ChoiceTemplates = enrichedNegotiateChoices,
            Priority = 100,
            NarrativeHints = negotiateHints,
            RequiredLocationId = context.NpcLocationId,
            RequiredNpcId = context.NpcId
        };

        // SITUATION 2: SERVICE (context-aware scaling, complete rewards from archetype)
        SituationArchetype serviceArchetype = SituationArchetypeCatalog.GetArchetype("service_execution_rest");
        List<ChoiceTemplate> serviceChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(serviceArchetype, serviceSitId, context);

        SituationTemplate serviceSituation = new SituationTemplate
        {
            Id = serviceSitId,
            Name = "Rest",
            Type = SituationType.Normal,
            NarrativeTemplate = null,
            ChoiceTemplates = serviceChoices,
            Priority = 90,
            NarrativeHints = new NarrativeHints
            {
                Tone = "restorative",
                Theme = "rest",
                Context = $"{serviceId}_provision"
            },
            RequiredLocationId = "generated:private_room",
            RequiredNpcId = null
        };

        // SITUATION 3: DEPART (context-aware scaling, merge archetype buffs with cleanup)
        SituationArchetype departArchetype = SituationArchetypeCatalog.GetArchetype("service_departure");
        List<ChoiceTemplate> departChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(departArchetype, departSitId, context);

        // Enrich with service-specific cleanup (return key, lock room)
        List<ChoiceTemplate> enrichedDepartChoices = new List<ChoiceTemplate>();
        foreach (ChoiceTemplate choice in departChoices)
        {
            ChoiceReward mergedReward = new ChoiceReward
            {
                ItemsToRemove = new List<string> { "generated:room_key" },
                LocationsToLock = new List<string> { "generated:private_room" },
                StateApplications = choice.RewardTemplate?.StateApplications ?? new List<StateApplication>()
            };

            enrichedDepartChoices.Add(new ChoiceTemplate
            {
                Id = choice.Id,
                ActionTextTemplate = choice.ActionTextTemplate,
                RequirementFormula = choice.RequirementFormula,
                CostTemplate = choice.CostTemplate,
                RewardTemplate = mergedReward,
                ActionType = choice.ActionType,
                ChallengeId = choice.ChallengeId,
                ChallengeType = choice.ChallengeType,
                NavigationPayload = choice.NavigationPayload
            });
        }

        SituationTemplate departureSituation = new SituationTemplate
        {
            Id = departSitId,
            Name = "Leave",
            Type = SituationType.Normal,
            NarrativeTemplate = null,
            ChoiceTemplates = enrichedDepartChoices,
            Priority = 80,
            NarrativeHints = new NarrativeHints
            {
                Tone = "conclusive",
                Theme = "departure",
                Context = $"{serviceId}_conclusion"
            },
            RequiredLocationId = "generated:private_room",
            RequiredNpcId = null
        };

        // Linear transitions: Negotiate → Service → Depart
        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationId = negotiateSitId,
            Transitions = new List<SituationTransition>
            {
                new SituationTransition
                {
                    SourceSituationId = negotiateSitId,
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

        // Dependent resources
        DependentLocationSpec privateRoomSpec = new DependentLocationSpec
        {
            TemplateId = "private_room",
            NamePattern = "{NPCName}'s Room",
            DescriptionPattern = "A private room at {NPCName}'s establishment.",
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
            DescriptionPattern = "A key to {NPCName}'s private room.",
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
                serviceSituation,
                departureSituation
            },
            SpawnRules = spawnRules,
            DependentLocations = new List<DependentLocationSpec> { privateRoomSpec },
            DependentItems = new List<DependentItemSpec> { roomKeySpec }
        };
    }


    private static SituationArchetype DetermineNegotiationArchetype(GenerationContext context)
    {
        if (context.NpcPersonality == PersonalityType.DEVOTED)
        {
            return SituationArchetypeCatalog.GetArchetype("social_maneuvering");
        }

        if (context.NpcPersonality == PersonalityType.MERCANTILE)
        {
            return SituationArchetypeCatalog.GetArchetype("service_transaction");
        }

        return SituationArchetypeCatalog.GetArchetype("service_transaction");
    }

    private static NarrativeHints GenerateServiceNegotiationHints(GenerationContext context, string serviceId)
    {
        NarrativeHints hints = new();

        if (context.NpcPersonality == PersonalityType.DEVOTED)
        {
            hints.Tone = "empathetic";
            hints.Theme = "human_connection";
        }
        else if (context.NpcPersonality == PersonalityType.MERCANTILE)
        {
            hints.Tone = "transactional";
            hints.Theme = "economic_exchange";
        }
        else
        {
            hints.Tone = "professional";
            hints.Theme = "service_request";
        }

        hints.Context = $"{serviceId}_negotiation";

        if (context.PlayerCoins < 10)
        {
            hints.Style = "desperate";
        }
        else
        {
            hints.Style = "standard";
        }

        return hints;
    }

    private static ChoiceReward GenerateServiceRewards(int tier)
    {
        return new ChoiceReward
        {
            TimeSegments = 8,
            Health = tier + 2,
            Stamina = tier + 3
        };
    }

    /// <summary>
    /// TRANSACTION_SEQUENCE archetype
    ///
    /// When Used: Economic exchanges (shopping, selling, trading)
    /// Situation Count: 3
    /// Pattern: Linear (browse → negotiate → complete)
    ///
    /// Situation 1 - Browse: Player views available items/services
    ///   - Archetype: information_gathering
    ///   - Choice: Select item category or leave
    ///
    /// Situation 2 - Negotiate: Player negotiates price
    ///   - Archetype: negotiation
    ///   - Choice: Accept price, haggle (Diplomacy), challenge, or refuse
    ///
    /// Situation 3 - Complete: Transaction finalized
    ///   - Archetype: negotiation
    ///   - Choices: 4 (stat-gated, money, challenge, fallback)
    ///   - Action: Finalize exchange of goods/coins
    /// </summary>
    private static SceneArchetypeDefinition GenerateTransactionSequence(
        int tier,
        GenerationContext context)
    {
        string browseSitId = "transaction_browse";
        string negotiateSitId = "transaction_negotiate";
        string completeSitId = "transaction_complete";

        SituationArchetype browseArchetype = SituationArchetypeCatalog.GetArchetype("information_gathering");
        List<ChoiceTemplate> browseChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(browseArchetype, browseSitId);

        SituationTemplate browseSituation = new SituationTemplate
        {
            Id = browseSitId,
            Type = SituationType.Normal,
            NarrativeTemplate = null,
            ChoiceTemplates = browseChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "exploratory",
                Theme = "merchant_inventory",
                Context = "browsing_goods"
            }
        };

        SituationArchetype negotiateArchetype = SituationArchetypeCatalog.GetArchetype("negotiation");
        List<ChoiceTemplate> negotiateChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(negotiateArchetype, negotiateSitId);

        SituationTemplate negotiateSituation = new SituationTemplate
        {
            Id = negotiateSitId,
            Type = SituationType.Normal,
            NarrativeTemplate = null,
            ChoiceTemplates = negotiateChoices,
            Priority = 90,
            NarrativeHints = new NarrativeHints
            {
                Tone = "transactional",
                Theme = "price_negotiation",
                Context = "haggling"
            }
        };

        SituationArchetype completeArchetype = SituationArchetypeCatalog.GetArchetype("negotiation");
        List<ChoiceTemplate> completeChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(completeArchetype, completeSitId);

        SituationTemplate completeSituation = new SituationTemplate
        {
            Id = completeSitId,
            Type = SituationType.Normal,
            NarrativeTemplate = null,
            ChoiceTemplates = completeChoices,
            Priority = 80,
            NarrativeHints = new NarrativeHints
            {
                Tone = "conclusive",
                Theme = "transaction_complete",
                Context = "exchange_finalized"
            }
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationId = browseSitId,
            Transitions = new List<SituationTransition>
            {
                new SituationTransition
                {
                    SourceSituationId = browseSitId,
                    DestinationSituationId = negotiateSitId,
                    Condition = TransitionCondition.Always
                },
                new SituationTransition
                {
                    SourceSituationId = negotiateSitId,
                    DestinationSituationId = completeSitId,
                    Condition = TransitionCondition.Always
                }
            }
        };

        return new SceneArchetypeDefinition
        {
            SituationTemplates = new List<SituationTemplate>
            {
                browseSituation,
                negotiateSituation,
                completeSituation
            },
            SpawnRules = spawnRules
        };
    }

    /// <summary>
    /// GATEKEEPER_SEQUENCE archetype
    ///
    /// When Used: Authority challenges at checkpoints, restricted areas
    /// Situation Count: 2
    /// Pattern: Linear (confront → pass)
    ///
    /// Situation 1 - Confront: Player faces gatekeeper
    ///   - Archetype: confrontation
    ///   - Choice: Authority stat, bribe, challenge, or retreat
    ///
    /// Situation 2 - Pass: Player passes checkpoint
    ///   - Archetype: social_maneuvering
    ///   - Choices: 4 (stat-gated, money, challenge, fallback)
    ///   - Action: Navigate through checkpoint to restricted area
    /// </summary>
    private static SceneArchetypeDefinition GenerateGatekeeperSequence(
        int tier,
        GenerationContext context)
    {
        string confrontSitId = "gatekeeper_confront";
        string passSitId = "gatekeeper_pass";

        SituationArchetype confrontArchetype = SituationArchetypeCatalog.GetArchetype("confrontation");
        List<ChoiceTemplate> confrontChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(confrontArchetype, confrontSitId);

        SituationTemplate confrontSituation = new SituationTemplate
        {
            Id = confrontSitId,
            Type = SituationType.Normal,
            NarrativeTemplate = null,
            ChoiceTemplates = confrontChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "tense",
                Theme = "authority_challenge",
                Context = "gatekeeper_encounter"
            }
        };

        SituationArchetype passArchetype = SituationArchetypeCatalog.GetArchetype("social_maneuvering");
        List<ChoiceTemplate> passChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(passArchetype, passSitId);

        SituationTemplate passSituation = new SituationTemplate
        {
            Id = passSitId,
            Type = SituationType.Normal,
            NarrativeTemplate = null,
            ChoiceTemplates = passChoices,
            Priority = 90,
            NarrativeHints = new NarrativeHints
            {
                Tone = "descriptive",
                Theme = "passage_granted",
                Context = "checkpoint_cleared"
            }
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationId = confrontSitId,
            Transitions = new List<SituationTransition>
            {
                new SituationTransition
                {
                    SourceSituationId = confrontSitId,
                    DestinationSituationId = passSitId,
                    Condition = TransitionCondition.Always
                }
            }
        };

        return new SceneArchetypeDefinition
        {
            SituationTemplates = new List<SituationTemplate>
            {
                confrontSituation,
                passSituation
            },
            SpawnRules = spawnRules
        };
    }

    /// <summary>
    /// CONSEQUENCE_REFLECTION archetype
    ///
    /// When Used: Player faces consequences of bad choices (sleeping rough, failed negotiations, moral transgressions)
    /// Situation Count: 1 (standalone reflection moment)
    /// Pattern: Standalone (no transitions, single situation)
    ///
    /// Situation 1 - Reflect: Player acknowledges consequences and makes recovery choices
    ///   - Archetype: crisis (emergency response to hardship)
    ///   - Choices: Immediate recovery actions (find food, seek help, push through, despair)
    ///   - Rewards: Minor recovery, achievement marking consequence path
    ///   - Theme: Regret, hardship, resilience
    ///
    /// Location-only scene (no NPC interaction)
    /// Context-aware based on Location properties and Player state
    /// </summary>
    private static SceneArchetypeDefinition GenerateConsequenceReflection(
        int tier,
        GenerationContext context)
    {
        string situationId = "consequence_reflection";

        SituationArchetype reflectionArchetype = SituationArchetypeCatalog.GetArchetype("crisis");
        List<ChoiceTemplate> reflectionChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(reflectionArchetype, situationId);

        // Single situation: Face the consequences
        SituationTemplate reflectionSituation = new SituationTemplate
        {
            Id = situationId,
            Type = SituationType.Normal,
            NarrativeTemplate = null,  // AI generates from hints
            ChoiceTemplates = reflectionChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "regretful",
                Theme = "consequence",
                Context = "hardship",
                Style = "somber"
            }
        };

        // Standalone pattern - single situation, no transitions
        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Standalone,
            InitialSituationId = situationId,
            Transitions = new List<SituationTransition>()  // No transitions
        };

        return new SceneArchetypeDefinition
        {
            SituationTemplates = new List<SituationTemplate> { reflectionSituation },
            SpawnRules = spawnRules
        };
    }

    /// <summary>
    /// INN_CRISIS_ESCALATION archetype
    ///
    /// When Used: Escalating crisis at location with NPC ally requiring player intervention
    /// Situation Count: 4
    /// Pattern: Linear (observe → investigate → support → resolve)
    ///
    /// Situation 1 - Observe: Player notices brewing trouble
    ///   - Archetype: investigation
    ///   - Choices: 4 (stat-gated, money, challenge, fallback)
    ///   - Action: Assess escalating crisis situation
    ///
    /// Situation 2 - Investigate: Player assesses the threat
    ///   - Archetype: investigation
    ///   - Choices: Insight to understand, coins for information, Mental challenge, fallback
    ///
    /// Situation 3 - Support: Player shows presence to support NPC ally
    ///   - Archetype: social_maneuvering
    ///   - Choices: Rapport to connect, coins for gesture, Social challenge, fallback
    ///
    /// Situation 4 - Resolve: Crisis moment requiring decisive action
    ///   - Archetype: crisis
    ///   - Choices: Authority to command, coins to bribe, Physical challenge to fight, fallback
    ///
    /// Reusable Pattern: Any NPC-based crisis escalating from observation to resolution
    /// </summary>
    private static SceneArchetypeDefinition GenerateInnCrisisEscalation(
        int tier,
        GenerationContext context)
    {
        List<SituationTemplate> situations = new List<SituationTemplate>();

        // Situation 1: Observation
        SituationArchetype observationArchetype = SituationArchetypeCatalog.GetArchetype("investigation");
        List<ChoiceTemplate> observationChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(observationArchetype, "notice_trouble");

        situations.Add(new SituationTemplate
        {
            Id = "notice_trouble",
            Type = SituationType.Normal,
            NarrativeTemplate = null,  // AI generates from hints
            ChoiceTemplates = observationChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "tense",
                Theme = "escalation",
                Context = "observation",
                Style = "direct"
            }
        });

        // Situation 2: Investigation (threat assessment)
        SituationArchetype investigationArchetype = SituationArchetypeCatalog.GetArchetype("investigation");
        List<ChoiceTemplate> investigationChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(investigationArchetype, "harassment_begins");

        situations.Add(new SituationTemplate
        {
            Id = "harassment_begins",
            Type = SituationType.Normal,
            NarrativeTemplate = null,
            ChoiceTemplates = investigationChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "threatening",
                Theme = "intimidation",
                Context = "threat_assessment",
                Style = "visceral"
            }
        });

        // Situation 3: Social Maneuvering (supporting ally)
        SituationArchetype socialArchetype = SituationArchetypeCatalog.GetArchetype("social_maneuvering");
        List<ChoiceTemplate> socialChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(socialArchetype, "elena_signals");

        situations.Add(new SituationTemplate
        {
            Id = "elena_signals",
            Type = SituationType.Normal,
            NarrativeTemplate = null,
            ChoiceTemplates = socialChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "urgent",
                Theme = "support",
                Context = "ally_protection",
                Style = "sparse"
            }
        });

        // Situation 4: Crisis (decisive action)
        SituationArchetype crisisArchetype = SituationArchetypeCatalog.GetArchetype("crisis");
        List<ChoiceTemplate> crisisChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(crisisArchetype, "crisis_confrontation");

        situations.Add(new SituationTemplate
        {
            Id = "crisis_confrontation",
            Type = SituationType.Crisis,
            NarrativeTemplate = null,
            ChoiceTemplates = crisisChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "explosive",
                Theme = "crisis",
                Context = "physical_confrontation",
                Style = "immediate"
            }
        });

        // Linear pattern - situation 1 → 2 → 3 → 4
        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationId = "notice_trouble",
            Transitions = new List<SituationTransition>
            {
                new SituationTransition { SourceSituationId = "notice_trouble", DestinationSituationId = "harassment_begins", Condition = TransitionCondition.Always },
                new SituationTransition { SourceSituationId = "harassment_begins", DestinationSituationId = "elena_signals", Condition = TransitionCondition.Always },
                new SituationTransition { SourceSituationId = "elena_signals", DestinationSituationId = "crisis_confrontation", Condition = TransitionCondition.Always }
            }
        };

        return new SceneArchetypeDefinition
        {
            SituationTemplates = situations,
            SpawnRules = spawnRules
        };
    }

    /// <summary>
    /// SHARED HELPER: Generate single-situation scene from situation archetype
    ///
    /// Pattern: Standalone (one situation, no transitions)
    /// Used by: All single_* scene archetypes
    ///
    /// This method wraps a situation archetype in scene archetype structure.
    /// Scene archetype DEFINES which situation archetype to use (design decision).
    /// JSON authors select scene archetype name, not situation archetype (configuration).
    ///
    /// Following HIGHLANDER principle: Scene archetype ID encodes BOTH structure (one situation)
    /// AND content (which situation archetype). No parameterization from JSON.
    /// </summary>
    private static SceneArchetypeDefinition GenerateSingleSituationScene(
        string situationArchetypeId,
        int tier,
        GenerationContext context,
        string themeContext,
        string tone)
    {
        string situationId = $"single_{situationArchetypeId}";

        SituationArchetype archetype = SituationArchetypeCatalog.GetArchetype(situationArchetypeId);
        List<ChoiceTemplate> choices = SituationArchetypeCatalog.GenerateChoiceTemplates(archetype, situationId);

        SituationTemplate situation = new SituationTemplate
        {
            Id = situationId,
            Type = SituationType.Normal,
            NarrativeTemplate = null,
            ChoiceTemplates = choices,
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = tone,
                Theme = themeContext,
                Context = situationArchetypeId,
                Style = "standard"
            }
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

}
