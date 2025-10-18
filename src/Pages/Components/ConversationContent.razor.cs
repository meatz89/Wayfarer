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
        [Parameter] public SocialChallengeContext Context { get; set; }
        [Parameter] public EventCallback OnConversationEnd { get; set; }
        [CascadingParameter] public GameScreenBase GameScreen { get; set; }

        [Inject] protected SocialFacade ConversationFacade { get; set; }
        [Inject] protected GameFacade GameFacade { get; set; }
        [Inject] protected GameWorld GameWorld { get; set; }
        [Inject] protected ItemRepository ItemRepository { get; set; }
        [Inject] protected SocialNarrativeService NarrativeService { get; set; }
        [Inject] protected DifficultyCalculationService DifficultyService { get; set; }

        /// <summary>
        /// PROJECTION PRINCIPLE: The CategoricalEffectResolver is a pure projection function
        /// that returns what WOULD happen without modifying state. Both UI (for preview)
        /// and game logic (for execution) call the resolver to get projections.
        /// This ensures the UI can accurately display what effects WILL occur.
        /// </summary>
        [Inject] protected SocialEffectResolver EffectResolver { get; set; }

        protected SocialSession Session { get; set; }
        protected CardInstance? SelectedCard { get; set; }
        protected int TotalSelectedInitiative => GetCardInitiativeCost(SelectedCard);
        protected bool IsConversationEnded { get; set; } = false;
        protected string EndReason { get; set; }

        // Action preview state
        protected bool ShowSpeakPreview { get; set; } = false;
        protected bool ShowListenPreview { get; set; } = false;

        // Static UI system - no animation, no state management

        // Static system - no animations
        // Track which request cards have already been moved from RequestPile to ActiveCards
        protected List<string> MovedGoalCardIds { get; set; } = new List<string>();

        protected string NpcName { get; set; }
        protected string LastNarrative { get; set; }
        protected string LastDialogue { get; set; }
        protected NarrativeProviderType LastProviderSource { get; set; } = NarrativeProviderType.JsonFallback; // Default to fallback

        // AI narrative generation state
        protected bool IsGeneratingNarrative { get; set; } = false;
        protected List<CardNarrative> CurrentCardNarratives { get; set; } = new List<CardNarrative>();
        protected NarrativeOutput CurrentNarrativeOutput { get; set; }
        private Task<NarrativeOutput> _initialNarrativeTask;

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
                if (Context.Npc == null)
                    throw new InvalidOperationException("Context.Npc cannot be null");
                NpcName = Context.Npc.Name;

                // Initialize conversation narrative
                await GenerateInitialNarrative();
            }
            else
            { }
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
                SocialTurnResult listenResult = await GameFacade.ExecuteListen();

                if (listenResult == null)
                {
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
                SocialTurnResult turnResult = await GameFacade.PlayConversationCard(SelectedCard);

                if (turnResult?.CardPlayResult == null)
                {
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
                    IReadOnlyList<CardInstance> handCards = ConversationFacade.GetHandCards();
                    if (handCards == null)
                        throw new InvalidOperationException("Facade returned null hand cards");
                    List<CardInstance> activeCards = handCards.ToList();

                    // Start AI generation
                    IsGeneratingNarrative = true;
                    StateHasChanged(); // Update UI to show loading state// Request phase 1 narrative (NPC dialogue only) for faster initial UI update
                    NarrativeOutput narrative = await NarrativeService.GenerateOnlyNPCDialogueAsync(
                        Session,
                        Context.Npc,
                        activeCards); if (narrative != null && !string.IsNullOrWhiteSpace(narrative.NPCDialogue))
                    {
                        ApplyNarrativeOutput(narrative);
                        // REMOVED: StateHasChanged() - prevents card DOM recreation
                        return narrative;
                    }
                }
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
            if (narrative == null) return; CurrentNarrativeOutput = narrative;

            // Update NPC dialogue and narrative
            if (!string.IsNullOrWhiteSpace(narrative.NPCDialogue))
            {
                LastDialogue = narrative.NPCDialogue;
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
                CurrentCardNarratives.AddRange(narrative.CardNarratives);
                foreach (CardNarrative cardNarrative in narrative.CardNarratives)
                {
                    if (!string.IsNullOrWhiteSpace(cardNarrative.NarrativeText))
                    { }
                }
            }
            else
            {// NOTE: Card narrative generation now handled synchronously in ExecuteListen
            }
        }

        private async Task GenerateCardNarrativesAsync(string npcDialogue)
        {
            if (string.IsNullOrWhiteSpace(npcDialogue) || Session == null || Context?.Npc == null)
                return;

            // Get the active cards for current state
            IReadOnlyList<CardInstance> handCards = ConversationFacade.GetHandCards();
            if (handCards == null)
                throw new InvalidOperationException("Facade returned null hand cards");
            List<CardInstance> activeCards = handCards.ToList();
            if (!activeCards.Any())
                return;

            // Call phase 2 to generate card narratives based on NPC dialogue
            List<CardNarrative> cardNarratives = await NarrativeService.GenerateOnlyCardNarrativesAsync(
                Session,
                Context.Npc,
                activeCards,
                npcDialogue);

            if (cardNarratives != null && cardNarratives.Any())
            {// Apply the card narratives to the UI
                CurrentCardNarratives.Clear();
                CurrentCardNarratives.AddRange(cardNarratives);

                foreach (CardNarrative cardNarrative in cardNarratives)
                {
                    if (!string.IsNullOrWhiteSpace(cardNarrative.NarrativeText))
                    { }
                }

                // REMOVED: StateHasChanged() - cards not in DOM yet during narrative generation
            }
            else
            { }
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

        protected string GetProperCardName(CardInstance card)
        {
            // Use the card's title - all cards must have titles from JSON
            return card.SocialCardTemplate.Title;
        }

        protected List<string> GetNpcStatusParts()
        {
            if (Context?.Npc == null) return new List<string>();
            NPC npc = Context.Npc;

            List<string> status = new List<string>();

            // NPC has Profession as an enum - default is 0
            if ((int)npc.Profession != 0)
                status.Add(npc.Profession.ToString());

            // Get current Venue name
            if (GameFacade == null) return status;
            Venue currentLocation = GameFacade.GetCurrentLocation();
            if (currentLocation != null && !string.IsNullOrEmpty(currentLocation.Name))
                status.Add(currentLocation.Name);

            return status;
        }

        private string GetInitialDialogue()
        {
            return "Hello, what brings you here?";
        }

        protected async Task EndConversation()
        {
            // Player clicked "End Conversation" button
            await OnConversationEnd.InvokeAsync();
        }

        /// <summary>
        /// Get the Venue context data for display in the Venue bar
        /// </summary>
        protected (string locationName, string spotName, string spotTraits) GetLocationContextParts()
        {
            if (GameFacade == null)
                throw new InvalidOperationException("GameFacade is null");

            Venue currentLocation = GameFacade.GetCurrentLocation();
            Location currentSpot = GameFacade.GetCurrentLocationSpot();

            if (currentLocation == null || currentSpot == null)
                throw new InvalidOperationException("Current location or spot is null");

            string locationName = currentLocation.Name;
            string spotName = currentSpot.Name;
            string spotTraits = GetSpotTraits(currentSpot);

            return (locationName, spotName, spotTraits);
        }

        /// <summary>
        /// Get location properties formatted for display
        /// </summary>
        private string GetSpotTraits(Location location)
        {
            if (location?.LocationProperties == null || !location.LocationProperties.Any())
                return "";

            List<string> propertyDescriptions = new List<string>();

            foreach (LocationPropertyType property in location.LocationProperties)
            {
                // Convert property enum to user-friendly description
                string description = property.ToString();
                propertyDescriptions.Add(description);
            }

            return string.Join(", ", propertyDescriptions);
        }

        // =============================================
        // GOAL CARD DETECTION & FILTERING
        // =============================================

        /// <summary>
        /// Detect if a card is a goal card (self-contained victory condition)
        /// Goal cards have Context.threshold but NO SocialCardTemplate
        /// </summary>
        protected bool IsGoalCard(CardInstance card)
        {
            if (card == null) return false;

            // Goal cards have threshold in Context and no system-specific template
            return card.Context?.threshold > 0 && card.SocialCardTemplate == null;
        }

        /// <summary>
        /// Get all goal cards currently in hand (unlocked at Momentum thresholds)
        /// </summary>
        protected List<CardInstance> GetAvailableGoalCards()
        {
            IReadOnlyList<CardInstance> handCards = ConversationFacade.GetHandCards();
            if (handCards == null)
                throw new InvalidOperationException("Facade returned null hand cards");
            return handCards.Where(c => IsGoalCard(c)).ToList();
        }

        protected List<CardDisplayInfo> GetAllDisplayCards()
        {
            // Static UI: Return simple list of current hand cards
            IReadOnlyList<CardInstance> handCards = ConversationFacade.GetHandCards();
            if (handCards == null)
                throw new InvalidOperationException("Facade returned null hand cards");

            // FILTER OUT GOAL CARDS - they render separately
            List<CardInstance> regularCards = handCards.Where(c => !IsGoalCard(c)).ToList();

            List<CardDisplayInfo> displayCards = new List<CardDisplayInfo>();
            foreach (CardInstance? card in regularCards)
            {
                displayCards.Add(new CardDisplayInfo(card));
            }
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
            if (card?.SocialCardTemplate == null) return "";
            if (Session == null) return "";

            // Use effect resolver to get projection with effect description
            CardEffectResult projection = EffectResolver.ProcessSuccessEffect(card, Session);
            return projection.EffectOnlyDescription;
        }

        // ===== NEW 4-RESOURCE SYSTEM METHODS =====

        /// <summary>
        /// Get current Initiative from session (starts at 0, built through cards)
        /// </summary>
        protected int GetCurrentInitiative()
        {
            if (Session == null)
                throw new InvalidOperationException("Session is null");
            return Session.CurrentInitiative;
        }

        /// <summary>
        /// Get maximum Initiative for current conversation type
        /// </summary>
        protected int GetMaxInitiative()
        {
            if (Session == null)
                throw new InvalidOperationException("Session is null");
            return Session.MaxInitiative;
        }

        /// <summary>
        /// Get current Cadence (-5 to +5 range)
        /// </summary>
        protected int GetCurrentCadence()
        {
            if (Session == null)
                throw new InvalidOperationException("Session is null");
            return Session.Cadence;
        }

        /// <summary>
        /// Get current Momentum for goal tracking
        /// </summary>
        protected int GetCurrentMomentum()
        {
            if (Session == null)
                throw new InvalidOperationException("Session is null");
            return Session.CurrentMomentum;
        }

        /// <summary>
        /// Get Initiative cost for a card (replaces Focus cost)
        /// </summary>
        protected int GetCardInitiativeCost(CardInstance card)
        {
            if (card?.SocialCardTemplate == null) return 0;

            // FIXED: Always use InitiativeCost from template (0 is a valid cost for Foundation cards!)
            return card.SocialCardTemplate.InitiativeCost;
        }

        // ===== NEW MOCKUP-SPECIFIC HELPER METHODS =====

        /// <summary>
        /// Get current Understanding from session
        /// </summary>
        protected int GetCurrentUnderstanding()
        {
            if (Session == null)
                throw new InvalidOperationException("Session is null");
            return Session.CurrentUnderstanding;
        }

        /// <summary>
        /// Get count of cards in Deck pile
        /// </summary>
        protected int GetDeckCount()
        {
            if (Session == null)
                throw new InvalidOperationException("Session is null");
            if (Session.Deck == null)
                throw new InvalidOperationException("Session.Deck is null");
            return Session.Deck.RemainingDeckCards;
        }

        /// <summary>
        /// Get count of cards in Spoken pile
        /// </summary>
        protected int GetSpokenCount()
        {
            if (Session == null)
                throw new InvalidOperationException("Session is null");
            if (Session.Deck == null)
                throw new InvalidOperationException("Session.Deck is null");
            return Session.Deck.SpokenPileCount;
        }

        /// <summary>
        /// Get count of cards in Mind pile
        /// </summary>
        protected int GetMindCount()
        {
            if (Session == null)
                throw new InvalidOperationException("Session is null");
            if (Session.Deck == null)
                throw new InvalidOperationException("Session.Deck is null");
            return Session.Deck.HandSize;
        }

        /// <summary>
        /// Calculate difficulty for a goal card using DifficultyCalculationService
        /// Returns calculated difficulty based on player's current modifiers (Understanding, tokens, familiarity)
        /// </summary>
        protected int GetGoalDifficulty(CardInstance goalCard)
        {
            if (goalCard?.GoalCardTemplate == null) return 0;
            if (DifficultyService == null || ItemRepository == null) return goalCard.GoalCardTemplate.threshold;

            // Find parent Goal from GameWorld by searching for GoalCard ID
            Goal parentGoal = FindParentGoal(goalCard.GoalCardTemplate.Id);
            if (parentGoal == null) return goalCard.GoalCardTemplate.threshold;

            // Calculate actual difficulty using DifficultyCalculationService with all modifiers
            DifficultyResult result = DifficultyService.CalculateDifficulty(parentGoal, ItemRepository);
            return result.FinalDifficulty;
        }

        private Goal FindParentGoal(string goalCardId)
        {
            if (GameWorld?.Goals == null) return null;

            foreach (Goal goal in GameWorld.Goals)
            {
                if (goal.GoalCards != null && goal.GoalCards.Any(gc => gc.Id == goalCardId))
                {
                    return goal;
                }
            }
            return null;
        }

        /// <summary>
        /// Get card persistence type for display (Statement/Echo only)
        /// </summary>
        protected string GetCardPersistenceType(CardInstance card)
        {
            return card?.SocialCardTemplate?.Persistence == PersistenceType.Echo ? "Echo" : "Statement";
        }

        /// <summary>
        /// Get Initiative generation amount for Foundation cards (Steamworld Quest pattern).
        /// This is derived categorically from the card's tier (depth), not stored as a property.
        /// </summary>
        protected int GetCardInitiativeGeneration(CardInstance card)
        {
            if (card?.SocialCardTemplate == null) return 0;
            return card.SocialCardTemplate.GetInitiativeGeneration();
        }

        /// <summary>
        /// Get tooltip explaining persistence type effects
        /// </summary>
        protected string GetPersistenceTooltip(CardInstance card)
        {
            if (card?.SocialCardTemplate?.Persistence == PersistenceType.Echo)
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
            if (card?.SocialCardTemplate?.Delivery == null) return "Standard";
            return card.SocialCardTemplate.Delivery.ToString();
        }

        /// <summary>
        /// Get card delivery CSS class for colored flag display
        /// </summary>
        protected string GetCardDeliveryClass(CardInstance card)
        {
            if (card?.SocialCardTemplate?.Delivery == null) return "delivery-standard";
            return $"delivery-{card.SocialCardTemplate.Delivery.ToString().ToLower()}";
        }

        /// <summary>
        /// Get card delivery effect text (Cadence change)
        /// </summary>
        protected string GetCardDeliveryCadenceEffect(CardInstance card)
        {
            if (card?.SocialCardTemplate?.Delivery == null) return "+1";

            return card.SocialCardTemplate.Delivery switch
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
            if (Session == null)
                throw new InvalidOperationException("Session is null");
            return Session.GetUnlockedMaxDepth();
        }

        protected bool IsTierUnlocked(int tier)
        {
            return Session?.UnlockedTiers?.Contains(tier) ?? (tier == 1);
        }

        protected SocialTier[] GetAllTiers()
        {
            return SocialTier.AllTiers;
        }

        protected int GetMaxUnderstanding()
        {
            // Get the maximum Understanding threshold from the highest tier
            return SocialTier.AllTiers.Max(t => t.UnderstandingThreshold);
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
            if (card?.SocialCardTemplate?.Move == null) return "Remark";
            return card.SocialCardTemplate.Move.ToString();
        }

        protected string GetCardMoveClass(CardInstance card)
        {
            if (card?.SocialCardTemplate?.Move == null) return "move-remark";
            return $"move-{card.SocialCardTemplate.Move.ToString().ToLower()}";
        }

        // =============================================
        // GOAL CARD PLAY
        // =============================================

        /// <summary>
        /// Play a goal card to complete the conversation
        /// Goal cards end the conversation immediately with success
        /// </summary>
        protected async Task PlayGoalCard(CardInstance goalCard)
        {
            if (goalCard == null || !IsGoalCard(goalCard)) return;
            if (IsProcessing || IsGeneratingNarrative) return;

            IsProcessing = true;
            StateHasChanged();

            try
            {
                // Goal cards use PlayConversationCard - SocialFacade handles goal card logic
                SocialTurnResult result = await GameFacade.PlayConversationCard(goalCard);

                if (result != null && result.Success)
                {
                    LastNarrative = result.Narrative != null ? result.Narrative.NarrativeText : "Request complete";
                    IsConversationEnded = true;
                    EndReason = "Request complete";

                    // Refresh resource display
                    if (GameScreen != null)
                    {
                        await GameScreen.RefreshResourceDisplay();
                    }
                }
            }
            finally
            {
                IsProcessing = false;
                StateHasChanged();
            }
        }
    }
}
