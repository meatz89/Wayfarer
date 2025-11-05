using Wayfarer.GameState;
using Wayfarer.GameState.Enums;

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
    /// Get scene archetype definition by ID with entity context for categorical property reading
    /// Called at parse time to generate context-aware multi-situation structure
    /// NPC can be null for location-only scenes (consequence, environmental, etc.)
    /// Reads NPC personality, Location properties, Player state to generate appropriate choices
    /// Throws InvalidDataException on unknown archetype ID (fail fast)
    /// </summary>
    public static SceneArchetypeDefinition GetSceneArchetype(
        string sceneArchetypeId,
        string serviceType,
        int tier,
        NPC contextNPC,
        Location contextLocation,
        Player contextPlayer)
    {
        return sceneArchetypeId?.ToLowerInvariant() switch
        {
            "service_with_location_access" => GenerateServiceWithLocationAccess(serviceType, tier, contextNPC, contextLocation, contextPlayer),
            "transaction_sequence" => GenerateTransactionSequence(tier, contextNPC, contextLocation, contextPlayer),
            "gatekeeper_sequence" => GenerateGatekeeperSequence(tier, contextNPC, contextLocation, contextPlayer),
            "consequence_reflection" => GenerateConsequenceReflection(tier, contextLocation, contextPlayer),
            "single_situation" => GenerateSingleSituation(serviceType, tier, contextNPC, contextLocation, contextPlayer),
            "inn_crisis_escalation" => GenerateInnCrisisEscalation(tier, contextNPC, contextLocation, contextPlayer),
            _ => throw new InvalidDataException($"Unknown scene archetype ID: '{sceneArchetypeId}'. Valid values: service_with_location_access, transaction_sequence, gatekeeper_sequence, consequence_reflection, single_situation, inn_crisis_escalation")
        };
    }

    /// <summary>
    /// SERVICE_WITH_LOCATION_ACCESS archetype
    ///
    /// When Used: Service requiring access to locked location (lodging, bathing, healing, storage, training)
    /// Situation Count: 4
    /// Pattern: Linear (negotiate → access → service → depart)
    ///
    /// Situation 1 - Negotiate: Player negotiates with service provider (NPC)
    ///   - Archetype: service_transaction
    ///   - Choice: Authority stat, coins, challenge, or refuse
    ///   - Grants: access_token item (e.g., room_key)
    ///
    /// Situation 2 - Access: Player enters locked location (AutoAdvance)
    ///   - Archetype: access_control
    ///   - Requires: access_token item
    ///   - Action: Unlock location, enter
    ///
    /// Situation 3 - Service: Player receives service benefit
    ///   - Archetype: service-specific (rest/cleanliness/healing/etc.)
    ///   - Action: Use service
    ///   - Rewards: Resource restoration, stat changes, time advancement
    ///
    /// Situation 4 - Depart: Player leaves, cleanup (AutoAdvance)
    ///   - Archetype: None (simple departure)
    ///   - Action: Leave location
    ///   - Cleanup: Remove access_token, re-lock location
    /// </summary>
    private static SceneArchetypeDefinition GenerateServiceWithLocationAccess(
        string serviceType,
        int tier,
        NPC contextNPC,
        Location contextLocation,
        Player contextPlayer)
    {
        // Generate 4 situation IDs
        string negotiateSitId = $"{serviceType}_negotiate";
        string accessSitId = $"{serviceType}_access";
        string serviceSitId = $"{serviceType}_service";
        string departSitId = $"{serviceType}_depart";

        // Context-aware archetype selection based on NPC personality
        string negotiateArchetype = DetermineNegotiationArchetype(contextNPC, contextLocation, contextPlayer);
        NarrativeHints negotiateHints = GenerateNegotiationHints(serviceType, contextNPC, contextLocation, contextPlayer);

        // Generate situation names based on service type
        string negotiateName = $"Secure {CapitalizeServiceType(serviceType)}";
        string accessName = "Enter";
        string serviceName = CapitalizeServiceType(serviceType);
        string departName = "Leave";

        // Situation 1: Negotiate with service provider
        SituationTemplate negotiateSituation = new SituationTemplate
        {
            Id = negotiateSitId,
            Name = negotiateName,
            Type = SituationType.Normal,
            ArchetypeId = negotiateArchetype,  // Context-aware archetype selection
            NarrativeTemplate = null,  // AI generates at finalization
            Priority = 100,
            NarrativeHints = negotiateHints,
            RequiredLocationId = contextNPC?.Location?.Id,  // Negotiate at NPC's location (common_room)
            RequiredNpcId = contextNPC?.ID  // NPC must be present for negotiation (elena)
        };

        // Situation 2: Access locked location (AutoAdvance)
        SituationTemplate accessSituation = new SituationTemplate
        {
            Id = accessSitId,
            Name = accessName,
            Type = SituationType.Normal,
            ArchetypeId = "access_control",  // SituationArchetypeCatalog generates choices
            NarrativeTemplate = null,  // AI generates
            Priority = 90,
            NarrativeHints = new NarrativeHints
            {
                Tone = "descriptive",
                Theme = "location_access",
                Context = $"{serviceType}_entry"
            },
            AutoProgressRewards = new ChoiceReward
            {
                TimeSegments = 1  // Accessing location costs 1 time segment
            },
            RequiredLocationId = contextLocation?.Id,  // Access at service location (upper_floor)
            RequiredNpcId = null  // No NPC requirement for access
        };

        // Situation 3: Service provision
        SituationTemplate serviceSituation = new SituationTemplate
        {
            Id = serviceSitId,
            Name = serviceName,
            Type = SituationType.Normal,
            ArchetypeId = GetServiceArchetype(serviceType),  // Service-specific archetype
            NarrativeTemplate = null,  // AI generates
            Priority = 80,
            NarrativeHints = new NarrativeHints
            {
                Tone = "restorative",
                Theme = $"{serviceType}_experience",
                Context = $"{serviceType}_provision"
            },
            AutoProgressRewards = GenerateServiceRewards(serviceType, tier),
            RequiredLocationId = contextLocation?.Id,  // Service at service location (upper_floor)
            RequiredNpcId = null  // No NPC requirement for service delivery
        };

        // Situation 4: Departure and cleanup (AutoAdvance)
        SituationTemplate departureSituation = new SituationTemplate
        {
            Id = departSitId,
            Name = departName,
            Type = SituationType.Normal,
            ArchetypeId = null,  // Simple departure, no choices needed
            NarrativeTemplate = null,  // AI generates
            Priority = 70,
            NarrativeHints = new NarrativeHints
            {
                Tone = "conclusive",
                Theme = "departure",
                Context = $"{serviceType}_conclusion"
            },
            AutoProgressRewards = new ChoiceReward
            {
                TimeSegments = 1  // Leaving costs 1 time segment
            },
            RequiredLocationId = contextNPC?.Location?.Id,  // Depart at NPC's location (common_room)
            RequiredNpcId = contextNPC?.ID  // Return to NPC for conclusion (elena)
        };

        // Generate spawn rules (linear progression)
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

        return new SceneArchetypeDefinition
        {
            SituationTemplates = new List<SituationTemplate>
            {
                negotiateSituation,
                accessSituation,
                serviceSituation,
                departureSituation
            },
            SpawnRules = spawnRules
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
        NPC contextNPC,
        Location contextLocation,
        Player contextPlayer)
    {
        string browseSitId = "transaction_browse";
        string negotiateSitId = "transaction_negotiate";
        string completeSitId = "transaction_complete";

        SituationTemplate browseSituation = new SituationTemplate
        {
            Id = browseSitId,
            Type = SituationType.Normal,
            ArchetypeId = "information_gathering",
            NarrativeTemplate = null,
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "exploratory",
                Theme = "merchant_inventory",
                Context = "browsing_goods"
            }
        };

        SituationTemplate negotiateSituation = new SituationTemplate
        {
            Id = negotiateSitId,
            Type = SituationType.Normal,
            ArchetypeId = "negotiation",
            NarrativeTemplate = null,
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
            ArchetypeId = null,
            NarrativeTemplate = null,
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
        NPC contextNPC,
        Location contextLocation,
        Player contextPlayer)
    {
        string confrontSitId = "gatekeeper_confront";
        string passSitId = "gatekeeper_pass";

        SituationTemplate confrontSituation = new SituationTemplate
        {
            Id = confrontSitId,
            Type = SituationType.Normal,
            ArchetypeId = "confrontation",
            NarrativeTemplate = null,
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
            ArchetypeId = null,
            NarrativeTemplate = null,
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
    /// Map service type to appropriate situation archetype
    /// </summary>
    private static string GetServiceArchetype(string serviceType)
    {
        return serviceType?.ToLowerInvariant() switch
        {
            "lodging" => "service_transaction",  // Rest/sleep service
            "bathing" => "service_transaction",  // Cleanliness service
            "healing" => "service_transaction",  // Health restoration
            "storage" => "service_transaction",  // Item management
            "training" => "skill_demonstration", // Skill increase
            _ => "service_transaction"  // Default
        };
    }

    /// <summary>
    /// Generate service-specific rewards based on service type and tier
    /// Tier scaling: Higher tiers cost more and provide greater benefits
    /// </summary>
    private static ChoiceReward GenerateServiceRewards(string serviceType, int tier)
    {
        ChoiceReward reward = new ChoiceReward();

        switch (serviceType?.ToLowerInvariant())
        {
            case "lodging":
                // Rest restores stamina, advances time to morning
                reward.TimeSegments = 6;  // Full night sleep
                reward.Stamina = 30 + (tier * 10);  // Scales with tier
                break;

            case "bathing":
                // Bathing costs time, restores cleanliness
                reward.TimeSegments = 2;  // Short activity
                break;

            case "healing":
                // Healing restores health
                reward.TimeSegments = 1;
                reward.Health = 20 + (tier * 10);  // Scales with tier
                break;

            case "storage":
                // Storage enables item management
                reward.TimeSegments = 1;
                break;

            case "training":
                // Training advances time significantly
                reward.TimeSegments = 4;
                break;

            default:
                reward.TimeSegments = 1;
                break;
        }

        return reward;
    }

    /// <summary>
    /// Context-aware negotiation archetype selection
    /// Reads NPC personality, Location properties, and Player state to determine appropriate archetype
    /// DEVOTED personality enables emotional/relationship paths
    /// MERCANTILE personality focuses on transactional exchanges
    /// Commercial locations enable work-exchange fallbacks
    /// Low player coins trigger poverty-aware alternatives
    /// </summary>
    private static string DetermineNegotiationArchetype(NPC contextNPC, Location contextLocation, Player contextPlayer)
    {
        // DEVOTED personality enables emotional connection paths (social_maneuvering)
        // Tutorial: Elena (DEVOTED) allows "help with evening service" emotional path
        if (contextNPC.PersonalityType == PersonalityType.DEVOTED)
        {
            return "social_maneuvering";  // Enables relationship-building, emotional appeal, service exchange
        }

        // MERCANTILE personality focuses on pure transactional exchanges
        if (contextNPC.PersonalityType == PersonalityType.MERCANTILE)
        {
            return "negotiation";  // Price disputes, haggling, trade optimization
        }

        // Default: service_transaction (standard payment-for-service pattern)
        return "service_transaction";  // Authority check, coin payment, challenge, refusal
    }

    /// <summary>
    /// Generate context-aware narrative hints based on entity properties
    /// Adjusts tone/theme/context based on NPC personality and location characteristics
    /// DEVOTED NPCs get warmer, more personal tones
    /// MERCANTILE NPCs get colder, more business-like tones
    /// Commercial locations emphasize work opportunities
    /// Low player coins emphasize scarcity themes
    /// </summary>
    private static NarrativeHints GenerateNegotiationHints(
        string serviceType,
        NPC contextNPC,
        Location contextLocation,
        Player contextPlayer)
    {
        NarrativeHints hints = new NarrativeHints();

        // Tone based on NPC personality
        if (contextNPC.PersonalityType == PersonalityType.DEVOTED)
        {
            hints.Tone = "empathetic";  // Warm, understanding, personal
            hints.Theme = "human_connection";  // Focus on relationships, not transactions
        }
        else if (contextNPC.PersonalityType == PersonalityType.MERCANTILE)
        {
            hints.Tone = "transactional";  // Cold, efficient, business-like
            hints.Theme = "economic_exchange";  // Focus on value, price, trade
        }
        else
        {
            hints.Tone = "professional";  // Neutral, standard service interaction
            hints.Theme = "service_request";  // Standard request pattern
        }

        // Context based on service type and location
        hints.Context = $"{serviceType}_negotiation";

        // Style modifiers based on player state
        if (contextPlayer.Coins < 10)  // Tutorial: player has 5 coins, 10-coin room triggers scarcity
        {
            hints.Style = "desperate";  // Scarcity-aware narrative
        }
        else
        {
            hints.Style = "standard";  // Normal transaction flow
        }

        return hints;
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
        Location contextLocation,
        Player contextPlayer)
    {
        string situationId = "consequence_reflection";

        // Single situation: Face the consequences
        SituationTemplate reflectionSituation = new SituationTemplate
        {
            Id = situationId,
            Type = SituationType.Normal,
            ArchetypeId = "crisis",  // Emergency response pattern (4 choices: stat-gated/money/challenge/fallback)
            NarrativeTemplate = null,  // AI generates from hints
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
    /// SINGLE_SITUATION archetype
    ///
    /// When Used: Generic single-situation scenes using any situation archetype
    /// Situation Count: 1 (standalone)
    /// Pattern: Standalone (no transitions, single situation)
    ///
    /// Parameterized by situationArchetypeId (passed via serviceType parameter):
    ///   - "negotiation": Price disputes, deal-making
    ///   - "confrontation": Authority challenges, barriers
    ///   - "investigation": Mysteries, information gathering
    ///   - "social_maneuvering": Reputation, relationships
    ///   - "crisis": Emergencies, high-stakes moments
    ///   - Plus 10 extended archetypes (service_transaction, access_control, etc.)
    ///
    /// Reusable pattern for procedural scene generation
    /// Context-aware based on NPC personality, Location properties, Player state
    /// </summary>
    private static SceneArchetypeDefinition GenerateSingleSituation(
        string situationArchetypeId,
        int tier,
        NPC contextNPC,
        Location contextLocation,
        Player contextPlayer)
    {
        string situationId = $"{situationArchetypeId}_situation";

        // Single situation using specified archetype
        SituationTemplate situation = new SituationTemplate
        {
            Id = situationId,
            Type = SituationType.Normal,
            ArchetypeId = situationArchetypeId,
            NarrativeTemplate = null,
            Priority = 100,
            NarrativeHints = GenerateContextualHints(situationArchetypeId, contextNPC, contextLocation, contextPlayer)
        };

        // Standalone pattern - single situation, no transitions
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

    /// <summary>
    /// Generate context-aware narrative hints based on situation archetype and entity properties
    /// Reads NPC personality, Location properties, Player state to generate appropriate tone/theme/context
    /// </summary>
    private static NarrativeHints GenerateContextualHints(
        string situationArchetypeId,
        NPC contextNPC,
        Location contextLocation,
        Player contextPlayer)
    {
        NarrativeHints hints = new NarrativeHints();

        // Base hints per situation archetype
        switch (situationArchetypeId?.ToLowerInvariant())
        {
            case "negotiation":
                hints.Tone = "business-like";
                hints.Theme = "economic_negotiation";
                hints.Context = "merchant_interaction";
                hints.Style = "pragmatic";
                break;

            case "confrontation":
                hints.Tone = "tense";
                hints.Theme = "authority_confrontation";
                hints.Context = "checkpoint_encounter";
                hints.Style = "direct";
                break;

            case "investigation":
                hints.Tone = "curious";
                hints.Theme = "information_exchange";
                hints.Context = "casual_inquiry";
                hints.Style = "conversational";
                break;

            case "social_maneuvering":
                hints.Tone = "calculating";
                hints.Theme = "social_capital";
                hints.Context = "relationship_building";
                hints.Style = "formal";
                break;

            case "crisis":
                hints.Tone = "urgent";
                hints.Theme = "emergency_response";
                hints.Context = "time_pressure";
                hints.Style = "tense";
                break;

            case "service_transaction":
                hints.Tone = "business-like";
                hints.Theme = "service_request";
                hints.Context = "transactional";
                hints.Style = "professional";
                break;

            case "access_control":
                hints.Tone = "determined";
                hints.Theme = "access_restriction";
                hints.Context = "security_challenge";
                hints.Style = "tactical";
                break;

            case "information_gathering":
                hints.Tone = "casual";
                hints.Theme = "information_network";
                hints.Context = "tavern_gossip";
                hints.Style = "conversational";
                break;

            case "skill_demonstration":
                hints.Tone = "professional";
                hints.Theme = "competency_proof";
                hints.Context = "guild_examination";
                hints.Style = "formal";
                break;

            case "reputation_challenge":
                hints.Tone = "confrontational";
                hints.Theme = "honor_defense";
                hints.Context = "public_challenge";
                hints.Style = "assertive";
                break;

            case "emergency_aid":
                hints.Tone = "urgent";
                hints.Theme = "medical_intervention";
                hints.Context = "emergency_response";
                hints.Style = "decisive";
                break;

            case "administrative_procedure":
                hints.Tone = "patient";
                hints.Theme = "bureaucratic_process";
                hints.Context = "official_paperwork";
                hints.Style = "procedural";
                break;

            case "trade_dispute":
                hints.Tone = "assertive";
                hints.Theme = "quality_complaint";
                hints.Context = "merchant_dispute";
                hints.Style = "firm";
                break;

            case "cultural_faux_pas":
                hints.Tone = "apologetic";
                hints.Theme = "cultural_mistake";
                hints.Context = "etiquette_breach";
                hints.Style = "conciliatory";
                break;

            case "recruitment":
                hints.Tone = "calculated";
                hints.Theme = "commitment_decision";
                hints.Context = "faction_recruitment";
                hints.Style = "cautious";
                break;

            default:
                hints.Tone = "neutral";
                hints.Theme = "generic_interaction";
                hints.Context = "standard_situation";
                hints.Style = "balanced";
                break;
        }

        // Context-aware adjustments based on entity properties
        if (contextNPC != null)
        {
            // NPC personality modifies tone
            if (contextNPC.PersonalityType == PersonalityType.DEVOTED)
            {
                hints.Tone = "empathetic";
            }
            else if (contextNPC.PersonalityType == PersonalityType.MERCANTILE)
            {
                hints.Tone = "transactional";
            }
            else if (contextNPC.PersonalityType == PersonalityType.PROUD)
            {
                hints.Tone = "authoritative";
            }
            else if (contextNPC.PersonalityType == PersonalityType.CUNNING)
            {
                hints.Tone = "calculating";
            }
        }

        // Location properties modify context (if available)
        if (contextLocation != null && contextLocation.LocationProperties.Contains(LocationPropertyType.Commercial))
        {
            hints.Context = $"{hints.Context}_commercial";
        }

        // Player state modifies style (if available)
        if (contextPlayer != null && contextPlayer.Coins < 10)
        {
            hints.Style = "desperate";
        }

        return hints;
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
        NPC contextNPC,
        Location contextLocation,
        Player contextPlayer)
    {
        List<SituationTemplate> situations = new List<SituationTemplate>();

        // Situation 1: Observation (auto-progress)
        situations.Add(new SituationTemplate
        {
            Id = "notice_trouble",
            Type = SituationType.Normal,
            ArchetypeId = null,  // No archetype - pure narrative observation
            NarrativeTemplate = null,  // AI generates from hints
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
        situations.Add(new SituationTemplate
        {
            Id = "harassment_begins",
            Type = SituationType.Normal,
            ArchetypeId = "investigation",  // Insight to assess, Mental challenge
            NarrativeTemplate = null,
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
        situations.Add(new SituationTemplate
        {
            Id = "elena_signals",
            Type = SituationType.Normal,
            ArchetypeId = "social_maneuvering",  // Rapport to support, Social challenge
            NarrativeTemplate = null,
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
        situations.Add(new SituationTemplate
        {
            Id = "crisis_confrontation",
            Type = SituationType.Crisis,
            ArchetypeId = "crisis",  // Authority to command, Physical challenge to fight
            NarrativeTemplate = null,
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
    /// Capitalize service type for display names
    /// "lodging" → "Lodging", "bathing" → "Bathing", "healing" → "Healing"
    /// </summary>
    private static string CapitalizeServiceType(string serviceType)
    {
        if (string.IsNullOrEmpty(serviceType))
            return "Service";

        return char.ToUpper(serviceType[0]) + serviceType.Substring(1);
    }
}
