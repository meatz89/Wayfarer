using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wayfarer.Pages
{
    public partial class ObligationQueueScreen : MainGameplayViewBase
    {
        [Inject] private NPCRepository NPCRepository { get; set; }
        [Inject] private ITimeManager TimeManager { get; set; }
        [Inject] private GameFacade GameFacade { get; set; }
        [Inject] private NavigationCoordinator NavigationCoordinator { get; set; }
        [Inject] private ObligationQueueManager QueueManager { get; set; }
        [Inject] private LocationRepository LocationRepository { get; set; }

        [Parameter] public CurrentViews ReturnView { get; set; } = CurrentViews.LocationScreen;

        // Unified obligation representation
        private class UnifiedObligation
        {
            public string Id { get; set; }
            public string Type { get; set; } // "delivery" or "meeting"
            public string TargetName { get; set; }
            public string TargetId { get; set; }
            public string LocationId { get; set; }
            public int DeadlineInMinutes { get; set; }
            public StakeType Stakes { get; set; }
            public string Description { get; set; }
            public string ConsequenceIfSuccess { get; set; }
            public string ConsequenceIfFailure { get; set; }
            public int Position { get; set; } // For delivery obligations
            public int Size { get; set; } // For delivery obligations
        }

        private string selectedObligationId = null;
        private bool isReordering = false;
        private string reorderingObligationId = null;

        private string GetCurrentTime()
        {
            return TimeDisplayHelper.GetFormattedTime(TimeManager);
        }

        private string GetNextDeadline()
        {
            var nextObligation = GetAllObligationsSorted().FirstOrDefault();
            if (nextObligation == null) return "";

            string targetShort = GetShortName(nextObligation.TargetName);
            string timeDesc = GetTimeDescription(nextObligation.DeadlineInMinutes);

            return nextObligation.DeadlineInMinutes switch
            {
                <= 180 => $"⚡ {targetShort}: {timeDesc}",
                <= 360 => $"🔥 {targetShort}: {timeDesc}",
                <= 1440 => $"⏰ {targetShort}: {timeDesc}",
                _ => $"📜 Next: {targetShort} {timeDesc}"
            };
        }

        private bool HasCriticalDeadlines()
        {
            return GetAllObligationsSorted().Any(o => o.DeadlineInMinutes <= 180);
        }

        private string GetCriticalWarning()
        {
            var critical = GetAllObligationsSorted()
                .Where(o => o.DeadlineInMinutes <= 180)
                .OrderBy(o => o.DeadlineInMinutes)
                .FirstOrDefault();

            if (critical == null) return "";

            return critical.Type == "meeting" 
                ? $"URGENT: Meet {critical.TargetName} within {critical.DeadlineInMinutes} minutes!"
                : $"CRITICAL: Deliver to {critical.TargetName} within {critical.DeadlineInMinutes} minutes!";
        }

        private int GetTotalSize()
        {
            return QueueManager.GetTotalSize();
        }

        private int GetMaxSize()
        {
            return GameFacade.GetPlayer().MaxSatchelSize;
        }

        private bool IsOverweight()
        {
            return GetTotalSize() > GetMaxSize();
        }

        private int GetTotalObligations()
        {
            return GetAllObligationsSorted().Count();
        }

        private bool HasAnyObligations()
        {
            return GetTotalObligations() > 0;
        }

        private List<UnifiedObligation> GetAllObligationsSorted()
        {
            var obligations = new List<UnifiedObligation>();
            
            // Add delivery obligations
            var deliveries = QueueManager.GetActiveObligations();
            for (int i = 0; i < deliveries.Length; i++)
            {
                var delivery = deliveries[i];
                if (delivery != null)
                {
                    obligations.Add(new UnifiedObligation
                    {
                        Id = delivery.Id,
                        Type = "delivery",
                        TargetName = delivery.RecipientName,
                        TargetId = delivery.RecipientId,
                        LocationId = GetNPCLocation(delivery.RecipientId),
                        DeadlineInMinutes = delivery.DeadlineInMinutes,
                        Stakes = delivery.Stakes,
                        Description = delivery.Description,
                        ConsequenceIfSuccess = delivery.ConsequenceIfDelivered,
                        ConsequenceIfFailure = delivery.ConsequenceIfLate,
                        Position = i + 1,
                        Size = 1 // Default size for delivery obligations
                    });
                }
            }
            
            // Add meeting obligations
            var meetings = QueueManager.GetActiveMeetingObligations();
            foreach (var meeting in meetings)
            {
                obligations.Add(new UnifiedObligation
                {
                    Id = meeting.Id,
                    Type = "meeting",
                    TargetName = meeting.RequesterName,
                    TargetId = meeting.RequesterId,
                    LocationId = GetNPCLocation(meeting.RequesterId), // Meeting at NPC's location
                    DeadlineInMinutes = meeting.DeadlineInMinutes,
                    Stakes = meeting.Stakes,
                    Description = meeting.Reason ?? "Urgent meeting requested",
                    ConsequenceIfSuccess = "Strengthen relationship",
                    ConsequenceIfFailure = "NPC becomes hostile",
                    Position = 0, // Meetings don't have queue positions
                    Size = 0
                });
            }
            
            // Sort by deadline (most urgent first)
            return obligations.OrderBy(o => o.DeadlineInMinutes).ToList();
        }

        private string GetNPCLocation(string npcId)
        {
            var npc = NPCRepository.GetById(npcId);
            return npc?.Location ?? "";
        }

        private void SelectObligation(string obligationId)
        {
            selectedObligationId = selectedObligationId == obligationId ? null : obligationId;
            StateHasChanged();
        }

        private string GetObligationType(UnifiedObligation obligation)
        {
            return obligation.Type;
        }

        private string GetTypeBadgeClass(UnifiedObligation obligation)
        {
            return obligation.Type == "delivery" ? "type-delivery" : "type-meeting";
        }

        private string GetObligationTypeText(UnifiedObligation obligation)
        {
            return obligation.Type == "delivery" ? "LETTER" : "MEETING";
        }

        private string GetDeadlineIcon(UnifiedObligation obligation)
        {
            return obligation.DeadlineInMinutes switch
            {
                <= 0 => "💀",
                <= 180 => "⚡",
                <= 360 => "🔥",
                <= 720 => "⏰",
                <= 1440 => "⏱️",
                _ => ""
            };
        }

        private string GetObligationTarget(UnifiedObligation obligation)
        {
            return obligation.Type == "delivery" 
                ? $"Deliver to {obligation.TargetName}"
                : $"Meet with {obligation.TargetName}";
        }

        private string GetObligationLocation(UnifiedObligation obligation)
        {
            if (string.IsNullOrEmpty(obligation.LocationId)) return "";
            
            var location = LocationRepository.GetLocation(obligation.LocationId);
            return location?.Name ?? "";
        }

        private string GetDeadlineClass(UnifiedObligation obligation)
        {
            return obligation.DeadlineInMinutes switch
            {
                <= 180 => "critical",
                <= 480 => "urgent",
                _ => "normal"
            };
        }

        private string GetDeadlineDisplay(UnifiedObligation obligation)
        {
            return GetTimeDescription(obligation.DeadlineInMinutes);
        }

        private string GetStakesClass(UnifiedObligation obligation)
        {
            return obligation.Stakes switch
            {
                StakeType.REPUTATION => "stakes-reputation",
                StakeType.WEALTH => "stakes-wealth",
                StakeType.SAFETY => "stakes-safety",
                StakeType.SECRET => "stakes-secret",
                _ => ""
            };
        }

        private string GetStakesText(UnifiedObligation obligation)
        {
            return obligation.Stakes.ToString();
        }

        private string GetObligationDescription(UnifiedObligation obligation)
        {
            return obligation.Description ?? 
                (obligation.Type == "delivery" 
                    ? "A letter that must be delivered" 
                    : "An urgent meeting request");
        }

        private bool HasConsequences(UnifiedObligation obligation)
        {
            return !string.IsNullOrEmpty(obligation.ConsequenceIfSuccess) || 
                   !string.IsNullOrEmpty(obligation.ConsequenceIfFailure);
        }

        private string GetSuccessConsequence(UnifiedObligation obligation)
        {
            return obligation.ConsequenceIfSuccess;
        }

        private string GetFailureConsequence(UnifiedObligation obligation)
        {
            return obligation.ConsequenceIfFailure;
        }

        private bool CanActOnObligation(UnifiedObligation obligation)
        {
            if (obligation.Type == "delivery")
            {
                // Can deliver if at recipient's location and it's position 1
                return obligation.Position == 1 && IsAtRecipientLocation(obligation);
            }
            else
            {
                // Can go to meeting if at meeting location
                return IsAtMeetingLocation(obligation);
            }
        }

        private bool CanReorderObligation(UnifiedObligation obligation)
        {
            // Only delivery obligations can be reordered, and only if not in position 1
            return obligation.Type == "delivery" && obligation.Position > 1;
        }

        private string GetPrimaryActionText(UnifiedObligation obligation)
        {
            if (obligation.Type == "delivery")
            {
                return IsAtRecipientLocation(obligation) ? "Deliver Letter" : "Travel to Recipient";
            }
            else
            {
                return IsAtMeetingLocation(obligation) ? "Start Meeting" : "Travel to Meeting";
            }
        }

        private void ActOnObligation(UnifiedObligation obligation)
        {
            if (obligation.Type == "delivery")
            {
                if (IsAtRecipientLocation(obligation))
                {
                    // TODO: Implement delivery through GameFacade
                    // GameFacade needs a DeliverLetter method
                }
                else
                {
                    // Navigate to recipient location
                    _ = NavigationCoordinator.NavigateToAsync(CurrentViews.LocationScreen);
                }
            }
            else
            {
                if (IsAtMeetingLocation(obligation))
                {
                    // Start conversation with NPC
                    _ = NavigationCoordinator.NavigateToAsync(CurrentViews.ConversationScreen, obligation.TargetId);
                }
                else
                {
                    // Navigate to meeting location  
                    _ = NavigationCoordinator.NavigateToAsync(CurrentViews.LocationScreen);
                }
            }
        }

        private void StartReorder(UnifiedObligation obligation)
        {
            isReordering = true;
            reorderingObligationId = obligation.Id;
            StateHasChanged();
        }

        private bool IsAtRecipientLocation(UnifiedObligation obligation)
        {
            var player = GameFacade.GetPlayer();
            if (player.CurrentLocationSpot == null) return false;
            return player.CurrentLocationSpot.LocationId == obligation.LocationId;
        }

        private bool IsAtMeetingLocation(UnifiedObligation obligation)
        {
            var player = GameFacade.GetPlayer();
            if (player.CurrentLocationSpot == null) return false;
            return player.CurrentLocationSpot.LocationId == obligation.LocationId;
        }

        private string GetTimeDescription(int minutes)
        {
            if (minutes <= 0) return "EXPIRED";
            if (minutes < 60) return $"{minutes}m";
            if (minutes < 120) return $"1h {minutes - 60}m";
            if (minutes < 1440) return $"{minutes / 60}h";
            
            int days = minutes / 1440;
            return days == 1 ? "Tomorrow" : $"{days} days";
        }

        private string GetShortName(string fullName)
        {
            if (fullName.StartsWith("Lord ")) return "Lord " + fullName.Substring(5).Substring(0, 1) + ".";
            return fullName.Split(' ')[0];
        }

        private async void HandleExitQueue()
        {
            await NavigationCoordinator.NavigateToAsync(CurrentViews.LocationScreen);
        }
    }
}