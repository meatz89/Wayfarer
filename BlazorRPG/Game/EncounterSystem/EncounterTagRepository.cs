/// <summary>
/// Repository of all encounter tags in the system
/// </summary>
public class EncounterTagRepository
{
    private readonly Dictionary<string, EncounterTag> _tags = new Dictionary<string, EncounterTag>();

    public EncounterTagRepository()
    {
        InitializeTags();
    }

    public EncounterTag GetTag(string id)
    {
        return _tags.ContainsKey(id) ? _tags[id] : null;
    }

    public IEnumerable<EncounterTag> GetAllTags()
    {
        return _tags.Values;
    }

    public IEnumerable<EncounterTag> GetTagsByElement(SignatureElementTypes elementType)
    {
        return _tags.Values.Where(t => t.SourceElement == elementType);
    }

    public IEnumerable<EncounterTag> GetTagsByThreshold(int threshold)
    {
        return _tags.Values.Where(t => t.ThresholdValue == threshold);
    }

    /// <summary>
    /// Initialize all possible tags with their effects
    /// </summary>
    private void InitializeTags()
    {
        // DOMINANCE TAGS (Force approach)

        // Level 3 Dominance Tags
        AddTag(
            "intimidation_tactics",
            "Intimidation Tactics",
            "Physical-related choices give +1 additional momentum",
            SignatureElementTypes.Dominance,
            3,
            new TagEffect
            {
                AffectedFocus = FocusTypes.Physical,
                MomentumModifier = 1
            }
        );

        AddTag(
            "forceful_authority",
            "Forceful Authority",
            "Relationship-related choices generate no pressure",
            SignatureElementTypes.Dominance,
            3,
            new TagEffect
            {
                AffectedFocus = FocusTypes.Relationship,
                ZeroPressure = true
            }
        );

        AddTag(
            "direct_approach",
            "Direct Approach",
            "Environment-related choices give +1 additional momentum",
            SignatureElementTypes.Dominance,
            3,
            new TagEffect
            {
                AffectedFocus = FocusTypes.Environment,
                MomentumModifier = 1
            }
        );

        // Level 5 Dominance Tags
        AddTag(
            "overwhelming_presence",
            "Overwhelming Presence",
            "Convert all pressure to momentum",
            SignatureElementTypes.Dominance,
            5,
            new TagEffect
            {
                IsSpecialEffect = true,
                SpecialEffectId = "convert_pressure_to_momentum"
            }
        );

        AddTag(
            "commanding_voice",
            "Commanding Voice",
            "Double momentum from relationship-related choices",
            SignatureElementTypes.Dominance,
            5,
            new TagEffect
            {
                AffectedFocus = FocusTypes.Relationship,
                DoubleMomentum = true
            }
        );

        AddTag(
            "forceful_breakthrough",
            "Forceful Breakthrough",
            "Ignore pressure from physical-related choices",
            SignatureElementTypes.Dominance,
            5,
            new TagEffect
            {
                AffectedFocus = FocusTypes.Physical,
                ZeroPressure = true
            }
        );

        // RAPPORT TAGS (Charm approach)

        // Level 3 Rapport Tags
        AddTag(
            "social_currency",
            "Social Currency",
            "Resource-related choices give +1 additional momentum",
            SignatureElementTypes.Rapport,
            3,
            new TagEffect
            {
                AffectedFocus = FocusTypes.Resource,
                MomentumModifier = 1
            }
        );

        AddTag(
            "emotional_insight",
            "Emotional Insight",
            "Relationship-related choices give +1 additional momentum",
            SignatureElementTypes.Rapport,
            3,
            new TagEffect
            {
                AffectedFocus = FocusTypes.Relationship,
                MomentumModifier = 1
            }
        );

        AddTag(
            "smooth_talker",
            "Smooth Talker",
            "Information-related choices generate no pressure",
            SignatureElementTypes.Rapport,
            3,
            new TagEffect
            {
                AffectedFocus = FocusTypes.Information,
                ZeroPressure = true
            }
        );

        // Level 5 Rapport Tags
        AddTag(
            "network_leverage",
            "Network Leverage",
            "Reduce pressure by 1 at end of each turn",
            SignatureElementTypes.Rapport,
            5,
            new TagEffect
            {
                IsSpecialEffect = true,
                SpecialEffectId = "reduce_pressure_each_turn",
                PressureModifier = -1
            }
        );

        AddTag(
            "captivating_presence",
            "Captivating Presence",
            "Double momentum from information-related choices",
            SignatureElementTypes.Rapport,
            5,
            new TagEffect
            {
                AffectedFocus = FocusTypes.Information,
                DoubleMomentum = true
            }
        );

        AddTag(
            "charming_negotiator",
            "Charming Negotiator",
            "Resource-related choices generate no pressure",
            SignatureElementTypes.Rapport,
            5,
            new TagEffect
            {
                AffectedFocus = FocusTypes.Resource,
                ZeroPressure = true
            }
        );

        // ANALYSIS TAGS (Wit approach)

        // Level 3 Analysis Tags
        AddTag(
            "market_insight",
            "Market Insight",
            "Information-related choices generate no pressure",
            SignatureElementTypes.Analysis,
            3,
            new TagEffect
            {
                AffectedFocus = FocusTypes.Information,
                ZeroPressure = true
            }
        );

        AddTag(
            "strategic_planning",
            "Strategic Planning",
            "Physical-related choices give +1 additional momentum",
            SignatureElementTypes.Analysis,
            3,
            new TagEffect
            {
                AffectedFocus = FocusTypes.Physical,
                MomentumModifier = 1
            }
        );

        AddTag(
            "resource_optimization",
            "Resource Optimization",
            "Resource-related choices generate no pressure",
            SignatureElementTypes.Analysis,
            3,
            new TagEffect
            {
                AffectedFocus = FocusTypes.Resource,
                ZeroPressure = true
            }
        );

        // Level 5 Analysis Tags
        AddTag(
            "negotiation_mastery",
            "Negotiation Mastery",
            "Double momentum from resource-related choices",
            SignatureElementTypes.Analysis,
            5,
            new TagEffect
            {
                AffectedFocus = FocusTypes.Resource,
                DoubleMomentum = true
            }
        );

        AddTag(
            "tactical_advantage",
            "Tactical Advantage",
            "Double momentum from physical-related choices",
            SignatureElementTypes.Analysis,
            5,
            new TagEffect
            {
                AffectedFocus = FocusTypes.Physical,
                DoubleMomentum = true
            }
        );

        AddTag(
            "comprehensive_understanding",
            "Comprehensive Understanding",
            "Information choices give +2 additional momentum",
            SignatureElementTypes.Analysis,
            5,
            new TagEffect
            {
                AffectedFocus = FocusTypes.Information,
                MomentumModifier = 2
            }
        );

        // PRECISION TAGS (Finesse approach)

        // Level 3 Precision Tags
        AddTag(
            "precision_footwork",
            "Precision Footwork",
            "Ignore 1 pressure from environment-related choices",
            SignatureElementTypes.Precision,
            3,
            new TagEffect
            {
                AffectedFocus = FocusTypes.Environment,
                PressureModifier = -1
            }
        );

        AddTag(
            "efficient_movement",
            "Efficient Movement",
            "Physical-related choices generate no pressure",
            SignatureElementTypes.Precision,
            3,
            new TagEffect
            {
                AffectedFocus = FocusTypes.Physical,
                ZeroPressure = true
            }
        );

        AddTag(
            "careful_allocation",
            "Careful Allocation",
            "Resource-related choices give +1 additional momentum",
            SignatureElementTypes.Precision,
            3,
            new TagEffect
            {
                AffectedFocus = FocusTypes.Resource,
                MomentumModifier = 1
            }
        );

        // Level 5 Precision Tags
        AddTag(
            "flawless_execution",
            "Flawless Execution",
            "Reduce all pressure by 1 at the end of each turn",
            SignatureElementTypes.Precision,
            5,
            new TagEffect
            {
                IsSpecialEffect = true,
                SpecialEffectId = "reduce_pressure_each_turn",
                PressureModifier = -1
            }
        );

        AddTag(
            "perfect_technique",
            "Perfect Technique",
            "Double momentum from physical-related choices",
            SignatureElementTypes.Precision,
            5,
            new TagEffect
            {
                AffectedFocus = FocusTypes.Physical,
                DoubleMomentum = true
            }
        );

        AddTag(
            "meticulous_approach",
            "Meticulous Approach",
            "All choices generate +1 additional momentum",
            SignatureElementTypes.Precision,
            5,
            new TagEffect
            {
                MomentumModifier = 1
            }
        );

        // CONCEALMENT TAGS (Stealth approach)

        // Level 3 Concealment Tags
        AddTag(
            "shadow_movement",
            "Shadow Movement",
            "+1 additional momentum for stealth choices",
            SignatureElementTypes.Concealment,
            3,
            new TagEffect
            {
                AffectedApproach = ApproachTypes.Stealth,
                MomentumModifier = 1
            }
        );

        AddTag(
            "unseen_threat",
            "Unseen Threat",
            "Take an additional turn with no pressure",
            SignatureElementTypes.Concealment,
            3,
            new TagEffect
            {
                IsSpecialEffect = true,
                SpecialEffectId = "additional_turn_no_pressure",
                ZeroPressure = true
            }
        );

        AddTag(
            "hidden_observation",
            "Hidden Observation",
            "Information-related choices give +1 additional momentum",
            SignatureElementTypes.Concealment,
            3,
            new TagEffect
            {
                AffectedFocus = FocusTypes.Information,
                MomentumModifier = 1
            }
        );

        // Level 5 Concealment Tags
        AddTag(
            "perfect_stealth",
            "Perfect Stealth",
            "Ignore all pressure from physical-related choices",
            SignatureElementTypes.Concealment,
            5,
            new TagEffect
            {
                AffectedFocus = FocusTypes.Physical,
                ZeroPressure = true
            }
        );

        AddTag(
            "perfect_ambush",
            "Perfect Ambush",
            "Gain maximum momentum (encounter auto-success)",
            SignatureElementTypes.Concealment,
            5,
            new TagEffect
            {
                IsSpecialEffect = true,
                SpecialEffectId = "encounter_auto_success",
                MomentumModifier = 15 // High value to ensure success
            }
        );

        AddTag(
            "vanish",
            "Vanish",
            "Ignore all pressure for one turn",
            SignatureElementTypes.Concealment,
            5,
            new TagEffect
            {
                ZeroPressure = true
            }
        );
    }

    private void AddTag(string id, string name, string description, SignatureElementTypes sourceElement,
                       int thresholdValue, TagEffect effect)
    {
        _tags[id] = new EncounterTag(id, name, description, sourceElement, thresholdValue, effect);
    }
}
