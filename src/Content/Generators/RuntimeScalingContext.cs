/// <summary>
/// Runtime scaling adjustments derived from entities at query-time.
/// Two-phase scaling model (arc42 §8.26):
/// 1. Parse-time: Catalogue generates rhythm structure + tier-based values
/// 2. Query-time: Entity-derived adjustments applied from RuntimeScalingContext
///
/// This separation enables:
/// - AI generates scene structures with rhythm-aware choices (parse-time)
/// - Entity context scales difficulty based on current relationships (query-time)
/// - Player sees adjusted requirements reflecting their standing with NPCs/locations
///
/// HIGHLANDER: All entity-derived scaling in one place.
/// DDR-007: Integer adjustments only (no multipliers).
/// </summary>
public class RuntimeScalingContext
{
    /// <summary>
    /// Stat requirement adjustment based on NPC demeanor.
    /// Friendly NPCs have lower requirements, hostile NPCs higher.
    /// Applied to all stat thresholds (Insight, Authority, Rapport, Diplomacy, Cunning).
    /// </summary>
    public int StatRequirementAdjustment { get; init; }

    /// <summary>
    /// Coin cost adjustment based on location quality.
    /// Basic locations are cheaper, premium locations more expensive.
    /// Applied to Coins costs in Consequence.
    /// </summary>
    public int CoinCostAdjustment { get; init; }

    /// <summary>
    /// Resolve cost adjustment based on power dynamic.
    /// When player is dominant, resolve costs are lower.
    /// Applied to Resolve costs in Consequence.
    /// </summary>
    public int ResolveCostAdjustment { get; init; }

    /// <summary>
    /// Time cost adjustment based on urgency.
    /// Urgent situations have lower time availability.
    /// Applied to TimeRequired in LocationAction.
    /// </summary>
    public int TimeCostAdjustment { get; init; }

    /// <summary>
    /// Create no-op scaling context (no adjustments).
    /// Used when no entities are available for derivation.
    /// </summary>
    public static RuntimeScalingContext None()
    {
        return new RuntimeScalingContext
        {
            StatRequirementAdjustment = 0,
            CoinCostAdjustment = 0,
            ResolveCostAdjustment = 0,
            TimeCostAdjustment = 0
        };
    }

    /// <summary>
    /// Derive scaling context from entities at query-time.
    /// Called when creating LocationAction from ChoiceTemplate.
    ///
    /// Scaling formulas (DDR-007: integer adjustments only):
    /// - StatRequirement: Hostile +2, Neutral +0, Friendly -2
    /// - CoinCost: Basic -3, Standard +0, Premium +5, Luxury +10
    /// - ResolveCost: Dominant -1, Equal +0, Submissive +1
    /// - TimeCost: Leisurely +0, Urgent +1
    ///
    /// See arc42/08_crosscutting_concepts.md §8.26 (Sir Brante Rhythm Pattern)
    /// </summary>
    public static RuntimeScalingContext FromEntities(NPC npc, Location location, Player player)
    {
        return new RuntimeScalingContext
        {
            StatRequirementAdjustment = DeriveStatAdjustment(npc),
            CoinCostAdjustment = DeriveCoinAdjustment(location),
            ResolveCostAdjustment = DeriveResolveAdjustment(npc, player),
            TimeCostAdjustment = DeriveTimeAdjustment(location, player)
        };
    }

    /// <summary>
    /// Derive stat requirement adjustment from NPC demeanor.
    /// Sir Brante pattern: friendly NPCs make skill checks easier.
    /// </summary>
    private static int DeriveStatAdjustment(NPC npc)
    {
        if (npc == null) return 0;

        // Derive demeanor from relationship flow
        NPCDemeanor demeanor = npc.RelationshipFlow switch
        {
            <= 9 => NPCDemeanor.Hostile,   // DISCONNECTED/GUARDED
            <= 14 => NPCDemeanor.Neutral,  // NEUTRAL
            _ => NPCDemeanor.Friendly      // RECEPTIVE/TRUSTING
        };

        return demeanor switch
        {
            NPCDemeanor.Hostile => 2,   // Harder when NPC hostile
            NPCDemeanor.Neutral => 0,   // Base difficulty
            NPCDemeanor.Friendly => -2, // Easier when NPC friendly
            _ => 0
        };
    }

