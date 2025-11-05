using Microsoft.AspNetCore.Components;

namespace Wayfarer.Pages.Components
{
    /// <summary>
    /// Exchange screen component that handles resource trades with NPCs or locations.
    /// Completely separate from Conversation system - no shared mechanics.
    /// 
    /// ARCHITECTURAL PRINCIPLES:
    /// - Follows the authoritative parent pattern - receives GameScreen via CascadingParameter
    /// - ExchangeContext passed as Parameter from parent (created by GameFacade)
    /// - All game state mutations go through ExchangeFacade
    /// - No conversation mechanics (no flow, no rapport, no patience)
    /// - Simple card selection with TRADE/LEAVE actions
    /// </summary>
    public class ExchangeContentBase : ComponentBase
    {
        [Parameter] public ExchangeContext Context { get; set; }
        [Parameter] public EventCallback OnExchangeEnd { get; set; }
        [CascadingParameter] public GameScreenBase GameScreen { get; set; }

        [Inject] protected ExchangeFacade ExchangeFacade { get; set; }
        [Inject] protected GameFacade GameFacade { get; set; }

        // Component state
        protected string SelectedExchangeId { get; set; }
        protected string CurrentNarrative { get; set; }
        protected bool IsProcessingTrade { get; set; } = false;
        protected ExchangeResult LastResult { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            GenerateInitialNarrative();
        }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            if (Context == null)
                throw new InvalidOperationException("Exchange context is required");
            if (Context.Session == null)
                throw new InvalidOperationException("Exchange session is required");

            // Reset selection when context changes
            if (Context.Session.SessionId != LastContextId)
            {
                SelectedExchangeId = null;
                LastResult = null;
                GenerateInitialNarrative();
                LastContextId = Context.Session.SessionId;
            }
        }

        private string LastContextId { get; set; }

        /// <summary>
        /// Gets the Venue context string for the header.
        /// </summary>
        protected string GetLocationContext()
        {
            if (Context == null)
                throw new InvalidOperationException("Exchange context is required");
            if (Context.LocationInfo != null)
            {
                string timeStr = GetTimeBlockDisplay(Context.CurrentTimeBlock);
                return $"{timeStr} - {Context.LocationInfo.VenueName}";
            }
            return "Unknown Location";
        }

        /// <summary>
        /// Gets a display string for the current time block.
        /// </summary>
        protected string GetTimeBlockDisplay(TimeBlocks timeBlock)
        {
            // Convert time block to approximate time
            return timeBlock switch
            {
                TimeBlocks.Morning => "Morning",
                TimeBlocks.Midday => "Midday",
                TimeBlocks.Afternoon => "Afternoon",
                TimeBlocks.Evening => "Evening",
                _ => "Unknown Time"
            };
        }

        /// <summary>
        /// Gets the NPC status line for display.
        /// </summary>
        protected string GetNpcStatusLine()
        {
            if (Context == null)
                throw new InvalidOperationException("Exchange context is required");
            if (Context.NpcInfo == null)
                return "";

            List<string> parts = new List<string>();

            // Add time-specific status
            parts.Add($"{GetTimeBlockDisplay(Context.CurrentTimeBlock)} business period");

            // Add merchant status if they have diplomacy tokens
            if (GetDiplomacyTokens() > 0)
            {
                parts.Add("Mercantile personality");
            }

            return string.Join(" • ", parts);
        }

        /// <summary>
        /// Gets the number of diplomacy tokens with this NPC.
        /// </summary>
        protected int GetDiplomacyTokens()
        {
            if (Context == null)
                throw new InvalidOperationException("Exchange context is required");
            if (Context.NpcInfo == null || Context.NpcInfo.TokenCounts == null)
                return 0;

            return Context.NpcInfo.TokenCounts.GetValueOrDefault(ConnectionType.Diplomacy, 0);
        }

        /// <summary>
        /// Gets the discount description based on diplomacy tokens.
        /// </summary>
        protected string GetDiscountDescription()
        {
            int tokens = GetDiplomacyTokens();
            if (tokens <= 0)
                return "No discount";

            // 5% discount per diplomacy token
            int discount = Math.Min(tokens * 5, 25); // Cap at 25%
            return $"-{discount}% discount on all prices";
        }

        /// <summary>
        /// Generates initial narrative when entering exchange mode.
        /// </summary>
        protected void GenerateInitialNarrative()
        {
            if (Context == null)
                throw new InvalidOperationException("Exchange context is required");

            if (Context.NpcInfo != null)
            {
                // NPC-based exchange
                CurrentNarrative = GenerateNpcGreeting();
            }
            else if (Context.LocationInfo != null)
            {
                // Location-based exchange
                CurrentNarrative = $"You examine the available services at {Context.LocationInfo.VenueName}.";
            }
            else
            {
                CurrentNarrative = "Exchange options are available.";
            }
        }

