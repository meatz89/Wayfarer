using Xunit;

/// <summary>
/// SNAPSHOT FACTORY TESTS
/// Tests that snapshot creation correctly captures entity state at a point in time
/// </summary>
public class SnapshotFactoryTests
{
    // ==================== LOCATION SNAPSHOT TESTS ====================

    [Fact]
    public void CreateLocationSnapshot_CapturesAllProperties()
    {
        // Arrange
        Location location = new Location
        {
            Name = "Town Square",
            HexPosition = new AxialCoordinates(3, 5),
            Purpose = LocationPurpose.Commercial,
            Privacy = LocationPrivacy.Public,
            Safety = LocationSafety.Safe,
            Activity = LocationActivity.Busy
        };

        // Act
        LocationSnapshot snapshot = SnapshotFactory.CreateLocationSnapshot(location);

        // Assert
        Assert.NotNull(snapshot);
        Assert.Equal("Town Square", snapshot.Name);
        Assert.Equal(3, snapshot.HexPosition.Q);
        Assert.Equal(5, snapshot.HexPosition.R);
        Assert.Equal(LocationPurpose.Commercial, snapshot.Purpose);
        Assert.Equal(LocationPrivacy.Public, snapshot.Privacy);
        Assert.Equal(LocationSafety.Safe, snapshot.Safety);
        Assert.Equal(LocationActivity.Busy, snapshot.Activity);
    }

    [Fact]
    public void CreateLocationSnapshot_ReturnsNullForNullInput()
    {
        // Act
        LocationSnapshot snapshot = SnapshotFactory.CreateLocationSnapshot(null);

        // Assert
        Assert.Null(snapshot);
    }

    [Fact]
    public void CreateLocationSnapshot_HandlesNullHexPosition()
    {
        // Arrange
        Location location = new Location
        {
            Name = "Test",
            HexPosition = null
        };

        // Act
        LocationSnapshot snapshot = SnapshotFactory.CreateLocationSnapshot(location);

        // Assert
        Assert.NotNull(snapshot);
        Assert.Equal(0, snapshot.HexPosition.Q);
        Assert.Equal(0, snapshot.HexPosition.R);
    }

    // ==================== NPC SNAPSHOT TESTS ====================

    [Fact]
    public void CreateNPCSnapshot_CapturesAllProperties()
    {
        // Arrange
        NPC npc = new NPC
        {
            Name = "John Smith",
            Profession = Profession.Merchant,
            PersonalityType = PersonalityType.Friendly,
            SocialStanding = SocialStanding.Commoner,
            StoryRole = StoryRole.Ally
        };

        // Act
        NPCSnapshot snapshot = SnapshotFactory.CreateNPCSnapshot(npc);

        // Assert
        Assert.NotNull(snapshot);
        Assert.Equal("John Smith", snapshot.Name);
        Assert.Equal(Profession.Merchant, snapshot.Profession);
        Assert.Equal(PersonalityType.Friendly, snapshot.PersonalityType);
        Assert.Equal(SocialStanding.Commoner, snapshot.SocialStanding);
        Assert.Equal(StoryRole.Ally, snapshot.StoryRole);
    }

    [Fact]
    public void CreateNPCSnapshot_ReturnsNullForNullInput()
    {
        // Act
        NPCSnapshot snapshot = SnapshotFactory.CreateNPCSnapshot(null);

        // Assert
        Assert.Null(snapshot);
    }

    // ==================== ROUTE SNAPSHOT TESTS ====================

    [Fact]
    public void CreateRouteSnapshot_CapturesAllProperties()
    {
        // Arrange
        Location origin = new Location { Name = "Town" };
        Location destination = new Location { Name = "Forest" };
        RouteOption route = new RouteOption
        {
            OriginLocation = origin,
            DestinationLocation = destination,
            Method = TravelMethod.Walk,
            BaseStaminaCost = 3,
            BaseCoinCost = 0,
            TerrainCategories = new List<TerrainCategory> { TerrainCategory.Road, TerrainCategory.Forest }
        };

        // Act
        RouteSnapshot snapshot = SnapshotFactory.CreateRouteSnapshot(route);

        // Assert
        Assert.NotNull(snapshot);
        Assert.Equal("Town", snapshot.OriginLocationName);
        Assert.Equal("Forest", snapshot.DestinationLocationName);
        Assert.Equal(TravelMethod.Walk, snapshot.Method);
        Assert.Equal(3, snapshot.BaseStaminaCost);
        Assert.Equal(0, snapshot.BaseCoinCost);
        Assert.Equal(2, snapshot.TerrainCategories.Count);
        Assert.Contains(TerrainCategory.Road, snapshot.TerrainCategories);
        Assert.Contains(TerrainCategory.Forest, snapshot.TerrainCategories);
    }

