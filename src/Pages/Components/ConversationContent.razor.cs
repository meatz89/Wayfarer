using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wayfarer.Pages.Components
{
    public class ConversationContentBase : ComponentBase
    {
        [Parameter] public ConversationContext Context { get; set; }
        [Parameter] public EventCallback OnConversationEnd { get; set; }
        [CascadingParameter] public GameScreenBase GameScreen { get; set; }

        [Inject] protected ConversationManager ConversationManager { get; set; }
        [Inject] protected GameFacade GameFacade { get; set; }

        protected ConversationSession Session { get; set; }
        protected HashSet<ConversationCard> SelectedCards { get; set; } = new();
        protected int TotalSelectedWeight => SelectedCards.Sum(c => c.GetEffectiveWeight(Session?.CurrentState ?? EmotionalState.GUARDED));
        protected bool IsProcessing { get; set; }
        
        protected string NpcName { get; set; }
        protected string LastNarrative { get; set; }
        protected string LastDialogue { get; set; }
        protected int ComfortThreshold => 10; // For letter generation (POC: 10+ comfort unlocks letters)

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
            SelectedCards.Clear();
            
            try
            {
                // Add notification for listening
                var messageSystem = GameFacade?.GetMessageSystem();
                if (messageSystem != null)
                {
                    messageSystem.AddSystemMessage("You listen carefully...", SystemMessageTypes.Info);
                }
                
                // Get TokenMechanicsManager for letter deck checking
                var tokenManager = GameFacade?.GetTokenMechanicsManager();
                
                // ExecuteListen is void, updates Session directly
                Session.ExecuteListen(tokenManager);
                
                // Generate narrative for the action
                GenerateListenNarrative();
                
                // Notify about cards drawn
                if (messageSystem != null && Session.HandCards.Any())
                {
                    messageSystem.AddSystemMessage($"Drew {Session.HandCards.Count} cards", SystemMessageTypes.Success);
                }
                
                // Refresh resources after listen action
                if (GameScreen != null)
                {
                    await GameScreen.RefreshResourceDisplay();
                }
                
                if (Session.ShouldEnd())
                {
                    // Add notification about conversation ending
                    if (messageSystem != null)
                    {
                        var reason = GetConversationEndReason();
                        messageSystem.AddSystemMessage(reason, SystemMessageTypes.Info);
                    }
                    
                    // Remove delay - conversations should end immediately
                    await OnConversationEnd.InvokeAsync();
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
                var messageSystem = GameFacade?.GetMessageSystem();
                
                // Check if this is an exchange card
                var exchangeCard = SelectedCards.FirstOrDefault(c => c.Category == CardCategory.EXCHANGE);
                if (exchangeCard != null && messageSystem != null)
                {
                    // For exchanges, show what's being traded
                    if (exchangeCard.Context?.ExchangeCost != null && exchangeCard.Context?.ExchangeReward != null)
                    {
                        if (exchangeCard.Context.ExchangeName == "Pass on this offer")
                        {
                            messageSystem.AddSystemMessage("Declining the exchange...", SystemMessageTypes.Info);
                        }
                        else
                        {
                            messageSystem.AddSystemMessage($"Trading: {exchangeCard.Context.ExchangeCost} for {exchangeCard.Context.ExchangeReward}", SystemMessageTypes.Info);
                        }
                    }
                }
                else if (messageSystem != null)
                {
                    messageSystem.AddSystemMessage($"Playing {SelectedCards.Count} card(s)...", SystemMessageTypes.Info);
                }
                
                // ExecuteSpeak expects HashSet<ConversationCard>
                var result = Session.ExecuteSpeak(SelectedCards);
                ProcessSpeakResult(result);
                
                // Add detailed notification for result
                if (messageSystem != null && result != null)
                {
                    // Check if this was an exchange and execute it
                    if (exchangeCard != null)
                    {
                        if (result.Results?.Any(r => r.Success) == true)
                        {
                            if (exchangeCard.Context?.ExchangeName == "Pass on this offer")
                            {
                                messageSystem.AddSystemMessage("Exchange declined", SystemMessageTypes.Info);
                            }
                            else
                            {
                                // Actually execute the exchange through GameFacade
                                if (exchangeCard.Context?.ExchangeData != null && Context?.NpcId != null)
                                {
                                    var exchangeSuccess = await GameFacade.ExecuteExchange(Context.NpcId, exchangeCard.Context.ExchangeData);
                                    if (exchangeSuccess)
                                    {
                                        messageSystem.AddSystemMessage($"Exchange successful! Received {exchangeCard.Context?.ExchangeReward}", SystemMessageTypes.Success);
                                    }
                                    else
                                    {
                                        messageSystem.AddSystemMessage("Exchange failed: Insufficient resources", SystemMessageTypes.Warning);
                                    }
                                }
                                else
                                {
                                    messageSystem.AddSystemMessage($"Exchange successful! Received {exchangeCard.Context?.ExchangeReward}", SystemMessageTypes.Success);
                                }
                            }
                        }
                        else if (result.Results?.Any(r => !r.Success) == true)
                        {
                            // Exchange failed - likely insufficient resources
                            messageSystem.AddSystemMessage("Exchange failed: Insufficient resources", SystemMessageTypes.Warning);
                        }
                    }
                    else if (result.Results?.Any() == true)
                    {
                        var successes = result.Results.Count(r => r.Success);
                        var failures = result.Results.Count(r => !r.Success);
                        
                        if (successes > 0)
                        {
                            messageSystem.AddSystemMessage($"{successes} card(s) succeeded! +{result.TotalComfort} comfort", SystemMessageTypes.Success);
                        }
                        if (failures > 0)
                        {
                            messageSystem.AddSystemMessage($"{failures} card(s) failed", SystemMessageTypes.Warning);
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
                    // Add notification about conversation ending
                    if (messageSystem != null)
                    {
                        var reason = GetConversationEndReason();
                        messageSystem.AddSystemMessage(reason, SystemMessageTypes.Info);
                    }
                    
                    // Remove delay - conversations should end immediately
                    await OnConversationEnd.InvokeAsync();
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
                // Legacy letter generation (keep for compatibility)
                else
                {
                    // Check if any played card can generate a letter and succeeded
                    var letterCard = result.Results?.FirstOrDefault(r => r.Card.CanDeliverLetter && r.Success);
                    if (letterCard != null)
                    {
                        // Crisis cards and special cards that generate letters immediately
                        GenerateLetter();
                    }
                    // Normal letter generation at comfort threshold
                    else if (Session.CurrentComfort >= ComfortThreshold)
                    {
                        GenerateLetter();
                    }
                }
            }
        }
        
        private void GenerateListenNarrative()
        {
            // Generate narrative for listen action based on state
            var stateRules = ConversationRules.States[Session.CurrentState];
            
            LastNarrative = Session.CurrentState switch
            {
                EmotionalState.DESPERATE => "You listen intently as their desperation becomes apparent...",
                EmotionalState.HOSTILE => "Their hostility is palpable as they speak...",
                EmotionalState.TENSE => "There's tension in their voice...",
                EmotionalState.GUARDED => "They speak carefully, choosing their words...",
                EmotionalState.OPEN => "They seem more comfortable talking now...",
                EmotionalState.EAGER => "They lean in, eager to share more...",
                EmotionalState.CONNECTED => "There's a real connection forming...",
                _ => "You listen attentively..."
            };
            
            LastDialogue = GetStateTransitionDialogue(Session.CurrentState);
        }
        
        private void GenerateSpeakNarrative(CardPlayResult result)
        {
            // Generate narrative based on cards played and result
            if (result.Results != null && result.Results.Any())
            {
                var successCount = result.Results.Count(r => r.Success);
                var totalCards = result.Results.Count;
                
                if (successCount == totalCards)
                {
                    LastNarrative = "Your words resonate perfectly...";
                }
                else if (successCount > 0)
                {
                    LastNarrative = "Some of your words find their mark...";
                }
                else
                {
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
        
        private string GetStateTransitionDialogue(EmotionalState newState)
        {
            return newState switch
            {
                EmotionalState.DESPERATE => "Please, I need your help urgently!",
                EmotionalState.HOSTILE => "I don't want to talk anymore!",
                EmotionalState.TENSE => "This is making me uncomfortable...",
                EmotionalState.GUARDED => "I suppose I can spare a moment...",
                EmotionalState.NEUTRAL => "Alright, let's talk.",
                EmotionalState.OPEN => "I'm glad we're having this conversation.",
                EmotionalState.EAGER => "Yes, yes! Tell me more!",
                EmotionalState.CONNECTED => "I feel like you really understand me.",
                _ => "Hmm..."
            };
        }
        
        private string GetResponseDialogue()
        {
            // Generate response based on current comfort level
            if (Session.CurrentComfort >= 15)
            {
                return "This conversation has been wonderful!";
            }
            else if (Session.CurrentComfort >= 10)
            {
                return "I appreciate you taking the time to talk.";
            }
            else if (Session.CurrentComfort >= 5)
            {
                return "I see what you mean...";
            }
            else
            {
                return "I'm not sure about this...";
            }
        }

        private void GenerateLetter()
        {
            // Prevent duplicate generation in same conversation
            if (Session == null || Session.LetterGenerated) return;
            
            // Determine letter tier based on comfort level
            LetterTier tier = DetermineLetterTier(Session.CurrentComfort);
            
            // Create the delivery obligation
            var obligation = CreateLetterFromComfort(tier);
            if (obligation == null)
            {
                Console.WriteLine("[ConversationContent] Failed to create letter - no valid recipients");
                return;
            }
            
            // Mark as generated to prevent duplicates
            Session.LetterGenerated = true;
            
            // Log the letter generation
            var npcName = Context?.Npc?.Name ?? "The NPC";
            Console.WriteLine($"[ConversationContent] Letter generated from {npcName}!");
            Console.WriteLine($"   → Tier: {GetTierDescription(tier)}");
            Console.WriteLine($"   → Deadline: {obligation.DeadlineInMinutes / 60}h | Payment: {obligation.Payment} coins");
            Console.WriteLine($"   → Comfort Level: {Session.CurrentComfort}");
            
            // Add the letter to the obligation queue
            var queueManager = GameFacade.GetObligationQueueManager();
            if (queueManager != null)
            {
                queueManager.AddObligation(obligation);
                Console.WriteLine($"[ConversationContent] Letter added to queue successfully!");
            }
            else
            {
                Console.WriteLine($"[ConversationContent] ERROR: Could not get ObligationQueueManager!");
            }
        }
        
        private LetterTier DetermineLetterTier(int comfort)
        {
            // Algorithm: Tier = Floor((Comfort - 5) / 5)
            // 5-9: Simple (T1)
            // 10-14: Important (T2)  
            // 15-19: Urgent (T3)
            // 20+: Critical (T4)
            
            if (comfort >= 20) return LetterTier.Critical;
            if (comfort >= 15) return LetterTier.Urgent;
            if (comfort >= 10) return LetterTier.Important;
            return LetterTier.Simple;
        }
        
        private DeliveryObligation CreateLetterFromComfort(LetterTier tier)
        {
            // For now, hardcode a recipient since we don't have access to all NPCs
            // In a real implementation, this would select from available NPCs
            var recipientId = "merchant_thomas"; // Default recipient
            var recipientName = "Thomas the Merchant";
            
            // Determine letter parameters based on tier
            var (deadline, payment, stakes, weight) = GetTierParameters(tier);
            
            // Get the NPC from context
            var npc = Context?.Npc;
            if (npc == null) return null;
            
            // Determine token type from NPC's available types
            var tokenType = DetermineTokenType(npc);
            
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
                GenerationReason = $"Generated from {Session.CurrentComfort} comfort in conversation"
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

        protected void ToggleCardSelection(ConversationCard card)
        {
            var conversationType = Context?.Type ?? ConversationType.Standard;
            if (conversationType == ConversationType.QuickExchange)
            {
                // For exchanges, selecting a card immediately plays it
                SelectedCards.Clear();
                SelectedCards.Add(card);
                _ = ExecuteSpeak();
                return;
            }
            
            if (SelectedCards.Contains(card))
            {
                SelectedCards.Remove(card);
            }
            else if (CanSelectCard(card))
            {
                SelectedCards.Add(card);
            }
            
            StateHasChanged();
        }

        protected bool CanSelectCard(ConversationCard card)
        {
            if (Session == null) return false;
            
            // Check if observation card is expired
            if (IsObservationExpired(card)) return false;
            
            // Check weight limit - use effective weight which accounts for free categories in certain states
            var effectiveWeight = card.GetEffectiveWeight(Session.CurrentState);
            var newWeight = TotalSelectedWeight + effectiveWeight;
            return newWeight <= GetWeightLimit();
        }

        protected bool IsCardSelected(ConversationCard card)
        {
            return SelectedCards.Contains(card);
        }

        protected bool CanSpeak()
        {
            return SelectedCards.Any() && TotalSelectedWeight <= GetWeightLimit();
        }

        protected async Task EndConversation()
        {
            // EndConversation doesn't exist on ConversationManager, just clear the session
            Session = null;
            await OnConversationEnd.InvokeAsync();
        }
        
        protected async Task ExitConversation()
        {
            // Allow player to manually exit conversation
            Console.WriteLine("[ConversationContent] Player manually exiting conversation");
            Session = null;
            await OnConversationEnd.InvokeAsync();
        }

        // UI Helper Methods
        protected string GetConversationModeTitle()
        {
            var conversationType = Context?.Type ?? ConversationType.Standard;
            return conversationType switch
            {
                ConversationType.QuickExchange => "Quick Exchange",
                ConversationType.Crisis => "Crisis Resolution",
                ConversationType.Standard => "Standard Conversation",
                ConversationType.Deep => "Deep Conversation",
                _ => "Conversation"
            };
        }

        protected string GetStateClass()
        {
            return Session?.CurrentState switch
            {
                EmotionalState.DESPERATE => "desperate",
                EmotionalState.HOSTILE => "hostile",
                EmotionalState.TENSE => "tense",
                _ => ""
            };
        }

        protected int GetWeightLimit()
        {
            if (Session == null) return 3;
            
            return Session.CurrentState switch
            {
                EmotionalState.GUARDED => 2,
                EmotionalState.TENSE => 1,
                EmotionalState.OVERWHELMED => 1,
                EmotionalState.CONNECTED => 4,
                EmotionalState.HOSTILE => 0,
                _ => 3
            };
        }

        protected string GetListenDetails()
        {
            if (Session == null) return "";
            
            return Session.CurrentState switch
            {
                EmotionalState.DESPERATE => "Draw 2 + Crisis • State → Hostile",
                EmotionalState.HOSTILE => "Draw 1 + 2 Crisis • Ends conversation",
                EmotionalState.GUARDED => "Draw 1 • State → Neutral",
                EmotionalState.OPEN => "Draw 3 • State unchanged",
                EmotionalState.EAGER => "Draw 3 • State unchanged",
                _ => "Draw 2 • State unchanged"
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
                EmotionalState.DESPERATE => "• Draw 2 + crisis • Crisis free • Listen worsens",
                EmotionalState.HOSTILE => "• Only crisis cards playable",
                EmotionalState.CONNECTED => "• Weight limit 4 • All comfort +2",
                EmotionalState.EAGER => "• 2+ same type → +3 comfort",
                _ => ""
            };
        }


        protected string GetComfortLabel()
        {
            if (Session == null) return "None";
            return Session.CurrentComfort switch
            {
                >= 20 => "Perfect Understanding",
                >= 15 => "Deep Connection",
                >= 10 => "Good Rapport",
                >= 5 => "Basic Trust",
                _ => "Tentative"
            };
        }

        protected int GetComfortProgress()
        {
            if (Session == null) return 0;
            return Math.Min(100, (Session.CurrentComfort * 100) / 20); // Scale to 20 max comfort for full bar
        }

        protected string GetCardClass(ConversationCard card)
        {
            // Map categories to CSS classes
            if (card.Category == CardCategory.CRISIS)
                return "crisis";
            if (card.Category == CardCategory.STATE)
                return "state";
            if (card.Category == CardCategory.EXCHANGE)
                return "exchange";
            if (card.Persistence == PersistenceType.OneShot)
                return "observation";
            return "comfort";
        }

        protected string GetCardName(ConversationCard card)
        {
            // For exchange cards, use the exchange name
            if (card.Category == CardCategory.EXCHANGE && card.Context?.ExchangeName != null)
                return card.Context.ExchangeName;
                
            // Generate name from template and context
            if (card.Context?.NPCName != null)
                return $"{card.Template} ({card.Context.NPCName})";
            return card.Template.ToString();
        }

        protected List<string> GetCardTags(ConversationCard card)
        {
            var tags = new List<string>();
            
            // Add card type
            tags.Add(card.Type.ToString());
            
            // Add persistence type
            tags.Add(card.Persistence.ToString());
            
            // Add category
            tags.Add(card.Category.ToString());
            
            return tags;
        }

        protected string GetSuccessEffect(ConversationCard card)
        {
            // For exchange cards, show the reward
            if (card.Category == CardCategory.EXCHANGE && card.Context?.ExchangeReward != null)
            {
                return $"Complete exchange: {card.Context.ExchangeReward}";
            }
            
            if (card.Category == CardCategory.STATE)
                return "Change state";
            
            // Calculate comfort from success
            var successChance = card.CalculateSuccessChance();
            return $"Success ({successChance}%): +{card.BaseComfort} comfort";
        }

        protected string GetFailureEffect(ConversationCard card)
        {
            // For exchange cards, no failure - it's a choice
            if (card.Category == CardCategory.EXCHANGE)
            {
                if (card.Context?.ExchangeName == "Pass on this offer")
                    return "Leave without trading";
                return "Execute trade";
            }
            
            if (card.Category == CardCategory.STATE)
                return "No change";
            
            // Failure typically gives 0 comfort
            return "Fail: +0 comfort";
        }
        
        protected string GetTagClass(string tag)
        {
            // Apply special classes to certain tags
            if (tag.Contains("COMFORT", StringComparison.OrdinalIgnoreCase))
                return "type-comfort";
            if (tag.Contains("STATE", StringComparison.OrdinalIgnoreCase))
                return "type-state";
            if (tag.Contains("CRISIS", StringComparison.OrdinalIgnoreCase))
                return "type-crisis";
            if (tag.Contains("BURDEN", StringComparison.OrdinalIgnoreCase))
                return "type-burden";
            if (tag.Contains("OneShot", StringComparison.OrdinalIgnoreCase))
                return "observation";
            if (tag.Contains("Persistent", StringComparison.OrdinalIgnoreCase))
                return "persistence";
            return "";
        }
        
        protected string GetCardText(ConversationCard card)
        {
            // For exchange cards, show the exchange details
            if (card.Category == CardCategory.EXCHANGE && card.Context != null)
            {
                if (card.Context.ExchangeCost != null && card.Context.ExchangeReward != null)
                {
                    return $"{card.Context.ExchangeCost} → {card.Context.ExchangeReward}";
                }
            }
            
            // Get the narrative text for the card - use template as the text
            return card.Template.ToString().Replace("_", " ");
        }
        
        protected string GetSuccessChance(ConversationCard card)
        {
            // Calculate success chance based on card type and state
            return card.CalculateSuccessChance().ToString();
        }
        
        protected string GetFailureChance(ConversationCard card)
        {
            // Calculate failure chance (inverse of success)
            var success = card.CalculateSuccessChance();
            return (100 - success).ToString();
        }

        private string GetInitialDialogue()
        {
            return Session?.CurrentState switch
            {
                EmotionalState.DESPERATE => "Please, I need your help urgently!",
                EmotionalState.HOSTILE => "What do you want?!",
                EmotionalState.TENSE => "I don't have much time...",
                _ => "Hello, what brings you here?"
            };
        }
        
        protected string GetConversationEndReason()
        {
            if (Session == null) return "Conversation ended";
            
            // Check various end conditions in priority order
            if (Session.LetterGenerated)
                return $"Letter obtained! Check your queue. (Comfort: {Session.CurrentComfort})";
                
            if (Session.CurrentPatience <= 0)
                return "NPC's patience exhausted - conversation ended";
                
            if (Session.CurrentState == EmotionalState.HOSTILE)
                return $"{NpcName} became hostile and ended the conversation";
                
            if (Context?.Type == ConversationType.QuickExchange)
                return "Exchange completed - conversation ended";
                
            if (!Session.HandCards.Any() && Session.Deck.RemainingCards == 0)
                return "No more cards available - conversation ended";
                
            // Default reason based on comfort level
            if (Session.CurrentComfort >= 10)
                return $"Conversation ended naturally (Comfort reached: {Session.CurrentComfort})";
            else
                return "Conversation ended";
        }
        
        /// <summary>
        /// Process letter negotiations and add resulting obligations to the player's queue
        /// </summary>
        private void ProcessLetterNegotiations(List<LetterNegotiationResult> negotiations)
        {
            var messageSystem = GameFacade?.GetMessageSystem();
            
            foreach (var negotiation in negotiations)
            {
                if (negotiation.CreatedObligation != null)
                {
                    // Add the obligation to the player's queue through the GameFacade
                    var queuePosition = GameFacade.AddLetterWithObligationEffects(negotiation.CreatedObligation);
                    
                    if (queuePosition > 0)
                    {
                        // Generate appropriate message based on negotiation success
                        var negotiationOutcome = negotiation.NegotiationSuccess ? "Successfully negotiated" : "Failed to negotiate";
                        var urgency = negotiation.FinalTerms.DeadlineHours <= 2 ? " - CRITICAL!" : 
                                     negotiation.FinalTerms.DeadlineHours <= 6 ? " - URGENT" : "";
                        
                        messageSystem?.AddSystemMessage(
                            $"{negotiationOutcome} letter: '{negotiation.SourceLetterCard.Title}' - {negotiation.FinalTerms.DeadlineHours}h deadline, {negotiation.FinalTerms.Payment} coins{urgency}",
                            negotiation.FinalTerms.DeadlineHours <= 2 ? SystemMessageTypes.Danger : SystemMessageTypes.Success
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
                    Console.WriteLine($"[ProcessLetterNegotiations] WARNING: No obligation created for negotiation {negotiation.LetterCardId}");
                }
            }
        }
        
        /// <summary>
        /// Get decay state CSS class for observation cards
        /// </summary>
        protected string GetObservationDecayClass(ConversationCard card)
        {
            if (!card.IsObservation || card.Context?.ObservationDecayState == null)
                return "";
                
            return card.Context.ObservationDecayState switch
            {
                ObservationDecayState.Fresh => "observation-fresh",
                ObservationDecayState.Stale => "observation-stale",
                ObservationDecayState.Expired => "observation-expired",
                _ => ""
            };
        }
        
        /// <summary>
        /// Get decay state description for observation cards
        /// </summary>
        protected string GetObservationDecayDescription(ConversationCard card)
        {
            if (!card.IsObservation)
                return "";
                
            return card.Context?.ObservationDecayDescription ?? "";
        }
        
        /// <summary>
        /// Check if observation card is expired (should show as unplayable)
        /// </summary>
        protected bool IsObservationExpired(ConversationCard card)
        {
            return card.IsObservation && 
                   card.Context?.ObservationDecayState == ObservationDecayState.Expired;
        }
    }
}