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
        protected bool IsConversationExhausted { get; set; } = false;
        protected string ExhaustionReason { get; set; } = "";
        
        protected string NpcName { get; set; }
        protected string LastNarrative { get; set; }
        protected string LastDialogue { get; set; }
        // Letter generation is handled by ConversationManager based on emotional state
        
        // Get current token balances with this NPC
        protected Dictionary<ConnectionType, int> CurrentTokens
        {
            get
            {
                if (Context?.NpcId != null && GameFacade != null)
                {
                    var tokenBalance = GameFacade.GetTokensWithNPC(Context.NpcId);
                    var result = new Dictionary<ConnectionType, int>();
                    foreach (var balance in tokenBalance.Balances)
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
            SelectedCards.Clear();
            
            try
            {
                // Add notification for listening
                var messageSystem = GameFacade?.GetMessageSystem();
                if (messageSystem != null)
                {
                    messageSystem.AddSystemMessage("You listen carefully...", SystemMessageTypes.Info);
                }
                
                // CRITICAL: Must use ConversationManager.ExecuteListen to properly handle letter delivery cards
                // The ConversationManager will pass the correct managers to the session
                ConversationManager.ExecuteListen();
                
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
                    // ONE-CARD RULE: Always exactly one card
                    var selectedCard = SelectedCards.FirstOrDefault();
                    if (selectedCard != null)
                    {
                        messageSystem.AddSystemMessage($"Playing {GetCardName(selectedCard)}...", SystemMessageTypes.Info);
                    }
                }
                
                // ExecuteSpeak expects HashSet<ConversationCard>
                // CRITICAL: Must use ConversationManager.ExecuteSpeak to handle special card effects like letter delivery
                var result = await ConversationManager.ExecuteSpeak(SelectedCards);
                ProcessSpeakResult(result);
                
                // Add detailed notification for result
                // NOTE: Exchange handling is done in ConversationManager.HandleSpecialCardEffectsAsync
                // Letter delivery is also handled there - no need to duplicate logic here
                if (messageSystem != null && result != null)
                {
                    if (result.Results?.Any() == true)
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
            // Remove auto-execute for exchanges - treat them like normal cards
            // Player must select exchange card then click SPEAK to confirm
            
            if (SelectedCards.Contains(card))
            {
                SelectedCards.Remove(card);
            }
            else if (CanSelectCard(card))
            {
                // For exchanges, enforce ONE card selection (matches SPEAK plays ONE card design)
                var conversationType = Context?.Type ?? ConversationType.Standard;
                if (conversationType == ConversationType.QuickExchange)
                {
                    SelectedCards.Clear(); // Only one exchange can be selected at a time
                }
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
            // End the conversation properly to calculate and award tokens
            if (Session != null && ConversationManager != null)
            {
                var outcome = ConversationManager.EndConversation();
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
            if (Session != null && ConversationManager != null)
            {
                var outcome = ConversationManager.EndConversation();
                Console.WriteLine($"[ConversationContent] Conversation ended with outcome: Comfort={outcome.TotalComfort}, TokensEarned={outcome.TokensEarned}");
            }
            
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
                EmotionalState.HOSTILE => 3,
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
                EmotionalState.HOSTILE => "• Only Crisis letters playable",
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
            if (card.Category == CardCategory.BURDEN)
                return "crisis";
            if (card.Category == CardCategory.STATE)
                return "state";
            if (card.Category == CardCategory.EXCHANGE)
                return "exchange";
            if (card.Persistence == PersistenceType.Fleeting)
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
            {
                // Use the actual SuccessState property from the card
                if (card.SuccessState.HasValue)
                {
                    // Format the state name properly
                    string stateName = card.SuccessState.Value switch
                    {
                        EmotionalState.NEUTRAL => "Neutral",
                        EmotionalState.OPEN => "Open",
                        EmotionalState.GUARDED => "Guarded",
                        EmotionalState.TENSE => "Tense",
                        EmotionalState.EAGER => "Eager",
                        EmotionalState.OVERWHELMED => "Overwhelmed",
                        EmotionalState.CONNECTED => "Connected",
                        EmotionalState.DESPERATE => "Desperate",
                        EmotionalState.HOSTILE => "Hostile",
                        _ => card.SuccessState.Value.ToString()
                    };
                    return $"→ {stateName}";
                }
                else
                {
                    // Fallback to template name matching if SuccessState is not set
                    if (card.Template.ToString().Contains("Warm") || card.Template.ToString().Contains("Open"))
                        return "→ Open";
                    else if (card.Template.ToString().Contains("Tense"))
                        return "→ Tense";
                    else if (card.Template.ToString().Contains("Overwhelmed"))
                        return "→ Overwhelmed";
                    else if (card.Template.ToString().Contains("Eager"))
                        return "→ Eager";
                    else
                        return "Change state";
                }
            }
            
            // Show comfort gain without redundant success percentage
            return $"+{card.BaseComfort} comfort";
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
            {
                // Check if card has a specific failure state
                if (card.FailureState.HasValue)
                {
                    string stateName = card.FailureState.Value switch
                    {
                        EmotionalState.NEUTRAL => "Neutral",
                        EmotionalState.OPEN => "Open",
                        EmotionalState.GUARDED => "Guarded",
                        EmotionalState.TENSE => "Tense",
                        EmotionalState.EAGER => "Eager",
                        EmotionalState.OVERWHELMED => "Overwhelmed",
                        EmotionalState.CONNECTED => "Connected",
                        EmotionalState.DESPERATE => "Desperate",
                        EmotionalState.HOSTILE => "Hostile",
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
            // Calculate success chance based on card type and state, including token bonuses
            return card.CalculateSuccessChance(CurrentTokens).ToString();
        }
        
        protected string GetFailureChance(ConversationCard card)
        {
            // Calculate failure chance (inverse of success)
            var success = card.CalculateSuccessChance(CurrentTokens);
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
        
        protected int GetExchangeSuccessRate(ConversationCard card)
        {
            if (card?.Context?.ExchangeData == null) return 0;
            
            var exchange = card.Context.ExchangeData;
            var baseRate = exchange.BaseSuccessRate;
            
            // Add Commerce token bonus (+5% per token)
            if (GameFacade != null && !string.IsNullOrEmpty(Context?.NpcId))
            {
                var tokenBalance = GameFacade.GetTokensWithNPC(Context.NpcId);
                var commerceTokens = tokenBalance.GetBalance(ConnectionType.Commerce);
                baseRate += commerceTokens * 5;
            }
            
            // Clamp between 5% and 95%
            return Math.Clamp(baseRate, 5, 95);
        }
        
        protected string GetTokenBonusText(ConversationCard card)
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
            Console.WriteLine($"[GetTokenBonusText] Card: {card.Template}, TokenType: {tokenType}, Count: {tokenCount}");
            
            if (tokenCount > 0)
            {
                // Crisis cards and Promise cards (including goals) get +10% per token, others get +5%
                int bonusPerToken = (card.Category == CardCategory.CRISIS || card.Category == CardCategory.PROMISE) ? 10 : 5;
                int bonus = tokenCount * bonusPerToken;
                var result = $"(+{bonus}% from {tokenCount} {tokenType})";
                Console.WriteLine($"[GetTokenBonusText] Returning: {result}");
                return result;
            }
            
            return "";
        }
        
        protected string GetConversationEndReason()
        {
            if (Session == null) return "Conversation ended";
            
            // Check various end conditions in priority order
            if (Session.LetterGenerated)
                return $"Letter obtained! Check your queue. (Comfort: {Session.CurrentComfort})";
                
            if (Session.CurrentPatience <= 0)
                return $"{NpcName}'s patience has been exhausted. They have no more time for you today.";
                
            if (Session.CurrentState == EmotionalState.HOSTILE)
                return $"{NpcName} has become hostile and refuses to continue speaking with you.";
                
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
                            $"{negotiationOutcome} letter: '{negotiation.SourcePromiseCard.Title}' - {negotiation.FinalTerms.DeadlineHours}h deadline, {negotiation.FinalTerms.Payment} coins{urgency}",
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
                    Console.WriteLine($"[ProcessLetterNegotiations] WARNING: No obligation created for negotiation {negotiation.PromiseCardId}");
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