    [Fact]
    public void CreateRouteSnapshot_ReturnsNullForNullInput()
    {
        // Act
        RouteSnapshot snapshot = SnapshotFactory.CreateRouteSnapshot(null);

        // Assert
        Assert.Null(snapshot);
    }

    [Fact]
    public void CreateRouteSnapshot_HandlesNullLocations()
    {
        // Arrange
        RouteOption route = new RouteOption
        {
            OriginLocation = null,
            DestinationLocation = null,
            Method = TravelMethod.Walk
        };

        // Act
        RouteSnapshot snapshot = SnapshotFactory.CreateRouteSnapshot(route);

        // Assert
        Assert.NotNull(snapshot);
        Assert.Equal("Unknown", snapshot.OriginLocationName);
        Assert.Equal("Unknown", snapshot.DestinationLocationName);
    }

    [Fact]
    public void CreateRouteSnapshot_HandlesNullTerrainCategories()
    {
        // Arrange
        RouteOption route = new RouteOption
        {
            TerrainCategories = null
        };

        // Act
        RouteSnapshot snapshot = SnapshotFactory.CreateRouteSnapshot(route);

        // Assert
        Assert.NotNull(snapshot);
        Assert.Empty(snapshot.TerrainCategories);
    }

    // ==================== PLACEMENT FILTER SNAPSHOT TESTS ====================

    [Fact]
    public void CreatePlacementFilterSnapshot_CapturesAllProperties()
    {
        // Arrange
        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.Location,
            SelectionStrategy = SelectionStrategy.Filter,
            PersonalityType = PersonalityType.Hostile,
            Profession = Profession.Guard,
            SocialStanding = SocialStanding.Noble,
            StoryRole = StoryRole.Antagonist,
            LocationRole = LocationRole.Boss,
            Privacy = LocationPrivacy.Private,
            Safety = LocationSafety.Dangerous,
            Activity = LocationActivity.Quiet,
            Purpose = LocationPurpose.Military,
            Terrain = TerrainCategory.Swamp,
            Structure = StructureCategory.Fort
        };

        // Act
        PlacementFilterSnapshot snapshot = SnapshotFactory.CreatePlacementFilterSnapshot(filter);

