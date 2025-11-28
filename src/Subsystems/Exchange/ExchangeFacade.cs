/// <summary>
/// Public facade for all exchange-related operations.
/// Handles resource trades, exchange validation, and NPC inventory management.
/// This is the ONLY public interface for the Exchange subsystem.
/// TWO PILLARS: Delegates all mutations to RewardApplicationService
/// </summary>
public class ExchangeFacade
{
    private readonly GameWorld _gameWorld;
    private readonly ExchangeOrchestrator _orchestrator;
    private readonly ExchangeValidator _validator;
    private readonly ExchangeProcessor _processor;
    private readonly RewardApplicationService _rewardApplicationService;

    // External dependencies
    private readonly TimeManager _timeManager;
    private readonly MessageSystem _messageSystem;

    public ExchangeFacade(
        GameWorld gameWorld,
        ExchangeOrchestrator orchestrator,
        ExchangeValidator validator,
        ExchangeProcessor processor,
        TimeManager timeManager,
        MessageSystem messageSystem,
        RewardApplicationService rewardApplicationService)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
        _rewardApplicationService = rewardApplicationService ?? throw new ArgumentNullException(nameof(rewardApplicationService));
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
        List<TokenCount> npcTokens = new List<TokenCount>(); // Would need TokenFacade
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
    public List<ExchangeOption> GetAvailableExchanges(NPC npc, PlayerResourceState playerResources, List<TokenCount> npcTokens, RelationshipTier relationshipTier)
    {
        if (npc == null)
        {
            return new List<ExchangeOption>();
        }

        // Get exchanges from GameWorld directly
        // Query GameWorld.NPCExchangeCards first, then check NPC.ExchangeDeck
        NPCExchangeCardEntry entry = _gameWorld.NPCExchangeCards.FirstOrDefault(x => x.Npc == npc);
        List<ExchangeCard> npcExchanges = entry?.ExchangeCards != null
            ? new List<ExchangeCard>(entry.ExchangeCards)
            : new List<ExchangeCard>();

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

        // Extract orthogonal categorical properties for domain validation
        List<string> spotDomains = new List<string>();
        if (currentSpot.Environment != default) spotDomains.Add(currentSpot.Environment.ToString());
        if (currentSpot.Setting != default) spotDomains.Add(currentSpot.Setting.ToString());
        if (currentSpot.Role != default) spotDomains.Add(currentSpot.Role.ToString());
        if (currentSpot.Purpose != default) spotDomains.Add(currentSpot.Purpose.ToString());
        if (currentSpot.Privacy != default) spotDomains.Add(currentSpot.Privacy.ToString());
        if (currentSpot.Safety != default) spotDomains.Add(currentSpot.Safety.ToString());
        if (currentSpot.Activity != default) spotDomains.Add(currentSpot.Activity.ToString());

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
                        MissingRequirements = validation.MissingRequirements
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
    public async Task<ExchangeResult> ExecuteExchange(NPC npc, ExchangeCard exchange, PlayerResourceState playerResources, List<TokenCount> npcTokens, RelationshipTier relationshipTier)
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
        if (!await ApplyExchangeCosts(exchange, player, npc))
        {
            return new ExchangeResult
            {
                Success = false,
                Message = "Failed to apply exchange costs"
            };
        }

        // Apply rewards (DOMAIN COLLECTION: strongly-typed result, no tuple)
        ExchangeRewardsApplied appliedRewards = await ApplyExchangeRewards(exchange, player, npc);

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
            RewardsGranted = appliedRewards.ResourcesGranted,
            ItemsGranted = appliedRewards.ItemsGranted
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
        List<TokenCount> npcTokens = new List<TokenCount>();
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
    private async Task<bool> ApplyExchangeCosts(ExchangeCard exchange, Player player, NPC npc)
    {
        // TWO PILLARS: Build Consequence from costs and validate via CompoundRequirement
        int coinCost = 0;
        int healthCost = 0;
        int hungerIncrease = 0;

        foreach (ResourceAmount cost in exchange.GetCostAsList())
        {
            switch (cost.Type)
            {
                case ResourceType.Coins:
                    coinCost += cost.Amount;
                    break;
                case ResourceType.Health:
                    healthCost += cost.Amount;
                    break;
                case ResourceType.Hunger:
                    hungerIncrease += cost.Amount;
                    break;
            }
        }

        // Validate affordability
        if (player.Coins < coinCost || player.Health < healthCost)
        {
            return false;
        }

        // Validate item availability BEFORE applying any costs
        foreach (Item item in exchange.Cost.ConsumedItems)
        {
            if (!player.Inventory.Contains(item))
            {
                _messageSystem.AddSystemMessage($"Missing required item: {item.Name}", SystemMessageTypes.Danger);
                return false;
            }
        }

        // TWO PILLARS: Apply ALL costs via single Consequence (resources + item removal)
        Consequence costConsequence = new Consequence
        {
            Coins = -coinCost,
            Health = -healthCost,
            Hunger = hungerIncrease,  // Hunger increase is positive
            ItemsToRemove = exchange.Cost.ConsumedItems.ToList()
        };
        await _rewardApplicationService.ApplyConsequence(costConsequence, null);

        return true;
    }

    /// <summary>
    /// Apply exchange resource and item rewards to player
    /// TWO PILLARS: Delegates ALL mutations to RewardApplicationService via single Consequence
    /// DOMAIN COLLECTION: Returns List-based strongly-typed result (no Dictionary/tuple)
    /// </summary>
    private async Task<ExchangeRewardsApplied> ApplyExchangeRewards(ExchangeCard exchange, Player player, NPC npc)
    {
        List<ResourceAmount> rewardsGranted = new List<ResourceAmount>();
        List<Item> itemsGranted = new List<Item>();

        // TWO PILLARS: Build Consequence from rewards
        int coinReward = 0;
        int healthReward = 0;
        int hungerDecrease = 0;
        List<Item> itemRewards = new List<Item>();

        foreach (ResourceAmount reward in exchange.GetRewardAsList())
        {
            switch (reward.Type)
            {
                case ResourceType.Coins:
                    coinReward += reward.Amount;
                    rewardsGranted.Add(new ResourceAmount(ResourceType.Coins, reward.Amount));
                    break;
                case ResourceType.Health:
                    healthReward += reward.Amount;
                    rewardsGranted.Add(new ResourceAmount(ResourceType.Health, reward.Amount));
                    break;
                case ResourceType.Hunger:
                    hungerDecrease += reward.Amount;
                    rewardsGranted.Add(new ResourceAmount(ResourceType.Hunger, reward.Amount));
                    break;
            }
        }

        // Collect item rewards (HIGHLANDER: Item objects, not strings)
        foreach (Item item in exchange.GetItemRewards())
        {
            itemRewards.Add(item);
            itemsGranted.Add(item);
        }

        // TWO PILLARS: Apply ALL rewards via single Consequence (resources + items)
        Consequence rewardConsequence = new Consequence
        {
            Coins = coinReward,
            Health = healthReward,
            Hunger = -hungerDecrease,  // Hunger decrease is negative (good for player)
            Items = itemRewards
        };
        await _rewardApplicationService.ApplyConsequence(rewardConsequence, null);

        return new ExchangeRewardsApplied { ResourcesGranted = rewardsGranted, ItemsGranted = itemsGranted };
    }
}

/// <summary>
/// Result of applying exchange rewards (DOMAIN COLLECTION: no Dictionary/tuple)
/// </summary>
public class ExchangeRewardsApplied
{
    public List<ResourceAmount> ResourcesGranted { get; set; } = new List<ResourceAmount>();
    public List<Item> ItemsGranted { get; set; } = new List<Item>();
}

/// <summary>
/// Result of executing an exchange
/// DOMAIN COLLECTION: List-based strongly-typed properties (no Dictionary)
/// HIGHLANDER: Item objects, not string IDs
/// </summary>
public class ExchangeResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public List<ResourceAmount> CostsApplied { get; set; } = new List<ResourceAmount>();
    public List<ResourceAmount> RewardsGranted { get; set; } = new List<ResourceAmount>();
    public List<Item> ItemsGranted { get; set; } = new List<Item>();
    public List<string> SideEffects { get; set; } = new List<string>();
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

