/// <summary>
/// Repository of all strategic tags in the game
/// </summary>
public class StrategicTagRepository
{
    // DOMINANCE-BASED STRATEGIC TAGS
    public static readonly StrategicTag OverwhelmingForce = new(
        "Overwhelming Force",
        StrategicEffectTypes.IncreaseMomentum,
        EncounterStateTags.Dominance);

    public static readonly StrategicTag CommandingPresence = new(
        "Commanding Presence",
        StrategicEffectTypes.DecreasePressure,
        EncounterStateTags.Dominance);

    public static readonly StrategicTag BruteForceBackfire = new(
        "Brute Force Backfire",
        StrategicEffectTypes.DecreaseMomentum,
        EncounterStateTags.Dominance);

    public static readonly StrategicTag EscalatingTension = new(
        "Escalating Tension",
        StrategicEffectTypes.IncreasePressure,
        EncounterStateTags.Dominance);

    // RAPPORT-BASED STRATEGIC TAGS
    public static readonly StrategicTag SocialCurrency = new(
        "Social Currency",
        StrategicEffectTypes.IncreaseMomentum,
        EncounterStateTags.Rapport);

    public static readonly StrategicTag CalmingInfluence = new(
        "Calming Influence",
        StrategicEffectTypes.DecreasePressure,
        EncounterStateTags.Rapport);

    public static readonly StrategicTag SocialDistraction = new(
        "Social Distraction",
        StrategicEffectTypes.DecreaseMomentum,
        EncounterStateTags.Rapport);

    public static readonly StrategicTag SocialAwkwardness = new(
        "Social Awkwardness",
        StrategicEffectTypes.IncreasePressure,
        EncounterStateTags.Rapport);

    // ANALYSIS-BASED STRATEGIC TAGS
    public static readonly StrategicTag InsightfulApproach = new(
        "Insightful Approach",
        StrategicEffectTypes.IncreaseMomentum,
        EncounterStateTags.Analysis);

    public static readonly StrategicTag CalculatedResponse = new(
        "Calculated Response",
        StrategicEffectTypes.DecreasePressure,
        EncounterStateTags.Analysis);

    public static readonly StrategicTag Overthinking = new(
        "Overthinking",
        StrategicEffectTypes.DecreaseMomentum,
        EncounterStateTags.Analysis);

    public static readonly StrategicTag AnalyticalAnxiety = new(
        "Analytical Anxiety",
        StrategicEffectTypes.IncreasePressure,
        EncounterStateTags.Analysis);

    // PRECISION-BASED STRATEGIC TAGS
    public static readonly StrategicTag MasterfulExecution = new(
        "Masterful Execution",
        StrategicEffectTypes.IncreaseMomentum,
        EncounterStateTags.Precision);

    public static readonly StrategicTag CarefulPositioning = new(
        "Careful Positioning",
        StrategicEffectTypes.DecreasePressure,
        EncounterStateTags.Precision);

    public static readonly StrategicTag RigidMethodology = new(
        "Rigid Methodology",
        StrategicEffectTypes.DecreaseMomentum,
        EncounterStateTags.Precision);

    public static readonly StrategicTag PerfectionistPressure = new(
        "Perfectionist Pressure",
        StrategicEffectTypes.IncreasePressure,
        EncounterStateTags.Precision);

    // CONCEALMENT-BASED STRATEGIC TAGS
    public static readonly StrategicTag TacticalAdvantage = new(
        "Tactical Advantage",
        StrategicEffectTypes.IncreaseMomentum,
        EncounterStateTags.Concealment);

    public static readonly StrategicTag InvisiblePresence = new(
        "Invisible Presence",
        StrategicEffectTypes.DecreasePressure,
        EncounterStateTags.Concealment);

    public static readonly StrategicTag OvercautiousApproach = new(
        "Overcautious Approach",
        StrategicEffectTypes.DecreaseMomentum,
        EncounterStateTags.Concealment);

    public static readonly StrategicTag SuspiciousBehavior = new(
        "Suspicious Behavior",
        StrategicEffectTypes.IncreasePressure,
        EncounterStateTags.Concealment);
}