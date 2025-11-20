/// <summary>
/// ⚠️ PARSE-TIME ONLY CATALOGUE ⚠️
///
/// Generates complete multi-situation Scene structures from archetype IDs at PARSE TIME.
/// Each archetype is INTENTIONALLY DESIGNED for specific fictional contexts with verisimilitude.
///
/// CATALOGUE PATTERN COMPLIANCE:
/// - Called ONLY from SceneTemplateParser at PARSE TIME
/// - NEVER called from facades, managers, or runtime code
/// - NEVER imported except in parser classes
/// - Generates SceneArchetypeDefinition that parser embeds in SceneTemplate
/// - Translation happens ONCE at game initialization
///
/// ARCHITECTURE:
/// JSON specifies sceneArchetypeId → Parser calls catalogue → Receives SituationTemplates + SpawnRules
/// → Parser stores in SceneTemplate → Runtime queries GameWorld.SceneTemplates (NO catalogue calls)
///
/// SCENE ARCHETYPES (Reusable patterns):
/// - inn_lodging: 3-situation inn lodging flow (negotiate → rest → depart)
/// - consequence_reflection: Single-situation consequence acknowledgment
///
/// Each archetype defines:
/// - Specific situation count and structure (intentional design)
/// - Situation archetypes (delegates to SituationArchetypeCatalog for choice generation)
/// - Transition rules (Linear, Standalone, etc.)
/// - Dependent resources (locations/items created by scene)
/// - Narrative hints for AI generation
/// </summary>
public static class SceneArchetypeCatalog
{
    /// <summary>
    /// Generate scene archetype definition by ID
    /// Called at parse time to generate complete scene structure
    /// Throws InvalidDataException on unknown archetype ID (fail fast)
    /// </summary>
    public static SceneArchetypeDefinition Generate(
        string archetypeId,
        int tier,
        GenerationContext context)
    {
        return archetypeId?.ToLowerInvariant() switch
        {
            "inn_lodging" => GenerateInnLodging(tier, context),
            "consequence_reflection" => GenerateConsequenceReflection(tier, context),
            "delivery_contract" => GenerateDeliveryContract(tier, context),
            "route_segment_travel" => GenerateRouteSegmentTravel(tier, context),

            _ => throw new InvalidDataException($"Unknown scene archetype ID: '{archetypeId}'. Valid archetypes: inn_lodging, consequence_reflection, delivery_contract, route_segment_travel")
        };
    }

    /// <summary>
    /// INN_LODGING archetype
    ///
    /// FICTIONAL CONTEXT: Player arrives at inn, needs lodging for the night
    /// REUSABLE: Works for any inn NPC in the game
    ///
    /// Situation Count: 3
    /// Pattern: Linear (negotiate → rest → depart)
    ///
    /// Situation 1 - Negotiate: Secure lodging with innkeeper
    ///   - Archetype: service_negotiation (Diplomacy/coins/challenge/fallback)
    ///   - Choices: 4 standard choices scaled by NPC demeanor and Quality
    ///   - Success rewards: Unlock private room, grant room key
    ///   - Fallback: Player leaves without securing lodging
    ///
    /// Situation 2 - Rest: Use the secured room to recover
    ///   - Archetype: service_execution_rest (different recovery approaches)
    ///   - Choices: 4 choices offering different risk/reward tradeoffs
    ///   - Rewards: Health/Stamina restoration scaled by EnvironmentQuality
    ///   - Time advancement: To next morning
    ///
    /// Situation 3 - Depart: Leave inn and continue journey
    ///   - Archetype: service_departure (organize belongings, return key)
    ///   - Choices: 2 choices (immediate/careful departure)
    ///   - Rewards: Remove room key, lock room, optional departure buffs
    ///
    /// Dependent Resources:
    ///   - private_room: Generated location (locked, requires room_key)
    ///   - room_key: Generated item (granted on negotiation, removed on departure)
    ///
    /// VERISIMILITUDE: Lodging at inn follows realistic flow - you negotiate access,
    /// use the room, then leave. Specific to inn lodging fiction, reusable anywhere.
    /// </summary>
    private static SceneArchetypeDefinition GenerateInnLodging(int tier, GenerationContext context)
    {
        string sceneId = "inn_lodging";
        string negotiateSitId = $"{sceneId}_negotiate";
        string restSitId = $"{sceneId}_rest";
        string departSitId = $"{sceneId}_depart";

        // SITUATION 1: SECURE LODGING
        // Tutorial A1 (sequence 1): Manual identity formation choices
        // Standard/A2+ (sequence null/2+): Use service_negotiation archetype with universal scaling
        List<ChoiceTemplate> negotiateChoices;

        if (context.AStorySequence.HasValue && context.AStorySequence.Value == 1)
        {
            // SIR BRANTE A1: Identity Formation (manually authored)
            // - NO stat requirements (player starts with 0)
            // - Each choice grants DIFFERENT stats (player chooses WHO to be)
            negotiateChoices = new List<ChoiceTemplate>
        {
            new ChoiceTemplate
            {
                Id = $"{negotiateSitId}_friendly",
                PathType = ChoicePathType.InstantSuccess,
                ActionTextTemplate = "Chat warmly with {NPCName}",
                RequirementFormula = new CompoundRequirement(),
                CostTemplate = new ChoiceCost { Coins = 10 },
                RewardTemplate = new ChoiceReward
                {
                    Rapport = 1
                },
                ActionType = ChoiceActionType.Instant
            },
            new ChoiceTemplate
            {
                Id = $"{negotiateSitId}_assertive",
                PathType = ChoicePathType.InstantSuccess,
                ActionTextTemplate = "Assert your need for accommodation",
                RequirementFormula = new CompoundRequirement(),
                CostTemplate = new ChoiceCost { Coins = 10 },
                RewardTemplate = new ChoiceReward
                {
                    Authority = 1
                },
                ActionType = ChoiceActionType.Instant
            },
            new ChoiceTemplate
            {
                Id = $"{negotiateSitId}_cunning",
                PathType = ChoicePathType.InstantSuccess,
                ActionTextTemplate = "Seek advantageous deal",
                RequirementFormula = new CompoundRequirement(),
                CostTemplate = new ChoiceCost { Coins = 10 },
                RewardTemplate = new ChoiceReward
                {
                    Cunning = 1
                },
                ActionType = ChoiceActionType.Instant
            },
            new ChoiceTemplate
            {
                Id = $"{negotiateSitId}_diplomatic",
                PathType = ChoicePathType.InstantSuccess,
                ActionTextTemplate = "Negotiate a fair arrangement",
                RequirementFormula = new CompoundRequirement(),
                CostTemplate = new ChoiceCost { Coins = 10 },
                RewardTemplate = new ChoiceReward
                {
                    Diplomacy = 1
                },
                ActionType = ChoiceActionType.Instant
            }
        };
        }
        else
        {
            // Standard/A2+: Use service_negotiation archetype with universal scaling
            SituationArchetype negotiateArchetype = SituationArchetypeCatalog.GetArchetype("service_negotiation");
            negotiateChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
                negotiateArchetype,
                negotiateSitId,
                context);

            // Enrich with room_key grant on successful paths
            List<ChoiceTemplate> enrichedChoices = new List<ChoiceTemplate>();
            foreach (ChoiceTemplate choice in negotiateChoices)
            {
                ChoiceReward baseReward = choice.RewardTemplate ?? new ChoiceReward();
                ChoiceReward successReward = choice.OnSuccessReward;

                if (choice.PathType == ChoicePathType.InstantSuccess || choice.PathType == ChoicePathType.Challenge)
                {
                    // DELETED: ItemIds property removed in HIGHLANDER refactoring
                    // TODO: Implement correct pattern for granting dependent items via choice rewards
                }

                enrichedChoices.Add(new ChoiceTemplate
                {
                    Id = choice.Id,
                    PathType = choice.PathType,
                    ActionTextTemplate = choice.ActionTextTemplate,
                    RequirementFormula = choice.RequirementFormula,
                    CostTemplate = choice.CostTemplate,
                    RewardTemplate = baseReward,
                    OnSuccessReward = successReward,
                    OnFailureReward = choice.OnFailureReward,
                    ActionType = choice.ActionType,
                    ChallengeId = choice.ChallengeId,
                    ChallengeType = choice.ChallengeType,
                    DeckId = choice.DeckId,
                    NavigationPayload = choice.NavigationPayload
                });
            }
            negotiateChoices = enrichedChoices;
        }

