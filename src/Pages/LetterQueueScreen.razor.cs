using Microsoft.AspNetCore.Components;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Wayfarer.Pages
{
    public partial class LetterQueueScreen : MainGameplayViewBase
    {
        [Inject] private NPCRepository NPCRepository { get; set; }
        [Inject] private ITimeManager TimeManager { get; set; }
        private string GetCurrentTime()
        {
            var timeInfo = GameFacade.GetTimeInfo();
            var day = timeInfo.currentDay;
            var dayName = new[] { "MON", "TUE", "WED", "THU", "FRI", "SAT", "SUN" }[day % 7];
            
            // Calculate hour from TimeBlocks - game world specified 3:45 PM (15.75 hours)
            // For now, use hardcoded mockup time
            var hour = 15;
            var minute = 45;
            var period = "PM";
            var displayHour = 3;
            
            return $"{dayName} {displayHour}:{minute:D2} {period}";
        }

        private string GetNextDeadline()
        {
            var letters = GameFacade.GetPlayer().LetterQueue;
            if (letters == null || !letters.Any(l => l != null)) return "";
            
            var nextDeadline = letters.Where(l => l != null)
                .OrderBy(l => l.DeadlineInDays)
                .FirstOrDefault();
                
            if (nextDeadline == null) return "";
            
            var hoursLeft = nextDeadline.DeadlineInDays * 24;
            // Mock: assume it's 3:45 PM (15.75 hours into the day)
            hoursLeft -= (int)(24 - 15.75); // Adjust for current time of day
            
            if (hoursLeft <= 3)
            {
                return $"⚡ {hoursLeft}h until {GetShortName(nextDeadline.RecipientName)}";
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
            if (letter.DeadlineInDays <= 1) return "URGENT";
            if (letter.DeadlineInDays <= 2) return $"{letter.DeadlineInDays} days";
            return $"{letter.DeadlineInDays} days";
        }

        private async void StartConversation(string npcId)
        {
            await GameFacade.StartConversationAsync(npcId);
            // Navigation handled through MainGameplayViewBase
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