        /// <summary>
        /// Generates a greeting from the NPC.
        /// </summary>
        protected string GenerateNpcGreeting()
        {
            if (Context == null)
                throw new InvalidOperationException("Exchange context is required");

            string npcName = Context.NpcInfo != null ? Context.NpcInfo.Name : "The merchant";

            if (Context.Session == null)
                throw new InvalidOperationException("Exchange session is required");
            if (Context.Session.AvailableExchanges == null)
                throw new InvalidOperationException("Available exchanges list is required");

            bool hasQueue = Context.Session.AvailableExchanges.Any(e => e.ExchangeCard != null && e.ExchangeCard.ExchangeType == ExchangeType.Service);

            if (hasQueue)
            {
                return $"{npcName} glances at you. \"I see you're a courier. Need supplies? Or perhaps you have time for a delivery?\"";
            }
            else
            {
                return $"{npcName} looks up. \"What can I do for you today?\"";
            }
        }

        /// <summary>
        /// Gets the list of available exchanges.
        /// </summary>
        protected List<ExchangeCard> GetAvailableExchanges()
        {
            if (Context == null)
                throw new InvalidOperationException("Exchange context is required");
            return Context.GetAvailableExchanges();
        }

        /// <summary>
        /// Gets the count of available exchanges.
        /// </summary>
        protected int GetAvailableCount()
        {
            return GetAvailableExchanges().Count;
        }

        /// <summary>
        /// Selects an exchange card.
        /// </summary>
        protected void SelectExchange(ExchangeCard exchange)
        {
            if (exchange == null || !Context.CanAfford(exchange))
                return;

            // Toggle selection
            if (SelectedExchangeId == exchange.Id)
            {
                SelectedExchangeId = null;
            }
            else
            {
                SelectedExchangeId = exchange.Id;
            }

            StateHasChanged();
        }

        /// <summary>
        /// Checks if a trade can be executed.
        /// </summary>
        protected bool CanExecuteTrade()
        {
            if (Context == null)
                throw new InvalidOperationException("Exchange context is required");

            if (IsProcessingTrade)
                return false;

            if (string.IsNullOrEmpty(SelectedExchangeId))
                return false;

            ExchangeCard exchange = GetAvailableExchanges().FirstOrDefault(e => e.Id == SelectedExchangeId);
            return exchange != null && Context.CanAfford(exchange);
        }

        /// <summary>
        /// Gets the details text for the trade action button.
        /// </summary>
        protected string GetTradeActionDetails()
        {
            if (IsProcessingTrade)
                return "Processing trade...";

            if (string.IsNullOrEmpty(SelectedExchangeId))
                return "Select an exchange";

            return "Execute selected exchange";
        }

        /// <summary>
        /// Executes the selected trade.
        /// </summary>
        protected async Task ExecuteTrade()
        {
            if (!CanExecuteTrade())
                return;

            IsProcessingTrade = true;
            StateHasChanged();

            try
            {
                if (Context == null)
                    throw new InvalidOperationException("Exchange context is required");

                // Execute the exchange through the facade
                string npcId = Context.NpcInfo != null ? Context.NpcInfo.NpcId : "";

                // Get required parameters
                if (Context.PlayerResources == null)
                    throw new InvalidOperationException("Player resources are required");
                PlayerResourceState playerResources = Context.PlayerResources;
                Dictionary<ConnectionType, int> npcTokens = Context.NpcInfo != null && Context.NpcInfo.TokenCounts != null
                    ? Context.NpcInfo.TokenCounts
                    : new Dictionary<ConnectionType, int>();
                RelationshipTier relationshipTier = RelationshipTier.None; // Default for now

                LastResult = await ExchangeFacade.ExecuteExchange(npcId, SelectedExchangeId, playerResources, npcTokens, relationshipTier);

                if (LastResult == null)
                    throw new InvalidOperationException("Exchange execution returned null result");

                if (LastResult.Success)
                {
                    // Generate success narrative
                    CurrentNarrative = GenerateSuccessNarrative(LastResult);

                    // Clear selection
                    SelectedExchangeId = null;

                    // Update context with new state
                    Context = await GameFacade.CreateExchangeContext(Context.NpcInfo != null ? Context.NpcInfo.NpcId : null);
                }
                else
                {
                    // Generate failure narrative
                    CurrentNarrative = GenerateFailureNarrative(LastResult);
                }
            }
            finally
            {
                IsProcessingTrade = false;
                StateHasChanged();
            }
        }