        SituationTemplate negotiateSituation = new SituationTemplate
        {
            Id = negotiateSitId,
            Name = "Secure Lodging",
            Type = SituationType.Normal,
            SystemType = TacticalSystemType.Social,
            NarrativeTemplate = null,  // AI generates from hints
            ChoiceTemplates = negotiateChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "welcoming",
                Theme = "first_impressions",
                Context = "securing_lodging",
                Style = "approachable"
            },
            // Hierarchical placement: Inherit scene base (common room + innkeeper)
            LocationFilter = null,  // Inherits scene BaseLocationFilter (common room)
            NpcFilter = null,       // Inherits scene BaseNpcFilter (innkeeper)
            RouteFilter = null
        };

        // SITUATION 2: EVENING IN ROOM
        // Tutorial A1: Identity formation (stat grants)
        // Standard: Recovery focus (Health/Stamina/Focus restoration)
        List<ChoiceTemplate> restChoices;

        if (context.AStorySequence.HasValue && context.AStorySequence.Value == 1)
        {
            // SIR BRANTE A1: Identity Formation choices
            restChoices = new List<ChoiceTemplate>
        {
            new ChoiceTemplate
            {
                Id = $"{restSitId}_study",
                PathType = ChoicePathType.InstantSuccess,
                ActionTextTemplate = "Read and study",
                RequirementFormula = new CompoundRequirement(),
                CostTemplate = new ChoiceCost(),
                RewardTemplate = new ChoiceReward
                {
                    Insight = 1,
                    Health = 1,
                    Stamina = 1,
                    Focus = 1
                },
                ActionType = ChoiceActionType.Instant
            },
            new ChoiceTemplate
            {
                Id = $"{restSitId}_plan",
                PathType = ChoicePathType.InstantSuccess,
                ActionTextTemplate = "Plan tomorrow's route",
                RequirementFormula = new CompoundRequirement(),
                CostTemplate = new ChoiceCost(),
                RewardTemplate = new ChoiceReward
                {
                    Cunning = 1  // Planning builds cunning
                },
                ActionType = ChoiceActionType.Instant
            },
            new ChoiceTemplate
            {
                Id = $"{restSitId}_rest",
                PathType = ChoicePathType.InstantSuccess,
                ActionTextTemplate = "Rest peacefully",
                RequirementFormula = new CompoundRequirement(),
                CostTemplate = new ChoiceCost(),
                RewardTemplate = new ChoiceReward
                {
                    Health = 3,  // Full rest restores health
                    Stamina = 3,
                    Focus = 3
                },
                ActionType = ChoiceActionType.Instant
            },
            new ChoiceTemplate
            {
                Id = $"{restSitId}_socialize",
                PathType = ChoicePathType.InstantSuccess,
                ActionTextTemplate = "Visit the common room",
                RequirementFormula = new CompoundRequirement(),
                CostTemplate = new ChoiceCost(),
                RewardTemplate = new ChoiceReward
                {
                    Rapport = 1  // Socializing builds rapport
                },
                ActionType = ChoiceActionType.Instant
            }
        };
        }
        else
        {
            // Standard: Simple rest with recovery tradeoffs
            restChoices = new List<ChoiceTemplate>
            {
                new ChoiceTemplate
                {
                    Id = $"{restSitId}_full_rest",
                    PathType = ChoicePathType.InstantSuccess,
                    ActionTextTemplate = "Rest through the night",
                    RequirementFormula = new CompoundRequirement(),
                    CostTemplate = new ChoiceCost(),
                    RewardTemplate = new ChoiceReward
                    {
                        Health = 15,
                        Stamina = 15,
                        Focus = 10
                    },
                    ActionType = ChoiceActionType.Instant
                },
                new ChoiceTemplate
                {
                    Id = $"{restSitId}_light_rest",
                    PathType = ChoicePathType.InstantSuccess,
                    ActionTextTemplate = "Light rest",
                    RequirementFormula = new CompoundRequirement(),
                    CostTemplate = new ChoiceCost(),
                    RewardTemplate = new ChoiceReward
                    {
                        Health = 8,
                        Stamina = 8,
                        Focus = 5
                    },
                    ActionType = ChoiceActionType.Instant
                }
            };
        }

        SituationTemplate restSituation = new SituationTemplate
        {
            Id = restSitId,
            Name = "Evening in Room",
            Type = SituationType.Normal,
            SystemType = TacticalSystemType.Mental,
            NarrativeTemplate = null,
            ChoiceTemplates = restChoices,
            Priority = 90,
            NarrativeHints = new NarrativeHints
            {
                Tone = "contemplative",
                Theme = "preparation",
                Context = "evening_choices",
                Style = "introspective"
            },
            // Hierarchical placement: Override to private room, no NPC
            LocationFilter = new PlacementFilter
            {
                PlacementType = PlacementType.Location,
                LocationTags = new List<string> { "DEPENDENT_LOCATION:private_room" }  // Marker replaced at spawn time
            },
            NpcFilter = new PlacementFilter  // Explicit "no NPC" override (empty filter = no match)
            {
                PlacementType = PlacementType.NPC,
                PersonalityTypes = new List<PersonalityType>()  // Empty list = no NPC wanted
            },
            RouteFilter = null
        };

        // SITUATION 3: MORNING DEPARTURE (Identity Formation - Sir Brante A1)
        // Two approaches to leaving, each building different stat
        List<ChoiceTemplate> departChoices = new List<ChoiceTemplate>();

        // Determine rewards based on tutorial context
        bool isA1Tutorial = context.AStorySequence.HasValue && context.AStorySequence.Value == 1;

        ChoiceReward earlyDepartureReward = new ChoiceReward
        {
            ItemsToRemove = new List<string> { "generated:room_key" },
            Cunning = 1  // Early planning shows cunning
        };

        ChoiceReward socializeReward = new ChoiceReward
        {
            ItemsToRemove = new List<string> { "generated:room_key" },
            Rapport = 1,
            BondChanges = new List<BondChange>
            {
                new BondChange
                {
                    NpcId = "SITUATION_NPC",
                    Delta = 1,
                    Reason = "Grateful for kind farewell"
                }
            }
        };

        // A1 tutorial: ALL departure choices spawn A2
        if (isA1Tutorial)
        {
            earlyDepartureReward.ScenesToSpawn = new List<SceneSpawnReward>
            {
                new SceneSpawnReward { SceneTemplateId = "a2_morning" }
            };
            socializeReward.ScenesToSpawn = new List<SceneSpawnReward>
            {
                new SceneSpawnReward { SceneTemplateId = "a2_morning" }
            };
        }

        departChoices.Add(new ChoiceTemplate
        {
            Id = $"{departSitId}_early",
            PathType = ChoicePathType.InstantSuccess,
            ActionTextTemplate = "Leave early",
            RequirementFormula = new CompoundRequirement(),
            CostTemplate = new ChoiceCost(),
            RewardTemplate = earlyDepartureReward,
            ActionType = ChoiceActionType.Instant
        });

        departChoices.Add(new ChoiceTemplate
        {
            Id = $"{departSitId}_socialize",
            PathType = ChoicePathType.InstantSuccess,
            ActionTextTemplate = "Take time to socialize",
            RequirementFormula = new CompoundRequirement(),
            CostTemplate = new ChoiceCost(),
            RewardTemplate = socializeReward,
            ActionType = ChoiceActionType.Instant
        });

        SituationTemplate departSituation = new SituationTemplate
        {
            Id = departSitId,
            Name = "Morning Departure",
            Type = SituationType.Normal,
            SystemType = TacticalSystemType.Social,
            NarrativeTemplate = null,
            ChoiceTemplates = departChoices,
            Priority = 80,
            NarrativeHints = new NarrativeHints
            {
                Tone = "forward-looking",
                Theme = "departure",
                Context = "morning_departure",
                Style = "optimistic"
            },
            // Hierarchical placement: Override to private room, no NPC (same as Rest situation)
            LocationFilter = new PlacementFilter
            {
                PlacementType = PlacementType.Location,
                LocationTags = new List<string> { "DEPENDENT_LOCATION:private_room" }  // Marker replaced at spawn time
            },
            NpcFilter = new PlacementFilter  // Explicit "no NPC" override (empty filter = no match)
            {
                PlacementType = PlacementType.NPC,
                PersonalityTypes = new List<PersonalityType>()  // Empty list = no NPC wanted
            },
            RouteFilter = null
        };

        // Linear spawn rules: Negotiate → Rest → Depart
        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationId = negotiateSitId,
            Transitions = new List<SituationTransition>
        {
            new SituationTransition
            {
                SourceSituationId = negotiateSitId,
                DestinationSituationId = restSitId,
                Condition = TransitionCondition.Always
            },
            new SituationTransition
            {
                SourceSituationId = restSitId,
                DestinationSituationId = departSitId,
                Condition = TransitionCondition.Always
            }
        }
        };

        // Generate dependent resources for inn lodging
        DependentResourceCatalog.DependentResources resources =
            DependentResourceCatalog.GenerateForActivity(ServiceActivityType.Lodging);

        return new SceneArchetypeDefinition
        {
            SituationTemplates = new List<SituationTemplate>
        {
            negotiateSituation,
            restSituation,
            departSituation
        },
            SpawnRules = spawnRules,
            DependentLocations = new List<DependentLocationSpec> { resources.LocationSpec },
            DependentItems = new List<DependentItemSpec> { resources.ItemSpec }
        };
    }

    /// <summary>
    /// CONSEQUENCE_REFLECTION archetype
    ///
    /// FICTIONAL CONTEXT: Player faces consequences of previous choices
    /// REUSABLE: Works anywhere player needs to acknowledge consequences
    ///
    /// Situation Count: 1 (Standalone)
    /// Pattern: Standalone (single situation, no progression)
    ///
    /// Situation 1 - Reflection: Acknowledge consequences and choose response
    ///   - Archetype: crisis (emergency/decisive action pattern)
    ///   - Choices: 4 standard choices representing different coping strategies
    ///   - Rewards: Acknowledgment of reality, potential minor recovery
    ///   - No time advancement (reflective moment)
    ///
    /// No Dependent Resources: Uses existing world locations
    ///
    /// VERISIMILITUDE: Reflective moment after consequences. Player must acknowledge
    /// reality and decide how to move forward. Single beat, then returns to world.
    /// </summary>
    private static SceneArchetypeDefinition GenerateConsequenceReflection(int tier, GenerationContext context)
    {
        string situationId = "consequence_reflection";

        SituationArchetype reflectionArchetype = SituationArchetypeCatalog.GetArchetype("crisis");
        List<ChoiceTemplate> reflectionChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            reflectionArchetype,
            situationId,
            context);  // Pass context for universal scaling

        SituationTemplate reflectionSituation = new SituationTemplate
        {
            Id = situationId,
            Name = "Morning Reflection",
            Type = SituationType.Normal,
            NarrativeTemplate = null,  // AI generates from hints
            ChoiceTemplates = reflectionChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "regretful",
                Theme = "consequence",
                Context = "morning_after",
                Style = "somber"
            },
            // Hierarchical placement: Inherit scene base location, no NPC (solo reflection)
            LocationFilter = null,  // Inherits scene BaseLocationFilter
            NpcFilter = new PlacementFilter  // Explicit "no NPC" override (solo reflection)
            {
                PlacementType = PlacementType.NPC,
                PersonalityTypes = new List<PersonalityType>()  // Empty list = no NPC wanted
            },
            RouteFilter = null
        };

        // Standalone pattern - single situation, no transitions
        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Standalone,
            InitialSituationId = situationId,
            Transitions = new List<SituationTransition>()  // Empty - standalone
        };

        return new SceneArchetypeDefinition
        {
            SituationTemplates = new List<SituationTemplate> { reflectionSituation },
            SpawnRules = spawnRules,
            DependentLocations = new List<DependentLocationSpec>(),  // No dependent resources
            DependentItems = new List<DependentItemSpec>()
        };
    }

    /// <summary>
    /// DELIVERY_CONTRACT archetype
    ///
    /// FICTIONAL CONTEXT: NPC offers delivery contract, player negotiates payment
    /// REUSABLE: Works for any delivery contract throughout game
    ///
    /// Situation Count: 2
    /// Pattern: Linear (offer → negotiation)
    ///
    /// Situation 1 - Offer: NPC presents delivery opportunity
    ///   - Choices: 2 choices (accept/decline)
    ///   - Accept: Advance to negotiation
    ///   - Decline: Repeatable (player can reconsider)
    ///
    /// Situation 2 - Negotiation: Determine contract payment terms
    ///   - Archetype: service_negotiation (stat-gated/money-gated/challenge/fallback)
    ///   - Choices: 4 standard choices scaled by NPC demeanor and Quality
    ///   - Success rewards: Variable coin payment (upfront), optional items
    ///   - Tutorial (A-story sequence 2): Spawns A3 route travel from all choices
    ///
    /// No Dependent Resources: Uses existing world locations
    ///
    /// VERISIMILITUDE: Contract negotiation follows realistic flow - opportunity presents,
    /// player decides whether to engage, then negotiates specific terms. Reusable for any
    /// delivery contract throughout game. Tutorial adds A3 spawning when sequence == 2.
    /// </summary>
    private static SceneArchetypeDefinition GenerateDeliveryContract(int tier, GenerationContext context)
    {
        string sceneId = "delivery_contract";
        string offerSitId = $"{sceneId}_offer";
        string negotiateSitId = $"{sceneId}_negotiate";

        // SITUATION 1: CONTRACT OFFER
        SituationTemplate offerSituation = new SituationTemplate
        {
            Id = offerSitId,
            Name = "Delivery Opportunity",
            Type = SituationType.Normal,
            NarrativeTemplate = null,  // AI generates from hints
            ChoiceTemplates = new List<ChoiceTemplate>
            {
                new ChoiceTemplate
                {
                    Id = $"{offerSitId}_accept",
                    PathType = ChoicePathType.InstantSuccess,
                    ActionTextTemplate = "Accept the opportunity",
                    RequirementFormula = new CompoundRequirement(),
                    CostTemplate = new ChoiceCost(),
                    RewardTemplate = new ChoiceReward(),  // Advances to negotiation
                    ActionType = ChoiceActionType.Instant
                },
                new ChoiceTemplate
                {
                    Id = $"{offerSitId}_decline",
                    PathType = ChoicePathType.Fallback,
                    ActionTextTemplate = "Not right now",
                    RequirementFormula = new CompoundRequirement(),
                    CostTemplate = new ChoiceCost(),
                    RewardTemplate = new ChoiceReward(),  // Stays at offer (repeatable)
                    ActionType = ChoiceActionType.Instant
                }
            },
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "opportunistic",
                Theme = "contract_offer",
                Context = "delivery_opportunity",
                Style = "direct"
            },
            // Hierarchical placement: Inherit scene base filters
            LocationFilter = null,
            NpcFilter = null,
            RouteFilter = null
        };

        // SITUATION 2: CONTRACT NEGOTIATION
        SituationArchetype negotiateArchetype = SituationArchetypeCatalog.GetArchetype("service_negotiation");
        List<ChoiceTemplate> negotiateChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            negotiateArchetype,
            negotiateSitId,
            context);

        // Enrich negotiation choices with contract-specific rewards
        // Tutorial A2: Lower stat requirements + spawn A3
        List<ChoiceTemplate> enrichedNegotiateChoices = new List<ChoiceTemplate>();
        foreach (ChoiceTemplate choice in negotiateChoices)
        {
            ChoiceReward baseReward = choice.RewardTemplate ?? new ChoiceReward();
            ChoiceReward successReward = choice.OnSuccessReward;
            ChoiceReward failureReward = choice.OnFailureReward;
            CompoundRequirement modifiedRequirement = choice.RequirementFormula;

            // Tutorial A2 (sequence 2): Lower stat requirements to 2 (player has 3-5 from A1)
            // Grant immediate coin payment, spawn A3
            if (context.AStorySequence.HasValue && context.AStorySequence.Value == 2)
            {
                // Lower stat requirements from 3 to 2 (achievable with A1 stats)
                if (modifiedRequirement != null && modifiedRequirement.OrPaths != null)
                {
                    foreach (OrPath path in modifiedRequirement.OrPaths)
                    {
                        if (path.NumericRequirements != null)
                        {
                            foreach (NumericRequirement req in path.NumericRequirements)
                            {
                                if (req.Type == "PlayerStat" && req.Threshold == 3)
                                {
                                    req.Threshold = 2;  // Lower to 2 for tutorial A2
                                    req.Label = req.Label.Replace("3+", "2+");
                                }
                            }
                        }
                        // Update path label
                        if (path.Label != null && path.Label.Contains("3+"))
                        {
                            path.Label = path.Label.Replace("3+", "2+");
                        }
                    }
                }

                SceneSpawnReward a3Spawn = new SceneSpawnReward
                {
                    SceneTemplateId = "a3_route_travel"
                };

                // Determine choice type and set immediate coin payment
                bool hasStatRequirement = modifiedRequirement != null &&
                                         modifiedRequirement.OrPaths != null &&
                                         modifiedRequirement.OrPaths.Any();
                bool hasCoinCost = choice.CostTemplate != null && choice.CostTemplate.Coins > 0;

                // Add A3 spawning and immediate coin payment to appropriate reward based on path type
                switch (choice.PathType)
                {
                    case ChoicePathType.InstantSuccess:
                        // Distinguish stat path vs money path
                        if (hasStatRequirement)
                        {
                            // Stat path (Rapport): Coins = 15, spawn A3
                            baseReward.Coins = 15;
                        }
                        else if (hasCoinCost)
                        {
                            // Money path: Coins = 13, spawn A3
                            baseReward.Coins = 13;
                        }
                        baseReward.ScenesToSpawn = new List<SceneSpawnReward> { a3Spawn };
                        break;

                    case ChoicePathType.Challenge:
                        // Challenge path: Success = 17, Failure = 8, spawn A3 regardless
                        if (successReward == null)
                            successReward = new ChoiceReward();
                        successReward.Coins = 17;
                        successReward.ScenesToSpawn = new List<SceneSpawnReward> { a3Spawn };

                        if (failureReward == null)
                            failureReward = new ChoiceReward();
                        failureReward.Coins = 8;
                        failureReward.ScenesToSpawn = new List<SceneSpawnReward> { a3Spawn };
                        break;

                    case ChoicePathType.Fallback:
                        // Fallback: Coins = 8 (standard rate), spawn A3
                        baseReward.Coins = 8;
                        baseReward.ScenesToSpawn = new List<SceneSpawnReward> { a3Spawn };
                        break;
                }
            }

            enrichedNegotiateChoices.Add(new ChoiceTemplate
            {
                Id = choice.Id,
                PathType = choice.PathType,
                ActionTextTemplate = choice.ActionTextTemplate,
                RequirementFormula = modifiedRequirement,  // Use modified requirements for A2
                CostTemplate = choice.CostTemplate,
                RewardTemplate = baseReward,
                OnSuccessReward = successReward,
                OnFailureReward = failureReward,
                ActionType = choice.ActionType,
                ChallengeId = choice.ChallengeId,
                ChallengeType = choice.ChallengeType,
                DeckId = choice.DeckId,
                NavigationPayload = choice.NavigationPayload
            });
        }

        SituationTemplate negotiateSituation = new SituationTemplate
        {
            Id = negotiateSitId,
            Name = "Contract Terms",
            Type = SituationType.Normal,
            SystemType = negotiateArchetype.ChallengeType,
            NarrativeTemplate = null,  // AI generates from hints
            ChoiceTemplates = enrichedNegotiateChoices,
            Priority = 90,
            NarrativeHints = new NarrativeHints
            {
                Tone = "transactional",
                Theme = "negotiation",
                Context = "contract_terms",
                Style = "businesslike"
            },
            // Hierarchical placement: Inherit scene base filters
            LocationFilter = null,
            NpcFilter = null,
            RouteFilter = null
        };

        // Linear spawn rules: Offer → Negotiation
        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationId = offerSitId,
            Transitions = new List<SituationTransition>
            {
                new SituationTransition
                {
                    SourceSituationId = offerSitId,
                    DestinationSituationId = negotiateSitId,
                    Condition = TransitionCondition.Always
                }
            }
        };

        return new SceneArchetypeDefinition
        {
            SituationTemplates = new List<SituationTemplate>
            {
                offerSituation,
                negotiateSituation
            },
            SpawnRules = spawnRules,
            DependentLocations = new List<DependentLocationSpec>(),  // No dependent resources
            DependentItems = new List<DependentItemSpec>()
        };
    }

    /// <summary>
    /// ROUTE_SEGMENT_TRAVEL archetype
    ///
    /// FICTIONAL CONTEXT: Player traveling multi-segment route, encountering obstacles
    /// REUSABLE: Works for any route travel scene throughout game
    ///
    /// Situation Count: 5
    /// Pattern: Linear (4 route obstacles → arrival)
    ///
    /// Situations 1-4: Route obstacles at SegmentIndex 0-3
    ///   - Each uses 4-choice pattern (stat-gated/money-gated/challenge/fallback)
    ///   - Different challenge types (Physical, Mental, Social, simple)
    ///   - Geographic placement via RouteFilter with SegmentIndex
    ///
    /// Situation 5: Arrival at destination location
    ///   - Single choice to complete travel
    ///   - Rewards: Completion bonus (tutorial: +10 coins when sequence == 3)
    ///   - Tutorial: Spawns next A-story scene
    ///
    /// No Dependent Resources: Uses existing world routes/locations
    ///
    /// VERISIMILITUDE: Route travel involves sequential progression through route segments,
    /// encountering obstacles that require decisions, before arriving at destination.
    /// Reusable for any multi-segment route travel throughout game.
    /// </summary>
    private static SceneArchetypeDefinition GenerateRouteSegmentTravel(int tier, GenerationContext context)
    {
        string sceneId = "route_segment_travel";
        string obstacle1SitId = $"{sceneId}_obstacle1";
        string obstacle2SitId = $"{sceneId}_obstacle2";
        string obstacle3SitId = $"{sceneId}_obstacle3";
        string approachSitId = $"{sceneId}_approach";
        string arrivalSitId = $"{sceneId}_arrival";

        // Tutorial A3 (sequence 3): Populate stat requirements + crisis damage control
        bool isA3Tutorial = context.AStorySequence.HasValue && context.AStorySequence.Value == 3;

        // SITUATION 1: PHYSICAL OBSTACLE (Segment 0)
        CompoundRequirement obstacle1AuthorityReq = new CompoundRequirement();
        ChoiceReward obstacle1FallbackReward = new ChoiceReward();

        if (isA3Tutorial)
        {
            // A3 CRISIS: Authority 3 required, fallback causes damage
            obstacle1AuthorityReq.OrPaths = new List<OrPath>
            {
                new OrPath
                {
                    Label = "Authority 3+",
                    NumericRequirements = new List<NumericRequirement>
                    {
                        new NumericRequirement
                        {
                            Type = "PlayerStat",
                            Context = "Authority",
                            Threshold = 3,
                            Label = "Authority 3+"
                        }
                    }
                }
            };
            obstacle1FallbackReward.Health = -10;  // Crisis: Damage control choice
        }

        SituationTemplate obstacle1Situation = new SituationTemplate
        {
            Id = obstacle1SitId,
            Name = "Forest Obstacle",
            Type = SituationType.Normal,
            NarrativeTemplate = null,  // AI generates from hints
            ChoiceTemplates = new List<ChoiceTemplate>
            {
                new ChoiceTemplate
                {
                    Id = $"{obstacle1SitId}_authority",
                    PathType = ChoicePathType.InstantSuccess,
                    ActionTextTemplate = "Direct locals to clear path",
                    RequirementFormula = obstacle1AuthorityReq,
                    CostTemplate = new ChoiceCost(),
                    RewardTemplate = new ChoiceReward(),
                    ActionType = ChoiceActionType.Instant
                },
                new ChoiceTemplate
                {
                    Id = $"{obstacle1SitId}_money",
                    PathType = ChoicePathType.InstantSuccess,
                    ActionTextTemplate = "Pay locals to clear it",
                    RequirementFormula = new CompoundRequirement(),
                    CostTemplate = new ChoiceCost { Coins = 5 },
                    RewardTemplate = new ChoiceReward(),
                    ActionType = ChoiceActionType.Instant
                },
                new ChoiceTemplate
                {
                    Id = $"{obstacle1SitId}_challenge",
                    PathType = ChoicePathType.Challenge,
                    ActionTextTemplate = "Clear obstacle yourself",
                    RequirementFormula = new CompoundRequirement(),
                    CostTemplate = new ChoiceCost(),  // Time/stamina costs
                    RewardTemplate = new ChoiceReward(),
                    OnSuccessReward = new ChoiceReward(),  // Understanding +1
                    OnFailureReward = new ChoiceReward(),  // Health -10
                    ChallengeType = TacticalSystemType.Physical,
                    ActionType = ChoiceActionType.Instant
                },
                new ChoiceTemplate
                {
                    Id = $"{obstacle1SitId}_fallback",
                    PathType = ChoicePathType.Fallback,
                    ActionTextTemplate = "Take longer detour",
                    RequirementFormula = new CompoundRequirement(),
                    CostTemplate = new ChoiceCost(),  // Time cost
                    RewardTemplate = obstacle1FallbackReward,  // A3: Health -10 (crisis!)
                    ActionType = ChoiceActionType.Instant
                }
            },
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "challenging",
                Theme = "physical_obstacle",
                Context = "route_travel",
                Style = "action"
            },
            RouteFilter = new PlacementFilter
            {
                PlacementType = PlacementType.Route,
                SegmentIndex = 0  // First segment
            },
            LocationFilter = null,
            NpcFilter = null
        };

        // SITUATION 2: MENTAL OBSTACLE (Segment 1)
        CompoundRequirement obstacle2InsightReq = new CompoundRequirement();
        ChoiceReward obstacle2FallbackReward = new ChoiceReward();

        if (isA3Tutorial)
        {
            // A3 CRISIS: Insight 3 required, fallback causes stamina loss
            obstacle2InsightReq.OrPaths = new List<OrPath>
            {
                new OrPath
                {
                    Label = "Insight 3+",
                    NumericRequirements = new List<NumericRequirement>
                    {
                        new NumericRequirement
                        {
                            Type = "PlayerStat",
                            Context = "Insight",
                            Threshold = 3,
                            Label = "Insight 3+"
                        }
                    }
                }
            };
            obstacle2FallbackReward.Stamina = -10;  // Crisis: Exhausting failed attempt
        }

        SituationTemplate obstacle2Situation = new SituationTemplate
        {
            Id = obstacle2SitId,
            Name = "River Crossing",
            Type = SituationType.Normal,
            NarrativeTemplate = null,
            ChoiceTemplates = new List<ChoiceTemplate>
            {
                new ChoiceTemplate
                {
                    Id = $"{obstacle2SitId}_insight",
                    PathType = ChoicePathType.InstantSuccess,
                    ActionTextTemplate = "Spot safe shallows through analysis",
                    RequirementFormula = obstacle2InsightReq,
                    CostTemplate = new ChoiceCost(),
                    RewardTemplate = new ChoiceReward(),
                    ActionType = ChoiceActionType.Instant
                },
                new ChoiceTemplate
                {
                    Id = $"{obstacle2SitId}_money",
                    PathType = ChoicePathType.InstantSuccess,
                    ActionTextTemplate = "Pay ferryman",
                    RequirementFormula = new CompoundRequirement(),
                    CostTemplate = new ChoiceCost { Coins = 8 },
                    RewardTemplate = new ChoiceReward(),
                    ActionType = ChoiceActionType.Instant
                },
                new ChoiceTemplate
                {
                    Id = $"{obstacle2SitId}_challenge",
                    PathType = ChoicePathType.Challenge,
                    ActionTextTemplate = "Study crossing carefully",
                    RequirementFormula = new CompoundRequirement(),
                    CostTemplate = new ChoiceCost(),  // Time/focus costs
                    RewardTemplate = new ChoiceReward(),
                    OnSuccessReward = new ChoiceReward(),  // Understanding +1
                    OnFailureReward = new ChoiceReward(),  // Health -5
                    ChallengeType = TacticalSystemType.Mental,
                    ActionType = ChoiceActionType.Instant
                },
                new ChoiceTemplate
                {
                    Id = $"{obstacle2SitId}_fallback",
                    PathType = ChoicePathType.Fallback,
                    ActionTextTemplate = "Wade across carefully",
                    RequirementFormula = new CompoundRequirement(),
                    CostTemplate = new ChoiceCost(),
                    RewardTemplate = obstacle2FallbackReward,  // A3: Stamina -10 (crisis!)
                    ActionType = ChoiceActionType.Instant
                }
            },
            Priority = 90,
            NarrativeHints = new NarrativeHints
            {
                Tone = "analytical",
                Theme = "mental_challenge",
                Context = "route_travel",
                Style = "thoughtful"
            },
            RouteFilter = new PlacementFilter
            {
                PlacementType = PlacementType.Route,
                SegmentIndex = 1  // Second segment
            },
            LocationFilter = null,
            NpcFilter = null
        };

        // SITUATION 3: SOCIAL OBSTACLE (Segment 2)
        CompoundRequirement obstacle3RapportReq = new CompoundRequirement();
        ChoiceReward obstacle3FallbackReward = new ChoiceReward();

        if (isA3Tutorial)
        {
            // A3 CRISIS: Rapport 3 required, fallback costs coins
            obstacle3RapportReq.OrPaths = new List<OrPath>
            {
                new OrPath
                {
                    Label = "Rapport 3+",
                    NumericRequirements = new List<NumericRequirement>
                    {
                        new NumericRequirement
                        {
                            Type = "PlayerStat",
                            Context = "Rapport",
                            Threshold = 3,
                            Label = "Rapport 3+"
                        }
                    }
                }
            };
            obstacle3FallbackReward.Coins = -5;  // Crisis: Forced to pay penalty
        }

        SituationTemplate obstacle3Situation = new SituationTemplate
        {
            Id = obstacle3SitId,
            Name = "Checkpoint Guard",
            Type = SituationType.Normal,
            NarrativeTemplate = null,
            ChoiceTemplates = new List<ChoiceTemplate>
            {
                new ChoiceTemplate
                {
                    Id = $"{obstacle3SitId}_rapport",
                    PathType = ChoicePathType.InstantSuccess,
                    ActionTextTemplate = "Friendly conversation about the road",
                    RequirementFormula = obstacle3RapportReq,
                    CostTemplate = new ChoiceCost(),
                    RewardTemplate = new ChoiceReward(),  // Guard bond +1
                    ActionType = ChoiceActionType.Instant
                },
                new ChoiceTemplate
                {
                    Id = $"{obstacle3SitId}_money",
                    PathType = ChoicePathType.InstantSuccess,
                    ActionTextTemplate = "Pay toll and inspection fee",
                    RequirementFormula = new CompoundRequirement(),
                    CostTemplate = new ChoiceCost { Coins = 10 },
                    RewardTemplate = new ChoiceReward(),
                    ActionType = ChoiceActionType.Instant
                },
                new ChoiceTemplate
                {
                    Id = $"{obstacle3SitId}_challenge",
                    PathType = ChoicePathType.Challenge,
                    ActionTextTemplate = "Persuade to waive toll",
                    RequirementFormula = new CompoundRequirement(),
                    CostTemplate = new ChoiceCost(),  // Time/resolve costs
                    RewardTemplate = new ChoiceReward(),
                    OnSuccessReward = new ChoiceReward(),  // Understanding +1, Guard bond +2
                    OnFailureReward = new ChoiceReward(),  // Coins -12, Guard bond -1
                    ChallengeType = TacticalSystemType.Social,
                    ActionType = ChoiceActionType.Instant
                },
                new ChoiceTemplate
                {
                    Id = $"{obstacle3SitId}_fallback",
                    PathType = ChoicePathType.Fallback,
                    ActionTextTemplate = "Wait patiently through thorough inspection",
                    RequirementFormula = new CompoundRequirement(),
                    CostTemplate = new ChoiceCost(),  // Time cost
                    RewardTemplate = obstacle3FallbackReward,  // A3: Coins -5 (crisis!)
                    ActionType = ChoiceActionType.Instant
                }
            },
            Priority = 80,
            NarrativeHints = new NarrativeHints
            {
                Tone = "transactional",
                Theme = "social_negotiation",
                Context = "route_travel",
                Style = "diplomatic"
            },
            RouteFilter = new PlacementFilter
            {
                PlacementType = PlacementType.Route,
                SegmentIndex = 2  // Third segment
            },
            LocationFilter = null,
            NpcFilter = null
        };

        // SITUATION 4: FINAL APPROACH (Segment 3)
        SituationTemplate approachSituation = new SituationTemplate
        {
            Id = approachSitId,
            Name = "Final Approach",
            Type = SituationType.Normal,
            NarrativeTemplate = null,
            ChoiceTemplates = new List<ChoiceTemplate>
            {
                new ChoiceTemplate
                {
                    Id = $"{approachSitId}_continue",
                    PathType = ChoicePathType.InstantSuccess,
                    ActionTextTemplate = "Head to destination",
                    RequirementFormula = new CompoundRequirement(),
                    CostTemplate = new ChoiceCost(),
                    RewardTemplate = new ChoiceReward(),
                    ActionType = ChoiceActionType.Instant
                },
                new ChoiceTemplate
                {
                    Id = $"{approachSitId}_rest",
                    PathType = ChoicePathType.Fallback,
                    ActionTextTemplate = "Catch breath before arrival",
                    RequirementFormula = new CompoundRequirement(),
                    CostTemplate = new ChoiceCost(),  // Time cost
                    RewardTemplate = new ChoiceReward(),  // Stamina +5, Focus +5
                    ActionType = ChoiceActionType.Instant
                }
            },
            Priority = 70,
            NarrativeHints = new NarrativeHints
            {
                Tone = "anticipatory",
                Theme = "arrival",
                Context = "route_travel",
                Style = "forward-looking"
            },
            RouteFilter = new PlacementFilter
            {
                PlacementType = PlacementType.Route,
                SegmentIndex = 3  // Fourth segment
            },
            LocationFilter = null,
            NpcFilter = null
        };

        // SITUATION 5: ARRIVAL AT DESTINATION
        List<ChoiceTemplate> arrivalChoices = new List<ChoiceTemplate>
        {
            new ChoiceTemplate
            {
                Id = $"{arrivalSitId}_complete",
                PathType = ChoicePathType.InstantSuccess,
                ActionTextTemplate = "Accept completion bonus and conclude business",
                RequirementFormula = new CompoundRequirement(),
                CostTemplate = new ChoiceCost(),
                RewardTemplate = new ChoiceReward(),
                ActionType = ChoiceActionType.Instant
            }
        };

        // Tutorial A3 (sequence 3): Fixed +10 coin completion bonus (totals with A2: 25/23/27/18/20)
        if (context.AStorySequence.HasValue && context.AStorySequence.Value == 3)
        {
            arrivalChoices[0].RewardTemplate.Coins = 10;
        }

        SituationTemplate arrivalSituation = new SituationTemplate
        {
            Id = arrivalSitId,
            Name = "Delivery Complete",
            Type = SituationType.Normal,
            NarrativeTemplate = null,
            ChoiceTemplates = arrivalChoices,
            Priority = 60,
            NarrativeHints = new NarrativeHints
            {
                Tone = "conclusive",
                Theme = "completion",
                Context = "delivery_success",
                Style = "satisfying"
            },
            LocationFilter = null,  // Inherits scene base (warehouse)
            NpcFilter = null,       // Inherits scene base (warehouse master)
            RouteFilter = null      // At destination location, not on route
        };

        // Linear spawn rules: Obstacle1 → Obstacle2 → Obstacle3 → Approach → Arrival
        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationId = obstacle1SitId,
            Transitions = new List<SituationTransition>
            {
                new SituationTransition
                {
                    SourceSituationId = obstacle1SitId,
                    DestinationSituationId = obstacle2SitId,
                    Condition = TransitionCondition.Always
                },
                new SituationTransition
                {
                    SourceSituationId = obstacle2SitId,
                    DestinationSituationId = obstacle3SitId,
                    Condition = TransitionCondition.Always
                },
                new SituationTransition
                {
                    SourceSituationId = obstacle3SitId,
                    DestinationSituationId = approachSitId,
                    Condition = TransitionCondition.Always
                },
                new SituationTransition
                {
                    SourceSituationId = approachSitId,
                    DestinationSituationId = arrivalSitId,
                    Condition = TransitionCondition.Always
                }
            }
        };

        return new SceneArchetypeDefinition
        {
            SituationTemplates = new List<SituationTemplate>
            {
                obstacle1Situation,
                obstacle2Situation,
                obstacle3Situation,
                approachSituation,
                arrivalSituation
            },
            SpawnRules = spawnRules,
            DependentLocations = new List<DependentLocationSpec>(),  // No dependent resources
            DependentItems = new List<DependentItemSpec>()
        };
    }
}
