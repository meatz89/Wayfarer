/// <summary>
/// Factory class for creating all encounter tag content
/// </summary>
public static class TagContentFactory
{
    /// <summary>
    /// Creates all tags and adds them to the provided repository
    /// </summary>
    public static void CreateAllTags(Dictionary<string, EncounterTag> tagDictionary)
    {
        CreateDominanceTags(tagDictionary);
        CreateRapportTags(tagDictionary);
        CreateAnalysisTags(tagDictionary);
        CreatePrecisionTags(tagDictionary);
        CreateConcealmentTags(tagDictionary);
        CreateMerchantGuildReactionTags(tagDictionary);
        CreateBanditCampReactionTags(tagDictionary);
    }

    #region Dominance Tags

    private static void CreateDominanceTags(Dictionary<string, EncounterTag> tagDictionary)
    {
        // Level 3: Intimidation Tactics
        var intimidationTactics = new EncounterTagBuilder()
            .WithId(TagRegistry.Dominance.IntimidationTactics)
            .WithName("Dominance Boost: Physical Focus (+1 momentum)")
            .WithDescription("Physical-related choices give +1 additional momentum")
            .WithSourceElement(SignatureElementTypes.Dominance)
            .WithThreshold(3)
            .AffectFocus(FocusTypes.Physical)
            .AddMomentum(1)
            .Build();
        tagDictionary[intimidationTactics.Id] = intimidationTactics;

        // Level 3: Forceful Authority
        var forcefulAuthority = new EncounterTagBuilder()
            .WithId(TagRegistry.Dominance.ForcefulAuthority)
            .WithName("Dominance Shield: Relationship Focus (no pressure)")
            .WithDescription("Relationship-related choices generate no pressure")
            .WithSourceElement(SignatureElementTypes.Dominance)
            .WithThreshold(3)
            .AffectFocus(FocusTypes.Relationship)
            .ZeroPressure()
            .Build();
        tagDictionary[forcefulAuthority.Id] = forcefulAuthority;

        // Level 3: Direct Approach
        var directApproach = new EncounterTagBuilder()
            .WithId(TagRegistry.Dominance.DirectApproach)
            .WithName("Dominance Boost: Environment Focus (+1 momentum)")
            .WithDescription("Environment-related choices give +1 additional momentum")
            .WithSourceElement(SignatureElementTypes.Dominance)
            .WithThreshold(3)
            .AffectFocus(FocusTypes.Environment)
            .AddMomentum(1)
            .Build();
        tagDictionary[directApproach.Id] = directApproach;

        // Level 5: Overwhelming Presence
        var overwhelmingPresence = new EncounterTagBuilder()
            .WithId(TagRegistry.Dominance.OverwhelmingPresence)
            .WithName("Dominance Mastery: Pressure Conversion")
            .WithDescription("Convert all pressure to momentum")
            .WithSourceElement(SignatureElementTypes.Dominance)
            .WithThreshold(5)
            .WithSpecialEffect("convert_pressure_to_momentum")
            .Build();
        tagDictionary[overwhelmingPresence.Id] = overwhelmingPresence;

        // Level 5: Commanding Voice
        var commandingVoice = new EncounterTagBuilder()
            .WithId(TagRegistry.Dominance.CommandingVoice)
            .WithName("Dominance Amplifier: Relationship Focus (2x momentum)")
            .WithDescription("Double momentum from relationship-related choices")
            .WithSourceElement(SignatureElementTypes.Dominance)
            .WithThreshold(5)
            .AffectFocus(FocusTypes.Relationship)
            .DoubleMomentum()
            .Build();
        tagDictionary[commandingVoice.Id] = commandingVoice;

        // Level 5: Forceful Breakthrough
        var forcefulBreakthrough = new EncounterTagBuilder()
            .WithId(TagRegistry.Dominance.ForcefulBreakthrough)
            .WithName("Dominance Immunity: Physical Focus (ignore pressure)")
            .WithDescription("Ignore pressure from physical-related choices")
            .WithSourceElement(SignatureElementTypes.Dominance)
            .WithThreshold(5)
            .AffectFocus(FocusTypes.Physical)
            .ZeroPressure()
            .Build();
        tagDictionary[forcefulBreakthrough.Id] = forcefulBreakthrough;
    }

    #endregion

    #region Rapport Tags

