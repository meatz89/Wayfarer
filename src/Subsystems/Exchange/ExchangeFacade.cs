/// <summary>
/// Public facade for all exchange-related operations.
/// Handles resource trades, exchange validation, and NPC inventory management.
/// This is the ONLY public interface for the Exchange subsystem.
/// </summary>
public class ExchangeFacade
{
    private readonly GameWorld _gameWorld;
    private readonly ExchangeOrchestrator _orchestrator;
    private readonly ExchangeValidator _validator;
    private readonly ExchangeProcessor _processor;

    // External dependencies
    private readonly TimeManager _timeManager;
    private readonly MessageSystem _messageSystem;

    public ExchangeFacade(
        GameWorld gameWorld,
        ExchangeOrchestrator orchestrator,
        ExchangeValidator validator,
        ExchangeProcessor processor,
        TimeManager timeManager,
        MessageSystem messageSystem)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
    }

    /// <summary>
    /// Create an exchange session with an NPC
    /// HIGHLANDER: Accepts NPC object, not string identifier
    /// </summary>
    public ExchangeSession CreateExchangeSession(NPC npc)
    {
        if (npc == null)
        {
            throw new ArgumentNullException(nameof(npc));
        }

        // Get available exchanges for this NPC - placeholder parameters for now
        PlayerResourceState playerResources = _gameWorld.GetPlayerResourceState();
        TimeBlocks timeBlock = _timeManager.GetCurrentTimeBlock();
        Dictionary<ConnectionType, int> npcTokens = new Dictionary<ConnectionType, int>(); // Would need TokenFacade
        RelationshipTier relationshipTier = RelationshipTier.None;
        List<ExchangeOption> availableExchanges = GetAvailableExchanges(npc, playerResources, npcTokens, relationshipTier);
        if (!availableExchanges.Any())
        {
            throw new InvalidOperationException($"NPC {npc.Name} has no available exchanges");
        }

        return _orchestrator.CreateSession(npc, availableExchanges);
    }

    /// <summary>
    /// Get all available exchanges for an NPC
    /// HIGHLANDER: Accepts NPC object, not string identifier
    /// </summary>
    public List<ExchangeOption> GetAvailableExchanges(NPC npc, PlayerResourceState playerResources, Dictionary<ConnectionType, int> npcTokens, RelationshipTier relationshipTier)
    {
        if (npc == null)
        {
            return new List<ExchangeOption>();
        }

        // Get exchanges from GameWorld directly
        // Query GameWorld.NPCExchangeCards first, then check NPC.ExchangeDeck
        NPCExchangeCardEntry entry = _gameWorld.NPCExchangeCards.FirstOrDefault(x => x.Npc == npc);
        List<ExchangeCard> npcExchanges = entry.ExchangeCards;

        // Also check NPC.ExchangeDeck
        foreach (ExchangeCard card in npc.ExchangeDeck)
        {
            if (!npcExchanges.Contains(card))
            {
                npcExchanges.Add(card);
            }
        }

        // Get player's current Venue for domain validation
        Player player = _gameWorld.GetPlayer();
        Location currentSpot = _gameWorld.GetPlayerCurrentLocation();
        if (currentSpot == null)
        {
            throw new InvalidOperationException("Player has no current location");
        }

        // Extract individual capability flags for domain validation
        List<string> spotDomains = new List<string>();
        foreach (LocationCapability capability in Enum.GetValues(typeof(LocationCapability)))
        {
            if (capability != LocationCapability.None && currentSpot.Capabilities.HasFlag(capability))
            {
                spotDomains.Add(capability.ToString());
            }
        }

        // Validate each exchange
        List<ExchangeOption> validExchanges = new List<ExchangeOption>();

        foreach (ExchangeCard exchange in npcExchanges)
        {
            // Check if exchange is valid in current context
            ExchangeValidationResult validation = _validator.ValidateExchange(
                exchange,
                npc,
                playerResources,
                npcTokens,
                relationshipTier,
                spotDomains);

            if (validation.IsVisible)
            {
                validExchanges.Add(new ExchangeOption
                {
                    ExchangeId = exchange.Name,
                    Name = exchange.Name,
                    Description = exchange.Description,
                    Cost = FormatCost(exchange.GetCostAsList()),
                    Reward = FormatReward(exchange.GetRewardAsList()),
                    CanAfford = validation.CanAfford,
                    ExchangeCard = exchange,
                    ValidationResult = new ExchangeValidationResult
                    {
                        IsValid = validation.IsValid,
                        IsVisible = validation.IsVisible,
                        ValidationMessage = validation.ValidationMessage,
                        RequirementDetails = string.Join(", ", validation.MissingRequirements)
                    }
                });
            }
        }

        return validExchanges;
    }

    /// <summary>
    /// Execute an exchange with an NPC
    /// HIGHLANDER: Accepts NPC object, not string ID
    /// </summary>
    public async Task<ExchangeResult> ExecuteExchange(NPC npc, ExchangeCard exchange, PlayerResourceState playerResources, Dictionary<ConnectionType, int> npcTokens, RelationshipTier relationshipTier)
    {
        if (npc == null)
        {
            return new ExchangeResult
            {
                Success = false,
                Message = "NPC not found"
            };
        }

        // HIGHLANDER: Exchange object passed directly, no lookup needed
        if (exchange == null)
        {
            return new ExchangeResult
            {
                Success = false,
                Message = "Exchange not provided"
            };
        }

        // Validate exchange
        ExchangeValidationResult validation = _validator.ValidateExchange(
            exchange,
            npc,
            playerResources,
            npcTokens,
            relationshipTier,
            new List<string>());

        if (!validation.IsValid)
        {
            return new ExchangeResult
            {
                Success = false,
                Message = validation.ValidationMessage
            };
        }

        // EXECUTE the exchange immediately (apply costs and rewards)
        Player player = _gameWorld.GetPlayer();

        // Apply costs
        if (!ApplyExchangeCosts(exchange, player, npc))
        {
            return new ExchangeResult
            {
                Success = false,
                Message = "Failed to apply exchange costs"
            };
        }

        // Apply rewards
        Dictionary<ResourceType, int> rewardsGranted = ApplyExchangeRewards(exchange, player, npc);
        List<string> itemsGranted = ApplyExchangeItemRewards(exchange, player);

        // Track exchange history in GameWorld
        ExchangeHistoryEntry historyEntry = new ExchangeHistoryEntry
        {
            ExchangeId = exchange.Name,
            ExchangeName = exchange.Name,
            NpcId = npc.Name,  // HIGHLANDER: Use Name as natural key (no separate ID)
            Timestamp = DateTime.Now,
            Day = _gameWorld.CurrentDay,
            TimeBlock = _gameWorld.CurrentTimeBlock,
            WasSuccessful = true
        };
        _gameWorld.ExchangeHistory.Add(historyEntry);

        // Record use (increments TimesUsed, marks IsCompleted for SingleUse)
        exchange.RecordUse();

        // Note: SingleUse exchanges become unavailable via IsExhausted() check, no need to remove from list

        // Generate success message
        _messageSystem.AddSystemMessage($"Exchange with {npc.Name} completed successfully", SystemMessageTypes.Success);

        return new ExchangeResult
        {
            Success = true,
            Message = "Exchange completed",
            RewardsGranted = rewardsGranted,
            ItemsGranted = itemsGranted
        };
    }

    /// <summary>
    /// Check if player can afford an exchange
    /// </summary>
    public bool CanAffordExchange(ExchangeCard exchange, PlayerResourceState playerResources)
    {
        // Need to get npcTokens to call validator properly
        // This method should be updated to accept additional parameters
        return false; // Placeholder - this method needs to be called with proper orchestration
    }

    /// <summary>
    /// Get exchange history for an NPC
    /// HIGHLANDER: Accepts NPC object, not string ID
    /// </summary>
    public List<ExchangeHistoryEntry> GetExchangeHistory(NPC npc)
    {
        return _gameWorld.ExchangeHistory
            .Where(h => h.NpcId == npc.Name)
            .ToList();
    }

    /// <summary>
    /// Check if NPC has any exchanges available
    /// HIGHLANDER: Accepts NPC object, not string ID
    /// </summary>
    public bool HasExchangesAvailable(NPC npc)
    {
        // Placeholder implementation - would need proper orchestration
        PlayerResourceState playerResources = _gameWorld.GetPlayerResourceState();
        Dictionary<ConnectionType, int> npcTokens = new Dictionary<ConnectionType, int>();
        RelationshipTier relationshipTier = RelationshipTier.None;
        return GetAvailableExchanges(npc, playerResources, npcTokens, relationshipTier).Any();
    }

    /// <summary>
    /// Initialize NPC exchange inventories - no longer needed (data already in GameWorld)
    /// </summary>
    public void InitializeNPCExchanges()
    {
        // No-op: Exchange data now loaded directly into GameWorld by parsers
    }

    /// <summary>
    /// Add an exchange to an NPC's deck (for dynamic exchanges)
    /// HIGHLANDER: Accepts NPC object, not string ID
    /// </summary>
    public void AddExchangeToNPC(NPC npc, ExchangeCard exchange)
    {
        if (npc == null)
        {
            throw new ArgumentNullException(nameof(npc));
        }

        npc.ExchangeDeck.Add(exchange);
    }

    /// <summary>
    /// Remove an exchange from an NPC's deck - marks as exhausted instead
    /// HIGHLANDER: Accepts NPC and ExchangeCard objects, not string IDs
    /// </summary>
    public void RemoveExchangeFromNPC(NPC npc, ExchangeCard exchangeCard)
    {
        if (npc == null)
        {
            throw new ArgumentNullException(nameof(npc));
        }

        if (exchangeCard == null)
        {
            throw new ArgumentNullException(nameof(exchangeCard));
        }

        if (!npc.ExchangeDeck.Contains(exchangeCard))
        {
            throw new ArgumentException($"Exchange {exchangeCard.Name} not found in NPC {npc.Name} deck");
        }

        exchangeCard.RecordUse(); // Marks as complete/exhausted
    }

    /// <summary>
    /// Get exchange requirements for display
    /// </summary>
    public ExchangeRequirements GetExchangeRequirements(ExchangeCard exchange)
    {
        // Get first token requirement if exists
        TokenCount firstToken = exchange.Cost.TokenRequirements.FirstOrDefault();
        ConnectionType? firstTokenType = firstToken?.Type;
        int minimumTokens = firstToken?.Count ?? 0;

        return new ExchangeRequirements
        {
            MinimumTokens = minimumTokens,
            RequiredTokenType = firstTokenType,
            RequiredDomains = exchange.RequiredDomains,
            ConsumedItems = exchange.Cost.ConsumedItems.Select(i => i.Name).ToList(),
            TimeRestrictions = exchange.AvailableTimeBlocks
        };
    }

    // Helper methods

    private string FormatExchangeDescription(ExchangeCard exchange)
    {
        List<string> parts = new List<string>();

        List<ResourceAmount> costs = exchange.GetCostAsList();
        List<ResourceAmount> rewards = exchange.GetRewardAsList();

        if (costs.Any())
        {
            parts.Add($"Pay: {FormatCost(costs)}");
        }

        if (rewards.Any())
        {
            parts.Add($"Receive: {FormatReward(rewards)}");
        }

        return string.Join(" → ", parts);
    }

    private string FormatCost(List<ResourceAmount> costs)
    {
        if (!costs.Any()) return "Free";

        IEnumerable<string> parts = costs.Select(c => $"{c.Amount} {GetResourceName(c.Type)}");
        return string.Join(", ", parts);
    }

    private string FormatReward(List<ResourceAmount> rewards)
    {
        if (!rewards.Any()) return "Nothing";

        IEnumerable<string> parts = rewards.Select(r => $"{r.Amount} {GetResourceName(r.Type)}");
        return string.Join(", ", parts);
    }

    private string GetResourceName(ResourceType type)
    {
        return type switch
        {
            ResourceType.Coins => "coins",
            ResourceType.Health => "health",
            ResourceType.Hunger => "food",
            ResourceType.TrustToken => "trust",
            ResourceType.DiplomacyToken => "diplomacy",
            ResourceType.StatusToken => "status",
            ResourceType.ShadowToken => "shadow",
            _ => type.ToString().ToLower()
        };
    }

    // ========== EXECUTION LOGIC ==========

    /// <summary>
    /// Apply exchange costs to player (resources + items)
    /// </summary>
    private bool ApplyExchangeCosts(ExchangeCard exchange, Player player, NPC npc)
    {
        // Apply resource costs
        foreach (ResourceAmount cost in exchange.GetCostAsList())
        {
            switch (cost.Type)
            {
                case ResourceType.Coins:
                    if (player.Coins < cost.Amount)
                        return false;
                    player.Coins -= cost.Amount;
                    break;

                case ResourceType.Health:
                    if (player.Health < cost.Amount)
                        return false;
                    player.Health -= cost.Amount;
                    break;

                case ResourceType.Hunger:
                    player.Hunger = Math.Min(player.MaxHunger, player.Hunger + cost.Amount);
                    break;
            }
        }

        // Apply item costs (consume items from inventory)
        // HIGHLANDER: Use Item objects directly, no string resolution needed
        foreach (Item item in exchange.Cost.ConsumedItems)
        {
            if (!player.Inventory.Contains(item))
            {
                _messageSystem.AddSystemMessage($"Missing required item: {item.Name}", SystemMessageTypes.Danger);
                return false;
            }
            player.Inventory.Remove(item);
        }

        return true;
    }

    /// <summary>
    /// Apply exchange resource rewards to player
    /// </summary>
    private Dictionary<ResourceType, int> ApplyExchangeRewards(ExchangeCard exchange, Player player, NPC npc)
    {
        Dictionary<ResourceType, int> rewardsGranted = new Dictionary<ResourceType, int>();

        foreach (ResourceAmount reward in exchange.GetRewardAsList())
        {
            switch (reward.Type)
            {
                case ResourceType.Coins:
                    player.Coins += reward.Amount;
                    rewardsGranted[ResourceType.Coins] = reward.Amount;
                    break;

                case ResourceType.Health:
                    int healthBefore = player.Health;
                    player.Health = Math.Min(player.MaxHealth, player.Health + reward.Amount);
                    rewardsGranted[ResourceType.Health] = player.Health - healthBefore;
                    break;

                case ResourceType.Hunger:
                    int hungerBefore = player.Hunger;
                    player.Hunger = Math.Max(0, player.Hunger - reward.Amount);
                    rewardsGranted[ResourceType.Hunger] = hungerBefore - player.Hunger;
                    break;
            }
        }

        return rewardsGranted;
    }

    /// <summary>
    /// Apply exchange item rewards to player
    /// HIGHLANDER: Use Item objects, not string IDs
    /// </summary>
    private List<string> ApplyExchangeItemRewards(ExchangeCard exchange, Player player)
    {
        List<string> itemsGranted = new List<string>();

        foreach (Item item in exchange.GetItemRewards())
        {
            player.Inventory.Add(item);
            itemsGranted.Add(item.Name);
        }

        return itemsGranted;
    }
}

