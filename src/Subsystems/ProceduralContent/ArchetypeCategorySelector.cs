/// <summary>
/// Context-aware archetype category selection for procedural A-story generation.
/// Replaces simple 8-cycle rotation with weighted scoring based on multiple factors.
///
/// DESIGN PRINCIPLES:
/// - Fully deterministic: same inputs ALWAYS produce same output
/// - NO player resource influence (Challenge Philosophy - no coddling)
/// - STRONG location context influence (Safety/Purpose drive category)
/// - Peaceful is purely context-based (earned through intensity history)
///
/// SCORING FACTORS:
/// 1. Base Rotation Score: Sequence-based baseline (maintains some predictability)
/// 2. Location Context Score: Strong influence from Safety and Purpose
/// 3. Intensity Balance Score: Balances recent Recovery/Standard/Demanding
/// 4. Rhythm Phase Score: Considers where we are in accumulation→test→recovery
///
/// See gdd/06_balance.md §6.8 for Challenge Philosophy.
/// </summary>
public class ArchetypeCategorySelector
{
    /// <summary>
    /// All available archetype categories for A-story scenes.
    /// </summary>
    private static readonly List<string> AllCategories = new List<string>
    {
        "Investigation",
        "Social",
        "Confrontation",
        "Crisis",
        "Peaceful"
    };

    /// <summary>
    /// Select archetype category using weighted scoring.
    /// Deterministic: same inputs always produce same category.
    ///
    /// CHALLENGE PHILOSOPHY: Player resources (Health, Stamina, Resolve) are NOT considered.
    /// Fair rhythm emerges from story structure and location context, not player state filtering.
    /// </summary>
    public static string SelectCategory(
        int sequence,
        Player player,
        Location targetLocation,
        AStoryContext context)
    {
        List<CategoryScore> scores = new List<CategoryScore>();

        foreach (string category in AllCategories)
        {
            int score = 0;

            // Factor 1: Base rotation score (maintains some predictability)
            score += CalculateBaseRotationScore(sequence, category);

            // Factor 2: Location context score (STRONG influence)
            score += CalculateLocationContextScore(targetLocation, category);

            // Factor 3: Intensity balance score (prevents monotony)
            score += CalculateIntensityBalanceScore(player, category);

            // Factor 4: Rhythm phase score (narrative flow)
            score += CalculateRhythmPhaseScore(player, category);

            // Factor 5: Anti-repetition penalty
            score += CalculateAntiRepetitionScore(context, category);

            scores.Add(new CategoryScore { Category = category, Score = score });
        }

        // Deterministic selection: highest score wins
        // Tiebreaker: use sequence hash to pick consistently
        List<CategoryScore> sortedScores = scores
            .OrderByDescending(s => s.Score)
            .ThenBy(s => GetDeterministicTiebreaker(sequence, s.Category))
            .ToList();

        return sortedScores.First().Category;
    }

    /// <summary>
    /// Factor 1: Base rotation score.
    /// Provides baseline predictability based on sequence position.
    /// Lower weight than location context - this is the "rotation with shifts" approach.
    /// </summary>
    private static int CalculateBaseRotationScore(int sequence, string category)
    {
        int cyclePosition = (sequence - 1) % 8;

        // Base category affinities by position (lower scores than location influence)
        // This maintains SOME rotation structure while allowing context to override
        return cyclePosition switch
        {
            0 => category == "Investigation" ? 15 : 0,
            1 => category == "Social" ? 15 : 0,
            2 => category == "Confrontation" ? 15 : 0,
            3 => category == "Crisis" ? 15 : 0,
            4 => category == "Investigation" ? 15 : 0,
            5 => category == "Social" ? 15 : 0,
            6 => category == "Confrontation" ? 15 : 0,
            7 => category == "Peaceful" ? 15 : 0,
            _ => 0
        };
    }

