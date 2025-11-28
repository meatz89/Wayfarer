/// <summary>
/// Compound Requirement - OR-based unlocking system
/// Multiple paths to unlock the same situation
/// Player needs to satisfy at least ONE complete OR path
/// </summary>
public class CompoundRequirement
{
    /// <summary>
    /// List of OR paths - player needs to satisfy at least ONE complete path
    /// Each path contains multiple AND requirements (all must be met within that path)
    /// </summary>
    public List<OrPath> OrPaths { get; set; } = new List<OrPath>();

    // ============================================
    // FACTORY METHODS
    // ============================================

    /// <summary>
    /// Sir Brante Dual-Nature Rule: Create requirement based on consequence costs.
    /// If consequence costs Resolve (negative value), adds Resolve >= 0 requirement.
    /// This encapsulates the willpower gate pattern - callers don't need to know the rule.
    ///
    /// Pattern: Choices that COST Resolve require Resolve >= 0 to attempt.
    /// This creates meaningful choice through scarcity - players cannot make costly
    /// choices until they've built positive resolve through earlier choices.
    /// </summary>
    public static CompoundRequirement CreateForConsequence(Consequence consequence)
    {
        CompoundRequirement requirement = new CompoundRequirement();

        // Sir Brante dual-nature rule: negative Resolve consequence requires Resolve >= 0
        if (consequence.Resolve < 0)
        {
            requirement.OrPaths.Add(new OrPath
            {
                Label = "Resolve 0+",
                ResolveRequired = 0
            });
        }

        return requirement;
    }

    /// <summary>
    /// Check if any path is satisfied by current game state
    /// Returns true if at least one complete path's requirements are all met
    /// </summary>
    public bool IsAnySatisfied(Player player, GameWorld gameWorld)
    {
        if (OrPaths == null || OrPaths.Count == 0)
            return true; // No requirements means always unlocked

        foreach (OrPath path in OrPaths)
        {
            if (path.IsSatisfied(player, gameWorld))
                return true; // Found a satisfied path
        }

        return false; // No path satisfied
    }

    /// <summary>
    /// Project which paths are satisfied and which are missing.
    /// Returns detailed status for Perfect Information UI display.
    /// </summary>
    public RequirementProjection GetProjection(Player player, GameWorld gameWorld)
    {
        if (OrPaths == null || OrPaths.Count == 0)
        {
            return RequirementProjection.NoRequirements();
        }

        List<PathProjection> paths = new List<PathProjection>();
        bool anyPathSatisfied = false;

        foreach (OrPath path in OrPaths)
        {
            PathProjection pathProjection = path.GetProjection(player, gameWorld);
            paths.Add(pathProjection);
            if (pathProjection.IsSatisfied)
            {
                anyPathSatisfied = true;
            }
        }

        return new RequirementProjection
        {
            HasRequirements = true,
            IsSatisfied = anyPathSatisfied,
            Paths = paths
        };
    }
}

/// <summary>
/// Single OR path - all requirements in this path must be met (AND logic within path)
/// Uses Explicit Property Principle: each requirement type has its own named property
/// instead of generic string-based Type/Context routing.
/// See arc42/08_crosscutting_concepts.md §8.19
/// </summary>
public class OrPath
{
    /// <summary>
    /// Display label for this unlock path (for UI)
    /// Example: "High Bond with Martha", "Complete Investigation", "Achieve Moral Standing"
    /// </summary>
    public string Label { get; set; }

    // ============================================
    // STAT REQUIREMENTS - explicit property per stat
    // ============================================
    public int? InsightRequired { get; set; }
    public int? RapportRequired { get; set; }
    public int? AuthorityRequired { get; set; }
    public int? DiplomacyRequired { get; set; }
    public int? CunningRequired { get; set; }

    // ============================================
    // RESOURCE REQUIREMENTS
    // ============================================
    public int? ResolveRequired { get; set; }
    public int? CoinsRequired { get; set; }

    // ============================================
    // PROGRESSION REQUIREMENTS
    // ============================================
    public int? SituationCountRequired { get; set; }

    // ============================================
    // RELATIONSHIP REQUIREMENTS (NPC object reference, not string)
    // ============================================
    public NPC BondNpc { get; set; }
    public int? BondStrengthRequired { get; set; }

    // ============================================
    // SCALE REQUIREMENTS (ScaleType enum, not string)
    // ============================================
    public ScaleType? RequiredScaleType { get; set; }
    public int? ScaleValueRequired { get; set; }

    // ============================================
    // BOOLEAN REQUIREMENTS (object references, not string IDs)
    // ============================================
    public Achievement RequiredAchievement { get; set; }
    public StateType? RequiredState { get; set; }
    public Item RequiredItem { get; set; }

