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
        [Parameter] public ConversationType ConversationType { get; set; } = ConversationType.FriendlyChat;
        [Parameter] public EventCallback OnConversationEnd { get; set; }

        [Inject] protected ConversationManager ConversationManager { get; set; }
        [Inject] protected GameFacade GameFacade { get; set; }
        [Inject] protected TimeManager TimeManager { get; set; }
        [Inject] protected NavigationManager Navigation { get; set; }
        [Inject] protected ObligationQueueManager LetterQueueManager { get; set; }
        [Inject] protected TimeBlockAttentionManager AttentionManager { get; set; }
        [Inject] protected TokenMechanicsManager TokenManager { get; set; }
        [Inject] protected ObservationManager ObservationManager { get; set; }

        protected ConversationSession Session { get; set; }
        protected HashSet<CardInstance> SelectedCards { get; set; } = new();
        protected ActionType SelectedAction { get; set; } = ActionType.None;
        protected CardPlayResult LastResult { get; set; }
        protected CardInstance CurrentConversationCard { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                // ConversationType is now passed directly as parameter
                // No need to get from NavigationCoordinator
                
                // Get any observation cards from GameFacade
                var observationCards = GetObservationCards();
                
                // Start the conversation with the specified type
                Session = ConversationManager.StartConversation(NpcId, ConversationType, observationCards);
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

        protected void ToggleCard(CardInstance card)
        {
            if (SelectedAction != ActionType.Speak) return;

            if (SelectedCards.Contains(card))
            {
                SelectedCards.Remove(card);
            }
            else if (CanSelectCard(card))
            {
                // Check if we can add this card
                var tempSelection = new HashSet<CardInstance>(SelectedCards) { card };
                if (ConversationManager.CanSelectCard(card, SelectedCards))
                {
                    SelectedCards.Add(card);
                }
            }
            StateHasChanged();
        }

        protected bool CanSelectCard(CardInstance card)
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
                LastResult = await ConversationManager.ExecuteSpeak(SelectedCards);
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
                // ONE-CARD RULE: Must have exactly one card selected
                return SelectedCards.Count == 1;
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
            var location = GameFacade.GetCurrentLocation();
            var spot = GameFacade.GetCurrentLocationSpot();
            return new List<string> { location?.Name ?? "Unknown", spot?.Name ?? "Somewhere" };
        }

        protected string GetCurrentSpot()
        {
            var spot = GameFacade.GetCurrentLocationSpot();
            return spot?.Name ?? "Somewhere";
        }

        protected string GetSpotAtmosphere()
        {
            var spot = GameFacade.GetCurrentLocationSpot();
            if (spot == null) 
            {
                return "";
            }

            var currentTime = TimeManager.GetCurrentTimeBlock();
            
            // Get all active properties (base + time-specific)
            var activeProperties = new List<SpotPropertyType>(spot.SpotProperties);
            if (spot.TimeSpecificProperties.ContainsKey(currentTime))
            {
                activeProperties.AddRange(spot.TimeSpecificProperties[currentTime]);
            }

            // Create immersive descriptions based on properties
            var descriptions = new List<string>();
            
            // Privacy descriptions
            if (activeProperties.Contains(SpotPropertyType.Private))
                descriptions.Add("away from prying eyes");
            else if (activeProperties.Contains(SpotPropertyType.Discrete))
                descriptions.Add("tucked into a quiet corner");
            else if (activeProperties.Contains(SpotPropertyType.Exposed))
                descriptions.Add("in full view of everyone");
                
            // Atmosphere descriptions
            if (activeProperties.Contains(SpotPropertyType.Quiet))
                descriptions.Add("peaceful and undisturbed");
            else if (activeProperties.Contains(SpotPropertyType.Loud))
                descriptions.Add("bustling with activity");
                
            // Comfort descriptions
            if (activeProperties.Contains(SpotPropertyType.Warm))
                descriptions.Add("warmed by the nearby hearth");
            else if (activeProperties.Contains(SpotPropertyType.Shaded))
                descriptions.Add("cool in the shade");
                
            // Social descriptions
            if (activeProperties.Contains(SpotPropertyType.Crossroads))
                descriptions.Add("where paths cross");
            else if (activeProperties.Contains(SpotPropertyType.Isolated))
                descriptions.Add("removed from the crowds");
                
            // Build final description
            if (descriptions.Count == 0)
                return "";
                
            if (descriptions.Count == 1)
                return $"A spot {descriptions[0]}.";
                
            if (descriptions.Count == 2)
                return $"A spot {descriptions[0]} and {descriptions[1]}.";
                
            // More than 2 descriptions
            var allButLast = string.Join(", ", descriptions.Take(descriptions.Count - 1));
            return $"A spot {allButLast}, and {descriptions.Last()}.";
        }
        
        protected List<string> GetSpotProperties()
        {
            var spot = GameFacade.GetCurrentLocationSpot();
            if (spot == null) 
            {
                Console.WriteLine("[GetSpotProperties] Spot is null");
                return new List<string>();
            }

            Console.WriteLine($"[GetSpotProperties] Spot: {spot.Name}, ID: {spot.SpotID}");
            Console.WriteLine($"[GetSpotProperties] Base properties count: {spot.SpotProperties.Count}");
            foreach (var prop in spot.SpotProperties)
            {
                Console.WriteLine($"[GetSpotProperties] Base property: {prop}");
            }

            var properties = new List<string>();
            var currentTime = TimeManager.GetCurrentTimeBlock();
            
            // Get all active properties (base + time-specific)
            var activeProperties = new List<SpotPropertyType>(spot.SpotProperties);
            if (spot.TimeSpecificProperties.ContainsKey(currentTime))
            {
                activeProperties.AddRange(spot.TimeSpecificProperties[currentTime]);
            }

            // Map relevant properties to display strings
            Console.WriteLine($"[GetSpotProperties] Active properties count: {activeProperties.Count}");
            foreach (var prop in activeProperties)
            {
                Console.WriteLine($"[GetSpotProperties] Processing property: {prop}");
                switch (prop)
                {
                    case SpotPropertyType.Private:
                        properties.Add("Private");
                        break;
                    case SpotPropertyType.Discrete:
                        properties.Add("Discrete");
                        break;
                    case SpotPropertyType.Public:
                        properties.Add("Public");
                        break;
                    case SpotPropertyType.Exposed:
                        properties.Add("Exposed");
                        break;
                    case SpotPropertyType.Quiet:
                        properties.Add("Quiet");
                        break;
                    case SpotPropertyType.Loud:
                        properties.Add("Loud");
                        break;
                }
            }

            Console.WriteLine($"[GetSpotProperties] Returning {properties.Count} properties: {string.Join(", ", properties)}");
            return properties;
        }

        protected int GetSpotComfortModifier()
        {
            var spot = GameFacade.GetCurrentLocationSpot();
            if (spot == null || Session == null) return 0;
            
            var currentTime = TimeManager.GetCurrentTimeBlock();
            return spot.CalculateComfortModifier(Session.NPC.PersonalityType, currentTime);
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
            if (TokenManager == null) return 0;
            return TokenManager.GetTokenCount(type);
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

        protected string GetComfortName()
        {
            return Session.CurrentComfort switch
            {
                >= 20 => "(Perfect)",
                >= 15 => "(Deep)",
                >= 10 => "(Good)",
                >= 5 => "(Basic)",
                _ => "(Tentative)"
            };
        }

        protected int GetDrawCount()
        {
            var rules = ConversationRules.States[Session.CurrentState];
            return rules.CardsOnListen;
        }

        protected bool HasOpportunities()
        {
            return Session.HandCards.Any(c => c.Persistence == PersistenceType.Fleeting);
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

        protected string GetCardClasses(CardInstance card)
        {
            var classes = new List<string>();
            
            classes.Add(card.Type.ToString().ToLower());
            classes.Add(card.Persistence.ToString().ToLower());
            
            // Add card category class for visual styling
            classes.Add(card.GetCategoryClass());
            
            return string.Join(" ", classes);
        }
        
        protected string GetObservationSourceDisplay(string source)
        {
            if (string.IsNullOrEmpty(source)) return "";
            
            // Convert internal IDs to human-readable text
            return source switch
            {
                "merchant_negotiations" => "From Observation",
                "guard_movements" => "From Observation", 
                "noble_gossip" => "From Observation",
                "worker_complaints" => "From Observation",
                _ => "From Observation"
            };
        }

        protected string GetStateChangeText(CardInstance card)
        {
            if (card.Category != CardCategory.State) return "";
            
            if (card.SuccessState.HasValue)
            {
                return $"{Session.CurrentState} → {card.SuccessState.Value}";
            }
            return "State change";
        }
        
        protected string GetSuccessEffect(CardInstance card)
        {
            if (card.Category == CardCategory.State && card.SuccessState.HasValue)
            {
                return $"{Session.CurrentState} → {card.SuccessState.Value}";
            }
            if (card.CanDeliverLetter)
            {
                return $"+{card.BaseComfort} comfort + deliver";
            }
            return $"+{card.BaseComfort} comfort";
        }

        // Exchange Mode Methods
        protected CardInstance GetCurrentConversationCard()
        {
            if (Session?.NPC == null || ConversationType != ConversationType.Commerce)
                return null;
                
            // Exchange cards are just regular conversation cards in the hand
            if (Session.HandCards.Count > 0)
            {
                // Return the first exchange card in hand
                return Session.HandCards.FirstOrDefault(c => c.IsExchange);
            }
            
            return null;
        }

        // DELETED AcceptExchange, DeclineExchange, and CanAffordExchange methods
        // Exchanges now use standard SPEAK action with card selection
        
        // Player Resource Methods
        protected int GetPlayerCoins()
        {
            return GameFacade?.GetPlayer()?.Coins ?? 0;
        }
        
        protected int GetPlayerHealth()
        {
            return GameFacade?.GetPlayer()?.Health ?? 100;
        }
        
        protected int GetPlayerHunger()
        {
            // Hunger is tracked separately from Player in the game rules
            return 0; // TODO: Get from hunger tracking system when implemented
        }
        
        protected int GetPlayerAttention()
        {
            var currentTime = TimeManager?.GetCurrentTimeBlock() ?? TimeBlocks.Morning;
            var attentionMgr = AttentionManager?.GetCurrentAttention(currentTime);
            return attentionMgr?.Current ?? 0;
        }
        
        protected int GetMaxAttention()
        {
            return 10; // Base max attention
        }
        
        protected string GetCurrentTimeBlock()
        {
            return TimeManager?.GetCurrentTimeBlock().ToString() ?? "Unknown";
        }

        protected string GetFailureEffect(CardInstance card)
        {
            if (card.Category == CardCategory.State && card.FailureState.HasValue)
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

        protected string GetConversationCardName(CardInstance card)
        {
            if (card.Context?.ExchangeData == null)
                return "Exchange";
                
            var exchange = card.Context.ExchangeData;
            
            // Use ExchangeName if available
            if (!string.IsNullOrEmpty(exchange.ExchangeName))
                return exchange.ExchangeName;
            
            // Use template ID to determine exchange type
            if (!string.IsNullOrEmpty(exchange.TemplateId))
            {
                return exchange.TemplateId switch
                {
                    "food_exchange" => "Food Exchange",
                    "healing_exchange" => "Healing Service",
                    "information_exchange" => "Information Trade",
                    "work_exchange" => "Labor Exchange",
                    "favor_exchange" => "Favor Trade",
                    "rest_exchange" => "Rest Service",
                    _ => "Resource Exchange"
                };
            }
            
            return "Resource Exchange";
        }
        
        protected string GetExchangeCostText(CardInstance card)
        {
            if (card.Context?.ExchangeData == null)
                return "";
                
            var exchange = card.Context.ExchangeData;
            var costs = exchange.Cost.Select(c => c.GetDisplayText());
            return string.Join(", ", costs);
        }
        
        protected string GetExchangeRewardText(CardInstance card)
        {
            if (card.Context?.ExchangeData == null)
                return "";
                
            var exchange = card.Context.ExchangeData;
            var rewards = exchange.Reward.Select(r => r.GetDisplayText());
            return string.Join(", ", rewards);
        }
        
        protected bool IsConversationCard(CardInstance card)
        {
            return card.Mechanics == CardMechanics.Exchange;
        }
        
        protected string GetCardDisplayName(CardInstance card)
        {
            // Special handling for exchange cards
            if (card.Mechanics == CardMechanics.Exchange)
                return GetConversationCardName(card);
                
            // Generate a display name based on the template ID
            return card.TemplateId switch
            {
                "OfferHelp" => "Offer Assistance",
                "ActiveListening" => "Listen Actively",
                "CasualInquiry" => "Casual Question",
                "DiscussBusiness" => "Discuss Business",
                "OfferWork" => "Propose Work",
                "DiscussPolitics" => "Political Discussion",
                "MakePromise" => "Make Promise",
                "ShowRespect" => "Show Respect",
                "ShareInformation" => "Share Information",
                "ImplyKnowledge" => "Hint at Knowledge",
                "RequestDiscretion" => "Request Discretion",
                "SimpleGreeting" => "Simple Greeting",
                "DesperateRequest" => "DESPERATE PLEA",
                "OpeningUp" => "Open Up",
                "CalmnessAttempt" => "Calm Reassurance",
                "DiscussObligation" => "Discuss Obligations",
                "DeliverLetter" => "Deliver Letter",
                "ShowingTension" => "Show Tension",
                "ExpressEmpathy" => "Express Empathy",
                "SharePersonal" => "Share Personal",
                "ProposeDeal" => "Propose Deal",
                "NegotiateTerms" => "Negotiate Terms",
                "AcknowledgePosition" => "Acknowledge Position",
                "ShareSecret" => "Share Secret",
                "MentionLetter" => "Mention Letter",
                _ => "Conversation Option"
            };
        }

        protected bool ShowOpportunityWarning()
        {
            return SelectedAction == ActionType.Listen && HasOpportunities();
        }

        protected string GetPersistenceIcon(PersistenceType persistence)
        {
            return persistence switch
            {
                PersistenceType.Persistent => "♻",
                PersistenceType.Fleeting => "⏱",
                _ => ""
            };
        }

        private List<CardInstance> GetObservationCards()
        {
            // Get observation cards from the ObservationManager and convert to card instances
            var observationCards = ObservationManager.GetObservationCardsAsConversationCards();
            return observationCards.Select(card => new CardInstance(card, "conversation")).ToList();
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