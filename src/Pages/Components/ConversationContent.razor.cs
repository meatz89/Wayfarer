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
        /// Processing flag for backend calls
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

        protected override async Task OnParametersSetAsync()
        {
            if (Context != null)
            {
                await InitializeFromContext();
                await base.OnParametersSetAsync();
            }
        }

        private async Task InitializeFromContext()
        {
            if (Context?.Session != null)
            {
                Session = Context.Session;
                NpcName = Context.Npc?.Name ?? "Unknown";

                // Initialize conversation narrative
                await GenerateInitialNarrative();

                Console.WriteLine($"[ConversationContent] Initialized with Session for NPC: {NpcName}");
            }
            else
            {
                Console.WriteLine("[ConversationContent] ERROR: Context or Session is null");
            }
        }

        protected async Task ExecuteListen()
        {
            // UI state management only - no game logic
            if (Session == null || IsProcessing) return;
            if (IsGeneratingNarrative) return;

            SelectedCard = null;

            try
            {
                IsProcessing = true;
                StateHasChanged(); // Update UI to disable buttons

                // Delegate to facade - all game logic handled there
                ConversationTurnResult listenResult = await GameFacade.ExecuteListen();

                if (listenResult == null)
                {
                    Console.WriteLine("[ExecuteListen] Action failed");
                    return;
                }

                // Apply narrative if received
                if (listenResult.Narrative != null)
                {
                    ApplyNarrativeOutput(listenResult.Narrative);

                    // Generate card narratives if needed
                    if (!string.IsNullOrWhiteSpace(listenResult.Narrative.NPCDialogue))
                    {
                        await GenerateCardNarrativesAsync(listenResult.Narrative.NPCDialogue);
                    }
                }
                else
                {
                    // Fallback narrative
                    GenerateListenNarrative();
                }

                // Check for request cards becoming available
                ConversationFacade.CheckAndMoveRequestCards();

                // Refresh resource display
                if (GameScreen != null)
                {
                    await GameScreen.RefreshResourceDisplay();
                }

                // Check conversation end state
                if (Session.ShouldEnd())
                {
                    IsConversationExhausted = true;
                    ExhaustionReason = "Conversation ended";
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
            // UI state management only - no game logic
            if (IsGeneratingNarrative || IsProcessing || Session == null || SelectedCard == null) return;

            CardInstance playedCard = SelectedCard;

            try
            {
                IsProcessing = true;
                StateHasChanged(); // Update UI to disable buttons

                // Delegate to facade - all game logic handled there
                ConversationTurnResult turnResult = await GameFacade.PlayConversationCard(SelectedCard);

                if (turnResult?.CardPlayResult == null)
                {
                    Console.WriteLine("[ExecuteSpeak] Action failed");
                    return;
                }

                // Apply narrative if received
                if (turnResult.Narrative != null)
                {
                    ApplyNarrativeOutput(turnResult.Narrative);
                }
                else
                {
                    // Fallback narrative
                    // Simple fallback narrative
                    LastNarrative = "Your words have an effect.";
                }

                // Check for conversation end (promise card success, etc.)
                bool wasSuccessful = turnResult.CardPlayResult.Results?.FirstOrDefault()?.Success ?? false;
                bool isPromiseCard = playedCard.CardType == CardType.Letter || playedCard.CardType == CardType.Promise;

                if (isPromiseCard && wasSuccessful)
                {
                    IsConversationExhausted = true;
                    ExhaustionReason = $"Success! {GetSuccessEffectDescription(playedCard)}";
                    LastNarrative = "Your words have the desired effect. The conversation concludes successfully.";
                    StateHasChanged();
                    return;
                }

                SelectedCard = null;

                // Refresh resource display
                if (GameScreen != null)
                {
                    await GameScreen.RefreshResourceDisplay();
                }

                // Check conversation end state
                if (Session.ShouldEnd())
                {
                    IsConversationExhausted = true;
                    ExhaustionReason = "Conversation ended";
                }
            }
            finally
            {
                IsProcessing = false;
                StateHasChanged();
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

        protected async Task ManuallyEndConversation()
        {
            // Player clicked "End Conversation" button
            await OnConversationEnd.InvokeAsync();
        }

        /// <summary>
        /// Process letter negotiations and add resulting obligations to the player's queue
        /// </summary>

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
        /// Get card effect description for the new system
        /// </summary>
        protected string GetCardEffectDescription(CardInstance card)
        {
            if (card?.ConversationCardTemplate == null) return "";

            return GetSuccessEffectDescription(card);
        }
    }
}