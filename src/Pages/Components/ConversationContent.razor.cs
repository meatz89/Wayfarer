using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Wayfarer.Pages.Components
{
    // Data classes for card dialogues JSON
    public class CardDialogues
    {
        public Dictionary<string, CardDialogue> dialogues { get; set; }
        public Dictionary<string, Dictionary<string, Dictionary<string, string>>> narrativeTemplates { get; set; }
    }

    public class SystemNarratives
    {
        public ConversationNarratives conversationNarratives { get; set; }
        public SystemMessages systemMessages { get; set; }
        public Dictionary<string, string> observationCardNames { get; set; }
        public Dictionary<string, string> observationCardDialogues { get; set; }
        public string burdenCardDialogue { get; set; }
        public Dictionary<string, string> exchangeCardDialogues { get; set; }
        public Dictionary<string, string> comfortCardDialogues { get; set; }
        public Dictionary<string, string> tokenCardDialogues { get; set; }
        public string letterCardDialogue { get; set; }
        public string defaultCardDialogue { get; set; }
    }

    public class ConversationNarratives
    {
        public string initialNarrative { get; set; }
        public Dictionary<string, string> listenNarratives { get; set; }
        public Dictionary<string, string> speakNarratives { get; set; }
        public Dictionary<string, string> stateDialogues { get; set; }
        public Dictionary<string, string> initialDialogues { get; set; }
        public Dictionary<string, string> comfortResponses { get; set; }
        public Dictionary<string, string> stateTransitionVerbs { get; set; }
        public Dictionary<string, string> cardStateTransitionDialogues { get; set; }
    }

    public class SystemMessages
    {
        public string listeningCarefully { get; set; }
        public string drewCards { get; set; }
        public string decliningExchange { get; set; }
        public string tradingExchange { get; set; }
        public string playingCard { get; set; }
        public string cardsSucceeded { get; set; }
        public string cardsFailed { get; set; }
        public Dictionary<string, string> conversationExhausted { get; set; }
        public Dictionary<string, string> letterNegotiation { get; set; }
    }

    public class CardDialogue
    {
        public string playerText { get; set; }
        public Dictionary<string, string> contextual { get; set; }
    }

    public class ConversationContentBase : ComponentBase
    {
        [Parameter] public ConversationContext Context { get; set; }
        [Parameter] public EventCallback OnConversationEnd { get; set; }
        [CascadingParameter] public GameScreenBase GameScreen { get; set; }

        [Inject] protected ConversationFacade ConversationFacade { get; set; }
        [Inject] protected GameFacade GameFacade { get; set; }

        protected ConversationSession Session { get; set; }
        protected HashSet<CardInstance> SelectedCards { get; set; } = new();
        protected int TotalSelectedWeight => SelectedCards.Sum(c => c.GetEffectiveWeight(Session?.CurrentState ?? EmotionalState.NEUTRAL));
        protected bool IsProcessing { get; set; }
        protected bool IsConversationExhausted { get; set; } = false;
        protected string ExhaustionReason { get; set; } = "";

        protected string NpcName { get; set; }
        protected string LastNarrative { get; set; }
        protected string LastDialogue { get; set; }
        // Letter generation is handled by ConversationManager based on emotional state

        // Card dialogues cache
        private static CardDialogues _cardDialogues;
        private static bool _cardDialoguesLoaded = false;

        // System narratives cache
        private static SystemNarratives _systemNarratives;
        private static bool _systemNarrativesLoaded = false;

        // Get current token balances with this NPC
        protected Dictionary<ConnectionType, int> CurrentTokens
        {
            get
            {
                if (Context?.NpcId != null && GameFacade != null)
                {
                    NPCTokenBalance tokenBalance = GameFacade.GetTokensWithNPC(Context.NpcId);
                    Dictionary<ConnectionType, int> result = new Dictionary<ConnectionType, int>();
                    foreach (TokenBalance balance in tokenBalance.Balances)
                    {
                        result[balance.TokenType] = balance.Amount;
                    }
                    return result;
                }
                return new Dictionary<ConnectionType, int>();
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await InitializeFromContext();
        }

        protected override async Task OnParametersSetAsync()
        {
            Console.WriteLine($"[ConversationContent.OnParametersSetAsync] Context parameter: '{Context?.NpcId}'");
            if (Context != null && Session?.NPC?.ID != Context.NpcId)
            {
                await InitializeFromContext();
            }
        }

        private async Task InitializeFromContext()
        {
            try
            {
                if (Context == null || !Context.IsValid)
                {
                    Console.WriteLine($"[ConversationContent.InitializeFromContext] Invalid context: {Context?.ErrorMessage}");
                    await OnConversationEnd.InvokeAsync();
                    return;
                }

                Console.WriteLine($"[ConversationContent.InitializeFromContext] Initializing with NpcId: '{Context.NpcId}'");

                // Use the conversation session from the context
                Session = Context.Session;
                NpcName = Context.Npc?.Name ?? "Unknown";

                // Generate initial narrative
                LoadSystemNarratives();
                LastNarrative = _systemNarratives?.conversationNarratives?.initialNarrative ?? "The conversation begins...";
                LastDialogue = GetInitialDialogue();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ConversationContent] Failed to initialize from context: {ex.Message}");
                await OnConversationEnd.InvokeAsync();
            }
        }


        protected async Task ExecuteListen()
        {
            if (IsProcessing || Session == null) return;

            IsProcessing = true;
            SelectedCards.Clear();

            try
            {
                // Add notification for listening
                MessageSystem? messageSystem = GameFacade?.GetMessageSystem();
                if (messageSystem != null)
                {
                    LoadSystemNarratives();
                    string message = _systemNarratives?.systemMessages?.listeningCarefully ?? "You listen carefully...";
                    messageSystem.AddSystemMessage(message, SystemMessageTypes.Info);
                }

                ConversationFacade.ExecuteListen();

                // Generate narrative for the action
                GenerateListenNarrative();

                // Notify about cards drawn
                if (messageSystem != null && Session.HandCards.Any())
                {
                    string drewMessage = _systemNarratives?.systemMessages?.drewCards ?? "Drew {0} cards";
                    messageSystem.AddSystemMessage(string.Format(drewMessage, Session.HandCards.Count), SystemMessageTypes.Success);
                }

                // Refresh resources after listen action
                if (GameScreen != null)
                {
                    await GameScreen.RefreshResourceDisplay();
                }

                if (Session.ShouldEnd())
                {
                    // Don't auto-exit! Set exhaustion state instead
                    IsConversationExhausted = true;
                    ExhaustionReason = GetConversationEndReason();

                    // Add notification about conversation ending
                    if (messageSystem != null)
                    {
                        messageSystem.AddSystemMessage(ExhaustionReason, SystemMessageTypes.Info);
                    }

                    // Don't invoke end - let player click button
                    // await OnConversationEnd.InvokeAsync();
                }
            }
            finally
            {
                IsProcessing = false;
                StateHasChanged();
            }
        }

        protected async Task ExecuteSpeak()
        {
            if (IsProcessing || Session == null || !SelectedCards.Any()) return;

            IsProcessing = true;

            try
            {
                // Add notification for speaking
                MessageSystem? messageSystem = GameFacade?.GetMessageSystem();

                // Check if this is an exchange card
                CardInstance? exchangeCard = SelectedCards.FirstOrDefault(c => c.Category == nameof(CardCategory.Exchange));
                if (exchangeCard != null && messageSystem != null)
                {
                    // For exchanges, show what's being traded
                    if (exchangeCard.Context?.ExchangeCost != null && exchangeCard.Context?.ExchangeReward != null)
                    {
                        if (exchangeCard.Context.ExchangeName == "Pass on this offer")
                        {
                            string decliningMsg = _systemNarratives?.systemMessages?.decliningExchange ?? "Declining the exchange...";
                            messageSystem.AddSystemMessage(decliningMsg, SystemMessageTypes.Info);
                        }
                        else
                        {
                            string tradingMsg = _systemNarratives?.systemMessages?.tradingExchange ?? "Trading: {0} for {1}";
                            messageSystem.AddSystemMessage(string.Format(tradingMsg, exchangeCard.Context.ExchangeCost, exchangeCard.Context.ExchangeReward), SystemMessageTypes.Info);
                        }
                    }
                }
                else if (messageSystem != null)
                {
                    // ONE-CARD RULE: Always exactly one card
                    CardInstance? selectedCard = SelectedCards.FirstOrDefault();
                    if (selectedCard != null)
                    {
                        string playingMsg = _systemNarratives?.systemMessages?.playingCard ?? "Playing {0}...";
                        messageSystem.AddSystemMessage(string.Format(playingMsg, GetCardName(selectedCard)), SystemMessageTypes.Info);
                    }
                }

                // ExecuteSpeak expects HashSet<ConversationCard>
                // CRITICAL: Must use ConversationManager.ExecuteSpeak to handle special card effects like letter delivery
                CardPlayResult result = await ConversationFacade.ExecuteSpeak(SelectedCards);
                ProcessSpeakResult(result);

                // Add detailed notification for result
                // NOTE: Exchange handling is done in ConversationManager.HandleSpecialCardEffectsAsync
                // Letter delivery is also handled there - no need to duplicate logic here
                if (messageSystem != null && result != null)
                {
                    if (result.Results?.Any() == true)
                    {
                        int successes = result.Results.Count(r => r.Success);
                        int failures = result.Results.Count(r => !r.Success);

                        if (successes > 0)
                        {
                            string successMsg = _systemNarratives?.systemMessages?.cardsSucceeded ?? "{0} card(s) succeeded! +{1} comfort";
                            messageSystem.AddSystemMessage(string.Format(successMsg, successes, result.TotalComfort), SystemMessageTypes.Success);
                        }
                        if (failures > 0)
                        {
                            string failureMsg = _systemNarratives?.systemMessages?.cardsFailed ?? "{0} card(s) failed";
                            messageSystem.AddSystemMessage(string.Format(failureMsg, failures), SystemMessageTypes.Warning);
                        }
                    }
                }

                SelectedCards.Clear();

                // Refresh resources after exchange/card play
                if (GameScreen != null)
                {
                    await GameScreen.RefreshResourceDisplay();
                }

                if (Session.ShouldEnd())
                {
                    // Don't auto-exit! Set exhaustion state instead
                    IsConversationExhausted = true;
                    ExhaustionReason = GetConversationEndReason();

                    // Add notification about conversation ending
                    if (messageSystem != null)
                    {
                        messageSystem.AddSystemMessage(ExhaustionReason, SystemMessageTypes.Info);
                    }

                    // Don't invoke end - let player click button
                    // await OnConversationEnd.InvokeAsync();
                }
            }
            finally
            {
                IsProcessing = false;
                StateHasChanged();
            }
        }

        private void ProcessSpeakResult(CardPlayResult result)
        {
            if (result != null)
            {
                // Generate narrative based on the result
                GenerateSpeakNarrative(result);

                // Process letter negotiations first (new system)
                if (result.LetterNegotiations?.Any() == true)
                {
                    ProcessLetterNegotiations(result.LetterNegotiations);
                }
                // Letter generation is handled by ConversationManager based on emotional state
                // Special cards that force letter generation are handled in ConversationManager.HandleSpecialCardEffectsAsync()
            }
        }

        private void GenerateListenNarrative()
        {
            // Generate narrative for listen action based on state
            LoadSystemNarratives();
            ConversationStateRules stateRules = ConversationRules.States[Session.CurrentState];

            if (_systemNarratives?.conversationNarratives?.listenNarratives != null)
            {
                string stateKey = Session.CurrentState.ToString();
                if (_systemNarratives.conversationNarratives.listenNarratives.TryGetValue(stateKey, out string? narrative))
                {
                    LastNarrative = narrative;
                }
                else
                {
                    LastNarrative = _systemNarratives.conversationNarratives.listenNarratives.GetValueOrDefault("default", "You listen attentively...");
                }
            }
            else
            {
                LastNarrative = "You listen attentively...";
            }

            LastDialogue = GetStateTransitionDialogue(Session.CurrentState);
        }

        private void GenerateSpeakNarrative(CardPlayResult result)
        {
            LoadSystemNarratives();
            // Generate narrative based on cards played and result
            if (result.Results != null && result.Results.Any())
            {
                int successCount = result.Results.Count(r => r.Success);
                int totalCards = result.Results.Count;

                if (_systemNarratives?.conversationNarratives?.speakNarratives != null)
                {
                    if (successCount == totalCards)
                    {
                        LastNarrative = _systemNarratives.conversationNarratives.speakNarratives.GetValueOrDefault("allSuccess", "Your words resonate perfectly...");
                    }
                    else if (successCount > 0)
                    {
                        LastNarrative = _systemNarratives.conversationNarratives.speakNarratives.GetValueOrDefault("partialSuccess", "Some of your words find their mark...");
                    }
                    else
                    {
                        LastNarrative = _systemNarratives.conversationNarratives.speakNarratives.GetValueOrDefault("allFailure", "Your words fall flat...");
                    }
                }
                else
                {
                    // Fallback to hardcoded values if JSON not loaded
                    if (successCount == totalCards)
                        LastNarrative = "Your words resonate perfectly...";
                    else if (successCount > 0)
                        LastNarrative = "Some of your words find their mark...";
                    else
                        LastNarrative = "Your words fall flat...";
                }

                // Add comfort gained info
                if (result.TotalComfort > 0)
                {
                    LastNarrative += $" (Comfort +{result.TotalComfort})";
                }
            }
            else
            {
                LastNarrative = _systemNarratives?.conversationNarratives?.speakNarratives?.GetValueOrDefault("default", "You speak your mind...") ?? "You speak your mind...";
            }

            // Update dialogue based on new state if changed
            if (result.NewState.HasValue)
            {
                LastDialogue = GetStateTransitionDialogue(result.NewState.Value);
            }
            else
            {
                LastDialogue = GetResponseDialogue();
            }
        }

        private string GetStateTransitionDialogue(EmotionalState newState)
        {
            LoadSystemNarratives();
            if (_systemNarratives?.conversationNarratives?.stateDialogues != null)
            {
                string stateKey = newState.ToString();
                if (_systemNarratives.conversationNarratives.stateDialogues.TryGetValue(stateKey, out string? dialogue))
                {
                    return dialogue;
                }
                return _systemNarratives.conversationNarratives.stateDialogues.GetValueOrDefault("default", "Hmm...");
            }

            // Fallback if JSON not loaded - 5 states only
            return newState switch
            {
                EmotionalState.DESPERATE => "Please, I need your help urgently!",
                EmotionalState.TENSE => "This is making me uncomfortable...",
                EmotionalState.NEUTRAL => "Alright, let's talk.",
                EmotionalState.OPEN => "I'm glad we're having this conversation.",
                EmotionalState.CONNECTED => "I feel like you really understand me.",
                _ => "Hmm..."
            };
        }

        private string GetResponseDialogue()
        {
            LoadSystemNarratives();
            // Generate response based on current comfort level (-3 to +3)
            if (_systemNarratives?.conversationNarratives?.comfortResponses != null)
            {
                if (Session.CurrentComfort >= 2)
                {
                    return _systemNarratives.conversationNarratives.comfortResponses.GetValueOrDefault("veryPositive", "This conversation has been wonderful!");
                }
                else if (Session.CurrentComfort >= 0)
                {
                    return _systemNarratives.conversationNarratives.comfortResponses.GetValueOrDefault("positive", "I appreciate you taking the time to talk.");
                }
                else if (Session.CurrentComfort >= -2)
                {
                    return _systemNarratives.conversationNarratives.comfortResponses.GetValueOrDefault("neutral", "I see what you mean...");
                }
                else
                {
                    return _systemNarratives.conversationNarratives.comfortResponses.GetValueOrDefault("negative", "I'm not sure about this...");
                }
            }

            // Fallback if JSON not loaded
            if (Session.CurrentComfort >= 2)
                return "This conversation has been wonderful!";
            else if (Session.CurrentComfort >= 0)
                return "I appreciate you taking the time to talk.";
            else if (Session.CurrentComfort >= -2)
                return "I see what you mean...";
            else
                return "I'm not sure about this...";
        }

        // Letter generation removed - handled by ConversationManager.TryGenerateLetter()
        // This avoids duplicate logic and ensures letters are generated based on emotional state
        private void GenerateLetter()
        {
            // This method is no longer used
            // Letter generation is handled by ConversationManager based on emotional state
        }

        // Letter tier determination removed - handled by ConversationManager
        private LetterTier DetermineLetterTier(int comfort)
        {
            // No longer used - ConversationManager determines letter properties based on linear scaling
            return LetterTier.Simple;
        }

        // Letter creation removed - handled by ConversationManager
        private DeliveryObligation CreateLetterFromComfort(LetterTier tier)
        {
            // For now, hardcode a recipient since we don't have access to all NPCs
            // In a real implementation, this would select from available NPCs
            string recipientId = "merchant_thomas"; // Default recipient
            string recipientName = "Thomas the Merchant";

            // Determine letter parameters based on tier
            (int deadline, int payment, StakeType stakes, EmotionalWeight weight) = GetTierParameters(tier);

            // Get the NPC from context
            NPC? npc = Context?.Npc;
            if (npc == null) return null;

            // Determine token type from NPC's available types
            ConnectionType tokenType = DetermineTokenType(npc);

            return new DeliveryObligation
            {
                Id = Guid.NewGuid().ToString(),
                SenderId = npc.ID,
                SenderName = npc.Name,
                RecipientId = recipientId,
                RecipientName = recipientName,
                TokenType = tokenType,
                DeadlineInMinutes = deadline,
                Payment = payment,
                Stakes = stakes,
                EmotionalWeight = weight,
                Tier = ConvertToTierLevel(tier),
                Description = GenerateLetterDescription(npc.Name, recipientName, tier),
                GenerationReason = $"Generated from conversation with {npc.Name}"
            };
        }

        private (int deadline, int payment, StakeType stakes, EmotionalWeight weight) GetTierParameters(LetterTier tier)
        {
            // EXACT specifications as requested
            return tier switch
            {
                LetterTier.Simple => (1440, 5, StakeType.REPUTATION, EmotionalWeight.LOW),      // 24h, 5 coins
                LetterTier.Important => (720, 10, StakeType.WEALTH, EmotionalWeight.MEDIUM),    // 12h, 10 coins
                LetterTier.Urgent => (360, 15, StakeType.STATUS, EmotionalWeight.HIGH),         // 6h, 15 coins
                LetterTier.Critical => (120, 20, StakeType.SAFETY, EmotionalWeight.CRITICAL),   // 2h, 20 coins
                _ => (1440, 5, StakeType.REPUTATION, EmotionalWeight.LOW)
            };
        }

        private ConnectionType DetermineTokenType(NPC npc)
        {
            // Use NPC's primary letter token type, or default to Trust
            if (npc.LetterTokenTypes != null && npc.LetterTokenTypes.Any())
            {
                return npc.LetterTokenTypes.First();
            }
            return ConnectionType.Trust;
        }

        private TierLevel ConvertToTierLevel(LetterTier tier)
        {
            return tier switch
            {
                LetterTier.Simple => TierLevel.T1,
                LetterTier.Important => TierLevel.T2,
                LetterTier.Urgent => TierLevel.T3,
                LetterTier.Critical => TierLevel.T3, // Map Critical to T3 as there's no T4
                _ => TierLevel.T1
            };
        }

        private string GenerateLetterDescription(string senderName, string recipientName, LetterTier tier)
        {
            return tier switch
            {
                LetterTier.Simple => $"A routine message from {senderName} to {recipientName}",
                LetterTier.Important => $"Important correspondence requiring timely delivery",
                LetterTier.Urgent => $"Urgent matter that cannot wait",
                LetterTier.Critical => $"CRITICAL: Lives may depend on this delivery",
                _ => $"Letter from {senderName}"
            };
        }

        private string GetTierDescription(LetterTier tier)
        {
            return tier switch
            {
                LetterTier.Simple => "simple",
                LetterTier.Important => "important",
                LetterTier.Urgent => "urgent",
                LetterTier.Critical => "CRITICAL",
                _ => "standard"
            };
        }

        // Internal enum for letter tiers
        private enum LetterTier
        {
            Simple,    // 5-9 comfort
            Important, // 10-14 comfort
            Urgent,    // 15-19 comfort
            Critical   // 20+ comfort
        }

        protected void ToggleCardSelection(CardInstance card)
        {
            // ONE CARD RULE: Only one card can be selected at a time for SPEAK action

            if (SelectedCards.Contains(card))
            {
                // Deselect the card
                SelectedCards.Remove(card);
            }
            else if (CanSelectCard(card))
            {
                // ONE CARD RULE: Clear any existing selection before selecting new card
                SelectedCards.Clear();
                SelectedCards.Add(card);
            }
            // If card can't be selected (over weight limit), do nothing

            StateHasChanged();
        }

        protected bool CanSelectCard(CardInstance card)
        {
            if (Session == null) return false;

            // Check if observation card is expired
            if (IsObservationExpired(card)) return false;

            // Check weight limit - card must not exceed current state's max weight
            int effectiveWeight = card.GetEffectiveWeight(Session.CurrentState);
            return effectiveWeight <= GetWeightLimit();
        }

        protected bool IsCardSelected(CardInstance card)
        {
            return SelectedCards.Contains(card);
        }

        protected string GetCardTypeLabel(CardInstance card)
        {
            if (card == null) return "Card";

            return card.Category switch
            {
                nameof(CardCategory.Comfort) => "Comfort",
                nameof(CardCategory.State) => "State card",
                nameof(CardCategory.Token) => "Token",
                nameof(CardCategory.Patience) => "Patience",
                nameof(CardCategory.Exchange) => "Exchange",
                nameof(CardCategory.Promise) => "Promise",
                nameof(CardCategory.Burden) => "Burden",
                _ => card.IsObservation ? "Observation" : card.Type.ToString()
            };
        }

        protected bool CanSpeak()
        {
            return SelectedCards.Any() && TotalSelectedWeight <= GetWeightLimit();
        }

        protected async Task EndConversation()
        {
            // End the conversation properly to calculate and award tokens
            if (Session != null)
            {
                ConversationOutcome outcome = ConversationFacade.EndConversation();
                Console.WriteLine($"[ConversationContent] Conversation ended with outcome: Comfort={outcome.TotalComfort}, TokensEarned={outcome.TokensEarned}");
            }

            Session = null;
            await OnConversationEnd.InvokeAsync();
        }

        protected async Task ExitConversation()
        {
            // Allow player to manually exit conversation
            Console.WriteLine("[ConversationContent] Player manually exiting conversation");

            // End the conversation properly to calculate and award tokens
            if (Session != null)
            {
                ConversationOutcome outcome = ConversationFacade.EndConversation();
                Console.WriteLine($"[ConversationContent] Conversation ended with outcome: Comfort={outcome.TotalComfort}, TokensEarned={outcome.TokensEarned}");
            }

            Session = null;
            await OnConversationEnd.InvokeAsync();
        }

        // UI Helper Methods
        protected string GetConversationModeTitle()
        {
            ConversationType conversationType = Context?.Type ?? ConversationType.FriendlyChat;
            return conversationType switch
            {
                ConversationType.Commerce => "Quick Exchange",
                ConversationType.Resolution => "Burden Resolution",
                ConversationType.FriendlyChat => "Friendly Conversation",
                _ => "Conversation"
            };
        }

        protected string GetStateClass()
        {
            return Session?.CurrentState switch
            {
                EmotionalState.DESPERATE => "desperate",
                EmotionalState.TENSE => "tense",
                EmotionalState.NEUTRAL => "neutral",
                EmotionalState.OPEN => "open",
                EmotionalState.CONNECTED => "connected",
                _ => ""
            };
        }

        protected int GetWeightLimit()
        {
            if (Session == null) return 3;

            // Use actual values from ConversationRules.States
            if (ConversationRules.States.TryGetValue(Session.CurrentState, out ConversationStateRules? rules))
            {
                return rules.MaxWeight;
            }

            return 3; // Default fallback
        }

        protected string GetListenDetails()
        {
            if (Session == null) return "";

            return Session.CurrentState switch
            {
                EmotionalState.DESPERATE => "Draw 1 card",
                EmotionalState.TENSE => "Draw 2 cards",
                EmotionalState.NEUTRAL => "Draw 2 cards",
                EmotionalState.OPEN => "Draw 3 cards",
                EmotionalState.CONNECTED => "Draw 3 cards",
                _ => "Draw 2 cards"
            };
        }

        protected string GetSpeakDetails()
        {
            return $"Weight limit: {GetWeightLimit()}";
        }

        protected string GetStateEffects()
        {
            if (Session == null) return "";

            return Session.CurrentState switch
            {
                EmotionalState.DESPERATE => "• Draw 1 • Weight limit 3 • Ends at -3 comfort",
                EmotionalState.TENSE => "• Draw 2 • Weight limit 4",
                EmotionalState.NEUTRAL => "• Draw 2 • Weight limit 5",
                EmotionalState.OPEN => "• Draw 3 • Weight limit 5",
                EmotionalState.CONNECTED => "• Draw 3 • Weight limit 6",
                _ => ""
            };
        }


        protected string GetComfortLabel()
        {
            if (Session == null) return "None";
            return Session.CurrentComfort switch
            {
                3 => "Perfect Understanding",
                2 => "Deep Connection",
                1 => "Good Rapport",
                0 => "Neutral",
                -1 => "Uncertain",
                -2 => "Tense",
                -3 => "Breaking Down",
                _ => "Unknown"
            };
        }

        protected int GetComfortProgress()
        {
            if (Session == null) return 50; // Center position for 0
            // Map -3 to +3 to 0% to 100%
            return (int)((Session.CurrentComfort + 3) * 100 / 6.0);
        }

        protected string GetComfortDotClass(int dotPosition)
        {
            if (Session == null) return "";

            List<string> classes = new List<string>();

            // Always add color class based on position
            if (dotPosition < 0)
                classes.Add("negative");
            else if (dotPosition > 0)
                classes.Add("positive");
            // Position 0 gets no color class (neutral)

            // Add current class if this is the current position
            if (dotPosition == Session.CurrentComfort)
                classes.Add("current");

            return string.Join(" ", classes);
        }

        protected string GetEmotionalStateDisplay()
        {
            if (Session == null) return "Unknown";

            return Session.CurrentState switch
            {
                EmotionalState.DESPERATE => "Desperate",
                EmotionalState.TENSE => "Tense",
                EmotionalState.NEUTRAL => "Neutral",
                EmotionalState.OPEN => "Open",
                EmotionalState.CONNECTED => "Connected",
                _ => Session.CurrentState.ToString()
            };
        }

        protected string GetListenActionText()
        {
            if (Session == null) return "Draw cards";

            return Session.CurrentState switch
            {
                EmotionalState.DESPERATE => "Draw desperate cards",
                EmotionalState.TENSE => "Draw tense cards",
                EmotionalState.NEUTRAL => "Draw neutral cards",
                EmotionalState.OPEN => "Draw open cards",
                EmotionalState.CONNECTED => "Draw connected cards",
                _ => "Draw cards"
            };
        }

        protected string GetSpeakActionText()
        {
            int limit = GetWeightLimit();
            return $"Play weight {limit} cards";
        }

        protected string GetProperCardName(CardInstance card)
        {
            // Generate meaningful card names based on type and category
            if (card.Category == nameof(CardCategory.Exchange) && card.Context?.ExchangeName != null)
                return card.Context.ExchangeName;

            if (card.Category == nameof(CardCategory.Burden))
                return "Address Past Failure";

            if (card.Category == nameof(CardCategory.State))
            {
                if (card.SuccessState.HasValue)
                {
                    string targetState = GetEmotionalStateForCard(card.SuccessState.Value);
                    return $"Let's {GetStateTransitionVerb(card.SuccessState.Value)}";
                }
                return "Change Approach";
            }

            if (card.IsObservation)
            {
                // Use context for observation names
                if (card.DisplayName != null)
                    return card.DisplayName;
                return "Share Observation";
            }

            // Comfort cards get contextual names (comfort cards have no specific type)
            if (card.Category == nameof(CardCategory.Comfort) && card.Type == CardType.Normal)
            {
                if (card.BaseComfort >= 2)
                    return "Deep Understanding";
                else if (card.BaseComfort == 1)
                    return "I Understand";
                else
                    return "Simple Response";
            }

            // Token cards are identified by having token types
            if (card.Type != CardType.Normal && card.Category == nameof(CardCategory.Comfort))
            {
                ConnectionType tokenType = card.GetConnectionType();
                return tokenType switch
                {
                    ConnectionType.Trust => "Build Trust",
                    ConnectionType.Commerce => "Discuss Business",
                    ConnectionType.Status => "Show Respect",
                    ConnectionType.Shadow => "Share Secret",
                    _ => "Connect"
                };
            }

            // Default to template ID with formatting
            return card.TemplateId?.Replace("_", " ") ?? "Unknown Card";
        }

        private string GetStateTransitionVerb(EmotionalState targetState)
        {
            LoadSystemNarratives();
            if (_systemNarratives?.conversationNarratives?.stateTransitionVerbs != null)
            {
                string stateKey = targetState.ToString();
                if (_systemNarratives.conversationNarratives.stateTransitionVerbs.TryGetValue(stateKey, out string? verb))
                {
                    return verb;
                }
                return _systemNarratives.conversationNarratives.stateTransitionVerbs.GetValueOrDefault("default", "change topics");
            }

            // Fallback if JSON not loaded
            return targetState switch
            {
                EmotionalState.DESPERATE => "stay urgent",
                EmotionalState.TENSE => "be careful",
                EmotionalState.NEUTRAL => "calm down",
                EmotionalState.OPEN => "open up",
                EmotionalState.CONNECTED => "connect deeply",
                _ => "change topics"
            };
        }

        private string GetEmotionalStateForCard(EmotionalState state)
        {
            return state switch
            {
                EmotionalState.DESPERATE => "desperate",
                EmotionalState.TENSE => "tense",
                EmotionalState.NEUTRAL => "neutral",
                EmotionalState.OPEN => "open",
                EmotionalState.CONNECTED => "connected",
                _ => state.ToString().ToLower()
            };
        }

        protected string GetProperCardDialogue(CardInstance card)
        {
            // Generate actual conversational dialogue instead of technical descriptions

            // Exchange cards
            if (card.Category == nameof(CardCategory.Exchange))
            {
                if (card.Context?.ExchangeName == "Pass on this offer")
                    return "Thank you, but I'll pass on this offer for now.";
                if (card.Context?.ExchangeReward != null)
                    return $"I'll take that deal - {card.Context.ExchangeCost} for {card.Context.ExchangeReward}.";
                return "Let's make a trade.";
            }

            // Burden cards - addressing past failures
            if (card.Category == nameof(CardCategory.Burden))
            {
                return $"{NpcName}, about last time... I know I let you down when I didn't deliver your previous message.";
            }

            // State transition cards
            if (card.Category == nameof(CardCategory.State))
            {
                if (card.SuccessState.HasValue)
                {
                    return card.SuccessState.Value switch
                    {
                        EmotionalState.DESPERATE => "This is urgent! We need to act now!",
                        EmotionalState.TENSE => "I understand this is sensitive. We should be careful how we proceed.",
                        EmotionalState.NEUTRAL => "Take a breath. We have time if we're smart about this. Let me help you think through the best approach.",
                        EmotionalState.OPEN => "I can see this matters to you. Please, tell me more about what's happening.",
                        EmotionalState.CONNECTED => "I feel like we really understand each other. Let's work through this together.",
                        _ => "Perhaps we should approach this differently."
                    };
                }
                return "Let me try a different approach...";
            }

            // Observation cards
            if (card.IsObservation)
            {
                if (card.DisplayName == "Merchant Route Knowledge")
                    return "I know a route through the merchant quarter that avoids the checkpoint entirely. We can reach Lord Blackwood faster.";
                if (card.DisplayName == "Guard Patterns")
                    return "I've been watching the guards. They change shifts at the third bell - that's our window.";
                if (card.DisplayName == "Court Gossip")
                    return "I overheard something at court that might interest you...";
                return "I noticed something earlier that might help...";
            }

            // Comfort cards based on weight/intensity
            if (card.Category == nameof(CardCategory.Comfort) && card.Type == CardType.Normal && card.BaseComfort > 0)
            {
                if (card.BaseComfort >= 2)
                    return "I completely understand how you feel. Your situation resonates deeply with me.";
                else if (card.BaseComfort == 1)
                    return "I understand how important this is, " + NpcName + ". Your future shouldn't be decided without your consent.";
                else
                    return "I hear what you're saying.";
            }

            // Token building cards (cards with specific token types)
            if (card.Type != CardType.Normal && card.Category == nameof(CardCategory.Comfort))
            {
                ConnectionType tokenType = card.GetConnectionType();
                return tokenType switch
                {
                    ConnectionType.Trust => "You can trust me with this. I'll handle it with the care it deserves.",
                    ConnectionType.Commerce => "This could be profitable for both of us. What are your terms?",
                    ConnectionType.Status => "Your reputation precedes you. It's an honor to assist someone of your standing.",
                    ConnectionType.Shadow => "Between you and me... I know how to keep a secret.",
                    _ => "I want to help you with this."
                };
            }

            // Promise/Letter cards
            if (card.DisplayName?.Contains("Letter") == true || card.DisplayName?.Contains("Accept") == true)
            {
                return "I'll take your letter to Lord Blackwood. For something this urgent, I'll do whatever it takes.";
            }

            // Default fallback - try to get from loaded dialogues or use display name
            string playerText = GetPlayerDialogueText(card.TemplateId, Session?.CurrentState);
            if (!string.IsNullOrEmpty(playerText))
                return playerText;

            return card.DisplayName ?? "Let me think about this...";
        }



        protected string GetTransitionHint()
        {
            if (Session == null) return "";

            if (Session.CurrentComfort == 3)
            {
                return Session.CurrentState switch
                {
                    EmotionalState.DESPERATE => "Tense!",
                    EmotionalState.TENSE => "Neutral!",
                    EmotionalState.NEUTRAL => "Open!",
                    EmotionalState.OPEN => "Connected!",
                    EmotionalState.CONNECTED => "Stays Connected",
                    _ => ""
                };
            }
            else if (Session.CurrentComfort == -3)
            {
                return Session.CurrentState switch
                {
                    EmotionalState.DESPERATE => "Ends!",
                    EmotionalState.TENSE => "Desperate!",
                    EmotionalState.NEUTRAL => "Tense!",
                    EmotionalState.OPEN => "Neutral!",
                    EmotionalState.CONNECTED => "Open!",
                    _ => ""
                };
            }

            return "";
        }

        protected int GetTokenCount(ConnectionType tokenType)
        {
            return CurrentTokens.GetValueOrDefault(tokenType, 0);
        }

        protected string GetTokenBonus(ConnectionType tokenType)
        {
            int count = GetTokenCount(tokenType);
            if (count > 0)
            {
                return $"(+{count * 5}%)";
            }
            return "";
        }

        protected string GetCardClass(CardInstance card)
        {
            // Map categories to CSS classes
            if (card.Category == nameof(CardCategory.Burden))
                return "crisis";
            if (card.Category == nameof(CardCategory.State))
                return "state";
            if (card.Category == nameof(CardCategory.Exchange))
                return "exchange";
            if (card.Persistence == PersistenceType.Fleeting)
                return "observation";
            return "comfort";
        }

        protected string GetCardName(CardInstance card)
        {
            // For exchange cards, use the exchange name
            if (card.Category == nameof(CardCategory.Exchange) && card.Context?.ExchangeName != null)
                return card.Context.ExchangeName;

            // Generate name from template and context
            if (card.Context?.NPCName != null)
                return $"{card.TemplateId} ({card.Context.NPCName})";
            return card.TemplateId;
        }

        protected List<string> GetCardTags(CardInstance card)
        {
            List<string> tags = new List<string>();

            // Add card type
            tags.Add(card.Type.ToString());

            // Add persistence type
            tags.Add(card.Persistence.ToString());

            // Add category
            tags.Add(card.Category.ToString());

            return tags;
        }

        protected string GetSuccessEffect(CardInstance card)
        {
            // For exchange cards, show the reward
            if (card.Category == nameof(CardCategory.Exchange) && card.Context?.ExchangeReward != null)
            {
                return $"Complete exchange: {card.Context.ExchangeReward}";
            }

            if (card.Category == nameof(CardCategory.State))
            {
                // Use the actual SuccessState property from the card
                if (card.SuccessState.HasValue)
                {
                    // Format the state name properly
                    string stateName = card.SuccessState.Value switch
                    {
                        EmotionalState.DESPERATE => "Desperate",
                        EmotionalState.TENSE => "Tense",
                        EmotionalState.NEUTRAL => "Neutral",
                        EmotionalState.OPEN => "Open",
                        EmotionalState.CONNECTED => "Connected",
                        _ => card.SuccessState.Value.ToString()
                    };
                    return $"→ {stateName}";
                }
            }

            // Show comfort gain without redundant success percentage
            return $"+{card.BaseComfort} comfort";
        }

        protected string GetFailureEffect(CardInstance card)
        {
            // For exchange cards, no failure - it's a choice
            if (card.Category == nameof(CardCategory.Exchange))
            {
                if (card.Context?.ExchangeName == "Pass on this offer")
                    return "Leave without trading";
                return "Execute trade";
            }

            if (card.Category == nameof(CardCategory.State))
            {
                // Check if card has a specific failure state
                if (card.FailureState.HasValue)
                {
                    string stateName = card.FailureState.Value switch
                    {
                        EmotionalState.DESPERATE => "Desperate",
                        EmotionalState.TENSE => "Tense",
                        EmotionalState.NEUTRAL => "Neutral",
                        EmotionalState.OPEN => "Open",
                        EmotionalState.CONNECTED => "Connected",
                        _ => card.FailureState.Value.ToString()
                    };
                    return $"→ {stateName}";
                }
                return "State unchanged";
            }

            // Failure typically gives 0 comfort
            return "+0 comfort";
        }

        protected string GetTagClass(string tag)
        {
            // Apply special classes to certain tags
            if (tag.Contains("COMFORT", StringComparison.OrdinalIgnoreCase))
                return "type-comfort";
            if (tag.Contains("STATE", StringComparison.OrdinalIgnoreCase))
                return "type-state";
            if (tag.Contains("BURDEN", StringComparison.OrdinalIgnoreCase))
                return "type-burden";
            if (tag.Contains("Persistent", StringComparison.OrdinalIgnoreCase))
                return "persistence";
            return "";
        }

        protected string GetCardText(CardInstance card)
        {
            // Load card dialogues if not loaded
            LoadCardDialogues();

            // For decline cards, show simple message
            if (card.Context?.IsDeclineCard == true)
            {
                return "Politely decline the offer and leave";
            }

            // For accept cards, we handle details in the exchange-details div
            if (card.Context?.IsAcceptCard == true)
            {
                return "Accept the merchant's offer";
            }

            // For exchange cards, show the exchange details
            if (card.Category == nameof(CardCategory.Exchange) && card.Context != null)
            {
                if (card.Context.ExchangeCost != null && card.Context.ExchangeReward != null)
                {
                    return $"{card.Context.ExchangeCost} → {card.Context.ExchangeReward}";
                }
            }

            // Try to get player dialogue from JSON based on template ID
            string playerText = GetPlayerDialogueText(card.TemplateId, Session?.CurrentState);
            if (!string.IsNullOrEmpty(playerText))
            {
                return playerText;
            }

            // Get the narrative text for the card - use template ID or display name
            return card.DisplayName ?? card.TemplateId?.Replace("_", " ") ?? "Unknown Card";
        }

        private void LoadCardDialogues()
        {
            if (_cardDialoguesLoaded) return;

            try
            {
                string contentPath = Path.Combine("Content", "Dialogues", "card_dialogues.json");
                if (File.Exists(contentPath))
                {
                    string json = File.ReadAllText(contentPath);
                    JsonSerializerOptions options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    _cardDialogues = JsonSerializer.Deserialize<CardDialogues>(json, options);
                    _cardDialoguesLoaded = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ConversationContent] Failed to load card dialogues: {ex.Message}");
                _cardDialogues = new CardDialogues { dialogues = new Dictionary<string, CardDialogue>() };
                _cardDialoguesLoaded = true;
            }
        }

        private void LoadSystemNarratives()
        {
            if (_systemNarrativesLoaded) return;

            try
            {
                string contentPath = Path.Combine("Content", "Dialogues", "system_narratives.json");
                if (File.Exists(contentPath))
                {
                    string json = File.ReadAllText(contentPath);
                    JsonSerializerOptions options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    _systemNarratives = JsonSerializer.Deserialize<SystemNarratives>(json, options);
                    _systemNarrativesLoaded = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ConversationContent] Failed to load system narratives: {ex.Message}");
                _systemNarratives = null;
                _systemNarrativesLoaded = true;
            }
        }

        private string GetPlayerDialogueText(string templateId, EmotionalState? currentState)
        {
            if (_cardDialogues?.dialogues == null || string.IsNullOrEmpty(templateId))
                return null;

            // Try direct template ID match first
            if (_cardDialogues.dialogues.TryGetValue(templateId, out CardDialogue? dialogue))
            {
                // Check for contextual dialogue based on current state
                if (currentState.HasValue && dialogue.contextual?.TryGetValue(currentState.Value.ToString(), out string? contextualText) == true)
                {
                    return contextualText;
                }

                // Return base player text
                return dialogue.playerText;
            }

            // Try common mappings for template IDs
            string mappedId = MapTemplateIdToDialogueKey(templateId);
            if (!string.IsNullOrEmpty(mappedId) && _cardDialogues.dialogues.TryGetValue(mappedId, out CardDialogue? mappedDialogue))
            {
                // Check for contextual dialogue based on current state
                if (currentState.HasValue && mappedDialogue.contextual?.TryGetValue(currentState.Value.ToString(), out string? contextualText) == true)
                {
                    return contextualText;
                }

                return mappedDialogue.playerText;
            }

            return null;
        }

        private string MapTemplateIdToDialogueKey(string templateId)
        {
            if (string.IsNullOrEmpty(templateId)) return null;

            // Map common template IDs to dialogue keys
            Dictionary<string, string> mappings = new Dictionary<string, string>
            {
                { "promise_to_help", "PromiseToHelp" },
                { "mention_guards", "MentionGuards" },
                { "calm_reassurance", "CalmReassurance" },
                { "simple_greeting", "SimpleGreeting" },
                { "active_listening", "ActiveListening" },
                { "offer_help", "OfferHelp" },
                { "share_personal", "SharePersonal" },
                { "discuss_business", "DiscussBusiness" },
                { "propose_deal", "ProposeDeal" },
                { "show_respect", "ShowRespect" },
                { "share_secret", "ShareSecret" },
                { "defuse_conflict", "DefuseConflict" },
                { "express_doubt", "ExpressDoubt" },
                { "offer_patience", "OfferPatience" },
                { "accept_letter", "AcceptLetter" },
                { "decline_letter", "DeclineLetter" },
                { "observation_share", "ObservationShare" },
                { "burden_apology", "BurdenApology" },
                { "exchange_accept", "ExchangeAccept" },
                { "exchange_decline", "ExchangeDecline" },
                { "exchange_barter", "ExchangeBarter" }
            };

            return mappings.TryGetValue(templateId.ToLower(), out string? mapped) ? mapped : null;
        }

        protected string GetSuccessChance(CardInstance card)
        {
            // Calculate success chance based on card type and state, including token bonuses
            return card.CalculateSuccessChance(CurrentTokens).ToString();
        }

        protected string GetFailureChance(CardInstance card)
        {
            // Calculate failure chance (inverse of success)
            int success = card.CalculateSuccessChance(CurrentTokens);
            return (100 - success).ToString();
        }

        private string GetInitialDialogue()
        {
            LoadSystemNarratives();
            if (_systemNarratives?.conversationNarratives?.initialDialogues != null && Session != null)
            {
                string stateKey = Session.CurrentState.ToString();
                if (_systemNarratives.conversationNarratives.initialDialogues.TryGetValue(stateKey, out string? dialogue))
                {
                    return dialogue;
                }
                return _systemNarratives.conversationNarratives.initialDialogues.GetValueOrDefault("default", "Hello, what brings you here?");
            }

            // Fallback if JSON not loaded - 5 states only
            return Session?.CurrentState switch
            {
                EmotionalState.DESPERATE => "Please, I need your help urgently!",
                EmotionalState.TENSE => "I don't have much time...",
                EmotionalState.NEUTRAL => "Hello, what brings you here?",
                EmotionalState.OPEN => "Good to see you! What can I do for you?",
                EmotionalState.CONNECTED => "My friend! How can I help?",
                _ => "Hello, what brings you here?"
            };
        }

        protected int GetExchangeSuccessRate(CardInstance card)
        {
            if (card?.Context?.ExchangeData == null) return 0;

            ExchangeData exchange = card.Context.ExchangeData;
            int baseRate = exchange.BaseSuccessRate;

            // Add Commerce token bonus (+5% per token)
            if (GameFacade != null && !string.IsNullOrEmpty(Context?.NpcId))
            {
                NPCTokenBalance tokenBalance = GameFacade.GetTokensWithNPC(Context.NpcId);
                int commerceTokens = tokenBalance.GetBalance(ConnectionType.Commerce);
                baseRate += commerceTokens * 5;
            }

            // Clamp between 5% and 95%
            return Math.Clamp(baseRate, 5, 95);
        }

        protected string GetTokenBonusText(CardInstance card)
        {
            if (card == null || CurrentTokens == null)
            {
                Console.WriteLine($"[GetTokenBonusText] Card null: {card == null}, CurrentTokens null: {CurrentTokens == null}");
                return "";
            }

            // Get the relevant token type from the card using its built-in method
            ConnectionType tokenType = card.GetConnectionType();

            // Get the token count
            int tokenCount = CurrentTokens.GetValueOrDefault(tokenType, 0);
            Console.WriteLine($"[GetTokenBonusText] Card: {card.TemplateId}, TokenType: {tokenType}, Count: {tokenCount}");

            if (tokenCount > 0)
            {
                // All cards (including goals) get +10% per token
                int bonusPerToken = 10;
                int bonus = tokenCount * bonusPerToken;
                string result = $"(+{bonus}% from {tokenCount} {tokenType})";
                Console.WriteLine($"[GetTokenBonusText] Returning: {result}");
                return result;
            }

            return "";
        }

        protected string GetExchangeCostDisplay(CardInstance card)
        {
            // Debug logging
            Console.WriteLine($"[GetExchangeCostDisplay] Card ID: {card?.TemplateId}");
            Console.WriteLine($"[GetExchangeCostDisplay] Context null: {card?.Context == null}");
            Console.WriteLine($"[GetExchangeCostDisplay] ExchangeData null: {card?.Context?.ExchangeData == null}");

            // Check for cost in Context.ExchangeData
            ExchangeData? exchangeData = card?.Context?.ExchangeData;
            if (exchangeData != null)
            {
                Console.WriteLine($"[GetExchangeCostDisplay] ExchangeData.Cost null: {exchangeData.Cost == null}");
                Console.WriteLine($"[GetExchangeCostDisplay] ExchangeData.Cost count: {exchangeData.Cost?.Count ?? 0}");
            }

            if (exchangeData?.Cost != null && exchangeData.Cost.Any())
            {
                IEnumerable<string> costParts = exchangeData.Cost.Select(c => c.GetDisplayText());
                string result = string.Join(", ", costParts);
                Console.WriteLine($"[GetExchangeCostDisplay] Returning: {result}");
                return result;
            }

            Console.WriteLine($"[GetExchangeCostDisplay] Returning default: Nothing");
            return "Nothing";
        }

        protected string GetExchangeRewardDisplay(CardInstance card)
        {
            // Check for reward in Context.ExchangeData
            ExchangeData? exchangeData = card?.Context?.ExchangeData;
            if (exchangeData?.Reward != null && exchangeData.Reward.Any())
            {
                IEnumerable<string> rewardParts = exchangeData.Reward.Select(r => r.GetDisplayText());
                return string.Join(", ", rewardParts);
            }

            return "Nothing";
        }

        protected ConnectionType GetExchangeTokenType(CardInstance card)
        {
            // For merchants, exchanges typically use Commerce tokens
            // Could be expanded based on card context or NPC type
            return ConnectionType.Commerce;
        }

        protected string GetTokenBonusPercentage(CardInstance card)
        {
            ConnectionType tokenType = GetExchangeTokenType(card);
            int tokenCount = GetTokenCount(tokenType);
            int bonusPercentage = tokenCount * 5; // 5% per token

            return bonusPercentage > 0 ? $"+{bonusPercentage}" : "+0";
        }

        protected string GetConversationEndReason()
        {
            LoadSystemNarratives();
            if (Session == null) return "Conversation ended";

            Dictionary<string, string>? exhaustedMessages = _systemNarratives?.systemMessages?.conversationExhausted;

            // Check various end conditions in priority order
            if (Session.LetterGenerated)
            {
                string msg = exhaustedMessages?.GetValueOrDefault("letterObtained", "Letter obtained! Check your queue. (Comfort: {0})") ?? "Letter obtained! Check your queue. (Comfort: {0})";
                return string.Format(msg, Session.CurrentComfort);
            }

            if (Session.CurrentPatience <= 0)
            {
                string msg = exhaustedMessages?.GetValueOrDefault("patienceExhausted", "{0}'s patience has been exhausted. They have no more time for you today.") ?? "{0}'s patience has been exhausted. They have no more time for you today.";
                return string.Format(msg, NpcName);
            }

            if (Session.CurrentState == EmotionalState.DESPERATE && Session.ComfortBattery <= -3)
            {
                string msg = exhaustedMessages?.GetValueOrDefault("conversationBroken", "{0} is too distressed to continue. The conversation has broken down.") ?? "{0} is too distressed to continue. The conversation has broken down.";
                return string.Format(msg, NpcName);
            }

            if (Context?.Type == ConversationType.Commerce)
            {
                return exhaustedMessages?.GetValueOrDefault("exchangeCompleted", "Exchange completed - conversation ended") ?? "Exchange completed - conversation ended";
            }

            if (!Session.HandCards.Any() && Session.Deck.RemainingCards == 0)
            {
                return exhaustedMessages?.GetValueOrDefault("noCardsAvailable", "No more cards available - conversation ended") ?? "No more cards available - conversation ended";
            }

            // Default reason based on comfort level
            if (Session.CurrentComfort >= 2)
            {
                string msg = exhaustedMessages?.GetValueOrDefault("endedNaturally", "Conversation ended naturally (Comfort: {0})") ?? "Conversation ended naturally (Comfort: {0})";
                return string.Format(msg, Session.CurrentComfort);
            }
            else
            {
                return exhaustedMessages?.GetValueOrDefault("default", "Conversation ended") ?? "Conversation ended";
            }
        }

        protected async Task ManuallyEndConversation()
        {
            // Player clicked "End Conversation" button
            await OnConversationEnd.InvokeAsync();
        }

        /// <summary>
        /// Process letter negotiations and add resulting obligations to the player's queue
        /// </summary>
        private void ProcessLetterNegotiations(List<LetterNegotiationResult> negotiations)
        {
            MessageSystem? messageSystem = GameFacade?.GetMessageSystem();

            foreach (LetterNegotiationResult negotiation in negotiations)
            {
                if (negotiation.CreatedObligation != null)
                {
                    // Add the obligation to the player's queue through the GameFacade
                    GameFacade.AddLetterWithObligationEffects(negotiation.CreatedObligation);

                    // For now, assume successful queue position
                    int queuePosition = 1;
                    if (queuePosition > 0)
                    {
                        LoadSystemNarratives();
                        Dictionary<string, string>? letterMessages = _systemNarratives?.systemMessages?.letterNegotiation;

                        // Generate appropriate message based on negotiation success
                        string negotiationOutcome = negotiation.NegotiationSuccess ? "Successfully negotiated" : "Failed to negotiate";
                        double deadlineHours = negotiation.FinalTerms.DeadlineMinutes / 60.0;

                        string urgencySuffix = "";
                        if (deadlineHours <= 2)
                            urgencySuffix = letterMessages?.GetValueOrDefault("criticalUrgency", " - CRITICAL!") ?? " - CRITICAL!";
                        else if (deadlineHours <= 6)
                            urgencySuffix = letterMessages?.GetValueOrDefault("urgentUrgency", " - URGENT") ?? " - URGENT";

                        string? msgTemplate = negotiation.NegotiationSuccess
                            ? letterMessages?.GetValueOrDefault("successfulNegotiation", "{0} letter: '{1}' - {2}h deadline, {3} coins")
                            : letterMessages?.GetValueOrDefault("failedNegotiation", "{0} letter: '{1}' - {2}h deadline, {3} coins");

                        if (msgTemplate == null)
                            msgTemplate = "{0} letter: '{1}' - {2}h deadline, {3} coins";

                        string message = string.Format(msgTemplate, negotiationOutcome,
                            negotiation.SourcePromiseCard.DisplayName ?? negotiation.SourcePromiseCard.TemplateId,
                            deadlineHours.ToString("F1"),
                            negotiation.FinalTerms.Payment) + urgencySuffix;

                        messageSystem?.AddSystemMessage(
                            message,
                            deadlineHours <= 2 ? SystemMessageTypes.Danger : SystemMessageTypes.Success
                        );

                        // Mark the letter as generated in the session
                        Session.LetterGenerated = true;

                        Console.WriteLine($"[ProcessLetterNegotiations] Added letter obligation '{negotiation.CreatedObligation.Id}' to queue position {queuePosition}");
                    }
                    else
                    {
                        LoadSystemNarratives();
                        string queueFullMsg = _systemNarratives?.systemMessages?.letterNegotiation?.GetValueOrDefault("queueFull", "Could not accept letter - queue may be full") ?? "Could not accept letter - queue may be full";
                        messageSystem?.AddSystemMessage(
                            queueFullMsg,
                            SystemMessageTypes.Warning
                        );
                    }
                }
                else
                {
                    Console.WriteLine($"[ProcessLetterNegotiations] WARNING: No obligation created for negotiation {negotiation.PromiseCardId}");
                }
            }
        }

        /// <summary>
        /// Get decay state CSS class for observation cards
        /// </summary>
        protected string GetObservationDecayClass(CardInstance card)
        {
            if (!card.IsObservation || card.Context?.ObservationDecayState == null)
                return "";

            if (Enum.TryParse<ObservationDecayState>(card.Context.ObservationDecayState, out ObservationDecayState decayState))
            {
                return decayState switch
                {
                    ObservationDecayState.Fresh => "observation-fresh",
                    ObservationDecayState.Expired => "observation-expired",
                    _ => ""
                };
            }
            return "";
        }

        /// <summary>
        /// Get decay state description for observation cards
        /// </summary>
        protected string GetObservationDecayDescription(CardInstance card)
        {
            if (!card.IsObservation)
                return "";

            return card.Context?.ObservationDecayDescription ?? "";
        }

        /// <summary>
        /// Check if observation card is expired (should show as unplayable)
        /// </summary>
        protected bool IsObservationExpired(CardInstance card)
        {
            return card.IsObservation &&
                   card.Context?.ObservationDecayState == nameof(ObservationDecayState.Expired);
        }

        /// <summary>
        /// Get the location context string for display in the location bar
        /// </summary>
        protected string GetLocationContext()
        {
            if (GameFacade == null) return "Unknown Location";

            Location currentLocation = GameFacade.GetCurrentLocation();
            LocationSpot currentSpot = GameFacade.GetCurrentLocationSpot();

            if (currentLocation == null || currentSpot == null)
                return "Unknown Location";

            string locationName = currentLocation.Name ?? "Unknown";
            string spotName = currentSpot.Name ?? "Unknown";
            string spotTraits = GetSpotTraits(currentSpot);

            if (!string.IsNullOrEmpty(spotTraits))
            {
                return $"{locationName} → {spotName} ({spotTraits})";
            }
            else
            {
                return $"{locationName} → {spotName}";
            }
        }

        /// <summary>
        /// Get spot properties formatted for display
        /// </summary>
        private string GetSpotTraits(LocationSpot spot)
        {
            if (spot?.SpotProperties == null || !spot.SpotProperties.Any())
                return "";

            List<string> propertyDescriptions = new List<string>();

            foreach (SpotPropertyType property in spot.SpotProperties)
            {
                // Convert property enum to user-friendly description
                string description = property switch
                {
                    SpotPropertyType.Private => "Private",
                    SpotPropertyType.Discrete => "Discrete",
                    SpotPropertyType.Public => "Public",
                    SpotPropertyType.Exposed => "Exposed",
                    SpotPropertyType.Quiet => "Quiet",
                    SpotPropertyType.Loud => "Loud",
                    SpotPropertyType.Warm => "Warm",
                    SpotPropertyType.Shaded => "Shaded",
                    SpotPropertyType.Crossroads => "Crossroads",
                    SpotPropertyType.Isolated => "Isolated",
                    SpotPropertyType.NobleFavored => "Noble-favored",
                    SpotPropertyType.CommonerHaunt => "Commoner haunt",
                    SpotPropertyType.MerchantHub => "Merchant hub",
                    SpotPropertyType.SacredGround => "Sacred ground",
                    SpotPropertyType.Commercial => "Commercial",
                    _ => property.ToString()
                };

                // Add patience bonus if this property provides it
                int patienceBonus = GetPropertyPatienceBonus(property);
                if (patienceBonus != 0)
                {
                    description += $", {(patienceBonus > 0 ? "+" : "")}{patienceBonus} patience";
                }

                propertyDescriptions.Add(description);
            }

            return string.Join(", ", propertyDescriptions);
        }

        /// <summary>
        /// Get patience bonus for a specific spot property
        /// </summary>
        private int GetPropertyPatienceBonus(SpotPropertyType property)
        {
            // Based on game design, certain properties affect conversation patience
            return property switch
            {
                SpotPropertyType.Private => 1,      // Private locations give +1 patience
                SpotPropertyType.Discrete => 1,     // Discrete locations help patience
                SpotPropertyType.Exposed => -1,     // Exposed locations reduce patience
                SpotPropertyType.Quiet => 1,        // Quiet locations help patience
                SpotPropertyType.Loud => -1,        // Loud locations hurt patience
                SpotPropertyType.Isolated => 1,     // Isolated spots help
                _ => 0
            };
        }

        // New methods for atmosphere and weight pool display
        protected string GetCurrentAtmosphereDisplay()
        {
            if (Session == null) return "Neutral";

            return Session.CurrentAtmosphere switch
            {
                ConversationAtmosphere.Neutral => "Neutral",
                ConversationAtmosphere.Prepared => "Prepared (+1 weight)",
                ConversationAtmosphere.Receptive => "Receptive (+1 card on LISTEN)",
                ConversationAtmosphere.Focused => "Focused (+20% success)",
                ConversationAtmosphere.Patient => "Patient (0 patience cost)",
                ConversationAtmosphere.Volatile => "Volatile (±1 comfort changes)",
                ConversationAtmosphere.Final => "Final (failure ends conversation)",
                ConversationAtmosphere.Informed => "Informed (next card auto-succeeds)",
                ConversationAtmosphere.Exposed => "Exposed (double comfort changes)",
                ConversationAtmosphere.Synchronized => "Synchronized (effects happen twice)",
                ConversationAtmosphere.Pressured => "Pressured (-1 card on LISTEN)",
                _ => Session.CurrentAtmosphere.ToString()
            };
        }

        protected string GetWeightPoolDisplay()
        {
            if (Session == null) return "0/5";
            return $"{Session.CurrentWeightPool}/{Session.GetEffectiveWeightCapacity()}";
        }

        protected string GetComfortBatteryDisplay()
        {
            if (Session == null) return "0";
            return Session.ComfortBattery.ToString("+0;-#;0");
        }

        protected string GetAtmosphereEffectDescription()
        {
            if (Session == null) return "";

            return Session.CurrentAtmosphere switch
            {
                ConversationAtmosphere.Prepared => "Weight capacity increased by 1",
                ConversationAtmosphere.Receptive => "Draw 1 extra card on LISTEN",
                ConversationAtmosphere.Focused => "All cards get +20% success chance",
                ConversationAtmosphere.Patient => "Conversation actions cost no patience",
                ConversationAtmosphere.Volatile => "Comfort changes are amplified by ±1",
                ConversationAtmosphere.Final => "Any failure will end the conversation",
                ConversationAtmosphere.Informed => "Your next card will automatically succeed",
                ConversationAtmosphere.Exposed => "All comfort changes are doubled",
                ConversationAtmosphere.Synchronized => "Card effects will happen twice",
                ConversationAtmosphere.Pressured => "Draw 1 fewer card on LISTEN",
                _ => ""
            };
        }
    }
}