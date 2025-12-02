using Microsoft.AspNetCore.Components;

namespace Wayfarer.Pages.Components
{
    public class MentalContentBase : ComponentBase
    {
        [Parameter] public MentalChallengeContext Context { get; set; }
        [Parameter] public EventCallback OnChallengeEnd { get; set; }
        [CascadingParameter] public GameScreenBase GameScreen { get; set; }

        [Inject] protected GameOrchestrator GameOrchestrator { get; set; }
        [Inject] protected GameWorld GameWorld { get; set; }
        [Inject] protected ItemRepository ItemRepository { get; set; }
        [Inject] protected DifficultyCalculationService DifficultyService { get; set; }

        /// <summary>
        /// PROJECTION PRINCIPLE: The MentalEffectResolver is a pure projection function
        /// that returns what WOULD happen without modifying state. Both UI (for preview)
        /// and game logic (for execution) call the resolver to get projections.
        /// Parallel to CategoricalEffectResolver in Conversation system.
        /// </summary>
        [Inject] protected MentalEffectResolver EffectResolver { get; set; }

        protected MentalSession Session => Context?.Session;
        protected List<CardInstance> Hand => GameOrchestrator?.IsMentalSessionActive() == true
            ? GameOrchestrator.GetMentalFacade().GetHand()
            : new List<CardInstance>();
        protected CardInstance SelectedCard { get; set; }
        protected string LastNarrative { get; set; }
        protected bool IsProcessing { get; set; } = false;
        protected bool IsObligationEnded { get; set; } = false;
        protected string EndReason { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
        }

        protected int GetCurrentTier()
        {
            if (Session == null || Session.UnlockedTiers == null || !Session.UnlockedTiers.Any())
                return 1;

            return Session.UnlockedTiers.Max();
        }

        protected int GetHandCount()
        {
            if (Hand == null)
                throw new InvalidOperationException("Hand is null");
            return Hand.Count;
        }

        // =============================================
        // RESOURCE DISPLAY METHODS (Parallel to Social)
        // =============================================

        protected int GetCurrentProgress()
        {
            if (Session == null)
                throw new InvalidOperationException("Session is null");
            return Session.CurrentProgress;
        }

        protected int GetCurrentAttention()
        {
            if (Session == null)
                throw new InvalidOperationException("Session is null");
            return Session.CurrentAttention;
        }

        protected int GetMaxAttention()
        {
            if (Session == null)
                throw new InvalidOperationException("Session is null");
            return Session.MaxAttention;
        }

        protected int GetCurrentExposure()
        {
            if (Session == null)
                throw new InvalidOperationException("Session is null");
            return Session.CurrentExposure;
        }

        protected int GetMaxExposure()
        {
            if (Session == null)
                throw new InvalidOperationException("Session is null");
            return Session.MaxExposure;
        }

        protected int GetProgressPercentage()
        {
            if (Session == null || Session.VictoryThreshold <= 0) return 0;
            return (int)((Session.CurrentProgress / (double)Session.VictoryThreshold) * 100);
        }

        protected int GetExposurePercentage()
        {
            if (Session == null || Session.MaxExposure <= 0) return 0;
            return (int)((Session.CurrentExposure / (double)Session.MaxExposure) * 100);
        }

        protected int GetDeckCount()
        {
            return GameOrchestrator?.IsMentalSessionActive() == true
                ? GameOrchestrator.GetMentalFacade().GetDeckCount()
                : 0;
        }

        protected int GetDiscardCount()
        {
            return GameOrchestrator?.IsMentalSessionActive() == true
                ? GameOrchestrator.GetMentalFacade().GetDiscardCount()
                : 0;
        }

        protected void SelectCard(CardInstance card)
        {
            SelectedCard = (SelectedCard == card) ? null : card;
        }

        protected async Task ExecuteObserve()
        {
            if (Session == null || IsProcessing)
                return;

            IsProcessing = true;
            StateHasChanged();

            try
            {
                MentalTurnResult result = await GameOrchestrator.ExecuteObserve();

                if (result == null)
                {
                    return;
                }

                if (result.Success)
                {
                    LastNarrative = result.Narrative;
                    SelectedCard = null;

                    // Refresh resource display after action
                    await GameScreen.RefreshResourceDisplay();

                    // Check if obligation should end
                    if (Session != null && Session.ShouldEnd())
                    {
                        IsObligationEnded = true;
                        EndReason = Session.CurrentProgress >= Session.VictoryThreshold
                            ? "Obligation complete!"
                            : "Maximum exposure reached";
                    }
                }
            }
            finally
            {
                IsProcessing = false;
                StateHasChanged();
            }
        }

