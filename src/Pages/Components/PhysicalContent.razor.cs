using Microsoft.AspNetCore.Components;

namespace Wayfarer.Pages.Components
{
    public class PhysicalContentBase : ComponentBase
    {
        [Parameter] public PhysicalChallengeContext Context { get; set; }
        [Parameter] public EventCallback OnChallengeEnd { get; set; }
        [CascadingParameter] public GameScreenBase GameScreen { get; set; }

        [Inject] protected GameOrchestrator GameOrchestrator { get; set; }
        [Inject] protected GameWorld GameWorld { get; set; }
        [Inject] protected ItemRepository ItemRepository { get; set; }
        [Inject] protected DifficultyCalculationService DifficultyService { get; set; }

        /// <summary>
        /// PROJECTION PRINCIPLE: The PhysicalEffectResolver is a pure projection function
        /// that returns what WOULD happen without modifying state. Both UI (for preview)
        /// and game logic (for execution) call the resolver to get projections.
        /// Parallel to CategoricalEffectResolver in Conversation system.
        /// </summary>
        [Inject] protected PhysicalEffectResolver EffectResolver { get; set; }

        protected PhysicalSession Session => Context?.Session;
        protected List<CardInstance> Hand => GameOrchestrator?.IsPhysicalSessionActive() == true
            ? GameOrchestrator.GetPhysicalFacade().GetHand()
            : new List<CardInstance>();
        protected CardInstance SelectedCard { get; set; }
        protected string LastNarrative { get; set; }
        protected bool IsProcessing { get; set; } = false;
        protected bool IsChallengeEnded { get; set; } = false;
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
        // RESOURCE DISPLAY METHODS (Parallel to Social/Mental)
        // =============================================

        protected int GetCurrentBreakthrough()
        {
            if (Session == null)
                throw new InvalidOperationException("Session is null");
            return Session.CurrentBreakthrough;
        }

        protected int GetCurrentExertion()
        {
            if (Session == null)
                throw new InvalidOperationException("Session is null");
            return Session.CurrentExertion;
        }

        protected int GetMaxExertion()
        {
            if (Session == null)
                throw new InvalidOperationException("Session is null");
            return Session.MaxExertion;
        }

        protected int GetCurrentDanger()
        {
            if (Session == null)
                throw new InvalidOperationException("Session is null");
            return Session.CurrentDanger;
        }

        protected int GetMaxDanger()
        {
            if (Session == null)
                throw new InvalidOperationException("Session is null");
            return Session.MaxDanger;
        }

        protected int GetCurrentAggression()
        {
            if (Session == null)
                throw new InvalidOperationException("Session is null");
            return Session.Aggression;
        }

        protected int GetBreakthroughPercentage()
        {
            if (Session == null) return 0;
            // Physical victory is determined by SituationCard play, not thresholds
            // This percentage is for UI display only (max progress ~20)
            int maxProgress = 20;
            return (int)((Session.CurrentBreakthrough / (double)maxProgress) * 100);
        }

        protected int GetDangerPercentage()
        {
            if (Session == null || Session.MaxDanger <= 0) return 0;
            return (int)((Session.CurrentDanger / (double)Session.MaxDanger) * 100);
        }

        protected int GetAggressionValue()
        {
            // Returns Aggression on -10 to +10 scale as 0-20 for UI display
            if (Session == null)
                throw new InvalidOperationException("Session is null");
            int aggression = Session.Aggression;
            return aggression + 10; // Convert -10..10 to 0..20
        }

        protected string GetAggressionClass()
        {
            if (Session == null)
                throw new InvalidOperationException("Session is null");
            int aggression = Session.Aggression;
            if (aggression < -5) return "overcautious";
            if (aggression > 5) return "reckless";
            return "balanced";
        }

        protected int GetDeckCount()
        {
            return GameOrchestrator?.IsPhysicalSessionActive() == true
                ? GameOrchestrator.GetPhysicalFacade().GetDeckCount()
                : 0;
        }

        protected int GetExhaustCount()
        {
            return GameOrchestrator?.IsPhysicalSessionActive() == true
                ? GameOrchestrator.GetPhysicalFacade().GetExhaustCount()
                : 0;
        }

        protected List<CardInstance> GetLockedCards()
        {
            return GameOrchestrator?.IsPhysicalSessionActive() == true
                ? GameOrchestrator.GetPhysicalFacade().GetLockedCards()
                : new List<CardInstance>();
        }

