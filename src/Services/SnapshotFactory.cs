/// <summary>
/// Factory for creating immutable snapshots of domain entities
/// Captures state at specific point in time for trace debugging
/// All methods are static - no instance needed
/// </summary>
public static class SnapshotFactory
{
    /// <summary>
    /// Create immutable snapshot of Location at current state
    /// </summary>
    public static LocationSnapshot CreateLocationSnapshot(Location location)
    {
        if (location == null) return null;

        return new LocationSnapshot
        {
            Name = location.Name,
            HexPosition = location.HexPosition ?? new AxialCoordinates(0, 0),
            Purpose = location.Purpose,
            Privacy = location.Privacy,
            Safety = location.Safety,
            Activity = location.Activity
        };
    }

    /// <summary>
    /// Create immutable snapshot of NPC at current state
    /// </summary>
    public static NPCSnapshot CreateNPCSnapshot(NPC npc)
    {
        if (npc == null) return null;

        return new NPCSnapshot
        {
            Name = npc.Name,
            Profession = npc.Profession,
            PersonalityType = npc.PersonalityType,
            SocialStanding = npc.SocialStanding,
            StoryRole = npc.StoryRole
        };
    }

    /// <summary>
    /// Create immutable snapshot of Route at current state
    /// </summary>
    public static RouteSnapshot CreateRouteSnapshot(RouteOption route)
    {
        if (route == null) return null;

        return new RouteSnapshot
        {
            OriginLocationName = route.OriginLocation?.Name ?? "Unknown",
            DestinationLocationName = route.DestinationLocation?.Name ?? "Unknown",
            Method = route.Method,
            BaseStaminaCost = route.BaseStaminaCost,
            BaseCoinCost = route.BaseCoinCost,
            TerrainCategories = route.TerrainCategories != null
                ? new List<TerrainCategory>(route.TerrainCategories)
                : new List<TerrainCategory>()
        };
    }

    /// <summary>
    /// Create immutable snapshot of PlacementFilter
    /// </summary>
    public static PlacementFilterSnapshot CreatePlacementFilterSnapshot(PlacementFilter filter)
    {
        if (filter == null) return null;

        return new PlacementFilterSnapshot
        {
            PlacementType = filter.PlacementType,
            SelectionStrategy = filter.SelectionStrategy,
            PersonalityType = filter.PersonalityType,
            Profession = filter.Profession,
            SocialStanding = filter.SocialStanding,
            StoryRole = filter.StoryRole,
            LocationRole = filter.LocationRole,
            Privacy = filter.Privacy,
            Safety = filter.Safety,
            Activity = filter.Activity,
            Purpose = filter.Purpose,
            Terrain = filter.Terrain,
            Structure = filter.Structure,
            RouteTier = filter.RouteTier
        };
    }

    /// <summary>
    /// Create immutable snapshot of CompoundRequirement
    /// Captures stat and resource requirements at execution time
    /// Uses explicit OrPath properties (Explicit Property Principle)
    /// </summary>
    public static RequirementSnapshot CreateRequirementSnapshot(CompoundRequirement requirement)
    {
        if (requirement == null || requirement.OrPaths == null || requirement.OrPaths.Count == 0)
            return null;

        OrPath firstPath = requirement.OrPaths.FirstOrDefault();
        if (firstPath == null)
            return null;

        RequirementSnapshot snapshot = new RequirementSnapshot
        {
            RequiredStates = new List<string>(),
            RequiredRapport = firstPath.RapportRequired,
            RequiredInsight = firstPath.InsightRequired,
            RequiredAuthority = firstPath.AuthorityRequired,
            RequiredDiplomacy = firstPath.DiplomacyRequired,
            RequiredCunning = firstPath.CunningRequired,
            RequiredCoins = firstPath.CoinsRequired
        };

        if (firstPath.RequiredState.HasValue)
        {
            snapshot.RequiredStates.Add(firstPath.RequiredState.Value.ToString());
        }

        return snapshot;
    }

    /// <summary>
    /// Create immutable snapshot of ChoiceCost
    /// </summary>
    public static CostSnapshot CreateCostSnapshot(ChoiceCost cost)
    {
        if (cost == null) return null;

        return new CostSnapshot
        {
            CoinsSpent = cost.Coins,
            StaminaSpent = cost.Stamina,
            FocusSpent = cost.Focus,
            HealthSpent = cost.Health,
            ResolveSpent = cost.Resolve,
            TimeSegmentsSpent = cost.TimeSegments
        };
    }