    private static void CreateRapportTags(Dictionary<string, EncounterTag> tagDictionary)
    {
        // Level 3: Social Currency
        var socialCurrency = new EncounterTagBuilder()
            .WithId(TagRegistry.Rapport.SocialCurrency)
            .WithName("Rapport Boost: Resource Focus (+1 momentum)")
            .WithDescription("Resource-related choices give +1 additional momentum")
            .WithSourceElement(SignatureElementTypes.Rapport)
            .WithThreshold(3)
            .AffectFocus(FocusTypes.Resource)
            .AddMomentum(1)
            .Build();
        tagDictionary[socialCurrency.Id] = socialCurrency;

        // Level 3: Emotional Insight
        var emotionalInsight = new EncounterTagBuilder()
            .WithId(TagRegistry.Rapport.EmotionalInsight)
            .WithName("Rapport Boost: Relationship Focus (+1 momentum)")
            .WithDescription("Relationship-related choices give +1 additional momentum")
            .WithSourceElement(SignatureElementTypes.Rapport)
            .WithThreshold(3)
            .AffectFocus(FocusTypes.Relationship)
            .AddMomentum(1)
            .Build();
        tagDictionary[emotionalInsight.Id] = emotionalInsight;

        // Level 3: Smooth Talker
        var smoothTalker = new EncounterTagBuilder()
            .WithId(TagRegistry.Rapport.SmoothTalker)
            .WithName("Rapport Shield: Information Focus (no pressure)")
            .WithDescription("Information-related choices generate no pressure")
            .WithSourceElement(SignatureElementTypes.Rapport)
            .WithThreshold(3)
            .AffectFocus(FocusTypes.Information)
            .ZeroPressure()
            .Build();
        tagDictionary[smoothTalker.Id] = smoothTalker;

        // Level 5: Network Leverage
        var networkLeverage = new EncounterTagBuilder()
            .WithId(TagRegistry.Rapport.NetworkLeverage)
            .WithName("Rapport Mastery: Pressure Reduction (-1 per turn)")
            .WithDescription("Reduce pressure by 1 at end of each turn")
            .WithSourceElement(SignatureElementTypes.Rapport)
            .WithThreshold(5)
            .WithSpecialEffect("reduce_pressure_each_turn")
            .ReducePressure(1)
            .Build();
        tagDictionary[networkLeverage.Id] = networkLeverage;

        // Level 5: Captivating Presence
        var captivatingPresence = new EncounterTagBuilder()
            .WithId(TagRegistry.Rapport.CaptivatingPresence)
            .WithName("Rapport Amplifier: Information Focus (2x momentum)")
            .WithDescription("Double momentum from information-related choices")
            .WithSourceElement(SignatureElementTypes.Rapport)
            .WithThreshold(5)
            .AffectFocus(FocusTypes.Information)
            .DoubleMomentum()
            .Build();
        tagDictionary[captivatingPresence.Id] = captivatingPresence;

        // Level 5: Charming Negotiator
        var charmingNegotiator = new EncounterTagBuilder()
            .WithId(TagRegistry.Rapport.CharmingNegotiator)
            .WithName("Rapport Shield: Resource Focus (no pressure)")
            .WithDescription("Resource-related choices generate no pressure")
            .WithSourceElement(SignatureElementTypes.Rapport)
            .WithThreshold(5)
            .AffectFocus(FocusTypes.Resource)
            .ZeroPressure()
            .Build();
        tagDictionary[charmingNegotiator.Id] = charmingNegotiator;
    }

    #endregion

    #region Analysis Tags

