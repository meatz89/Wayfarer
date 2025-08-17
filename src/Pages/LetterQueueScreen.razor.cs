using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wayfarer.Pages.Helpers;
using Wayfarer.Services;

namespace Wayfarer.Pages
{
    public partial class LetterQueueScreen : MainGameplayViewBase
    {
        [Inject] private NPCRepository NPCRepository { get; set; }
        [Inject] private ITimeManager TimeManager { get; set; }
        [Inject] private ILetterQueueOperations QueueOperations { get; set; }

        [Parameter] public CurrentViews ReturnView { get; set; } = CurrentViews.LocationScreen;

        private bool _isReordering = false;
        private int? _selectedForReorder = null;
        private int? expandedPosition = null;
        private string GetCurrentTime()
        {
            return TimeDisplayHelper.GetFormattedTime(TimeManager);
        }

        private string GetNextDeadline()
        {
            Letter[] letters = GameFacade.GetPlayer().LetterQueue;
            if (letters == null || !letters.Any(l => l != null)) return "";

            Letter? nextDeadline = letters.Where(l => l != null && l.DeadlineInHours > 0)
                .OrderBy(l => l.DeadlineInHours)
                .FirstOrDefault();

            if (nextDeadline == null) return "";

            int hoursLeft = nextDeadline.DeadlineInHours;
            string recipientShort = GetShortName(nextDeadline.RecipientName);

            // Use our human-readable format
            string timeDesc = GetShortDeadline(nextDeadline);

            if (hoursLeft <= 3)
            {
                return $"⚡ {recipientShort}: {timeDesc}";
            }
            else if (hoursLeft <= 6)
            {
                return $"🔥 {recipientShort}: {timeDesc}";
            }
            else if (hoursLeft <= 24)
            {
                return $"⏰ {recipientShort}: {timeDesc}";
            }
            else
            {
                return $"📜 Next: {recipientShort} {timeDesc}";
            }
        }

        private string GetShortName(string fullName)
        {
            if (fullName.StartsWith("Lord ")) return "Lord " + fullName.Substring(5).Substring(0, 1) + ".";
            return fullName.Split(' ')[0];
        }

        private Letter GetLetterAtPosition(int position)
        {
            Letter[] queue = GameFacade.GetPlayer().LetterQueue;
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
            int hoursLeft = letter.DeadlineInHours;
            int currentHour = TimeManager.GetCurrentTimeHours();
            int daysAway = hoursLeft / 24;
            int hoursRemaining = hoursLeft % 24;

            if (hoursLeft <= 0) return "Letter has expired - permanent consequences applied";
            if (hoursLeft <= 3) return $"CRITICAL: {hoursLeft} hour{(hoursLeft == 1 ? "" : "s")} remaining!";

            if (daysAway == 0)
            {
                return $"Due today ({hoursLeft} hours remaining)";
            }
            else if (daysAway == 1)
            {
                return $"Due tomorrow ({hoursLeft} hours remaining)";
            }
            else
            {
                return $"Due in {daysAway} days and {hoursRemaining} hours";
            }
        }

        private string GetDeadlineIcon(Letter letter)
        {
            return letter.DeadlineInHours switch
            {
                <= 0 => "💀",    // Expired
                <= 3 => "⚡",    // Critical
                <= 6 => "🔥",    // Very Urgent  
                <= 12 => "⏰",   // Urgent
                <= 24 => "⏱️",   // Today
                _ => ""          // Normal
            };
        }

