using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wayfarer.Pages.Components
{
    public class StrangerListBase : ComponentBase
    {
        [Inject] protected GameFacade GameFacade { get; set; }
        [Inject] protected GameWorld GameWorld { get; set; }

        [Parameter] public string LocationId { get; set; }
        [Parameter] public EventCallback OnActionExecuted { get; set; }

        [CascadingParameter] protected GameScreenBase GameScreen { get; set; }

        protected List<StrangerNPC> AvailableStrangers { get; set; } = new();

        protected override void OnInitialized()
        {
            RefreshStrangers();
        }

        protected override void OnParametersSet()
        {
            RefreshStrangers();
        }

        protected void RefreshStrangers()
        {
            if (string.IsNullOrEmpty(LocationId))
            {
                // Get current location if not specified
                Location currentLocation = GameFacade.GetCurrentLocation();
                LocationId = currentLocation?.Id;
            }

            if (!string.IsNullOrEmpty(LocationId))
            {
                AvailableStrangers = GameFacade.GetAvailableStrangers(LocationId);
            }
            else
            {
                AvailableStrangers = new List<StrangerNPC>();
            }
        }

        protected async Task StartStrangerConversation(string strangerId, string conversationType)
        {
            // For now, use the simpler approach - start conversation without specific type
            ConversationContext context = GameFacade.StartStrangerConversation(strangerId);
            if (context != null)
            {
                RefreshStrangers();
                await OnActionExecuted.InvokeAsync();
            }
            else
            {
                StrangerNPC stranger = AvailableStrangers.FirstOrDefault(s => s.Id == strangerId);
                string strangerName = stranger?.Name ?? "stranger";
                GameFacade.GetMessageSystem().AddSystemMessage(
                    $"Unable to start conversation with {strangerName}. They may be unavailable or you lack the required resources.",
                    SystemMessageTypes.Warning);
            }
        }

        protected bool CanAffordConversation(string conversationType)
        {
            AttentionStateInfo attentionState = GameFacade.GetCurrentAttentionState();
            int requiredAttention = GetAttentionCost(conversationType);
            return attentionState.Current >= requiredAttention;
        }

        protected int GetAttentionCost(string conversationType)
        {
            // Different conversation types have different attention costs
            return conversationType.ToLower() switch
            {
                "friendly_chat" => 1,
                "deep_conversation" => 2,
                "business_discussion" => 1,
                "casual_talk" => 1,
                _ => 1
            };
        }

        protected string GetPersonalityDescription(PersonalityType personality)
        {
            return personality switch
            {
                PersonalityType.DEVOTED => "Family-oriented and emotionally invested",
                PersonalityType.MERCANTILE => "Business-focused and practical",
                PersonalityType.PROUD => "Status-conscious and formal",
                PersonalityType.CUNNING => "Information-focused and calculating",
                PersonalityType.STEADFAST => "Duty-bound and reliable",
                _ => personality.ToString()
            };
        }

        protected string GetStrangerDescription(StrangerNPC stranger)
        {
            // Generate contextual description based on personality and level
            string baseDesc = stranger.Personality switch
            {
                PersonalityType.DEVOTED => "A family-oriented person with deep emotional connections.",
                PersonalityType.MERCANTILE => "A business-minded person focused on practical matters.",
                PersonalityType.PROUD => "A status-conscious person who values proper protocol.",
                PersonalityType.CUNNING => "A sharp individual who deals in information and secrets.",
                PersonalityType.STEADFAST => "A dependable person with strong moral convictions.",
                _ => "An interesting person to talk with."
            };

            string levelDesc = stranger.Level switch
            {
                1 => " Simple conversations yield basic insights.",
                2 => " Engaging discussions provide substantial learning.",
                3 => " Deep conversations offer significant wisdom.",
                _ => ""
            };

            return baseDesc + levelDesc;
        }

        protected string GetConversationTypeDisplay(string conversationType)
        {
            return conversationType switch
            {
                "friendly_chat" => "Friendly Chat",
                "deep_conversation" => "Deep Conversation",
                "business_discussion" => "Business Discussion",
                "casual_talk" => "Casual Talk",
                _ => conversationType.Replace("_", " ").ToTitleCase()
            };
        }

        protected string GetRewardsPreview(StrangerConversation conversation)
        {
            if (conversation?.Rewards == null || !conversation.Rewards.Any())
            {
                return "Experience and insights";
            }

            List<string> rewardTexts = new List<string>();
            StrangerReward reward = conversation.Rewards.First(); // Show first tier reward as preview

            if (reward.Coins > 0)
                rewardTexts.Add($"{reward.Coins} coins");
            if (reward.Health > 0)
                rewardTexts.Add($"+{reward.Health} health");
            if (reward.Food > 0)
                rewardTexts.Add($"+{reward.Food} food");
            if (!string.IsNullOrEmpty(reward.Item))
                rewardTexts.Add(reward.Item);

            return rewardTexts.Any() ? string.Join(", ", rewardTexts) : "Experience and insights";
        }

        /// <summary>
        /// Call this method to refresh the display when strangers change
        /// </summary>
        public void UpdateDisplay()
        {
            RefreshStrangers();
            StateHasChanged();
        }
    }
}

// Extension method for title case conversion
public static class StringExtensions
{
    public static string ToTitleCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        string[] words = input.Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length > 0)
            {
                words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
            }
        }
        return string.Join(" ", words);
    }
}