        // Assert
        Assert.NotNull(snapshot);
        Assert.Equal(PlacementType.Location, snapshot.PlacementType);
        Assert.Equal(SelectionStrategy.Filter, snapshot.SelectionStrategy);
        Assert.Equal(PersonalityType.Hostile, snapshot.PersonalityType);
        Assert.Equal(Profession.Guard, snapshot.Profession);
        Assert.Equal(SocialStanding.Noble, snapshot.SocialStanding);
        Assert.Equal(StoryRole.Antagonist, snapshot.StoryRole);
        Assert.Equal(LocationRole.Boss, snapshot.LocationRole);
        Assert.Equal(LocationPrivacy.Private, snapshot.Privacy);
        Assert.Equal(LocationSafety.Dangerous, snapshot.Safety);
        Assert.Equal(LocationActivity.Quiet, snapshot.Activity);
        Assert.Equal(LocationPurpose.Military, snapshot.Purpose);
        Assert.Equal(TerrainCategory.Swamp, snapshot.Terrain);
        Assert.Equal(StructureCategory.Fort, snapshot.Structure);
    }

    [Fact]
    public void CreatePlacementFilterSnapshot_ReturnsNullForNullInput()
    {
        // Act
        PlacementFilterSnapshot snapshot = SnapshotFactory.CreatePlacementFilterSnapshot(null);

        // Assert
        Assert.Null(snapshot);
    }

    // ==================== REQUIREMENT SNAPSHOT TESTS ====================

    [Fact]
    public void CreateRequirementSnapshot_CapturesStatRequirements()
    {
        // Arrange
        CompoundRequirement requirement = new CompoundRequirement
        {
            OrPaths = new List<OrPath>
            {
                new OrPath
                {
                    RapportRequired = 10,
                    InsightRequired = 5,
                    AuthorityRequired = 3,
                    DiplomacyRequired = 2,
                    CunningRequired = 1
                }
            }
        };

        // Act
        RequirementSnapshot snapshot = SnapshotFactory.CreateRequirementSnapshot(requirement);

        // Assert
        Assert.NotNull(snapshot);
        Assert.Equal(10, snapshot.RequiredRapport);
        Assert.Equal(5, snapshot.RequiredInsight);
        Assert.Equal(3, snapshot.RequiredAuthority);
        Assert.Equal(2, snapshot.RequiredDiplomacy);
        Assert.Equal(1, snapshot.RequiredCunning);
    }

    [Fact]
    public void CreateRequirementSnapshot_CapturesResourceRequirements()
    {
        // Arrange
        CompoundRequirement requirement = new CompoundRequirement
        {
            OrPaths = new List<OrPath>
            {
                new OrPath
                {
                    CoinsRequired = 50
                }
            }
        };

        // Act
        RequirementSnapshot snapshot = SnapshotFactory.CreateRequirementSnapshot(requirement);

        // Assert
        Assert.NotNull(snapshot);
        Assert.Equal(50, snapshot.RequiredCoins);
    }

    [Fact]
    public void CreateRequirementSnapshot_CapturesStateRequirements()
    {
        // Arrange
        CompoundRequirement requirement = new CompoundRequirement
        {
            OrPaths = new List<OrPath>
            {
                new OrPath
                {
                    RequiredState = StateType.Exhausted
                }
            }
        };

        // Act
        RequirementSnapshot snapshot = SnapshotFactory.CreateRequirementSnapshot(requirement);

        // Assert
        Assert.NotNull(snapshot);
        Assert.Single(snapshot.RequiredStates);
        Assert.Contains("Exhausted", snapshot.RequiredStates);
    }

    [Fact]
    public void CreateRequirementSnapshot_ReturnsNullForNullInput()
    {
        // Act
        RequirementSnapshot snapshot = SnapshotFactory.CreateRequirementSnapshot(null);

        // Assert
        Assert.Null(snapshot);
    }

    [Fact]
    public void CreateRequirementSnapshot_ReturnsNullForEmptyOrPaths()
    {
        // Arrange
        CompoundRequirement requirement = new CompoundRequirement
        {
            OrPaths = new List<OrPath>()
        };

        // Act
        RequirementSnapshot snapshot = SnapshotFactory.CreateRequirementSnapshot(requirement);

        // Assert
        Assert.Null(snapshot);
    }

    // ==================== COST SNAPSHOT TESTS ====================

    [Fact]
    public void CreateCostSnapshot_ExtractsNegativeValuesAsPositive()
    {
        // Arrange
        Consequence consequence = new Consequence
        {
            Coins = -10,
            Stamina = -3,
            Focus = -2,
            Health = -1,
            Resolve = -5,
            TimeSegments = 2
        };

        // Act
        CostSnapshot snapshot = SnapshotFactory.CreateCostSnapshot(consequence);

        // Assert
        Assert.NotNull(snapshot);
        Assert.Equal(10, snapshot.CoinsSpent);
        Assert.Equal(3, snapshot.StaminaSpent);
        Assert.Equal(2, snapshot.FocusSpent);
        Assert.Equal(1, snapshot.HealthSpent);
        Assert.Equal(5, snapshot.ResolveSpent);
        Assert.Equal(2, snapshot.TimeSegmentsSpent);
    }

    [Fact]
    public void CreateCostSnapshot_IgnoresPositiveValues()
    {
        // Arrange - Positive values are rewards, not costs
        Consequence consequence = new Consequence
        {
            Coins = 10,  // Positive = reward, not cost
            Stamina = 3
        };

        // Act
        CostSnapshot snapshot = SnapshotFactory.CreateCostSnapshot(consequence);

        // Assert - Positive values should not appear as costs
        Assert.NotNull(snapshot);
        Assert.Equal(0, snapshot.CoinsSpent);
        Assert.Equal(0, snapshot.StaminaSpent);
    }

    [Fact]
    public void CreateCostSnapshot_ReturnsNullForNullInput()
    {
        // Act
        CostSnapshot snapshot = SnapshotFactory.CreateCostSnapshot(null);

        // Assert
        Assert.Null(snapshot);
    }

    // ==================== REWARD SNAPSHOT TESTS ====================

    [Fact]
    public void CreateRewardSnapshot_ExtractsPositiveResources()
    {
        // Arrange
        Consequence consequence = new Consequence
        {
            Coins = 50,
            Resolve = 5,
            Health = 3,
            Stamina = 2,
            Focus = 1
        };

        // Act
        RewardSnapshot snapshot = SnapshotFactory.CreateRewardSnapshot(consequence);

        // Assert
        Assert.NotNull(snapshot);
        Assert.Equal(50, snapshot.CoinsGained);
        Assert.Equal(5, snapshot.ResolveGained);
        Assert.Equal(3, snapshot.HealthGained);
        Assert.Equal(2, snapshot.StaminaGained);
        Assert.Equal(1, snapshot.FocusGained);
    }

    [Fact]
    public void CreateRewardSnapshot_ExtractsStatGains()
    {
        // Arrange
        Consequence consequence = new Consequence
        {
            Insight = 3,
            Rapport = 2,
            Authority = 1,
            Diplomacy = 4,
            Cunning = 5
        };

        // Act
        RewardSnapshot snapshot = SnapshotFactory.CreateRewardSnapshot(consequence);

        // Assert
        Assert.NotNull(snapshot);
        Assert.Equal(3, snapshot.InsightGained);
        Assert.Equal(2, snapshot.RapportGained);
        Assert.Equal(1, snapshot.AuthorityGained);
        Assert.Equal(4, snapshot.DiplomacyGained);
        Assert.Equal(5, snapshot.CunningGained);
    }

    [Fact]
    public void CreateRewardSnapshot_IgnoresNegativeValues()
    {
        // Arrange - Negative values are costs, not rewards
        Consequence consequence = new Consequence
        {
            Coins = -10,  // Negative = cost, not reward
            Health = -3
        };

        // Act
        RewardSnapshot snapshot = SnapshotFactory.CreateRewardSnapshot(consequence);

        // Assert - Negative values should not appear as gains
        Assert.NotNull(snapshot);
        Assert.Equal(0, snapshot.CoinsGained);
        Assert.Equal(0, snapshot.HealthGained);
    }

    [Fact]
    public void CreateRewardSnapshot_SummarizesBondChanges()
    {
        // Arrange
        NPC npc1 = new NPC { Name = "Alice" };
        NPC npc2 = new NPC { Name = "Bob" };
        Consequence consequence = new Consequence
        {
            BondChanges = new List<BondChange>
            {
                new BondChange { Npc = npc1, Delta = 2 },
                new BondChange { Npc = npc2, Delta = -1 }
            }
        };

        // Act
        RewardSnapshot snapshot = SnapshotFactory.CreateRewardSnapshot(consequence);

        // Assert
        Assert.NotNull(snapshot);
        Assert.Equal(2, snapshot.BondChanges.Count);
        Assert.Contains("Alice: +2", snapshot.BondChanges);
        Assert.Contains("Bob: -1", snapshot.BondChanges);
    }

    [Fact]
    public void CreateRewardSnapshot_SummarizesStateApplications()
    {
        // Arrange
        Consequence consequence = new Consequence
        {
            StateApplications = new List<StateApplication>
            {
                new StateApplication { StateType = StateType.Exhausted, Apply = true },
                new StateApplication { StateType = StateType.Inspired, Apply = false }
            }
        };

        // Act
        RewardSnapshot snapshot = SnapshotFactory.CreateRewardSnapshot(consequence);

        // Assert
        Assert.NotNull(snapshot);
        Assert.Equal(2, snapshot.StatesApplied.Count);
        Assert.Contains("+Exhausted", snapshot.StatesApplied);
        Assert.Contains("-Inspired", snapshot.StatesApplied);
    }

    [Fact]
    public void CreateRewardSnapshot_SummarizesAchievements()
    {
        // Arrange
        Consequence consequence = new Consequence
        {
            Achievements = new List<Achievement>
            {
                new Achievement { Name = "First Steps" },
                new Achievement { Name = "Master Trader" }
            }
        };

        // Act
        RewardSnapshot snapshot = SnapshotFactory.CreateRewardSnapshot(consequence);

        // Assert
        Assert.NotNull(snapshot);
        Assert.Equal(2, snapshot.AchievementsGranted.Count);
        Assert.Contains("First Steps", snapshot.AchievementsGranted);
        Assert.Contains("Master Trader", snapshot.AchievementsGranted);
    }

    [Fact]
    public void CreateRewardSnapshot_SummarizesItems()
    {
        // Arrange
        Consequence consequence = new Consequence
        {
            Items = new List<Item>
            {
                new Item { Name = "Sword" },
                new Item { Name = "Shield" }
            }
        };

        // Act
        RewardSnapshot snapshot = SnapshotFactory.CreateRewardSnapshot(consequence);

        // Assert
        Assert.NotNull(snapshot);
        Assert.Equal(2, snapshot.ItemsGranted.Count);
        Assert.Contains("Sword", snapshot.ItemsGranted);
        Assert.Contains("Shield", snapshot.ItemsGranted);
    }

    [Fact]
    public void CreateRewardSnapshot_ReturnsNullForNullInput()
    {
        // Act
        RewardSnapshot snapshot = SnapshotFactory.CreateRewardSnapshot(null);

        // Assert
        Assert.Null(snapshot);
    }

    [Fact]
    public void CreateRewardSnapshot_HandlesNullCollections()
    {
        // Arrange
        Consequence consequence = new Consequence
        {
            BondChanges = null,
            StateApplications = null,
            Achievements = null,
            Items = null
        };

        // Act
        RewardSnapshot snapshot = SnapshotFactory.CreateRewardSnapshot(consequence);

        // Assert
        Assert.NotNull(snapshot);
        Assert.Empty(snapshot.BondChanges);
        Assert.Empty(snapshot.StatesApplied);
        Assert.Empty(snapshot.AchievementsGranted);
        Assert.Empty(snapshot.ItemsGranted);
    }

    // ==================== PLAYER STATE SNAPSHOT TESTS ====================

    [Fact]
    public void CreatePlayerStateSnapshot_CapturesAllStats()
    {
        // Arrange
        Player player = new Player
        {
            Insight = 10,
            Rapport = 8,
            Authority = 5,
            Diplomacy = 6,
            Cunning = 3
        };
        GameWorld gameWorld = new GameWorld
        {
            CurrentDay = 5,
            CurrentTimeBlock = TimeBlocks.Evening
        };

        // Act
        PlayerStateSnapshot snapshot = SnapshotFactory.CreatePlayerStateSnapshot(player, gameWorld);

        // Assert
        Assert.NotNull(snapshot);
        Assert.Equal(10, snapshot.Insight);
        Assert.Equal(8, snapshot.Rapport);
        Assert.Equal(5, snapshot.Authority);
        Assert.Equal(6, snapshot.Diplomacy);
        Assert.Equal(3, snapshot.Cunning);
    }

    [Fact]
    public void CreatePlayerStateSnapshot_CapturesAllResources()
    {
        // Arrange
        Player player = new Player
        {
            Coins = 100,
            Health = 80,
            Stamina = 60,
            Focus = 50,
            Resolve = 40
        };
        GameWorld gameWorld = new GameWorld();

        // Act
        PlayerStateSnapshot snapshot = SnapshotFactory.CreatePlayerStateSnapshot(player, gameWorld);

        // Assert
        Assert.NotNull(snapshot);
        Assert.Equal(100, snapshot.Coins);
        Assert.Equal(80, snapshot.Health);
        Assert.Equal(60, snapshot.Stamina);
        Assert.Equal(50, snapshot.Focus);
        Assert.Equal(40, snapshot.Resolve);
    }

    [Fact]
    public void CreatePlayerStateSnapshot_CapturesTimeContext()
    {
        // Arrange
        Player player = new Player();
        GameWorld gameWorld = new GameWorld
        {
            CurrentDay = 15,
            CurrentTimeBlock = TimeBlocks.Night
        };

        // Act
        PlayerStateSnapshot snapshot = SnapshotFactory.CreatePlayerStateSnapshot(player, gameWorld);

        // Assert
        Assert.NotNull(snapshot);
        Assert.Equal(15, snapshot.CurrentDay);
        Assert.Equal(TimeBlocks.Night, snapshot.CurrentTimeBlock);
    }

    [Fact]
    public void CreatePlayerStateSnapshot_CapturesActiveStates()
    {
        // Arrange
        Player player = new Player
        {
            ActiveStates = new List<ActiveState>
            {
                new ActiveState { Type = StateType.Exhausted },
                new ActiveState { Type = StateType.Inspired }
            }
        };
        GameWorld gameWorld = new GameWorld();

        // Act
        PlayerStateSnapshot snapshot = SnapshotFactory.CreatePlayerStateSnapshot(player, gameWorld);

        // Assert
        Assert.NotNull(snapshot);
        Assert.Equal(2, snapshot.ActiveStates.Count);
        Assert.Contains("Exhausted", snapshot.ActiveStates);
        Assert.Contains("Inspired", snapshot.ActiveStates);
    }

    [Fact]
    public void CreatePlayerStateSnapshot_ReturnsNullForNullPlayer()
    {
        // Arrange
        GameWorld gameWorld = new GameWorld();

        // Act
        PlayerStateSnapshot snapshot = SnapshotFactory.CreatePlayerStateSnapshot(null, gameWorld);

        // Assert
        Assert.Null(snapshot);
    }

    [Fact]
    public void CreatePlayerStateSnapshot_HandlesNullActiveStates()
    {
        // Arrange
        Player player = new Player { ActiveStates = null };
        GameWorld gameWorld = new GameWorld();

        // Act
        PlayerStateSnapshot snapshot = SnapshotFactory.CreatePlayerStateSnapshot(player, gameWorld);

        // Assert
        Assert.NotNull(snapshot);
        Assert.Empty(snapshot.ActiveStates);
    }
}