        protected async Task ExecuteAct()
        {
            if (SelectedCard == null || IsProcessing || Session == null)
                return;

            IsProcessing = true;
            StateHasChanged();

            try
            {
                MentalTurnResult result = await GameOrchestrator.ExecuteAct(SelectedCard);

                if (result == null)
                {
                    return;
                }

                if (result.Success)
                {
                    LastNarrative = result.Narrative;
                    SelectedCard = null;

                    // Refresh resource display after action
                    await GameScreen.RefreshResourceDisplay();

                    // Check if obligation should end
                    if (Session != null && Session.ShouldEnd())
                    {
                        IsObligationEnded = true;
                        EndReason = Session.CurrentProgress >= Session.VictoryThreshold
                            ? "Obligation complete!"
                            : "Maximum exposure reached";
                    }
                }
            }
            finally
            {
                IsProcessing = false;
                StateHasChanged();
            }
        }

        protected async Task EndObligation()
        {
            if (IsProcessing || Session == null)
                return;

            IsProcessing = true;
            StateHasChanged();

            try
            {
                MentalOutcome outcome = GameOrchestrator.EndMentalSession();

                if (outcome != null)
                {
                    LastNarrative = outcome.Success
                        ? $"Obligation complete! Progress: {outcome.FinalProgress}, Exposure: {outcome.FinalExposure}"
                        : $"Obligation incomplete. Progress: {outcome.FinalProgress}, Exposure: {outcome.FinalExposure}";
                }

                await OnChallengeEnd.InvokeAsync();
            }
            finally
            {
                IsProcessing = false;
                StateHasChanged();
            }
        }

        /// <summary>
        /// PROJECTION PRINCIPLE: Get card effect preview for UI display
        /// Uses Act as default action type for preview (positive action)
        /// Parallel to ConversationContent.GetCardEffect()
        /// </summary>
        protected string GetCardEffect(CardInstance card)
        {
            if (card?.MentalCardTemplate == null) return "";
            if (Session == null) return "";
            if (GameOrchestrator == null) return "";

            Player player = GameOrchestrator.GetPlayer();

            // PROJECTION: Get effect projection using Act as default action (positive action for preview)
            MentalCardEffectResult projection = EffectResolver.ProjectCardEffects(card, Session, player, MentalActionType.Act);

            return projection.EffectDescription;
        }

        /// <summary>
        /// PROJECTION PRINCIPLE: Get effect-only description (for card tooltips)
        /// Parallel to ConversationContent.GetCardEffectOnlyDescription()
        /// </summary>
        protected string GetCardEffectOnlyDescription(CardInstance card)
        {
            if (card?.MentalCardTemplate == null) return "";
            if (Session == null) return "";
            if (GameOrchestrator == null) return "";

            Player player = GameOrchestrator.GetPlayer();

            // Use Act as default action type for preview
            MentalCardEffectResult projection = EffectResolver.ProjectCardEffects(card, Session, player, MentalActionType.Act);
            return projection.EffectDescription;
        }

        /// <summary>
        /// PERFECT INFORMATION: Get detailed effect breakdown showing base + bonuses + total
        /// Returns structured breakdown for UI display with transparency
        /// </summary>
        protected MentalCardEffectResult GetEffectBreakdown(CardInstance card, MentalActionType actionType)
        {
            if (card?.MentalCardTemplate == null) return null;
            if (Session == null) return null;
            if (GameOrchestrator == null) return null;

            Player player = GameOrchestrator.GetPlayer();

            // Get full projection with bonus tracking
            MentalCardEffectResult projection = EffectResolver.ProjectCardEffects(card, Session, player, actionType);
            return projection;
        }

        // =============================================
        // TIER UNLOCK SYSTEM (Parallel to Social)
        // =============================================

        protected int GetCurrentUnderstanding()
        {
            if (Session == null)
                throw new InvalidOperationException("Session is null");
            return Session.CurrentUnderstanding;
        }

        protected int GetUnderstandingPercentage()
        {
            int max = GetMaxUnderstanding();
            if (max <= 0) return 0;
            return (int)((GetCurrentUnderstanding() / (double)max) * 100);
        }

        protected int GetUnlockedMaxDepth()
        {
            if (Session == null || Session.UnlockedTiers == null || !Session.UnlockedTiers.Any())
                return 1;

            return Session.UnlockedTiers.Max();
        }