        private string GetShortDeadline(Letter letter)
        {
            int hoursLeft = letter.DeadlineInHours;
            int currentHour = TimeManager.GetCurrentTimeHours();
            int currentMinute = TimeManager.GetCurrentMinutes();
            int targetHour = (currentHour + hoursLeft) % 24;
            int daysAway = hoursLeft / 24;

            // EXPIRED
            if (hoursLeft <= 0) return "EXPIRED!";

            // CRITICAL: Less than 3 hours - show exact time
            if (hoursLeft == 1) return "1 HOUR!";
            if (hoursLeft == 2) return "2 HOURS!";
            if (hoursLeft <= 3) return $"{hoursLeft} HOURS!";

            // URGENT: Today - use medieval time references
            if (daysAway == 0)
            {
                // Calculate actual target time
                int targetTime = currentHour + hoursLeft;
                if (targetTime >= 24) targetTime -= 24;

                return targetTime switch
                {
                    >= 6 and < 9 => "By Dawn",
                    >= 9 and < 12 => "By Morning",
                    12 => "By Midday",
                    >= 12 and < 15 => "By Afternoon",
                    >= 15 and < 18 => "By Evening",
                    >= 18 and < 21 => "By Nightfall",
                    >= 21 and < 24 => "By Late Night",
                    _ => "Tonight"
                };
            }

            // TOMORROW
            if (daysAway == 1)
            {
                return targetHour switch
                {
                    >= 6 and < 12 => "Tomorrow Morn",
                    >= 12 and < 18 => "Tomorrow Aft.",
                    _ => "Tomorrow Eve"
                };
            }

            // DISTANT (2+ days)
            if (daysAway == 2) return "In 2 days";
            if (daysAway == 3) return "In 3 days";
            return $"In {daysAway} days";
        }

        private string GetDeadlineClass(Letter letter)
        {
            if (letter.DeadlineInHours <= 0) return "deadline-expired";
            if (letter.DeadlineInHours <= 3) return "deadline-critical";
            if (letter.DeadlineInHours <= 6) return "deadline-urgent";
            if (letter.DeadlineInHours <= 24) return "deadline-today";
            if (letter.DeadlineInHours <= 48) return "deadline-tomorrow";
            return "deadline-normal";
        }

        private int GetTotalWeight()
        {
            Letter[] queue = GameFacade.GetPlayer().LetterQueue;
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
            ViewModels.ConversationViewModel? conversation = await GameFacade.StartConversationAsync(npcId);
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
            (Location location, LocationSpot spot) location = GameFacade.GetCurrentLocation();
            return location.location?.Name ?? "Unknown";
        }

        private string GetSpotPath()
        {
            (Location location, LocationSpot spot) location = GameFacade.GetCurrentLocation();
            return location.spot?.Name ?? "Unknown";
        }

        private string GetCurrentSpotName()
        {
            (Location location, LocationSpot spot) location = GameFacade.GetCurrentLocation();
            return location.spot?.Name ?? "Unknown Location";
        }

        private string GetAtmosphereText()
        {
            (Location location, LocationSpot spot) location = GameFacade.GetCurrentLocation();
            if (location.spot?.SpotID == "copper_kettle")
            {
                return "Warm firelight. Clinking mugs. Low conversations blend with lute music. Smell of roasted meat.";
            }
            return location.spot?.Description ?? "A quiet place.";
        }

        private string GetStakesDisplay(StakeType stakes)
        {
            return stakes switch
            {
                StakeType.REPUTATION => "Social Standing",
                StakeType.WEALTH => "Financial Survival",
                StakeType.SAFETY => "Physical Danger",
                StakeType.SECRET => "Hidden Truth",
                _ => stakes.ToString()
            };
        }

        private string GetEmotionalWeightDisplay(EmotionalWeight weight)
        {
            return weight switch
            {
                EmotionalWeight.LOW => "Routine",
                EmotionalWeight.MEDIUM => "Important",
                EmotionalWeight.HIGH => "Life-Changing",
                EmotionalWeight.CRITICAL => "Life or Death",
                _ => weight.ToString()
            };
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
            (Location location, LocationSpot spot) location = GameFacade.GetCurrentLocation();
            if (location.spot == null) return new List<NPC>();

            // Get NPCs at the current spot and time
            TimeBlocks currentTime = TimeManager.GetCurrentTimeBlock();
            return NPCRepository.GetNPCsForLocationSpotAndTime(location.spot.SpotID, currentTime);
        }

        private void TogglePosition(int position)
        {
            if (expandedPosition == position)
            {
                expandedPosition = null;
            }
            else
            {
                expandedPosition = position;
            }
        }