    /// <summary>
    /// Check if this path is satisfied by current game state
    /// Returns true if ALL requirements in this path are met
    /// </summary>
    public bool IsSatisfied(Player player, GameWorld gameWorld)
    {
        // Check stat requirements
        if (InsightRequired.HasValue && player.Insight < InsightRequired.Value) return false;
        if (RapportRequired.HasValue && player.Rapport < RapportRequired.Value) return false;
        if (AuthorityRequired.HasValue && player.Authority < AuthorityRequired.Value) return false;
        if (DiplomacyRequired.HasValue && player.Diplomacy < DiplomacyRequired.Value) return false;
        if (CunningRequired.HasValue && player.Cunning < CunningRequired.Value) return false;

        // Check resource requirements
        if (ResolveRequired.HasValue && player.Resolve < ResolveRequired.Value) return false;
        if (CoinsRequired.HasValue && player.Coins < CoinsRequired.Value) return false;

        // Check progression
        if (SituationCountRequired.HasValue && player.CompletedSituations.Count < SituationCountRequired.Value) return false;

        // Check relationship (uses object reference, not string ID)
        if (BondNpc != null && BondStrengthRequired.HasValue)
        {
            NPCTokenEntry entry = player.NPCTokens.FirstOrDefault(t => t.Npc == BondNpc);
            int totalBond = entry != null ? entry.Trust + entry.Diplomacy + entry.Status + entry.Shadow : 0;
            if (totalBond < BondStrengthRequired.Value) return false;
        }

        // Check scale (uses ScaleType enum, not string)
        if (RequiredScaleType.HasValue && ScaleValueRequired.HasValue)
        {
            int scaleValue = GetScaleValue(player, RequiredScaleType.Value);
            // Positive threshold: scale >= threshold
            // Negative threshold: scale <= threshold
            if (ScaleValueRequired.Value >= 0 && scaleValue < ScaleValueRequired.Value) return false;
            if (ScaleValueRequired.Value < 0 && scaleValue > ScaleValueRequired.Value) return false;
        }

        // Check boolean requirements (uses object references, not string IDs)
        if (RequiredAchievement != null && !player.EarnedAchievements.Any(a => a.Achievement == RequiredAchievement)) return false;
        if (RequiredState.HasValue && !player.ActiveStates.Any(s => s.Type == RequiredState.Value)) return false;
        if (RequiredItem != null && !player.HasItem(RequiredItem)) return false;

        return true;
    }

