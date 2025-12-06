/// <summary>
/// Utility methods for archetype category and intensity mapping.
/// Used for intensity tracking when A-story scenes complete.
///
/// NOTE: SelectCategory has been REMOVED (HIGHLANDER violation).
/// Scene selection now uses ProceduralAStoryService.SelectArchetypeCategory
/// which takes SceneSelectionInputs (context injection pattern).
///
/// This class now ONLY provides static mapping utilities:
/// - MapArchetypeToCategory: SceneArchetypeType → Category string
/// - MapArchetypeToIntensity: SceneArchetypeType → ArchetypeIntensity
///
/// See arc42/08_crosscutting_concepts.md §8.28 for Context Injection.
/// </summary>
public static class ArchetypeCategorySelector
{
    /// <summary>
    /// Map SceneArchetypeType to its category string.
    /// Used for anti-repetition checking and intensity recording.
    /// PUBLIC: Called by SituationCompletionHandler for intensity tracking.
    ///
    /// FAIL-FAST: Throws for unknown archetypes to catch authoring errors.
    /// Encounter patterns mapped to "Encounter" category (should not be tracked for A-story rhythm).
    /// </summary>
    public static string MapArchetypeToCategory(SceneArchetypeType archetype)
    {
        return archetype switch
        {
            // Investigation category (Standard intensity)
            SceneArchetypeType.InvestigateLocation => "Investigation",
            SceneArchetypeType.GatherTestimony => "Investigation",
            SceneArchetypeType.SeekAudience => "Investigation",
            SceneArchetypeType.DiscoverArtifact => "Investigation",
            SceneArchetypeType.UncoverConspiracy => "Investigation",

            // Social category (Standard intensity)
            SceneArchetypeType.MeetOrderMember => "Social",

            // Confrontation category (Demanding intensity)
            SceneArchetypeType.ConfrontAntagonist => "Confrontation",

            // Crisis category (Demanding intensity)
            SceneArchetypeType.UrgentDecision => "Crisis",
            SceneArchetypeType.MoralCrossroads => "Crisis",

            // Peaceful category (Recovery intensity)
            SceneArchetypeType.QuietReflection => "Peaceful",
            SceneArchetypeType.CasualEncounter => "Peaceful",
            SceneArchetypeType.ScholarlyPursuit => "Peaceful",

            // Encounter patterns - should NOT be tracked for A-story rhythm
            // If these reach intensity recording, it indicates a data authoring error
            // (encounter pattern marked as MainStory category)
            SceneArchetypeType.InnLodging => "Encounter",
            SceneArchetypeType.ConsequenceReflection => "Encounter",
            SceneArchetypeType.DeliveryContract => "Encounter",
            SceneArchetypeType.RouteSegmentTravel => "Encounter",

            // FAIL-FAST: Unknown archetype is a code error (enum extended without updating mapping)
            _ => throw new InvalidOperationException(
                $"Unknown SceneArchetypeType '{archetype}' - add explicit mapping to ArchetypeCategorySelector")
        };
    }

    /// <summary>
    /// Map SceneArchetypeType to its intensity level.
    /// Used for intensity recording when A-story scenes complete.
    /// PUBLIC: Called by SituationCompletionHandler for intensity tracking.
    ///
    /// FAIL-FAST: Throws for unknown archetypes to catch authoring errors.
    /// Encounter patterns mapped to Standard (should not be tracked for A-story rhythm).
    /// </summary>
    public static ArchetypeIntensity MapArchetypeToIntensity(SceneArchetypeType archetype)
    {
        return archetype switch
        {
            // Peaceful category = Recovery intensity (earned structural respite)
            SceneArchetypeType.QuietReflection => ArchetypeIntensity.Recovery,
            SceneArchetypeType.CasualEncounter => ArchetypeIntensity.Recovery,
            SceneArchetypeType.ScholarlyPursuit => ArchetypeIntensity.Recovery,

            // Investigation category = Standard intensity
            SceneArchetypeType.InvestigateLocation => ArchetypeIntensity.Standard,
            SceneArchetypeType.GatherTestimony => ArchetypeIntensity.Standard,
            SceneArchetypeType.SeekAudience => ArchetypeIntensity.Standard,
            SceneArchetypeType.DiscoverArtifact => ArchetypeIntensity.Standard,
            SceneArchetypeType.UncoverConspiracy => ArchetypeIntensity.Standard,

            // Social category = Standard intensity
            SceneArchetypeType.MeetOrderMember => ArchetypeIntensity.Standard,

            // Confrontation category = Demanding intensity
            SceneArchetypeType.ConfrontAntagonist => ArchetypeIntensity.Demanding,

            // Crisis category = Demanding intensity
            SceneArchetypeType.UrgentDecision => ArchetypeIntensity.Demanding,
            SceneArchetypeType.MoralCrossroads => ArchetypeIntensity.Demanding,

            // Encounter patterns - should NOT be tracked for A-story rhythm
            // If these reach intensity recording, it indicates a data authoring error
            SceneArchetypeType.InnLodging => ArchetypeIntensity.Standard,
            SceneArchetypeType.ConsequenceReflection => ArchetypeIntensity.Standard,
            SceneArchetypeType.DeliveryContract => ArchetypeIntensity.Standard,
            SceneArchetypeType.RouteSegmentTravel => ArchetypeIntensity.Standard,

            // FAIL-FAST: Unknown archetype is a code error (enum extended without updating mapping)
            _ => throw new InvalidOperationException(
                $"Unknown SceneArchetypeType '{archetype}' - add explicit mapping to ArchetypeCategorySelector")
        };
    }
}