        protected bool IsTierUnlocked(int tier)
        {
            if (Session == null || Session.UnlockedTiers == null) return tier == 1;
            return Session.UnlockedTiers.Contains(tier);
        }

        protected int GetTierUnlockThreshold(int tier)
        {
            return SocialSession.GetTierUnlockThreshold(tier);
        }

        protected MentalTier[] GetAllTiers()
        {
            return MentalTier.AllTiers;
        }

        protected int GetMaxUnderstanding()
        {
            return MentalTier.AllTiers.Max(t => t.UnderstandingThreshold);
        }

        // =============================================
        // CARD DISPLAY HELPERS (Parallel to Social)
        // =============================================

        protected string GetCardMethod(CardInstance card)
        {
            // Method represents how card affects ObserveActBalance
            if (card?.MentalCardTemplate == null) return "";

            return card.MentalCardTemplate.Method.ToString();
        }

        protected string GetCardMethodClass(CardInstance card)
        {
            string method = GetCardMethod(card);
            return $"card-method method-{method.ToLower()}";
        }

        protected string GetCardCategory(CardInstance card)
        {
            // MentalCategory: Obligation type
            if (card?.MentalCardTemplate == null)
                throw new InvalidOperationException("Card or template is null");
            return card.MentalCardTemplate.Category.ToString();
        }

        protected string GetCardCategoryClass(CardInstance card)
        {
            string category = GetCardCategory(card);
            return $"card-category category-{category.ToLower()}";
        }

        protected string GetCardPersistenceType(CardInstance card)
        {
            // Mental cards are all single-use techniques (no persistent tools yet)
            return "Technique";
        }

        protected string GetPersistenceTooltip(CardInstance card)
        {
            return "This technique returns to your deck after playing";
        }

        // =============================================
        // CARD VALIDATION & SELECTION
        // =============================================

        protected bool CanSelectCard(CardInstance card)
        {
            if (Session == null || card == null) return false;
            if (IsProcessing) return false;

            // Basic validation: check if player has enough Attention to play card
            if (card.MentalCardTemplate == null)
                throw new InvalidOperationException("Card template is null");
            int attentionCost = card.MentalCardTemplate.AttentionCost;
            return Session.CurrentAttention >= attentionCost;
        }

        protected string GetCardCssClasses(CardInstance card)
        {
            if (card == null) return "card";

            List<string> classes = new List<string> { "card" };

            if (SelectedCard?.InstanceId == card.InstanceId)
            {
                classes.Add("selected");
            }

            return string.Join(" ", classes);
        }

        protected string GetCardName(CardInstance card)
        {
            if (card?.MentalCardTemplate == null)
                throw new InvalidOperationException("Card or template is null");
            return card.MentalCardTemplate.Name;
        }

        // =============================================
        // DISCOVERY TRACKING SYSTEM
        // =============================================

        protected Dictionary<DiscoveryType, List<string>> GetDiscoveries()
        {
            if (Session == null)
                throw new InvalidOperationException("Session is null");
            if (Session.Discoveries == null)
                throw new InvalidOperationException("Session.Discoveries is null");
            return Session.Discoveries;
        }

        protected List<string> GetDiscoveriesOfType(DiscoveryType type)
        {
            if (Session?.Discoveries == null) return new List<string>();
            return Session.Discoveries.TryGetValue(type, out List<string> discoveries)
                ? discoveries
                : new List<string>();
        }

        protected int GetTotalDiscoveryCount()
        {
            if (Session?.Discoveries == null) return 0;
            return Session.Discoveries.Values.Sum(list => list.Count);
        }

        protected bool HasDiscoveries()
        {
            return GetTotalDiscoveryCount() > 0;
        }

        // =============================================
        // PHASE & PROGRESSION DISPLAY
        // =============================================

        protected int GetCurrentPhase()
        {
            if (Session == null)
                throw new InvalidOperationException("Session is null");
            return Session.CurrentPhaseIndex;
        }

        protected int GetVictoryThreshold()
        {
            if (Session == null)
                throw new InvalidOperationException("Session is null");
            return Session.VictoryThreshold;
        }

        protected bool IsObligationComplete()
        {
            if (Session == null) return false;
            return Session.CurrentProgress >= Session.VictoryThreshold;
        }

        protected bool IsObligationFailed()
        {
            if (Session == null) return false;
            return Session.CurrentExposure >= Session.MaxExposure;
        }

        protected string GetObligationStatus()
        {
            if (IsObligationComplete()) return "Complete";
            if (IsObligationFailed()) return "Exposed";
            return "Active";
        }

