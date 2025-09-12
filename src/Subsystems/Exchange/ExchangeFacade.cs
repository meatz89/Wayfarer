using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wayfarer.Subsystems.ResourceSubsystem;
using Wayfarer.Subsystems.TokenSubsystem;

namespace Wayfarer.Subsystems.ExchangeSubsystem
{
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
        private readonly ResourceFacade _resourceFacade;
        private readonly TokenFacade _tokenFacade;
        private readonly TimeManager _timeManager;
        private readonly MessageSystem _messageSystem;

        public ExchangeFacade(
            GameWorld gameWorld,
            ExchangeOrchestrator orchestrator,
            ExchangeValidator validator,
            ExchangeProcessor processor,
            ExchangeInventory inventory,
            ResourceFacade resourceFacade,
            TokenFacade tokenFacade,
            TimeManager timeManager,
            MessageSystem messageSystem)
        {
            _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
            _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _resourceFacade = resourceFacade ?? throw new ArgumentNullException(nameof(resourceFacade));
            _tokenFacade = tokenFacade ?? throw new ArgumentNullException(nameof(tokenFacade));
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

            // Get available exchanges for this NPC
            List<ExchangeOption> availableExchanges = GetAvailableExchanges(npcId);
            if (!availableExchanges.Any())
            {
                return null;
            }

            return _orchestrator.CreateSession(npc, availableExchanges);
        }

        /// <summary>
        /// Get all available exchanges for an NPC
        /// </summary>
        public List<ExchangeOption> GetAvailableExchanges(string npcId)
        {
            NPC? npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == npcId);
            if (npc == null)
            {
                return new List<ExchangeOption>();
            }

            // Get exchanges from NPC's inventory
            List<ExchangeData> npcExchanges = _inventory.GetNPCExchanges(npcId);
            
            // Get player's current location for domain validation
            Player player = _gameWorld.GetPlayer();
            LocationSpot currentSpot = _gameWorld.WorldState.locationSpots
                .FirstOrDefault(s => s.SpotID == player.CurrentLocationSpot?.SpotID);
            
            // Convert SpotProperties to domain strings for validation
            List<string> spotDomains = currentSpot?.SpotProperties?
                .Select(p => p.ToString())
                .ToList() ?? new List<string>();

            // Validate each exchange
            List<ExchangeOption> validExchanges = new List<ExchangeOption>();
            PlayerResourceState playerResources = _gameWorld.GetPlayerResourceState();

            foreach (ExchangeData exchange in npcExchanges)
            {
                // Check if exchange is valid in current context
                ExchangeValidationResult validation = _validator.ValidateExchange(
                    exchange, 
                    npc, 
                    playerResources, 
                    spotDomains);

                if (validation.IsVisible)
                {
                    validExchanges.Add(new ExchangeOption
                    {
                        ExchangeId = exchange.Id,
                        Name = exchange.ExchangeName ?? "Trade",
                        Description = exchange.Description ?? FormatExchangeDescription(exchange),
                        Cost = FormatCost(exchange.Costs),
                        Reward = FormatReward(exchange.Rewards),
                        CanAfford = validation.CanAfford,
                        ExchangeData = exchange,
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
        public async Task<ExchangeResult> ExecuteExchange(string npcId, string exchangeId)
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
            ExchangeData? exchange = _inventory.GetExchange(npcId, exchangeId);
            if (exchange == null)
            {
                Console.WriteLine($"[ExchangeFacade] Exchange not found! Available NPCs: {string.Join(", ", _inventory.GetNPCsWithExchanges())}");
                var availableExchanges = _inventory.GetNPCExchanges(npcId);
                Console.WriteLine($"[ExchangeFacade] Available exchanges for '{npcId}': {string.Join(", ", availableExchanges.Select(e => e.Id))}");
                return new ExchangeResult
                {
                    Success = false,
                    Message = "Exchange not found"
                };
            }

            // Validate exchange
            PlayerResourceState playerResources = _gameWorld.GetPlayerResourceState();
            ExchangeValidationResult validation = _validator.ValidateExchange(
                exchange, 
                npc, 
                playerResources, 
                new List<string>());

            if (!validation.IsValid)
            {
                return new ExchangeResult
                {
                    Success = false,
                    Message = validation.ValidationMessage ?? "Cannot afford this exchange"
                };
            }

            // Process the exchange
            ExchangeResult result = await _processor.ProcessExchange(exchange, npc);

            // Track exchange history if successful
            if (result.Success)
            {
                _inventory.RecordExchange(npcId, exchangeId);
                
                // Check if this was a unique exchange
                if (exchange.IsUnique)
                {
                    _inventory.RemoveExchange(npcId, exchangeId);
                }
            }

            return result;
        }

        /// <summary>
        /// Check if player can afford an exchange
        /// </summary>
        public bool CanAffordExchange(ExchangeData exchange)
        {
            PlayerResourceState playerResources = _gameWorld.GetPlayerResourceState();
            return _validator.CanAffordExchange(exchange, playerResources);
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
            return GetAvailableExchanges(npcId).Any();
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
        public void AddExchangeToNPC(string npcId, ExchangeData exchange)
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
        public ExchangeRequirements GetExchangeRequirements(ExchangeData exchange)
        {
            return new ExchangeRequirements
            {
                MinimumTokens = exchange.MinimumTokensRequired,
                RequiredTokenType = exchange.RequiredTokenType,
                RequiredDomains = exchange.RequiredDomains,
                RequiredItems = exchange.RequiredItems,
                TimeRestrictions = exchange.TimeRestrictions
            };
        }

        // Helper methods

        private string FormatExchangeDescription(ExchangeData exchange)
        {
            List<string> parts = new List<string>();
            
            if (exchange.Costs.Any())
            {
                parts.Add($"Pay: {FormatCost(exchange.Costs)}");
            }
            
            if (exchange.Rewards.Any())
            {
                parts.Add($"Receive: {FormatReward(exchange.Rewards)}");
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
                ResourceType.Attention => "attention",
                ResourceType.TrustToken => "trust",
                ResourceType.CommerceToken => "commerce",
                ResourceType.StatusToken => "status",
                ResourceType.ShadowToken => "shadow",
                _ => type.ToString().ToLower()
            };
        }
    }

    /// <summary>
    /// Represents an active exchange session
    /// </summary>
    public class ExchangeSession
    {
        public string SessionId { get; set; }
        public NPC NPC { get; set; }
        public List<ExchangeOption> AvailableExchanges { get; set; }
        public DateTime StartTime { get; set; }
        public bool IsActive { get; set; }
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

}