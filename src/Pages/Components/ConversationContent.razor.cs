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
        protected int TotalSelectedInitiative => GetCardInitiativeCost(SelectedCard);
        protected bool IsConversationEnded { get; set; } = false;
        protected string EndReason { get; set; } = "";

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
            // Request cards don't use narrative styling (no TEMPLATE badge needed)
            if (card?.ConversationCardTemplate?.CardType == CardType.Request)
                return "";

            // Check if this specific card has AI-generated narrative
            if (card != null && CurrentCardNarratives != null)
            {
                CardNarrative cardNarrative = CurrentCardNarratives.FirstOrDefault(cn => cn.CardId == card.ConversationCardTemplate.Id);
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

                // Request card activation already handled in backend ConversationFacade.ExecuteListen()
                // No need to check again here - CheckGoalCardActivation uses correctly reduced momentum

                // Refresh resource display
                if (GameScreen != null)
                {
                    await GameScreen.RefreshResourceDisplay();
                }

                // Check conversation end state
                if (Session.ShouldEnd())
                {
                    IsConversationEnded = true;
                    EndReason = "Conversation ended";
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

                // Check if backend signals conversation should end (request cards, etc.)
                if (turnResult.EndsConversation)
                {
                    IsConversationEnded = true;
                    EndReason = "Request completed";
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
                    IsConversationEnded = true;
                    EndReason = "Conversation ended";
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

            // Cards not selectable when conversation has ended
            if (IsConversationEnded) return false;

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
            // Use the card's title - all cards must have titles from JSON
            return card.ConversationCardTemplate.Title;
        }

        protected string GetDoubtSlotClass(int slotNumber)
        {
            int currentDoubt = Session?.CurrentDoubt ?? 0;
            return slotNumber <= currentDoubt ? "filled" : "empty";
        }

        protected List<string> GetNpcStatusParts()
        {
            NPC? npc = Context?.Npc;
            if (npc == null) return new List<string>();

            List<string> status = new List<string>();

            // NPC has Profession as an enum - default is 0
            if ((int)npc.Profession != 0)
                status.Add(npc.Profession.ToString());

            // Get current location name
            Location? currentLocation = GameFacade?.GetCurrentLocation();
            if (!string.IsNullOrEmpty(currentLocation?.Name))
                status.Add(currentLocation.Name);

            return status;
        }

        protected string GetCardName(CardInstance card)
        {
            // Use the card's title - all cards must have titles from JSON
            return card.ConversationCardTemplate.Title;
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

            // PROJECTION PRINCIPLE: ALWAYS use resolver for ALL effects (no SuccessType check)
            CardEffectResult projection = EffectResolver.ProcessSuccessEffect(card, Session);

            // Use comprehensive effect description
            return projection.EffectDescription.Replace(", +", " +").Replace("Promise made, ", "");
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
        /// Get the location context data for display in the location bar
        /// </summary>
        protected (string locationName, string spotName, string spotTraits) GetLocationContextParts()
        {
            if (GameFacade == null) return ("Unknown Location", "", "");

            Location currentLocation = GameFacade.GetCurrentLocation();
            LocationSpot currentSpot = GameFacade.GetCurrentLocationSpot();

            if (currentLocation == null || currentSpot == null)
                return ("Unknown Location", "", "");

            string locationName = currentLocation.Name ?? "Unknown";
            string spotName = currentSpot.Name ?? "Unknown";
            string spotTraits = GetSpotTraits(currentSpot);

            return (locationName, spotName, spotTraits);
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

            // No Impulse cards in current system
            return new List<CardInstance>();
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
                .Where(c => (c.ConversationCardTemplate.CardType == CardType.Request || c.ConversationCardTemplate.CardType == CardType.Promise)) // Request cards have Opening property
                .ToList();
        }

        /// <summary>
        /// Get critical exhausts (request cards) from a list of cards
        /// </summary>
        protected List<CardInstance> GetCriticalExhausts(List<CardInstance> cards)
        {
            return cards.Where(c => (c.ConversationCardTemplate.CardType == CardType.Request || c.ConversationCardTemplate.CardType == CardType.Promise)).ToList();
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
            string cardId = card.InstanceId ?? card.ConversationCardTemplate.Id ?? "";


            // Add selected state
            if (SelectedCard?.InstanceId == cardId)
            {
                classes.Add("selected");
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

            // CRITICAL: Request cards MUST appear first (user requirement)
            var sortedCards = handCards
                .OrderBy(card =>
                {
                    CardType type = card.ConversationCardTemplate.CardType;
                    // Request cards (Letter/Promise/Burden) get priority 0, others get priority 1
                    return (type == CardType.Request || type == CardType.Promise || type == CardType.Burden) ? 0 : 1;
                })
                .ToList();

            var displayCards = new List<CardDisplayInfo>();
            foreach (var card in sortedCards)
            {
                displayCards.Add(new CardDisplayInfo(card));
            }

            Console.WriteLine($"[GetAllDisplayCards] Returning {displayCards.Count} static display cards (request cards first)");
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

        // NOTE: Card display methods (GetCardStatClass, GetCardStatName, GetCardDepth,
        // GetTraitClass, GetTraitDisplayName, GetTraitTooltip,
        // HasCardRequirement, GetCardRequirement, HasCardCost, GetCardCost)
        // are now in TacticalCard.razor.cs shared component

        /// <summary>
        /// Get card effect description (effect formula only, excludes Initiative generation property)
        /// </summary>
        protected string GetCardEffectDescription(CardInstance card)
        {
            if (card?.ConversationCardTemplate == null) return "";

            // Placeholder until effect resolver is implemented
            return "Effect description placeholder";
        }

        // ===== NEW 4-RESOURCE SYSTEM METHODS =====

        /// <summary>
        /// Get current Initiative from session (starts at 0, built through cards)
        /// </summary>
        protected int GetCurrentInitiative()
        {
            return Session?.CurrentInitiative ?? 0;
        }

        /// <summary>
        /// Get maximum Initiative for current conversation type
        /// </summary>
        protected int GetMaxInitiative()
        {
            return Session?.MaxInitiative ?? 10;
        }

        /// <summary>
        /// Get current Cadence (-5 to +5 range)
        /// </summary>
        protected int GetCurrentCadence()
        {
            return Session?.Cadence ?? 0;
        }

        /// <summary>
        /// Get current Momentum for goal tracking
        /// </summary>
        protected int GetCurrentMomentum()
        {
            return Session?.CurrentMomentum ?? 0;
        }

        /// <summary>
        /// Calculate Cadence meter position as percentage (0-100%)
        /// </summary>
        protected double GetCadencePosition()
        {
            int cadence = GetCurrentCadence();
            // Convert from -5 to +5 range to 0-100% position
            return ((cadence + 5) / 10.0) * 100;
        }

        /// <summary>
        /// Get Initiative cost for a card (replaces Focus cost)
        /// </summary>
        protected int GetCardInitiativeCost(CardInstance card)
        {
            if (card?.ConversationCardTemplate == null) return 0;

            // FIXED: Always use InitiativeCost from template (0 is a valid cost for Foundation cards!)
            return card.ConversationCardTemplate.InitiativeCost;
        }

        // ===== NEW MOCKUP-SPECIFIC HELPER METHODS =====

        /// <summary>
        /// Get momentum as percentage (0-100%) for resource bar display
        /// </summary>
        protected double GetMomentumPercentage()
        {
            int momentum = GetCurrentMomentum();
            // Assuming max momentum of 16 for percentage calculation
            return Math.Min(100, (momentum / 16.0) * 100);
        }

        /// <summary>
        /// Get current Understanding from session
        /// </summary>
        protected int GetCurrentUnderstanding()
        {
            return Session?.CurrentUnderstanding ?? 0;
        }

        /// <summary>
        /// Get Understanding as percentage (0-100%) for resource bar display
        /// Calculated based on the maximum tier unlock threshold
        /// </summary>
        protected double GetUnderstandingPercentage()
        {
            int understanding = GetCurrentUnderstanding();
            int maxUnderstanding = GetMaxUnderstanding();
            return Math.Min(100, (understanding / (double)maxUnderstanding) * 100);
        }

        /// <summary>
        /// Get CSS class for cadence scale segments matching mockup (-5 to +5 range)
        /// </summary>
        protected string GetCadenceSegmentClass(int segmentValue)
        {
            // Simple logic: <0 negative, =0 neutral, >0 positive
            if (segmentValue < 0) return "negative";
            if (segmentValue == 0) return "neutral";
            return "positive";
        }

        /// <summary>
        /// Get doubt as percentage (0-100%) for resource bar display
        /// </summary>
        protected double GetDoubtPercentage()
        {
            int doubt = Session?.CurrentDoubt ?? 0;
            int maxDoubt = Session?.MaxDoubt ?? 10;
            return Math.Min(100, (doubt / (double)maxDoubt) * 100);
        }

        /// <summary>
        /// Get count of cards in Deck pile
        /// </summary>
        protected int GetDeckCount()
        {
            return Session?.Deck?.RemainingDeckCards ?? 0;
        }

        /// <summary>
        /// Get count of cards in Spoken pile
        /// </summary>
        protected int GetSpokenCount()
        {
            return Session?.Deck?.SpokenPileCount ?? 0;
        }

        /// <summary>
        /// Get count of cards in Mind pile
        /// </summary>
        protected int GetMindCount()
        {
            return Session?.Deck?.HandSize ?? 0;
        }

        /// <summary>
        /// Get card persistence type for display (Statement/Echo only)
        /// </summary>
        protected string GetCardPersistenceType(CardInstance card)
        {
            return card?.ConversationCardTemplate?.Persistence == PersistenceType.Echo ? "Echo" : "Statement";
        }

        /// <summary>
        /// Get Initiative generation amount for Foundation cards (Steamworld Quest pattern).
        /// This is derived categorically from the card's tier (depth), not stored as a property.
        /// </summary>
        protected int GetCardInitiativeGeneration(CardInstance card)
        {
            return card?.ConversationCardTemplate?.GetInitiativeGeneration() ?? 0;
        }

        /// <summary>
        /// Get tooltip explaining persistence type effects
        /// </summary>
        protected string GetPersistenceTooltip(CardInstance card)
        {
            if (card?.ConversationCardTemplate?.Persistence == PersistenceType.Echo)
            {
                return "Echo: Repeatable card that doesn't count toward statement requirements";
            }
            else
            {
                return "Statement: Counts toward unlocking signature cards for this stat";
            }
        }


        // Delivery display methods
        /// <summary>
        /// Get card delivery type (how it affects Cadence on SPEAK)
        /// </summary>
        protected string GetCardDelivery(CardInstance card)
        {
            if (card?.ConversationCardTemplate?.Delivery == null) return "Standard";
            return card.ConversationCardTemplate.Delivery.ToString();
        }

        /// <summary>
        /// Get card delivery CSS class for colored flag display
        /// </summary>
        protected string GetCardDeliveryClass(CardInstance card)
        {
            if (card?.ConversationCardTemplate?.Delivery == null) return "delivery-standard";
            return $"delivery-{card.ConversationCardTemplate.Delivery.ToString().ToLower()}";
        }

        /// <summary>
        /// Get card delivery effect text (Cadence change)
        /// </summary>
        protected string GetCardDeliveryCadenceEffect(CardInstance card)
        {
            if (card?.ConversationCardTemplate?.Delivery == null) return "+1";

            return card.ConversationCardTemplate.Delivery switch
            {
                DeliveryType.Commanding => "+2",
                DeliveryType.Standard => "+1",
                DeliveryType.Measured => "+0",
                DeliveryType.Yielding => "-1",
                _ => "+1"
            };
        }

        // Tier unlock system methods
        protected int GetUnlockedMaxDepth()
        {
            return Session?.GetUnlockedMaxDepth() ?? 2; // Default to Tier 1 max
        }

        protected bool IsTierUnlocked(int tier)
        {
            return Session?.UnlockedTiers?.Contains(tier) ?? (tier == 1);
        }

        protected int GetTierUnlockThreshold(int tier)
        {
            return ConversationSession.GetTierUnlockThreshold(tier);
        }

        protected ConversationTier[] GetAllTiers()
        {
            return ConversationTier.AllTiers;
        }

        protected int GetMaxUnderstanding()
        {
            // Get the maximum Understanding threshold from the highest tier
            return ConversationTier.AllTiers.Max(t => t.UnderstandingThreshold);
        }

        // Statement count methods
        protected int GetStatementCount(string statName)
        {
            if (Session == null) return 0;

            if (Enum.TryParse<PlayerStatType>(statName, true, out PlayerStatType stat))
            {
                return Session.GetStatementCount(stat);
            }

            return 0;
        }


        // ConversationalMove display methods
        protected string GetCardMove(CardInstance card)
        {
            if (card?.ConversationCardTemplate?.Move == null) return "Remark";
            return card.ConversationCardTemplate.Move.ToString();
        }

        protected string GetCardMoveClass(CardInstance card)
        {
            if (card?.ConversationCardTemplate?.Move == null) return "move-remark";
            return $"move-{card.ConversationCardTemplate.Move.ToString().ToLower()}";
        }
    }
}