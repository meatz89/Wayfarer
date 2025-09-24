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
    /// </summary>
    public class ConversationContentBase : ComponentBase
    {
        [Parameter] public ConversationContextBase Context { get; set; }
        [Parameter] public EventCallback OnConversationEnd { get; set; }
        [CascadingParameter] public GameScreenBase GameScreen { get; set; }

        [Inject] protected ConversationFacade ConversationFacade { get; set; }
        [Inject] protected GameFacade GameFacade { get; set; }
        [Inject] protected ConversationNarrativeService NarrativeService { get; set; }

        /// <summary>
        /// PROJECTION PRINCIPLE: The CategoricalEffectResolver is a pure projection function
        /// that returns what WOULD happen without modifying state. Both UI (for preview)
        /// and game logic (for execution) call the resolver to get projections.
        /// This ensures the UI can accurately display what effects WILL occur.
        /// </summary>
        [Inject] protected CategoricalEffectResolver EffectResolver { get; set; }

        protected ConversationSession Session { get; set; }
        protected CardInstance? SelectedCard { get; set; } = null;
        protected int TotalSelectedFocus => SelectedCard?.Focus ?? 0;
        protected bool IsConversationExhausted { get; set; } = false;
        protected string ExhaustionReason { get; set; } = "";

        // Action preview state
        protected bool ShowSpeakPreview { get; set; } = false;
        protected bool ShowListenPreview { get; set; } = false;

        // Static UI system - no animation, no state management

        // Static system - no animations
        // Track which request cards have already been moved from RequestPile to ActiveCards
        protected HashSet<string> MovedRequestCardIds { get; set; } = new();

        protected string NpcName { get; set; }
        protected string LastNarrative { get; set; }
        protected string LastDialogue { get; set; }
        protected NarrativeProviderType LastProviderSource { get; set; } = NarrativeProviderType.JsonFallback; // Default to fallback

        // AI narrative generation state
        protected bool IsGeneratingNarrative { get; set; } = false;
        protected List<CardNarrative> CurrentCardNarratives { get; set; } = new List<CardNarrative>();
        protected NarrativeOutput CurrentNarrativeOutput { get; set; }
        private Task<NarrativeOutput> _initialNarrativeTask = null;

        // Static UI - no animation blocking needed

        /// <summary>
        /// Legacy processing flag - still used for backend calls, but will be replaced by animation orchestration
        /// </summary>
        protected bool IsProcessing { get; set; } = false;

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

        protected async Task ExecuteListen()
        {
            // UI ANIMATION ORCHESTRATION: Block during animations for better UX
            if (Session == null || IsProcessing) return;

            // Static UI - no animation interruption handling needed

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
                    // REMOVED: StateHasChanged() - prevents card DOM recreation
                }

                // Clear the task so we don't reuse it again
                _initialNarrativeTask = null;
            }

            // Return early if AI is still generating (shouldn't happen after await above)
            if (IsGeneratingNarrative) return;

            SelectedCard = null;

            // SYNCHRONOUS PRINCIPLE: Clean up old animation states periodically
            // Legacy animation cleanup removed - choreography system handles cleanup

            // Store current cards before listen for animation tracking
            List<CardInstance> previousCards = ConversationFacade.GetHandCards()?.ToList() ?? new List<CardInstance>();

            try
            {
                // SYNCHRONOUS PRINCIPLE: Set processing flag during backend call only
                IsProcessing = true;
                StateHasChanged(); // Update UI to disable buttons

                // Validate conversation is still active before executing
                if (!GameFacade.IsConversationActive())
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

                // Static UI - no shadow state needed

                ConversationTurnResult listenResult = null;

                try
                {
                    listenResult = await GameFacade.ExecuteListen();

                    // EDGE CASE: Check for failed action results
                    if (listenResult == null)
                    {
                        Console.WriteLine("[ExecuteListen] Action failed");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ExecuteListen] Exception during action: {ex.Message}");
                    throw; // Re-throw to maintain error visibility
                }

                // SYNCHRONOUS PRINCIPLE: Backend call complete, clear processing flag
                IsProcessing = false;

                // Track cards after action
                List<CardInstance> currentCards = ConversationFacade.GetHandCards()?.ToList() ?? new List<CardInstance>();
                List<CardInstance> drawnCards = currentCards
                    .Where(c => !previousCards.Any(pc => pc.InstanceId == c.InstanceId))
                    .ToList();

                // CRITICAL: Wait for ALL narrative generation to complete BEFORE allowing cards to render
                if (_initialNarrativeTask == null)
                {
                    // Use AI-generated narrative from the result
                    if (listenResult?.Narrative != null &&
                        !string.IsNullOrWhiteSpace(listenResult.Narrative.NPCDialogue))
                    {
                        Console.WriteLine("[ConversationContent.ExecuteListen] Applying narrative from listen result and waiting for card narratives");
                        // Apply the complete narrative output including card narratives
                        ApplyNarrativeOutput(listenResult.Narrative);

                        // AWAIT card narrative generation to complete before adding cards to DOM
                        if (drawnCards.Any())
                        {
                            await GenerateCardNarrativesAsync(listenResult.Narrative.NPCDialogue);
                        }
                    }
                    else
                    {
                        Console.WriteLine("[ConversationContent.ExecuteListen] Using fallback narrative generation");
                        // Fallback to simple generated narrative when AI unavailable
                        GenerateListenNarrative();
                    }
                }
                else
                {
                    Console.WriteLine("[ConversationContent.ExecuteListen] Skipping narrative generation - already applied initial narrative");
                }

                // Check request pile for newly available cards based on Momentum
                CheckRequestPileThresholds();

                // Static UI - cards appear instantly with final narrative text
                if (drawnCards.Any())
                {
                    Console.WriteLine($"[ExecuteListen] Adding {drawnCards.Count} new cards to display (narrative complete)");
                    // Cards will appear instantly when GetAllDisplayCards() is called
                }
                else
                {
                    Console.WriteLine("[ExecuteListen] No new cards drawn");
                }

                // Notify about cards drawn
                IReadOnlyList<CardInstance> handCards = ConversationFacade.GetHandCards();
                if (messageSystem != null && handCards.Any())
                {
                    messageSystem.AddSystemMessage(string.Format("Drew {0} cards", handCards.Count), SystemMessageTypes.Success);
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
                // SYNCHRONOUS PRINCIPLE: Always clear processing flag
                IsProcessing = false;
                StateHasChanged();
            }
        }

        protected async Task ExecuteSpeak()
        {
            // UI ANIMATION ORCHESTRATION: Block during animations for better UX
            if (IsGeneratingNarrative || IsProcessing || Session == null || SelectedCard == null) return;

            // Static UI - no animation interruption handling needed

            try
            {
                // SYNCHRONOUS PRINCIPLE: Set processing flag during backend call only
                IsProcessing = true;

                // Clean up old animation states periodically
                // Legacy animation cleanup removed - choreography system handles cleanup

                // Store cards before speak to track what gets removed
                List<CardInstance> cardsBeforeSpeak = ConversationFacade.GetHandCards()?.ToList() ?? new List<CardInstance>();

                StateHasChanged(); // Update UI to disable buttons

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

                // Static UI - no shadow state needed

                // Static system - card immediately removed
                // REMOVED: StateHasChanged() - prevents DOM recreation during animation

                // ExecuteSpeak expects a single card - this removes it from hand
                ConversationTurnResult turnResult = null;
                CardPlayResult result = null;

                try
                {
                    turnResult = await GameFacade.PlayConversationCard(SelectedCard);
                    result = turnResult?.CardPlayResult;

                    // EDGE CASE: Check for failed action results
                    if (result == null || (result.Results?.All(r => !r.Success) ?? true))
                    {
                        Console.WriteLine("[ExecuteSpeak] Action failed");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ExecuteSpeak] Exception during action: {ex.Message}");
                    throw; // Re-throw to maintain error visibility
                }

                // SYNCHRONOUS PRINCIPLE: Backend call complete, clear processing flag
                IsProcessing = false;

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
                // REMOVED: StateHasChanged() - prevents DOM recreation during animation

                // Static system - card immediately removed
                bool wasSuccessful = result?.Results?.FirstOrDefault()?.Success ?? false;

                // NO DELAY - game state is already updated, animation is just visual

                // Check if this was a promise/goal card that succeeded
                bool isPromiseCard = playedCard.CardType == CardType.Letter || playedCard.CardType == CardType.Promise || playedCard.CardType == CardType.Letter;
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

                // Track cards after action
                List<CardInstance> currentCardsAfterSpeak = ConversationFacade.GetHandCards()?.ToList() ?? new List<CardInstance>();

                // SIMPLIFIED: Played card animation handled above, other cards removed immediately
                // Check for new cards drawn (if Threading success effect or other draw effects)
                List<CardInstance> drawnCards = currentCardsAfterSpeak
                    .Where(c => !cardsBeforeSpeak.Any(bc => bc.InstanceId == c.InstanceId))
                    .ToList();

                Console.WriteLine($"[ExecuteSpeak] Card played: {playedCard?.ConversationCardTemplate?.Id}, new cards: {drawnCards.Count}");
                // Static UI - played cards disappear instantly, new cards appear instantly

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
                // SYNCHRONOUS PRINCIPLE: Always clear processing flag
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
                    List<CardInstance> activeCards = ConversationFacade.GetHandCards()?.ToList() ?? new List<CardInstance>();

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
                        // REMOVED: StateHasChanged() - prevents card DOM recreation
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
                // REMOVED: StateHasChanged() - prevents card DOM recreation
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
                foreach (CardNarrative cardNarrative in narrative.CardNarratives)
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
                // NOTE: Card narrative generation now handled synchronously in ExecuteListen
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
                List<CardInstance> activeCards = ConversationFacade.GetHandCards()?.ToList() ?? new List<CardInstance>();
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

                    foreach (CardNarrative cardNarrative in cardNarratives)
                    {
                        if (!string.IsNullOrWhiteSpace(cardNarrative.NarrativeText))
                        {
                            Console.WriteLine($"[ConversationContent.GenerateCardNarrativesAsync] Card {cardNarrative.CardId}: {cardNarrative.NarrativeText.Substring(0, Math.Min(50, cardNarrative.NarrativeText.Length))}...");
                        }
                    }

                    // REMOVED: StateHasChanged() - cards not in DOM yet during narrative generation
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

        private void GenerateListenNarrative()
        {
            LastNarrative = "You listen attentively...";

            // For Request conversations, display the request text on LISTEN
            if (Context?.ConversationTypeId == "request" && !string.IsNullOrEmpty(Context.RequestText))
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
            return GameFacade.CanPlayCard(card, Session);
        }

        protected bool CanSpeak()
        {
            return SelectedCard != null && TotalSelectedFocus <= GetFocusLimit();
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
            if (Session == null)
            {
                Console.WriteLine("ERROR: Session is null in GetFocusLimit");
                throw new InvalidOperationException("Session not initialized - cannot get focus limit");
            }

            // Use actual values from ConversationRules.States
            if (ConversationRules.States.TryGetValue(Session.CurrentState, out ConversationStateRules? rules))
            {
                return rules.MaxFocus;
            }

            Console.WriteLine($"ERROR: No rules found for conversation state {Session.CurrentState} in GetFocusLimit");
            throw new InvalidOperationException($"Missing conversation rules for state: {Session.CurrentState}");
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

        protected string GetSpeakActionText()
        {
            if (SelectedCard != null)
            {
                int focus = SelectedCard.Focus;
                int remainingAfter = (Session?.GetAvailableFocus() ?? 0) - focus;
                string continueHint = remainingAfter > 0 ? $" (Can SPEAK {remainingAfter} more)" : " (Must LISTEN after)";
                return $"Play Card ({focus} focus)";
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

            if (card.CardType == CardType.Letter)
                return "Address Past Failure";

            if (card.CardType == CardType.Observation)
            {
                return "Share Observation";
            }

            // Default to template ID with formatting
            return card.Id?.Replace("_", " ") ?? "Unknown Card";
        }

        protected int GetRequestMomentumThreshold(CardInstance card)
        {
            // Goal cards MUST have momentum threshold in context - no fallbacks
            if (card?.Context == null)
            {
                throw new InvalidOperationException($"Goal card '{card?.Id}' has no context!");
            }

            if (card.Context.MomentumThreshold <= 0)
            {
                throw new InvalidOperationException($"Goal card '{card.Id}' has invalid momentum threshold: {card.Context.MomentumThreshold}");
            }

            return card.Context.MomentumThreshold;
        }

        private void CheckRequestPileThresholds()
        {
            try
            {
                if (Session?.MomentumManager == null)
                {
                    Console.WriteLine("[CheckRequestPileThresholds] Session or components are null - skipping");
                    return;
                }

                int currentMomentum = Session.MomentumManager.CurrentMomentum;
                List<CardInstance> cardsToMove = new List<CardInstance>();
                IReadOnlyList<CardInstance> requestCards = ConversationFacade.GetRequestCards();

                foreach (CardInstance card in requestCards)
                {
                    // Skip cards that have already been moved
                    if (MovedRequestCardIds.Contains(card.InstanceId))
                    {
                        continue;
                    }

                    int threshold = GetRequestMomentumThreshold(card);
                    if (currentMomentum >= threshold)
                    {
                        cardsToMove.Add(card);
                    }
                }

                // Use SessionCardDeck's proper method instead of direct manipulation
                // This prevents card loss and maintains proper encapsulation
                if (cardsToMove.Count > 0)
                {
                    Console.WriteLine($"[CheckRequestPileThresholds] Checking request thresholds with momentum {currentMomentum}");

                    // Let ConversationFacade handle the move properly
                    ConversationFacade.CheckAndMoveRequestCards();

                    // Track and notify for moved cards
                    foreach (CardInstance card in cardsToMove)
                    {
                        // Track that this card has been moved
                        MovedRequestCardIds.Add(card.InstanceId);

                        // Mark card as playable now that Momentum threshold is met
                        card.IsPlayable = true;

                        // Notify player
                        MessageSystem? messageSystem = GameFacade?.GetMessageSystem();
                        if (messageSystem != null)
                        {
                            string cardName = GetCardName(card);
                            string message = $"{cardName} is now available (Momentum {currentMomentum}/{GetRequestMomentumThreshold(card)})";
                            messageSystem.AddSystemMessage(message, SystemMessageTypes.Success);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CheckRequestPileThresholds] Error: {ex.Message}");
                // Don't rethrow - just log and continue
            }
        }

        protected int GetCurrentMomentum()
        {
            return Session?.MomentumManager?.CurrentMomentum ?? 0;
        }

        protected string GetDoubtSlotClass(int slotNumber)
        {
            int currentDoubt = Session?.CurrentDoubt ?? 0;
            return slotNumber <= currentDoubt ? "filled" : "empty";
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

            return string.Join(" <span class='icon-bullet'></span> ", status);
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

        /// <summary>
        /// PROJECTION PRINCIPLE: ALWAYS use effect resolver to get projection of what WOULD happen.
        /// This ensures UI accurately shows what effects will occur without modifying game state.
        /// NO FALLBACKS - the resolver is the single source of truth for effect projections.
        /// </summary>
        protected string GetSuccessEffect(CardInstance card)
        {
            // For exchange cards, show the reward
            if (card.Context?.ExchangeData?.Rewards != null)
            {
                return $"Complete exchange: {FormatResourceList(card.Context.ExchangeData.Rewards)}";
            }

            // PROJECTION PRINCIPLE: ALWAYS use resolver for ALL effects
            if (card.SuccessType != SuccessEffectType.None)
            {
                CardEffectResult projection = EffectResolver.ProcessSuccessEffect(card, Session);

                if (projection.MomentumChange != 0)
                {
                    return projection.MomentumChange > 0
                        ? $"+{projection.MomentumChange} Momentum"
                        : $"{projection.MomentumChange} Momentum";
                }

                if (projection.CardsToAdd?.Count > 0)
                {
                    int count = projection.CardsToAdd.Count;
                    return $"Draw {count} card{(count == 1 ? "" : "s")}";
                }

                if (projection.FocusAdded != 0)
                {
                    return projection.FocusAdded > 0
                        ? $"+{projection.FocusAdded} focus"
                        : $"{projection.FocusAdded} focus";
                }

                if (projection.FlowChange != 0)
                {
                    return projection.FlowChange > 0
                        ? $"+{projection.FlowChange} flow"
                        : $"{projection.FlowChange} flow";
                }

                if (!string.IsNullOrEmpty(projection.EffectDescription))
                {
                    // Use the special effect description from projection
                    // This handles Promising and other special types
                    return projection.EffectDescription.Replace(", +", " +").Replace("Promise made, ", "");
                }
            }

            // No effect
            return "No effect";
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

        protected ConnectionType GetExchangeTokenType(CardInstance card)
        {
            // For merchants, exchanges typically use Commerce tokens
            // Could be expanded based on card context or NPC type
            return ConnectionType.Commerce;
        }


        protected string GetConversationEndReason()
        {
            if (Session == null) return "Conversation ended";

            // Check various end conditions in priority order
            if (Session.LetterGenerated)
            {
                return string.Format("Letter obtained! Check your queue. (Flow: {0})", Session.FlowBattery);
            }

            if (Session.CurrentDoubt >= Session.MaxDoubt)
            {
                return string.Format("{0}'s doubt has been exhausted. They have no more time for you today.", NpcName);
            }

            if (Session.CurrentState == ConnectionState.DISCONNECTED && Session.FlowBattery <= -3)
            {
                return string.Format("{0} is too distressed to continue. The conversation has broken down.", NpcName);
            }


            if (!ConversationFacade.GetHandCards().Any() && Session.Deck.RemainingDrawCards == 0)
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
                return $"{locationName} <span class='icon-arrow-right'></span> {spotName} ({spotTraits})";
            }
            else
            {
                return $"{locationName} <span class='icon-arrow-right'></span> {spotName}";
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
                string description = property.ToString();
                propertyDescriptions.Add(description);
            }

            return string.Join(", ", propertyDescriptions);
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
                IReadOnlyList<CardInstance> handCards = ConversationFacade.GetHandCards();
                if (handCards != null)
                {
                    foreach (CardInstance c in handCards)
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
        /// PACKET 6: Get enhanced success effect description
        /// </summary>
        /// <summary>
        /// PROJECTION PRINCIPLE: Wrapper that delegates to projection-based GetSuccessEffect
        /// </summary>
        protected string GetSuccessEffectDescription(CardInstance card)
        {
            return GetSuccessEffect(card);
        }

        // PACKET 7: Action Preview System Implementation

        /// <summary>
        /// Get cards that will exhaust on SPEAK action (Impulse cards)
        /// </summary>
        protected List<CardInstance> GetImpulseCards()
        {
            IReadOnlyList<CardInstance> handCards = ConversationFacade.GetHandCards();
            if (handCards == null) return new List<CardInstance>();

            return handCards
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
            IReadOnlyList<CardInstance> handCards = ConversationFacade.GetHandCards();
            if (handCards == null) return new List<CardInstance>();

            return handCards
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
        /// Get CSS classes for a card based on its animation state
        /// </summary>
        protected string GetCardCssClasses(CardInstance card)
        {
            if (card == null) return "card";

            List<string> classes = new List<string> { "card" };
            string cardId = card.InstanceId ?? card.Id ?? "";

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

            // Add category class for left border styling
            if (card?.ConversationCardTemplate != null)
            {
                classes.Add(card.ConversationCardTemplate.Category.ToString().ToLower());
            }

            return string.Join(" ", classes);
        }

        /// <summary>
        /// Get all display cards for rendering with enhanced slot-aware presentation models.
        /// During animations, returns presentation models with animation coordination.
        /// </summary>
        protected List<CardDisplayInfo> GetAllDisplayCards()
        {
            // Static UI: Return simple list of current hand cards
            var handCards = ConversationFacade.GetHandCards()?.ToList() ?? new List<CardInstance>();
            var displayCards = new List<CardDisplayInfo>();

            foreach (var card in handCards)
            {
                displayCards.Add(new CardDisplayInfo(card));
            }

            Console.WriteLine($"[GetAllDisplayCards] Returning {displayCards.Count} static display cards");
            return displayCards;
        }

        /// <summary>
        /// Get CSS variables for the static card container.
        /// Static display only - no animations, no slot coordination.
        /// </summary>
        protected string GetContainerCSSVariables()
        {
            // Static container variables only
            return "--container-state: static;";
        }

        /// <summary>
        /// Get the position of a card in the current hand
        /// </summary>
        protected int GetCardPosition(CardInstance card)
        {
            if (card == null) return -1;

            IReadOnlyList<CardInstance> handCards = ConversationFacade.GetHandCards();
            if (handCards == null) return -1;

            for (int i = 0; i < handCards.Count; i++)
            {
                if (handCards[i].InstanceId == card.InstanceId)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Mark cards for exhaust animation - SIMPLIFIED: No longer needed
        /// </summary>
        protected void MarkCardsForExhaust(List<CardInstance> cardsToExhaust)
        {
            // Cards are removed immediately, no exhaust animation
        }

        private string FormatResourceList(List<ResourceAmount> resources)
        {
            if (resources == null || resources.Count == 0)
                return "nothing";

            return string.Join(", ", resources.Select(r => $"{r.Amount} {r.Type.ToString().ToLower()}"));
        }

        // ===== NEW UI UPDATE METHODS =====

        /// <summary>
        /// Returns short personality rule text for the NPC bar badge
        /// </summary>
        protected string GetPersonalityRuleShort()
        {
            if (Session?.NPC == null) return "";

            // Get the actual mechanical rules from the personality type
            PersonalityType personality = Session.NPC.PersonalityType;
            return personality switch
            {
                PersonalityType.DEVOTED => "Devoted: Momentum losses doubled",
                PersonalityType.MERCANTILE => "Mercantile: Highest focus +30% success",
                PersonalityType.PROUD => "Proud: Cards must ascend in focus",
                PersonalityType.CUNNING => "Cunning: Same focus as prev -2 Momentum",
                PersonalityType.STEADFAST => "Steadfast: Momentum changes capped ±2",
                _ => $"{personality}: Special rules apply"
            };
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
            List<RequestGoal> goals = new List<RequestGoal>();

            // Get request cards from the REQUEST PILE, not hand cards
            IReadOnlyList<CardInstance> requestCards = ConversationFacade.GetRequestCards();
            if (requestCards != null)
            {
                // Look for request/promise cards in the request pile
                IOrderedEnumerable<IGrouping<int, CardInstance>> requestCardsInPile = requestCards
                .Where(c => c.CardType == CardType.Letter || c.CardType == CardType.Promise || c.CardType == CardType.Letter)
                .Where(c => c.ConversationCardTemplate?.MomentumThreshold > 0)
                .GroupBy(c => c.ConversationCardTemplate.MomentumThreshold)
                .OrderBy(g => g.Key);

                foreach (IGrouping<int, CardInstance>? group in requestCardsInPile)
                {
                    int threshold = group.Key;
                    CardInstance firstCard = group.First();

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
                        _ => firstCard.ConversationCardTemplate?.Description ?? "Special Request"
                    };

                    goals.Add(new RequestGoal
                    {
                        Threshold = threshold,
                        Name = goalName,
                        Reward = reward
                    });
                }
            }

            // NO FALLBACKS - if no request cards, show no goals
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
        /// Returns CSS class for goal cards based on current Momentum vs threshold
        /// </summary>
        protected string GetGoalCardClass(int threshold)
        {
            if (Session?.MomentumManager == null) return "";

            int currentMomentum = Session.MomentumManager.CurrentMomentum;

            if (currentMomentum >= threshold)
                return "achievable";
            else if (currentMomentum >= threshold - 2) // Close to achievable
                return "active";
            else
                return "";
        }

        /// <summary>
        /// Returns CSS class name for card stat badges (lowercase stat name)
        /// </summary>
        protected string GetCardStatClass(CardInstance card)
        {
            if (card?.ConversationCardTemplate?.BoundStat == null) return "";

            return card.ConversationCardTemplate.BoundStat.Value.ToString().ToLower();
        }

        /// <summary>
        /// Returns the stat type as a lowercase string for applying stat-based color classes
        /// </summary>
        protected string GetCardStatColorClass(CardInstance card)
        {
            if (card?.ConversationCardTemplate?.BoundStat == null) return "";

            return card.ConversationCardTemplate.BoundStat.Value.ToString().ToLower();
        }

        /// <summary>
        /// Returns proper display name for card's bound stat
        /// </summary>
        protected string GetCardStatName(CardInstance card)
        {
            if (card?.ConversationCardTemplate?.BoundStat == null) return "";

            return GetStatDisplayName(card.ConversationCardTemplate.BoundStat.Value);
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
        /// Returns the effective level of a card based on player's stat level
        /// </summary>
        protected int GetCardLevel(CardInstance card)
        {
            if (card?.ConversationCardTemplate?.BoundStat == null || GameFacade == null) return 1;

            try
            {
                PlayerStats stats = GameFacade.GetPlayerStats();
                return stats.GetLevel(card.ConversationCardTemplate.BoundStat.Value);
            }
            catch
            {
                return 1;
            }
        }

        /// <summary>
        /// Returns the stat bonus text for Expression cards (e.g., "+2 momentum")
        /// </summary>
        protected string GetStatBonus(CardInstance card)
        {
            if (card?.ConversationCardTemplate?.BoundStat == null || GameFacade == null) return "";

            // Only Expression cards get momentum bonuses
            if (card.ConversationCardTemplate.Category != CardCategory.Expression) return "";

            try
            {
                PlayerStats stats = GameFacade.GetPlayerStats();
                int statLevel = stats.GetLevel(card.ConversationCardTemplate.BoundStat.Value);

                // Level 2 = +1, Level 3 = +2, Level 4 = +3, Level 5 = +4
                if (statLevel >= 2)
                {
                    int bonus = statLevel - 1;
                    return $"+{bonus} momentum";
                }
            }
            catch
            {
                // Fallback to no bonus display
            }

            return "";
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

        /// <summary>
        /// Returns display-friendly name for card category
        /// </summary>
        protected string GetCategoryDisplayName(CardCategory category)
        {
            return category switch
            {
                CardCategory.Expression => "Expression",
                CardCategory.Realization => "Realization",
                CardCategory.Regulation => "Regulation",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Returns CSS class for card category styling
        /// </summary>
        protected string GetCategoryClass(ConversationCard card)
        {
            if (card == null) return "category-unknown";

            return card.Category switch
            {
                CardCategory.Expression => "category-expression",
                CardCategory.Realization => "category-realization",
                CardCategory.Regulation => "category-regulation",
                _ => "category-unknown"
            };
        }

        /// <summary>
        /// Get momentum calculation display for a card
        /// </summary>
        protected string GetMomentumCalculation(CardInstance card)
        {
            if (card?.ConversationCardTemplate == null) return "";

            // Only show for Expression cards
            if (card.ConversationCardTemplate.Category != CardCategory.Expression)
            {
                return "";
            }

            // Get the player stats for bonus calculation
            PlayerStats playerStats = null;
            if (GameFacade != null)
            {
                try
                {
                    playerStats = GameFacade.GetPlayerStats();
                }
                catch { }
            }

            int baseMomentum = card.ConversationCardTemplate.GetMomentumEffect(Session, playerStats);
            return $"Gain {baseMomentum} momentum";
        }

        /// <summary>
        /// Get card effect description for the new system
        /// </summary>
        protected string GetCardEffectDescription(CardInstance card)
        {
            if (card?.ConversationCardTemplate == null) return "";

            return GetSuccessEffectDescription(card);
        }

        /// <summary>
        /// Get preview text for LISTEN button
        /// </summary>
        protected string GetListenPreview()
        {
            if (Session == null) return "";

            List<string> preview = new List<string>();

            // Show doubt increase
            int baseDoubtIncrease = 3; // From desperate plea
            int unspentFocusPenalty = Session.GetAvailableFocus();
            int totalDoubt = Session.CurrentDoubt + baseDoubtIncrease + unspentFocusPenalty;

            preview.Add($"Doubt <span class='icon-arrow-right'></span> {totalDoubt} (+{baseDoubtIncrease} base{(unspentFocusPenalty > 0 ? $" +{unspentFocusPenalty} unspent" : "")})");;

            // Show momentum erosion (Devoted doubles losses)
            int erosion = totalDoubt;
            if (Context?.Npc?.PersonalityType == PersonalityType.DEVOTED)
            {
                erosion *= 2;
                int newMomentum = Math.Max(0, Session.CurrentMomentum - erosion);
                preview.Add($"Momentum {Session.CurrentMomentum} <span class='icon-arrow-right'></span> {newMomentum} (Devoted 2x!)");
            }
            else
            {
                int newMomentum = Math.Max(0, Session.CurrentMomentum - erosion);
                preview.Add($"Momentum {Session.CurrentMomentum} <span class='icon-arrow-right'></span> {newMomentum}");
            }

            // Show draw with impulse penalties
            int impulseCount = Session.Deck.HandCards.Count(c => c.Persistence == PersistenceType.Impulse);
            int drawCount = Session.GetDrawCount() - impulseCount;
            if (impulseCount > 0)
            {
                preview.Add($"Draw {drawCount} cards ({Session.GetDrawCount()} - {impulseCount} Impulses)");
            }
            else
            {
                preview.Add($"Draw {Session.GetDrawCount()} cards");
            }

            return string.Join("<br/>", preview);
        }

        /// <summary>
        /// Get preview text for SPEAK button
        /// </summary>
        protected string GetSpeakPreview()
        {
            if (Session == null) return "";

            if (SelectedCard == null)
            {
                return $"Select a card to play ({Session.GetAvailableFocus()} focus available)";
            }

            int focusCost = SelectedCard.GetEffectiveFocus(Session.CurrentState);
            return $"Play card (Cost: {focusCost} focus)";
        }

    }
}