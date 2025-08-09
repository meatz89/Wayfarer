using Microsoft.AspNetCore.Components;
using System;
using System.Linq;
using System.Collections.Generic;
using Wayfarer.Pages.Helpers;

namespace Wayfarer.Pages
{
    public partial class LetterQueueScreen : MainGameplayViewBase
    {
        [Inject] private NPCRepository NPCRepository { get; set; }
        [Inject] private ITimeManager TimeManager { get; set; }
        private string GetCurrentTime()
        {
            return TimeDisplayHelper.GetFormattedTime(TimeManager);
        }

        private string GetNextDeadline()
        {
            var letters = GameFacade.GetPlayer().LetterQueue;
            if (letters == null || !letters.Any(l => l != null)) return "";
            
            var nextDeadline = letters.Where(l => l != null)
                .OrderBy(l => l.DeadlineInHours)
                .FirstOrDefault();
                
            if (nextDeadline == null) return "";
            
            var hoursLeft = nextDeadline.DeadlineInHours;
            
            if (hoursLeft <= 3)
            {
                return $"⚡ {hoursLeft}h until {GetShortName(nextDeadline.RecipientName)}";
            }
            else if (hoursLeft <= 24)
            {
                return $"⏰ {hoursLeft}h until {GetShortName(nextDeadline.RecipientName)}";
            }
            else
            {
                var days = hoursLeft / 24;
                var hours = hoursLeft % 24;
                return $"⏰ {days}d {hours}h until {GetShortName(nextDeadline.RecipientName)}";
            }
        }

        private string GetShortName(string fullName)
        {
            if (fullName.StartsWith("Lord ")) return "Lord " + fullName.Substring(5).Substring(0, 1) + ".";
            return fullName.Split(' ')[0];
        }

        private Letter GetLetterAtPosition(int position)
        {
            var queue = GameFacade.GetPlayer().LetterQueue;
            if (queue == null || position > queue.Length || position < 1) return null;
            return queue[position - 1];
        }

        private string GetStakesClass(StakeType stakes)
        {
            return stakes switch
            {
                StakeType.REPUTATION => "reputation",
                StakeType.WEALTH => "wealth",
                StakeType.SAFETY => "safety",
                StakeType.SECRET => "secret",
                _ => ""
            };
        }

        private string GetDeadlineDisplay(Letter letter)
        {
            return letter.GetDeadlineDescription();
        }

        private string GetShortDeadline(Letter letter)
        {
            if (letter.DeadlineInHours <= 0) return "!EXP";
            if (letter.DeadlineInHours <= 2) return $"!{letter.DeadlineInHours}h";
            if (letter.DeadlineInHours <= 6) return $"{letter.DeadlineInHours}h";
            if (letter.DeadlineInHours <= 24) return $"{letter.DeadlineInHours}h";
            return $"{letter.DeadlineInHours / 24}d";
        }

        private string GetDeadlineClass(Letter letter)
        {
            if (letter.DeadlineInHours <= 0) return "deadline-expired";
            if (letter.DeadlineInHours <= 2) return "deadline-critical";
            if (letter.DeadlineInHours <= 6) return "deadline-urgent";
            return "deadline-normal";
        }

        private int GetTotalWeight()
        {
            var queue = GameFacade.GetPlayer().LetterQueue;
            if (queue == null) return 0;
            return queue.Where(l => l != null).Sum(l => l.Weight);
        }

        private int GetMaxWeight()
        {
            return 12; // Max weight capacity
        }

        private async Task StartConversation(string npcId)
        {
            Console.WriteLine($"[LetterQueueScreen] Starting conversation with NPC: {npcId}");
            var conversation = await GameFacade.StartConversationAsync(npcId);
            Console.WriteLine($"[LetterQueueScreen] Conversation created: {conversation != null}");
            
            if (conversation != null)
            {
                // Set the selected NPC for the conversation screen
                SelectedNpcId = npcId;
                
                // Navigate to conversation screen
                Console.WriteLine($"[LetterQueueScreen] OnNavigate null? {OnNavigate == null}");
                if (OnNavigate != null)
                {
                    Console.WriteLine($"[LetterQueueScreen] Navigating to ConversationScreen");
                    OnNavigate.Invoke(CurrentViews.ConversationScreen);
                }
                else
                {
                    Console.WriteLine($"[LetterQueueScreen] ERROR: OnNavigate is null!");
                }
                StateHasChanged();
            }
        }

        private int GetTokenCount(ConnectionType tokenType)
        {
            // Not used in current implementation
            return 0;
        }

        private string GetLocationPath()
        {
            var location = GameFacade.GetCurrentLocation();
            return location.location?.Name ?? "Unknown";
        }

        private string GetSpotPath()
        {
            var location = GameFacade.GetCurrentLocation();
            return location.spot?.Name ?? "Unknown";
        }

        private string GetCurrentSpotName()
        {
            var location = GameFacade.GetCurrentLocation();
            return location.spot?.Name ?? "Unknown Location";
        }

        private string GetAtmosphereText()
        {
            var location = GameFacade.GetCurrentLocation();
            if (location.spot?.SpotID == "copper_kettle")
            {
                return "Warm firelight. Clinking mugs. Low conversations blend with lute music. Smell of roasted meat.";
            }
            return location.spot?.Description ?? "A quiet place.";
        }

        private string GetStakesDisplay(StakeType stakes)
        {
            return stakes.ToString();
        }

        private string GetNPCMood(NPC npc)
        {
            // Return emoji based on NPC state
            if (npc.ID == "elena") return "😰";
            if (npc.ID == "bertram") return "😊";
            return "😐";
        }

        private string GetNPCDescription(NPC npc)
        {
            if (npc.ID == "elena")
                return "Sitting alone at a corner table, nervously fidgeting with a sealed letter";
            if (npc.ID == "bertram")
                return "Polishing glasses behind the bar, occasionally glancing your way with a welcoming nod";
            return npc.Description ?? "Standing nearby";
        }

        private string GetInteractionText(NPC npc)
        {
            if (npc.ID == "elena") return "Approach her table";
            if (npc.ID == "bertram") return "Ask about rumors";
            return "Talk to " + npc.Name;
        }

        private string GetInteractionCost(NPC npc)
        {
            if (npc.ID == "elena") return "Start conversation";
            if (npc.ID == "bertram") return "10 min";
            return "Free";
        }

        private List<NPC> GetNPCsAtCurrentSpot()
        {
            var location = GameFacade.GetCurrentLocation();
            if (location.spot == null) return new List<NPC>();
            
            // Get NPCs at the current spot and time
            TimeBlocks currentTime = TimeManager.GetCurrentTimeBlock();
            return NPCRepository.GetNPCsForLocationSpotAndTime(location.spot.SpotID, currentTime);
        }
    }
}