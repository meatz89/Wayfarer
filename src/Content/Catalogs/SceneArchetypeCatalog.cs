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
/// SCENE ARCHETYPES (13 total - All HIGHLANDER-compliant):
///
/// HIGHLANDER: All archetypes use SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext()
/// RhythmPattern (Building/Crisis/Mixed) determines choice structure, not archetype category.
/// See arc42/08_crosscutting_concepts.md §8.26 (Sir Brante Rhythm Pattern)
///
/// - InnLodging: 3-situation inn lodging flow (negotiate → rest → depart)
/// - ConsequenceReflection: Single-situation consequence acknowledgment
/// - DeliveryContract: Contract acceptance and delivery flow
/// - RouteSegmentTravel: 5-situation travel flow (3 obstacles → approach → arrival)
/// - SeekAudience: Player seeks audience with authority figure
/// - InvestigateLocation: Player investigates location for clues
/// - GatherTestimony: Player gathers testimony from witnesses
/// - ConfrontAntagonist: Player confronts antagonist
/// - MeetOrderMember: Player meets order member
/// - DiscoverArtifact: Player discovers artifact
/// - UncoverConspiracy: Player uncovers conspiracy
/// - UrgentDecision: Player faces urgent decision
/// - MoralCrossroads: Player faces moral dilemma
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
    /// Resolve specific archetype from category with exclusions (CATALOGUE PATTERN - PARSE-TIME ONLY).
    /// Called by Parser when DTO has ArchetypeCategory instead of explicit SceneArchetype.
    /// Uses sequence-based deterministic selection (no Random) for consistent procedural generation.
    ///
    /// FAIL-FAST: Throws if category unknown or all archetypes excluded.
    /// </summary>
    /// <param name="category">Category name: "Investigation", "Social", "Confrontation", "Crisis"</param>
    /// <param name="excludedArchetypes">List of archetype names to exclude (anti-repetition)</param>
    /// <param name="sequence">Sequence number for deterministic selection</param>
    /// <returns>Resolved SceneArchetypeType</returns>
    public static SceneArchetypeType ResolveFromCategory(
        string category,
        List<string> excludedArchetypes,
        int sequence)
    {
        List<SceneArchetypeType> candidates = GetArchetypesForCategory(category);

        if (!candidates.Any())
        {
            throw new InvalidOperationException(
                $"Cannot resolve archetype: Unknown category '{category}'. " +
                $"Valid categories: Investigation, Social, Confrontation, Crisis.");
        }

        List<SceneArchetypeType> excluded = new List<SceneArchetypeType>();
        if (excludedArchetypes != null)
        {
            foreach (string name in excludedArchetypes)
            {
                if (Enum.TryParse<SceneArchetypeType>(name, true, out SceneArchetypeType archetypeType))
                {
                    excluded.Add(archetypeType);
                }
            }
        }

        List<SceneArchetypeType> available = candidates
            .Where(a => !excluded.Contains(a))
            .ToList();

        if (!available.Any())
        {
            available = candidates;
        }

        int selectionIndex = sequence % available.Count;
        return available[selectionIndex];
    }

    // ===================================================================
    // SCENE ARCHETYPES - All archetypes use HIGHLANDER-compliant generation
    // RhythmPattern determines choice structure, not archetype category
    // See arc42/08_crosscutting_concepts.md §8.26 (Sir Brante Rhythm Pattern)
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
        // HIGHLANDER: ONE path for all scenes - RhythmPattern determines choice structure
        // Building rhythm = stat grants (A1 tutorial), Mixed/Crisis = standard negotiation
        // See arc42/08_crosscutting_concepts.md §8.26 (Sir Brante Rhythm Pattern)
        SituationArchetype negotiateArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.ServiceNegotiation);
        List<ChoiceTemplate> negotiateChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            negotiateArchetype,
            negotiateSitId,
            context);

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
        // HIGHLANDER: ONE path for all scenes - RhythmPattern determines rest behavior
        // Building rhythm = high restoration + stat grants (identity formation)
        // Crisis rhythm = low restoration, anxious night (damage mitigation)
        // Mixed rhythm = resource distribution choices (standard trade-offs)
        // See arc42/08_crosscutting_concepts.md §8.26 (Sir Brante Rhythm Pattern)
        SituationArchetype restArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.ServiceExecutionRest);
        List<ChoiceTemplate> restChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            restArchetype,
            restSitId,
            context);

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

        // SITUATION 3: MORNING DEPARTURE
        // HIGHLANDER: ONE path for all scenes - RhythmPattern determines departure behavior
        // Building rhythm = both paths positive with stat grants (identity formation)
        // Crisis rhythm = quick exit safe, lingering has penalty (damage mitigation)
        // Mixed rhythm = standard 2-choice departure (trade-offs)
        // See arc42/08_crosscutting_concepts.md §8.26 (Sir Brante Rhythm Pattern)
        SituationArchetype departArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.ServiceDeparture);
        List<ChoiceTemplate> departChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            departArchetype,
            departSitId,
            context);

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
                    Consequence = new Consequence(),  // Advances to negotiation
                    ActionType = ChoiceActionType.Instant
                },
                new ChoiceTemplate
                {
                    Id = $"{offerSitId}_decline",
                    PathType = ChoicePathType.Fallback,
                    ActionTextTemplate = "Not right now",
                    RequirementFormula = new CompoundRequirement(),
                    Consequence = new Consequence(),  // Stays at offer (repeatable)
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

        // HIGHLANDER: ONE path for all scenes - RhythmPattern + Tier handle difficulty scaling
        // Stat requirements scale via Tier/NPCDemeanor/PowerDynamic (see SituationArchetypeCatalog)
        // Scene spawning handled by EnrichMainStoryFinalChoices in parser
        // Fallback enrichment for post-commitment context
        List<ChoiceTemplate> enrichedNegotiateChoices = new List<ChoiceTemplate>();
        foreach (ChoiceTemplate choice in negotiateChoices)
        {
            string enrichedActionText = choice.PathType == ChoicePathType.Fallback
                ? "Back out of the deal"
                : choice.ActionTextTemplate;

            ChoiceTemplate enrichedChoice = new ChoiceTemplate
            {
                Id = choice.Id,
                PathType = choice.PathType,
                ActionTextTemplate = enrichedActionText,
                RequirementFormula = choice.RequirementFormula,
                Consequence = choice.Consequence,
                OnSuccessConsequence = choice.OnSuccessConsequence,
                OnFailureConsequence = choice.OnFailureConsequence,
                ActionType = choice.ActionType,
                ChallengeId = choice.ChallengeId,
                ChallengeType = choice.ChallengeType,
                DeckId = choice.DeckId,
                NavigationPayload = choice.NavigationPayload
            };

            enrichedNegotiateChoices.Add(enrichedChoice);
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
    /// ROUTE_SEGMENT_TRAVEL archetype - HIGHLANDER COMPLIANT
    ///
    /// FICTIONAL CONTEXT: Player traveling multi-segment route, encountering obstacles
    /// REUSABLE: Works for any route travel scene throughout game
    ///
    /// Situation Count: 5
    /// Pattern: Linear (3 route obstacles → approach → arrival)
    ///
    /// HIGHLANDER: ALL situations use SituationArchetypeCatalog with RhythmPattern.
    /// NO inline choices, NO isCrisis branching.
    ///
    /// Situations 1-3: Route obstacles using domain-appropriate archetypes
    ///   - Obstacle 1 (Physical): Confrontation archetype (Authority-based)
    ///   - Obstacle 2 (Mental): Investigation archetype (Insight-based)
    ///   - Obstacle 3 (Social): SocialManeuvering archetype (Rapport-based)
    ///
    /// Situation 4: Final Approach using RestPreparation archetype
    /// Situation 5: Arrival using ServiceTransaction archetype
    ///
    /// RhythmPattern determines choice structure for ALL situations uniformly.
    /// See arc42/08_crosscutting_concepts.md §8.26 (Sir Brante Rhythm Pattern)
    /// </summary>
    private static SceneArchetypeDefinition GenerateRouteSegmentTravel(int tier, GenerationContext context)
    {
        string sceneId = "route_segment_travel";
        string obstacle1SitId = $"{sceneId}_obstacle1";
        string obstacle2SitId = $"{sceneId}_obstacle2";
        string obstacle3SitId = $"{sceneId}_obstacle3";
        string approachSitId = $"{sceneId}_approach";
        string arrivalSitId = $"{sceneId}_arrival";

        // SITUATION 1: PHYSICAL OBSTACLE (Segment 0)
        // HIGHLANDER: Uses Confrontation archetype - Authority-based, Physical challenge
        // RhythmPattern determines: Building=stat grants, Crisis=penalty avoidance, Mixed=trade-offs
        SituationArchetype obstacle1Archetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Confrontation);
        List<ChoiceTemplate> obstacle1Choices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            obstacle1Archetype,
            obstacle1SitId,
            context);

        SituationTemplate obstacle1Situation = new SituationTemplate
        {
            Id = obstacle1SitId,
            Name = "Forest Obstacle",
            Type = SituationType.Normal,
            SystemType = obstacle1Archetype.ChallengeType,
            NarrativeTemplate = null,
            ChoiceTemplates = obstacle1Choices,
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
                SegmentIndex = 0
            },
            LocationFilter = new PlacementFilter
            {
                PlacementType = PlacementType.Location,
                Proximity = PlacementProximity.SameLocation
            },
            NpcFilter = null
        };

        // SITUATION 2: MENTAL OBSTACLE (Segment 1)
        // HIGHLANDER: Uses Investigation archetype - Insight-based, Mental challenge
        SituationArchetype obstacle2Archetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Investigation);
        List<ChoiceTemplate> obstacle2Choices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            obstacle2Archetype,
            obstacle2SitId,
            context);

        SituationTemplate obstacle2Situation = new SituationTemplate
        {
            Id = obstacle2SitId,
            Name = "River Crossing",
            Type = SituationType.Normal,
            SystemType = obstacle2Archetype.ChallengeType,
            NarrativeTemplate = null,
            ChoiceTemplates = obstacle2Choices,
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
                SegmentIndex = 1
            },
            LocationFilter = new PlacementFilter
            {
                PlacementType = PlacementType.Location,
                Proximity = PlacementProximity.SameLocation
            },
            NpcFilter = null
        };

        // SITUATION 3: SOCIAL OBSTACLE (Segment 2)
        // HIGHLANDER: Uses SocialManeuvering archetype - Rapport-based, Social challenge
        SituationArchetype obstacle3Archetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.SocialManeuvering);
        List<ChoiceTemplate> obstacle3Choices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            obstacle3Archetype,
            obstacle3SitId,
            context);

        SituationTemplate obstacle3Situation = new SituationTemplate
        {
            Id = obstacle3SitId,
            Name = "Checkpoint Guard",
            Type = SituationType.Normal,
            SystemType = obstacle3Archetype.ChallengeType,
            NarrativeTemplate = null,
            ChoiceTemplates = obstacle3Choices,
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
                SegmentIndex = 2
            },
            LocationFilter = new PlacementFilter
            {
                PlacementType = PlacementType.Location,
                Proximity = PlacementProximity.SameLocation
            },
            NpcFilter = null
        };

        // SITUATION 4: FINAL APPROACH (Segment 3)
        // HIGHLANDER: Uses RestPreparation archetype - preparation before arrival
        SituationArchetype approachArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.RestPreparation);
        List<ChoiceTemplate> approachChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            approachArchetype,
            approachSitId,
            context);

        SituationTemplate approachSituation = new SituationTemplate
        {
            Id = approachSitId,
            Name = "Final Approach",
            Type = SituationType.Normal,
            SystemType = approachArchetype.ChallengeType,
            NarrativeTemplate = null,
            ChoiceTemplates = approachChoices,
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
                SegmentIndex = 3
            },
            LocationFilter = new PlacementFilter
            {
                PlacementType = PlacementType.Location,
                Proximity = PlacementProximity.SameLocation
            },
            NpcFilter = null
        };

        // SITUATION 5: ARRIVAL AT DESTINATION
        // HIGHLANDER: Uses ServiceTransaction archetype - completion of journey
        SituationArchetype arrivalArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.ServiceTransaction);
        List<ChoiceTemplate> arrivalChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            arrivalArchetype,
            arrivalSitId,
            context);

        SituationTemplate arrivalSituation = new SituationTemplate
        {
            Id = arrivalSitId,
            Name = "Delivery Complete",
            Type = SituationType.Normal,
            SystemType = arrivalArchetype.ChallengeType,
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
            RouteFilter = null
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
            Consequence consequence = choice.Consequence ?? new Consequence();

            // NEW ARCHITECTURE: Dual-model accessibility - situation presence at dependent location grants access
            // No need for reward-based unlock - when situation advances to meeting_chamber, access is automatic

            enrichedNegotiateChoices.Add(new ChoiceTemplate
            {
                Id = choice.Id,
                PathType = choice.PathType,
                ActionTextTemplate = choice.ActionTextTemplate,
                RequirementFormula = choice.RequirementFormula,
                Consequence = consequence,
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

        return new SceneArchetypeDefinition
        {
            SituationTemplates = situations,
            SpawnRules = spawnRules
        };
    }
}