        // =============================================
        // CATEGORY TRACKING SYSTEM
        // =============================================

        protected int GetCategoryCount(MentalCategory category)
        {
            if (Session == null)
                throw new InvalidOperationException("Session is null");
            return Session.GetCategoryCount(category);
        }

        protected Dictionary<MentalCategory, int> GetAllCategoryCounts()
        {
            if (Session == null)
                throw new InvalidOperationException("Session is null");
            if (Session.CategoryCounts == null)
                throw new InvalidOperationException("Session.CategoryCounts is null");
            return Session.CategoryCounts;
        }

        // =============================================
        // TIME & RESOURCE TRACKING
        // =============================================

        protected int GetTimeSegmentsSpent()
        {
            if (Session == null)
                throw new InvalidOperationException("Session is null");
            return Session.TimeSegmentsSpent;
        }

        protected int GetDrawCount()
        {
            if (Session == null)
                throw new InvalidOperationException("Session is null");
            return Session.GetDrawCount();
        }

        // =============================================
        // Venue CONTEXT (SHARED ACROSS ALL CHALLENGES)
        // =============================================

        protected (string locationName, string spotName, string spotTraits) GetLocationContextParts()
        {
            if (GameOrchestrator == null)
                throw new InvalidOperationException("GameOrchestrator is null");

            Venue currentVenue = GameOrchestrator.GetCurrentLocation().Venue;
            Location currentSpot = GameOrchestrator.GetCurrentLocation();

            if (currentVenue == null || currentSpot == null)
                throw new InvalidOperationException("Current location or spot is null");

            string locationName = currentVenue.Name;
            string spotName = currentSpot.Name;
            string spotTraits = GetSpotTraits(currentSpot);

            return (locationName, spotName, spotTraits);
        }

        private string GetSpotTraits(Location location)
        {
            if (location == null)
                return "";

            List<string> propertyDescriptions = new List<string>();

            // Extract orthogonal categorical properties
            if (location.Environment != default) propertyDescriptions.Add(location.Environment.ToString());
            if (location.Setting != default) propertyDescriptions.Add(location.Setting.ToString());
            if (location.Role != default) propertyDescriptions.Add(location.Role.ToString());
            if (location.Purpose != default) propertyDescriptions.Add(location.Purpose.ToString());
            if (location.Privacy != default) propertyDescriptions.Add(location.Privacy.ToString());
            if (location.Safety != default) propertyDescriptions.Add(location.Safety.ToString());
            if (location.Activity != default) propertyDescriptions.Add(location.Activity.ToString());

            return string.Join(", ", propertyDescriptions);
        }

        // =============================================
        // MANUAL OBLIGATION END
        // =============================================

        protected async Task ManuallyEndObligation()
        {
            if (IsProcessing) return;

            await OnChallengeEnd.InvokeAsync();
        }

        // =============================================
        // CARD COST & RESOURCE DISPLAY
        // =============================================

        protected int GetCardAttentionCost(CardInstance card)
        {
            if (card?.MentalCardTemplate == null) return 0;
            return card.MentalCardTemplate.AttentionCost;
        }

        protected int GetCardAttentionGeneration(CardInstance card)
        {
            // Foundation cards (Depth 1-2) generate +1 Attention
            if (card?.MentalCardTemplate == null) return 0;
            return card.MentalCardTemplate.Depth <= 2 ? 1 : 0;
        }

        protected bool IsFoundationCard(CardInstance card)
        {
            return card?.MentalCardTemplate?.Depth <= 2;
        }

        // =============================================
        // SITUATION CARD DETECTION & FILTERING
        // =============================================

        /// <summary>
        /// Detect if a card is a situation card (self-contained victory condition)
        /// Situation cards have Context.threshold but NO MentalCardTemplate
        /// </summary>
        protected bool IsSituationCard(CardInstance card)
        {
            if (card == null) return false;

            // Situation cards have threshold in Context and no system-specific template
            return card.Context?.threshold > 0 && card.MentalCardTemplate == null;
        }

        /// <summary>
        /// Get all situation cards currently in hand (unlocked at Progress thresholds)
        /// </summary>
        protected List<CardInstance> GetAvailableSituationCards()
        {
            if (Hand == null)
                throw new InvalidOperationException("Hand is null");
            return Hand.Where(c => IsSituationCard(c)).ToList();
        }

        // =============================================
        // CARD DISPLAY LIST
        // =============================================

