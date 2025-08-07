using Microsoft.AspNetCore.Components;
using System;
using System.Linq;

namespace Wayfarer.Pages
{
    public partial class LetterQueueScreen : MainGameplayViewBase
    {
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
    }
}