        /// <summary>
        /// Generates narrative for successful exchange.
        /// </summary>
        protected string GenerateSuccessNarrative(ExchangeResult result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));
            if (!result.Success)
                throw new InvalidOperationException("Cannot generate success narrative for failed exchange");

            string rewardDesc = "";
            if ((result.RewardsGranted != null && result.RewardsGranted.Any()) ||
                (result.ItemsGranted != null && result.ItemsGranted.Any()))
            {
                IEnumerable<string> rewards = result.RewardsGranted != null
                    ? result.RewardsGranted.Select(kvp => $"{kvp.Value} {kvp.Key}")
                    : new List<string>();
                List<string> items = result.ItemsGranted != null ? result.ItemsGranted : new List<string>();
                IEnumerable<string> allRewards = rewards.Concat(items);
                rewardDesc = string.Join(", ", allRewards);
            }

            if (Context == null)
                throw new InvalidOperationException("Exchange context is required");

            if (Context.NpcInfo != null)
            {
                return $"{Context.NpcInfo.Name} nods. \"Good doing business with you. You receive {rewardDesc}.\"";
            }
            else
            {
                return $"Exchange completed successfully. You receive {rewardDesc}.";
            }
        }

        /// <summary>
        /// Generates narrative for failed exchange.
        /// </summary>
        protected string GenerateFailureNarrative(ExchangeResult result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            if (string.IsNullOrEmpty(result.Message))
                return "The exchange could not be completed.";

            return result.Message;
        }

        /// <summary>
        /// Exits the exchange and returns to location.
        /// </summary>
        protected async Task ExitExchange()
        {
            if (Context == null)
                throw new InvalidOperationException("Exchange context is required");

            // End the exchange session
            if (Context.Session != null)
            {
                Context.Session.EndSession();
            }

            // Notify parent
            await OnExchangeEnd.InvokeAsync();

            // Return to Venue through GameScreen
            await GameScreen.ReturnToLocation();
        }

        /// <summary>
        /// Gets the cost display for an exchange.
        /// </summary>
        protected string GetCostDisplay(ExchangeCard exchange)
        {
            if (exchange == null)
                throw new ArgumentNullException(nameof(exchange));
            if (exchange.Cost == null)
                return "Free";

            List<string> parts = new List<string>();

            // Apply diplomacy discount if applicable
            int discount = GetCommerceDiscount();

            foreach (ResourceAmount resource in exchange.Cost.Resources)
            {
                int amount = resource.Amount;
                if (resource.Type == ResourceType.Coins && discount > 0)
                {
                    int discounted = (int)(amount * (1 - discount / 100.0));
                    if (discounted < amount)
                    {
                        parts.Add($"<span class='original-cost'>{amount}</span>{discounted} {resource.Type}");
                        continue;
                    }
                }
                parts.Add($"{amount} {resource.Type}");
            }

            // Add token requirements
            foreach (KeyValuePair<ConnectionType, int> token in exchange.Cost.TokenRequirements)
            {
                parts.Add($"Requires {token.Value} {token.Key} tokens");
            }

            // Add item requirements
            foreach (string itemId in exchange.Cost.ConsumedItemIds)
            {
                parts.Add($"Consumes {itemId}");
            }

            return parts.Count > 0 ? string.Join(", ", parts) : "Free";
        }

        /// <summary>
        /// Gets the diplomacy discount percentage.
        /// </summary>
        protected int GetCommerceDiscount()
        {
            int tokens = GetDiplomacyTokens();
            return Math.Min(tokens * 5, 25); // 5% per token, max 25%
        }

        /// <summary>
        /// Gets the reward display for an exchange.
        /// </summary>
        protected string GetRewardDisplay(ExchangeCard exchange)
        {
            if (exchange == null)
                throw new ArgumentNullException(nameof(exchange));
            if (exchange.Reward == null)
                return "Nothing";

            return exchange.Reward.GetDescription();
        }

        /// <summary>
        /// Gets the affordability reason for an exchange.
        /// </summary>
        protected string GetAffordabilityReason(ExchangeCard exchange)
        {
            if (exchange == null)
                return "Invalid exchange";

            if (Context == null)
                throw new InvalidOperationException("Exchange context is required");

            // Check resource costs
            foreach (ResourceAmount cost in exchange.Cost.Resources)
            {
                int available = GetAvailableResource(cost.Type);
                if (available < cost.Amount)
                {
                    int needed = cost.Amount - available;
                    return $"You only have {available} {cost.Type} - need {needed} more";
                }
            }

            // Check token requirements
            foreach (KeyValuePair<ConnectionType, int> token in exchange.Cost.TokenRequirements)
            {
                int playerTokens = Context.PlayerTokens.GetValueOrDefault(token.Key, 0);
                if (playerTokens < token.Value)
                {
                    int needed = token.Value - playerTokens;
                    return $"Requires {token.Value} {token.Key} tokens - you have {playerTokens}";
                }
            }

            // Check consumed item costs (resource costs, not boolean gates)
            foreach (string itemId in exchange.Cost.ConsumedItemIds)
            {
                if (!Context.PlayerInventory.ContainsKey(itemId) || Context.PlayerInventory[itemId] <= 0)
                {
                    return $"Requires {itemId} (will be consumed)";
                }
            }

            return "Cannot afford this exchange";
        }

        /// <summary>
        /// Gets the amount of a specific resource the player has.
        /// </summary>
        protected int GetAvailableResource(ResourceType type)
        {
            if (Context == null)
                throw new InvalidOperationException("Exchange context is required");
            if (Context.PlayerResources == null)
                throw new InvalidOperationException("Player resources are required");

            return type switch
            {
                ResourceType.Coins => Context.PlayerResources.Coins,
                ResourceType.Health => Context.PlayerResources.Health,
                ResourceType.Hunger => Context.PlayerResources.Stamina,
                _ => 0
            };
        }
    }
}
