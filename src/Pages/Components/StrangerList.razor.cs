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

        [Parameter] public string VenueId { get; set; }
        [Parameter] public EventCallback OnActionExecuted { get; set; }

        [CascadingParameter] protected GameScreenBase GameScreen { get; set; }

        protected List<NPC> AvailableStrangers { get; set; } = new();

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
            if (string.IsNullOrEmpty(VenueId))
            {
                // Get current Venue if not specified
                Venue currentLocation = GameFacade.GetCurrentLocation();
                VenueId = currentLocation?.Id;
            }

            if (!string.IsNullOrEmpty(VenueId))
            {
                AvailableStrangers = GameFacade.GetAvailableStrangers(VenueId);
            }
            else
            {
                AvailableStrangers = new List<NPC>();
            }
        }

        protected async Task StartStrangerConversation(string strangerId, string conversationType)
        {
            // For now, use the simpler approach - start conversation without specific type
            SocialChallengeContext context = GameFacade.StartStrangerConversation(strangerId);
            if (context != null)
            {
                RefreshStrangers();
                await OnActionExecuted.InvokeAsync();
            }
            else
            {
                NPC stranger = AvailableStrangers.FirstOrDefault(s => s.ID == strangerId);
                string strangerName = stranger?.Name ?? "stranger";
                GameFacade.GetMessageSystem().AddSystemMessage(
                    $"Unable to start conversation with {strangerName}. They may be unavailable or you lack the required resources.",
                    SystemMessageTypes.Warning);
            }
        }

        protected bool CanAffordConversation(string requestId)
        {
            return true;
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

        protected string GetStrangerDescription(NPC stranger)
        {
            // Generate contextual description based on personality and level
            string baseDesc = stranger.PersonalityType switch
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

        protected string GetRewardsPreview(GoalCard request)
        {
            if (request?.Rewards == null || !request.Rewards.Any())
            {
                return "Experience and insights";
            }

            List<string> rewardTexts = new List<string>();
            GoalReward reward = request.Rewards.First(); // Show first tier reward as preview

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