using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Wayfarer.GameState.Enums;

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
        [Inject] protected ConversationNarrativeService NarrativeService { get; set; }

        protected ConversationSession Session { get; set; }
        protected CardInstance? SelectedCard { get; set; } = null;
        protected int TotalSelectedFocus => SelectedCard?.Focus ?? 0;
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
        // Track which request cards have already been moved from RequestPile to ActiveCards
        protected HashSet<string> MovedRequestCardIds { get; set; } = new();

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
        protected NarrativeProviderType LastProviderSource { get; set; } = NarrativeProviderType.JsonFallback; // Default to fallback
        
        // AI narrative generation state
        protected bool IsGeneratingNarrative { get; set; } = false;
        protected List<CardNarrative> CurrentCardNarratives { get; set; } = new List<CardNarrative>();
        protected NarrativeOutput CurrentNarrativeOutput { get; set; }
        private Task<NarrativeOutput> _initialNarrativeTask = null;
        
        protected string GetNarrativeClass()
        {
            if (IsGeneratingNarrative)
                return "json-fallback narrative-loading";
            return LastProviderSource == NarrativeProviderType.AIGenerated ? "ai-generated" : "json-fallback";
        }

        protected string GetCardNarrativeClass(CardInstance card)
        {
            // Check if this specific card has AI-generated narrative
            if (card != null && CurrentCardNarratives != null)
            {
                CardNarrative cardNarrative = CurrentCardNarratives.FirstOrDefault(cn => cn.CardId == card.Id);
                if (cardNarrative != null && !string.IsNullOrWhiteSpace(cardNarrative.NarrativeText))
                {
                    return cardNarrative.ProviderSource == NarrativeProviderType.AIGenerated ? "ai-generated" : "json-fallback";
                }
            }
            
            // Default to template/fallback
            return "json-fallback";
        }
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

                // Generate initial narrative using AI if available
                await GenerateInitialNarrative();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ConversationContent] Failed to initialize from context: {ex.Message}");
                await OnConversationEnd.InvokeAsync();
            }
        }


        protected async Task ExecuteListen()
        {
            if (Session == null) return;
            
            // If initial narrative is still generating, wait for it instead of triggering new generation
            if (_initialNarrativeTask != null && !_initialNarrativeTask.IsCompleted)
            {
                Console.WriteLine("[ConversationContent.ExecuteListen] Waiting for initial narrative to complete...");
                NarrativeOutput initialNarrative = await _initialNarrativeTask;
                
                if (initialNarrative != null)
                {
                    Console.WriteLine($"[ConversationContent.ExecuteListen] Using initial narrative. Card narratives: {initialNarrative.CardNarratives?.Count ?? 0}");
                    // Apply the narratives that were generated initially
                    ApplyNarrativeOutput(initialNarrative);
                    StateHasChanged();
                }
                
                // Clear the task so we don't reuse it again
                _initialNarrativeTask = null;
            }
            
            // Return early if AI is still generating (shouldn't happen after await above)
            if (IsGeneratingNarrative) return;

            SelectedCard = null;

            // Store current cards before listen for animation tracking
            List<CardInstance> previousCards = Session?.HandCards?.ToList() ?? new List<CardInstance>();

            // Mark any opening cards for exhaustion
            List<CardInstance> openingCards = previousCards.Where(c => c.Persistence == PersistenceType.Opening).ToList();
            if (openingCards.Any())
            {
                MarkCardsForExhaust(openingCards);
                await Task.Delay(250); // Let exhaust animation play
            }

            try
            {
                // Validate conversation is still active before executing
                if (!ConversationFacade.IsConversationActive())
                {
                    Console.WriteLine("[ExecuteListen] Warning: Conversation is not active");
                    return;
                }

                // Add notification for listening
                MessageSystem? messageSystem = GameFacade?.GetMessageSystem();
                if (messageSystem != null)
                {
                    messageSystem.AddSystemMessage("You listen carefully...", SystemMessageTypes.Info);
                }

                ConversationTurnResult listenResult = await ConversationFacade.ExecuteListen();

                // Only generate new narrative if we didn't already use the initial one
                if (_initialNarrativeTask == null)
                {
                    // Use AI-generated narrative from the result
                    if (listenResult?.Narrative != null && 
                        !string.IsNullOrWhiteSpace(listenResult.Narrative.NPCDialogue))
                    {
                        Console.WriteLine("[ConversationContent.ExecuteListen] Applying new narrative from listen result");
                        // Apply the complete narrative output including card narratives
                        ApplyNarrativeOutput(listenResult.Narrative);
                    }
                    else
                    {
                        // Fallback to simple generated narrative when AI unavailable
                        GenerateListenNarrative();
                    }
                }
                else
                {
                    Console.WriteLine("[ConversationContent.ExecuteListen] Skipping narrative generation - already applied initial narrative");
                }

                // Check request pile for newly available cards based on rapport
                CheckRequestPileThresholds();

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
                StateHasChanged();
            }
        }

        protected async Task ExecuteSpeak()
        {
            if (IsGeneratingNarrative || Session == null || SelectedCard == null) return;

            try
            {
                // Add notification for speaking
                MessageSystem? messageSystem = GameFacade?.GetMessageSystem();

                // Check if this is an exchange card (identified by exchange data in context)
                if (SelectedCard.Context?.ExchangeData != null && messageSystem != null)
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
                ConversationTurnResult turnResult = await ConversationFacade.ExecuteSpeakSingleCard(SelectedCard);
                CardPlayResult result = turnResult?.CardPlayResult;

                // Use AI-generated narrative from turn result
                if (turnResult?.Narrative != null)
                {
                    // First check for card-specific narrative
                    if (turnResult.Narrative.CardNarratives != null)
                    {
                        CardNarrative cardNarrative = turnResult.Narrative.CardNarratives.FirstOrDefault(cn => cn.CardId == playedCard.Id);
                        if (cardNarrative != null && !string.IsNullOrWhiteSpace(cardNarrative.NarrativeText))
                        {
                            LastNarrative = cardNarrative.NarrativeText;
                        }
                    }
                    // Then check for general narrative text
                    else if (!string.IsNullOrWhiteSpace(turnResult.Narrative.NarrativeText))
                    {
                        LastNarrative = turnResult.Narrative.NarrativeText;
                    }
                    // Finally fall back to simple generated narrative
                    else
                    {
                        ProcessSpeakResult(result);
                    }
                    
                    // Use AI NPC dialogue if available
                    if (!string.IsNullOrWhiteSpace(turnResult.Narrative.NPCDialogue))
                    {
                        LastDialogue = turnResult.Narrative.NPCDialogue;
                    }
                    else if (result?.NewState != null)
                    {
                        LastDialogue = GetStateTransitionDialogue(result.NewState.Value);
                    }
                }
                else
                {
                    // Fallback to simple generated narrative when AI unavailable
                    ProcessSpeakResult(result);
                }
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

                    // Check if the success effect type is Advancing (ends conversation)
                    if (playedCard.SuccessType == SuccessEffectType.Advancing)
                    {
                        LastNarrative = "The conversation advances significantly. Your connection deepens.";
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
                            messageSystem.AddSystemMessage(string.Format("{0} card(s) succeeded! +{1} flow", successes, result.FinalFlow), SystemMessageTypes.Success);
                        }
                        if (failures > 0)
                        {
                            messageSystem.AddSystemMessage(string.Format("{0} card(s) failed", failures), SystemMessageTypes.Warning);
                        }
                    }
                }

                // Mark impulse cards for exhaust animation (after a delay)
                List<CardInstance> impulseCards = Session?.HandCards?
                    .Where(c => c.Persistence == PersistenceType.Impulse && c.InstanceId != playedCard.InstanceId)
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

        private async Task GenerateInitialNarrative()
        {
            // Set fallback immediately for instant display
            LastNarrative = "The conversation begins...";
            LastDialogue = GetInitialDialogue();
            LastProviderSource = NarrativeProviderType.JsonFallback;
            StateHasChanged(); // Update UI with fallback
            
            // Start the AI generation task but don't await it yet
            _initialNarrativeTask = GenerateInitialNarrativeAsync();
        }
        
        private async Task<NarrativeOutput> GenerateInitialNarrativeAsync()
        {
            try
            {
                // Try to get AI-generated initial narrative
                if (Session != null && Context?.Npc != null && NarrativeService != null)
                {
                    // Get the active cards for the initial state
                    List<CardInstance> activeCards = Session.HandCards?.ToList() ?? new List<CardInstance>();
                    
                    // Start AI generation
                    IsGeneratingNarrative = true;
                    StateHasChanged(); // Update UI to show loading state
                    
                    Console.WriteLine("[ConversationContent] Starting initial AI narrative generation (Phase 1 - NPC dialogue only)...");
                    
                    // Request phase 1 narrative (NPC dialogue only) for faster initial UI update
                    NarrativeOutput narrative = await NarrativeService.GenerateOnlyNPCDialogueAsync(
                        Session,
                        Context.Npc,
                        activeCards);
                    
                    Console.WriteLine($"[ConversationContent] Phase 1 AI narrative received. Has NPC dialogue: {!string.IsNullOrWhiteSpace(narrative?.NPCDialogue)}");
                    
                    if (narrative != null && !string.IsNullOrWhiteSpace(narrative.NPCDialogue))
                    {
                        ApplyNarrativeOutput(narrative);
                        StateHasChanged(); // Update UI with AI narrative
                        return narrative;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ConversationContent] Failed to generate initial AI narrative: {ex.Message}");
            }
            finally
            {
                IsGeneratingNarrative = false;
                StateHasChanged(); // Ensure loading state is cleared
            }
            
            return null;
        }
        
        private void ApplyNarrativeOutput(NarrativeOutput narrative)
        {
            if (narrative == null) return;
            
            Console.WriteLine($"[ConversationContent.ApplyNarrativeOutput] Applying narrative output. Provider: {narrative.ProviderSource}");
            
            CurrentNarrativeOutput = narrative;
            
            // Update NPC dialogue and narrative
            if (!string.IsNullOrWhiteSpace(narrative.NPCDialogue))
            {
                LastDialogue = narrative.NPCDialogue;
                Console.WriteLine($"[ConversationContent.ApplyNarrativeOutput] Set NPC dialogue: {LastDialogue.Substring(0, Math.Min(50, LastDialogue.Length))}...");
            }
            if (!string.IsNullOrWhiteSpace(narrative.NarrativeText))
            {
                LastNarrative = narrative.NarrativeText;
            }
            
            LastProviderSource = narrative.ProviderSource;
            
            // Apply card narratives
            CurrentCardNarratives.Clear();
            if (narrative.CardNarratives != null && narrative.CardNarratives.Any())
            {
                Console.WriteLine($"[ConversationContent.ApplyNarrativeOutput] Applying {narrative.CardNarratives.Count} card narratives");
                CurrentCardNarratives.AddRange(narrative.CardNarratives);
                foreach (var cardNarrative in narrative.CardNarratives)
                {
                    if (!string.IsNullOrWhiteSpace(cardNarrative.NarrativeText))
                    {
                        Console.WriteLine($"[ConversationContent.ApplyNarrativeOutput] Card {cardNarrative.CardId}: {cardNarrative.NarrativeText.Substring(0, Math.Min(50, cardNarrative.NarrativeText.Length))}...");
                    }
                }
            }
            else
            {
                Console.WriteLine("[ConversationContent.ApplyNarrativeOutput] No card narratives in output");
                // Trigger second phase to generate card narratives based on NPC dialogue
                if (!string.IsNullOrWhiteSpace(narrative.NPCDialogue))
                {
                    _ = GenerateCardNarrativesAsync(narrative.NPCDialogue);
                }
            }
        }
        
        private async Task GenerateCardNarrativesAsync(string npcDialogue)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(npcDialogue) || Session == null || Context?.Npc == null)
                    return;
                
                Console.WriteLine("[ConversationContent.GenerateCardNarrativesAsync] Starting second phase card narrative generation");
                
                // Get the active cards for current state
                List<CardInstance> activeCards = Session.HandCards?.ToList() ?? new List<CardInstance>();
                if (!activeCards.Any())
                    return;
                
                // Call phase 2 to generate card narratives based on NPC dialogue
                List<CardNarrative> cardNarratives = await NarrativeService.GenerateOnlyCardNarrativesAsync(
                    Session,
                    Context.Npc,
                    activeCards,
                    npcDialogue);
                
                if (cardNarratives != null && cardNarratives.Any())
                {
                    Console.WriteLine($"[ConversationContent.GenerateCardNarrativesAsync] Generated {cardNarratives.Count} card narratives");
                    
                    // Apply the card narratives to the UI
                    CurrentCardNarratives.Clear();
                    CurrentCardNarratives.AddRange(cardNarratives);
                    
                    foreach (var cardNarrative in cardNarratives)
                    {
                        if (!string.IsNullOrWhiteSpace(cardNarrative.NarrativeText))
                        {
                            Console.WriteLine($"[ConversationContent.GenerateCardNarrativesAsync] Card {cardNarrative.CardId}: {cardNarrative.NarrativeText.Substring(0, Math.Min(50, cardNarrative.NarrativeText.Length))}...");
                        }
                    }
                    
                    // Update UI to show the new card narratives
                    StateHasChanged();
                }
                else
                {
                    Console.WriteLine("[ConversationContent.GenerateCardNarrativesAsync] No card narratives generated in phase 2");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ConversationContent.GenerateCardNarrativesAsync] Error generating card narratives: {ex.Message}");
            }
        }
        
        private ConversationState BuildConversationState(ConversationSession session)
        {
            int rapport = session.RapportManager?.CurrentRapport ?? 0;
            TopicLayer currentLayer = rapport <= 5 ? TopicLayer.Deflection :
                                    rapport <= 10 ? TopicLayer.Gateway :
                                    TopicLayer.Core;
            
            return new ConversationState
            {
                Flow = session.FlowBattery,
                Rapport = rapport,
                Atmosphere = session.CurrentAtmosphere,
                Focus = session.GetAvailableFocus(),
                Patience = session.CurrentPatience,
                CurrentState = session.CurrentState,
                CurrentTopicLayer = currentLayer,
                HighestTopicLayerReached = currentLayer,
                TotalTurns = session.TurnNumber,
                ConversationHistory = new List<string>()
            };
        }
        
        private NPCData BuildNPCData(NPC npc)
        {
            return new NPCData
            {
                NpcId = npc.ID,
                Name = npc.Name,
                Personality = npc.PersonalityType,
                CurrentCrisis = npc.CurrentState == ConnectionState.DISCONNECTED ? "personal_troubles" : null,
                CurrentTopic = "general"
            };
        }
        
        private CardCollection BuildCardCollection(List<CardInstance> cards)
        {
            CardCollection collection = new CardCollection();
            foreach (CardInstance card in cards)
            {
                CardInfo cardInfo = new CardInfo
                {
                    Id = card.Id,
                    Focus = card.Focus,
                    Difficulty = card.Difficulty,
                    Effect = card.Description ?? "",
                    Persistence = card.Persistence,
                    NarrativeCategory = "standard"
                };
                collection.Cards.Add(cardInfo);
            }
            return collection;
        }

        private void GenerateListenNarrative()
        {
            LastNarrative = "You listen attentively...";
            
            // For Request conversations, display the request text on LISTEN
            if (Context?.Type == ConversationType.Request && !string.IsNullOrEmpty(Context.RequestText))
            {
                LastDialogue = Context.RequestText;
            }
            else
            {
                LastDialogue = GetStateTransitionDialogue(Session.CurrentState);
            }
        }

        private void GenerateSpeakNarrative(CardPlayResult result)
        {
            // Generate fallback narrative based on success/failure
            if (result.Results != null && result.Results.Any())
            {
                // Fallback to generic narrative if no player narrative provided
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
                if (result.FinalFlow > 0)
                {
                    LastNarrative += " (Flow +1)";
                }
                else if (result.FinalFlow < 0)
                {
                    LastNarrative += " (Flow -1)";
                }
            }
            else
            {
                LastNarrative = "You speak your mind...";
            }

            // Show card's dialogue fragment if available  
            if (SelectedCard != null && !string.IsNullOrEmpty(SelectedCard.DialogueFragment))
            {
                // Display the card's dialogue fragment as player dialogue
                LastDialogue = SelectedCard.DialogueFragment;
            }
            // Otherwise update dialogue based on new state if changed
            else if (result.NewState.HasValue)
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
            if (Session.FlowBattery >= 2)
                return "This conversation has been wonderful!";
            else if (Session.FlowBattery >= 0)
                return "I appreciate you taking the time to talk.";
            else if (Session.FlowBattery >= -2)
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
                DeadlineInSegments = deadline, // Deadline is already in segments
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
            // Deadlines converted to segments (1 day = 36 segments, etc.)
            return tier switch
            {
                LetterTier.Simple => (36, 5, StakeType.REPUTATION, EmotionalFocus.LOW),      // 1 day, 5 coins
                LetterTier.Important => (18, 10, StakeType.WEALTH, EmotionalFocus.MEDIUM),    // 12 seg, 10 coins
                LetterTier.Urgent => (9, 15, StakeType.STATUS, EmotionalFocus.HIGH),         // 6 seg, 15 coins
                LetterTier.Critical => (3, 20, StakeType.SAFETY, EmotionalFocus.CRITICAL),   // 2 seg, 20 coins
                _ => (36, 5, StakeType.REPUTATION, EmotionalFocus.LOW)
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

            // Show categorical properties as labels
            List<string> labels = new List<string>();

            // Add persistence type
            labels.Add(card.Persistence.ToString());

            // Add card type if special
            if (card.CardType != CardType.Conversation)
            {
                labels.Add(card.CardType.ToString());
            }

            // Add effect types if present
            if (card.SuccessType != SuccessEffectType.None)
            {
                labels.Add(card.SuccessType.ToString());
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
                Console.WriteLine($"[ConversationContent] Conversation ended with outcome: Flow={outcome.FinalFlow}, TokensEarned={outcome.TokensEarned}");
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
                Console.WriteLine($"[ConversationContent] Conversation ended with outcome: Flow={outcome.FinalFlow}, TokensEarned={outcome.TokensEarned}");
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
            return Session.FlowBattery switch
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
            return (int)((Session.FlowBattery + 3) * 100 / 6.0);
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
            if (card.Context?.ExchangeData != null && card.Context?.ExchangeName != null)
                return card.Context.ExchangeName;

            if (card.CardType == CardType.BurdenGoal)
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
            // Goal cards MUST have rapport threshold in context - no fallbacks
            if (card?.Context == null)
            {
                throw new InvalidOperationException($"Goal card '{card?.Id}' has no context!");
            }
            
            if (card.Context.RapportThreshold <= 0)
            {
                throw new InvalidOperationException($"Goal card '{card.Id}' has invalid rapport threshold: {card.Context.RapportThreshold}");
            }
            
            return card.Context.RapportThreshold;
        }

        private void CheckRequestPileThresholds()
        {
            try
            {
                if (Session?.RequestPile == null || Session.RapportManager == null || Session.ActiveCards == null)
                {
                    Console.WriteLine("[CheckRequestPileThresholds] Session or components are null - skipping");
                    return;
                }

                var currentRapport = Session.RapportManager.CurrentRapport;
                var cardsToMove = new List<CardInstance>();

                foreach (var card in Session.RequestPile)
                {
                    // Skip cards that have already been moved
                    if (MovedRequestCardIds.Contains(card.InstanceId))
                    {
                        continue;
                    }

                    var threshold = GetRequestRapportThreshold(card);
                    if (currentRapport >= threshold)
                    {
                        cardsToMove.Add(card);
                    }
                }

                foreach (var card in cardsToMove)
                {
                    Console.WriteLine($"[CheckRequestPileThresholds] Moving card {card.Id} from RequestPile to ActiveCards (Rapport {currentRapport})");
                    Session.RequestPile.Remove(card);
                    Session.ActiveCards.Add(card);

                    // Track that this card has been moved
                    MovedRequestCardIds.Add(card.InstanceId);

                    // Mark card as playable now that rapport threshold is met
                    card.IsPlayable = true;

                    // Notify player
                    MessageSystem? messageSystem = GameFacade?.GetMessageSystem();
                    if (messageSystem != null)
                    {
                        string cardName = GetCardName(card);
                        string message = $"{cardName} is now available (Rapport {currentRapport}/{GetRequestRapportThreshold(card)})";
                        messageSystem.AddSystemMessage(message, SystemMessageTypes.Success);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CheckRequestPileThresholds] Error: {ex.Message}");
                // Don't rethrow - just log and continue
            }
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
                if (slot.IsOccupied && slot.DeliveryObligation != null && slot.DeliveryObligation.DeadlineInSegments_Display > 0)
                {
                    if (slot.DeliveryObligation.DeadlineInSegments_Display < shortestDeadline)
                    {
                        shortestDeadline = slot.DeliveryObligation.DeadlineInSegments_Display;
                        mostUrgent = slot.DeliveryObligation;
                    }
                }
            }

            if (mostUrgent != null)
            {
                int segments = mostUrgent.DeadlineInSegments_Display;
                if (segments > 0)
                    return $"{mostUrgent.RecipientName}'s letter deadline: {segments} seg";
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
            // First check for AI-generated narrative
            if (CurrentCardNarratives != null && card != null)
            {
                CardNarrative cardNarrative = CurrentCardNarratives.FirstOrDefault(cn => cn.CardId == card.Id);
                if (cardNarrative != null && !string.IsNullOrEmpty(cardNarrative.NarrativeText))
                    return cardNarrative.NarrativeText;
            }
            
            // Fallback to card's Description property or name
            return !string.IsNullOrEmpty(card.Description) ? card.Description : GetProperCardName(card);
        }



        protected string GetTransitionHint()
        {
            if (Session == null) return "";

            if (Session.FlowBattery == 3)
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
            else if (Session.FlowBattery == -3)
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
            if (card.CardType == CardType.BurdenGoal)
                classes.Add("crisis");
            else if (card.Context?.ExchangeData != null)
                classes.Add("exchange");
            else if (card.CardType == CardType.Observation)
                classes.Add("observation");
            else
                classes.Add("flow");

            // Add impulse indicator if applicable
            if (card.Persistence == PersistenceType.Impulse)
                classes.Add("impulse");

            // Request cards get special styling (Impulse + Opening)
            if (card.Persistence == PersistenceType.Impulse && card.Persistence == PersistenceType.Opening)
                classes.Add("request");

            return string.Join(" ", classes);
        }

        protected int CountImpulseCards()
        {
            return Session?.HandCards?.Count(c => c.Persistence == PersistenceType.Impulse) ?? 0;
        }

        protected bool HasRequestCards()
        {
            return Session?.HandCards?.Any(c => (c.CardType == CardType.Letter || c.CardType == CardType.Promise)) ?? false;
        }

        protected string GetCardName(CardInstance card)
        {
            // Use the card's description as its name
            if (!string.IsNullOrEmpty(card.Description))
                return card.Description;

            // For exchange cards, use the exchange name
            if (card.Context?.ExchangeData != null && card.Context?.ExchangeName != null)
                return card.Context.ExchangeName;

            // Generate name from template and context
            if (card.Context?.NPCName != null)
                return $"{card.Id} ({card.Context.NPCName})";
            return card.Id;
        }

        protected List<string> GetCardTags(CardInstance card)
        {
            List<string> tags = new List<string>();

            // Add persistence type as tag
            if (card.Persistence != null)
                tags.Add(card.Persistence.ToString());

            // Add card type for special cards
            if (card.CardType == CardType.Promise)
                tags.Add("Promise");
            else if (card.CardType == CardType.Letter)
                tags.Add("Letter");
            else if (card.CardType == CardType.BurdenGoal)
                tags.Add("Burden");

            // Add success type if meaningful
            if (card.SuccessType != SuccessEffectType.None)
                tags.Add(card.SuccessType.ToString());

            // Add failure type if meaningful
            if (card.FailureType != FailureEffectType.None)
                tags.Add(card.FailureType.ToString());

            // Add exhaust type if meaningful
            if (card.ExhaustType != ExhaustEffectType.None)
                tags.Add(card.ExhaustType.ToString());

            return tags;
        }

        protected string GetSuccessEffect(CardInstance card)
        {
            // For exchange cards, show the reward
            if (card.Context?.ExchangeData?.Rewards != null)
            {
                return $"Complete exchange: {FormatResourceList(card.Context.ExchangeData.Rewards)}";
            }

            // Check if card has a categorical success effect
            if (card.SuccessType != SuccessEffectType.None)
            {
                int magnitude = GetMagnitudeFromDifficulty(card.Difficulty);

                // Display the effect based on type
                return card.SuccessType switch
                {
                    SuccessEffectType.Rapport => magnitude > 0 ? $"+{magnitude} rapport" : $"{magnitude} rapport",
                    SuccessEffectType.Threading => $"Draw {magnitude} card{(magnitude == 1 ? "" : "s")}",
                    SuccessEffectType.Atmospheric => "Set atmosphere",
                    SuccessEffectType.Focusing => $"+{magnitude} focus",
                    SuccessEffectType.Promising => "Move promise to position 1",
                    SuccessEffectType.Advancing => "Advance connection state",
                    _ => "Effect"
                };
            }

            // No effect
            return "No effect";
        }

        protected string GetFailureEffect(CardInstance card)
        {
            // For exchange cards, no failure - it's a choice
            if (card.Context?.ExchangeData != null)
            {
                if (card.Context?.ExchangeName == "Pass on this offer")
                    return "Leave without trading";
                return "Execute trade";
            }

            // Check if card has a categorical failure effect
            if (card.FailureType != FailureEffectType.None)
            {
                int magnitude = GetMagnitudeFromDifficulty(card.Difficulty);

                // Display the effect based on type
                return card.FailureType switch
                {
                    FailureEffectType.Overreach => "Clear entire hand",
                    FailureEffectType.Backfire => $"-{magnitude} rapport",
                    FailureEffectType.Disrupting => "Discard cards with focus 3+",
                    _ => "Force LISTEN"
                };
            }

            // Default failure effect is forcing LISTEN
            return "Force LISTEN";
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
            if (card.Context?.ExchangeData != null)
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
            // Calculate success chance based on card type and state, including all bonuses
            int baseChance = GetBaseSuccessPercentage(card.Difficulty);

            // Add rapport modifier (each point = 1% success modifier)
            int rapportBonus = Session?.RapportManager?.GetSuccessModifier() ?? 0;

            // Add atmosphere modifier (Focused gives +20%)
            int atmosphereBonus = ConversationFacade?.GetAtmosphereManager()?.GetSuccessPercentageBonus() ?? 0;

            // Add level bonus (Level 2: +10%, Level 4: +20% total)
            int levelBonus = GetLevelBonus(card);

            // Add personality modifier (e.g., Mercantile +30% for highest focus)
            int personalityBonus = GetPersonalityModifier(card);

            // Calculate total, clamped to valid percentage range
            int totalChance = Math.Max(0, Math.Min(100, baseChance + rapportBonus + atmosphereBonus + levelBonus + personalityBonus));

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
                            ResourceType.Hunger => "food",
                            ResourceType.Attention => "attention",
                            _ => reward.Type.ToString().ToLower()
                        };
                        rewardParts.Add($"{reward.Amount} {resourceName}");
                    }
                }

                // Item rewards are handled through the Resource system

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
                return string.Format("Letter obtained! Check your queue. (Flow: {0})", Session.FlowBattery);
            }

            if (Session.CurrentPatience <= 0)
            {
                return string.Format("{0}'s patience has been exhausted. They have no more time for you today.", NpcName);
            }

            if (Session.CurrentState == ConnectionState.DISCONNECTED && Session.FlowBattery <= -3)
            {
                return string.Format("{0} is too distressed to continue. The conversation has broken down.", NpcName);
            }


            if (!Session.HandCards.Any() && Session.Deck.RemainingCards == 0)
            {
                return "No more cards available - conversation ended";
            }

            // Default reason based on flow level
            if (Session.FlowBattery >= 2)
            {
                return string.Format("Conversation ended naturally (Flow: {0})", Session.FlowBattery);
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
                        int deadlineSegments = negotiation.FinalTerms.DeadlineSegments;

                        string urgencySuffix = "";
                        if (deadlineSegments <= 4)
                            urgencySuffix = " - CRITICAL!";
                        else if (deadlineSegments <= 12)
                            urgencySuffix = " - URGENT";

                        string msgTemplate = "{0} letter: '{1}' - {2}seg deadline, {3} coins";

                        string message = string.Format(msgTemplate, negotiationOutcome,
                            negotiation.SourcePromiseCard.Description ?? negotiation.SourcePromiseCard.Id,
                            deadlineSegments.ToString(),
                            negotiation.FinalTerms.Payment) + urgencySuffix;

                        messageSystem?.AddSystemMessage(
                            message,
                            deadlineSegments <= 4 ? SystemMessageTypes.Danger : SystemMessageTypes.Success
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
        /// PACKET 6: Get persistence type for a card for display
        /// </summary>
        protected string GetCardPersistenceLabel(CardInstance card)
        {
            if (card == null) return "";
            return card.Persistence switch
            {
                PersistenceType.Thought => "Thought",
                PersistenceType.Impulse => "Impulse",
                PersistenceType.Opening => "Opening",
                _ => "Thought"
            };
        }

        /// <summary>
        /// PACKET 6: Get CSS class for persistence badge
        /// </summary>
        protected string GetPersistenceClass(PersistenceType persistence)
        {
            return persistence.ToString().ToLower();
        }

        /// <summary>
        /// Get CSS class for persistence tag (enhanced from mockup)
        /// </summary>
        protected string GetPersistenceTagClass(PersistenceType persistence)
        {
            return persistence switch
            {
                PersistenceType.Impulse => "tag-impulse",
                PersistenceType.Opening => "tag-opening",
                PersistenceType.Thought => "tag-thought",
                _ => "tag-" + persistence.ToString().ToLower()
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
        /// Get the personality rule description from the session's PersonalityEnforcer
        /// </summary>
        protected string GetPersonalityRuleDescription()
        {
            return Session?.PersonalityEnforcer?.GetRuleDescription() ?? "";
        }

        /// <summary>
        /// Check if a card play would violate personality rules
        /// </summary>
        protected bool IsIllegalPlay(CardInstance card)
        {
            if (Session?.PersonalityEnforcer == null || card == null) return false;

            bool isValid = Session.PersonalityEnforcer.ValidatePlay(card, out string violationMessage);
            return !isValid;  // Return true if play is illegal
        }

        /// <summary>
        /// Get level bonus for a card
        /// </summary>
        protected int GetLevelBonus(CardInstance card)
        {
            if (card == null) return 0;

            // Level 2 adds +10% success
            // Level 4 adds another +10% success (cumulative)
            int levelBonus = 0;
            if (card.Level >= 2) levelBonus += 10;
            if (card.Level >= 4) levelBonus += 10;

            return levelBonus;
        }

        /// <summary>
        /// Get personality modifier for a card
        /// </summary>
        protected int GetPersonalityModifier(CardInstance card)
        {
            if (Session?.PersonalityEnforcer == null || card == null) return 0;

            // For Mercantile personality: Only the card with the HIGHEST focus in hand gets +30%
            if (Session.NPC?.PersonalityType == PersonalityType.MERCANTILE)
            {
                // Find the maximum focus among all available cards in hand
                int maxFocusInHand = 0;
                if (Session.ActiveCards?.Cards != null)
                {
                    foreach (var c in Session.ActiveCards.Cards)
                    {
                        int focusCost = c.GetEffectiveFocus(Session.CurrentState);
                        if (focusCost > maxFocusInHand)
                        {
                            maxFocusInHand = focusCost;
                        }
                    }
                }

                // Only apply bonus if this card has the maximum focus
                int cardFocus = card.GetEffectiveFocus(Session.CurrentState);
                if (cardFocus == maxFocusInHand && maxFocusInHand > 0)
                {
                    return 30;  // Mercantile gives +30% to highest focus card only
                }
            }

            return 0;
        }

        /// <summary>
        /// Get complete success modifier breakdown
        /// </summary>
        protected string GetSuccessModifierBreakdown(CardInstance card)
        {
            List<string> modifiers = new List<string>();

            // Base chance
            int baseChance = GetBaseSuccessPercentage(card.Difficulty);
            modifiers.Add($"{baseChance}% base");

            // Rapport modifier
            int rapportBonus = GetRapportModifier();
            if (rapportBonus != 0)
            {
                string sign = rapportBonus > 0 ? "+" : "";
                modifiers.Add($"{sign}{rapportBonus}% rapport");
            }

            // Level bonus
            int levelBonus = GetLevelBonus(card);
            if (levelBonus > 0)
            {
                modifiers.Add($"+{levelBonus}% level");
            }

            // Personality modifier
            int personalityMod = GetPersonalityModifier(card);
            if (personalityMod > 0)
            {
                modifiers.Add($"+{personalityMod}% personality");
            }

            return string.Join(", ", modifiers);
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
        /// PACKET 6: Get icon for persistence badge
        /// </summary>
        protected string GetPersistenceIcon(PersistenceType persistence)
        {
            return persistence switch
            {
                PersistenceType.Impulse => "⚡",
                PersistenceType.Opening => "⏰",
                PersistenceType.Thought => "✓",
                _ => ""
            };
        }

        /// <summary>
        /// PACKET 6: Get label for persistence badge
        /// </summary>
        protected string GetPersistenceLabel(PersistenceType persistence)
        {
            return persistence switch
            {
                PersistenceType.Impulse => "Impulse",
                PersistenceType.Opening => "Opening",
                PersistenceType.Thought => "Thought",
                _ => persistence.ToString()
            };
        }

        /// <summary>
        /// PACKET 6: Get tooltip for persistence badge
        /// </summary>
        protected string GetPersistenceTooltip(PersistenceType persistence)
        {
            return persistence switch
            {
                PersistenceType.Impulse => "Removed after SPEAK if unplayed",
                PersistenceType.Opening => "Removed after LISTEN if unplayed",
                PersistenceType.Thought => "Stays until played",
                _ => ""
            };
        }

        /// <summary>
        /// PACKET 6: Get additional CSS classes based on card properties
        /// </summary>
        protected string GetCardPropertyClasses(CardInstance card)
        {
            List<string> classes = new List<string>();

            if (card?.Persistence == PersistenceType.Impulse)
                classes.Add("has-impulse");
            if (card?.Persistence == PersistenceType.Opening)
                classes.Add("has-opening");
            if (card?.CardType == CardType.BurdenGoal)
                classes.Add("has-burden");

            return string.Join(" ", classes);
        }

        /// <summary>
        /// PACKET 6: Check if card has an exhaust effect
        /// </summary>
        protected bool HasExhaustEffect(CardInstance card)
        {
            // Only impulse and opening cards have exhaust effects
            return card?.Persistence == PersistenceType.Impulse || card?.Persistence == PersistenceType.Opening;
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
            // Get exhaust effect from the card's categorical data
            if (card?.ExhaustType != null && card.ExhaustType != ExhaustEffectType.None)
            {
                int magnitude = GetMagnitudeFromDifficulty(card.Difficulty);
                return card.ExhaustType switch
                {
                    ExhaustEffectType.Threading => $"Draw {magnitude} card{(magnitude == 1 ? "" : "s")}",
                    ExhaustEffectType.Focusing => $"+{magnitude} focus",
                    ExhaustEffectType.Regret => $"-{magnitude} rapport",
                    _ => card.ExhaustType.ToString()
                };
            }

            // Fallback descriptions based on persistence
            if (card?.Persistence == PersistenceType.Impulse)
            {
                return "Card removed";
            }
            else if (card?.Persistence == PersistenceType.Opening)
            {
                return "Card removed";
            }

            return "";
        }

        /// <summary>
        /// PACKET 6: Calculate magnitude from difficulty
        /// </summary>
        private int GetMagnitudeFromDifficulty(Difficulty difficulty)
        {
            return difficulty switch
            {
                Difficulty.VeryEasy => 1,
                Difficulty.Easy => 1,
                Difficulty.Medium => 2,
                Difficulty.Hard => 3,
                Difficulty.VeryHard => 4,
                _ => 1
            };
        }

        /// <summary>
        /// PACKET 6: Get description for EndConversation effects
        /// </summary>
        private string GetEndConversationDescription(string reason)
        {
            // Use the provided reason string

            if (!string.IsNullOrEmpty(reason))
            {
                return reason switch
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
                .Where(c => c.Persistence == PersistenceType.Impulse && c != SelectedCard) // Don't include the played card
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
                .Where(c => (c.CardType == CardType.Letter || c.CardType == CardType.Promise)) // Request cards have Opening property
                .ToList();
        }

        /// <summary>
        /// Get critical exhausts (request cards) from a list of cards
        /// </summary>
        protected List<CardInstance> GetCriticalExhausts(List<CardInstance> cards)
        {
            return cards.Where(c => (c.CardType == CardType.Letter || c.CardType == CardType.Promise)).ToList();
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
            if (card?.Persistence == PersistenceType.Impulse && card.Persistence == PersistenceType.Opening == true)
            {
                return "ENDS CONVERSATION!";
            }
            else if (card?.Persistence == PersistenceType.Impulse)
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
            if (card.Persistence == PersistenceType.Impulse)
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
            List<CardInstance> impulseCards = currentCards.Where(c => c.Persistence == PersistenceType.Impulse).ToList();
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
                .Where(c => c.Persistence == PersistenceType.Impulse && c.InstanceId != playedCard.InstanceId)
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

    // ===== NEW UI UPDATE METHODS =====

    /// <summary>
    /// Helper class for player stat display information
    /// </summary>
    public class PlayerStatInfo
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public int CurrentXP { get; set; }
        public int RequiredXP { get; set; }
    }

    /// <summary>
    /// Returns list of player stats for display in conversation UI
    /// </summary>
    protected List<PlayerStatInfo> GetPlayerStats()
    {
        var result = new List<PlayerStatInfo>();

        if (GameFacade == null) return result;

        try
        {
            PlayerStats stats = GameFacade.GetPlayerStats();

            // Get all five core stats
            foreach (PlayerStatType stat in Enum.GetValues<PlayerStatType>())
            {
                result.Add(new PlayerStatInfo
                {
                    Name = GetStatDisplayName(stat),
                    Level = stats.GetLevel(stat),
                    CurrentXP = stats.GetXP(stat),
                    RequiredXP = stats.GetXPToNextLevel(stat)
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ConversationContent.GetPlayerStats] Error retrieving player stats: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Get display name for a stat type
    /// </summary>
    private string GetStatDisplayName(PlayerStatType stat)
    {
        return stat switch
        {
            PlayerStatType.Insight => "Insight",
            PlayerStatType.Rapport => "Rapport",
            PlayerStatType.Authority => "Authority",
            PlayerStatType.Commerce => "Commerce",
            PlayerStatType.Cunning => "Cunning",
            _ => stat.ToString()
        };
    }

    /// <summary>
    /// Returns short personality rule text for the NPC bar badge
    /// </summary>
    protected string GetPersonalityRuleShort()
    {
        if (Session?.NPC == null) return "";

        // Get the actual mechanical rules from the personality type
        var personality = Session.NPC.PersonalityType;
        return personality switch
        {
            PersonalityType.DEVOTED => "Devoted: Rapport losses doubled",
            PersonalityType.MERCANTILE => "Mercantile: Highest focus +30% success",
            PersonalityType.PROUD => "Proud: Cards must ascend in focus",
            PersonalityType.CUNNING => "Cunning: Same focus as prev -2 rapport",
            PersonalityType.STEADFAST => "Steadfast: Rapport changes capped ±2",
            _ => $"{personality}: Special rules apply"
        };
    }

    /// <summary>
    /// Returns CSS class for patience slots based on current usage
    /// </summary>
    protected string GetPatienceSlotClass(int slot)
    {
        if (Session == null) return "";

        int currentPatienceUsed = Session.CurrentPatience;

        if (slot <= currentPatienceUsed)
            return "used";
        else if (slot == currentPatienceUsed + 1)
            return "current";
        else
            return "";
    }

    /// <summary>
    /// Returns CSS classes for flow battery segments based on current flow value
    /// </summary>
    protected string GetFlowSegmentClass(int segment)
    {
        if (Session == null) return "";

        int currentFlow = Session.FlowBattery;

        // Negative segments (-3 to -1)
        if (segment < 0)
        {
            return currentFlow <= segment ? "active" : "inactive";
        }
        // Positive segments (1 to 3)
        else if (segment > 0)
        {
            return currentFlow >= segment ? "active" : "inactive";
        }

        return "";
    }

    /// <summary>
    /// Returns list of request goals/thresholds for the current conversation
    /// </summary>
    protected List<RequestGoal> GetRequestGoals()
    {
        var goals = new List<RequestGoal>();

        // First check if we have an active session with request cards
        if (Session?.ActiveCards != null)
        {
            // Look for request/promise cards that are already loaded in the session
            var requestCardsInSession = Session.ActiveCards.Cards
                .Concat(Session.DrawPile?.Cards ?? new List<CardInstance>())
                .Where(c => c.CardType == CardType.Letter || c.CardType == CardType.Promise || c.CardType == CardType.BurdenGoal)
                .Where(c => c.Template?.RapportThreshold > 0)
                .GroupBy(c => c.Template.RapportThreshold)
                .OrderBy(g => g.Key);

            foreach (var group in requestCardsInSession)
            {
                var threshold = group.Key;
                var firstCard = group.First();

                // Determine reward based on threshold
                string reward = threshold switch
                {
                    <= 5 => "1 Trust token",
                    <= 10 => "2 Trust tokens",
                    <= 15 => "3 Trust tokens + Observation",
                    _ => "Special reward"
                };

                // Use descriptive name based on threshold
                string goalName = threshold switch
                {
                    <= 5 => "Basic Delivery",
                    <= 10 => "Priority Delivery",
                    <= 15 => "Immediate Action",
                    _ => firstCard.Template?.Description ?? "Special Request"
                };

                goals.Add(new RequestGoal
                {
                    Threshold = threshold,
                    Name = goalName,
                    Reward = reward
                });
            }
        }

        // Fallback to hardcoded goals if no session cards found
        if (!goals.Any())
        {
            goals.Add(new RequestGoal { Threshold = 5, Name = "Basic Progress", Reward = "1 Trust token" });
            goals.Add(new RequestGoal { Threshold = 10, Name = "Good Progress", Reward = "2 Trust tokens" });
            goals.Add(new RequestGoal { Threshold = 15, Name = "Excellent Progress", Reward = "3 Trust tokens + Observation" });
        }

        return goals;
    }

    /// <summary>
    /// Helper class for request goal information
    /// </summary>
    public class RequestGoal
    {
        public int Threshold { get; set; }
        public string Name { get; set; }
        public string Reward { get; set; }
    }

    /// <summary>
    /// Returns the name of the current request/letter being worked on
    /// </summary>
    protected string GetRequestName()
    {
        // Try to get the current letter or conversation context
        if (Session?.NPC != null)
        {
            return $"Conversation with {Session.NPC.Name}";
        }

        return "Current Request";
    }

    /// <summary>
    /// Returns CSS class for goal cards based on current rapport vs threshold
    /// </summary>
    protected string GetGoalCardClass(int threshold)
    {
        if (Session?.RapportManager == null) return "";

        int currentRapport = Session.RapportManager.CurrentRapport;

        if (currentRapport >= threshold)
            return "achievable";
        else if (currentRapport >= threshold - 2) // Close to achievable
            return "active";
        else
            return "";
    }

    /// <summary>
    /// Returns CSS class name for card stat badges (lowercase stat name)
    /// </summary>
    protected string GetCardStatClass(CardInstance card)
    {
        if (card?.Template?.BoundStat == null) return "";

        return card.Template.BoundStat.Value.ToString().ToLower();
    }

    /// <summary>
    /// Returns the stat type as a lowercase string for applying stat-based color classes
    /// </summary>
    protected string GetCardStatColorClass(CardInstance card)
    {
        if (card?.Template?.BoundStat == null) return "";

        return card.Template.BoundStat.Value.ToString().ToLower();
    }

    /// <summary>
    /// Analyzes card effects and returns appropriate effect tags
    /// </summary>
    protected List<string> GetCardEffectTags(CardInstance card)
    {
        var tags = new List<string>();

        if (card?.Template == null) return tags;

        // Success effect tags
        if (card.SuccessType != SuccessEffectType.None)
        {
            tags.Add(card.SuccessType.ToString());
        }

        // Failure effect tags
        if (card.FailureType != FailureEffectType.None)
        {
            tags.Add(card.FailureType.ToString());
        }

        // Exhaust effect tags (only if not None)
        if (card.ExhaustType != ExhaustEffectType.None)
        {
            tags.Add($"Exhaust: {card.ExhaustType}");
        }

        return tags;
    }

    /// <summary>
    /// Returns proper display name for card's bound stat
    /// </summary>
    protected string GetCardStatName(CardInstance card)
    {
        if (card?.Template?.BoundStat == null) return "";

        return GetStatDisplayName(card.Template.BoundStat.Value);
    }

    /// <summary>
    /// Returns the effective level of a card based on player's stat level
    /// </summary>
    protected int GetCardLevel(CardInstance card)
    {
        if (card?.Template?.BoundStat == null || GameFacade == null) return 1;

        try
        {
            PlayerStats stats = GameFacade.GetPlayerStats();
            return stats.GetLevel(card.Template.BoundStat.Value);
        }
        catch
        {
            return 1;
        }
    }

    /// <summary>
    /// Returns XP gain for playing cards in this conversation based on conversation level
    /// </summary>
    protected int GetXPGain()
    {
        // XP is based on conversation difficulty/level
        // For now, return a base value - this should be enhanced based on actual conversation level
        if (Session?.NPC != null)
        {
            // If it's a stranger conversation, use stranger level
            // Otherwise use base conversation XP
            return 1; // Level 1 conversations give 1 XP, Level 2 give 2 XP, Level 3 give 3 XP
        }

        return 1;
    }

}
}