    /// <summary>
    /// Factor 2: Location context score (STRONG influence).
    /// Location Safety and Purpose significantly influence category selection.
    /// This is the primary driver of context-aware selection per Q4: A (Strong).
    /// </summary>
    private static int CalculateLocationContextScore(Location location, string category)
    {
        if (location == null) return 0;

        int score = 0;

        // Safety strongly influences category (Q4: A - Strong location influence)
        score += location.Safety switch
        {
            LocationSafety.Dangerous => category switch
            {
                "Crisis" => 30,        // Dangerous locations strongly favor Crisis
                "Confrontation" => 25, // Also favor Confrontation
                "Investigation" => 10, // Can investigate danger
                "Social" => -10,       // Social less appropriate in danger
                "Peaceful" => -20,     // Peaceful very inappropriate in danger
                _ => 0
            },
            LocationSafety.Risky => category switch
            {
                "Confrontation" => 20, // Risky favors Confrontation
                "Investigation" => 15, // Investigation fits risky environments
                "Crisis" => 10,        // Some Crisis potential
                "Social" => 5,         // Social possible but less natural
                "Peaceful" => -5,      // Peaceful less appropriate
                _ => 0
            },
            LocationSafety.Safe => category switch
            {
                "Peaceful" => 25,      // Safe locations favor Peaceful
                "Social" => 20,        // Social thrives in safety
                "Investigation" => 10, // Can investigate safely
                "Confrontation" => -5, // Confrontation less natural
                "Crisis" => -15,       // Crisis less likely in safe places
                _ => 0
            },
            _ => 0
        };

        // Purpose influences category
        score += location.Purpose switch
        {
            LocationPurpose.Governance => category switch
            {
                "Confrontation" => 15, // Political confrontation
                "Social" => 10,        // Political maneuvering
                "Crisis" => 5,         // Political crisis
                _ => 0
            },
            LocationPurpose.Commerce => category switch
            {
                "Investigation" => 15, // Trade investigations
                "Social" => 10,        // Merchant dealings
                _ => 0
            },
            LocationPurpose.Worship => category switch
            {
                "Peaceful" => 20,      // Worship spaces favor reflection
                "Social" => 10,        // Community gathering
                "Investigation" => 5,  // Seeking wisdom
                _ => 0
            },
            LocationPurpose.Dwelling => category switch
            {
                "Social" => 15,        // Home visits, hospitality
                "Peaceful" => 10,      // Domestic peace
                _ => 0
            },
            LocationPurpose.Civic => category switch
            {
                "Social" => 15,        // Community interaction
                "Investigation" => 10, // Public inquiries
                "Confrontation" => 5,  // Public disputes
                _ => 0
            },
            _ => 0
        };

        return score;
    }

    /// <summary>
    /// Factor 3: Intensity balance score.
    /// Prevents monotony by balancing Recovery/Standard/Demanding.
    /// Q6: C - Peaceful is purely context-based, triggered by intensity history.
    /// </summary>
    private static int CalculateIntensityBalanceScore(Player player, string category)
    {
        int score = 0;

        // Get intensity counts from history
        int demandingCount = player.GetRecentIntensityCount(ArchetypeIntensity.Demanding);
        int recoveryCount = player.GetRecentIntensityCount(ArchetypeIntensity.Recovery);
        int standardCount = player.GetRecentIntensityCount(ArchetypeIntensity.Standard);

        // Scenes since last of each intensity
        int scenesSinceRecovery = player.GetScenesSinceIntensity(ArchetypeIntensity.Recovery);
        int scenesSinceDemanding = player.GetScenesSinceIntensity(ArchetypeIntensity.Demanding);

        // Heavy demanding history strongly triggers Peaceful (context-based recovery)
        if (player.IsIntensityHeavy())
        {
            score += category switch
            {
                "Peaceful" => 40,      // Strong push toward recovery
                "Social" => 15,        // Social as lighter alternative
                "Investigation" => 10, // Investigation as moderate option
                "Confrontation" => -10,
                "Crisis" => -20,
                _ => 0
            };
        }

        // No recovery in 6+ scenes strongly favors Peaceful
        if (scenesSinceRecovery >= 6)
        {
            score += category switch
            {
                "Peaceful" => 35,
                "Social" => 10,
                _ => 0
            };
        }

        // Too much recovery recently reduces Peaceful, increases Demanding
        if (recoveryCount >= 2 && player.SceneIntensityHistory.Count >= 4)
        {
            score += category switch
            {
                "Peaceful" => -25,     // Don't pile up recovery
                "Crisis" => 15,        // Time for challenge
                "Confrontation" => 10, // Time for confrontation
                _ => 0
            };
        }

        // No demanding content recently - increase demand
        if (scenesSinceDemanding >= 4)
        {
            score += category switch
            {
                "Crisis" => 20,
                "Confrontation" => 15,
                "Peaceful" => -10,
                _ => 0
            };
        }

        return score;
    }

    /// <summary>
    /// Factor 4: Rhythm phase score.
    /// Considers narrative flow: accumulation → test → recovery.
    /// Just completed Crisis/Peaceful affects next selection.
    /// </summary>
    private static int CalculateRhythmPhaseScore(Player player, string category)
    {
        if (player.SceneIntensityHistory.Count == 0) return 0;

        int score = 0;
        SceneIntensityRecord lastScene = player.SceneIntensityHistory.Last();

        // Just completed Crisis - favor recovery/lighter content
        if (lastScene.WasCrisisRhythm)
        {
            score += category switch
            {
                "Investigation" => 20, // Investigation good for recovery
                "Social" => 15,        // Social good for recovery
                "Peaceful" => 10,      // Peaceful if intensity was high
                "Confrontation" => -5,
                "Crisis" => -15,       // Avoid back-to-back Crisis
                _ => 0
            };
        }

        // Just completed Peaceful - favor engagement
        if (lastScene.Intensity == ArchetypeIntensity.Recovery)
        {
            score += category switch
            {
                "Investigation" => 15, // Re-engagement through investigation
                "Social" => 15,        // Re-engagement through social
                "Confrontation" => 10, // Ready for challenge
                "Peaceful" => -20,     // Don't repeat Peaceful
                "Crisis" => 5,         // Crisis possible after rest
                _ => 0
            };
        }

        // Track momentum - multiple standard in a row builds toward demanding
        int consecutiveStandard = 0;
        for (int i = player.SceneIntensityHistory.Count - 1; i >= 0; i--)
        {
            if (player.SceneIntensityHistory[i].Intensity == ArchetypeIntensity.Standard)
            {
                consecutiveStandard++;
            }
            else break;
        }

        if (consecutiveStandard >= 3)
        {
            score += category switch
            {
                "Crisis" => 20,        // Build-up leads to Crisis
                "Confrontation" => 15, // Or Confrontation
                "Peaceful" => -5,
                _ => 0
            };
        }

        return score;
    }