    /// <summary>
    /// Derive coin cost adjustment from location quality.
    /// Higher quality locations charge more for services.
    /// </summary>
    private static int DeriveCoinAdjustment(Location location)
    {
        if (location == null) return 0;

        // Derive quality from location tier
        Quality quality = location.Tier switch
        {
            1 => Quality.Basic,
            2 => Quality.Standard,
            3 => Quality.Premium,
            >= 4 => Quality.Luxury,
            _ => Quality.Standard
        };

        return quality switch
        {
            Quality.Basic => -3,    // Cheaper at basic locations
            Quality.Standard => 0,  // Base cost
            Quality.Premium => 5,   // More expensive at premium
            Quality.Luxury => 10,   // Most expensive at luxury
            _ => 0
        };
    }

    /// <summary>
    /// Derive resolve cost adjustment from power dynamic.
    /// When player is dominant, spending resolve is less taxing.
    /// </summary>
    private static int DeriveResolveAdjustment(NPC npc, Player player)
    {
        if (npc == null) return 0;

        // Derive power dynamic from NPC tier
        PowerDynamic power = npc.Tier switch
        {
            >= 4 => PowerDynamic.Submissive,  // High tier NPC = player submissive
            <= 2 => PowerDynamic.Dominant,    // Low tier NPC = player dominant
            _ => PowerDynamic.Equal           // Mid tier = equal footing
        };

        return power switch
        {
            PowerDynamic.Dominant => -1,   // Less resolve cost when dominant
            PowerDynamic.Equal => 0,       // Base resolve cost
            PowerDynamic.Submissive => 1,  // More resolve cost when submissive
            _ => 0
        };
    }

    /// <summary>
    /// Derive time cost adjustment from urgency.
    /// Urgent situations have time pressure.
    /// </summary>
    private static int DeriveTimeAdjustment(Location location, Player player)
    {
        if (location == null) return 0;

        // Urgent locations (Governance, Civic) add time pressure
        if (location.Purpose == LocationPurpose.Governance ||
            location.Purpose == LocationPurpose.Civic)
        {
            return 1;  // Additional time segment cost
        }

        return 0;  // No time pressure
    }

    /// <summary>
    /// Apply stat requirement adjustment to a base value.
    /// Ensures result is never negative.
    /// </summary>
    public int ApplyStatAdjustment(int baseValue)
    {
        int adjusted = baseValue + StatRequirementAdjustment;
        return adjusted < 0 ? 0 : adjusted;
    }

    /// <summary>
    /// Apply coin cost adjustment to a base value.
    /// Ensures result is never negative.
    /// </summary>
    public int ApplyCoinAdjustment(int baseValue)
    {
        int adjusted = baseValue + CoinCostAdjustment;
        return adjusted < 0 ? 0 : adjusted;
    }

    /// <summary>
    /// Apply resolve cost adjustment to a base value.
    /// Ensures result is never negative.
    /// </summary>
    public int ApplyResolveAdjustment(int baseValue)
    {
        int adjusted = baseValue + ResolveCostAdjustment;
        return adjusted < 0 ? 0 : adjusted;
    }