    /// <summary>
    /// Create immutable snapshot of ChoiceReward
    /// </summary>
    public static RewardSnapshot CreateRewardSnapshot(ChoiceReward reward)
    {
        if (reward == null) return null;

        RewardSnapshot snapshot = new RewardSnapshot
        {
            CoinsGained = reward.Coins,
            ResolveGained = reward.Resolve,
            HealthGained = reward.Health,
            StaminaGained = reward.Stamina,
            FocusGained = reward.Focus,
            InsightGained = reward.Insight,
            RapportGained = reward.Rapport,
            AuthorityGained = reward.Authority,
            DiplomacyGained = reward.Diplomacy,
            CunningGained = reward.Cunning,
            BondChanges = new List<string>(),
            ItemsGranted = new List<string>(),
            StatesApplied = new List<string>(),
            AchievementsGranted = new List<string>()
        };

        // Summarize bond changes
        if (reward.BondChanges != null)
        {
            foreach (BondChange bondChange in reward.BondChanges)
            {
                string npcName = bondChange.Npc?.Name ?? "Unknown NPC";
                snapshot.BondChanges.Add($"{npcName}: {bondChange.Delta:+#;-#;0}");
            }
        }

        // Summarize state applications
        if (reward.StateApplications != null)
        {
            foreach (StateApplication stateApp in reward.StateApplications)
            {
                string action = stateApp.Apply ? "Apply" : "Remove";
                snapshot.StatesApplied.Add($"{stateApp.StateType} ({action})");
            }
        }

        // Summarize achievements
        if (reward.Achievements != null)
        {
            foreach (Achievement achievement in reward.Achievements)
            {
                snapshot.AchievementsGranted.Add(achievement.Name);
            }
        }

        return snapshot;
    }

    /// <summary>
    /// Create immutable snapshot of Player state
    /// Optional - for detailed analysis
    /// </summary>
    public static PlayerStateSnapshot CreatePlayerStateSnapshot(Player player, GameWorld gameWorld)
    {
        if (player == null) return null;

        return new PlayerStateSnapshot
        {
            Insight = player.Insight,
            Rapport = player.Rapport,
            Authority = player.Authority,
            Diplomacy = player.Diplomacy,
            Cunning = player.Cunning,
            Coins = player.Coins,
            Health = player.Health,
            Stamina = player.Stamina,
            Focus = player.Focus,
            Resolve = player.Resolve,
            CurrentDay = gameWorld.CurrentDay,
            CurrentTimeBlock = gameWorld.CurrentTimeBlock,
            ActiveStates = player.ActiveStates != null
                ? player.ActiveStates.Select(s => s.Type.ToString()).ToList()
                : new List<string>()
        };
    }

    /// <summary>
    /// Create RewardSnapshot from Consequence (unified pattern adapter)
    /// Converts Consequence (negative for costs, positive for rewards) to RewardSnapshot (positive values only)
    /// </summary>
    public static RewardSnapshot CreateRewardSnapshotFromConsequence(Consequence consequence)
    {
        if (consequence == null) return null;

        RewardSnapshot snapshot = new RewardSnapshot
        {
            CoinsGained = consequence.Coins > 0 ? consequence.Coins : 0,
            ResolveGained = consequence.Resolve > 0 ? consequence.Resolve : 0,
            HealthGained = consequence.Health > 0 ? consequence.Health : 0,
            StaminaGained = consequence.Stamina > 0 ? consequence.Stamina : 0,
            FocusGained = consequence.Focus > 0 ? consequence.Focus : 0,
            InsightGained = consequence.Insight,
            RapportGained = consequence.Rapport,
            AuthorityGained = consequence.Authority,
            DiplomacyGained = consequence.Diplomacy,
            CunningGained = consequence.Cunning,
            BondChanges = new List<string>(),
            ItemsGranted = new List<string>(),
            StatesApplied = new List<string>(),
            AchievementsGranted = new List<string>()
        };

        // Summarize bond changes
        if (consequence.BondChanges != null)
        {
            foreach (BondChange bondChange in consequence.BondChanges)
            {
                string npcName = bondChange.Npc?.Name ?? "Unknown NPC";
                snapshot.BondChanges.Add($"{npcName}: {bondChange.Delta:+#;-#;0}");
            }
        }

        // Summarize state applications
        if (consequence.StateApplications != null)
        {
            foreach (StateApplication stateApp in consequence.StateApplications)
            {
                string action = stateApp.Apply ? "+" : "-";
                snapshot.StatesApplied.Add($"{action}{stateApp.StateType}");
            }
        }

        // Summarize achievements
        if (consequence.Achievements != null)
        {
            foreach (Achievement achievement in consequence.Achievements)
            {
                snapshot.AchievementsGranted.Add(achievement.Name);
            }
        }

        // Summarize items
        if (consequence.Items != null)
        {
            foreach (Item item in consequence.Items)
            {
                snapshot.ItemsGranted.Add(item.Name);
            }
        }

        return snapshot;
    }
}
