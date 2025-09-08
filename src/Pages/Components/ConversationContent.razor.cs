using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Wayfarer.Pages.Components
{

    /// <summary>
    /// Conversation screen component that handles NPC interactions through card-based dialogue.
    /// 
    /// CRITICAL: BLAZOR SERVERPRERENDERED CONSEQUENCES
    /// ================================================
    /// This component renders TWICE due to ServerPrerendered mode:
    /// 1. During server-side prerendering (static HTML generation)
    /// 2. After establishing interactive SignalR connection
    /// 
    /// ARCHITECTURAL PRINCIPLES:
    /// - OnParametersSetAsync() runs TWICE - InitializeFromContext checks Context change
    /// - ConversationContext passed as Parameter from parent (created after interactive)
    /// - Session state maintained in component (recreated each render is OK)
    /// - Card selections and actions only happen after interactive connection
    /// 
    /// IMPLEMENTATION REQUIREMENTS:
    /// - InitializeFromContext() only runs when Context.NpcId changes (safe guard)
    /// - Conversation state is ephemeral (OK to recreate on each render)
    /// - All game state mutations go through GameFacade (has idempotence)
    /// - Attention managed by TimeBlockAttentionManager (singleton, persists)
    /// </summary>
    public class ConversationContentBase : ComponentBase
    {
        [Parameter] public ConversationContextBase Context { get; set; }
        [Parameter] public EventCallback OnConversationEnd { get; set; }
        [CascadingParameter] public GameScreenBase GameScreen { get; set; }

        [Inject] protected ConversationFacade ConversationFacade { get; set; }
        [Inject] protected GameFacade GameFacade { get; set; }

        protected ConversationSession Session { get; set; }
        protected CardInstance? SelectedCard { get; set; } = null;
        protected int TotalSelectedFocus => SelectedCard?.Focus ?? 0;
        protected bool IsProcessing { get; set; }
        protected bool IsConversationExhausted { get; set; } = false;
        protected string ExhaustionReason { get; set; } = "";

        // Action preview state
        protected bool ShowSpeakPreview { get; set; } = false;
        protected bool ShowListenPreview { get; set; } = false;

        // Helper managers using composition
        protected CardAnimationManager AnimationManager { get; set; } = new();
        protected CardDisplayManager DisplayManager { get; set; } = new();

        // Delegated properties
        protected Dictionary<string, CardAnimationState> CardStates => AnimationManager.CardStates;
        protected HashSet<string> NewCardIds { get; set; } = new();
        protected HashSet<string> ExhaustingCardIds => AnimationManager.ExhaustingCardIds;
        protected List<AnimatingCard> AnimatingCards => AnimationManager.AnimatingCards;

        protected int GetBaseSuccessPercentage(Difficulty difficulty)
        {
            return difficulty switch
            {
                Difficulty.VeryEasy => 85,
                Difficulty.Easy => 70,
                Difficulty.Medium => 60,
                Difficulty.Hard => 50,
                Difficulty.VeryHard => 40,
                _ => 60
            };
        }

        protected string NpcName { get; set; }
        protected string LastNarrative { get; set; }
        protected string LastDialogue { get; set; }
        // Letter generation is handled by ConversationManager based on connection state

        // Removed dead dialogue/narrative caching - JSON files don't exist

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
                LastNarrative = "The conversation begins...";
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
            SelectedCard = null;

            // Store current cards before listen for animation tracking
            List<CardInstance> previousCards = Session?.HandCards?.ToList() ?? new List<CardInstance>();

            // Mark any opening cards for exhaustion
            List<CardInstance> openingCards = previousCards.Where(c => c.Properties.Contains(CardProperty.Opening)).ToList();
            if (openingCards.Any())
            {
                MarkCardsForExhaust(openingCards);
                await Task.Delay(250); // Let exhaust animation play
            }

            try
            {
                // Add notification for listening
                MessageSystem? messageSystem = GameFacade?.GetMessageSystem();
                if (messageSystem != null)
                {
                    messageSystem.AddSystemMessage("You listen carefully...", SystemMessageTypes.Info);
                }

                ConversationFacade.ExecuteListen();

                // Generate narrative for the action
                GenerateListenNarrative();

                // Track newly drawn cards for slide-in animation
                List<CardInstance> currentCards = Session?.HandCards?.ToList() ?? new List<CardInstance>();
                TrackNewlyDrawnCards(previousCards, currentCards);

                // Notify about cards drawn
                if (messageSystem != null && Session.HandCards.Any())
                {
                    messageSystem.AddSystemMessage(string.Format("Drew {0} cards", Session.HandCards.Count), SystemMessageTypes.Success);
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
            if (IsProcessing || Session == null || SelectedCard == null) return;

            IsProcessing = true;

            try
            {
                // Add notification for speaking
                MessageSystem? messageSystem = GameFacade?.GetMessageSystem();

                // Check if this is an exchange card
                if (SelectedCard.CardType == CardType.Exchange && messageSystem != null)
                {
                    // For exchanges, show what's being traded
                    if (SelectedCard.Context?.ExchangeData?.Costs != null && SelectedCard.Context?.ExchangeData?.Rewards != null)
                    {
                        if (SelectedCard.Context.ExchangeName == "Pass on this offer")
                        {
                            messageSystem.AddSystemMessage("Declining the exchange...", SystemMessageTypes.Info);
                        }
                        else
                        {
                            messageSystem.AddSystemMessage(string.Format("Trading: {0} for {1}", FormatResourceList(SelectedCard.Context.ExchangeData.Costs), FormatResourceList(SelectedCard.Context.ExchangeData.Rewards)), SystemMessageTypes.Info);
                        }
                    }
                }
                else if (messageSystem != null)
                {
                    // ONE-CARD RULE: Always exactly one card
                    messageSystem.AddSystemMessage(string.Format("Playing {0}...", GetCardName(SelectedCard)), SystemMessageTypes.Info);
                }

                // Store the played card and its position BEFORE playing it
                CardInstance playedCard = SelectedCard;
                int cardPosition = GetCardPosition(playedCard);

                // ExecuteSpeak expects a single card - this removes it from hand
                CardPlayResult result = await ConversationFacade.ExecuteSpeakSingleCard(SelectedCard);

                // Generate and show narrative immediately based on result
                ProcessSpeakResult(result);
                StateHasChanged(); // Show the narrative text

                // Mark the played card with success/failure animation
                // The card is now removed from hand, but we'll keep it in AnimatingCards for display
                bool wasSuccessful = result?.Results?.FirstOrDefault()?.Success ?? false;
                AddAnimatingCard(playedCard, wasSuccessful, cardPosition);
                StateHasChanged(); // Show the card animation

                // Delay to let player see the result clearly
                await Task.Delay(750);

                // Check if this was a promise/goal card that succeeded
                bool isPromiseCard = playedCard.CardType == CardType.Letter || playedCard.CardType == CardType.Promise || playedCard.CardType == CardType.BurdenGoal;
                if (isPromiseCard && wasSuccessful)
                {
                    // Promise card succeeded - conversation ends in victory!
                    // The actual effect (letter delivery, obligation creation, etc.) is handled by the card's SuccessEffect
                    IsConversationExhausted = true;

                    // Get the success effect description from the card
                    string effectDescription = GetSuccessEffectDescription(playedCard);
                    ExhaustionReason = $"Success! {effectDescription}";

                    // Check if the success effect has EndConversation with specific narrative
                    if (playedCard.SuccessEffect?.Type == CardEffectType.EndConversation)
                    {
                        string endReason = playedCard.SuccessEffect.Value ?? "success";
                        LastNarrative = endReason == "success" ? "The conversation ends successfully." :
                                       $"The conversation ends. {effectDescription}";
                    }
                    else
                    {
                        LastNarrative = "Your words have the desired effect. The conversation concludes successfully.";
                    }

                    // Add success notification
                    if (messageSystem != null)
                    {
                        messageSystem.AddSystemMessage($"Conversation won! {effectDescription}", SystemMessageTypes.Success);
                    }

                    StateHasChanged();
                    return;
                }

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
                            messageSystem.AddSystemMessage(string.Format("{0} card(s) succeeded! +{1} flow", successes, result.TotalFlow), SystemMessageTypes.Success);
                        }
                        if (failures > 0)
                        {
                            messageSystem.AddSystemMessage(string.Format("{0} card(s) failed", failures), SystemMessageTypes.Warning);
                        }
                    }
                }

                // Mark impulse cards for exhaust animation (after a delay)
                List<CardInstance> impulseCards = Session?.HandCards?
                    .Where(c => c.Properties.Contains(CardProperty.Impulse) && c.InstanceId != playedCard.InstanceId)
                    .ToList() ?? new List<CardInstance>();

                if (impulseCards.Any())
                {
                    await Task.Delay(400); // Wait for play animation to start
                    MarkCardsForExhaust(impulseCards);
                }

                SelectedCard = null;

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
                // Letter generation is handled by ConversationManager based on connection state
                // Special cards that force letter generation are handled in ConversationManager.HandleSpecialCardEffectsAsync()
            }
        }

        private void GenerateListenNarrative()
        {
            LastNarrative = "You listen attentively...";
            LastDialogue = GetStateTransitionDialogue(Session.CurrentState);
        }

        private void GenerateSpeakNarrative(CardPlayResult result)
        {
            // Generate narrative based on cards played and result
            if (result.Results != null && result.Results.Any())
            {
                int successCount = result.Results.Count(r => r.Success);
                int totalCards = result.Results.Count;

                // Get roll and threshold info for more specific narrative
                SingleCardResult firstResult = result.Results.First();
                int roll = firstResult.Roll;
                int threshold = firstResult.SuccessChance;
                int margin = Math.Abs(roll - threshold);

                if (successCount == totalCards)
                {
                    // Vary success narrative based on margin
                    if (margin > 30)
                        LastNarrative = "Your words resonate perfectly!";
                    else if (margin > 15)
                        LastNarrative = "They nod in understanding.";
                    else
                        LastNarrative = "Your words find their mark.";
                }
                else if (successCount > 0)
                {
                    LastNarrative = "Some of your words find their mark...";
                }
                else
                {
                    // Vary failure narrative based on margin
                    if (margin > 30)
                        LastNarrative = "Your words fall completely flat...";
                    else if (margin > 15)
                        LastNarrative = "They seem unconvinced...";
                    else
                        LastNarrative = "Your approach doesn't quite land.";
                }

                // Add flow info more subtly
                if (result.TotalFlow > 0)
                {
                    LastNarrative += " (Flow +1)";
                }
                else if (result.TotalFlow < 0)
                {
                    LastNarrative += " (Flow -1)";
                }
            }
            else
            {
                LastNarrative = "You speak your mind...";
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

        private string GetStateTransitionDialogue(ConnectionState newState)
        {

            // Fallback if JSON not loaded - 5 states only
            return newState switch
            {
                ConnectionState.DISCONNECTED => "Please, I need your help urgently!",
                ConnectionState.GUARDED => "This is making me unflowable...",
                ConnectionState.NEUTRAL => "Alright, let's talk.",
                ConnectionState.RECEPTIVE => "I'm glad we're having this conversation.",
                ConnectionState.TRUSTING => "I feel like you really understand me.",
                _ => "Hmm..."
            };
        }

        private string GetResponseDialogue()
        {
            // Generate response based on current flow level (-3 to +3)
            if (Session.CurrentFlow >= 2)
                return "This conversation has been wonderful!";
            else if (Session.CurrentFlow >= 0)
                return "I appreciate you taking the time to talk.";
            else if (Session.CurrentFlow >= -2)
                return "I see what you mean...";
            else
                return "I'm not sure about this...";
        }

        // Letter generation removed - handled by ConversationManager.TryGenerateLetter()
        // This avoids duplicate logic and ensures letters are generated based on connection state
        private void GenerateLetter()
        {
            // This method is no longer used
            // Letter generation is handled by ConversationManager based on connection state
        }

        // Letter tier determination removed - handled by ConversationManager
        private LetterTier DetermineLetterTier(int flow)
        {
            // No longer used - ConversationManager determines letter properties based on linear scaling
            return LetterTier.Simple;
        }

        // Letter creation removed - handled by ConversationManager
        private DeliveryObligation CreateLetterFromFlow(LetterTier tier)
        {
            // For now, hardcode a recipient since we don't have access to all NPCs
            // In a real implementation, this would select from available NPCs
            string recipientId = "merchant_thomas"; // Default recipient
            string recipientName = "Thomas the Merchant";

            // Determine letter parameters based on tier
            (int deadline, int payment, StakeType stakes, EmotionalFocus focus) = GetTierParameters(tier);

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
                EmotionalFocus = focus,
                Tier = ConvertToTierLevel(tier),
                Description = GenerateLetterDescription(npc.Name, recipientName, tier),
                GenerationReason = $"Generated from conversation with {npc.Name}"
            };
        }

        private (int deadline, int payment, StakeType stakes, EmotionalFocus focus) GetTierParameters(LetterTier tier)
        {
            // EXACT specifications as requested
            return tier switch
            {
                LetterTier.Simple => (1440, 5, StakeType.REPUTATION, EmotionalFocus.LOW),      // 24h, 5 coins
                LetterTier.Important => (720, 10, StakeType.WEALTH, EmotionalFocus.MEDIUM),    // 12h, 10 coins
                LetterTier.Urgent => (360, 15, StakeType.STATUS, EmotionalFocus.HIGH),         // 6h, 15 coins
                LetterTier.Critical => (120, 20, StakeType.SAFETY, EmotionalFocus.CRITICAL),   // 2h, 20 coins
                _ => (1440, 5, StakeType.REPUTATION, EmotionalFocus.LOW)
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
            Simple,    // 5-9 flow
            Important, // 10-14 flow
            Urgent,    // 15-19 flow
            Critical   // 20+ flow
        }

        protected void ToggleCardSelection(CardInstance card)
        {
            // ONE CARD RULE: Only one card can be selected at a time for SPEAK action

            if (SelectedCard == card)
            {
                // Deselect the card
                SelectedCard = null;
            }
            else if (CanSelectCard(card))
            {
                // Select the new card (replaces any existing selection)
                SelectedCard = card;
            }
            // If card can't be selected (over focus limit), do nothing

            StateHasChanged();
        }

        protected bool CanSelectCard(CardInstance card)
        {
            if (Session == null || card == null) return false;

            // UI MUST ONLY ASK BACKEND - NO GAME LOGIC IN UI
            return ConversationFacade.CanPlayCard(card, Session);
        }

        protected bool IsCardSelected(CardInstance card)
        {
            return SelectedCard == card;
        }

        protected string GetCardTypeLabel(CardInstance card)
        {
            if (card == null) return "Card";

            // Show ALL properties as labels
            List<string> labels = new List<string>();

            foreach (CardProperty property in card.Properties)
            {
                labels.Add(property.ToString());
            }

            // If no properties, show "Standard"
            if (labels.Count == 0)
            {
                labels.Add("Standard");
            }

            return string.Join(" • ", labels);
        }

        protected bool CanSpeak()
        {
            return SelectedCard != null && TotalSelectedFocus <= GetFocusLimit();
        }

        protected async Task EndConversation()
        {
            // End the conversation properly to calculate and award tokens
            if (Session != null)
            {
                ConversationOutcome outcome = ConversationFacade.EndConversation();
                Console.WriteLine($"[ConversationContent] Conversation ended with outcome: Flow={outcome.TotalFlow}, TokensEarned={outcome.TokensEarned}");
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
                Console.WriteLine($"[ConversationContent] Conversation ended with outcome: Flow={outcome.TotalFlow}, TokensEarned={outcome.TokensEarned}");
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
                ConnectionState.DISCONNECTED => "disconnected",
                ConnectionState.GUARDED => "guarded",
                ConnectionState.NEUTRAL => "neutral",
                ConnectionState.RECEPTIVE => "receptive",
                ConnectionState.TRUSTING => "connected",
                _ => ""
            };
        }

        protected int GetFocusLimit()
        {
            if (Session == null) return 3;

            // Use actual values from ConversationRules.States
            if (ConversationRules.States.TryGetValue(Session.CurrentState, out ConversationStateRules? rules))
            {
                return rules.MaxFocus;
            }

            return 3; // Default fallback
        }

        protected string GetListenDetails()
        {
            if (Session == null) return "";

            return Session.CurrentState switch
            {
                ConnectionState.DISCONNECTED => "Draw 1 card",
                ConnectionState.GUARDED => "Draw 2 cards",
                ConnectionState.NEUTRAL => "Draw 2 cards",
                ConnectionState.RECEPTIVE => "Draw 3 cards",
                ConnectionState.TRUSTING => "Draw 3 cards",
                _ => "Draw 2 cards"
            };
        }

        protected string GetSpeakDetails()
        {
            if (SelectedCard != null)
            {
                int focus = SelectedCard.Focus;
                return $"Play {GetCardName(SelectedCard)} (Focus: {focus})";
            }
            return $"Select a card (Focus limit: {GetFocusLimit()})";
        }

        protected string GetStateEffects()
        {
            if (Session == null) return "";

            return Session.CurrentState switch
            {
                ConnectionState.DISCONNECTED => "• Draw 1 • Focus limit 3 • Ends at -3 flow",
                ConnectionState.GUARDED => "• Draw 2 • Focus limit 4",
                ConnectionState.NEUTRAL => "• Draw 2 • Focus limit 5",
                ConnectionState.RECEPTIVE => "• Draw 3 • Focus limit 5",
                ConnectionState.TRUSTING => "• Draw 3 • Focus limit 6",
                _ => ""
            };
        }


        protected string GetFlowLabel()
        {
            if (Session == null) return "None";
            return Session.CurrentFlow switch
            {
                3 => "Perfect Understanding",
                2 => "Deep Connection",
                1 => "Good Rapport",
                0 => "Neutral",
                -1 => "Uncertain",
                -2 => "Guarded",
                -3 => "Breaking Down",
                _ => "Unknown"
            };
        }

        protected int GetFlowProgress()
        {
            if (Session == null) return 50; // Center position for 0
            // Map -3 to +3 to 0% to 100%
            return (int)((Session.CurrentFlow + 3) * 100 / 6.0);
        }

        protected string GetFlowDotTooltip(int dotPosition)
        {
            if (Session == null) return "";

            int flow = Math.Clamp(Session.FlowBattery, -3, 3);

            if (dotPosition == flow)
                return "Current flow level";
            else if (dotPosition == 0)
                return "Neutral (battery reset point)";
            else if (dotPosition == 3)
                return "Positive transition threshold";
            else if (dotPosition == -3)
                return Session.CurrentState == ConnectionState.DISCONNECTED ?
                    "DANGER: Conversation ends here!" :
                    "Negative transition threshold";
            else if (dotPosition > 0)
                return $"Flow +{dotPosition}";
            else
                return $"Flow {dotPosition}";
        }

        protected string GetFlowDotClass(int dotPosition)
        {
            if (Session == null) return "";

            List<string> classes = new List<string>();

            // Clamp flow to valid range
            int flow = Math.Clamp(Session.FlowBattery, -3, 3);

            // Always add color class based on position
            if (dotPosition < 0)
                classes.Add("negative");
            else if (dotPosition > 0)
                classes.Add("positive");
            else
                classes.Add("neutral");

            // Add current class if this is the current position
            if (dotPosition == flow)
                classes.Add("current");

            // Add filled class for intermediate positions
            if ((flow > 0 && dotPosition > 0 && dotPosition < flow) ||
                (flow < 0 && dotPosition < 0 && dotPosition > flow))
                classes.Add("filled");

            return string.Join(" ", classes);
        }

        protected string GetConnectionStateDisplay()
        {
            if (Session == null) return "Unknown";

            return Session.CurrentState switch
            {
                ConnectionState.DISCONNECTED => "Disconnected",
                ConnectionState.GUARDED => "Guarded",
                ConnectionState.NEUTRAL => "Neutral",
                ConnectionState.RECEPTIVE => "Receptive",
                ConnectionState.TRUSTING => "Connected",
                _ => Session.CurrentState.ToString()
            };
        }

        protected string GetListenActionText()
        {
            if (Session == null) return "Draw cards";

            string drawText = Session.CurrentState switch
            {
                ConnectionState.DISCONNECTED => "Draw 1 card",
                ConnectionState.GUARDED => "Draw 2 cards",
                ConnectionState.NEUTRAL => "Draw 2 cards",
                ConnectionState.RECEPTIVE => "Draw 3 cards",
                ConnectionState.TRUSTING => "Draw 3 cards",
                _ => "Draw cards"
            };

            // Add atmosphere modifier info
            if (Session.CurrentAtmosphere == AtmosphereType.Receptive)
                drawText += " (+1 from Receptive)";
            else if (Session.CurrentAtmosphere == AtmosphereType.Pressured)
                drawText += " (-1 from Pressured)";

            return drawText + " & refresh focus";
        }

        protected string GetSpeakActionText()
        {
            if (SelectedCard != null)
            {
                int focus = SelectedCard.Focus;
                int remainingAfter = (Session?.GetAvailableFocus() ?? 0) - focus;
                string continueHint = remainingAfter > 0 ? $" (Can SPEAK {remainingAfter} more)" : " (Must LISTEN after)";
                return $"Play {GetProperCardName(SelectedCard)} ({focus} focus){continueHint}";
            }

            int availableFocus = Session?.GetAvailableFocus() ?? 0;
            if (availableFocus == 0)
                return "No focus remaining - must LISTEN to refresh";
            else if (availableFocus == 1)
                return "Select a card to play (1 focus remaining)";
            else
                return $"Select a card to play ({availableFocus} focus available)";
        }

        protected string GetProperCardName(CardInstance card)
        {
            // Use the card's description as its display name
            if (!string.IsNullOrEmpty(card.Description))
                return card.Description;

            // Generate meaningful card names based on properties
            if (card.CardType == CardType.Exchange && card.Context?.ExchangeName != null)
                return card.Context.ExchangeName;

            if (card.Properties.Contains(CardProperty.Burden))
                return "Address Past Failure";

            if (card.CardType == CardType.Observation)
            {
                return "Share Observation";
            }

            // Default to template ID with formatting
            return card.Id?.Replace("_", " ") ?? "Unknown Card";
        }

        protected int GetRequestRapportThreshold(CardInstance card)
        {
            // Check if rapport threshold is stored in the card context
            if (card?.Context?.RapportThreshold > 0)
            {
                return card.Context.RapportThreshold;
            }

            // Default threshold for requests is 5 rapport if not specified
            return 5;
        }

        protected int GetCurrentRapport()
        {
            return Session?.RapportManager?.CurrentRapport ?? 0;
        }

        protected string GetNpcStatusLine()
        {
            NPC? npc = Context?.Npc;
            if (npc == null) return "";

            List<string> status = new List<string>();

            // NPC has Profession as an enum - default is 0
            if ((int)npc.Profession != 0)
                status.Add(npc.Profession.ToString());

            // Get current location name
            Location? currentLocation = GameFacade?.GetCurrentLocation();
            if (!string.IsNullOrEmpty(currentLocation?.Name))
                status.Add(currentLocation.Name);

            return string.Join(" • ", status);
        }

        protected string GetDeadlineWarning()
        {
            // Check if there are any urgent letters with deadlines
            LetterQueueViewModel? letterQueue = GameFacade?.GetLetterQueue();
            if (letterQueue?.QueueSlots == null) return null;

            // Find the most urgent letter from queue slots
            LetterViewModel mostUrgent = null;
            int shortestDeadline = int.MaxValue;

            foreach (QueueSlotViewModel slot in letterQueue.QueueSlots)
            {
                if (slot.IsOccupied && slot.DeliveryObligation != null && slot.DeliveryObligation.DeadlineInHours > 0)
                {
                    if (slot.DeliveryObligation.DeadlineInHours < shortestDeadline)
                    {
                        shortestDeadline = slot.DeliveryObligation.DeadlineInHours;
                        mostUrgent = slot.DeliveryObligation;
                    }
                }
            }

            if (mostUrgent != null)
            {
                int hours = mostUrgent.DeadlineInHours;
                if (hours > 0)
                    return $"{mostUrgent.RecipientName}'s letter deadline: {hours}h";
                else
                    return $"{mostUrgent.RecipientName}'s letter deadline soon!";
            }

            return null;
        }

        private string GetStateTransitionVerb(ConnectionState targetState)
        {
            // Simple hardcoded verbs since JSON files don't exist
            return targetState switch
            {
                ConnectionState.DISCONNECTED => "stay urgent",
                ConnectionState.GUARDED => "be careful",
                ConnectionState.NEUTRAL => "calm down",
                ConnectionState.RECEPTIVE => "open up",
                ConnectionState.TRUSTING => "connect deeply",
                _ => "change topics"
            };
        }

        private string GetConnectionStateForCard(ConnectionState state)
        {
            return state switch
            {
                ConnectionState.DISCONNECTED => "disconnected",
                ConnectionState.GUARDED => "guarded",
                ConnectionState.NEUTRAL => "neutral",
                ConnectionState.RECEPTIVE => "receptive",
                ConnectionState.TRUSTING => "connected",
                _ => state.ToString().ToLower()
            };
        }

        protected string GetProperCardDialogue(CardInstance card)
        {
            // Simply use the card's Description property or name
            return !string.IsNullOrEmpty(card.Description) ? card.Description : GetProperCardName(card);
        }



        protected string GetTransitionHint()
        {
            if (Session == null) return "";

            if (Session.CurrentFlow == 3)
            {
                return Session.CurrentState switch
                {
                    ConnectionState.DISCONNECTED => "Guarded!",
                    ConnectionState.GUARDED => "Neutral!",
                    ConnectionState.NEUTRAL => "Open!",
                    ConnectionState.RECEPTIVE => "Connected!",
                    ConnectionState.TRUSTING => "Stays Connected",
                    _ => ""
                };
            }
            else if (Session.CurrentFlow == -3)
            {
                return Session.CurrentState switch
                {
                    ConnectionState.DISCONNECTED => "Ends!",
                    ConnectionState.GUARDED => "Disconnected!",
                    ConnectionState.NEUTRAL => "Guarded!",
                    ConnectionState.RECEPTIVE => "Neutral!",
                    ConnectionState.TRUSTING => "Open!",
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

        protected string GetRapportSourceDescription()
        {
            if (Session?.RapportManager == null || CurrentTokens == null)
                return "";

            int totalTokens = CurrentTokens.Values.Sum();
            int startingRapport = totalTokens * 3; // 3 rapport per token
            int currentRapport = Session.RapportManager.CurrentRapport;
            int gained = currentRapport - startingRapport;

            if (totalTokens == 0)
                return "No starting rapport";
            else if (gained == 0)
                return $"Started at {startingRapport} ({totalTokens} token{(totalTokens > 1 ? "s" : "")} × 3)";
            else if (gained > 0)
                return $"Started at {startingRapport} ({totalTokens} token{(totalTokens > 1 ? "s" : "")} × 3), gained +{gained} through play";
            else
                return $"Started at {startingRapport} ({totalTokens} token{(totalTokens > 1 ? "s" : "")} × 3), lost {Math.Abs(gained)} through play";
        }

        protected string GetCardClass(CardInstance card)
        {
            // Build class list for card styling
            List<string> classes = new List<string>();

            // Primary property-based classes
            if (card.Properties.Contains(CardProperty.Burden))
                classes.Add("crisis");
            else if (card.CardType == CardType.Exchange)
                classes.Add("exchange");
            else if (card.CardType == CardType.Observation)
                classes.Add("observation");
            else
                classes.Add("flow");

            // Add impulse indicator if applicable
            if (card.Properties.Contains(CardProperty.Impulse))
                classes.Add("impulse");

            // Request cards get special styling (Impulse + Opening)
            if (card.Properties.Contains(CardProperty.Impulse) && card.Properties.Contains(CardProperty.Opening))
                classes.Add("request");

            return string.Join(" ", classes);
        }

        protected int CountImpulseCards()
        {
            return Session?.HandCards?.Count(c => c.Properties.Contains(CardProperty.Impulse)) ?? 0;
        }

        protected bool HasRequestCards()
        {
            return Session?.HandCards?.Any(c => c.Properties.Contains(CardProperty.Impulse) && c.Properties.Contains(CardProperty.Opening)) ?? false;
        }

        protected string GetCardName(CardInstance card)
        {
            // Use the card's description as its name
            if (!string.IsNullOrEmpty(card.Description))
                return card.Description;

            // For exchange cards, use the exchange name
            if (card.CardType == CardType.Exchange && card.Context?.ExchangeName != null)
                return card.Context.ExchangeName;

            // Generate name from template and context
            if (card.Context?.NPCName != null)
                return $"{card.Id} ({card.Context.NPCName})";
            return card.Id;
        }

        protected List<string> GetCardTags(CardInstance card)
        {
            List<string> tags = new List<string>();

            // Add card properties as tags
            foreach (CardProperty property in card.Properties)
            {
                tags.Add(property.ToString());
            }

            // Add derived property tags
            if (card.Properties.Contains(CardProperty.Impulse) && card.Properties.Contains(CardProperty.Opening))
                tags.Add("Request");
            else if (!card.Properties.Contains(CardProperty.Impulse) && !card.Properties.Contains(CardProperty.Opening))
                tags.Add("Persistent");

            // Category is now represented by properties
            // Already added above via properties loop

            return tags;
        }

        protected string GetSuccessEffect(CardInstance card)
        {
            // For exchange cards, show the reward
            if (card.CardType == CardType.Exchange && card.Context?.ExchangeData?.Rewards != null)
            {
                return $"Complete exchange: {FormatResourceList(card.Context.ExchangeData.Rewards)}";
            }

            // Check if card has a success effect
            if (card.SuccessEffect != null && card.SuccessEffect.Type != CardEffectType.None)
            {
                // Display the effect based on type
                switch (card.SuccessEffect.Type)
                {
                    case CardEffectType.AddRapport:
                        int rapportValue = int.TryParse(card.SuccessEffect.Value, out int val) ? val : 0;
                        return rapportValue > 0 ? $"+{rapportValue} rapport" : $"{rapportValue} rapport";

                    case CardEffectType.ScaleRapportByFlow:
                        int flowRapport = Session?.CurrentFlow ?? 0;
                        return $"+{flowRapport} rapport (scales with flow {Session?.CurrentFlow ?? 0})";

                    case CardEffectType.ScaleRapportByPatience:
                        int patienceRapport = Session?.CurrentPatience / 3 ?? 0;
                        return $"+{patienceRapport} rapport (scales with patience {Session?.CurrentPatience ?? 0})";

                    case CardEffectType.ScaleRapportByFocus:
                        int focusRapport = Session?.GetAvailableFocus() ?? 0;
                        return $"+{focusRapport} rapport (scales with focus {Session?.GetAvailableFocus() ?? 0})";

                    case CardEffectType.SetAtmosphere:
                        return $"Set {card.SuccessEffect.Value} atmosphere";

                    case CardEffectType.DrawCards:
                        return $"Draw {card.SuccessEffect.Value} card(s)";

                    case CardEffectType.AddFocus:
                        return $"+{card.SuccessEffect.Value} focus";

                    default:
                        return "Effect";
                }
            }

            // No effect
            return "No effect";
        }

        protected string GetFailureEffect(CardInstance card)
        {
            // For exchange cards, no failure - it's a choice
            if (card.CardType == CardType.Exchange)
            {
                if (card.Context?.ExchangeName == "Pass on this offer")
                    return "Leave without trading";
                return "Execute trade";
            }

            // Check if card has a failure effect
            if (card.FailureEffect != null && card.FailureEffect.Type != CardEffectType.None)
            {
                // Display the effect based on type
                switch (card.FailureEffect.Type)
                {
                    case CardEffectType.AddRapport:
                        int rapportValue = int.TryParse(card.FailureEffect.Value, out int val) ? val : 0;
                        return rapportValue > 0 ? $"+{rapportValue} rapport" : rapportValue < 0 ? $"{rapportValue} rapport" : "No effect";

                    case CardEffectType.ScaleRapportByFlow:
                        int flowPenalty = Math.Abs(Session?.CurrentFlow ?? 0);
                        return $"-{flowPenalty} rapport (scales with flow {Session?.CurrentFlow ?? 0})";

                    case CardEffectType.ScaleRapportByPatience:
                        int patienceRapport = Session?.CurrentPatience / 3 ?? 0;
                        return $"-{patienceRapport} rapport (scales with patience {Session?.CurrentPatience ?? 0})";

                    case CardEffectType.ScaleRapportByFocus:
                        int focusPenalty = Session?.GetAvailableFocus() ?? 0;
                        return $"-{focusPenalty} rapport (scales with focus {Session?.GetAvailableFocus() ?? 0})";

                    case CardEffectType.SetAtmosphere:
                        return $"Set {card.FailureEffect.Value} atmosphere";

                    case CardEffectType.DrawCards:
                        return $"Draw {card.FailureEffect.Value} card(s)";

                    case CardEffectType.AddFocus:
                        return $"+{card.FailureEffect.Value} focus";

                    default:
                        return "Effect";
                }
            }

            // No effect
            return "No effect";
        }

        protected string GetTagClass(string tag)
        {
            // Apply special classes to certain tags
            if (tag.Contains("FLOW", StringComparison.OrdinalIgnoreCase))
                return "type-flow";
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
            if (card.CardType == CardType.Exchange && card.Context != null)
            {
                if (card.Context?.ExchangeData?.Costs != null && card.Context?.ExchangeData?.Rewards != null)
                {
                    return $"{FormatResourceList(card.Context.ExchangeData.Costs)} → {FormatResourceList(card.Context.ExchangeData.Rewards)}";
                }
            }

            // Use the card's Description property if available
            if (!string.IsNullOrEmpty(card.Description))
                return card.Description;

            // Get the narrative text for the card - use template ID
            return card.Id?.Replace("_", " ") ?? "Unknown Card";
        }




        protected string GetSuccessChance(CardInstance card)
        {
            // Calculate success chance based on card type and state, including token bonuses
            int baseChance = GetBaseSuccessPercentage(card.Difficulty);

            // Add rapport modifier (each point = 1% success modifier)
            int rapportBonus = Session?.RapportManager?.GetSuccessModifier() ?? 0;

            // Add atmosphere modifier (Focused gives +20%)
            int atmosphereBonus = ConversationFacade?.GetAtmosphereManager()?.GetSuccessPercentageBonus() ?? 0;

            // Calculate total, clamped to valid percentage range
            int totalChance = Math.Max(0, Math.Min(100, baseChance + rapportBonus + atmosphereBonus));

            // For auto-success atmosphere, always show 100%
            if (ConversationFacade?.GetAtmosphereManager()?.ShouldAutoSucceed() == true)
            {
                totalChance = 100;
            }

            return totalChance.ToString();
        }

        protected string GetFailureChance(CardInstance card)
        {
            // Calculate failure chance (inverse of success)
            int successChance = int.Parse(GetSuccessChance(card));
            return (100 - successChance).ToString();
        }

        protected string GetSuccessChanceBreakdown(CardInstance card)
        {
            // Show breakdown of success chance modifiers
            int baseChance = GetBaseSuccessPercentage(card.Difficulty);
            int rapportBonus = Session?.RapportManager?.GetSuccessModifier() ?? 0;
            int atmosphereBonus = ConversationFacade?.GetAtmosphereManager()?.GetSuccessPercentageBonus() ?? 0;

            string breakdown = $"Base: {baseChance}%";

            if (rapportBonus != 0)
            {
                string rapportSign = rapportBonus > 0 ? "+" : "";
                breakdown += $"\nRapport: {rapportSign}{rapportBonus}%";
            }

            if (atmosphereBonus != 0)
            {
                breakdown += $"\nAtmosphere: +{atmosphereBonus}%";
            }

            if (ConversationFacade?.GetAtmosphereManager()?.ShouldAutoSucceed() == true)
            {
                breakdown += "\nInformed: Auto-success!";
            }

            return breakdown;
        }

        protected int GetRapportModifier()
        {
            return Session?.RapportManager?.GetSuccessModifier() ?? 0;
        }

        protected int GetAtmosphereModifier()
        {
            return ConversationFacade?.GetAtmosphereManager()?.GetSuccessPercentageBonus() ?? 0;
        }

        private string GetInitialDialogue()
        {
            return Session?.CurrentState switch
            {
                ConnectionState.DISCONNECTED => "Please, I need your help urgently!",
                ConnectionState.GUARDED => "I don't have much time...",
                ConnectionState.NEUTRAL => "Hello, what brings you here?",
                ConnectionState.RECEPTIVE => "Good to see you! What can I do for you?",
                ConnectionState.TRUSTING => "My friend! How can I help?",
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
            Console.WriteLine($"[GetTokenBonusText] Card: {card.Id}, TokenType: {tokenType}, Count: {tokenCount}");

            if (tokenCount > 0)
            {
                // All cards (including requests) get +10% per token
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
            // Use the consistent FormatResourceList method
            if (card?.Context?.ExchangeData?.Costs != null && card.Context.ExchangeData.Costs.Any())
            {
                return FormatResourceList(card.Context.ExchangeData.Costs);
            }

            return "Free";
        }

        protected string GetExchangeRewardDisplay(CardInstance card)
        {
            // First check Context.ExchangeData
            if (card?.Context?.ExchangeData != null)
            {
                List<string> rewardParts = new List<string>();

                // Add standard resource rewards
                if (card.Context.ExchangeData.Rewards != null && card.Context.ExchangeData.Rewards.Any())
                {
                    foreach (ResourceAmount reward in card.Context.ExchangeData.Rewards)
                    {
                        string resourceName = reward.Type switch
                        {
                            ResourceType.Coins => "coins",
                            ResourceType.Health => "health",
                            ResourceType.Food => "food",
                            ResourceType.Attention => "attention",
                            _ => reward.Type.ToString().ToLower()
                        };
                        rewardParts.Add($"{reward.Amount} {resourceName}");
                    }
                }

                // TODO: Add item rewards from PlayerReceives when implemented in new architecture
                // if (card.Context.ExchangeData.PlayerReceives != null && card.Context.ExchangeData.PlayerReceives.Any())
                // {
                //     foreach (KeyValuePair<string, int> item in card.Context.ExchangeData.PlayerReceives)
                //     {
                //         if (item.Key == "items")
                //         {
                //             rewardParts.Add($"{item.Value} items");
                //         }
                //         else
                //         {
                //             rewardParts.Add(item.Value > 1 ? $"{item.Value} {item.Key}" : item.Key.Replace("_", " "));
                //         }
                //     }
                // }

                if (rewardParts.Any())
                {
                    return string.Join(", ", rewardParts);
                }
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
            if (Session == null) return "Conversation ended";

            // Check various end conditions in priority order
            if (Session.LetterGenerated)
            {
                return string.Format("Letter obtained! Check your queue. (Flow: {0})", Session.CurrentFlow);
            }

            if (Session.CurrentPatience <= 0)
            {
                return string.Format("{0}'s patience has been exhausted. They have no more time for you today.", NpcName);
            }

            if (Session.CurrentState == ConnectionState.DISCONNECTED && Session.FlowBattery <= -3)
            {
                return string.Format("{0} is too distressed to continue. The conversation has broken down.", NpcName);
            }

            if (Context?.Type == ConversationType.Commerce)
            {
                return "Exchange completed - conversation ended";
            }

            if (!Session.HandCards.Any() && Session.Deck.RemainingCards == 0)
            {
                return "No more cards available - conversation ended";
            }

            // Default reason based on flow level
            if (Session.CurrentFlow >= 2)
            {
                return string.Format("Conversation ended naturally (Flow: {0})", Session.CurrentFlow);
            }
            else
            {
                return "Conversation ended";
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
                        // Generate appropriate message based on negotiation success
                        string negotiationOutcome = negotiation.NegotiationSuccess ? "Successfully negotiated" : "Failed to negotiate";
                        double deadlineHours = negotiation.FinalTerms.DeadlineMinutes / 60.0;

                        string urgencySuffix = "";
                        if (deadlineHours <= 2)
                            urgencySuffix = " - CRITICAL!";
                        else if (deadlineHours <= 6)
                            urgencySuffix = " - URGENT";

                        string msgTemplate = "{0} letter: '{1}' - {2}h deadline, {3} coins";

                        string message = string.Format(msgTemplate, negotiationOutcome,
                            negotiation.SourcePromiseCard.Description ?? negotiation.SourcePromiseCard.Id,
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
                        messageSystem?.AddSystemMessage(
                            "Could not accept letter - queue may be full",
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
            if (card.CardType != CardType.Observation || card.Context?.ObservationDecayState == null)
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
            if (card.CardType != CardType.Observation)
                return "";

            return card.Context?.ObservationDecayDescription ?? "";
        }

        /// <summary>
        /// Check if observation card is expired (should show as unplayable)
        /// </summary>
        protected bool IsObservationExpired(CardInstance card)
        {
            return card.CardType == CardType.Observation &&
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

        /// <summary>
        /// PACKET 6: Get all properties for a card for display
        /// </summary>
        protected IEnumerable<CardProperty> GetCardProperties(CardInstance card)
        {
            // Return ALL properties directly from the card
            if (card?.Properties != null && card.Properties.Count > 0)
            {
                return card.Properties;
            }

            // If no properties defined, default to Persistent
            return new List<CardProperty> { CardProperty.Persistent };
        }

        /// <summary>
        /// PACKET 6: Get CSS class for property badge
        /// </summary>
        protected string GetPropertyClass(CardProperty property)
        {
            return property.ToString().ToLower();
        }

        /// <summary>
        /// Get CSS class for property tag (enhanced from mockup)
        /// </summary>
        protected string GetPropertyTagClass(CardProperty property)
        {
            return property switch
            {
                CardProperty.Impulse => "tag-impulse",
                CardProperty.Opening => "tag-opening",
                CardProperty.Persistent => "tag-persistent",
                CardProperty.Burden => "tag-burden",
                _ => "tag-" + property.ToString().ToLower()
            };
        }

        /// <summary>
        /// Get user-friendly difficulty label
        /// </summary>
        protected string GetDifficultyLabel(Difficulty difficulty)
        {
            return difficulty switch
            {
                Difficulty.VeryEasy => "Very Easy",
                Difficulty.Easy => "Easy",
                Difficulty.Medium => "Medium",
                Difficulty.Hard => "Hard",
                Difficulty.VeryHard => "Very Hard",
                _ => difficulty.ToString()
            };
        }

        /// <summary>
        /// Check if card has atmosphere effect
        /// </summary>
        protected bool HasAtmosphereEffect(CardInstance card)
        {
            // For now, check if card changes atmosphere based on mockup patterns
            // This would be enhanced with actual atmosphere effect data
            return card.CardType == CardType.Observation ||
                   GetProperCardName(card).Contains("Interrupt") ||
                   GetProperCardName(card).Contains("Merchant Routes");
        }

        /// <summary>
        /// Get atmosphere effect label for card
        /// </summary>
        protected string GetAtmosphereEffectLabel(CardInstance card)
        {
            string cardName = GetProperCardName(card);
            if (cardName.Contains("Interrupt"))
                return "Receptive";
            if (cardName.Contains("Merchant Routes"))
                return "Informed";
            if (card.CardType == CardType.Observation)
                return "Focused";
            return "";
        }

        /// <summary>
        /// PACKET 6: Get icon for property badge
        /// </summary>
        protected string GetPropertyIcon(CardProperty property)
        {
            return property switch
            {
                CardProperty.Impulse => "⚡",
                CardProperty.Opening => "⏰",
                CardProperty.Burden => "⛓️",
                CardProperty.Skeleton => "💀",
                CardProperty.Persistent => "✓",
                _ => ""
            };
        }

        /// <summary>
        /// PACKET 6: Get label for property badge
        /// </summary>
        protected string GetPropertyLabel(CardProperty property)
        {
            return property switch
            {
                CardProperty.Impulse => "Impulse",
                CardProperty.Opening => "Opening",
                CardProperty.Burden => "Burden",
                CardProperty.Skeleton => "Skeleton",
                CardProperty.Persistent => "Persistent",
                _ => property.ToString()
            };
        }

        /// <summary>
        /// PACKET 6: Get tooltip for property badge
        /// </summary>
        protected string GetPropertyTooltip(CardProperty property)
        {
            return property switch
            {
                CardProperty.Impulse => "Removed after SPEAK if unplayed",
                CardProperty.Opening => "Removed after LISTEN if unplayed",
                CardProperty.Burden => "Blocks a deck slot",
                CardProperty.Skeleton => "System-generated card",
                CardProperty.Persistent => "Stays until played",
                _ => ""
            };
        }

        /// <summary>
        /// PACKET 6: Get additional CSS classes based on card properties
        /// </summary>
        protected string GetCardPropertyClasses(CardInstance card)
        {
            List<string> classes = new List<string>();

            if (card?.Properties.Contains(CardProperty.Impulse) == true)
                classes.Add("has-impulse");
            if (card?.Properties.Contains(CardProperty.Impulse) == true && card.Properties.Contains(CardProperty.Opening) == true)
                classes.Add("has-opening"); // Request cards have Opening
            if (card?.Properties.Contains(CardProperty.Burden) == true)
                classes.Add("has-burden");

            return string.Join(" ", classes);
        }

        /// <summary>
        /// PACKET 6: Check if card has an exhaust effect
        /// </summary>
        protected bool HasExhaustEffect(CardInstance card)
        {
            // Only impulse and opening cards have exhaust effects
            return card?.Properties.Contains(CardProperty.Impulse) == true || card?.Properties.Contains(CardProperty.Opening) == true;
        }

        /// <summary>
        /// PACKET 6: Get enhanced success effect description
        /// </summary>
        protected string GetSuccessEffectDescription(CardInstance card)
        {
            // CardInstance doesn't have SuccessEffect yet, so fall back to old system
            return GetSuccessEffect(card);
        }

        /// <summary>
        /// PACKET 6: Get enhanced failure effect description
        /// </summary>
        protected string GetFailureEffectDescription(CardInstance card)
        {
            // CardInstance doesn't have FailureEffect yet, so fall back to old system
            string oldEffect = GetFailureEffect(card);
            return string.IsNullOrEmpty(oldEffect) || oldEffect == "+0 flow" ?
                "No effect" : oldEffect;
        }

        /// <summary>
        /// PACKET 6: Get exhaust effect description
        /// </summary>
        protected string GetExhaustEffectDescription(CardInstance card)
        {
            // Get exhaust effect from the card's actual data
            if (card?.ExhaustEffect != null)
            {
                return DescribeCardEffect(card.ExhaustEffect);
            }

            // Fallback descriptions if no exhaust effect defined
            if (card?.Properties.Contains(CardProperty.Impulse) == true)
            {
                return "Card removed";
            }
            else if (card?.Properties.Contains(CardProperty.Opening) == true)
            {
                return "Card removed";
            }

            return "";
        }

        /// <summary>
        /// PACKET 6: Describe a card effect in user-friendly terms
        /// </summary>
        private string DescribeCardEffect(CardEffect effect)
        {
            if (effect == null || effect.Type == CardEffectType.None)
                return "No effect";

            return effect.Type switch
            {
                CardEffectType.AddRapport => $"{(effect.Value?.StartsWith("-") == true ? "" : "+")}{effect.Value} rapport",
                CardEffectType.DrawCards => $"Draw {effect.Value} card{(effect.Value == "1" ? "" : "s")}",
                CardEffectType.AddFocus => $"Add {effect.Value} focus",
                CardEffectType.SetAtmosphere => $"Atmosphere: {effect.Value}",
                CardEffectType.EndConversation => GetEndConversationDescription(effect),
                CardEffectType.ScaleRapportByFlow => $"+X rapport (X = {effect.Value})",
                CardEffectType.ScaleRapportByPatience => $"+X rapport (X = {effect.Value})",
                CardEffectType.ScaleRapportByFocus => $"+X rapport (X = {effect.Value})",
                CardEffectType.RapportReset => "Reset rapport to starting value",
                CardEffectType.FocusRefresh => "Refresh focus",
                CardEffectType.FreeNextAction => "Next action costs no patience",
                _ => effect.Type.ToString()
            };
        }

        /// <summary>
        /// PACKET 6: Get description for EndConversation effects
        /// </summary>
        private string GetEndConversationDescription(CardEffect effect)
        {
            // Use the Value property for the reason
            
            if (effect?.Value != null)
            {
                return effect.Value switch
                {
                    "success" => "End conversation (success)",
                    "failure" => "End conversation (failure)",
                    "abandoned" => "End conversation (abandoned)",
                    "request_exhausted" => "Conversation fails",
                    _ => "End conversation"
                };
            }

            return "End conversation";
        }

        // New methods for atmosphere and focus display
        protected string GetCurrentAtmosphereDisplay()
        {
            if (Session == null) return "Neutral";

            return Session.CurrentAtmosphere switch
            {
                AtmosphereType.Neutral => "Neutral",
                AtmosphereType.Prepared => "Prepared (+1 focus)",
                AtmosphereType.Receptive => "Receptive (+1 card on LISTEN)",
                AtmosphereType.Focused => "Focused (+20% success)",
                AtmosphereType.Patient => "Patient (0 patience cost)",
                AtmosphereType.Volatile => "Volatile (±1 flow changes)",
                AtmosphereType.Informed => "Informed (next card auto-succeeds)",
                AtmosphereType.Exposed => "Exposed (double flow changes)",
                AtmosphereType.Synchronized => "Synchronized (effects happen twice)",
                AtmosphereType.Pressured => "Pressured (-1 card on LISTEN)",
                _ => Session.CurrentAtmosphere.ToString()
            };
        }

        protected string GetFocusDisplay()
        {
            if (Session == null) return "0/5";
            // Display available focus / max capacity (not spent focus)
            return $"{Session.GetAvailableFocus()}/{Session.GetEffectiveFocusCapacity()}";
        }

        protected string GetFlowBatteryDisplay()
        {
            if (Session == null) return "0";
            return Session.FlowBattery.ToString("+0;-#;0");
        }

        protected string GetAtmosphereEffectDescription()
        {
            if (Session == null) return "";

            return Session.CurrentAtmosphere switch
            {
                AtmosphereType.Prepared => "Focus capacity increased by 1",
                AtmosphereType.Receptive => "Draw 1 extra card on LISTEN",
                AtmosphereType.Focused => "All cards get +20% success chance",
                AtmosphereType.Patient => "LISTEN costs no patience",
                AtmosphereType.Volatile => "Flow changes are amplified by ±1",
                AtmosphereType.Informed => "Your next card will automatically succeed",
                AtmosphereType.Exposed => "All flow changes are doubled",
                AtmosphereType.Synchronized => "Card effects will happen twice",
                AtmosphereType.Pressured => "Draw 1 fewer card on LISTEN",
                _ => "(No special effects)"
            };
        }

        protected string GetAtmosphereIcon()
        {
            if (Session == null) return "";

            return Session.CurrentAtmosphere switch
            {
                AtmosphereType.Prepared => "💪",
                AtmosphereType.Receptive => "👂",
                AtmosphereType.Focused => "🎯",
                AtmosphereType.Patient => "⏳",
                AtmosphereType.Volatile => "⚡",
                AtmosphereType.Informed => "🧠",
                AtmosphereType.Exposed => "👁",
                AtmosphereType.Synchronized => "🔄",
                AtmosphereType.Pressured => "😰",
                _ => "◯"
            };
        }

        protected string GetAtmosphereClass()
        {
            if (Session == null) return "";

            return Session.CurrentAtmosphere switch
            {
                AtmosphereType.Neutral => "atmosphere-neutral",
                AtmosphereType.Prepared => "atmosphere-prepared",
                AtmosphereType.Receptive => "atmosphere-receptive",
                AtmosphereType.Focused => "atmosphere-focused",
                AtmosphereType.Patient => "atmosphere-patient",
                AtmosphereType.Volatile => "atmosphere-volatile",
                AtmosphereType.Informed => "atmosphere-informed",
                AtmosphereType.Exposed => "atmosphere-exposed",
                AtmosphereType.Synchronized => "atmosphere-synchronized",
                AtmosphereType.Pressured => "atmosphere-pressured",
                _ => ""
            };
        }

        protected bool HasTemporaryAtmosphereEffects()
        {
            // Check if AtmosphereManager has temporary effects
            // This would need to be exposed through ConversationFacade
            return false; // For now, until we expose this through the facade
        }

        protected string GetTemporaryEffectsDescription()
        {
            // Get temporary effects description from AtmosphereManager
            // This would need to be exposed through ConversationFacade
            return ""; // For now, until we expose this through the facade
        }

        // PACKET 7: Action Preview System Implementation

        /// <summary>
        /// Show SPEAK action preview on hover
        /// </summary>
        protected void ShowSpeakPreviewHandler()
        {
            ShowSpeakPreview = true;
            ShowListenPreview = false;
            StateHasChanged();
        }

        /// <summary>
        /// Show LISTEN action preview on hover
        /// </summary>
        protected void ShowListenPreviewHandler()
        {
            ShowListenPreview = true;
            ShowSpeakPreview = false;
            StateHasChanged();
        }

        /// <summary>
        /// Hide all action previews
        /// </summary>
        protected void HidePreviewHandler()
        {
            ShowSpeakPreview = false;
            ShowListenPreview = false;
            StateHasChanged();
        }

        /// <summary>
        /// Get cards that will exhaust on SPEAK action (Impulse cards)
        /// </summary>
        protected List<CardInstance> GetImpulseCards()
        {
            if (Session?.HandCards == null) return new List<CardInstance>();

            return Session.HandCards
                .Where(c => c.Properties.Contains(CardProperty.Impulse) && c != SelectedCard) // Don't include the played card
                .ToList();
        }

        /// <summary>
        /// Get NPC observation cards available for playing
        /// </summary>
        protected List<CardInstance> GetNPCObservationCards()
        {
            return Session?.NPCObservationCards ?? new List<CardInstance>();
        }

        /// <summary>
        /// Play an observation card (0 focus cost, consumed after use)
        /// </summary>
        protected async Task PlayObservationCard(CardInstance observationCard)
        {
            if (Session == null || observationCard == null) return;

            // Observation cards cost 0 focus and count as SPEAK actions
            // Set the card as selected and execute speak
            SelectedCard = observationCard;
            
            // Execute the SPEAK action
            await ExecuteSpeak();
            
            // Remove the observation card from the session (consumed permanently)
            Session.NPCObservationCards.Remove(observationCard);
        }

        /// <summary>
        /// Get cards that will exhaust on LISTEN action (Opening cards)
        /// </summary>
        protected List<CardInstance> GetOpeningCards()
        {
            if (Session?.HandCards == null) return new List<CardInstance>();

            return Session.HandCards
                .Where(c => c.Properties.Contains(CardProperty.Impulse) && c.Properties.Contains(CardProperty.Opening)) // Request cards have Opening property
                .ToList();
        }

        /// <summary>
        /// Get critical exhausts (request cards) from a list of cards
        /// </summary>
        protected List<CardInstance> GetCriticalExhausts(List<CardInstance> cards)
        {
            return cards.Where(c => c.Properties.Contains(CardProperty.Impulse) && c.Properties.Contains(CardProperty.Opening)).ToList();
        }

        /// <summary>
        /// Generate SPEAK preview content
        /// </summary>
        protected RenderFragment GetSpeakPreviewContent()
        {
            return builder =>
        {
            int sequence = 0;

            // Selected card action
            if (SelectedCard != null)
            {
                builder.OpenElement(sequence++, "div");
                builder.AddAttribute(sequence++, "class", "selected-action");
                builder.AddContent(sequence++, $"✓ Play: {GetProperCardName(SelectedCard)} (costs {SelectedCard.Focus} focus)");
                builder.CloseElement();
            }

            List<CardInstance> exhaustingCards = GetImpulseCards();
            List<CardInstance> criticalExhausts = GetCriticalExhausts(exhaustingCards);

            // Critical request warnings
            if (criticalExhausts.Any())
            {
                builder.OpenElement(sequence++, "div");
                builder.AddAttribute(sequence++, "class", "critical-warning");
                builder.AddContent(sequence++, "⚠️ REQUEST CARDS WILL EXHAUST - CONVERSATION WILL END!");

                foreach (CardInstance request in criticalExhausts)
                {
                    builder.OpenElement(sequence++, "div");
                    builder.AddContent(sequence++, $"• {GetProperCardName(request)} → CONVERSATION FAILS");
                    builder.CloseElement();
                }
                builder.CloseElement();
            }

            // Regular impulse exhausts
            List<CardInstance> regularExhausts = exhaustingCards.Except(criticalExhausts).ToList();
            if (regularExhausts.Any())
            {
                builder.OpenElement(sequence++, "div");
                builder.AddAttribute(sequence++, "class", "exhaust-list");
                builder.AddContent(sequence++, "Cards that will exhaust:");

                foreach (CardInstance? card in regularExhausts)
                {
                    builder.OpenElement(sequence++, "div");
                    builder.AddContent(sequence++, $"• {GetProperCardName(card)}");

                    string exhaustEffect = GetExhaustEffectDescription(card);
                    if (!string.IsNullOrEmpty(exhaustEffect) && exhaustEffect != "No exhaust effect")
                    {
                        builder.OpenElement(sequence++, "span");
                        builder.AddContent(sequence++, $" → {exhaustEffect}");
                        builder.CloseElement();
                    }
                    builder.CloseElement();
                }
                builder.CloseElement();
            }

            // No exhausts message
            if (!exhaustingCards.Any())
            {
                builder.OpenElement(sequence++, "div");
                builder.AddAttribute(sequence++, "class", "no-exhausts");
                builder.AddContent(sequence++, "No cards will exhaust");
                builder.CloseElement();
            }
        };
        }

        /// <summary>
        /// Generate LISTEN preview content
        /// </summary>
        protected RenderFragment GetListenPreviewContent()
        {
            return builder =>
        {
            int sequence = 0;

            // Listen effects
            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "listen-effects");

            int cardsToDraw = GetCardDrawCount();
            int maxFocus = GetMaxFocus();

            builder.AddContent(sequence++, $"• Draw {cardsToDraw} cards");
            builder.OpenElement(sequence++, "br");
            builder.CloseElement();
            builder.AddContent(sequence++, $"• Refresh focus to {maxFocus}");
            builder.CloseElement();

            List<CardInstance> exhaustingCards = GetOpeningCards();
            List<CardInstance> criticalExhausts = GetCriticalExhausts(exhaustingCards);

            // Critical request warnings
            if (criticalExhausts.Any())
            {
                builder.OpenElement(sequence++, "div");
                builder.AddAttribute(sequence++, "class", "critical-warning");
                builder.AddContent(sequence++, "⚠️ REQUEST CARDS WILL EXHAUST - CONVERSATION WILL END!");

                foreach (CardInstance request in criticalExhausts)
                {
                    builder.OpenElement(sequence++, "div");
                    builder.AddContent(sequence++, $"• {GetProperCardName(request)} → CONVERSATION FAILS");
                    builder.CloseElement();
                }
                builder.CloseElement();
            }

            // Regular opening exhausts (though currently all requests are both Impulse + Opening)
            List<CardInstance> regularExhausts = exhaustingCards.Except(criticalExhausts).ToList();
            if (regularExhausts.Any())
            {
                builder.OpenElement(sequence++, "div");
                builder.AddAttribute(sequence++, "class", "exhaust-list");
                builder.AddContent(sequence++, "Cards that will exhaust:");

                foreach (CardInstance? card in regularExhausts)
                {
                    builder.OpenElement(sequence++, "div");
                    builder.AddContent(sequence++, $"• {GetProperCardName(card)}");

                    string exhaustEffect = GetExhaustEffectDescription(card);
                    if (!string.IsNullOrEmpty(exhaustEffect) && exhaustEffect != "No exhaust effect")
                    {
                        builder.OpenElement(sequence++, "span");
                        builder.AddContent(sequence++, $" → {exhaustEffect}");
                        builder.CloseElement();
                    }
                    builder.CloseElement();
                }
                builder.CloseElement();
            }

            // No exhausts message
            if (!exhaustingCards.Any())
            {
                builder.OpenElement(sequence++, "div");
                builder.AddAttribute(sequence++, "class", "no-exhausts");
                builder.AddContent(sequence++, "No cards will exhaust");
                builder.CloseElement();
            }
        };
        }

        /// <summary>
        /// Get number of cards to draw on LISTEN
        /// </summary>
        protected int GetCardDrawCount()
        {
            if (Session == null) return 2;

            int baseDraw = Session.CurrentState switch
            {
                ConnectionState.DISCONNECTED => 1,
                ConnectionState.GUARDED => 2,
                ConnectionState.NEUTRAL => 2,
                ConnectionState.RECEPTIVE => 3,
                ConnectionState.TRUSTING => 3,
                _ => 2
            };

            // Apply atmosphere modifiers
            if (Session.CurrentAtmosphere == AtmosphereType.Receptive)
                baseDraw += 1;
            else if (Session.CurrentAtmosphere == AtmosphereType.Pressured)
                baseDraw = Math.Max(1, baseDraw - 1);

            return baseDraw;
        }

        /// <summary>
        /// Get max focus capacity
        /// </summary>
        protected int GetMaxFocus()
        {
            if (Session == null) return 5;
            return Session.GetEffectiveFocusCapacity();
        }

        /// <summary>
        /// Enhanced exhaust effect description for preview system
        /// </summary>
        protected string GetPreviewExhaustEffect(CardInstance card)
        {
            if (card?.Properties.Contains(CardProperty.Impulse) == true && card.Properties.Contains(CardProperty.Opening) == true)
            {
                return "ENDS CONVERSATION!";
            }
            else if (card?.Properties.Contains(CardProperty.Impulse) == true)
            {
                // Check if card has specific exhaust effects
                // For now, use generic effect
                return "Draw 1 card"; // Default impulse exhaust effect
            }

            return "No effect";
        }

        /// <summary>
        /// Get atmospheric scaling information for cards based on current atmosphere
        /// </summary>
        protected string GetAtmosphereScaling(CardInstance card)
        {
            if (Session?.CurrentAtmosphere == AtmosphereType.Volatile)
            {
                return "Flow effects ±1 from Volatile";
            }
            else if (Session?.CurrentAtmosphere == AtmosphereType.Focused)
            {
                return "Success rate +20% from Focused";
            }
            else if (Session?.CurrentAtmosphere == AtmosphereType.Exposed)
            {
                return "All effects doubled from Exposed";
            }

            return "";
        }

        /// <summary>
        /// Get connection state transparency info
        /// </summary>
        protected string GetConnectionStateInfo()
        {
            if (Session == null) return "";

            ConversationStateRules? stateRules = ConversationRules.States.GetValueOrDefault(Session.CurrentState);
            if (stateRules == null) return "";

            return $"Focus: {stateRules.MaxFocus}, Draw: {stateRules.CardsOnListen}";
        }

        /// <summary>
        /// Get flow threshold preview
        /// </summary>
        protected string GetFlowThresholdPreview()
        {
            if (Session == null) return "";

            if (Session.FlowBattery == 2)
            {
                string nextState = GetNextPositiveState(Session.CurrentState);
                return $"At +3: {nextState} state";
            }
            else if (Session.FlowBattery == -2)
            {
                string nextState = GetNextNegativeState(Session.CurrentState);
                if (Session.CurrentState == ConnectionState.DISCONNECTED)
                {
                    return "At -3: Conversation ends!";
                }
                return $"At -3: {nextState} state";
            }

            return "";
        }

        private string GetNextPositiveState(ConnectionState current)
        {
            return current switch
            {
                ConnectionState.DISCONNECTED => "Guarded",
                ConnectionState.GUARDED => "Neutral",
                ConnectionState.NEUTRAL => "Receptive",
                ConnectionState.RECEPTIVE => "Connected",
                ConnectionState.TRUSTING => "Connected",
                _ => "Unknown"
            };
        }

        private string GetNextNegativeState(ConnectionState current)
        {
            return current switch
            {
                ConnectionState.TRUSTING => "Receptive",
                ConnectionState.RECEPTIVE => "Neutral",
                ConnectionState.NEUTRAL => "Guarded",
                ConnectionState.GUARDED => "Disconnected",
                ConnectionState.DISCONNECTED => "Ends",
                _ => "Unknown"
            };
        }

        // === CARD ANIMATION METHODS ===

        /// <summary>
        /// Get CSS classes for a card based on its animation state
        /// </summary>
        protected string GetCardCssClasses(CardInstance card)
        {
            if (card == null) return "card";

            List<string> classes = new List<string> { "card" };
            string cardId = card.InstanceId ?? card.Id ?? "";

            // Check if this is a new card (recently drawn)
            if (NewCardIds.Contains(cardId))
            {
                classes.Add("card-new");
            }

            // Check if card has an animation state
            if (CardStates.TryGetValue(cardId, out CardAnimationState? state))
            {
                switch (state.State)
                {
                    case "played-success":
                        classes.Add("card-played-success");
                        break;
                    case "played-failure":
                        classes.Add("card-played-failure");
                        break;
                    case "exhausting":
                        classes.Add("card-exhausting");
                        break;
                }
            }

            // Check if card is being exhausted
            if (ExhaustingCardIds.Contains(cardId))
            {
                classes.Add("card-exhausting");
            }

            // Add warning for impulse cards
            if (card.Properties.Contains(CardProperty.Impulse))
            {
                classes.Add("card-impulse-warning");
            }

            // Add selected state
            if (SelectedCard?.InstanceId == cardId)
            {
                classes.Add("selected");
            }

            return string.Join(" ", classes);
        }

        /// <summary>
        /// Track newly drawn cards for slide-in animation
        /// </summary>
        protected void TrackNewlyDrawnCards(List<CardInstance> previousCards, List<CardInstance> currentCards)
        {
            // Clear old new card tracking
            NewCardIds.Clear();

            // Get IDs of previous cards
            HashSet<string> previousIds = new HashSet<string>(previousCards.Select(c => c.InstanceId ?? c.Id ?? ""));

            // Find and mark new cards
            List<CardInstance> newCards = new List<CardInstance>();
            foreach (CardInstance card in currentCards)
            {
                string cardId = card.InstanceId ?? card.Id ?? "";
                if (!previousIds.Contains(cardId))
                {
                    NewCardIds.Add(cardId);
                    newCards.Add(card);
                }
            }

            // Use animation manager to track new cards
            if (newCards.Any())
            {
                AnimationManager.MarkNewCards(newCards, NewCardIds, () => InvokeAsync(StateHasChanged));
            }
        }

        /// <summary>
        /// Get all cards to display (both regular hand cards and animating cards)
        /// </summary>
        protected List<CardDisplayInfo> GetAllDisplayCards()
        {
            return DisplayManager.GetAllDisplayCards(Session, AnimatingCards);
        }

        /// <summary>
        /// Get the position of a card in the current hand
        /// </summary>
        protected int GetCardPosition(CardInstance card)
        {
            return DisplayManager.GetCardPosition(card, Session);
        }

        /// <summary>
        /// Add a card to the animating cards list for post-play animation
        /// </summary>
        protected void AddAnimatingCard(CardInstance card, bool success, int originalPosition = -1)
        {
            AnimationManager.AddAnimatingCard(card, success, originalPosition, () => InvokeAsync(StateHasChanged));
        }

        /// <summary>
        /// Mark a card as successfully played
        /// </summary>
        protected void MarkCardAsPlayed(CardInstance card, bool success)
        {
            AnimationManager.MarkCardAsPlayed(card, success, () => InvokeAsync(StateHasChanged));
        }

        /// <summary>
        /// Mark cards for exhaust animation
        /// </summary>
        protected void MarkCardsForExhaust(List<CardInstance> cardsToExhaust)
        {
            AnimationManager.MarkCardsForExhaust(cardsToExhaust, () => InvokeAsync(StateHasChanged));
        }

        /// <summary>
        /// Update ExecuteListen to track newly drawn cards
        /// </summary>
        protected async Task ExecuteListenWithAnimations()
        {
            // Store current cards before listen
            List<CardInstance> previousCards = Session?.HandCards?.ToList() ?? new List<CardInstance>();

            // Execute the normal listen action
            await ExecuteListen();

            // Track newly drawn cards
            List<CardInstance> currentCards = Session?.HandCards?.ToList() ?? new List<CardInstance>();
            TrackNewlyDrawnCards(previousCards, currentCards);

            // Mark impulse cards that will exhaust on next SPEAK
            List<CardInstance> impulseCards = currentCards.Where(c => c.Properties.Contains(CardProperty.Impulse)).ToList();
            // These will be marked when SPEAK happens
        }

        /// <summary>
        /// Update ExecuteSpeak to show success/failure and exhaust animations
        /// </summary>
        protected async Task ExecuteSpeakWithAnimations()
        {
            if (SelectedCard == null) return;

            // Store the selected card info
            CardInstance playedCard = SelectedCard;

            // Execute the normal speak action (will be modified to track result)
            await ExecuteSpeak();

            // Determine if the play was successful (need to get this from the result)
            // For now, we'll simulate - in reality, we need to get this from ConversationFacade result
            bool wasSuccessful = new Random().Next(100) < 60; // Placeholder - get actual result

            // Mark the played card with animation
            MarkCardAsPlayed(playedCard, wasSuccessful);

            // Get impulse cards to exhaust
            List<CardInstance> impulseCards = Session?.HandCards?
                .Where(c => c.Properties.Contains(CardProperty.Impulse) && c.InstanceId != playedCard.InstanceId)
                .ToList() ?? new List<CardInstance>();

            if (impulseCards.Any())
            {
                // Wait for played card animation to partially complete
                await Task.Delay(250);
                MarkCardsForExhaust(impulseCards);
            }
        }
    
    private string FormatResourceList(List<ResourceAmount> resources)
    {
        if (resources == null || resources.Count == 0)
            return "nothing";
            
        return string.Join(", ", resources.Select(r => $"{r.Amount} {r.Type.ToString().ToLower()}"));
    }

}
}