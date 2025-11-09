using Wayfarer.GameState;
using Wayfarer.GameState.Enums;
using Wayfarer.Content.Generators;

namespace Wayfarer.Content.Catalogues;

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

            _ => throw new InvalidDataException($"Unknown scene archetype ID: '{archetypeId}'. Valid archetypes: inn_lodging, consequence_reflection")
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

        // SITUATION 1: NEGOTIATE LODGING
        SituationArchetype negotiateArchetype = SituationArchetypeCatalog.GetArchetype("service_negotiation");
        List<ChoiceTemplate> negotiateChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            negotiateArchetype,
            negotiateSitId,
            context);

        // Enrich negotiate choices with room unlock rewards (route by PathType)
        List<ChoiceTemplate> enrichedNegotiateChoices = new List<ChoiceTemplate>();
        foreach (ChoiceTemplate choice in negotiateChoices)
        {
            switch (choice.PathType)
            {
                case ChoicePathType.InstantSuccess:
                    // Stat/money paths: immediate success unlocks room
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
                    // Challenge path: unlock on success only
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
                    // Fallback: no rewards, player leaves without room
                    enrichedNegotiateChoices.Add(choice);
                    break;
            }
        }

        SituationTemplate negotiateSituation = new SituationTemplate
        {
            Id = negotiateSitId,
            Name = "Secure Lodging",
            Type = SituationType.Normal,
            NarrativeTemplate = null,  // AI generates from hints
            ChoiceTemplates = enrichedNegotiateChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "transactional",
                Theme = "negotiation",
                Context = "securing_lodging",
                Style = "direct"
            },
            RequiredLocationId = context.LocationId,
            RequiredNpcId = context.NpcId
        };

        // SITUATION 2: REST IN ROOM
        SituationArchetype restArchetype = SituationArchetypeCatalog.GetArchetype("service_execution_rest");
        List<ChoiceTemplate> restChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            restArchetype,
            restSitId,
            context);

        SituationTemplate restSituation = new SituationTemplate
        {
            Id = restSitId,
            Name = "Rest",
            Type = SituationType.Normal,
            NarrativeTemplate = null,
            ChoiceTemplates = restChoices,
            Priority = 90,
            NarrativeHints = new NarrativeHints
            {
                Tone = "restorative",
                Theme = "rest",
                Context = "recovery",
                Style = "peaceful"
            },
            RequiredLocationId = "generated:private_room",
            RequiredNpcId = null  // No NPC in private room
        };

        // SITUATION 3: DEPART INN
        SituationArchetype departArchetype = SituationArchetypeCatalog.GetArchetype("service_departure");
        List<ChoiceTemplate> departChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            departArchetype,
            departSitId,
            context);

        // Enrich depart choices with cleanup rewards (return key, lock room)
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
                PathType = choice.PathType,
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

        SituationTemplate departSituation = new SituationTemplate
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
                Context = "morning_departure",
                Style = "forward-looking"
            },
            RequiredLocationId = "generated:private_room",
            RequiredNpcId = null
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
            RequiredLocationId = context.LocationId,  // Works for both NPC-placed and Location-placed scenes
            RequiredNpcId = null  // No NPC for solo reflection
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
}