    private static void CreateAnalysisTags(Dictionary<string, EncounterTag> tagDictionary)
    {
        // Level 3: Market Insight
        var marketInsight = new EncounterTagBuilder()
            .WithId(TagRegistry.Analysis.MarketInsight)
            .WithName("Analysis Shield: Information Focus (no pressure)")
            .WithDescription("Information-related choices generate no pressure")
            .WithSourceElement(SignatureElementTypes.Analysis)
            .WithThreshold(3)
            .AffectFocus(FocusTypes.Information)
            .ZeroPressure()
            .Build();
        tagDictionary[marketInsight.Id] = marketInsight;

        // Level 3: Strategic Planning
        var strategicPlanning = new EncounterTagBuilder()
            .WithId(TagRegistry.Analysis.StrategicPlanning)
            .WithName("Analysis Boost: Physical Focus (+1 momentum)")
            .WithDescription("Physical-related choices give +1 additional momentum")
            .WithSourceElement(SignatureElementTypes.Analysis)
            .WithThreshold(3)
            .AffectFocus(FocusTypes.Physical)
            .AddMomentum(1)
            .Build();
        tagDictionary[strategicPlanning.Id] = strategicPlanning;

        // Level 3: Resource Optimization
        var resourceOptimization = new EncounterTagBuilder()
            .WithId(TagRegistry.Analysis.ResourceOptimization)
            .WithName("Analysis Shield: Resource Focus (no pressure)")
            .WithDescription("Resource-related choices generate no pressure")
            .WithSourceElement(SignatureElementTypes.Analysis)
            .WithThreshold(3)
            .AffectFocus(FocusTypes.Resource)
            .ZeroPressure()
            .Build();
        tagDictionary[resourceOptimization.Id] = resourceOptimization;

        // Level 5: Negotiation Mastery
        var negotiationMastery = new EncounterTagBuilder()
            .WithId(TagRegistry.Analysis.NegotiationMastery)
            .WithName("Analysis Amplifier: Resource Focus (2x momentum)")
            .WithDescription("Double momentum from resource-related choices")
            .WithSourceElement(SignatureElementTypes.Analysis)
            .WithThreshold(5)
            .AffectFocus(FocusTypes.Resource)
            .DoubleMomentum()
            .Build();
        tagDictionary[negotiationMastery.Id] = negotiationMastery;

        // Level 5: Tactical Advantage
        var tacticalAdvantage = new EncounterTagBuilder()
            .WithId(TagRegistry.Analysis.TacticalAdvantage)
            .WithName("Analysis Amplifier: Physical Focus (2x momentum)")
            .WithDescription("Double momentum from physical-related choices")
            .WithSourceElement(SignatureElementTypes.Analysis)
            .WithThreshold(5)
            .AffectFocus(FocusTypes.Physical)
            .DoubleMomentum()
            .Build();
        tagDictionary[tacticalAdvantage.Id] = tacticalAdvantage;

        // Level 5: Comprehensive Understanding
        var comprehensiveUnderstanding = new EncounterTagBuilder()
            .WithId(TagRegistry.Analysis.ComprehensiveUnderstanding)
            .WithName("Analysis Boost: Information Focus (+2 momentum)")
            .WithDescription("Information choices give +2 additional momentum")
            .WithSourceElement(SignatureElementTypes.Analysis)
            .WithThreshold(5)
            .AffectFocus(FocusTypes.Information)
            .AddMomentum(2)
            .Build();
        tagDictionary[comprehensiveUnderstanding.Id] = comprehensiveUnderstanding;
    }

    #endregion

    #region Precision Tags

    private static void CreatePrecisionTags(Dictionary<string, EncounterTag> tagDictionary)
    {
        // Level 3: Precision Footwork
        var precisionFootwork = new EncounterTagBuilder()
            .WithId(TagRegistry.Precision.PrecisionFootwork)
            .WithName("Precision Reducer: Environment Focus (-1 pressure)")
            .WithDescription("Ignore 1 pressure from environment-related choices")
            .WithSourceElement(SignatureElementTypes.Precision)
            .WithThreshold(3)
            .AffectFocus(FocusTypes.Environment)
            .ReducePressure(1)
            .Build();
        tagDictionary[precisionFootwork.Id] = precisionFootwork;

        // Level 3: Efficient Movement
        var efficientMovement = new EncounterTagBuilder()
            .WithId(TagRegistry.Precision.EfficientMovement)
            .WithName("Precision Shield: Physical Focus (no pressure)")
            .WithDescription("Physical-related choices generate no pressure")
            .WithSourceElement(SignatureElementTypes.Precision)
            .WithThreshold(3)
            .AffectFocus(FocusTypes.Physical)
            .ZeroPressure()
            .Build();
        tagDictionary[efficientMovement.Id] = efficientMovement;

        // Level 3: Careful Allocation
        var carefulAllocation = new EncounterTagBuilder()
            .WithId(TagRegistry.Precision.CarefulAllocation)
            .WithName("Precision Boost: Resource Focus (+1 momentum)")
            .WithDescription("Resource-related choices give +1 additional momentum")
            .WithSourceElement(SignatureElementTypes.Precision)
            .WithThreshold(3)
            .AffectFocus(FocusTypes.Resource)
            .AddMomentum(1)
            .Build();
        tagDictionary[carefulAllocation.Id] = carefulAllocation;

        // Level 5: Flawless Execution
        var flawlessExecution = new EncounterTagBuilder()
            .WithId(TagRegistry.Precision.FlawlessExecution)
            .WithName("Precision Mastery: Pressure Reduction (-1 per turn)")
            .WithDescription("Reduce all pressure by 1 at the end of each turn")
            .WithSourceElement(SignatureElementTypes.Precision)
            .WithThreshold(5)
            .WithSpecialEffect("reduce_pressure_each_turn")
            .ReducePressure(1)
            .Build();
        tagDictionary[flawlessExecution.Id] = flawlessExecution;

        // Level 5: Perfect Technique
        var perfectTechnique = new EncounterTagBuilder()
            .WithId(TagRegistry.Precision.PerfectTechnique)
            .WithName("Precision Amplifier: Physical Focus (2x momentum)")
            .WithDescription("Double momentum from physical-related choices")
            .WithSourceElement(SignatureElementTypes.Precision)
            .WithThreshold(5)
            .AffectFocus(FocusTypes.Physical)
            .DoubleMomentum()
            .Build();
        tagDictionary[perfectTechnique.Id] = perfectTechnique;

        // Level 5: Meticulous Approach
        var meticulousApproach = new EncounterTagBuilder()
            .WithId(TagRegistry.Precision.MeticulousApproach)
            .WithName("Precision Mastery: Universal Boost (+1 momentum)")
            .WithDescription("All choices generate +1 additional momentum")
            .WithSourceElement(SignatureElementTypes.Precision)
            .WithThreshold(5)
            .AddMomentum(1)
            .Build();
        tagDictionary[meticulousApproach.Id] = meticulousApproach;
    }

