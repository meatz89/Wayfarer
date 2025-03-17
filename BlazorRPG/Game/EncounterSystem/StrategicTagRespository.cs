/// <summary>
/// Repository of all strategic tags in the game
/// </summary>
public class StrategicTagRepository
{
    // DOMINANCE-BASED STRATEGIC TAGS
    public static readonly StrategicTag OverwhelmingForce = new(
        "Overwhelming Force",
        StrategicEffectTypes.IncreaseMomentum,
        ApproachTags.Dominance);

    public static readonly StrategicTag CommandingPresence = new(
        "Commanding Presence",
        StrategicEffectTypes.DecreasePressure,
        ApproachTags.Dominance);

    public static readonly StrategicTag BruteForceBackfire = new(
        "Brute Force Backfire",
        StrategicEffectTypes.DecreaseMomentum,
        ApproachTags.Dominance);

    public static readonly StrategicTag EscalatingTension = new(
        "Escalating Tension",
        StrategicEffectTypes.IncreasePressure,
        ApproachTags.Dominance);

    // RAPPORT-BASED STRATEGIC TAGS
    public static readonly StrategicTag SocialCurrency = new(
        "Social Currency",
        StrategicEffectTypes.IncreaseMomentum,
        ApproachTags.Rapport);

    public static readonly StrategicTag CalmingInfluence = new(
        "Calming Influence",
        StrategicEffectTypes.DecreasePressure,
        ApproachTags.Rapport);

    public static readonly StrategicTag SocialDistraction = new(
        "Social Distraction",
        StrategicEffectTypes.DecreaseMomentum,
        ApproachTags.Rapport);

    public static readonly StrategicTag SocialAwkwardness = new(
        "Social Awkwardness",
        StrategicEffectTypes.IncreasePressure,
        ApproachTags.Rapport);

    // ANALYSIS-BASED STRATEGIC TAGS
    public static readonly StrategicTag InsightfulApproach = new(
        "Insightful Approach",
        StrategicEffectTypes.IncreaseMomentum,
        ApproachTags.Analysis);

    public static readonly StrategicTag CalculatedResponse = new(
        "Calculated Response",
        StrategicEffectTypes.DecreasePressure,
        ApproachTags.Analysis);

    public static readonly StrategicTag Overthinking = new(
        "Overthinking",
        StrategicEffectTypes.DecreaseMomentum,
        ApproachTags.Analysis);

    public static readonly StrategicTag AnalyticalAnxiety = new(
        "Analytical Anxiety",
        StrategicEffectTypes.IncreasePressure,
        ApproachTags.Analysis);

    // PRECISION-BASED STRATEGIC TAGS
    public static readonly StrategicTag MasterfulExecution = new(
        "Masterful Execution",
        StrategicEffectTypes.IncreaseMomentum,
        ApproachTags.Precision);

    public static readonly StrategicTag CarefulPositioning = new(
        "Careful Positioning",
        StrategicEffectTypes.DecreasePressure,
        ApproachTags.Precision);

    public static readonly StrategicTag RigidMethodology = new(
        "Rigid Methodology",
        StrategicEffectTypes.DecreaseMomentum,
        ApproachTags.Precision);

    public static readonly StrategicTag PerfectionistPressure = new(
        "Perfectionist Pressure",
        StrategicEffectTypes.IncreasePressure,
        ApproachTags.Precision);

    // CONCEALMENT-BASED STRATEGIC TAGS
    public static readonly StrategicTag TacticalAdvantage = new(
        "Tactical Advantage",
        StrategicEffectTypes.IncreaseMomentum,
        ApproachTags.Concealment);

    public static readonly StrategicTag InvisiblePresence = new(
        "Invisible Presence",
        StrategicEffectTypes.DecreasePressure,
        ApproachTags.Concealment);

    public static readonly StrategicTag OvercautiousApproach = new(
        "Overcautious Approach",
        StrategicEffectTypes.DecreaseMomentum,
        ApproachTags.Concealment);

    public static readonly StrategicTag SuspiciousBehavior = new(
        "Suspicious Behavior",
        StrategicEffectTypes.IncreasePressure,
        ApproachTags.Concealment);
}