        protected List<CardDisplayInfo> GetAllDisplayCards()
        {
            List<CardInstance> handCards = Hand ?? new List<CardInstance>();
            List<CardDisplayInfo> displayCards = new List<CardDisplayInfo>();

            // FILTER OUT SITUATION CARDS - they render separately
            foreach (CardInstance card in handCards.Where(c => !IsSituationCard(c)))
            {
                displayCards.Add(new CardDisplayInfo(card));
            }

            return displayCards;
        }

        protected int GetCardPosition(CardInstance card)
        {
            if (card == null || Hand == null) return -1;

            for (int i = 0; i < Hand.Count; i++)
            {
                if (Hand[i].InstanceId == card.InstanceId)
                {
                    return i;
                }
            }
            return -1;
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

        // =============================================
        // OBLIGATION STATUS DISPLAY
        // =============================================

        protected string GetProgressStatusText()
        {
            if (Session == null) return "No active obligation";

            int progress = Session.CurrentProgress;
            int threshold = Session.VictoryThreshold;
            int exposure = Session.CurrentExposure;
            int maxExposure = Session.MaxExposure;

            return $"Progress: {progress}/{threshold} | Exposure: {exposure}/{maxExposure}";
        }

        protected bool ShouldShowVictoryIndicator()
        {
            return IsObligationComplete();
        }

        protected bool ShouldShowFailureIndicator()
        {
            return IsObligationFailed();
        }

        // =============================================
        // ATTENTION ECONOMY DISPLAY
        // =============================================

        protected string GetAttentionStatusClass()
        {
            int attention = GetCurrentAttention();
            int max = GetMaxAttention();

            double percentage = max > 0 ? (attention / (double)max) * 100 : 0;

            if (percentage >= 80) return "attention-high";
            if (percentage >= 40) return "attention-medium";
            return "attention-low";
        }

        protected bool CanAffordCard(CardInstance card)
        {
            int cost = GetCardAttentionCost(card);
            int current = GetCurrentAttention();
            return current >= cost;
        }

        // =============================================
        // SITUATION CARD PLAY
        // =============================================

        /// <summary>
        /// Calculate difficulty for a situation card using DifficultyCalculationService
        /// Returns calculated difficulty based on player's current modifiers (Understanding, tokens, familiarity)
        /// </summary>
        protected int GetSituationDifficulty(CardInstance situationCard)
        {
            if (situationCard?.SituationCardTemplate == null) return 0;
            if (DifficultyService == null || ItemRepository == null) return situationCard.SituationCardTemplate.threshold;

            // Find parent Situation from GameWorld by searching for SituationCardTemplate object
            Situation parentSituation = FindParentSituation(situationCard.SituationCardTemplate);
            if (parentSituation == null) return situationCard.SituationCardTemplate.threshold;

            // Get base difficulty from deck
            int baseDifficulty = GetBaseDifficultyForSituation(parentSituation);

            // Calculate actual difficulty using DifficultyCalculationService with all modifiers
            DifficultyResult result = DifficultyService.CalculateDifficulty(parentSituation, baseDifficulty, ItemRepository);
            return result.FinalDifficulty;
        }

        private int GetBaseDifficultyForSituation(Situation situation)
        {
            if (GameWorld == null) return 10;

            MentalChallengeDeck deck = situation.Deck as MentalChallengeDeck;
            return deck?.DangerThreshold ?? 10;
        }

        private Situation FindParentSituation(SituationCard situationCard)
        {
            if (GameWorld?.Scenes == null) return null;
            if (situationCard == null) return null;

            foreach (Situation situation in GameWorld.Scenes.SelectMany(s => s.Situations))
            {
                if (situation.SituationCards != null && situation.SituationCards.Any(gc => gc == situationCard))
                {
                    return situation;
                }
            }
            return null;
        }

        /// <summary>
        /// Play a situation card to complete the obligation
        /// Situation cards end the session immediately with success
        /// </summary>
        protected async Task PlaySituationCard(CardInstance situationCard)
        {
            if (situationCard == null || !IsSituationCard(situationCard)) return;
            if (IsProcessing) return;

            IsProcessing = true;
            StateHasChanged();

            try
            {
                // Situation cards use ExecuteAct - MentalFacade handles situation card logic
                MentalTurnResult result = await GameOrchestrator.ExecuteAct(situationCard);

                if (result != null && result.Success)
                {
                    LastNarrative = result.Narrative;
                    IsObligationEnded = true;
                    EndReason = "Obligation complete";

                    // Refresh resource display
                    await GameScreen.RefreshResourceDisplay();
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
