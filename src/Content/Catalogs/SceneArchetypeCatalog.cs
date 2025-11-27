/// <summary>
/// ⚠️ PARSE-TIME ONLY CATALOGUE ⚠️
///
/// Generates complete multi-situation Scene structures from strongly-typed archetype enums at PARSE TIME.
/// Each archetype is INTENTIONALLY DESIGNED for specific fictional contexts with verisimilitude.
///
/// HIGHLANDER COMPLIANT: ONE catalogue for ALL scene archetypes (A-story, B-story, C-story)
///
/// CATALOGUE PATTERN COMPLIANCE:
/// - Called ONLY from SceneTemplateParser at PARSE TIME
/// - NEVER called from facades, managers, or runtime code
/// - NEVER imported except in parser classes
/// - Generates SceneArchetypeDefinition that parser embeds in SceneTemplate
/// - Translation happens ONCE at game initialization
///
/// ARCHITECTURE:
/// JSON specifies SceneArchetypeType enum → Parser calls catalogue → Receives SituationTemplates + SpawnRules
/// → Parser stores in SceneTemplate → Runtime queries GameWorld.SceneTemplates (NO catalogue calls)
///
/// SCENE ARCHETYPES (13 total - Reusable patterns):
///
/// SERVICE PATTERNS (4):
/// - InnLodging: 3-situation inn lodging flow (negotiate → rest → depart)
/// - ConsequenceReflection: Single-situation consequence acknowledgment
/// - DeliveryContract: Contract acceptance and delivery flow
/// - RouteSegmentTravel: Travel between locations
///
/// NARRATIVE PATTERNS (9):
/// - SeekAudience: Player seeks audience with authority figure (negotiate_access → audience)
/// - InvestigateLocation: Player investigates location for clues (search → analyze → conclude)
/// - GatherTestimony: Player gathers testimony from witnesses (approach → interview)
/// - ConfrontAntagonist: Player confronts antagonist (accuse → resolve)
/// - MeetOrderMember: Player meets order member (contact → negotiate → revelation)
/// - DiscoverArtifact: Player discovers artifact (locate → acquire)
/// - UncoverConspiracy: Player uncovers conspiracy (suspect → proof → expose → consequence)
/// - UrgentDecision: Player faces urgent decision (crisis → decision)
/// - MoralCrossroads: Player faces moral dilemma (dilemma → choice → consequence)
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
    /// Generate scene archetype definition from strongly-typed enum.
    /// Called at parse time to generate complete scene structure.
    /// Compiler ensures exhaustiveness - no runtime unknown archetype errors.
    /// </summary>
    public static SceneArchetypeDefinition Generate(
        SceneArchetypeType archetypeType,
        int tier,
        GenerationContext context)
    {
        return archetypeType switch
        {
            // Service patterns (4)
            SceneArchetypeType.InnLodging => GenerateInnLodging(tier, context),
            SceneArchetypeType.ConsequenceReflection => GenerateConsequenceReflection(tier, context),
            SceneArchetypeType.DeliveryContract => GenerateDeliveryContract(tier, context),
            SceneArchetypeType.RouteSegmentTravel => GenerateRouteSegmentTravel(tier, context),

            // Narrative patterns (9)
            SceneArchetypeType.SeekAudience => GenerateSeekAudience(tier, context),
            SceneArchetypeType.InvestigateLocation => GenerateInvestigateLocation(tier, context),
            SceneArchetypeType.GatherTestimony => GenerateGatherTestimony(tier, context),
            SceneArchetypeType.ConfrontAntagonist => GenerateConfrontAntagonist(tier, context),
            SceneArchetypeType.MeetOrderMember => GenerateMeetOrderMember(tier, context),
            SceneArchetypeType.DiscoverArtifact => GenerateDiscoverArtifact(tier, context),
            SceneArchetypeType.UncoverConspiracy => GenerateUncoverConspiracy(tier, context),
            SceneArchetypeType.UrgentDecision => GenerateUrgentDecision(tier, context),
            SceneArchetypeType.MoralCrossroads => GenerateMoralCrossroads(tier, context),

            _ => throw new InvalidOperationException($"Unhandled scene archetype type: {archetypeType}")
        };
    }

    /// <summary>
    /// Get available archetypes for category (for procedural A-story generation)
    /// Returns archetypes currently implemented in catalog for given category
    /// Prevents drift between catalog and procedural selection lists
    ///
    /// CATEGORIES (4-part rotation):
    /// - Investigation: SeekAudience, InvestigateLocation, GatherTestimony
    /// - Social: MeetOrderMember
    /// - Confrontation: ConfrontAntagonist
    /// - Crisis: UrgentDecision, MoralCrossroads
    ///
    /// Note: Discovery archetypes (DiscoverArtifact, UncoverConspiracy) not included in rotation
    /// Can be added to rotation cycle when design requires them
    /// </summary>
    public static List<SceneArchetypeType> GetArchetypesForCategory(string category)
    {
        return category switch
        {
            "Investigation" => new List<SceneArchetypeType>
            {
                SceneArchetypeType.InvestigateLocation,
                SceneArchetypeType.GatherTestimony,
                SceneArchetypeType.SeekAudience
            },
            "Social" => new List<SceneArchetypeType>
            {
                SceneArchetypeType.MeetOrderMember
            },
            "Confrontation" => new List<SceneArchetypeType>
            {
                SceneArchetypeType.ConfrontAntagonist
            },
            "Crisis" => new List<SceneArchetypeType>
            {
                SceneArchetypeType.UrgentDecision,
                SceneArchetypeType.MoralCrossroads
            },
            _ => new List<SceneArchetypeType>() // Empty list for unknown category
        };
    }

    /// <summary>
    /// Get all available narrative archetype types (for validation and procedural selection)
    /// Returns list of all implemented narrative archetypes (excludes service patterns)
    /// </summary>
    public static List<SceneArchetypeType> GetAllNarrativeArchetypes()
    {
        return new List<SceneArchetypeType>
        {
            SceneArchetypeType.SeekAudience,
            SceneArchetypeType.InvestigateLocation,
            SceneArchetypeType.GatherTestimony,
            SceneArchetypeType.ConfrontAntagonist,
            SceneArchetypeType.MeetOrderMember,
            SceneArchetypeType.DiscoverArtifact,
            SceneArchetypeType.UncoverConspiracy,
            SceneArchetypeType.UrgentDecision,
            SceneArchetypeType.MoralCrossroads
        };
    }

    // ===================================================================
    // SERVICE PATTERNS (4) - Transactional C-story content
    // ===================================================================

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

        // Generate location filter for lodging service
        // EntityResolver.FindOrCreateLocation uses filter to find existing OR create new
        DependentResourceCatalog.DependentResources resources =
            DependentResourceCatalog.GenerateForActivity(ServiceActivityType.Lodging);
        PlacementFilter serviceLocationFilter = resources.LocationFilter;

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
                ActionTextTemplate = "Chat warmly with the innkeeper",
                RequirementFormula = new CompoundRequirement(),
                CostTemplate = new ChoiceCost { Coins = 5 },
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
                CostTemplate = new ChoiceCost { Coins = 5 },
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
                CostTemplate = new ChoiceCost { Coins = 5 },
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
                CostTemplate = new ChoiceCost { Coins = 5 },
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
            SituationArchetype negotiateArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.ServiceNegotiation);
            negotiateChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
                negotiateArchetype,
                negotiateSitId,
                context);
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
            // Explicit categorical filters for inn lodging negotiation
            // EntityResolver searches ONLY within CurrentVenue for matching entities
            LocationFilter = new PlacementFilter
            {
                PlacementType = PlacementType.Location,
                Purpose = LocationPurpose.Commerce
            },
            NpcFilter = new PlacementFilter
            {
                PlacementType = PlacementType.NPC,
                Profession = Professions.Innkeeper
            },
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
            // Use LocationFilter for categorical matching via EntityResolver
            LocationFilter = serviceLocationFilter,
            NpcFilter = null,
            RouteFilter = null
        };

        // SITUATION 3: MORNING DEPARTURE (Identity Formation - Sir Brante A1)
        // Two approaches to leaving, each building different stat
        List<ChoiceTemplate> departChoices = new List<ChoiceTemplate>();

        // Determine rewards based on tutorial context
        bool isA1Tutorial = context.AStorySequence.HasValue && context.AStorySequence.Value == 1;

        ChoiceReward earlyDepartureReward = new ChoiceReward
        {
            Cunning = 1  // Early planning shows cunning
        };

        ChoiceReward socializeReward = new ChoiceReward
        {
            Rapport = 1
            // Bond changes with situation NPC not supported in catalog-generated templates
            // (NPC not resolved until instantiation)
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
            // Same filter as rest situation = same location resolved
            LocationFilter = serviceLocationFilter,
            NpcFilter = null,
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

        return new SceneArchetypeDefinition
        {
            SituationTemplates = new List<SituationTemplate>
            {
                negotiateSituation,
                restSituation,
                departSituation
            },
            SpawnRules = spawnRules
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

        SituationArchetype reflectionArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Crisis);
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
            // Explicit filters for solo reflection (any location, no NPC)
            LocationFilter = new PlacementFilter
            {
                PlacementType = PlacementType.Location
                // Empty filter = matches any location within venue
            },
            NpcFilter = null,
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
            SpawnRules = spawnRules
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
            // Explicit filters for delivery contract offer (merchant at commerce location)
            LocationFilter = new PlacementFilter
            {
                PlacementType = PlacementType.Location,
                Purpose = LocationPurpose.Commerce
            },
            NpcFilter = new PlacementFilter
            {
                PlacementType = PlacementType.NPC,
                Profession = Professions.Merchant
            },
            RouteFilter = null
        };

        // SITUATION 2: CONTRACT NEGOTIATION
        SituationArchetype negotiateArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.ServiceNegotiation);
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
                        // Fallback: Player breaks commitment after accepting - consequences but no requirements
                        // See arc42/08 §8.16 Fallback Context Rules: Post-commitment fallback has penalty
                        baseReward.Rapport = -1;  // Breaking commitment disappoints the merchant
                        break;
                }
            }

            // Override action text for Fallback to reflect post-commitment context
            string enrichedActionText = choice.PathType == ChoicePathType.Fallback
                ? "Back out of the deal"
                : choice.ActionTextTemplate;

            enrichedNegotiateChoices.Add(new ChoiceTemplate
            {
                Id = choice.Id,
                PathType = choice.PathType,  // Keep original PathType (Fallback stays Fallback)
                ActionTextTemplate = enrichedActionText,
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
            // Explicit filters for contract negotiation (merchant at commerce location)
            LocationFilter = new PlacementFilter
            {
                PlacementType = PlacementType.Location,
                Purpose = LocationPurpose.Commerce
            },
            NpcFilter = new PlacementFilter
            {
                PlacementType = PlacementType.NPC,
                Profession = Professions.Merchant
            },
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
            SpawnRules = spawnRules
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
            LocationFilter = new PlacementFilter
            {
                PlacementType = PlacementType.Location,
                Proximity = PlacementProximity.SameLocation
            },
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
            LocationFilter = new PlacementFilter
            {
                PlacementType = PlacementType.Location,
                Proximity = PlacementProximity.SameLocation
            },
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
            LocationFilter = new PlacementFilter
            {
                PlacementType = PlacementType.Location,
                Proximity = PlacementProximity.SameLocation
            },
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
            LocationFilter = new PlacementFilter
            {
                PlacementType = PlacementType.Location,
                Proximity = PlacementProximity.SameLocation
            },
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
            // RouteDestination: Use route's destination location (resolved from earlier RouteFilter situations)
            // Purpose remains as categorical validation hint
            LocationFilter = new PlacementFilter
            {
                PlacementType = PlacementType.Location,
                Proximity = PlacementProximity.RouteDestination,
                Purpose = LocationPurpose.Commerce
            },
            NpcFilter = new PlacementFilter
            {
                PlacementType = PlacementType.NPC,
                Profession = Professions.Merchant
            },
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
            SpawnRules = spawnRules
        };
    }

    // ===================================================================
    // NARRATIVE PATTERNS (9) - A-story and B-story content
    // ===================================================================

    /// <summary>
    /// SEEK_AUDIENCE archetype
    ///
    /// FICTIONAL CONTEXT: Player seeks audience with authority figure or important NPC
    /// STORY PURPOSE: Gatekeeper challenge to access next story beat
    ///
    /// Situation Count: 2
    /// Pattern: Linear (negotiate_access → audience)
    ///
    /// Situation 1 - Negotiate Access: Convince gatekeeper to grant audience
    ///   - Archetype: negotiation (Diplomacy/coins/challenge/fallback)
    ///   - Guaranteed path: Fallback (wait patiently) always succeeds
    ///   - Success rewards: Unlock meeting location, advance to audience
    ///
    /// Situation 2 - Audience: Meet with authority figure
    ///   - Archetype: confrontation (Authority/Social challenge)
    ///   - Guaranteed path: Fallback (respectful submission) always succeeds
    ///   - Success rewards: Story revelation, next A-scene spawn
    ///
    /// Dependent Resources: Meeting chamber (generated location)
    ///
    /// GUARANTEED PROGRESSION: Both situations have fallback choices with no requirements
    /// FINAL SITUATION: ALL choices spawn next A-scene (guaranteed forward progress)
    /// </summary>
    private static SceneArchetypeDefinition GenerateSeekAudience(int tier, GenerationContext context)
    {
        string sceneId = "seek_audience";

        // SITUATION 1: NEGOTIATE ACCESS
        SituationArchetype negotiateArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Negotiation);
        List<ChoiceTemplate> negotiateChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            negotiateArchetype,
            $"{sceneId}_negotiate",
            context);

        // Enrich with unlock rewards
        List<ChoiceTemplate> enrichedNegotiateChoices = new List<ChoiceTemplate>();
        foreach (ChoiceTemplate choice in negotiateChoices)
        {
            ChoiceReward reward = choice.RewardTemplate ?? new ChoiceReward();

            // NEW ARCHITECTURE: Dual-model accessibility - situation presence at dependent location grants access
            // No need for reward-based unlock - when situation advances to meeting_chamber, access is automatic

            enrichedNegotiateChoices.Add(new ChoiceTemplate
            {
                Id = choice.Id,
                PathType = choice.PathType,
                ActionTextTemplate = choice.ActionTextTemplate,
                RequirementFormula = choice.RequirementFormula,
                CostTemplate = choice.CostTemplate,
                RewardTemplate = reward,
                ActionType = choice.ActionType,
                ChallengeType = choice.ChallengeType
            });
        }

        SituationTemplate negotiateSituation = new SituationTemplate
        {
            Id = $"{sceneId}_negotiate",
            Type = SituationType.Normal,
            ChoiceTemplates = enrichedNegotiateChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "formal",
                Theme = "access_negotiation",
                Context = "seeking_audience",
                Style = "diplomatic"
            },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        // HIGHLANDER: Use PlacementFilter for categorical matching
        // EntityResolver.FindOrCreateLocation finds existing OR creates new
        PlacementFilter meetingChamberFilter = new PlacementFilter
        {
            PlacementType = PlacementType.Location,
            Privacy = LocationPrivacy.Private,
            Safety = LocationSafety.Safe,
            Activity = LocationActivity.Quiet,
            Purpose = LocationPurpose.Governance,
            SelectionStrategy = PlacementSelectionStrategy.Random
        };

        // SITUATION 2: AUDIENCE
        SituationArchetype audienceArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Confrontation);
        List<ChoiceTemplate> audienceChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            audienceArchetype,
            $"{sceneId}_audience",
            context);

        SituationTemplate audienceSituation = new SituationTemplate
        {
            Id = $"{sceneId}_audience",
            Type = SituationType.Normal,
            ChoiceTemplates = audienceChoices,
            Priority = 90,
            NarrativeHints = new NarrativeHints
            {
                Tone = "tense",
                Theme = "authority_meeting",
                Context = "formal_audience",
                Style = "dramatic"
            },
            // HIGHLANDER: Use LocationFilter for categorical matching
            LocationFilter = meetingChamberFilter,
            NpcFilter = null,
            RouteFilter = null
        };

        // Linear spawn rules
        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationId = $"{sceneId}_negotiate",
            Transitions = new List<SituationTransition>
        {
            new SituationTransition
            {
                SourceSituationId = $"{sceneId}_negotiate",
                DestinationSituationId = $"{sceneId}_audience",
                Condition = TransitionCondition.Always
            }
        }
        };

        List<SituationTemplate> situations = new List<SituationTemplate>
    {
        negotiateSituation,
        audienceSituation
    };

        // CRITICAL: Enrich final situation to spawn next A-scene (infinite progression)
        EnrichFinalSituationWithNextASceneSpawn(situations, context);

        return new SceneArchetypeDefinition
        {
            SituationTemplates = situations,
            SpawnRules = spawnRules
        };
    }

    /// <summary>
    /// INVESTIGATE_LOCATION archetype
    ///
    /// FICTIONAL CONTEXT: Player investigates location for clues about the Order
    /// STORY PURPOSE: Procedural investigation advancing mystery
    ///
    /// Situation Count: 3
    /// Pattern: Linear (search → analyze → conclude)
    ///
    /// Classic investigation flow reusable for any location-based mystery beat.
    /// </summary>
    private static SceneArchetypeDefinition GenerateInvestigateLocation(int tier, GenerationContext context)
    {
        string sceneId = "investigate_location";

        // SITUATION 1: SEARCH LOCATION
        SituationArchetype searchArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Investigation);
        List<ChoiceTemplate> searchChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            searchArchetype,
            $"{sceneId}_search",
            context);

        // Enrich with evidence rewards
        List<ChoiceTemplate> enrichedSearchChoices = new List<ChoiceTemplate>();
        foreach (ChoiceTemplate choice in searchChoices)
        {
            // NOTE: Dependent item granting (evidence) cannot be handled at catalog time
            // because the items don't exist until scene instantiation.

            enrichedSearchChoices.Add(choice); // Pass through unchanged
        }

        SituationTemplate searchSituation = new SituationTemplate
        {
            Id = $"{sceneId}_search",
            Type = SituationType.Normal,
            ChoiceTemplates = enrichedSearchChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "mysterious",
                Theme = "investigation",
                Context = "searching_clues",
                Style = "atmospheric"
            },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,  // Solo situation - no NPC
            RouteFilter = null
        };

        // SITUATION 2: ANALYZE EVIDENCE
        SituationArchetype analyzeArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Investigation);
        List<ChoiceTemplate> analyzeChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            analyzeArchetype,
            $"{sceneId}_analyze",
            context);

        SituationTemplate analyzeSituation = new SituationTemplate
        {
            Id = $"{sceneId}_analyze",
            Type = SituationType.Normal,
            ChoiceTemplates = analyzeChoices,
            Priority = 90,
            NarrativeHints = new NarrativeHints
            {
                Tone = "focused",
                Theme = "deduction",
                Context = "analyzing_evidence",
                Style = "cerebral"
            },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,  // Solo situation - no NPC
            RouteFilter = null
        };

        // SITUATION 3: CONCLUDE INVESTIGATION
        SituationArchetype concludeArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Investigation);
        List<ChoiceTemplate> concludeChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            concludeArchetype,
            $"{sceneId}_conclude",
            context);

        SituationTemplate concludeSituation = new SituationTemplate
        {
            Id = $"{sceneId}_conclude",
            Type = SituationType.Normal,
            ChoiceTemplates = concludeChoices,
            Priority = 80,
            NarrativeHints = new NarrativeHints
            {
                Tone = "resolute",
                Theme = "revelation",
                Context = "investigation_conclusion",
                Style = "climactic"
            },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,  // Solo situation - no NPC
            RouteFilter = null
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationId = $"{sceneId}_search",
            Transitions = new List<SituationTransition>
        {
            new SituationTransition
            {
                SourceSituationId = $"{sceneId}_search",
                DestinationSituationId = $"{sceneId}_analyze",
                Condition = TransitionCondition.Always
            },
            new SituationTransition
            {
                SourceSituationId = $"{sceneId}_analyze",
                DestinationSituationId = $"{sceneId}_conclude",
                Condition = TransitionCondition.Always
            }
        }
        };

        List<SituationTemplate> situations = new List<SituationTemplate>
    {
        searchSituation,
        analyzeSituation,
        concludeSituation
    };

        // CRITICAL: Enrich final situation to spawn next A-scene (infinite progression)
        // Generic logic - works for ANY sequence (A1→A2, A2→A3, A3→A4, A10→A11, etc.)
        // If next template exists (authored) → uses it
        // If next template doesn't exist → RewardApplicationService generates procedurally
        EnrichFinalSituationWithNextASceneSpawn(situations, context);

        return new SceneArchetypeDefinition
        {
            SituationTemplates = situations,
            SpawnRules = spawnRules
        };
    }

    /// <summary>
    /// GATHER_TESTIMONY archetype
    ///
    /// FICTIONAL CONTEXT: Player gathers testimony from witness/informant about Order activities
    /// STORY PURPOSE: Social information gathering advancing investigation
    ///
    /// Situation Count: 2
    /// Pattern: Linear (approach → interview)
    /// </summary>
    private static SceneArchetypeDefinition GenerateGatherTestimony(int tier, GenerationContext context)
    {
        string sceneId = "gather_testimony";

        // SITUATION 1: APPROACH WITNESS
        SituationArchetype approachArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.SocialManeuvering);
        List<ChoiceTemplate> approachChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            approachArchetype,
            $"{sceneId}_approach",
            context);

        SituationTemplate approachSituation = new SituationTemplate
        {
            Id = $"{sceneId}_approach",
            Type = SituationType.Normal,
            ChoiceTemplates = approachChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "careful",
                Theme = "approach",
                Context = "gaining_confidence",
                Style = "subtle"
            },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        // SITUATION 2: INTERVIEW
        SituationArchetype interviewArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Negotiation);
        List<ChoiceTemplate> interviewChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            interviewArchetype,
            $"{sceneId}_interview",
            context);

        SituationTemplate interviewSituation = new SituationTemplate
        {
            Id = $"{sceneId}_interview",
            Type = SituationType.Normal,
            ChoiceTemplates = interviewChoices,
            Priority = 90,
            NarrativeHints = new NarrativeHints
            {
                Tone = "probing",
                Theme = "interrogation",
                Context = "gathering_testimony",
                Style = "investigative"
            },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationId = $"{sceneId}_approach",
            Transitions = new List<SituationTransition>
        {
            new SituationTransition
            {
                SourceSituationId = $"{sceneId}_approach",
                DestinationSituationId = $"{sceneId}_interview",
                Condition = TransitionCondition.Always
            }
        }
        };

        List<SituationTemplate> situations = new List<SituationTemplate>
    {
        approachSituation,
        interviewSituation
    };

        // CRITICAL: Enrich final situation to spawn next A-scene (infinite progression)
        EnrichFinalSituationWithNextASceneSpawn(situations, context);

        return new SceneArchetypeDefinition
        {
            SituationTemplates = situations,
            SpawnRules = spawnRules
        };
    }

    /// <summary>
    /// CONFRONT_ANTAGONIST archetype
    ///
    /// FICTIONAL CONTEXT: Player confronts antagonist with evidence/accusation
    /// STORY PURPOSE: Dramatic confrontation advancing conflict
    ///
    /// Situation Count: 2
    /// Pattern: Linear (accusation → resolution)
    /// </summary>
    private static SceneArchetypeDefinition GenerateConfrontAntagonist(int tier, GenerationContext context)
    {
        string sceneId = "confront_antagonist";

        // SITUATION 1: ACCUSATION
        SituationArchetype accuseArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Confrontation);
        List<ChoiceTemplate> accuseChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            accuseArchetype,
            $"{sceneId}_accuse",
            context);

        SituationTemplate accuseSituation = new SituationTemplate
        {
            Id = $"{sceneId}_accuse",
            Type = SituationType.Normal,
            ChoiceTemplates = accuseChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "confrontational",
                Theme = "accusation",
                Context = "dramatic_confrontation",
                Style = "intense"
            },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        // SITUATION 2: RESOLUTION
        SituationArchetype resolveArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Crisis);
        List<ChoiceTemplate> resolveChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            resolveArchetype,
            $"{sceneId}_resolve",
            context);

        SituationTemplate resolveSituation = new SituationTemplate
        {
            Id = $"{sceneId}_resolve",
            Type = SituationType.Normal,
            ChoiceTemplates = resolveChoices,
            Priority = 90,
            NarrativeHints = new NarrativeHints
            {
                Tone = "decisive",
                Theme = "resolution",
                Context = "confrontation_outcome",
                Style = "climactic"
            },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationId = $"{sceneId}_accuse",
            Transitions = new List<SituationTransition>
        {
            new SituationTransition
            {
                SourceSituationId = $"{sceneId}_accuse",
                DestinationSituationId = $"{sceneId}_resolve",
                Condition = TransitionCondition.Always
            }
        }
        };

        List<SituationTemplate> situations = new List<SituationTemplate>
    {
        accuseSituation,
        resolveSituation
    };

        // CRITICAL: Enrich final situation to spawn next A-scene (infinite progression)
        EnrichFinalSituationWithNextASceneSpawn(situations, context);

        return new SceneArchetypeDefinition
        {
            SituationTemplates = situations,
            SpawnRules = spawnRules
        };
    }

    /// <summary>
    /// MEET_ORDER_MEMBER archetype
    ///
    /// FICTIONAL CONTEXT: Player meets member of scattered Order, each knows one piece of mystery
    /// STORY PURPOSE: Incremental revelation, deepening pursuit
    ///
    /// Situation Count: 3
    /// Pattern: Linear (contact → negotiate_info → revelation)
    /// </summary>
    private static SceneArchetypeDefinition GenerateMeetOrderMember(int tier, GenerationContext context)
    {
        string sceneId = "meet_order_member";

        // SITUATION 1: INITIAL CONTACT
        SituationArchetype contactArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.SocialManeuvering);
        List<ChoiceTemplate> contactChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            contactArchetype,
            $"{sceneId}_contact",
            context);

        SituationTemplate contactSituation = new SituationTemplate
        {
            Id = $"{sceneId}_contact",
            Type = SituationType.Normal,
            ChoiceTemplates = contactChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "cautious",
                Theme = "first_contact",
                Context = "meeting_order_member",
                Style = "mysterious"
            },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        // SITUATION 2: NEGOTIATE INFORMATION
        SituationArchetype negotiateArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Negotiation);
        List<ChoiceTemplate> negotiateChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            negotiateArchetype,
            $"{sceneId}_negotiate",
            context);

        SituationTemplate negotiateSituation = new SituationTemplate
        {
            Id = $"{sceneId}_negotiate",
            Type = SituationType.Normal,
            ChoiceTemplates = negotiateChoices,
            Priority = 90,
            NarrativeHints = new NarrativeHints
            {
                Tone = "tense",
                Theme = "information_exchange",
                Context = "negotiating_knowledge",
                Style = "strategic"
            },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        // SITUATION 3: REVELATION
        SituationArchetype revelationArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Investigation);
        List<ChoiceTemplate> revelationChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            revelationArchetype,
            $"{sceneId}_revelation",
            context);

        SituationTemplate revelationSituation = new SituationTemplate
        {
            Id = $"{sceneId}_revelation",
            Type = SituationType.Normal,
            ChoiceTemplates = revelationChoices,
            Priority = 80,
            NarrativeHints = new NarrativeHints
            {
                Tone = "revelatory",
                Theme = "knowledge_gained",
                Context = "order_secret_revealed",
                Style = "impactful"
            },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationId = $"{sceneId}_contact",
            Transitions = new List<SituationTransition>
        {
            new SituationTransition
            {
                SourceSituationId = $"{sceneId}_contact",
                DestinationSituationId = $"{sceneId}_negotiate",
                Condition = TransitionCondition.Always
            },
            new SituationTransition
            {
                SourceSituationId = $"{sceneId}_negotiate",
                DestinationSituationId = $"{sceneId}_revelation",
                Condition = TransitionCondition.Always
            }
        }
        };

        List<SituationTemplate> situations = new List<SituationTemplate>
    {
        contactSituation,
        negotiateSituation,
        revelationSituation
    };

        // CRITICAL: Enrich final situation to spawn next A-scene (infinite progression)
        EnrichFinalSituationWithNextASceneSpawn(situations, context);

        return new SceneArchetypeDefinition
        {
            SituationTemplates = situations,
            SpawnRules = spawnRules
        };
    }

    /// <summary>
    /// DISCOVER_ARTIFACT archetype
    ///
    /// FICTIONAL CONTEXT: Player discovers Order artifact (relic, document, tool)
    /// STORY PURPOSE: Physical progression token, lore advancement
    ///
    /// Situation Count: 2
    /// Pattern: Linear (locate → acquire)
    /// </summary>
    private static SceneArchetypeDefinition GenerateDiscoverArtifact(int tier, GenerationContext context)
    {
        string sceneId = "discover_artifact";

        // SITUATION 1: LOCATE ARTIFACT
        SituationArchetype locateArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Investigation);
        List<ChoiceTemplate> locateChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            locateArchetype,
            $"{sceneId}_locate",
            context);

        SituationTemplate locateSituation = new SituationTemplate
        {
            Id = $"{sceneId}_locate",
            Type = SituationType.Normal,
            ChoiceTemplates = locateChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "anticipatory",
                Theme = "discovery",
                Context = "locating_artifact",
                Style = "atmospheric"
            },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,  // Solo situation - no NPC
            RouteFilter = null
        };

        // SITUATION 2: ACQUIRE ARTIFACT
        SituationArchetype acquireArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Crisis);
        List<ChoiceTemplate> acquireChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            acquireArchetype,
            $"{sceneId}_acquire",
            context);

        // Enrich with artifact reward
        List<ChoiceTemplate> enrichedAcquireChoices = new List<ChoiceTemplate>();
        foreach (ChoiceTemplate choice in acquireChoices)
        {
            // NOTE: Dependent item granting (order_artifact) cannot be handled at catalog time
            // because the items don't exist until scene instantiation.

            enrichedAcquireChoices.Add(choice); // Pass through unchanged
        }

        SituationTemplate acquireSituation = new SituationTemplate
        {
            Id = $"{sceneId}_acquire",
            Type = SituationType.Normal,
            ChoiceTemplates = enrichedAcquireChoices,
            Priority = 90,
            NarrativeHints = new NarrativeHints
            {
                Tone = "triumphant",
                Theme = "acquisition",
                Context = "claiming_artifact",
                Style = "epic"
            },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,  // Solo situation - no NPC
            RouteFilter = null
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationId = $"{sceneId}_locate",
            Transitions = new List<SituationTransition>
        {
            new SituationTransition
            {
                SourceSituationId = $"{sceneId}_locate",
                DestinationSituationId = $"{sceneId}_acquire",
                Condition = TransitionCondition.Always
            }
        }
        };

        List<SituationTemplate> situations = new List<SituationTemplate>
    {
        locateSituation,
        acquireSituation
    };

        // CRITICAL: Enrich final situation to spawn next A-scene (infinite progression)
        EnrichFinalSituationWithNextASceneSpawn(situations, context);

        return new SceneArchetypeDefinition
        {
            SituationTemplates = situations,
            SpawnRules = spawnRules
        };
    }

    /// <summary>
    /// UNCOVER_CONSPIRACY archetype
    ///
    /// FICTIONAL CONTEXT: Player uncovers conspiracy related to Order's fall
    /// STORY PURPOSE: Major plot revelation, raising stakes
    ///
    /// Situation Count: 4
    /// Pattern: Linear (suspect → gather_proof → expose → consequence)
    /// </summary>
    private static SceneArchetypeDefinition GenerateUncoverConspiracy(int tier, GenerationContext context)
    {
        string sceneId = "uncover_conspiracy";

        // SITUATION 1: SUSPICION
        SituationArchetype suspectArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Investigation);
        List<ChoiceTemplate> suspectChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            suspectArchetype,
            $"{sceneId}_suspect",
            context);

        SituationTemplate suspectSituation = new SituationTemplate
        {
            Id = $"{sceneId}_suspect",
            Type = SituationType.Normal,
            ChoiceTemplates = suspectChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "suspicious",
                Theme = "conspiracy",
                Context = "initial_suspicion",
                Style = "tense"
            },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,  // Solo situation - no NPC
            RouteFilter = null
        };

        // SITUATION 2: GATHER PROOF
        SituationArchetype proofArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Investigation);
        List<ChoiceTemplate> proofChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            proofArchetype,
            $"{sceneId}_proof",
            context);

        SituationTemplate proofSituation = new SituationTemplate
        {
            Id = $"{sceneId}_proof",
            Type = SituationType.Normal,
            ChoiceTemplates = proofChoices,
            Priority = 90,
            NarrativeHints = new NarrativeHints
            {
                Tone = "determined",
                Theme = "evidence_gathering",
                Context = "proving_conspiracy",
                Style = "methodical"
            },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,  // Solo situation - no NPC
            RouteFilter = null
        };

        // SITUATION 3: EXPOSE
        SituationArchetype exposeArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Confrontation);
        List<ChoiceTemplate> exposeChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            exposeArchetype,
            $"{sceneId}_expose",
            context);

        SituationTemplate exposeSituation = new SituationTemplate
        {
            Id = $"{sceneId}_expose",
            Type = SituationType.Normal,
            ChoiceTemplates = exposeChoices,
            Priority = 80,
            NarrativeHints = new NarrativeHints
            {
                Tone = "dramatic",
                Theme = "revelation",
                Context = "exposing_conspiracy",
                Style = "climactic"
            },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        // SITUATION 4: CONSEQUENCE
        SituationArchetype consequenceArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Crisis);
        List<ChoiceTemplate> consequenceChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            consequenceArchetype,
            $"{sceneId}_consequence",
            context);

        SituationTemplate consequenceSituation = new SituationTemplate
        {
            Id = $"{sceneId}_consequence",
            Type = SituationType.Normal,
            ChoiceTemplates = consequenceChoices,
            Priority = 70,
            NarrativeHints = new NarrativeHints
            {
                Tone = "grave",
                Theme = "consequence",
                Context = "conspiracy_aftermath",
                Style = "sobering"
            },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationId = $"{sceneId}_suspect",
            Transitions = new List<SituationTransition>
        {
            new SituationTransition
            {
                SourceSituationId = $"{sceneId}_suspect",
                DestinationSituationId = $"{sceneId}_proof",
                Condition = TransitionCondition.Always
            },
            new SituationTransition
            {
                SourceSituationId = $"{sceneId}_proof",
                DestinationSituationId = $"{sceneId}_expose",
                Condition = TransitionCondition.Always
            },
            new SituationTransition
            {
                SourceSituationId = $"{sceneId}_expose",
                DestinationSituationId = $"{sceneId}_consequence",
                Condition = TransitionCondition.Always
            }
        }
        };

        List<SituationTemplate> situations = new List<SituationTemplate>
    {
        suspectSituation,
        proofSituation,
        exposeSituation,
        consequenceSituation
    };

        // CRITICAL: Enrich final situation to spawn next A-scene (infinite progression)
        EnrichFinalSituationWithNextASceneSpawn(situations, context);

        return new SceneArchetypeDefinition
        {
            SituationTemplates = situations,
            SpawnRules = spawnRules
        };
    }

    /// <summary>
    /// URGENT_DECISION archetype
    ///
    /// FICTIONAL CONTEXT: Player must make urgent time-pressured decision
    /// STORY PURPOSE: Crisis moment, testing player values
    ///
    /// Situation Count: 2
    /// Pattern: Linear (crisis → decision)
    /// </summary>
    private static SceneArchetypeDefinition GenerateUrgentDecision(int tier, GenerationContext context)
    {
        string sceneId = "urgent_decision";

        // SITUATION 1: CRISIS EMERGES
        SituationArchetype crisisArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Crisis);
        List<ChoiceTemplate> crisisChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            crisisArchetype,
            $"{sceneId}_crisis",
            context);

        SituationTemplate crisisSituation = new SituationTemplate
        {
            Id = $"{sceneId}_crisis",
            Type = SituationType.Crisis,
            ChoiceTemplates = crisisChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "urgent",
                Theme = "crisis",
                Context = "emergency",
                Style = "intense"
            },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        // SITUATION 2: DECISION
        SituationArchetype decisionArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Crisis);
        List<ChoiceTemplate> decisionChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            decisionArchetype,
            $"{sceneId}_decision",
            context);

        SituationTemplate decisionSituation = new SituationTemplate
        {
            Id = $"{sceneId}_decision",
            Type = SituationType.Crisis,
            ChoiceTemplates = decisionChoices,
            Priority = 90,
            NarrativeHints = new NarrativeHints
            {
                Tone = "desperate",
                Theme = "decision",
                Context = "urgent_choice",
                Style = "high_stakes"
            },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationId = $"{sceneId}_crisis",
            Transitions = new List<SituationTransition>
        {
            new SituationTransition
            {
                SourceSituationId = $"{sceneId}_crisis",
                DestinationSituationId = $"{sceneId}_decision",
                Condition = TransitionCondition.Always
            }
        }
        };

        List<SituationTemplate> situations = new List<SituationTemplate>
    {
        crisisSituation,
        decisionSituation
    };

        // CRITICAL: Enrich final situation to spawn next A-scene (infinite progression)
        EnrichFinalSituationWithNextASceneSpawn(situations, context);

        return new SceneArchetypeDefinition
        {
            SituationTemplates = situations,
            SpawnRules = spawnRules
        };
    }

    /// <summary>
    /// MORAL_CROSSROADS archetype
    ///
    /// FICTIONAL CONTEXT: Player faces moral dilemma with lasting consequences
    /// STORY PURPOSE: Player agency, values expression
    ///
    /// Situation Count: 3
    /// Pattern: Linear (dilemma → choice → consequence)
    /// </summary>
    private static SceneArchetypeDefinition GenerateMoralCrossroads(int tier, GenerationContext context)
    {
        string sceneId = "moral_crossroads";

        // SITUATION 1: DILEMMA PRESENTED
        SituationArchetype dilemmaArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.SocialManeuvering);
        List<ChoiceTemplate> dilemmaChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            dilemmaArchetype,
            $"{sceneId}_dilemma",
            context);

        SituationTemplate dilemmaSituation = new SituationTemplate
        {
            Id = $"{sceneId}_dilemma",
            Type = SituationType.Normal,
            ChoiceTemplates = dilemmaChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "conflicted",
                Theme = "moral_dilemma",
                Context = "ethical_choice",
                Style = "thoughtful"
            },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        // SITUATION 2: MORAL CHOICE
        SituationArchetype choiceArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Crisis);
        List<ChoiceTemplate> choiceChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            choiceArchetype,
            $"{sceneId}_choice",
            context);

        SituationTemplate choiceSituation = new SituationTemplate
        {
            Id = $"{sceneId}_choice",
            Type = SituationType.Normal,
            ChoiceTemplates = choiceChoices,
            Priority = 90,
            NarrativeHints = new NarrativeHints
            {
                Tone = "weighty",
                Theme = "moral_stance",
                Context = "defining_moment",
                Style = "impactful"
            },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        // SITUATION 3: CONSEQUENCE
        SituationArchetype consequenceArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.SocialManeuvering);
        List<ChoiceTemplate> consequenceChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            consequenceArchetype,
            $"{sceneId}_consequence",
            context);

        SituationTemplate consequenceSituation = new SituationTemplate
        {
            Id = $"{sceneId}_consequence",
            Type = SituationType.Normal,
            ChoiceTemplates = consequenceChoices,
            Priority = 80,
            NarrativeHints = new NarrativeHints
            {
                Tone = "reflective",
                Theme = "consequence",
                Context = "moral_aftermath",
                Style = "somber"
            },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationId = $"{sceneId}_dilemma",
            Transitions = new List<SituationTransition>
        {
            new SituationTransition
            {
                SourceSituationId = $"{sceneId}_dilemma",
                DestinationSituationId = $"{sceneId}_choice",
                Condition = TransitionCondition.Always
            },
            new SituationTransition
            {
                SourceSituationId = $"{sceneId}_choice",
                DestinationSituationId = $"{sceneId}_consequence",
                Condition = TransitionCondition.Always
            }
        }
        };

        List<SituationTemplate> situations = new List<SituationTemplate>
    {
        dilemmaSituation,
        choiceSituation,
        consequenceSituation
    };

        // CRITICAL: Enrich final situation to spawn next A-scene (infinite progression)
        EnrichFinalSituationWithNextASceneSpawn(situations, context);

        return new SceneArchetypeDefinition
        {
            SituationTemplates = situations,
            SpawnRules = spawnRules
        };
    }

    // ===================================================================
    // INFINITE A-STORY PROGRESSION: Final Situation Enrichment
    // ===================================================================

    /// <summary>
    /// Enrich final situation to spawn next A-scene (CRITICAL for infinite progression)
    ///
    /// GUARANTEED PROGRESSION PATTERN:
    /// - Final situation ALL choices spawn next A-scene
    /// - Ensures forward progress regardless of player choices
    /// - Infinite A-story loop: A11 → A12 → A13 → ... → infinity
    ///
    /// Called after generating situation templates, before returning definition
    /// Modifies final situation's choice templates in-place
    ///
    /// Generic logic - works for ANY sequence number.
    /// If next template exists (authored) → uses it.
    /// If next template doesn't exist → RewardApplicationService generates procedurally.
    /// NO HARDCODED SPECIAL CASES.
    /// </summary>
    private static void EnrichFinalSituationWithNextASceneSpawn(
        List<SituationTemplate> situations,
        GenerationContext context)
    {
        if (!context.AStorySequence.HasValue)
        {
            return; // Not an A-story scene, no enrichment needed
        }

        if (situations.Count == 0)
        {
            return; // No situations to enrich
        }

        // Final situation = last situation in list
        SituationTemplate finalSituation = situations[situations.Count - 1];

        // Next A-scene ID (generic - no special cases)
        string nextASceneId = $"a_story_{context.AStorySequence.Value + 1}";

        // Enrich ALL choices with SceneSpawnReward
        List<ChoiceTemplate> enrichedChoices = new List<ChoiceTemplate>();
        foreach (ChoiceTemplate choice in finalSituation.ChoiceTemplates)
        {
            ChoiceReward reward = choice.RewardTemplate ?? new ChoiceReward();

            // Add next A-scene spawn reward
            // Uses template's PlacementFilter for categorical resolution (no override needed)
            reward.ScenesToSpawn = new List<SceneSpawnReward>
        {
            new SceneSpawnReward
            {
                SceneTemplateId = nextASceneId
                // PlacementFilterOverride = null (uses template's filter)
            }
        };

            enrichedChoices.Add(new ChoiceTemplate
            {
                Id = choice.Id,
                PathType = choice.PathType,
                ActionTextTemplate = choice.ActionTextTemplate,
                RequirementFormula = choice.RequirementFormula,
                CostTemplate = choice.CostTemplate,
                RewardTemplate = reward,
                ActionType = choice.ActionType,
                ChallengeType = choice.ChallengeType
            });
        }

        // Replace final situation's choices with enriched versions
        finalSituation.ChoiceTemplates.Clear();
        finalSituation.ChoiceTemplates.AddRange(enrichedChoices);
    }
}
