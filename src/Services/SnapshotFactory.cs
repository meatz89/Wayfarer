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
            PersonalityTypes = filter.PersonalityTypes != null
                ? new List<PersonalityType>(filter.PersonalityTypes)
                : new List<PersonalityType>(),
            Professions = filter.Professions != null
                ? new List<Professions>(filter.Professions)
                : new List<Professions>(),
            SocialStandings = filter.SocialStandings != null
                ? new List<NPCSocialStanding>(filter.SocialStandings)
                : new List<NPCSocialStanding>(),
            StoryRoles = filter.StoryRoles != null
                ? new List<NPCStoryRole>(filter.StoryRoles)
                : new List<NPCStoryRole>(),
            LocationTypes = filter.LocationTypes != null
                ? new List<LocationTypes>(filter.LocationTypes)
                : new List<LocationTypes>(),
            PrivacyLevels = filter.PrivacyLevels != null
                ? new List<LocationPrivacy>(filter.PrivacyLevels)
                : new List<LocationPrivacy>(),
            SafetyLevels = filter.SafetyLevels != null
                ? new List<LocationSafety>(filter.SafetyLevels)
                : new List<LocationSafety>(),
            ActivityLevels = filter.ActivityLevels != null
                ? new List<LocationActivity>(filter.ActivityLevels)
                : new List<LocationActivity>(),
            Purposes = filter.Purposes != null
                ? new List<LocationPurpose>(filter.Purposes)
                : new List<LocationPurpose>(),
            TerrainTypes = filter.TerrainTypes != null
                ? new List<string>(filter.TerrainTypes)
                : new List<string>(),
            RouteTier = filter.RouteTier
        };
    }

    /// <summary>
    /// Create immutable snapshot of CompoundRequirement
    /// Captures stat and resource requirements at execution time
    /// </summary>
    public static RequirementSnapshot CreateRequirementSnapshot(CompoundRequirement requirement)
    {
        if (requirement == null || requirement.OrPaths == null || requirement.OrPaths.Count == 0)
            return null;

        // For simplicity, capture first OR path's requirements
        // Full implementation would need to capture all paths
        OrPath firstPath = requirement.OrPaths.FirstOrDefault();
        if (firstPath == null || firstPath.NumericRequirements == null)
            return null;

        RequirementSnapshot snapshot = new RequirementSnapshot
        {
            RequiredStates = new List<string>()
        };

        // Extract stat requirements from numeric requirements
        foreach (NumericRequirement numReq in firstPath.NumericRequirements)
        {
            // NumericRequirement uses Type and Context pattern for player stats
            if (numReq.Type == "PlayerStat" && !string.IsNullOrEmpty(numReq.Context))
            {
                if (numReq.Context.Equals("Rapport", StringComparison.OrdinalIgnoreCase))
                    snapshot.RequiredRapport = numReq.Threshold;
                else if (numReq.Context.Equals("Insight", StringComparison.OrdinalIgnoreCase))
                    snapshot.RequiredInsight = numReq.Threshold;
                else if (numReq.Context.Equals("Authority", StringComparison.OrdinalIgnoreCase))
                    snapshot.RequiredAuthority = numReq.Threshold;
                else if (numReq.Context.Equals("Diplomacy", StringComparison.OrdinalIgnoreCase))
                    snapshot.RequiredDiplomacy = numReq.Threshold;
                else if (numReq.Context.Equals("Cunning", StringComparison.OrdinalIgnoreCase))
                    snapshot.RequiredCunning = numReq.Threshold;
            }
            else if (numReq.Type == "Coins")
            {
                snapshot.RequiredCoins = numReq.Threshold;
            }
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
}