        protected void SelectCard(CardInstance card)
        {
            SelectedCard = (SelectedCard == card) ? null : card;
        }

        protected async Task ExecuteAssess()
        {
            if (Session == null || IsProcessing)
                return;

            IsProcessing = true;
            StateHasChanged();

            try
            {
                PhysicalTurnResult result = await GameOrchestrator.ExecuteAssess();

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

                    // Check if challenge should end
                    if (Session != null && Session.ShouldEnd())
                    {
                        IsChallengeEnded = true;
                        // Physical sessions only end on Danger threshold (failure)
                        // Victory is determined by SituationCard play in facade
                        EndReason = "Maximum danger reached";
                    }
                }
            }
            finally
            {
                IsProcessing = false;
                StateHasChanged();
            }
        }

        protected async Task ExecuteExecute()
        {
            if (SelectedCard == null || IsProcessing || Session == null)
                return;

            IsProcessing = true;
            StateHasChanged();

            try
            {
                PhysicalTurnResult result = await GameOrchestrator.ExecuteExecute(SelectedCard);

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

                    // Check if challenge should end
                    if (Session != null && Session.ShouldEnd())
                    {
                        IsChallengeEnded = true;
                        // Physical sessions only end on Danger threshold (failure)
                        // Victory is determined by SituationCard play in facade
                        EndReason = "Maximum danger reached";
                    }
                }
            }
            finally
            {
                IsProcessing = false;
                StateHasChanged();
            }
        }

        protected async Task EndChallenge()
        {
            if (IsProcessing || Session == null)
                return;

            IsProcessing = true;
            StateHasChanged();

            try
            {
                PhysicalOutcome outcome = await GameOrchestrator.EndPhysicalSession();

                if (outcome != null)
                {
                    LastNarrative = outcome.Success
                        ? $"Challenge complete! Progress: {outcome.FinalProgress}, Danger: {outcome.FinalDanger}"
                        : $"Challenge incomplete. Progress: {outcome.FinalProgress}, Danger: {outcome.FinalDanger}";
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
        /// Uses Execute as default action type for preview (positive action)
        /// Parallel to ConversationContent.GetCardEffect()
        /// </summary>
        protected string GetCardEffect(CardInstance card)
        {
            if (card?.PhysicalCardTemplate == null) return "";
            if (Session == null) return "";
            if (GameOrchestrator == null) return "";

            Player player = GameOrchestrator.GetPlayer();

            // PROJECTION: Get effect projection using Execute as default action (positive action for preview)
            PhysicalCardEffectResult projection = EffectResolver.ProjectCardEffects(card, Session, player, PhysicalActionType.Execute);

            return projection.EffectDescription;
        }

        /// <summary>
        /// PROJECTION PRINCIPLE: Get effect-only description (for card tooltips)
        /// Parallel to ConversationContent.GetCardEffectOnlyDescription()
        /// </summary>
        protected string GetCardEffectOnlyDescription(CardInstance card)
        {
            if (card?.PhysicalCardTemplate == null) return "";
            if (Session == null) return "";
            if (GameOrchestrator == null) return "";

            Player player = GameOrchestrator.GetPlayer();

            // Use Execute as default action type for preview
            PhysicalCardEffectResult projection = EffectResolver.ProjectCardEffects(card, Session, player, PhysicalActionType.Execute);
            return projection.EffectDescription;
        }

        /// <summary>
        /// PERFECT INFORMATION: Get detailed effect breakdown showing base + bonuses + total
        /// Returns structured breakdown for UI display with transparency
        /// </summary>
        protected PhysicalCardEffectResult GetEffectBreakdown(CardInstance card, PhysicalActionType actionType)
        {
            if (card?.PhysicalCardTemplate == null) return null;
            if (Session == null) return null;
            if (GameOrchestrator == null) return null;

            Player player = GameOrchestrator.GetPlayer();

            // Get full projection with bonus tracking
            PhysicalCardEffectResult projection = EffectResolver.ProjectCardEffects(card, Session, player, actionType);
            return projection;
        }

        // =============================================
        // TIER UNLOCK SYSTEM (Parallel to Social/Mental)
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
            return Session?.UnlockedTiers?.Contains(tier) ?? (tier == 1);
        }

        protected int GetTierUnlockThreshold(int tier)
        {
            // Simple tier threshold: 20 Understanding per tier (1=20, 2=40, 3=60, 4=80, 5=100)
            // Physical uses simplified tier system compared to Social's complex tier cards
            return tier * 20;
        }

        protected PhysicalTier[] GetAllTiers()
        {
            // Physical uses simplified tier system without tier card definitions
            // Tiers unlock based solely on Understanding thresholds
            return new PhysicalTier[0];
        }

        protected int GetMaxUnderstanding()
        {
            // Get the maximum Understanding threshold from the highest tier
            return PhysicalTier.AllTiers.Max(t => t.UnderstandingThreshold);
        }

        // =============================================
        // CARD DISPLAY HELPERS (Parallel to Social/Mental)
        // =============================================

        protected string GetCardApproach(CardInstance card)
        {
            // Approach represents how card affects AssessExecuteBalance
            // Reckless (extreme positive), Aggressive (positive), Standard (neutral), Methodical (negative)
            if (card?.PhysicalCardTemplate == null) return "";

            return card.PhysicalCardTemplate.Approach.ToString();
        }

        protected string GetCardApproachClass(CardInstance card)
        {
            string approach = GetCardApproach(card);
            return $"card-approach approach-{approach.ToLower()}";
        }

        protected string GetCardCategory(CardInstance card)
        {
            // PhysicalCategory represents the type of physical action
            if (card?.PhysicalCardTemplate == null)
                throw new InvalidOperationException("Card or template is null");
            return card.PhysicalCardTemplate.Category.ToString();
        }

        protected string GetCardCategoryClass(CardInstance card)
        {
            string category = GetCardCategory(card);
            return $"card-category category-{category.ToLower()}";
        }

        protected string GetCardPersistenceType(CardInstance card)
        {
            // Physical cards are all single-use techniques (no persistent tools yet)
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

            // Basic validation: check if player has enough Exertion to play card
            if (card.PhysicalCardTemplate == null)
                throw new InvalidOperationException("Card template is null");
            int exertionCost = card.PhysicalCardTemplate.ExertionCost;
            return Session.CurrentExertion >= exertionCost;
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
            if (card?.PhysicalCardTemplate == null)
                throw new InvalidOperationException("Card or template is null");
            return card.PhysicalCardTemplate.Name;
        }

        // =============================================
        // DISCOVERY TRACKING SYSTEM
        // =============================================

        protected List<string> GetDiscoveriesOfType(DiscoveryType type)
        {
            if (Session == null) return new List<string>();
            return Session.GetDiscoveriesForType(type);
        }

        protected int GetTotalDiscoveryCount()
        {
            if (Session == null) return 0;
            return Session.GetTotalDiscoveryCount();
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
            // Physical sessions don't use VictoryThreshold - victory is determined by SituationCard play
            // This is for UI display only
            return 20;
        }

        protected bool IsChallengeComplete()
        {
            // Physical sessions don't complete via Breakthrough threshold
            // Victory is determined by SituationCard play (handled in facade)
            return false;
        }

        protected bool IsChallengeFailed()
        {
            if (Session == null) return false;
            return Session.CurrentDanger >= Session.MaxDanger;
        }

        protected string GetChallengeStatus()
        {
            if (IsChallengeComplete()) return "Complete";
            if (IsChallengeFailed()) return "Failed";
            return "Active";
        }

        protected int GetBreakthroughThreshold()
        {
            // Physical sessions don't use VictoryThreshold - victory is determined by SituationCard play
            // This is for UI display only (typical max progress ~20)
            return 20;
        }

        // =============================================
        // AGGRESSION BALANCE STATE DISPLAY
        // =============================================

        protected string GetAggressionState()
        {
            if (Session == null) return "Balanced";

            if (Session.IsRecklessBalance()) return "Reckless";
            if (Session.IsOvercautiousBalance()) return "Overcautious";
            return "Balanced";
        }

        protected string GetAggressionStateClass()
        {
            if (Session == null) return "balanced";

            if (Session.IsRecklessBalance()) return "reckless";
            if (Session.IsOvercautiousBalance()) return "overcautious";
            return "balanced";
        }

        // =============================================
        // CATEGORY TRACKING SYSTEM
        // =============================================

        protected int GetCategoryCount(PhysicalCategory category)
        {
            if (Session == null)
                throw new InvalidOperationException("Session is null");
            return Session.GetCategoryCount(category);
        }

        protected int GetTotalCategoryPlayCount()
        {
            if (Session == null) return 0;
            return Session.AggressiveCount + Session.DefensiveCount +
                   Session.TacticalCount + Session.EvasiveCount + Session.EnduranceCount;
        }

        // =============================================
        // APPROACH HISTORY TRACKING
        // =============================================

        protected int GetApproachHistory()
        {
            if (Session == null)
                throw new InvalidOperationException("Session is null");
            return Session.ApproachHistory;
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
        // MANUAL CHALLENGE END
        // =============================================

        protected async Task ManuallyEndChallenge()
        {
            if (IsProcessing) return;

            await OnChallengeEnd.InvokeAsync();
        }

        // =============================================
        // CARD COST & RESOURCE DISPLAY
        // =============================================

        protected int GetCardExertionCost(CardInstance card)
        {
            if (card?.PhysicalCardTemplate == null) return 0;
            return card.PhysicalCardTemplate.ExertionCost;
        }

        protected int GetCardExertionGeneration(CardInstance card)
        {
            // Foundation cards (Depth 1-2) generate +1 Exertion
            if (card?.PhysicalCardTemplate == null) return 0;
            return card.PhysicalCardTemplate.GetExertionGeneration();
        }

        protected bool IsFoundationCard(CardInstance card)
        {
            return card?.PhysicalCardTemplate?.Depth <= 2;
        }

        // =============================================
        // SITUATION CARD DETECTION & FILTERING
        // =============================================

        /// <summary>
        /// Detect if a card is a situation card (self-contained victory condition)
        /// Situation cards have Context.threshold but NO PhysicalCardTemplate
        /// </summary>
        protected bool IsSituationCard(CardInstance card)
        {
            if (card == null) return false;

            // Situation cards have threshold in Context and no system-specific template
            return card.Context?.threshold > 0 && card.PhysicalCardTemplate == null;
        }

        /// <summary>
        /// Get all situation cards currently in hand (unlocked at Breakthrough thresholds)
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

        protected int GetCardExertion(CardInstance card)
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
        // CHALLENGE STATUS DISPLAY
        // =============================================

        protected string GetChallengeStatusText()
        {
            if (Session == null) return "No active challenge";

            int breakthrough = Session.CurrentBreakthrough;
            int danger = Session.CurrentDanger;
            int maxDanger = Session.MaxDanger;

            // Physical victory is determined by SituationCard play, not thresholds
            return $"Breakthrough: {breakthrough} | Danger: {danger}/{maxDanger}";
        }

        protected bool ShouldShowVictoryIndicator()
        {
            return IsChallengeComplete();
        }

        protected bool ShouldShowFailureIndicator()
        {
            return IsChallengeFailed();
        }

        // =============================================
        // EXERTION ECONOMY DISPLAY
        // =============================================

        protected string GetExertionStatusClass()
        {
            int exertion = GetCurrentExertion();
            int max = GetMaxExertion();

            double percentage = max > 0 ? (exertion / (double)max) * 100 : 0;

            if (percentage >= 80) return "exertion-high";
            if (percentage >= 40) return "exertion-medium";
            return "exertion-low";
        }

        protected bool CanAffordCard(CardInstance card)
        {
            int cost = GetCardExertionCost(card);
            int current = GetCurrentExertion();
            return current >= cost;
        }

        // =============================================
        // AGGRESSION SCALE DISPLAY
        // =============================================

        protected string GetAggressionSegmentClass(int segmentValue)
        {
            // Aggression scale: -10 (Overcautious) to +10 (Reckless)
            if (segmentValue < -5) return "overcautious";
            if (segmentValue > 5) return "reckless";
            return "balanced";
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

            PhysicalChallengeDeck deck = situation.Deck as PhysicalChallengeDeck;
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
        /// Play a situation card to complete the challenge
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
                // Situation cards use ExecuteExecute - PhysicalFacade handles situation card logic
                PhysicalTurnResult result = await GameOrchestrator.ExecuteExecute(situationCard);

                if (result != null && result.Success)
                {
                    LastNarrative = result.Narrative;
                    IsChallengeEnded = true;
                    EndReason = "Challenge complete";

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