    /// <summary>
    /// Create scaled CompoundRequirement by applying adjustments to stat thresholds.
    /// Returns new instance - does NOT mutate original.
    /// </summary>
    public CompoundRequirement ApplyToRequirement(CompoundRequirement original)
    {
        if (original == null) return null;

        CompoundRequirement scaled = new CompoundRequirement
        {
            OrPaths = new List<OrPath>()
        };

        foreach (OrPath originalPath in original.OrPaths)
        {
            OrPath scaledPath = new OrPath
            {
                Label = originalPath.Label,

                // Apply stat adjustments
                InsightRequired = originalPath.InsightRequired.HasValue
                    ? ApplyStatAdjustment(originalPath.InsightRequired.Value)
                    : null,
                RapportRequired = originalPath.RapportRequired.HasValue
                    ? ApplyStatAdjustment(originalPath.RapportRequired.Value)
                    : null,
                AuthorityRequired = originalPath.AuthorityRequired.HasValue
                    ? ApplyStatAdjustment(originalPath.AuthorityRequired.Value)
                    : null,
                DiplomacyRequired = originalPath.DiplomacyRequired.HasValue
                    ? ApplyStatAdjustment(originalPath.DiplomacyRequired.Value)
                    : null,
                CunningRequired = originalPath.CunningRequired.HasValue
                    ? ApplyStatAdjustment(originalPath.CunningRequired.Value)
                    : null,

                // Apply resolve adjustment
                ResolveRequired = originalPath.ResolveRequired.HasValue
                    ? ApplyResolveAdjustment(originalPath.ResolveRequired.Value)
                    : null,

                // Apply coin adjustment
                CoinsRequired = originalPath.CoinsRequired.HasValue
                    ? ApplyCoinAdjustment(originalPath.CoinsRequired.Value)
                    : null,

                // Copy unchanged requirements
                HealthRequired = originalPath.HealthRequired,
                StaminaRequired = originalPath.StaminaRequired,
                FocusRequired = originalPath.FocusRequired,
                HungerCapacityRequired = originalPath.HungerCapacityRequired,
                SituationCountRequired = originalPath.SituationCountRequired,
                BondNpc = originalPath.BondNpc,
                BondStrengthRequired = originalPath.BondStrengthRequired,
                RequiredScaleType = originalPath.RequiredScaleType,
                ScaleValueRequired = originalPath.ScaleValueRequired,
                RequiredAchievement = originalPath.RequiredAchievement,
                RequiredState = originalPath.RequiredState,
                RequiredItem = originalPath.RequiredItem
            };

            scaled.OrPaths.Add(scaledPath);
        }

        return scaled;
    }

    /// <summary>
    /// Create scaled Consequence by applying cost adjustments.
    /// Returns new instance - does NOT mutate original.
    /// </summary>
    public Consequence ApplyToConsequence(Consequence original)
    {
        if (original == null) return null;

        // Create new consequence with adjusted values
        // Only adjust COSTS (negative values)
        return new Consequence
        {
            // Apply coin adjustment to costs
            Coins = original.Coins < 0
                ? -(ApplyCoinAdjustment(-original.Coins))
                : original.Coins,

            // Apply resolve adjustment to costs
            Resolve = original.Resolve < 0
                ? -(ApplyResolveAdjustment(-original.Resolve))
                : original.Resolve,

            // Copy unchanged values
            Health = original.Health,
            Stamina = original.Stamina,
            Focus = original.Focus,
            Hunger = original.Hunger,

            // Stat rewards unchanged (not scaled at query-time)
            Insight = original.Insight,
            Rapport = original.Rapport,
            Authority = original.Authority,
            Diplomacy = original.Diplomacy,
            Cunning = original.Cunning,

            // Copy non-resource consequences
            BondChanges = original.BondChanges,
            ScaleChanges = original.ScaleChanges,
            StatesToApply = original.StatesToApply,
            StatesToRemove = original.StatesToRemove,
            AchievementsToGrant = original.AchievementsToGrant,
            LocationsToUnlock = original.LocationsToUnlock,
            LocationsToLock = original.LocationsToLock,
            RoutesToUnlock = original.RoutesToUnlock,
            RoutesToLock = original.RoutesToLock,
            ItemsToGrant = original.ItemsToGrant,
            ItemsToRemove = original.ItemsToRemove,
            ScenesToSpawn = original.ScenesToSpawn,
            TimeSegments = original.TimeSegments
        };
    }
}
