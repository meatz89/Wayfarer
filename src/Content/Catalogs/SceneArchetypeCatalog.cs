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
            "transaction_sequence" => GenerateTransactionSequence(tier, context),
            "gatekeeper_sequence" => GenerateGatekeeperSequence(tier, context),
            "consequence_reflection" => GenerateConsequenceReflection(tier, context),
            "inn_crisis_escalation" => GenerateInnCrisisEscalation(tier, context),
            _ => throw new InvalidDataException($"Unknown scene archetype ID: '{archetypeId}'. Valid archetypes: service_with_location_access, transaction_sequence, gatekeeper_sequence, consequence_reflection, inn_crisis_escalation")
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
    ///   - Choice: Rapport/Diplomacy vs. Coins
    ///
    /// Situation 2 - Access: Enter private location (auto-progress)
    ///   - Archetype: None (automatic)
    ///   - Action: Unlock and enter generated location
    ///
    /// Situation 3 - Service: Receive service benefit (auto-progress)
    ///   - Archetype: None (automatic)
    ///   - Reward: Tier-scaled health/stamina restoration + time cost
    ///
    /// Situation 4 - Depart: Leave and cleanup (auto-progress)
    ///   - Archetype: None (automatic)
    ///   - Action: Remove key, lock location, return to base
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

        SituationArchetype negotiateArchetype = DetermineNegotiationArchetype(context);
        NarrativeHints negotiateHints = GenerateServiceNegotiationHints(context, serviceId);
        List<ChoiceTemplate> negotiateChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(negotiateArchetype, negotiateSitId);

        // Add unlock rewards to negotiate choices (all successful negotiation paths grant room access)
        // Stat-gated and money choices get immediate rewards
        // Challenge choice gets OnSuccessReward (applied after challenge completion)
        // Fallback choice gets no rewards (represents leaving without service)
        List<ChoiceTemplate> enrichedChoices = new List<ChoiceTemplate>();
        foreach (ChoiceTemplate choice in negotiateChoices)
        {
            if (choice.Id.EndsWith("_stat") || choice.Id.EndsWith("_money"))
            {
                // Instant success paths: unlock room and grant key immediately
                enrichedChoices.Add(new ChoiceTemplate
                {
                    Id = choice.Id,
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
            }
            else if (choice.Id.EndsWith("_challenge"))
            {
                // Challenge path: unlock room and grant key on success only
                enrichedChoices.Add(new ChoiceTemplate
                {
                    Id = choice.Id,
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
            }
            else
            {
                // Fallback path: no rewards (player leaves without securing lodging)
                enrichedChoices.Add(choice);
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
                Context = $"{serviceId}_entry"
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
                Theme = $"{serviceId}_experience",
                Context = $"{serviceId}_provision"
            },
            AutoProgressRewards = GenerateServiceRewards(tier),
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
                Context = $"{serviceId}_conclusion"
            },
            AutoProgressRewards = new ChoiceReward
            {
                TimeSegments = 1,
                ItemsToRemove = new List<string> { "generated:room_key" },
                LocationsToLock = new List<string> { "generated:private_room" }
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
    ///   - Archetype: None (automatic)
    ///   - Action: Exchange goods/coins
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

        SituationTemplate completeSituation = new SituationTemplate
        {
            Id = completeSitId,
            Type = SituationType.Normal,
            NarrativeTemplate = null,
            ChoiceTemplates = new List<ChoiceTemplate>(),
            Priority = 80,
            NarrativeHints = new NarrativeHints
            {
                Tone = "conclusive",
                Theme = "transaction_complete",
                Context = "exchange_finalized"
            },
            AutoProgressRewards = new ChoiceReward
            {
                TimeSegments = 1
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
    /// Situation 2 - Pass: Player passes checkpoint (AutoAdvance)
    ///   - Archetype: None (automatic)
    ///   - Action: Enter restricted area
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

        SituationTemplate passSituation = new SituationTemplate
        {
            Id = passSitId,
            Type = SituationType.Normal,
            NarrativeTemplate = null,
            ChoiceTemplates = new List<ChoiceTemplate>(),
            Priority = 90,
            NarrativeHints = new NarrativeHints
            {
                Tone = "descriptive",
                Theme = "passage_granted",
                Context = "checkpoint_cleared"
            },
            AutoProgressRewards = new ChoiceReward
            {
                TimeSegments = 1
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
    /// Situation 1 - Observe: Player notices brewing trouble (auto-progress)
    ///   - No archetype (narrative only)
    ///   - Rewards: Resolve +1, Time +1
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

        // Situation 1: Observation (auto-progress)
        situations.Add(new SituationTemplate
        {
            Id = "notice_trouble",
            Type = SituationType.Normal,
            NarrativeTemplate = null,  // AI generates from hints
            ChoiceTemplates = new List<ChoiceTemplate>(),  // Auto-progress, no choices
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "tense",
                Theme = "escalation",
                Context = "observation",
                Style = "direct"
            },
            AutoProgressRewards = new ChoiceReward
            {
                Resolve = 1,
                TimeSegments = 1
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

}
