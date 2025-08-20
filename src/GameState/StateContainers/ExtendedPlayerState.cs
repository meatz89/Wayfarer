using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;


/// <summary>
/// Immutable state container for complete player data.
/// All modifications must go through operations/commands.
/// </summary>
public sealed class ExtendedPlayerState
{
    // Core identity
    public string Name { get; }
    public Genders Gender { get; }
    public string Background { get; }
    public Professions Archetype { get; }

    // Progression systems
    public int Level { get; }
    public int CurrentXP { get; }
    public int XPToNextLevel { get; }

    // Resources (already in PlayerResourceState but repeated here for completeness)
    public int Coins { get; }
    public int Stamina { get; }
    public int Concentration { get; }
    public int Health { get; }
    public int Food { get; }
    public int PatronLeverage { get; }
    public int MaxStamina { get; }
    public int MaxConcentration { get; }
    public int MaxHealth { get; }

    // Inventory
    public InventoryState Inventory { get; }

    // Location knowledge
    public ImmutableHashSet<string> LocationActionAvailability { get; }
    public ImmutableList<string> DiscoveredLocationIds { get; }
    public ImmutableList<string> UnlockedTravelMethods { get; }
    public ImmutableList<string> UnlockedNPCIds { get; }
    public ImmutableList<string> KnownLocations { get; }
    public ImmutableList<string> KnownLocationSpots { get; }
    public ImmutableDictionary<string, ImmutableList<RouteOption>> KnownRoutes { get; }
    public ImmutableList<string> KnownContracts { get; }

    // Current location
    public string CurrentLocationId { get; }
    public string CurrentLocationSpotId { get; }

    // DeliveryObligation Queue System
    public ImmutableArray<Letter> LetterQueue { get; }
    public ImmutableDictionary<ConnectionType, int> ConnectionTokens { get; }
    public ImmutableDictionary<string, ImmutableDictionary<ConnectionType, int>> NPCTokens { get; }
    public ImmutableList<DeliveryObligation> CarriedLetters { get; }

    // Queue manipulation tracking
    public int LastMorningSwapDay { get; }
    public int LastLetterBoardDay { get; }
    public ImmutableList<DeliveryObligation> DailyBoardLetters { get; }

    // DeliveryObligation history tracking
    public ImmutableDictionary<string, LetterHistory> NPCLetterHistory { get; }

    // Standing Obligations System
    public ImmutableList<StandingObligation> StandingObligations { get; }

    // Token Favor System
    public ImmutableList<string> PurchasedFavors { get; }
    public ImmutableList<string> UnlockedLocationIds { get; }
    public ImmutableList<string> UnlockedServices { get; }

    // Scenario tracking
    public ImmutableList<DeliveryObligation> DeliveredLetters { get; }
    public int TotalLettersDelivered { get; }
    public int TotalLettersExpired { get; }
    public int TotalTokensSpent { get; }

    // Skills
    public PlayerSkills Skills { get; }

    // Goals
    public ImmutableList<Goal> ActiveGoals { get; }
    public ImmutableList<Goal> CompletedGoals { get; }
    public ImmutableList<Goal> FailedGoals { get; }

    // Memories
    public ImmutableList<MemoryFlag> Memories { get; }

    // Relationships
    public RelationshipList Relationships { get; }

    // Initialization flag
    public bool IsInitialized { get; }

