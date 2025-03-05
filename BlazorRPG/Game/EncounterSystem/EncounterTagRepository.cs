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

        // Automatically validate the repository during initialization
        bool isValid = TagValidationSystem.ValidateRepository(this);
        if (!isValid)
        {
            // This helps catch missing tags at runtime
            throw new InvalidOperationException("EncounterTagRepository initialization failed: Some tags defined in TagRegistry are missing from the repository. See console for details.");
        }

        // Check for any undefined tags
        TagValidationSystem.CheckForUndefinedTags(this);
    }

    public EncounterTag GetTag(string id)
    {
        return _tags.ContainsKey(id) ? _tags[id] : null;
    }

    // Type-safe tag retrieval methods
    public EncounterTag GetDominanceTag(string dominanceTagId)
    {
        if (!TagRegistry.Dominance.AllTags.Contains(dominanceTagId))
        {
            throw new ArgumentException($"Invalid Dominance tag ID: {dominanceTagId}");
        }
        return GetTag(dominanceTagId);
    }

    public EncounterTag GetRapportTag(string rapportTagId)
    {
        if (!TagRegistry.Rapport.AllTags.Contains(rapportTagId))
        {
            throw new ArgumentException($"Invalid Rapport tag ID: {rapportTagId}");
        }
        return GetTag(rapportTagId);
    }

    public EncounterTag GetAnalysisTag(string analysisTagId)
    {
        if (!TagRegistry.Analysis.AllTags.Contains(analysisTagId))
        {
            throw new ArgumentException($"Invalid Analysis tag ID: {analysisTagId}");
        }
        return GetTag(analysisTagId);
    }

    public EncounterTag GetPrecisionTag(string precisionTagId)
    {
        if (!TagRegistry.Precision.AllTags.Contains(precisionTagId))
        {
            throw new ArgumentException($"Invalid Precision tag ID: {precisionTagId}");
        }
        return GetTag(precisionTagId);
    }

    public EncounterTag GetConcealmentTag(string concealmentTagId)
    {
        if (!TagRegistry.Concealment.AllTags.Contains(concealmentTagId))
        {
            throw new ArgumentException($"Invalid Concealment tag ID: {concealmentTagId}");
        }
        return GetTag(concealmentTagId);
    }

    public EncounterTag GetLocationReactionTag(string locationReactionTagId)
    {
        bool isAnyLocationTag =
            TagRegistry.LocationReaction.MerchantGuild.Contains(locationReactionTagId) ||
            TagRegistry.LocationReaction.BanditCamp.Contains(locationReactionTagId);

        if (!isAnyLocationTag)
        {
            throw new ArgumentException($"Invalid Location Reaction tag ID: {locationReactionTagId}");
        }
        return GetTag(locationReactionTagId);
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
            TagRegistry.Dominance.IntimidationTactics,
            "Dominance Boost: Physical Focus (+1 momentum)",
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
            TagRegistry.Dominance.ForcefulAuthority,
            "Dominance Shield: Relationship Focus (no pressure)",
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
            TagRegistry.Dominance.DirectApproach,
            "Dominance Boost: Environment Focus (+1 momentum)",
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
            TagRegistry.Dominance.OverwhelmingPresence,
            "Dominance Mastery: Pressure Conversion",
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
            TagRegistry.Dominance.CommandingVoice,
            "Dominance Amplifier: Relationship Focus (2x momentum)",
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
            TagRegistry.Dominance.ForcefulBreakthrough,
            "Dominance Immunity: Physical Focus (ignore pressure)",
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
            TagRegistry.Rapport.SocialCurrency,
            "Rapport Boost: Resource Focus (+1 momentum)",
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
            TagRegistry.Rapport.EmotionalInsight,
            "Rapport Boost: Relationship Focus (+1 momentum)",
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
            TagRegistry.Rapport.SmoothTalker,
            "Rapport Shield: Information Focus (no pressure)",
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
            TagRegistry.Rapport.NetworkLeverage,
            "Rapport Mastery: Pressure Reduction (-1 per turn)",
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
            TagRegistry.Rapport.CaptivatingPresence,
            "Rapport Amplifier: Information Focus (2x momentum)",
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
            TagRegistry.Rapport.CharmingNegotiator,
            "Rapport Shield: Resource Focus (no pressure)",
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
            TagRegistry.Analysis.MarketInsight,
            "Analysis Shield: Information Focus (no pressure)",
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
            TagRegistry.Analysis.StrategicPlanning,
            "Analysis Boost: Physical Focus (+1 momentum)",
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
            TagRegistry.Analysis.ResourceOptimization,
            "Analysis Shield: Resource Focus (no pressure)",
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
            TagRegistry.Analysis.NegotiationMastery,
            "Analysis Amplifier: Resource Focus (2x momentum)",
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
            TagRegistry.Analysis.TacticalAdvantage,
            "Analysis Amplifier: Physical Focus (2x momentum)",
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
            TagRegistry.Analysis.ComprehensiveUnderstanding,
            "Analysis Boost: Information Focus (+2 momentum)",
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
            TagRegistry.Precision.PrecisionFootwork,
            "Precision Reducer: Environment Focus (-1 pressure)",
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
            TagRegistry.Precision.EfficientMovement,
            "Precision Shield: Physical Focus (no pressure)",
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
            TagRegistry.Precision.CarefulAllocation,
            "Precision Boost: Resource Focus (+1 momentum)",
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
            TagRegistry.Precision.FlawlessExecution,
            "Precision Mastery: Pressure Reduction (-1 per turn)",
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
            TagRegistry.Precision.PerfectTechnique,
            "Precision Amplifier: Physical Focus (2x momentum)",
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
            TagRegistry.Precision.MeticulousApproach,
            "Precision Mastery: Universal Boost (+1 momentum)",
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
            TagRegistry.Concealment.ShadowMovement,
            "Concealment Boost: Stealth Actions (+1 momentum)",
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
            TagRegistry.Concealment.UnseenThreat,
            "Concealment Mastery: Extra Turn (no pressure)",
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
            TagRegistry.Concealment.HiddenObservation,
            "Concealment Boost: Information Focus (+1 momentum)",
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
            TagRegistry.Concealment.PerfectStealth,
            "Concealment Immunity: Physical Focus (ignore pressure)",
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
            TagRegistry.Concealment.PerfectAmbush,
            "Concealment Mastery: Auto-Success (max momentum)",
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
            TagRegistry.Concealment.Vanish,
            "Concealment Mastery: Pressure Immunity (one turn)",
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
        // Merchant Guild negative tags

        // Social Faux Pas - Triggered by Force approach in the refined Guild
        EncounterTag socialFauxPasTag = new EncounterTag(
            TagRegistry.LocationReaction.SocialFauxPas,
            "Momentum Block: Relationship Focus (0 momentum)",
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
            TagRegistry.LocationReaction.MarketSuspicion,
            "Pressure Increase: All Actions (+1 pressure)",
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
            TagRegistry.LocationReaction.HostileTerritory,
            "Pressure Magnifier: Relationship Focus (+2 pressure)",
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
            TagRegistry.LocationReaction.UnstableGround,
            "Momentum Block: Environment Focus (0 momentum)",
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
    }

    private void AddTag(string id, string name, string description, SignatureElementTypes sourceElement,
                      int thresholdValue, TagEffect effect)
    {
        _tags[id] = new EncounterTag(id, name, description, sourceElement, thresholdValue, effect);
    }
}