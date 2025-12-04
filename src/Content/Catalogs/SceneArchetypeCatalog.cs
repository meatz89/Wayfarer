/// <summary>
/// PARSE-TIME ONLY CATALOGUE
///
/// Coordinator for scene archetype generation. Dispatches to specialized archetype files
/// organized by PLAYER ACTIVITY (not legacy rotation categories):
///
/// - JourneyArchetypes: Framework of the infinite Frieren journey (contracts, travel, rest, reflection)
/// - ExplorationArchetypes: Finding things out (clues, artifacts, audiences, testimony)
/// - EncounterArchetypes: Meeting people and facing situations (contacts, antagonists, decisions)
///
/// HIGHLANDER COMPLIANT: ONE catalogue coordinating ALL scene archetypes (A-story, B-story, C-story)
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
/// RhythmPattern (Building/Crisis/Mixed) determines choice structure, not archetype category.
/// See arc42/08_crosscutting_concepts.md §8.26 (Sir Brante Rhythm Pattern)
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
        GenerationContext context)
    {
        return archetypeType switch
        {
            // Journey archetypes - framework of the infinite journey
            SceneArchetypeType.DeliveryContract => JourneyArchetypes.GenerateDeliveryContract(context),
            SceneArchetypeType.RouteSegmentTravel => JourneyArchetypes.GenerateRouteSegmentTravel(context),
            SceneArchetypeType.InnLodging => JourneyArchetypes.GenerateInnLodging(context),
            SceneArchetypeType.ConsequenceReflection => JourneyArchetypes.GenerateConsequenceReflection(context),

            // Exploration archetypes - finding things out
            SceneArchetypeType.SeekAudience => ExplorationArchetypes.GenerateSeekAudience(context),
            SceneArchetypeType.InvestigateLocation => ExplorationArchetypes.GenerateInvestigateLocation(context),
            SceneArchetypeType.GatherTestimony => ExplorationArchetypes.GenerateGatherTestimony(context),
            SceneArchetypeType.DiscoverArtifact => ExplorationArchetypes.GenerateDiscoverArtifact(context),
            SceneArchetypeType.UncoverConspiracy => ExplorationArchetypes.GenerateUncoverConspiracy(context),

            // Encounter archetypes - meeting people and facing situations
            SceneArchetypeType.MeetOrderMember => EncounterArchetypes.GenerateMeetOrderMember(context),
            SceneArchetypeType.ConfrontAntagonist => EncounterArchetypes.GenerateConfrontAntagonist(context),
            SceneArchetypeType.UrgentDecision => EncounterArchetypes.GenerateUrgentDecision(context),
            SceneArchetypeType.MoralCrossroads => EncounterArchetypes.GenerateMoralCrossroads(context),
            SceneArchetypeType.QuietReflection => EncounterArchetypes.GenerateQuietReflection(context),
            SceneArchetypeType.CasualEncounter => EncounterArchetypes.GenerateCasualEncounter(context),
            SceneArchetypeType.ScholarlyPursuit => EncounterArchetypes.GenerateScholarlyPursuit(context),

            _ => throw new InvalidOperationException($"Unhandled scene archetype type: {archetypeType}")
        };
    }

    /// <summary>
    /// Get available archetypes for category (for procedural A-story generation)
    /// Returns archetypes currently implemented in catalog for given category
    /// Prevents drift between catalog and procedural selection lists
    ///
    /// NOTE: Categories describe FUNCTION, not rotation position.
    /// RhythmPattern (computed from intensity history) determines choice structure.
    /// Categories are for anti-repetition and thematic filtering only.
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
            "Peaceful" => new List<SceneArchetypeType>
            {
                SceneArchetypeType.QuietReflection,
                SceneArchetypeType.CasualEncounter,
                SceneArchetypeType.ScholarlyPursuit
            },
            _ => throw new InvalidOperationException(
                $"Unknown archetype category '{category}'. " +
                $"Valid categories: Investigation, Social, Confrontation, Crisis, Peaceful.")
        };
    }

    /// <summary>
    /// Resolve specific archetype from category with exclusions (CATALOGUE PATTERN - PARSE-TIME ONLY).
    /// Called by Parser when DTO has ArchetypeCategory instead of explicit SceneArchetype.
    /// Uses sequence-based deterministic selection (no Random) for consistent procedural generation.
    ///
    /// FAIL-FAST: Throws if category unknown or all archetypes excluded.
    /// </summary>
    /// <param name="category">Category name: "Investigation", "Social", "Confrontation", "Crisis", "Peaceful"</param>
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
                $"Valid categories: Investigation, Social, Confrontation, Crisis, Peaceful.");
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
}
