using Microsoft.AspNetCore.Components;

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
                if (currentLocation == null)
                {
                    throw new InvalidOperationException("Current location not found");
                }
                VenueId = currentLocation.Id;
            }

            AvailableStrangers = GameFacade.GetAvailableStrangers(VenueId);
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
            if (conversationType == null)
            {
                throw new InvalidOperationException("Conversation type is required");
            }

            return conversationType switch
            {
                "friendly_chat" => "Friendly Chat",
                "deep_conversation" => "Deep Conversation",
                "business_discussion" => "Business Discussion",
                "casual_talk" => "Casual Talk",
                _ => conversationType.Replace("_", " ").ToTitleCase()
            };
        }

        protected string GetRewardsPreview(Situation situation)
        {
            if (situation?.SituationCards == null || !situation.SituationCards.Any())
            {
                return "Experience and insights";
            }

            // Show first situation card's rewards as preview
            SituationCard firstSituationCard = situation.SituationCards.First();
            SituationCardRewards rewards = firstSituationCard.Rewards;

            if (rewards == null)
            {
                return "Experience and insights";
            }

            List<string> rewardTexts = new List<string>();

            if (rewards.Coins.HasValue && rewards.Coins.Value > 0)
                rewardTexts.Add($"{rewards.Coins.Value} coins");
            if (rewards.Progress.HasValue && rewards.Progress.Value > 0)
                rewardTexts.Add($"+{rewards.Progress.Value} progress");
            if (rewards.Breakthrough.HasValue && rewards.Breakthrough.Value > 0)
                rewardTexts.Add($"+{rewards.Breakthrough.Value} breakthrough");
            if (!string.IsNullOrEmpty(rewards.Item))
                rewardTexts.Add(rewards.Item);

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