    #endregion

    #region Concealment Tags

    private static void CreateConcealmentTags(Dictionary<string, EncounterTag> tagDictionary)
    {
        // Level 3: Shadow Movement
        var shadowMovement = new EncounterTagBuilder()
            .WithId(TagRegistry.Concealment.ShadowMovement)
            .WithName("Concealment Boost: Stealth Actions (+1 momentum)")
            .WithDescription("+1 additional momentum for stealth choices")
            .WithSourceElement(SignatureElementTypes.Concealment)
            .WithThreshold(3)
            .AffectApproach(ApproachTypes.Stealth)
            .AddMomentum(1)
            .Build();
        tagDictionary[shadowMovement.Id] = shadowMovement;

        // Level 3: Unseen Threat
        var unseenThreat = new EncounterTagBuilder()
            .WithId(TagRegistry.Concealment.UnseenThreat)
            .WithName("Concealment Mastery: Extra Turn (no pressure)")
            .WithDescription("Take an additional turn with no pressure")
            .WithSourceElement(SignatureElementTypes.Concealment)
            .WithThreshold(3)
            .WithSpecialEffect("additional_turn_no_pressure")
            .ZeroPressure()
            .Build();
        tagDictionary[unseenThreat.Id] = unseenThreat;

        // Level 3: Hidden Observation
        var hiddenObservation = new EncounterTagBuilder()
            .WithId(TagRegistry.Concealment.HiddenObservation)
            .WithName("Concealment Boost: Information Focus (+1 momentum)")
            .WithDescription("Information-related choices give +1 additional momentum")
            .WithSourceElement(SignatureElementTypes.Concealment)
            .WithThreshold(3)
            .AffectFocus(FocusTypes.Information)
            .AddMomentum(1)
            .Build();
        tagDictionary[hiddenObservation.Id] = hiddenObservation;

        // Level 5: Perfect Stealth
        var perfectStealth = new EncounterTagBuilder()
            .WithId(TagRegistry.Concealment.PerfectStealth)
            .WithName("Concealment Immunity: Physical Focus (ignore pressure)")
            .WithDescription("Ignore all pressure from physical-related choices")
            .WithSourceElement(SignatureElementTypes.Concealment)
            .WithThreshold(5)
            .AffectFocus(FocusTypes.Physical)
            .ZeroPressure()
            .Build();
        tagDictionary[perfectStealth.Id] = perfectStealth;

        // Level 5: Perfect Ambush
        var perfectAmbush = new EncounterTagBuilder()
            .WithId(TagRegistry.Concealment.PerfectAmbush)
            .WithName("Concealment Mastery: Auto-Success (max momentum)")
            .WithDescription("Gain maximum momentum (encounter auto-success)")
            .WithSourceElement(SignatureElementTypes.Concealment)
            .WithThreshold(5)
            .WithSpecialEffect("encounter_auto_success")
            .AddMomentum(15) // High value to ensure success
            .Build();
        tagDictionary[perfectAmbush.Id] = perfectAmbush;

        // Level 5: Vanish
        var vanish = new EncounterTagBuilder()
            .WithId(TagRegistry.Concealment.Vanish)
            .WithName("Concealment Mastery: Pressure Immunity (one turn)")
            .WithDescription("Ignore all pressure for one turn")
            .WithSourceElement(SignatureElementTypes.Concealment)
            .WithThreshold(5)
            .ZeroPressure()
            .Build();
        tagDictionary[vanish.Id] = vanish;
    }