/// <summary>
/// Result of executing an exchange
/// </summary>
public class ExchangeResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public Dictionary<ResourceType, int> CostsApplied { get; set; }
    public Dictionary<ResourceType, int> RewardsGranted { get; set; }
    public List<string> ItemsGranted { get; set; } = new List<string>();
    public List<string> SideEffects { get; set; }
    public ExchangeOperationData OperationData { get; set; }
}

/// <summary>
/// Data for GameFacade to execute an exchange operation
/// </summary>
public class ExchangeOperationData
{
    public List<ResourceAmount> Costs { get; set; } = new List<ResourceAmount>();
    public List<ResourceAmount> Rewards { get; set; } = new List<ResourceAmount>();
    public List<string> ItemRewards { get; set; } = new List<string>();
    public bool AdvancesTime { get; set; }
    public int TimeAdvancementHours { get; set; }
    public bool AffectsRelationship { get; set; }
    public int FlowModifier { get; set; }
    public bool ConsumesPatience { get; set; }
    public int PatienceCost { get; set; }
    public string UnlocksExchangeId { get; set; }
    public string TriggerEvent { get; set; }
    /// <summary>
    /// NPC this exchange is with
    /// HIGHLANDER: Object reference only, no string ID
    /// </summary>
    public NPC Npc { get; set; }
    public string ExchangeId { get; set; }
    public bool IsUnique { get; set; }
    public ConnectionState? ConnectionStateChange { get; set; }
}

/// <summary>
/// Exchange requirements for display
/// </summary>
public class ExchangeRequirements
{
    public int MinimumTokens { get; set; }
    public ConnectionType? RequiredTokenType { get; set; }
    public List<string> RequiredDomains { get; set; }
    public List<string> ConsumedItems { get; set; }
    public List<TimeBlocks> TimeRestrictions { get; set; }
}

/// <summary>
/// Historical record of an exchange
/// </summary>
public class ExchangeHistoryEntry
{
    public string ExchangeId { get; set; }
    public string ExchangeName { get; set; }
    public string NpcId { get; set; }  // ✅ Strongly-typed property for NPC reference (no ID parsing)
    public DateTime Timestamp { get; set; }
    public int Day { get; set; }
    public TimeBlocks TimeBlock { get; set; }
    public bool WasSuccessful { get; set; }
}