        private void HandleSlotClick(int position)
        {
            if (_isReordering)
            {
                if (_selectedForReorder == position)
                {
                    CancelReorder();
                }
                else
                {
                    CompleteReorder(position);
                }
            }
            else
            {
                TogglePosition(position);
            }
        }

        private async Task DeliverLetter()
        {
            QueueOperationResult result = await QueueOperations.DeliverFromPosition1Async();
            if (result.Success)
            {
                expandedPosition = null;
                StateHasChanged();
            }
            else
            {
                // Show error message - would normally use MessageSystem
                Console.WriteLine($"Delivery failed: {result.FailureReason}");
            }
        }

        private async Task StartReorder(int position)
        {
            Letter letter = GetLetterAtPosition(position);
            if (letter != null)
            {
                _isReordering = true;
                _selectedForReorder = position;
                StateHasChanged();
            }
        }

        private async Task CompleteReorder(int targetPosition)
        {
            if (_selectedForReorder.HasValue)
            {
                QueueOperationResult result = await QueueOperations.TryReorderAsync(_selectedForReorder.Value, targetPosition);
                if (result.Success)
                {
                    _isReordering = false;
                    _selectedForReorder = null;
                    StateHasChanged();
                }
            }
        }

        private void CancelReorder()
        {
            _isReordering = false;
            _selectedForReorder = null;
            StateHasChanged();
        }

        private void NavigateToLocation()
        {
            OnNavigate?.Invoke(CurrentViews.LocationScreen);
        }

        private void NavigateToConversation()
        {
            OnNavigate?.Invoke(CurrentViews.ConversationScreen);
        }

        private void NavigateToDelivery()
        {
            Letter letter = GetLetterAtPosition(1);
            if (letter != null)
            {
                // Navigate to location screen to handle delivery
                OnNavigate?.Invoke(CurrentViews.LocationScreen);
            }
        }

        private async Task TryMorningSwap(int position)
        {
            // For morning swap, we swap with position 1
            if (position > 1)
            {
                QueueOperationResult result = await QueueOperations.TryMorningSwapAsync(1, position);
                if (!result.Success)
                {
                    Console.WriteLine($"Morning swap failed: {result.FailureReason}");
                }
                expandedPosition = null;
                StateHasChanged();
            }
        }

        private bool CanDeliverNow()
        {
            // Check if we can deliver the letter at position 1 right now
            return QueueOperations.CanPerformOperation(QueueOperationType.Deliver, 1);
        }

        private void NavigateToRecipient()
        {
            // Navigate to travel screen to go to recipient's location
            Letter letter = GetLetterAtPosition(1);
            if (letter != null)
            {
                // TODO: Pass recipient location to travel screen
                OnNavigate?.Invoke(CurrentViews.TravelScreen);
            }
        }

        private void HandleExitQueue()
        {
            Console.WriteLine($"[LetterQueueScreen] HandleExitQueue - returning to {ReturnView}");
            OnNavigate?.Invoke(ReturnView);
        }

        private bool HasCriticalDeadlines()
        {
            Letter[] letters = GameFacade.GetPlayer().LetterQueue;
            if (letters == null) return false;
            return letters.Any(l => l != null && l.DeadlineInHours <= 3 && l.DeadlineInHours > 0);
        }

        private Letter GetMostUrgentLetter()
        {
            Letter[] letters = GameFacade.GetPlayer().LetterQueue;
            if (letters == null) return null;

            return letters.Where(l => l != null && l.DeadlineInHours > 0)
                .OrderBy(l => l.DeadlineInHours)
                .FirstOrDefault();
        }

        private string GetCriticalLetterWarning()
        {
            Letter urgent = GetMostUrgentLetter();
            if (urgent == null) return "";

            if (urgent.DeadlineInHours <= 1)
                return $"⚠️ CRITICAL: {urgent.RecipientName} letter expires in 1 HOUR!";
            else if (urgent.DeadlineInHours <= 3)
                return $"⚠️ URGENT: {urgent.RecipientName} letter expires in {urgent.DeadlineInHours} hours!";
            return "";
        }
    }
}