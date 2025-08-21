using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wayfarer.Pages
{
    public enum ActionType { None, Listen, Speak }

    public class ConversationScreenBase : ComponentBase
    {
        [Parameter] public string NpcId { get; set; }
        [Parameter] public EventCallback OnConversationEnd { get; set; }

        [Inject] protected ConversationManager ConversationManager { get; set; }
        [Inject] protected GameFacade GameFacade { get; set; }
        [Inject] protected ITimeManager TimeManager { get; set; }
        [Inject] protected NPCRelationshipTracker RelationshipTracker { get; set; }
        [Inject] protected NavigationManager Navigation { get; set; }
        [Inject] protected ObligationQueueManager LetterQueueManager { get; set; }

        protected ConversationSession Session { get; set; }
        protected HashSet<ConversationCard> SelectedCards { get; set; } = new();
        protected ActionType SelectedAction { get; set; } = ActionType.None;
        protected CardPlayResult LastResult { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                // Get any observation cards from GameFacade
                var observationCards = GetObservationCards();
                
                // Start the conversation
                Session = ConversationManager.StartConversation(NpcId, observationCards);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start conversation: {ex.Message}");
                Navigation.NavigateTo("/location");
            }
        }

        protected void SelectAction(ActionType action)
        {
            SelectedAction = action;
            if (action == ActionType.Listen)
            {
                SelectedCards.Clear();
            }
            StateHasChanged();
        }

        protected void ToggleCard(ConversationCard card)
        {
            if (SelectedAction != ActionType.Speak) return;

            if (SelectedCards.Contains(card))
            {
                SelectedCards.Remove(card);
            }
            else if (CanSelectCard(card))
            {
                // Check if we can add this card
                var tempSelection = new HashSet<ConversationCard>(SelectedCards) { card };
                if (ConversationManager.CanSelectCard(card, SelectedCards))
                {
                    SelectedCards.Add(card);
                }
            }
            StateHasChanged();
        }

        protected bool CanSelectCard(ConversationCard card)
        {
            if (SelectedAction != ActionType.Speak) return false;
            return ConversationManager.CanSelectCard(card, SelectedCards);
        }

        protected async Task ExecuteAction()
        {
            // Check if session is still active
            if (Session == null || !ConversationManager.IsConversationActive)
            {
                Console.WriteLine("[ConversationScreen] No active session, returning to location");
                await OnConversationEnd.InvokeAsync();
                return;
            }
            
            if (SelectedAction == ActionType.Listen)
            {
                ConversationManager.ExecuteListen();
                SelectedCards.Clear();
            }
            else if (SelectedAction == ActionType.Speak && SelectedCards.Any())
            {
                LastResult = ConversationManager.ExecuteSpeak(SelectedCards);
                SelectedCards.Clear();
            }

            SelectedAction = ActionType.None;
            
            // Check if conversation ended
            if (!ConversationManager.IsConversationActive)
            {
                var outcome = Session.CheckThresholds();
                // Show outcome and navigate back
                await Task.Delay(2000);
                await OnConversationEnd.InvokeAsync();
            }
            
            StateHasChanged();
        }

        protected bool CanExecute()
        {
            if (SelectedAction == ActionType.Listen)
                return true;
            
            if (SelectedAction == ActionType.Speak)
            {
                var rules = ConversationRules.States[Session.CurrentState];
                if (rules.RequiredCards.HasValue)
                    return SelectedCards.Count >= rules.RequiredCards.Value;
                return SelectedCards.Any();
            }
            
            return false;
        }

        // UI Helper Methods
        protected string GetCurrentTimeDisplay()
        {
            return TimeManager.GetFormattedTimeDisplay();
        }

        protected string GetUrgentDeadline()
        {
            var queue = GameFacade.GetLetterQueue();
            var urgentSlot = queue?.QueueSlots?
                .FirstOrDefault(s => s.IsOccupied && s.DeliveryObligation?.DeadlineInHours < 6);
            
            if (urgentSlot?.DeliveryObligation != null)
            {
                return $"{urgentSlot.DeliveryObligation.SenderName}'s letter: {urgentSlot.DeliveryObligation.DeadlineInHours}h remain";
            }
            return null;
        }

        protected List<string> GetLocationPath()
        {
            var (location, spot) = GameFacade.GetCurrentLocation();
            return new List<string> { location?.Name ?? "Unknown", spot?.Name ?? "Somewhere" };
        }

        protected string GetCurrentSpot()
        {
            var (_, spot) = GameFacade.GetCurrentLocation();
            return spot?.Name ?? "Somewhere";
        }

        protected string GetStateClass()
        {
            return Session.CurrentState.ToString().ToLower();
        }

        protected string GetStateDescription()
        {
            var state = Session.CurrentState;
            var description = state.ToString();
            
            if (state == EmotionalState.DESPERATE)
            {
                var queue = GameFacade.GetLetterQueue();
                var urgentSlot = queue?.QueueSlots?
                    .FirstOrDefault(s => s.IsOccupied && s.DeliveryObligation?.SenderName == Session.NPC.Name);
                if (urgentSlot?.DeliveryObligation != null)
                {
                    description += $" • Letter deadline in {urgentSlot.DeliveryObligation.DeadlineInHours}h!";
                }
            }
            
            return description;
        }

        protected int GetTokenCount(ConnectionType type)
        {
            var relationship = RelationshipTracker.GetRelationship(Session.NPC.ID);
            return type switch
            {
                ConnectionType.Trust => relationship.Trust,
                ConnectionType.Commerce => relationship.Commerce,
                ConnectionType.Status => relationship.Status,
                ConnectionType.Shadow => relationship.Shadow,
                _ => 0
            };
        }

        protected string GetTokenEffect(ConnectionType type)
        {
            var count = GetTokenCount(type);
            return type switch
            {
                ConnectionType.Trust => count >= 2 ? $"+{count/2} turns" : "no bonus",
                ConnectionType.Commerce => count > 0 ? $"+{count} coin mult" : "no bonus",
                ConnectionType.Status => count > 0 ? $"+{count*3}% success" : "no bonus",
                ConnectionType.Shadow => count >= 3 ? "protects cards" : "no bonus",
                _ => "no bonus"
            };
        }

        protected int GetComfortTarget()
        {
            if (Session.CurrentComfort >= 15) return 20;
            if (Session.CurrentComfort >= 10) return 15;
            if (Session.CurrentComfort >= 5) return 10;
            return 5;
        }

        protected int GetComfortPercent()
        {
            var target = GetComfortTarget();
            return Math.Min(100, (Session.CurrentComfort * 100) / target);
        }

        protected string GetDepthName()
        {
            return Session.CurrentDepth switch
            {
                0 => "(Surface)",
                1 => "(Personal)",
                2 => "(Intimate)",
                3 => "(Soul-deep)",
                _ => ""
            };
        }

        protected int GetDrawCount()
        {
            var rules = ConversationRules.States[Session.CurrentState];
            return rules.CardsOnListen;
        }

        protected bool HasOpportunities()
        {
            return Session.HandCards.Any(c => c.Persistence == PersistenceType.Opportunity);
        }

        protected int GetCurrentWeight()
        {
            return SelectedCards.Sum(c => c.GetEffectiveWeight(Session.CurrentState));
        }

        protected int GetMaxWeight()
        {
            var rules = ConversationRules.States[Session.CurrentState];
            return rules.MaxWeight;
        }

        protected bool IsOverWeight()
        {
            return GetCurrentWeight() > GetMaxWeight();
        }

        protected bool HasCrisisSelected()
        {
            return SelectedCards.Any(c => c.IsCrisis);
        }

        protected string GetCardClasses(ConversationCard card)
        {
            var classes = new List<string>();
            
            classes.Add(card.Type.ToString().ToLower());
            classes.Add(card.Persistence.ToString().ToLower());
            
            if (card.IsCrisis) classes.Add("crisis");
            if (card.IsStateCard) classes.Add("state-changer");
            if (card.IsObservation) classes.Add("observation-card");
            if (card.CanDeliverLetter) classes.Add("letter-delivery");
            
            return string.Join(" ", classes);
        }

        protected string GetStateChangeText(ConversationCard card)
        {
            if (!card.IsStateCard) return "";
            
            if (card.SuccessState.HasValue)
            {
                return $"{Session.CurrentState} → {card.SuccessState.Value}";
            }
            return "State change";
        }

        protected int GetStatusTokens()
        {
            var relationship = RelationshipTracker.GetRelationship(Session.NPC.ID);
            return relationship.Status;
        }

        protected string GetSuccessEffect(ConversationCard card)
        {
            if (card.IsStateCard && card.SuccessState.HasValue)
            {
                return $"{Session.CurrentState} → {card.SuccessState.Value}";
            }
            if (card.CanDeliverLetter)
            {
                return $"+{card.BaseComfort} comfort + deliver";
            }
            return $"+{card.BaseComfort} comfort";
        }

        protected string GetFailureEffect(ConversationCard card)
        {
            if (card.IsStateCard && card.FailureState.HasValue)
            {
                return $"{Session.CurrentState} → {card.FailureState.Value}";
            }
            return "+1 comfort";
        }

        protected string GetPlayButtonText()
        {
            if (SelectedAction == ActionType.None)
                return "Select LISTEN or SPEAK";
            
            if (SelectedAction == ActionType.Listen)
                return $"Listen to {Session.NPC.Name} (costs 1 turn)";
            
            if (SelectedAction == ActionType.Speak)
            {
                if (!SelectedCards.Any())
                    return "Choose your response...";
                
                if (HasCrisisSelected())
                    return "🔥 DESPERATE ACTION (costs 1 turn)";
                
                var manager = new CardSelectionManager(Session.CurrentState);
                foreach (var card in SelectedCards)
                {
                    manager.ToggleCard(card);
                }
                
                return manager.GetSelectionDescription() + " (costs 1 turn)";
            }
            
            return "Select action";
        }

        protected string GetPlayInfo()
        {
            if (SelectedAction == ActionType.Listen)
            {
                var rules = ConversationRules.States[Session.CurrentState];
                return $"Draw {rules.CardsOnListen} new thoughts, but fleeting opportunities pass";
            }
            
            if (SelectedAction == ActionType.Speak && SelectedCards.Any())
            {
                var totalComfort = SelectedCards.Sum(c => c.BaseComfort);
                var types = SelectedCards.Select(c => c.Type).Distinct();
                
                if (types.Count() == 1 && SelectedCards.Count > 1)
                {
                    var bonus = SelectedCards.Count switch
                    {
                        2 => 2,
                        3 => 5,
                        _ => 8
                    };
                    return $"Expected: {totalComfort} comfort +{bonus} set bonus! (if all succeed)";
                }
                
                return $"Expected: {totalComfort} comfort (if all succeed)";
            }
            
            return $"{Session.CurrentPatience} turns remaining • Each turn advances time";
        }

        protected bool ShowOpportunityWarning()
        {
            return SelectedAction == ActionType.Listen && HasOpportunities();
        }

        private List<ConversationCard> GetObservationCards()
        {
            // This would get observation cards from the game state
            // For now, return empty list
            return new List<ConversationCard>();
        }

        protected EmotionalState GetNPCStartingState()
        {
            if (Session?.NPC == null) return EmotionalState.NEUTRAL;
            
            // Determine starting state from letter deadlines as per docs
            var obligations = LetterQueueManager.GetActiveObligations();
            var npcLetters = obligations.Where(o => o.SenderId == Session.NPC.ID || o.SenderName == Session.NPC.Name);
            var mostUrgent = npcLetters.OrderBy(o => o.DeadlineInMinutes).FirstOrDefault();
            
            if (mostUrgent == null) return EmotionalState.NEUTRAL;
            
            // Apply rules from conversation-system.md line 385-391
            if (mostUrgent.Stakes == StakeType.SAFETY && mostUrgent.DeadlineInMinutes < 360) // <6 hours
                return EmotionalState.DESPERATE;
            if (mostUrgent.DeadlineInMinutes < 720) // <12 hours
                return EmotionalState.TENSE;
            
            return EmotionalState.NEUTRAL;
        }

        protected int? GetMinutesUntilDeadline()
        {
            if (Session?.NPC == null) return null;
            
            // Check for meeting obligation first
            var meeting = GetMeetingObligation();
            if (meeting != null)
            {
                return meeting.DeadlineInMinutes;
            }
            
            var obligations = LetterQueueManager.GetActiveObligations();
            var npcLetters = obligations.Where(o => o.SenderId == Session.NPC.ID || o.SenderName == Session.NPC.Name);
            var mostUrgent = npcLetters.OrderBy(o => o.DeadlineInMinutes).FirstOrDefault();
            
            return mostUrgent?.DeadlineInMinutes;
        }

        protected MeetingObligation GetMeetingObligation()
        {
            if (Session?.NPC == null) return null;
            return LetterQueueManager.GetMeetingWithNPC(Session.NPC.ID);
        }
    }
}