    #endregion

    #region Location Reaction Tags

    private static void CreateMerchantGuildReactionTags(Dictionary<string, EncounterTag> tagDictionary)
    {
        // Social Faux Pas
        var socialFauxPas = new EncounterTagBuilder()
            .WithId(TagRegistry.LocationReaction.SocialFauxPas)
            .WithName("Momentum Block: Relationship Focus (0 momentum)")
            .WithDescription("Your aggressive manner has offended the merchants. Relationship-focused choices generate no momentum.")
            .WithSourceElement(SignatureElementTypes.Rapport)
            .AsLocationReaction()
            .AsNegative()
            .AffectFocus(FocusTypes.Relationship)
            .BlockMomentum()
            .ActivateOnApproach(
                "force_guild_trigger",
                "Using Force in the Merchant Guild is a social blunder",
                ApproachTypes.Force
            )
            .RemoveOnApproach(
                "charm_removal",
                "Charm can help recover from social mistakes",
                ApproachTypes.Charm
            )
            .Build();
        tagDictionary[socialFauxPas.Id] = socialFauxPas;

        // Market Suspicion
        var marketSuspicion = new EncounterTagBuilder()
            .WithId(TagRegistry.LocationReaction.MarketSuspicion)
            .WithName("Pressure Increase: All Actions (+1 pressure)")
            .WithDescription("The merchants are watching you carefully. All choices generate +1 pressure.")
            .WithSourceElement(SignatureElementTypes.Concealment)
            .AsLocationReaction()
            .AsNegative()
            .AddPressure(1)
            .ActivateOnApproach(
                "stealth_guild_trigger",
                "Using Stealth in the Merchant Guild raises suspicion",
                ApproachTypes.Stealth
            )
            .RemoveOnSignature(
                "rapport_removal",
                "Building enough Rapport can dispel suspicion",
                3,
                SignatureElementTypes.Rapport
            )
            .Build();
        tagDictionary[marketSuspicion.Id] = marketSuspicion;
    }

    private static void CreateBanditCampReactionTags(Dictionary<string, EncounterTag> tagDictionary)
    {
        // Hostile Territory
        var hostileTerritory = new EncounterTagBuilder()
            .WithId(TagRegistry.LocationReaction.HostileTerritory)
            .WithName("Pressure Magnifier: Relationship Focus (+2 pressure)")
            .WithDescription("Your charming manner is seen as weakness. Relationship-focused choices generate +2 pressure.")
            .WithSourceElement(SignatureElementTypes.Rapport)
            .AsLocationReaction()
            .AsNegative()
            .AffectFocus(FocusTypes.Relationship)
            .AddPressure(2)
            .ActivateOnApproach(
                "charm_bandit_trigger",
                "Using Charm in the Bandit Camp is seen as weakness",
                ApproachTypes.Charm
            )
            .RemoveOnApproach(
                "force_removal",
                "Show of Force can earn respect in the camp",
                ApproachTypes.Force
            )
            .Build();
        tagDictionary[hostileTerritory.Id] = hostileTerritory;

        // Unstable Ground
        var unstableGround = new EncounterTagBuilder()
            .WithId(TagRegistry.LocationReaction.UnstableGround)
            .WithName("Momentum Block: Environment Focus (0 momentum)")
            .WithDescription("The chaotic environment works against you. Environment-focused choices generate no momentum.")
            .WithSourceElement(SignatureElementTypes.Precision)
            .AsLocationReaction()
            .AsNegative()
            .AffectFocus(FocusTypes.Environment)
            .BlockMomentum()
            .ActivateOnFocus(
                "environment_bandit_trigger",
                "Focusing on the Environment in the chaotic camp is difficult",
                FocusTypes.Environment
            )
            .RemoveOnApproach(
                "wit_removal",
                "Wit can help make sense of the chaotic environment",
                ApproachTypes.Wit
            )
            .Build();
        tagDictionary[unstableGround.Id] = unstableGround;
    }

    #endregion
}