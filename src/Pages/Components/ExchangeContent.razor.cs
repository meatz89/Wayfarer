using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wayfarer.Subsystems.ExchangeSubsystem;

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
            
            // Reset selection when context changes
            if (Context?.Session?.SessionId != LastContextId)
            {
                SelectedExchangeId = null;
                LastResult = null;
                GenerateInitialNarrative();
                LastContextId = Context?.Session?.SessionId;
            }
        }

        private string LastContextId { get; set; }

        /// <summary>
        /// Gets the location context string for the header.
        /// </summary>
        protected string GetLocationContext()
        {
            if (Context?.LocationInfo != null)
            {
                var timeStr = GetTimeBlockDisplay(Context.CurrentTimeBlock);
                return $"{timeStr} - {Context.LocationInfo.Name}";
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
                TimeBlocks.Dawn => "Dawn",
                TimeBlocks.Morning => "Morning",
                TimeBlocks.Midday => "Midday",
                TimeBlocks.Afternoon => "Afternoon",
                TimeBlocks.Evening => "Evening",
                TimeBlocks.Night => "Night",
                _ => "Unknown Time"
            };
        }

        /// <summary>
        /// Gets the NPC status line for display.
        /// </summary>
        protected string GetNpcStatusLine()
        {
            if (Context?.NpcInfo == null)
                return "";

            var parts = new List<string>();
            
            // Add time-specific status
            parts.Add($"{GetTimeBlockDisplay(Context.CurrentTimeBlock)} business period");
            
            // Add merchant status if they have commerce tokens
            if (GetCommerceTokens() > 0)
            {
                parts.Add("Mercantile personality");
            }

            return string.Join(" • ", parts);
        }

        /// <summary>
        /// Gets the number of commerce tokens with this NPC.
        /// </summary>
        protected int GetCommerceTokens()
        {
            if (Context?.NpcInfo?.TokenCounts == null)
                return 0;

            return Context.NpcInfo.TokenCounts.GetValueOrDefault(ConnectionType.Commerce, 0);
        }

        /// <summary>
        /// Gets the discount description based on commerce tokens.
        /// </summary>
        protected string GetDiscountDescription()
        {
            var tokens = GetCommerceTokens();
            if (tokens <= 0)
                return "No discount";

            // 5% discount per commerce token
            var discount = Math.Min(tokens * 5, 25); // Cap at 25%
            return $"-{discount}% discount on all prices";
        }

        /// <summary>
        /// Generates initial narrative when entering exchange mode.
        /// </summary>
        protected void GenerateInitialNarrative()
        {
            if (Context?.NpcInfo != null)
            {
                // NPC-based exchange
                CurrentNarrative = GenerateNpcGreeting();
            }
            else if (Context?.LocationInfo != null)
            {
                // Location-based exchange
                CurrentNarrative = $"You examine the available services at {Context.LocationInfo.Name}.";
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
            var npcName = Context?.NpcInfo?.Name ?? "The merchant";
            var hasQueue = Context?.Session?.AvailableExchanges?.Any(e => e.ExchangeType == ExchangeType.Service) ?? false;
            
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
            return Context?.GetAvailableExchanges() ?? new List<ExchangeCard>();
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
            if (IsProcessingTrade)
                return false;

            if (string.IsNullOrEmpty(SelectedExchangeId))
                return false;

            var exchange = GetAvailableExchanges().FirstOrDefault(e => e.Id == SelectedExchangeId);
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
                // Execute the exchange through the facade
                var npcId = Context?.NpcInfo?.NpcId ?? "";
                LastResult = await ExchangeFacade.ExecuteExchange(npcId, SelectedExchangeId);

                if (LastResult?.Success == true)
                {
                    // Generate success narrative
                    CurrentNarrative = GenerateSuccessNarrative(LastResult);
                    
                    // Clear selection
                    SelectedExchangeId = null;

                    // Update context with new state
                    Context = await GameFacade.CreateExchangeContext(Context.NpcInfo?.NpcId);
                }
                else if (LastResult != null)
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
            if (result == null || !result.Success)
                return "The exchange was completed.";

            var rewardDesc = "";
            if ((result.RewardsGranted != null && result.RewardsGranted.Any()) || 
                (result.ItemsGranted != null && result.ItemsGranted.Any()))
            {
                var rewards = result.RewardsGranted?.Select(kvp => $"{kvp.Value} {kvp.Key}") ?? new List<string>();
                var items = result.ItemsGranted ?? new List<string>();
                var allRewards = rewards.Concat(items);
                rewardDesc = string.Join(", ", allRewards);
            }

            if (Context?.NpcInfo != null)
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
            if (string.IsNullOrEmpty(result?.Message))
                return "The exchange could not be completed.";

            return result.Message;
        }

        /// <summary>
        /// Exits the exchange and returns to location.
        /// </summary>
        protected async Task ExitExchange()
        {
            // End the exchange session
            if (Context?.Session != null)
            {
                Context.Session.EndSession();
            }

            // Notify parent
            await OnExchangeEnd.InvokeAsync();
            
            // Return to location through GameScreen
            if (GameScreen != null)
            {
                await GameScreen.ReturnToLocation();
            }
        }

        /// <summary>
        /// Gets the cost display for an exchange.
        /// </summary>
        protected string GetCostDisplay(ExchangeCard exchange)
        {
            if (exchange?.Cost == null)
                return "Free";

            var parts = new List<string>();
            
            // Apply commerce discount if applicable
            var discount = GetCommerceDiscount();
            
            foreach (var resource in exchange.Cost.Resources)
            {
                var amount = resource.Amount;
                if (resource.Type == ResourceType.Coins && discount > 0)
                {
                    var discounted = (int)(amount * (1 - discount / 100.0));
                    if (discounted < amount)
                    {
                        parts.Add($"<span class='original-cost'>{amount}</span>{discounted} {resource.Type}");
                        continue;
                    }
                }
                parts.Add($"{amount} {resource.Type}");
            }

            // Add token requirements
            foreach (var token in exchange.Cost.TokenRequirements)
            {
                parts.Add($"Requires {token.Value} {token.Key} tokens");
            }

            // Add item requirements
            foreach (var itemId in exchange.Cost.ConsumedItemIds)
            {
                parts.Add($"Consumes {itemId}");
            }

            return parts.Count > 0 ? string.Join(", ", parts) : "Free";
        }

        /// <summary>
        /// Gets the commerce discount percentage.
        /// </summary>
        protected int GetCommerceDiscount()
        {
            var tokens = GetCommerceTokens();
            return Math.Min(tokens * 5, 25); // 5% per token, max 25%
        }

        /// <summary>
        /// Gets the reward display for an exchange.
        /// </summary>
        protected string GetRewardDisplay(ExchangeCard exchange)
        {
            if (exchange?.Reward == null)
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

            // Check resource costs
            foreach (var cost in exchange.Cost.Resources)
            {
                var available = GetAvailableResource(cost.Type);
                if (available < cost.Amount)
                {
                    var needed = cost.Amount - available;
                    return $"You only have {available} {cost.Type} - need {needed} more";
                }
            }

            // Check token requirements
            foreach (var token in exchange.Cost.TokenRequirements)
            {
                var playerTokens = Context.PlayerTokens.GetValueOrDefault(token.Key, 0);
                if (playerTokens < token.Value)
                {
                    var needed = token.Value - playerTokens;
                    return $"Requires {token.Value} {token.Key} tokens - you have {playerTokens}";
                }
            }

            // Check item requirements
            foreach (var itemId in exchange.Cost.RequiredItemIds)
            {
                if (!Context.PlayerInventory.ContainsKey(itemId) || Context.PlayerInventory[itemId] <= 0)
                {
                    return $"Requires {itemId}";
                }
            }

            return "Cannot afford this exchange";
        }

        /// <summary>
        /// Gets the amount of a specific resource the player has.
        /// </summary>
        protected int GetAvailableResource(ResourceType type)
        {
            if (Context?.PlayerResources == null)
                return 0;

            return type switch
            {
                ResourceType.Coins => Context.PlayerResources.Coins,
                ResourceType.Health => Context.PlayerResources.Health,
                ResourceType.Hunger => Context.PlayerResources.Stamina,
                ResourceType.Attention => Context.CurrentAttention,
                _ => 0
            };
        }
    }
}