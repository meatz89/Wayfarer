using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    private readonly ExchangeInventory _inventory;

    // External dependencies
    private readonly TimeManager _timeManager;
    private readonly MessageSystem _messageSystem;

    public ExchangeFacade(
        GameWorld gameWorld,
        ExchangeOrchestrator orchestrator,
        ExchangeValidator validator,
        ExchangeProcessor processor,
        ExchangeInventory inventory,
        TimeManager timeManager,
        MessageSystem messageSystem)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
    }

    /// <summary>
    /// Create an exchange session with an NPC
    /// </summary>
    public ExchangeSession CreateExchangeSession(string npcId)
    {
        NPC? npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == npcId);
        if (npc == null)
        {
            throw new ArgumentException($"NPC with ID {npcId} not found");
        }

        // Get available exchanges for this NPC - placeholder parameters for now
        PlayerResourceState playerResources = _gameWorld.GetPlayerResourceState();
        TimeBlocks timeBlock = _timeManager.GetCurrentTimeBlock();
        Dictionary<ConnectionType, int> npcTokens = new Dictionary<ConnectionType, int>(); // Would need TokenFacade
        RelationshipTier relationshipTier = RelationshipTier.None;
        List<ExchangeOption> availableExchanges = GetAvailableExchanges(npcId, playerResources, npcTokens, relationshipTier);
        if (!availableExchanges.Any())
        {
            return null;
        }

        return _orchestrator.CreateSession(npc, availableExchanges);
    }

    /// <summary>
    /// Get all available exchanges for an NPC
    /// </summary>
    public List<ExchangeOption> GetAvailableExchanges(string npcId, PlayerResourceState playerResources, Dictionary<ConnectionType, int> npcTokens, RelationshipTier relationshipTier)
    {
        NPC? npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == npcId);
        if (npc == null)
        {
            return new List<ExchangeOption>();
        }

        // Get exchanges from NPC's inventory
        List<ExchangeCard> npcExchanges = _inventory.GetNPCExchanges(npcId);

        // Get player's current Venue for domain validation
        Player player = _gameWorld.GetPlayer();
        Location currentSpot = _gameWorld.WorldState.locations
            .FirstOrDefault(s => s.Id == player.CurrentLocation?.Id);

        // Convert SpotProperties to domain strings for validation
        List<string> spotDomains = currentSpot?.LocationProperties?
            .Select(p => p.ToString())
            .ToList() ?? new List<string>();

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
                    ExchangeId = exchange.Id,
                    Name = exchange.Name ?? "Trade",
                    Description = exchange.Description ?? FormatExchangeDescription(exchange),
                    Cost = FormatCost(exchange.GetCostAsList()),
                    Reward = FormatReward(exchange.GetRewardAsList()),
                    CanAfford = validation.CanAfford,
                    ExchangeCard = exchange,
                    ValidationResult = new global::ExchangeValidationResult
                    {
                        IsValid = validation.IsValid,
                        IsVisible = validation.IsVisible,
                        ValidationMessage = validation.ValidationMessage,
                        RequirementDetails = string.Join(", ", validation.MissingRequirements ?? new List<string>())
                    }
                });
            }
        }

        return validExchanges;
    }

    /// <summary>
    /// Execute an exchange with an NPC
    /// </summary>
    public async Task<ExchangeResult> ExecuteExchange(string npcId, string exchangeId, PlayerResourceState playerResources, Dictionary<ConnectionType, int> npcTokens, RelationshipTier relationshipTier)
    {
        // Get NPC
        NPC? npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == npcId);
        if (npc == null)
        {
            return new ExchangeResult
            {
                Success = false,
                Message = "NPC not found"
            };
        }

        // Get exchange data
        Console.WriteLine($"[ExchangeFacade] Attempting to get exchange - NpcId: '{npcId}', ExchangeId: '{exchangeId}'");
        ExchangeCard? exchange = _inventory.GetExchange(npcId, exchangeId);
        if (exchange == null)
        {
            Console.WriteLine($"[ExchangeFacade] Exchange not found! Available NPCs: {string.Join(", ", _inventory.GetNPCsWithExchanges())}");
            List<ExchangeCard> availableExchanges = _inventory.GetNPCExchanges(npcId);
            Console.WriteLine($"[ExchangeFacade] Available exchanges for '{npcId}': {string.Join(", ", availableExchanges.Select(e => e.Id))}");
            return new ExchangeResult
            {
                Success = false,
                Message = "Exchange not found"
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
                Message = validation.ValidationMessage ?? "Cannot afford this exchange"
            };
        }

        // Process the exchange - return operation data for GameFacade to execute
        ExchangeOperationData operationData = _processor.PrepareExchangeOperation(exchange, npc, playerResources);
        ExchangeResult result = new ExchangeResult
        {
            Success = true,
            Message = "Exchange ready for execution",
            OperationData = operationData
        };

        // Track exchange history if successful
        if (result.Success)
        {
            _inventory.RecordExchange(npcId, exchangeId);

            // Check if this was a single-use exchange
            if (exchange.SingleUse)
            {
                _inventory.RemoveExchange(npcId, exchangeId);
            }
        }

        return result;
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
    /// </summary>
    public List<ExchangeHistoryEntry> GetExchangeHistory(string npcId)
    {
        return _inventory.GetExchangeHistory(npcId);
    }

    /// <summary>
    /// Check if NPC has any exchanges available
    /// </summary>
    public bool HasExchangesAvailable(string npcId)
    {
        // Placeholder implementation - would need proper orchestration
        PlayerResourceState playerResources = _gameWorld.GetPlayerResourceState();
        Dictionary<ConnectionType, int> npcTokens = new Dictionary<ConnectionType, int>();
        RelationshipTier relationshipTier = RelationshipTier.None;
        return GetAvailableExchanges(npcId, playerResources, npcTokens, relationshipTier).Any();
    }

    /// <summary>
    /// Initialize NPC exchange inventories from GameWorld data
    /// </summary>
    public void InitializeNPCExchanges()
    {
        _inventory.InitializeFromGameWorld(_gameWorld);
    }

    /// <summary>
    /// Add an exchange to an NPC's inventory (for dynamic exchanges)
    /// </summary>
    public void AddExchangeToNPC(string npcId, ExchangeCard exchange)
    {
        _inventory.AddExchange(npcId, exchange);
    }

    /// <summary>
    /// Remove an exchange from an NPC's inventory
    /// </summary>
    public void RemoveExchangeFromNPC(string npcId, string exchangeId)
    {
        _inventory.RemoveExchange(npcId, exchangeId);
    }

    /// <summary>
    /// Get exchange requirements for display
    /// </summary>
    public ExchangeRequirements GetExchangeRequirements(ExchangeCard exchange)
    {
        // Get first token requirement if exists
        ConnectionType? firstTokenType = exchange.Cost?.TokenRequirements?.Keys.FirstOrDefault();
        int minimumTokens = exchange.Cost?.TokenRequirements?.Values.FirstOrDefault() ?? 0;

        return new ExchangeRequirements
        {
            MinimumTokens = minimumTokens,
            RequiredTokenType = firstTokenType,
            RequiredDomains = exchange.RequiredDomains,
            RequiredItems = exchange.Cost?.RequiredItemIds?.ToList() ?? new List<string>(),
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
    public string NPCId { get; set; }
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
    public List<string> RequiredItems { get; set; }
    public List<TimeBlocks> TimeRestrictions { get; set; }
}

/// <summary>
/// Historical record of an exchange
/// </summary>
public class ExchangeHistoryEntry
{
    public string ExchangeId { get; set; }
    public string ExchangeName { get; set; }
    public DateTime Timestamp { get; set; }
    public int Day { get; set; }
    public TimeBlocks TimeBlock { get; set; }
    public bool WasSuccessful { get; set; }
}