    // Constructor (private to enforce use of builder or factory methods)
    private ExtendedPlayerState(
        string name,
        Genders gender,
        string background,
        Professions archetype,
        int level,
        int currentXP,
        int xpToNextLevel,
        int coins,
        int stamina,
        int concentration,
        int health,
        int food,
        int patronLeverage,
        int maxStamina,
        int maxConcentration,
        int maxHealth,
        InventoryState inventory,
        IEnumerable<string> locationActionAvailability,
        IEnumerable<string> discoveredLocationIds,
        IEnumerable<string> unlockedTravelMethods,
        IEnumerable<string> unlockedNPCIds,
        IEnumerable<string> knownLocations,
        IEnumerable<string> knownLocationSpots,
        IDictionary<string, List<RouteOption>> knownRoutes,
        IEnumerable<string> knownContracts,
        string currentLocationId,
        string currentLocationSpotId,
        Letter[] letterQueue,
        IDictionary<ConnectionType, int> connectionTokens,
        IDictionary<string, Dictionary<ConnectionType, int>> npcTokens,
        IEnumerable<Letter> carriedLetters,
        int lastMorningSwapDay,
        int lastLetterBoardDay,
        IEnumerable<Letter> dailyBoardLetters,
        IDictionary<string, LetterHistory> npcLetterHistory,
        IEnumerable<StandingObligation> standingObligations,
        IEnumerable<string> purchasedFavors,
        IEnumerable<string> unlockedLocationIds,
        IEnumerable<string> unlockedServices,
        IEnumerable<Letter> deliveredLetters,
        int totalLettersDelivered,
        int totalLettersExpired,
        int totalTokensSpent,
        PlayerSkills skills,
        IEnumerable<Goal> activeGoals,
        IEnumerable<Goal> completedGoals,
        IEnumerable<Goal> failedGoals,
        IEnumerable<MemoryFlag> memories,
        RelationshipList relationships,
        bool isInitialized)
    {
        Name = name;
        Gender = gender;
        Background = background;
        Archetype = archetype;
        Level = level;
        CurrentXP = currentXP;
        XPToNextLevel = xpToNextLevel;
        Coins = coins;
        Stamina = stamina;
        Concentration = concentration;
        Health = health;
        Food = food;
        PatronLeverage = patronLeverage;
        MaxStamina = maxStamina;
        MaxConcentration = maxConcentration;
        MaxHealth = maxHealth;
        Inventory = inventory;
        LocationActionAvailability = locationActionAvailability?.ToImmutableHashSet() ?? ImmutableHashSet<string>.Empty;
        DiscoveredLocationIds = discoveredLocationIds?.ToImmutableList() ?? ImmutableList<string>.Empty;
        UnlockedTravelMethods = unlockedTravelMethods?.ToImmutableList() ?? ImmutableList<string>.Empty;
        UnlockedNPCIds = unlockedNPCIds?.ToImmutableList() ?? ImmutableList<string>.Empty;
        KnownLocations = knownLocations?.ToImmutableList() ?? ImmutableList<string>.Empty;
        KnownLocationSpots = knownLocationSpots?.ToImmutableList() ?? ImmutableList<string>.Empty;

        // Convert known routes to immutable
        ImmutableDictionary<string, ImmutableList<RouteOption>> immutableRoutes = knownRoutes?.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value?.ToImmutableList() ?? ImmutableList<RouteOption>.Empty
        ).ToImmutableDictionary() ?? ImmutableDictionary<string, ImmutableList<RouteOption>>.Empty;
        KnownRoutes = immutableRoutes;

        KnownContracts = knownContracts?.ToImmutableList() ?? ImmutableList<string>.Empty;
        CurrentLocationId = currentLocationId;
        CurrentLocationSpotId = currentLocationSpotId;
        LetterQueue = letterQueue?.ToImmutableArray() ?? ImmutableArray<Letter>.Empty;
        ConnectionTokens = connectionTokens?.ToImmutableDictionary() ?? ImmutableDictionary<ConnectionType, int>.Empty;

