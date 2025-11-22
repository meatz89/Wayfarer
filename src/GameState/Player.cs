public class Player
{
    // Core identity
    public string Name { get; set; }
    public Genders Gender { get; set; }
    public string Background { get; set; }

    // Archetype
    public Professions Archetype { get; set; }

    // Progression systems - ALL values from JSON via ApplyInitialConfiguration
    public int Level { get; set; }
    public int CurrentXP { get; set; }
    public int XPToNextLevel { get; set; }

    // Resources - ALL values from JSON via ApplyInitialConfiguration
    public int Coins { get; set; }
    public int Health { get; set; }
    public int Hunger { get; set; }
    public int Stamina { get; set; }
    public int MaxStamina { get; set; }

    public int MinHealth { get; set; }
    public int MaxHealth { get; set; }
    public int MaxFocus { get; set; }
    public int MaxHunger { get; set; }

    public Inventory Inventory { get; set; } = new Inventory(10); // 10 weight capacity per documentation

    // Relationships with characters
    public RelationshipList Relationships { get; set; } = new();

    // Venue knowledge - Moved from action system
    // HIGHLANDER: Object references only, no string IDs
    public List<Location> LocationActionAvailability { get; set; } = new List<Location>();

    // Travel capabilities
    public List<string> UnlockedTravelMethods { get; set; } = new List<string>();

    // Five Stats - Simple integers like Coins/Health/Stamina
    // Granted directly through choice rewards (Sir Brante pattern)
    // No XP, no levels, no complex progression - just numbers
    public int Insight { get; set; } = 0;
    public int Rapport { get; set; } = 0;
    public int Authority { get; set; } = 0;
    public int Diplomacy { get; set; } = 0;
    public int Cunning { get; set; } = 0;

    // Hex-first architecture: Player position is hex coordinates
    // Location derived via: hexMap.GetHex(player.CurrentPosition)?.LocationId
    public AxialCoordinates CurrentPosition { get; set; }
    public List<MemoryFlag> Memories { get; private set; } = new List<MemoryFlag>();

    public List<KnownRouteEntry> KnownRoutes { get; private set; } = new List<KnownRouteEntry>();

    public List<MeetingObligation> MeetingObligations { get; set; } = new List<MeetingObligation>();
    public List<NPCTokenEntry> NPCTokens { get; private set; } = new List<NPCTokenEntry>();

    // Physical DeliveryObligation Carrying
    public int MaxSatchelSize { get; set; } = 12; // Maximum size capacity for letters in satchel

    // Queue manipulation tracking
    public int LastMorningSwapDay { get; set; } = -1; // Track when morning swap was last used

    // Standing Obligations System
    public List<StandingObligation> StandingObligations { get; private set; } = new List<StandingObligation>();

    // Active Obligations (Core Loop design)
    // Tracks obligations player has activated (NPCCommissioned have deadlines)
    // HIGHLANDER: Object references ONLY, no ActiveObligationIds
    public List<Obligation> ActiveObligations { get; set; } = new List<Obligation>();

    // ============================================
    // DELIVERY JOB SYSTEM (Core Loop - Phase 3)
    // ============================================

    /// <summary>
    /// Active delivery job (null = no active job)
    /// Player can only have ONE active delivery job at a time
    /// HIGHLANDER: Object reference ONLY, no ActiveDeliveryJobId
    /// </summary>
    public DeliveryJob ActiveDeliveryJob { get; set; }

    /// <summary>
    /// Check if player has an active delivery job
    /// </summary>
    public bool HasActiveDeliveryJob => ActiveDeliveryJob != null;

    // Equipment ownership: Player.Inventory stores Item objects (HIGHLANDER principle)
    // Inventory class has List<Item> Items property with direct object references
    // No ID lookups - objects stored directly

    // Route Familiarity System (0-5 scale per route)
    // ID is route ID, level is familiarity level (0=Unknown, 5=Mastered)
    public List<FamiliarityEntry> RouteFamiliarity { get; set; } = new List<FamiliarityEntry>();

    // Location Familiarity System (Work Packet 1)
    // ID is Location ID (spot like "courtyard", "mill_entrance"), level is familiarity level (0-3)
    public List<FamiliarityEntry> LocationFamiliarity { get; set; } = new List<FamiliarityEntry>();

    // ============================================
    // INTERACTION HISTORY (Procedural Content Generation - LeastRecent Selection Strategy)
    // ============================================

    /// <summary>
    /// Location visit timestamps for LeastRecent selection strategy
    /// Tracks LAST visit time per location (one record per location, update in place)
    /// Used by procedural scene placement to prefer locations player hasn't visited recently
    /// </summary>
    public List<LocationVisitRecord> LocationVisits { get; set; } = new List<LocationVisitRecord>();

    /// <summary>
    /// NPC interaction history with timestamps for LeastRecent selection strategy
    /// Tracks when player last interacted with each NPC
    /// Used by procedural scene placement to prefer NPCs player hasn't interacted with recently
    /// </summary>
    public List<NPCInteractionRecord> NPCInteractions { get; set; } = new List<NPCInteractionRecord>();

    /// <summary>
    /// Route traversal history with timestamps for LeastRecent selection strategy
    /// Tracks when player last traveled each route
    /// Used by procedural scene placement to prefer routes player hasn't traversed recently
    /// </summary>
    public List<RouteTraversalRecord> RouteTraversals { get; set; } = new List<RouteTraversalRecord>();

    /// <summary>
    /// Observations collected by player during exploration
    /// Mental system uses observation objects directly (HIGHLANDER: no IDs)
    /// </summary>
    public List<Observation> CollectedObservations { get; set; } = new List<Observation>();

    /// <summary>
    /// Active injury cards accumulated from Physical challenge failures
    /// These cards are added to Physical challenge decks as permanent debuffs
    /// Physical system uses card objects directly (HIGHLANDER: no IDs)
    /// </summary>
    public List<PhysicalCard> InjuryCards { get; set; } = new List<PhysicalCard>();

    // Reputation system - Physical success builds reputation affecting Social and Physical engagements
    public int Reputation { get; set; } = 0;

    // Physical progression - Mastery cubes earned through repeated challenge success (0-10 per deck)
    // Reduces Physical Danger threshold for specific challenge types (Combat, Athletics, etc.)
    public List<MasteryCubeEntry> MasteryCubes { get; set; } = new List<MasteryCubeEntry>();

    // Mental resource - Focus depletes with obligation, recovers with rest
    // 6-point scale: Each point = ~16.7% of capacity
    // Below 2 Focus: Exposure accumulates faster (+1 per action)
    // Maximum: 6, Cost: 1-2 per obligation start
    public int Focus { get; set; } = 6;

    // Mental resource - Understanding cumulative expertise (0-10 scale)
    // Granted by ALL Mental challenges (+1 to +3 based on difficulty)
    // Used by DifficultyModifiers to reduce Exposure baseline
    // Never depletes - permanent player growth (Mental challenge expertise)
    // Competition: Multiple obligations need it, limited Focus/Time to accumulate
    // Strategic choice: Build Understanding through easy challenges, or attempt hard challenges early
    public int Understanding { get; set; } = 0;

    // Narrative knowledge tokens - Acquired through conversations, observations, and emergencies
    // Used for: Unlocking conversation trees, gating dialogue responses, quest progression
    // Distinct from Understanding (which is Mental challenge expertise level 0-10)
    // Examples: "guard_routine", "secret_passage", "npc_motivation"
    public List<string> Knowledge { get; set; } = new List<string>();

    // Scene-Situation Architecture additions (Sir Brante inspired progression)

    /// <summary>
    /// Resolve - universal consumable resource (0-30, similar to Willpower in Sir Brante)
    /// Used to unlock situations and make difficult choices
    /// More restrictive than Focus - creates genuine strategic choices
    /// </summary>
    public int Resolve { get; set; } = 30; // Start at max

    /// <summary>
    /// Player Scales - 6 moral/behavioral axes (-10 to +10 each)
    /// Strongly-typed nested object (NOT list or dictionary)
    /// Both extremes unlock content (different archetypes, not better/worse)
    /// </summary>
    public PlayerScales Scales { get; set; } = new PlayerScales();

    /// <summary>
    /// Active States - temporary conditions currently affecting player
    /// List of active state instances with segment-based duration tracking
    /// Examples: Wounded, Exhausted, Trusted, Celebrated, Obsessed
    /// </summary>
    public List<ActiveState> ActiveStates { get; set; } = new List<ActiveState>();

    /// <summary>
    /// Earned Achievements - milestone markers player has achieved
    /// List of achievement instances with segment-based earned time
    /// Categories: Physical, Social, Investigation, Economic, Political
    /// </summary>
    public List<PlayerAchievement> EarnedAchievements { get; set; } = new List<PlayerAchievement>();

    /// <summary>
    /// Completed Situation IDs - tracking which situations player has finished
    /// Used for spawn rules and requirement checking
    /// Situations can spawn child situations creating cascading chains
    /// HIGHLANDER: Object references ONLY, no CompletedSituationIds
    /// </summary>
    public List<Situation> CompletedSituations { get; set; } = new List<Situation>();

    public void AddKnownRoute(RouteOption route)
    {
        // ZERO NULL TOLERANCE: route must never be null (architectural guarantee from caller)
        // HIGHLANDER: Use Location object for lookup, not string name
        Location origin = route.OriginLocation;

        KnownRouteEntry routeEntry = KnownRoutes.FirstOrDefault(kr => kr.OriginLocation == origin);
        if (routeEntry == null)
        {
            routeEntry = new KnownRouteEntry { OriginLocation = origin };
            KnownRoutes.Add(routeEntry);
        }

        // Only add if not already known
        if (!routeEntry.Routes.Any(r => r.DestinationLocation == route.DestinationLocation))
        {
            routeEntry.Routes.Add(route);
        }
    }

    /// <summary>
    /// Get familiarity level for a route (0-5 scale)
    /// HIGHLANDER: Accept RouteOption object, not string ID
    /// ZERO NULL TOLERANCE: route must never be null
    /// </summary>
    public int GetRouteFamiliarity(RouteOption route)
    {
        FamiliarityEntry entry = RouteFamiliarity.FirstOrDefault(f => f.EntityId == route.Name);
        return entry != null ? entry.Level : 0;
    }

    /// <summary>
    /// Set route familiarity to a specific value (max 5)
    /// HIGHLANDER: Accept RouteOption object, not string ID
    /// ZERO NULL TOLERANCE: route must never be null
    /// </summary>
    public void SetRouteFamiliarity(RouteOption route, int level)
    {
        FamiliarityEntry existing = RouteFamiliarity.FirstOrDefault(f => f.EntityId == route.Name);
        if (existing != null)
        {
            existing.Level = level;
        }
        else
        {
            RouteFamiliarity.Add(new FamiliarityEntry { EntityId = route.Name, Level = level });
        }
    }

    /// <summary>
    /// Increase route familiarity after successful travel (max 5)
    /// HIGHLANDER: Accept RouteOption object, not string ID
    /// ZERO NULL TOLERANCE: route must never be null
    /// </summary>
    public void IncreaseRouteFamiliarity(RouteOption route, int amount = 1)
    {
        int current = GetRouteFamiliarity(route);
        SetRouteFamiliarity(route, Math.Min(5, current + amount));
    }

    /// <summary>
    /// Check if route is mastered (familiarity = 5)
    /// HIGHLANDER: Accept RouteOption object, not string ID
    /// ZERO NULL TOLERANCE: route must never be null
    /// </summary>
    public bool IsRouteMastered(RouteOption route)
    {
        return GetRouteFamiliarity(route) >= 5;
    }

    /// <summary>
    /// Get familiarity level for a Location (0-3 scale)
    /// HIGHLANDER: Accept Location object, not string ID
    /// ZERO NULL TOLERANCE: location must never be null
    /// </summary>
    public int GetLocationFamiliarity(Location location)
    {
        FamiliarityEntry entry = LocationFamiliarity.FirstOrDefault(f => f.EntityId == location.Name);
        return entry != null ? entry.Level : 0;
    }

    /// <summary>
    /// Set Location familiarity to a specific value (max 3)
    /// HIGHLANDER: Accept Location object, not string ID
    /// ZERO NULL TOLERANCE: location must never be null
    /// </summary>
    public void SetLocationFamiliarity(Location location, int value)
    {
        int clampedValue = Math.Min(3, Math.Max(0, value));
        FamiliarityEntry existing = LocationFamiliarity.FirstOrDefault(f => f.EntityId == location.Name);
        if (existing != null)
        {
            existing.Level = clampedValue;
        }
        else
        {
            LocationFamiliarity.Add(new FamiliarityEntry { EntityId = location.Name, Level = clampedValue });
        }
    }

    // ============================================
    // NPC TOKEN MANAGEMENT (Connection system)
    // ============================================

    /// <summary>
    /// Get token count for specific NPC and connection type
    /// HIGHLANDER: Accept NPC object, not string ID
    /// ZERO NULL TOLERANCE: npc must never be null
    /// </summary>
    public int GetNPCTokenCount(NPC npc, ConnectionType type)
    {
        // HIGHLANDER: NPCTokenEntry.Npc is object reference, not string ID
        NPCTokenEntry entry = NPCTokens.FirstOrDefault(t => t.Npc == npc);
        return entry != null ? entry.GetTokenCount(type) : 0;
    }

    /// <summary>
    /// Set token count for specific NPC and connection type
    /// HIGHLANDER: Accept NPC object, not string ID
    /// ZERO NULL TOLERANCE: npc must never be null
    /// </summary>
    public void SetNPCTokenCount(NPC npc, ConnectionType type, int count)
    {
        // HIGHLANDER: NPCTokenEntry.Npc is object reference, not string ID
        NPCTokenEntry entry = NPCTokens.FirstOrDefault(t => t.Npc == npc);
        if (entry == null)
        {
            entry = new NPCTokenEntry { Npc = npc };
            NPCTokens.Add(entry);
        }
        entry.SetTokenCount(type, count);
    }

    /// <summary>
    /// Get NPC token entry (creates if doesn't exist)
    /// HIGHLANDER: Accepts NPC object, not string ID
    /// </summary>
    public NPCTokenEntry GetNPCTokenEntry(NPC npc)
    {
        NPCTokenEntry entry = NPCTokens.FirstOrDefault(t => t.Npc == npc);
        if (entry == null)
        {
            entry = new NPCTokenEntry { Npc = npc };
            NPCTokens.Add(entry);
        }
        return entry;
    }

    public void AddMemory(string key, string description, int currentDay, int importance, int expirationDays = -1)
    {
        // Remove any existing memory with same key
        Memories.RemoveAll(m => m.Key == key);

        // Add new memory
        Memories.Add(new MemoryFlag
        {
            Key = key,
            Description = description,
            CreationDay = currentDay,
            ExpirationDay = expirationDays == -1 ? -1 : currentDay + expirationDays,
            Importance = importance
        });
    }

    public bool HasMemory(string key, int currentDay)
    {
        return Memories.Any(m => m.Key == key && m.IsActive(currentDay));
    }

    public MemoryFlag GetMemory(string key)
    {
        return Memories.FirstOrDefault(m => m.Key == key);
    }

    public Player()
    {
        Background = GameRules.StandardRuleset.Background;
        Inventory = new Inventory(10); // 10 weight capacity

        // HIGHLANDER PRINCIPLE: ALL values set by ApplyInitialConfiguration from JSON
        // NO hardcoded defaults - values come from package starting conditions
    }

    public bool ModifyHealth(int count)
    {
        int newHealth = Math.Clamp(Health + count, 0, MaxHealth);
        if (newHealth != Health)
        {
            Health = newHealth;
            return true;
        }
        return false;
    }

    public bool ModifyHunger(int amount)
    {
        int newHunger = Math.Clamp(Hunger + amount, 0, MaxHunger);
        if (newHunger != Hunger)
        {
            Hunger = newHunger;
            return true;
        }
        return false;
    }

    public bool ReduceHunger(int amount)
    {
        // Eating reduces hunger
        return ModifyHunger(-amount);
    }

    // Hunger helper methods (moved from HungerManager)
    public bool IsStarving()
    {
        return Hunger >= 80;
    }

    public bool IsHungry()
    {
        return Hunger >= 50;
    }

    public string GetHungerLevelDescription()
    {
        if (Hunger >= 80) return "Starving";
        if (Hunger >= 50) return "Hungry";
        if (Hunger >= 20) return "Peckish";
        return "Well Fed";
    }

    public void AddExperiencePoints(int xpBonus)
    {
        int newExperiencePoints = Math.Max(0, CurrentXP + xpBonus);
        if (newExperiencePoints != CurrentXP)
        {
            CurrentXP = newExperiencePoints;
        }
    }

    public void AddCoins(int count)
    {
        int newCoins = Math.Max(0, Coins + count);
        if (newCoins != Coins)
        {
            Coins = newCoins;
        }
    }

    public void ModifyCoins(int amount)
    {
        Coins += amount;
        if (Coins < 0)
        {
            Coins = 0;
        }
    }

    internal void SetCoins(int value)
    {
        Coins = Math.Max(0, value);
    }

    /// <summary>
    /// Apply initial player configuration from package starting conditions
    /// ZERO NULL TOLERANCE: config must never be null (architectural guarantee from caller)
    /// </summary>
    public void ApplyInitialConfiguration(PlayerInitialConfig config, GameWorld gameWorld)
    {

        // Progression
        if (config.Level.HasValue) Level = config.Level.Value;
        if (config.CurrentXP.HasValue) CurrentXP = config.CurrentXP.Value;
        if (config.XPToNextLevel.HasValue) XPToNextLevel = config.XPToNextLevel.Value;

        // Resources
        if (config.Coins.HasValue) Coins = config.Coins.Value;
        if (config.Health.HasValue) Health = config.Health.Value;
        if (config.MaxHealth.HasValue) MaxHealth = config.MaxHealth.Value;
        if (config.MinHealth.HasValue) MinHealth = config.MinHealth.Value;
        if (config.Hunger.HasValue) Hunger = config.Hunger.Value;
        if (config.MaxHunger.HasValue) MaxHunger = config.MaxHunger.Value;
        if (config.StaminaPoints.HasValue) Stamina = config.StaminaPoints.Value;
        if (config.MaxStamina.HasValue) MaxStamina = config.MaxStamina.Value;
        if (config.Focus.HasValue) Focus = config.Focus.Value;
        if (config.MaxFocus.HasValue) MaxFocus = config.MaxFocus.Value;

        // Apply satchel capacity - update inventory max weight
        if (config.SatchelCapacity.HasValue)
        {
            Inventory = new Inventory(config.SatchelCapacity.Value);
        }

        // Add initial items to inventory
        // ZERO NULL TOLERANCE: InitialItems can be null/empty (optional configuration)
        if (config.InitialItems != null && config.InitialItems.Count > 0)
        {
            foreach (ResourceEntry entry in config.InitialItems)
            {
                // HIGHLANDER: Resolve string itemId to Item object from GameWorld.Items
                // Case-insensitive lookup for robustness against data inconsistencies
                Item item = gameWorld.Items.FirstOrDefault(i =>
                    string.Equals(i.Name, entry.ResourceType, StringComparison.OrdinalIgnoreCase));
                // FAIL-FAST: If item reference is invalid, this is data error
                if (item == null)
                {
                    throw new InvalidOperationException(
                        $"Initial item '{entry.ResourceType}' not found in GameWorld.Items - check package configuration");
                }

                for (int i = 0; i < entry.Amount; i++)
                {
                    Inventory.Add(item);
                }
            }
        }

        // SatchelWeight represents initial weight from starting obligations (Viktor's package)
        // This will be handled by the obligation system when adding starting obligations
    }

    /// <summary>
    /// Get current total weight (inventory + carried letters)
    /// </summary>
    public int GetCurrentWeight(ItemRepository itemRepository)
    {
        int inventoryWeight = Inventory.GetUsedWeight();
        return inventoryWeight;
    }

    /// <summary>
    /// Check if player can carry additional weight
    /// </summary>
    public bool CanCarry(int additionalWeight, ItemRepository itemRepository)
    {
        int currentWeight = GetCurrentWeight(itemRepository);
        int maxWeight = Inventory.GetCapacity();
        return (currentWeight + additionalWeight) <= maxWeight;
    }

    /// <summary>
    /// Get current weight as ratio (for UI display)
    /// </summary>
    public WeightStatus GetWeightStatus(ItemRepository itemRepository)
    {
        return new WeightStatus(GetCurrentWeight(itemRepository), Inventory.GetCapacity());
    }

    public void AddKnownLocation(Location location)
    {
        if (!LocationActionAvailability.Contains(location))
        {
            LocationActionAvailability.Add(location);
        }
    }

    // ============================================
    // ITEM LIFECYCLE SYSTEM (Multi-Situation Scene Pattern)
    // ============================================

    /// <summary>
    /// Remove item from inventory
    /// Returns true if item was present and removed, false if not found
    /// Used for: Consuming keys, removing temporary access tokens, cleanup
    /// Part of item lifecycle pattern: grant → require → remove
    /// HIGHLANDER: Accepts Item object, not string ID
    /// </summary>
    public bool RemoveItem(Item item)
    {
        return Inventory.Remove(item);
    }

    /// <summary>
    /// Check if player possesses specific item by name
    /// Used for: Item possession requirements, gated progression
    /// Part of item lifecycle pattern: required for situation activation
    /// HIGHLANDER: Inventory.GetAllItems() returns List<Item>, not List<string>
    /// </summary>
    public bool HasItem(string itemName)
    {
        return Inventory.GetAllItems().Any(item => item.Name == itemName);
    }

}