    /// <summary>
    /// Project the satisfaction status of each requirement in this path.
    /// Returns detailed status including current values and gaps.
    /// </summary>
    public PathProjection GetProjection(Player player, GameWorld gameWorld)
    {
        List<RequirementStatus> requirements = new List<RequirementStatus>();
        bool allSatisfied = true;

        // Project stat requirements
        if (InsightRequired.HasValue)
        {
            bool satisfied = player.Insight >= InsightRequired.Value;
            requirements.Add(new RequirementStatus
            {
                Label = $"Insight {InsightRequired.Value}+",
                IsSatisfied = satisfied,
                CurrentValue = player.Insight,
                RequiredValue = InsightRequired.Value
            });
            if (!satisfied) allSatisfied = false;
        }

        if (RapportRequired.HasValue)
        {
            bool satisfied = player.Rapport >= RapportRequired.Value;
            requirements.Add(new RequirementStatus
            {
                Label = $"Rapport {RapportRequired.Value}+",
                IsSatisfied = satisfied,
                CurrentValue = player.Rapport,
                RequiredValue = RapportRequired.Value
            });
            if (!satisfied) allSatisfied = false;
        }

        if (AuthorityRequired.HasValue)
        {
            bool satisfied = player.Authority >= AuthorityRequired.Value;
            requirements.Add(new RequirementStatus
            {
                Label = $"Authority {AuthorityRequired.Value}+",
                IsSatisfied = satisfied,
                CurrentValue = player.Authority,
                RequiredValue = AuthorityRequired.Value
            });
            if (!satisfied) allSatisfied = false;
        }

        if (DiplomacyRequired.HasValue)
        {
            bool satisfied = player.Diplomacy >= DiplomacyRequired.Value;
            requirements.Add(new RequirementStatus
            {
                Label = $"Diplomacy {DiplomacyRequired.Value}+",
                IsSatisfied = satisfied,
                CurrentValue = player.Diplomacy,
                RequiredValue = DiplomacyRequired.Value
            });
            if (!satisfied) allSatisfied = false;
        }

        if (CunningRequired.HasValue)
        {
            bool satisfied = player.Cunning >= CunningRequired.Value;
            requirements.Add(new RequirementStatus
            {
                Label = $"Cunning {CunningRequired.Value}+",
                IsSatisfied = satisfied,
                CurrentValue = player.Cunning,
                RequiredValue = CunningRequired.Value
            });
            if (!satisfied) allSatisfied = false;
        }

        // Project resource requirements
        if (ResolveRequired.HasValue)
        {
            bool satisfied = player.Resolve >= ResolveRequired.Value;
            requirements.Add(new RequirementStatus
            {
                Label = $"Resolve {ResolveRequired.Value}+",
                IsSatisfied = satisfied,
                CurrentValue = player.Resolve,
                RequiredValue = ResolveRequired.Value
            });
            if (!satisfied) allSatisfied = false;
        }

        if (CoinsRequired.HasValue)
        {
            bool satisfied = player.Coins >= CoinsRequired.Value;
            requirements.Add(new RequirementStatus
            {
                Label = $"Coins {CoinsRequired.Value}+",
                IsSatisfied = satisfied,
                CurrentValue = player.Coins,
                RequiredValue = CoinsRequired.Value
            });
            if (!satisfied) allSatisfied = false;
        }

        // Project progression requirements
        if (SituationCountRequired.HasValue)
        {
            bool satisfied = player.CompletedSituations.Count >= SituationCountRequired.Value;
            requirements.Add(new RequirementStatus
            {
                Label = $"Complete {SituationCountRequired.Value} situations",
                IsSatisfied = satisfied,
                CurrentValue = player.CompletedSituations.Count,
                RequiredValue = SituationCountRequired.Value
            });
            if (!satisfied) allSatisfied = false;
        }

        // Project relationship requirements
        if (BondNpc != null && BondStrengthRequired.HasValue)
        {
            NPCTokenEntry entry = player.NPCTokens.FirstOrDefault(t => t.Npc == BondNpc);
            int totalBond = entry != null ? entry.Trust + entry.Diplomacy + entry.Status + entry.Shadow : 0;
            bool satisfied = totalBond >= BondStrengthRequired.Value;
            requirements.Add(new RequirementStatus
            {
                Label = $"Bond {BondStrengthRequired.Value}+ with {BondNpc.Name}",
                IsSatisfied = satisfied,
                CurrentValue = totalBond,
                RequiredValue = BondStrengthRequired.Value
            });
            if (!satisfied) allSatisfied = false;
        }

        // Project scale requirements
        if (RequiredScaleType.HasValue && ScaleValueRequired.HasValue)
        {
            int scaleValue = GetScaleValue(player, RequiredScaleType.Value);
            bool satisfied = ScaleValueRequired.Value >= 0
                ? scaleValue >= ScaleValueRequired.Value
                : scaleValue <= ScaleValueRequired.Value;
            string direction = ScaleValueRequired.Value >= 0 ? "+" : "";
            requirements.Add(new RequirementStatus
            {
                Label = $"{RequiredScaleType.Value} {direction}{ScaleValueRequired.Value}",
                IsSatisfied = satisfied,
                CurrentValue = scaleValue,
                RequiredValue = ScaleValueRequired.Value
            });
            if (!satisfied) allSatisfied = false;
        }

        // Project boolean requirements
        if (RequiredAchievement != null)
        {
            bool satisfied = player.EarnedAchievements.Any(a => a.Achievement == RequiredAchievement);
            requirements.Add(new RequirementStatus
            {
                Label = $"Achievement: {RequiredAchievement.Name}",
                IsSatisfied = satisfied,
                CurrentValue = satisfied ? 1 : 0,
                RequiredValue = 1
            });
            if (!satisfied) allSatisfied = false;
        }

        if (RequiredState.HasValue)
        {
            bool satisfied = player.ActiveStates.Any(s => s.Type == RequiredState.Value);
            requirements.Add(new RequirementStatus
            {
                Label = $"State: {RequiredState.Value}",
                IsSatisfied = satisfied,
                CurrentValue = satisfied ? 1 : 0,
                RequiredValue = 1
            });
            if (!satisfied) allSatisfied = false;
        }

        if (RequiredItem != null)
        {
            bool satisfied = player.HasItem(RequiredItem);
            requirements.Add(new RequirementStatus
            {
                Label = $"Item: {RequiredItem.Name}",
                IsSatisfied = satisfied,
                CurrentValue = satisfied ? 1 : 0,
                RequiredValue = 1
            });
            if (!satisfied) allSatisfied = false;
        }

        return new PathProjection
        {
            Label = Label,
            IsSatisfied = allSatisfied,
            Requirements = requirements
        };
    }

    private int GetScaleValue(Player player, ScaleType scaleType)
    {
        return scaleType switch
        {
            ScaleType.Morality => player.Scales.Morality,
            ScaleType.Lawfulness => player.Scales.Lawfulness,
            ScaleType.Method => player.Scales.Method,
            ScaleType.Caution => player.Scales.Caution,
            ScaleType.Transparency => player.Scales.Transparency,
            ScaleType.Fame => player.Scales.Fame,
            _ => 0
        };
    }
}
