/// <summary>
/// Central registry of all tag identifiers to ensure type safety
/// </summary>
public static class TagRegistry
{
    // Dominance (Force) Tags
    public static class Dominance
    {
        // Level 3
        public const string IntimidationTactics = "intimidation_tactics";
        public const string ForcefulAuthority = "forceful_authority";
        public const string DirectApproach = "direct_approach";

        // Level 5
        public const string OverwhelmingPresence = "overwhelming_presence";
        public const string CommandingVoice = "commanding_voice";
        public const string ForcefulBreakthrough = "forceful_breakthrough";

        // Collection for easy access
        public static readonly string[] AllTags = new[]
        {
            IntimidationTactics, ForcefulAuthority, DirectApproach,
            OverwhelmingPresence, CommandingVoice, ForcefulBreakthrough
        };
    }

    // Rapport (Charm) Tags
    public static class Rapport
    {
        // Level 3
        public const string SocialCurrency = "social_currency";
        public const string EmotionalInsight = "emotional_insight";
        public const string SmoothTalker = "smooth_talker";

        // Level 5
        public const string NetworkLeverage = "network_leverage";
        public const string CaptivatingPresence = "captivating_presence";
        public const string CharmingNegotiator = "charming_negotiator";

        // Collection for easy access
        public static readonly string[] AllTags = new[]
        {
            SocialCurrency, EmotionalInsight, SmoothTalker,
            NetworkLeverage, CaptivatingPresence, CharmingNegotiator
        };
    }

    // Analysis (Wit) Tags
    public static class Analysis
    {
        // Level 3
        public const string MarketInsight = "market_insight";
        public const string StrategicPlanning = "strategic_planning";
        public const string ResourceOptimization = "resource_optimization";

        // Level 5
        public const string NegotiationMastery = "negotiation_mastery";
        public const string TacticalAdvantage = "tactical_advantage";
        public const string ComprehensiveUnderstanding = "comprehensive_understanding";

        // Collection for easy access
        public static readonly string[] AllTags = new[]
        {
            MarketInsight, StrategicPlanning, ResourceOptimization,
            NegotiationMastery, TacticalAdvantage, ComprehensiveUnderstanding
        };
    }

    // Precision (Finesse) Tags
    public static class Precision
    {
        // Level 3
        public const string PrecisionFootwork = "precision_footwork";
        public const string EfficientMovement = "efficient_movement";
        public const string CarefulAllocation = "careful_allocation";

        // Level 5
        public const string FlawlessExecution = "flawless_execution";
        public const string PerfectTechnique = "perfect_technique";
        public const string MeticulousApproach = "meticulous_approach";

        // Collection for easy access
        public static readonly string[] AllTags = new[]
        {
            PrecisionFootwork, EfficientMovement, CarefulAllocation,
            FlawlessExecution, PerfectTechnique, MeticulousApproach
        };
    }

    // Concealment (Stealth) Tags
    public static class Concealment
    {
        // Level 3
        public const string ShadowMovement = "shadow_movement";
        public const string UnseenThreat = "unseen_threat";
        public const string HiddenObservation = "hidden_observation";

        // Level 5
        public const string PerfectStealth = "perfect_stealth";
        public const string PerfectAmbush = "perfect_ambush";
        public const string Vanish = "vanish";

        // Collection for easy access
        public static readonly string[] AllTags = new[]
        {
            ShadowMovement, UnseenThreat, HiddenObservation,
            PerfectStealth, PerfectAmbush, Vanish
        };
    }

    // Location Reaction Tags
    public static class LocationReaction
    {
        // Merchant Guild
        public const string SocialFauxPas = "social_faux_pas";
        public const string MarketSuspicion = "market_suspicion";

        // Bandit Camp
        public const string HostileTerritory = "hostile_territory";
        public const string UnstableGround = "unstable_ground";

        // Collections by location
        public static readonly string[] MerchantGuild = new[] { SocialFauxPas, MarketSuspicion };
        public static readonly string[] BanditCamp = new[] { HostileTerritory, UnstableGround };
    }
}
