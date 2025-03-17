/// <summary>
/// Repository of all strategic tags in the game
/// </summary>
public class StrategicTagRepository
{
    // DOMINANCE-BASED STRATEGIC TAGS
    public static readonly StrategicTag OverwhelmingForce = new(
        "Overwhelming Force",
        StrategicEffectTypes.IncreaseMomentum,
        null,
        ApproachTags.Dominance);

    public static readonly StrategicTag CommandingPresence = new(
        "Commanding Presence",
        StrategicEffectTypes.DecreasePressure,
        null,
        ApproachTags.Dominance);

    public static readonly StrategicTag BruteForceBackfire = new(
        "Brute Force Backfire",
        StrategicEffectTypes.DecreaseMomentum,
        null,
        ApproachTags.Dominance);

    public static readonly StrategicTag EscalatingTension = new(
        "Escalating Tension",
        StrategicEffectTypes.IncreasePressure,
        null,
        ApproachTags.Dominance);

    // RAPPORT-BASED STRATEGIC TAGS
    public static readonly StrategicTag SocialCurrency = new(
        "Social Currency",
        StrategicEffectTypes.IncreaseMomentum,
        null,
        ApproachTags.Rapport);

    public static readonly StrategicTag CalmingInfluence = new(
        "Calming Influence",
        StrategicEffectTypes.DecreasePressure,
        null,
        ApproachTags.Rapport);

    public static readonly StrategicTag SocialDistraction = new(
        "Social Distraction",
        StrategicEffectTypes.DecreaseMomentum,
        null,
        ApproachTags.Rapport);

    public static readonly StrategicTag SocialAwkwardness = new(
        "Social Awkwardness",
        StrategicEffectTypes.IncreasePressure,
        null,
        ApproachTags.Rapport);

    // ANALYSIS-BASED STRATEGIC TAGS
    public static readonly StrategicTag InsightfulApproach = new(
        "Insightful Approach",
        StrategicEffectTypes.IncreaseMomentum,
        null,
        ApproachTags.Analysis);

    public static readonly StrategicTag CalculatedResponse = new(
        "Calculated Response",
        StrategicEffectTypes.DecreasePressure,
        null,
        ApproachTags.Analysis);

    public static readonly StrategicTag Overthinking = new(
        "Overthinking",
        StrategicEffectTypes.DecreaseMomentum,
        null,
        ApproachTags.Analysis);

    public static readonly StrategicTag AnalyticalAnxiety = new(
        "Analytical Anxiety",
        StrategicEffectTypes.IncreasePressure,
        null,
        ApproachTags.Analysis);

    // PRECISION-BASED STRATEGIC TAGS
    public static readonly StrategicTag MasterfulExecution = new(
        "Masterful Execution",
        StrategicEffectTypes.IncreaseMomentum,
        null,
        ApproachTags.Precision);

    public static readonly StrategicTag CarefulPositioning = new(
        "Careful Positioning",
        StrategicEffectTypes.DecreasePressure,
        null,
        ApproachTags.Precision);

    public static readonly StrategicTag RigidMethodology = new(
        "Rigid Methodology",
        StrategicEffectTypes.DecreaseMomentum,
        null,
        ApproachTags.Precision);

    public static readonly StrategicTag PerfectionistPressure = new(
        "Perfectionist Pressure",
        StrategicEffectTypes.IncreasePressure,
        null,
        ApproachTags.Precision);

    // CONCEALMENT-BASED STRATEGIC TAGS
    public static readonly StrategicTag TacticalAdvantage = new(
        "Tactical Advantage",
        StrategicEffectTypes.IncreaseMomentum,
        null,
        ApproachTags.Concealment);

    public static readonly StrategicTag InvisiblePresence = new(
        "Invisible Presence",
        StrategicEffectTypes.DecreasePressure,
        null,
        ApproachTags.Concealment);

    public static readonly StrategicTag OvercautiousApproach = new(
        "Overcautious Approach",
        StrategicEffectTypes.DecreaseMomentum,
        null,
        ApproachTags.Concealment);

    public static readonly StrategicTag SuspiciousBehavior = new(
        "Suspicious Behavior",
        StrategicEffectTypes.IncreasePressure,
        null,
        ApproachTags.Concealment);
}