        // Convert NPC tokens to immutable
        ImmutableDictionary<string, ImmutableDictionary<ConnectionType, int>> immutableNpcTokens = npcTokens?.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value?.ToImmutableDictionary() ?? ImmutableDictionary<ConnectionType, int>.Empty
        ).ToImmutableDictionary() ?? ImmutableDictionary<string, ImmutableDictionary<ConnectionType, int>>.Empty;
        NPCTokens = immutableNpcTokens;

        CarriedLetters = carriedLetters?.ToImmutableList() ?? ImmutableList<DeliveryObligation>.Empty;
        LastMorningSwapDay = lastMorningSwapDay;
        LastLetterBoardDay = lastLetterBoardDay;
        DailyBoardLetters = dailyBoardLetters?.ToImmutableList() ?? ImmutableList<DeliveryObligation>.Empty;
        NPCLetterHistory = npcLetterHistory?.ToImmutableDictionary() ?? ImmutableDictionary<string, LetterHistory>.Empty;
        StandingObligations = standingObligations?.ToImmutableList() ?? ImmutableList<StandingObligation>.Empty;
        PurchasedFavors = purchasedFavors?.ToImmutableList() ?? ImmutableList<string>.Empty;
        UnlockedLocationIds = unlockedLocationIds?.ToImmutableList() ?? ImmutableList<string>.Empty;
        UnlockedServices = unlockedServices?.ToImmutableList() ?? ImmutableList<string>.Empty;
        DeliveredLetters = deliveredLetters?.ToImmutableList() ?? ImmutableList<DeliveryObligation>.Empty;
        TotalLettersDelivered = totalLettersDelivered;
        TotalLettersExpired = totalLettersExpired;
        TotalTokensSpent = totalTokensSpent;
        Skills = skills;
        ActiveGoals = activeGoals?.ToImmutableList() ?? ImmutableList<Goal>.Empty;
        CompletedGoals = completedGoals?.ToImmutableList() ?? ImmutableList<Goal>.Empty;
        FailedGoals = failedGoals?.ToImmutableList() ?? ImmutableList<Goal>.Empty;
        Memories = memories?.ToImmutableList() ?? ImmutableList<MemoryFlag>.Empty;
        Relationships = relationships;
        IsInitialized = isInitialized;
    }

    /// <summary>
    /// Creates an ExtendedPlayerState from a mutable Player object.
    /// </summary>
    public static ExtendedPlayerState FromPlayer(Player player)
    {
        return new ExtendedPlayerState(
            player.Name,
            player.Gender,
            player.Background,
            player.Archetype,
            player.Level,
            player.CurrentXP,
            player.XPToNextLevel,
            player.Coins,
            player.Stamina,
            player.Concentration,
            player.Health,
            player.Food,
            player.PatronLeverage,
            player.MaxStamina,
            player.MaxConcentration,
            player.MaxHealth,
            InventoryState.FromInventory(player.Inventory),
            player.LocationActionAvailability,
            player.DiscoveredLocationIds,
            player.UnlockedTravelMethods,
            player.UnlockedNPCIds,
            player.KnownLocations,
            player.KnownLocationSpots,
            player.KnownRoutes,
            player.KnownContracts,
            player.CurrentLocationSpot?.LocationId,
            player.CurrentLocationSpot?.SpotID,
            player.ObligationQueue,
            player.ConnectionTokens,
            player.NPCTokens,
            player.CarriedLetters,
            player.LastMorningSwapDay,
            player.LastLetterBoardDay,
            player.DailyBoardLetters,
            player.NPCLetterHistory,
            player.StandingObligations,
            player.PurchasedFavors,
            player.UnlockedLocationIds,
            player.UnlockedServices,
            player.DeliveredLetters,
            player.TotalLettersDelivered,
            player.TotalLettersExpired,
            player.TotalTokensSpent,
            player.Skills,
            player.ActiveGoals,
            player.CompletedGoals,
            player.FailedGoals,
            player.Memories,
            player.Relationships,
            player.IsInitialized);
    }

    // Helper methods for state queries
    public bool CanPerformDangerousTravel()
    {
        return Stamina >= 4;
    }

    public bool CanPerformNobleSocialConversation()
    {
        return Stamina >= 3;
    }

    public bool HasMemory(string key, int currentDay)
    {
        return Memories.Any(m => m.Key == key && m.IsActive(currentDay));
    }

    public int GetRelationshipLevel(string npcId)
    {
        return Relationships?.GetLevel(npcId) ?? 0;
    }

    public bool HasItem(string item)
    {
        return Inventory.HasItem(item);
    }

    public int GetItemCount(string item)
    {
        return Inventory.GetItemCount(item);
    }


    // Builder pattern for complex state creation
    public class Builder
    {
        private string _name = "";
        private Genders _gender = Genders.Female;
        private string _background = "";
        private Professions _archetype = Professions.Soldier;
        private int _level = 1;
        private int _currentXP = 0;
        private int _xpToNextLevel = 100;
        private int _coins = 5;
        private int _stamina = 6;
        private int _concentration = 10;
        private int _health = 10;
        private int _food = 0;
        private int _patronLeverage = 0;
        private int _maxStamina = 10;
        private int _maxConcentration = 10;
        private int _maxHealth = 10;
        private InventoryState _inventory = new InventoryState(6);
        private HashSet<string> _locationActionAvailability = new();
        private List<string> _discoveredLocationIds = new();
        private List<string> _unlockedTravelMethods = new();
        private List<string> _unlockedNPCIds = new();
        private List<string> _knownLocations = new();
        private List<string> _knownLocationSpots = new();
        private Dictionary<string, List<RouteOption>> _knownRoutes = new();
        private List<string> _knownContracts = new();
        private string _currentLocationId = "";
        private string _currentLocationSpotId = "";
        private DeliveryObligation[] _letterQueue = new Letter[8];
        private Dictionary<ConnectionType, int> _connectionTokens = new();
        private Dictionary<string, Dictionary<ConnectionType, int>> _npcTokens = new();
        private List<DeliveryObligation> _carriedLetters = new();
        private int _lastMorningSwapDay = -1;
        private int _lastLetterBoardDay = -1;
        private List<DeliveryObligation> _dailyBoardLetters = new();
        private Dictionary<string, LetterHistory> _npcLetterHistory = new();
        private List<StandingObligation> _standingObligations = new();
        private List<string> _purchasedFavors = new();
        private List<string> _unlockedLocationIds = new();
        private List<string> _unlockedServices = new();
        private List<DeliveryObligation> _deliveredLetters = new();
        private int _totalLettersDelivered = 0;
        private int _totalLettersExpired = 0;
        private int _totalTokensSpent = 0;
        private PlayerSkills _skills = new();
        private List<Goal> _activeGoals = new();
        private List<Goal> _completedGoals = new();
        private List<Goal> _failedGoals = new();
        private List<MemoryFlag> _memories = new();
        private RelationshipList _relationships = new();
        private bool _isInitialized = false;

        public Builder FromPlayer(Player player)
        {
            _name = player.Name;
            _gender = player.Gender;
            _background = player.Background;
            _archetype = player.Archetype;
            _level = player.Level;
            _currentXP = player.CurrentXP;
            _xpToNextLevel = player.XPToNextLevel;
            _coins = player.Coins;
            _stamina = player.Stamina;
            _concentration = player.Concentration;
            _health = player.Health;
            _food = player.Food;
            _patronLeverage = player.PatronLeverage;
            _maxStamina = player.MaxStamina;
            _maxConcentration = player.MaxConcentration;
            _maxHealth = player.MaxHealth;
            _inventory = InventoryState.FromInventory(player.Inventory);
            _locationActionAvailability = new HashSet<string>(player.LocationActionAvailability);
            _discoveredLocationIds = new List<string>(player.DiscoveredLocationIds);
            _unlockedTravelMethods = new List<string>(player.UnlockedTravelMethods);
            _unlockedNPCIds = new List<string>(player.UnlockedNPCIds);
            _knownLocations = new List<string>(player.KnownLocations);
            _knownLocationSpots = new List<string>(player.KnownLocationSpots);
            _knownRoutes = new Dictionary<string, List<RouteOption>>(player.KnownRoutes);
            _knownContracts = new List<string>(player.KnownContracts);
            _currentLocationId = player.CurrentLocationSpot?.LocationId;
            _currentLocationSpotId = player.CurrentLocationSpot?.SpotID;
            _letterQueue = player.ObligationQueue.ToArray();
            _connectionTokens = new Dictionary<ConnectionType, int>(player.ConnectionTokens);
            _npcTokens = player.NPCTokens.ToDictionary(
                kvp => kvp.Key,
                kvp => new Dictionary<ConnectionType, int>(kvp.Value)
            );
            _carriedLetters = new List<DeliveryObligation>(player.CarriedLetters);
            _lastMorningSwapDay = player.LastMorningSwapDay;
            _lastLetterBoardDay = player.LastLetterBoardDay;
            _dailyBoardLetters = new List<DeliveryObligation>(player.DailyBoardLetters);
            _npcLetterHistory = new Dictionary<string, LetterHistory>(player.NPCLetterHistory);
            _standingObligations = new List<StandingObligation>(player.StandingObligations);
            _purchasedFavors = new List<string>(player.PurchasedFavors);
            _unlockedLocationIds = new List<string>(player.UnlockedLocationIds);
            _unlockedServices = new List<string>(player.UnlockedServices);
            _deliveredLetters = new List<DeliveryObligation>(player.DeliveredLetters);
            _totalLettersDelivered = player.TotalLettersDelivered;
            _totalLettersExpired = player.TotalLettersExpired;
            _totalTokensSpent = player.TotalTokensSpent;
            _skills = player.Skills;
            _activeGoals = new List<Goal>(player.ActiveGoals);
            _completedGoals = new List<Goal>(player.CompletedGoals);
            _failedGoals = new List<Goal>(player.FailedGoals);
            _memories = new List<MemoryFlag>(player.Memories);
            _relationships = player.Relationships;
            _isInitialized = player.IsInitialized;
            return this;
        }

        public Builder WithCoins(int coins)
        {
            _coins = coins;
            return this;
        }

        public Builder WithStamina(int stamina)
        {
            _stamina = stamina;
            return this;
        }

        public Builder WithInventory(InventoryState inventory)
        {
            _inventory = inventory;
            return this;
        }

        public Builder WithAddedItem(string itemId)
        {
            _inventory = _inventory.WithAddedItem(itemId);
            return this;
        }

        public Builder WithRemovedItem(string itemId)
        {
            _inventory = _inventory.WithRemovedItem(itemId);
            return this;
        }

        public ExtendedPlayerState Build()
        {
            return new ExtendedPlayerState(
                _name, _gender, _background, _archetype,
                _level, _currentXP, _xpToNextLevel,
                _coins, _stamina, _concentration, _health, _food, _patronLeverage,
                _maxStamina, _maxConcentration, _maxHealth,
                _inventory, _locationActionAvailability, _discoveredLocationIds,
                _unlockedTravelMethods, _unlockedNPCIds, _knownLocations,
                _knownLocationSpots, _knownRoutes, _knownContracts,
                _currentLocationId, _currentLocationSpotId,
                _letterQueue, _connectionTokens, _npcTokens,
                _carriedLetters, _lastMorningSwapDay, _lastLetterBoardDay,
                _dailyBoardLetters, _npcLetterHistory, _standingObligations,
                _purchasedFavors, _unlockedLocationIds, _unlockedServices,
                _deliveredLetters, _totalLettersDelivered, _totalLettersExpired,
                _totalTokensSpent, _skills, _activeGoals, _completedGoals,
                _failedGoals, _memories, _relationships, _isInitialized);
        }
    }
}