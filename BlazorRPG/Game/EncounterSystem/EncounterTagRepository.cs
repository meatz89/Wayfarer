
/// <summary>
/// Repository of all encounter tags in the system, including negative location reaction tags
/// </summary>
public class EncounterTagRepository
{
    private readonly Dictionary<string, EncounterTag> _tags = new Dictionary<string, EncounterTag>();

    public EncounterTagRepository()
    {
        InitializePlayerTags();
        InitializeLocationReactionTags();
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

    public IEnumerable<EncounterTag> GetLocationReactionTags()
    {
        return _tags.Values.Where(t => t.IsLocationReaction);
    }

    /// <summary>
    /// Initialize positive player tags (threshold-based)
    /// </summary>
    private void InitializePlayerTags()
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

    /// <summary>
    /// Initialize negative location reaction tags
    /// </summary>
    private void InitializeLocationReactionTags()
    {
        // Harbor Warehouse negative tags

        // Security Alert - Triggered by too much Force approach
        EncounterTag securityAlertTag = new EncounterTag(
            "security_alert",
            "Security Alert",
            "Guards are watching you carefully. Force-related choices generate double pressure.",
            SignatureElementTypes.Dominance, // Not used for threshold, but for categorization
            0, // No threshold
            new TagEffect
            {
                IsNegative = true,
                AffectedApproach = ApproachTypes.Force,
                DoublePressure = true
            }
        );

        // Add trigger for Force approach
        securityAlertTag.ActivationTriggers.Add(new TagTrigger(
            "force_trigger",
            "Using Force approach in a warehouse triggers security",
            ApproachTypes.Force
        ));

        // Add removal trigger for Stealth approach
        securityAlertTag.RemovalTriggers.Add(new TagTrigger(
            "stealth_removal",
            "Using Stealth can shake off security attention",
            ApproachTypes.Stealth
        ));

        securityAlertTag.IsLocationReaction = true;
        _tags[securityAlertTag.Id] = securityAlertTag;

        // Narrow Passages - Triggered by using Physical focus in the cramped warehouse
        EncounterTag narrowPassagesTag = new EncounterTag(
            "narrow_passages",
            "Narrow Passages",
            "The cramped spaces make physical actions difficult. Physical-focused choices generate +1 pressure.",
            SignatureElementTypes.Precision,
            0,
            new TagEffect
            {
                IsNegative = true,
                AffectedFocus = FocusTypes.Physical,
                PressureModifier = 1
            }
        );

        narrowPassagesTag.ActivationTriggers.Add(new TagTrigger(
            "physical_trigger",
            "Using Physical focus activates the cramped space disadvantage",
            null,
            FocusTypes.Physical
        ));

        narrowPassagesTag.RemovalTriggers.Add(new TagTrigger(
            "precision_removal",
            "Precision approach can help navigate the cramped spaces",
            ApproachTypes.Finesse
        ));

        narrowPassagesTag.IsLocationReaction = true;
        _tags[narrowPassagesTag.Id] = narrowPassagesTag;

        // Merchant Guild negative tags

        // Social Faux Pas - Triggered by Force approach in the refined Guild
        EncounterTag socialFauxPasTag = new EncounterTag(
            "social_faux_pas",
            "Social Faux Pas",
            "Your aggressive manner has offended the merchants. Relationship-focused choices generate no momentum.",
            SignatureElementTypes.Rapport,
            0,
            new TagEffect
            {
                IsNegative = true,
                AffectedFocus = FocusTypes.Relationship,
                BlockMomentum = true
            }
        );

        socialFauxPasTag.ActivationTriggers.Add(new TagTrigger(
            "force_guild_trigger",
            "Using Force in the Merchant Guild is a social blunder",
            ApproachTypes.Force
        ));

        socialFauxPasTag.RemovalTriggers.Add(new TagTrigger(
            "charm_removal",
            "Charm can help recover from social mistakes",
            ApproachTypes.Charm
        ));

        socialFauxPasTag.IsLocationReaction = true;
        _tags[socialFauxPasTag.Id] = socialFauxPasTag;

        // Market Suspicion - Triggered by Stealth approach in the watchful Guild
        EncounterTag marketSuspicionTag = new EncounterTag(
            "market_suspicion",
            "Market Suspicion",
            "The merchants are watching you carefully. All choices generate +1 pressure.",
            SignatureElementTypes.Concealment,
            0,
            new TagEffect
            {
                IsNegative = true,
                PressureModifier = 1
            }
        );

        marketSuspicionTag.ActivationTriggers.Add(new TagTrigger(
            "stealth_guild_trigger",
            "Using Stealth in the Merchant Guild raises suspicion",
            ApproachTypes.Stealth
        ));

        marketSuspicionTag.RemovalTriggers.Add(new TagTrigger(
            "rapport_removal",
            "Building enough Rapport can dispel suspicion",
            null,
            null,
            3,
            SignatureElementTypes.Rapport
        ));

        marketSuspicionTag.IsLocationReaction = true;
        _tags[marketSuspicionTag.Id] = marketSuspicionTag;

        // Bandit Camp negative tags

        // Hostile Territory - Triggered by Charm approach in the distrustful camp
        EncounterTag hostileTerritoryTag = new EncounterTag(
            "hostile_territory",
            "Hostile Territory",
            "Your charming manner is seen as weakness. Relationship-focused choices generate +2 pressure.",
            SignatureElementTypes.Rapport,
            0,
            new TagEffect
            {
                IsNegative = true,
                AffectedFocus = FocusTypes.Relationship,
                PressureModifier = 2
            }
        );

        hostileTerritoryTag.ActivationTriggers.Add(new TagTrigger(
            "charm_bandit_trigger",
            "Using Charm in the Bandit Camp is seen as weakness",
            ApproachTypes.Charm
        ));

        hostileTerritoryTag.RemovalTriggers.Add(new TagTrigger(
            "force_removal",
            "Show of Force can earn respect in the camp",
            ApproachTypes.Force
        ));

        hostileTerritoryTag.IsLocationReaction = true;
        _tags[hostileTerritoryTag.Id] = hostileTerritoryTag;

        // Unstable Ground - Triggered by any environment focus in the chaotic camp
        EncounterTag unstableGroundTag = new EncounterTag(
            "unstable_ground",
            "Unstable Ground",
            "The chaotic environment works against you. Environment-focused choices generate no momentum.",
            SignatureElementTypes.Precision,
            0,
            new TagEffect
            {
                IsNegative = true,
                AffectedFocus = FocusTypes.Environment,
                BlockMomentum = true
            }
        );

        unstableGroundTag.ActivationTriggers.Add(new TagTrigger(
            "environment_bandit_trigger",
            "Focusing on the Environment in the chaotic camp is difficult",
            null,
            FocusTypes.Environment
        ));

        unstableGroundTag.RemovalTriggers.Add(new TagTrigger(
            "wit_removal",
            "Wit can help make sense of the chaotic environment",
            ApproachTypes.Wit
        ));

        unstableGroundTag.IsLocationReaction = true;
        _tags[unstableGroundTag.Id] = unstableGroundTag;

        // Royal Court negative tags

        // Court Etiquette - Triggered by Force or Stealth approaches in the formal court
        EncounterTag courtEtiquetteTag = new EncounterTag(
            "court_etiquette",
            "Court Etiquette Violation",
            "Your actions have violated court protocol. All choices generate +1 pressure.",
            SignatureElementTypes.Rapport,
            0,
            new TagEffect
            {
                IsNegative = true,
                PressureModifier = 1
            }
        );

        courtEtiquetteTag.ActivationTriggers.Add(new TagTrigger(
            "force_court_trigger",
            "Using Force in court violates etiquette",
            ApproachTypes.Force
        ));

        courtEtiquetteTag.ActivationTriggers.Add(new TagTrigger(
            "stealth_court_trigger",
            "Using Stealth in court violates etiquette",
            ApproachTypes.Stealth
        ));

        courtEtiquetteTag.RemovalTriggers.Add(new TagTrigger(
            "charm_court_removal",
            "Charm can smooth over etiquette violations",
            ApproachTypes.Charm
        ));

        courtEtiquetteTag.IsLocationReaction = true;
        _tags[courtEtiquetteTag.Id] = courtEtiquetteTag;

        // Court Politics - Triggered by focusing too much on Information
        EncounterTag courtPoliticsTag = new EncounterTag(
            "court_politics",
            "Court Politics",
            "Your questions have drawn unwanted attention. Information-focused choices generate double pressure.",
            SignatureElementTypes.Analysis,
            0,
            new TagEffect
            {
                IsNegative = true,
                AffectedFocus = FocusTypes.Information,
                DoublePressure = true
            }
        );

        courtPoliticsTag.ActivationTriggers.Add(new TagTrigger(
            "information_court_trigger",
            "Seeking too much information draws political attention",
            null,
            FocusTypes.Information,
            2, // Requires at least 2 uses
            null,
            true // Cumulative effect
        ));

        courtPoliticsTag.RemovalTriggers.Add(new TagTrigger(
            "relationship_court_removal",
            "Building relationships can deflect political attention",
            null,
            FocusTypes.Relationship
        ));

        courtPoliticsTag.IsLocationReaction = true;
        _tags[courtPoliticsTag.Id] = courtPoliticsTag;
    }

    private void AddTag(string id, string name, string description, SignatureElementTypes sourceElement,
                       int thresholdValue, TagEffect effect)
    {
        _tags[id] = new EncounterTag(id, name, description, sourceElement, thresholdValue, effect);
    }
}