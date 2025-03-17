/// <summary>
/// Repository of all narrative tags in the game
/// </summary>
public class NarrativeTagRepository
{
    // DOMINANCE-BASED NARRATIVE TAGS
    public static readonly NarrativeTag IntimidatingPresence = new(
        "Intimidating Presence",
        new FocusThresholdCondition(EncounterStateTags.Dominance, 3),
        FocusTags.Relationship);

    public static readonly NarrativeTag BattleRage = new(
        "Battle Rage",
        new FocusThresholdCondition(EncounterStateTags.Dominance, 3),
        FocusTags.Information);

    public static readonly NarrativeTag BruteForceFixation = new(
        "Brute Force Fixation",
        new FocusThresholdCondition(EncounterStateTags.Dominance, 4),
        FocusTags.Physical);

    public static readonly NarrativeTag TunnelVision = new(
        "Tunnel Vision",
        new FocusThresholdCondition(EncounterStateTags.Dominance, 3),
        FocusTags.Environment);

    public static readonly NarrativeTag DestructiveImpulse = new(
        "Destructive Impulse",
        new FocusThresholdCondition(EncounterStateTags.Dominance, 3),
        FocusTags.Resource);

    // RAPPORT-BASED NARRATIVE TAGS
    public static readonly NarrativeTag SuperficialCharm = new(
        "Superficial Charm",
        new FocusThresholdCondition(EncounterStateTags.Rapport, 4),
        FocusTags.Relationship);

    public static readonly NarrativeTag SocialDistraction = new(
        "Social Distraction",
        new FocusThresholdCondition(EncounterStateTags.Rapport, 3),
        FocusTags.Information);

    public static readonly NarrativeTag HesitantPoliteness = new(
        "Hesitant Politeness",
        new FocusThresholdCondition(EncounterStateTags.Rapport, 3),
        FocusTags.Physical);

    public static readonly NarrativeTag PublicAwareness = new(
        "Public Awareness",
        new FocusThresholdCondition(EncounterStateTags.Rapport, 3),
        FocusTags.Environment);

    public static readonly NarrativeTag GenerousSpirit = new(
        "Generous Spirit",
        new FocusThresholdCondition(EncounterStateTags.Rapport, 4),
        FocusTags.Resource);

    // ANALYSIS-BASED NARRATIVE TAGS
    public static readonly NarrativeTag ColdCalculation = new(
        "Cold Calculation",
        new FocusThresholdCondition(EncounterStateTags.Analysis, 3),
        FocusTags.Relationship);

    public static readonly NarrativeTag AnalysisParalysis = new(
        "Analysis Paralysis",
        new FocusThresholdCondition(EncounterStateTags.Analysis, 4),
        FocusTags.Information);

    public static readonly NarrativeTag Overthinking = new(
        "Overthinking",
        new FocusThresholdCondition(EncounterStateTags.Analysis, 3),
        FocusTags.Physical);

    public static readonly NarrativeTag DetailFixation = new(
        "Detail Fixation",
        new FocusThresholdCondition(EncounterStateTags.Analysis, 3),
        FocusTags.Environment);

    public static readonly NarrativeTag TheoreticalMindset = new(
        "Theoretical Mindset",
        new FocusThresholdCondition(EncounterStateTags.Analysis, 4),
        FocusTags.Resource);

    // PRECISION-BASED NARRATIVE TAGS
    public static readonly NarrativeTag MechanicalInteraction = new(
        "Mechanical Interaction",
        new FocusThresholdCondition(EncounterStateTags.Precision, 3),
        FocusTags.Relationship);

    public static readonly NarrativeTag NarrowFocus = new(
        "Narrow Focus",
        new FocusThresholdCondition(EncounterStateTags.Precision, 3),
        FocusTags.Information);

    public static readonly NarrativeTag PerfectionistParalysis = new(
        "Perfectionist Paralysis",
        new FocusThresholdCondition(EncounterStateTags.Precision, 4),
        FocusTags.Physical);

    public static readonly NarrativeTag DetailObsession = new(
        "Detail Obsession",
        new FocusThresholdCondition(EncounterStateTags.Precision, 3),
        FocusTags.Environment);

    public static readonly NarrativeTag InefficientPerfectionism = new(
        "Inefficient Perfectionism",
        new FocusThresholdCondition(EncounterStateTags.Precision, 4),
        FocusTags.Resource);

    // CONCEALMENT-BASED NARRATIVE TAGS
    public static readonly NarrativeTag ShadowVeil = new(
        "Shadow Veil",
        new FocusThresholdCondition(EncounterStateTags.Concealment, 3),
        FocusTags.Relationship);

    public static readonly NarrativeTag ParanoidMindset = new(
        "Paranoid Mindset",
        new FocusThresholdCondition(EncounterStateTags.Concealment, 3),
        FocusTags.Information);

    public static readonly NarrativeTag CautiousRestraint = new(
        "Cautious Restraint",
        new FocusThresholdCondition(EncounterStateTags.Concealment, 3),
        FocusTags.Physical);

    public static readonly NarrativeTag HidingPlaceFixation = new(
        "Hiding Place Fixation",
        new FocusThresholdCondition(EncounterStateTags.Concealment, 4),
        FocusTags.Environment);

    public static readonly NarrativeTag HoardingInstinct = new(
        "Hoarding Instinct",
        new FocusThresholdCondition(EncounterStateTags.Concealment, 3),
        FocusTags.Resource);
}