    /// <summary>
    /// Factor 5: Anti-repetition penalty.
    /// Penalizes categories that were used very recently.
    /// </summary>
    private static int CalculateAntiRepetitionScore(AStoryContext context, string category)
    {
        // Check if this category was used in the last 2 scenes
        List<string> recentCategories = context.RecentArchetypes
            .TakeLast(2)
            .Select(a => MapArchetypeToCategory(a))
            .ToList();

        if (recentCategories.Contains(category))
        {
            return -15; // Penalty for recent repetition
        }

        return 0;
    }

    /// <summary>
    /// Map SceneArchetypeType to its category string.
    /// Used for anti-repetition checking and intensity recording.
    /// PUBLIC: Called by SituationCompletionHandler for intensity tracking.
    /// </summary>
    public static string MapArchetypeToCategory(SceneArchetypeType archetype)
    {
        return archetype switch
        {
            SceneArchetypeType.InvestigateLocation => "Investigation",
            SceneArchetypeType.GatherTestimony => "Investigation",
            SceneArchetypeType.SeekAudience => "Investigation",
            SceneArchetypeType.DiscoverArtifact => "Investigation",
            SceneArchetypeType.UncoverConspiracy => "Investigation",
            SceneArchetypeType.MeetOrderMember => "Social",
            SceneArchetypeType.ConfrontAntagonist => "Confrontation",
            SceneArchetypeType.UrgentDecision => "Crisis",
            SceneArchetypeType.MoralCrossroads => "Crisis",
            SceneArchetypeType.QuietReflection => "Peaceful",
            SceneArchetypeType.CasualEncounter => "Peaceful",
            SceneArchetypeType.ScholarlyPursuit => "Peaceful",
            _ => "Investigation" // Default fallback
        };
    }

    /// <summary>
    /// Map SceneArchetypeType to its intensity level.
    /// Used for intensity recording when A-story scenes complete.
    /// PUBLIC: Called by SituationCompletionHandler for intensity tracking.
    /// </summary>
    public static ArchetypeIntensity MapArchetypeToIntensity(SceneArchetypeType archetype)
    {
        return archetype switch
        {
            // Peaceful category = Recovery intensity
            SceneArchetypeType.QuietReflection => ArchetypeIntensity.Recovery,
            SceneArchetypeType.CasualEncounter => ArchetypeIntensity.Recovery,
            SceneArchetypeType.ScholarlyPursuit => ArchetypeIntensity.Recovery,

            // Investigation/Social categories = Standard intensity
            SceneArchetypeType.InvestigateLocation => ArchetypeIntensity.Standard,
            SceneArchetypeType.GatherTestimony => ArchetypeIntensity.Standard,
            SceneArchetypeType.SeekAudience => ArchetypeIntensity.Standard,
            SceneArchetypeType.DiscoverArtifact => ArchetypeIntensity.Standard,
            SceneArchetypeType.UncoverConspiracy => ArchetypeIntensity.Standard,
            SceneArchetypeType.MeetOrderMember => ArchetypeIntensity.Standard,

            // Crisis/Confrontation categories = Demanding intensity
            SceneArchetypeType.ConfrontAntagonist => ArchetypeIntensity.Demanding,
            SceneArchetypeType.UrgentDecision => ArchetypeIntensity.Demanding,
            SceneArchetypeType.MoralCrossroads => ArchetypeIntensity.Demanding,

            // Service patterns default to Standard (not tracked for A-story rhythm)
            _ => ArchetypeIntensity.Standard
        };
    }

    /// <summary>
    /// Deterministic tiebreaker using sequence and category.
    /// Ensures same inputs always produce same output even when scores tie.
    /// </summary>
    private static int GetDeterministicTiebreaker(int sequence, string category)
    {
        // Use hash of sequence + category for deterministic ordering
        return (sequence * 31 + category.GetHashCode()) % 1000;
    }

    /// <summary>
    /// Internal class for scoring categories.
    /// </summary>
    private class CategoryScore
    {
        public string Category { get; set; }
        public int Score { get; set; }
    }
}
