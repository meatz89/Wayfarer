using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wayfarer.Pages.Components
{
    public class MentalContentBase : ComponentBase
    {
        [Parameter] public MentalChallengeContext Context { get; set; }
        [Parameter] public EventCallback OnChallengeEnd { get; set; }
        [CascadingParameter] public GameScreenBase GameScreen { get; set; }

        [Inject] protected GameFacade GameFacade { get; set; }
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
        protected List<CardInstance> Hand => GameFacade?.IsMentalSessionActive() == true
            ? GameFacade.GetMentalFacade().GetHand()
            : new List<CardInstance>();
        protected CardInstance SelectedCard { get; set; }
        protected string LastNarrative { get; set; }
        protected bool IsProcessing { get; set; } = false;
        protected bool IsInvestigationEnded { get; set; } = false;
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
            return Hand?.Count ?? 0;
        }

        // =============================================
        // RESOURCE DISPLAY METHODS (Parallel to Social)
        // =============================================

        protected int GetCurrentProgress()
        {
            return Session?.CurrentProgress ?? 0;
        }

        protected int GetCurrentAttention()
        {
            return Session?.CurrentAttention ?? 0;
        }

        protected int GetMaxAttention()
        {
            return Session?.MaxAttention ?? 10;
        }

        protected int GetCurrentExposure()
        {
            return Session?.CurrentExposure ?? 0;
        }

        protected int GetMaxExposure()
        {
            return Session?.MaxExposure ?? 10;
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
            return GameFacade?.IsMentalSessionActive() == true
                ? GameFacade.GetMentalFacade().GetDeckCount()
                : 0;
        }

        protected int GetDiscardCount()
        {
            return GameFacade?.IsMentalSessionActive() == true
                ? GameFacade.GetMentalFacade().GetDiscardCount()
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
                MentalTurnResult result = await GameFacade.ExecuteObserve();

                if (result == null)
                {
                    Console.WriteLine("[MentalContent] ExecuteObserve failed - null result");
                    return;
                }

                if (result.Success)
                {
                    LastNarrative = result.Narrative;
                    SelectedCard = null;

                    // Refresh resource display after action
                    if (GameScreen != null)
                    {
                        await GameScreen.RefreshResourceDisplay();
                    }

                    // Check if investigation should end
                    if (Session != null && Session.ShouldEnd())
                    {
                        IsInvestigationEnded = true;
                        EndReason = Session.CurrentProgress >= Session.VictoryThreshold
                            ? "Investigation complete!"
                            : "Maximum exposure reached";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MentalContent] Error in ExecuteObserve: {ex.Message}");
                LastNarrative = $"Error: {ex.Message}";
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
                MentalTurnResult result = await GameFacade.ExecuteAct(SelectedCard);

                if (result == null)
                {
                    Console.WriteLine("[MentalContent] ExecuteAct failed - null result");
                    return;
                }

                if (result.Success)
                {
                    LastNarrative = result.Narrative;
                    SelectedCard = null;

                    // Refresh resource display after action
                    if (GameScreen != null)
                    {
                        await GameScreen.RefreshResourceDisplay();
                    }

                    // Check if investigation should end
                    if (Session != null && Session.ShouldEnd())
                    {
                        IsInvestigationEnded = true;
                        EndReason = Session.CurrentProgress >= Session.VictoryThreshold
                            ? "Investigation complete!"
                            : "Maximum exposure reached";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MentalContent] Error in ExecuteAct: {ex.Message}");
                LastNarrative = $"Error: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
                StateHasChanged();
            }
        }

        protected async Task EndInvestigation()
        {
            if (IsProcessing || Session == null)
                return;

            IsProcessing = true;
            StateHasChanged();

            try
            {
                MentalOutcome outcome = GameFacade.EndMentalSession();

                if (outcome != null)
                {
                    LastNarrative = outcome.Success
                        ? $"Investigation complete! Progress: {outcome.FinalProgress}, Exposure: {outcome.FinalExposure}"
                        : $"Investigation incomplete. Progress: {outcome.FinalProgress}, Exposure: {outcome.FinalExposure}";
                }

                await OnChallengeEnd.InvokeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MentalContent] Error in EndInvestigation: {ex.Message}");
                LastNarrative = $"Error: {ex.Message}";
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
            if (GameFacade == null) return "";

            Player player = GameFacade.GetPlayer();

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
            if (GameFacade == null) return "";

            Player player = GameFacade.GetPlayer();

            // Use Act as default action type for preview
            MentalCardEffectResult projection = EffectResolver.ProjectCardEffects(card, Session, player, MentalActionType.Act);
            return projection.EffectDescription ?? "";
        }

        /// <summary>
        /// PERFECT INFORMATION: Get detailed effect breakdown showing base + bonuses + total
        /// Returns structured breakdown for UI display with transparency
        /// </summary>
        protected MentalCardEffectResult GetEffectBreakdown(CardInstance card, MentalActionType actionType)
        {
            if (card?.MentalCardTemplate == null) return null;
            if (Session == null) return null;
            if (GameFacade == null) return null;

            Player player = GameFacade.GetPlayer();

            // Get full projection with bonus tracking
            MentalCardEffectResult projection = EffectResolver.ProjectCardEffects(card, Session, player, actionType);
            return projection;
        }

        // =============================================
        // TIER UNLOCK SYSTEM (Parallel to Social)
        // =============================================

        protected int GetCurrentUnderstanding()
        {
            return Session?.CurrentUnderstanding ?? 0;
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
            // MentalCategory: Investigation type
            return card?.MentalCardTemplate?.Category.ToString() ?? "Unknown";
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
            int attentionCost = card?.MentalCardTemplate?.AttentionCost ?? 0;
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
            return card?.MentalCardTemplate?.Name ?? "Unknown";
        }

        // =============================================
        // DISCOVERY TRACKING SYSTEM
        // =============================================

        protected Dictionary<DiscoveryType, List<string>> GetDiscoveries()
        {
            return Session?.Discoveries ?? new Dictionary<DiscoveryType, List<string>>();
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
            return Session?.CurrentPhaseIndex ?? 0;
        }

        protected int GetVictoryThreshold()
        {
            return Session?.VictoryThreshold ?? 20;
        }

        protected bool IsInvestigationComplete()
        {
            if (Session == null) return false;
            return Session.CurrentProgress >= Session.VictoryThreshold;
        }

        protected bool IsInvestigationFailed()
        {
            if (Session == null) return false;
            return Session.CurrentExposure >= Session.MaxExposure;
        }

        protected string GetInvestigationStatus()
        {
            if (IsInvestigationComplete()) return "Complete";
            if (IsInvestigationFailed()) return "Exposed";
            return "Active";
        }

        // =============================================
        // CATEGORY TRACKING SYSTEM
        // =============================================

        protected int GetCategoryCount(MentalCategory category)
        {
            return Session?.GetCategoryCount(category) ?? 0;
        }

        protected Dictionary<MentalCategory, int> GetAllCategoryCounts()
        {
            return Session?.CategoryCounts ?? new Dictionary<MentalCategory, int>();
        }

        // =============================================
        // TIME & RESOURCE TRACKING
        // =============================================

        protected int GetTimeSegmentsSpent()
        {
            return Session?.TimeSegmentsSpent ?? 0;
        }

        protected int GetDrawCount()
        {
            return Session?.GetDrawCount() ?? 3;
        }

        // =============================================
        // Venue CONTEXT (SHARED ACROSS ALL CHALLENGES)
        // =============================================

        protected (string locationName, string spotName, string spotTraits) GetLocationContextParts()
        {
            if (GameFacade == null) return ("Unknown Location", "", "");

            Venue currentLocation = GameFacade.GetCurrentLocation();
            Location currentSpot = GameFacade.GetCurrentLocationSpot();

            if (currentLocation == null || currentSpot == null)
                return ("Unknown Location", "", "");

            string locationName = currentLocation.Name ?? "Unknown";
            string spotName = currentSpot.Name ?? "Unknown";
            string spotTraits = GetSpotTraits(currentSpot);

            return (locationName, spotName, spotTraits);
        }

        private string GetSpotTraits(Location location)
        {
            if (location?.LocationProperties == null || !location.LocationProperties.Any())
                return "";

            List<string> propertyDescriptions = new List<string>();

            foreach (LocationPropertyType property in location.LocationProperties)
            {
                string description = property.ToString();
                propertyDescriptions.Add(description);
            }

            return string.Join(", ", propertyDescriptions);
        }

        // =============================================
        // MANUAL INVESTIGATION END
        // =============================================

        protected async Task ManuallyEndInvestigation()
        {
            if (IsProcessing) return;

            await OnChallengeEnd.InvokeAsync();
        }

        // =============================================
        // CARD COST & RESOURCE DISPLAY
        // =============================================

        protected int GetCardAttentionCost(CardInstance card)
        {
            return card?.MentalCardTemplate?.AttentionCost ?? 0;
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
        // GOAL CARD DETECTION & FILTERING
        // =============================================

        /// <summary>
        /// Detect if a card is a goal card (self-contained victory condition)
        /// Goal cards have Context.threshold but NO MentalCardTemplate
        /// </summary>
        protected bool IsGoalCard(CardInstance card)
        {
            if (card == null) return false;

            // Goal cards have threshold in Context and no system-specific template
            return card.Context?.threshold > 0 && card.MentalCardTemplate == null;
        }

        /// <summary>
        /// Get all goal cards currently in hand (unlocked at Progress thresholds)
        /// </summary>
        protected List<CardInstance> GetAvailableGoalCards()
        {
            return Hand?.Where(c => IsGoalCard(c)).ToList() ?? new List<CardInstance>();
        }

        // =============================================
        // CARD DISPLAY LIST
        // =============================================

        protected List<CardDisplayInfo> GetAllDisplayCards()
        {
            List<CardInstance> handCards = Hand ?? new List<CardInstance>();
            List<CardDisplayInfo> displayCards = new List<CardDisplayInfo>();

            // FILTER OUT GOAL CARDS - they render separately
            foreach (CardInstance card in handCards.Where(c => !IsGoalCard(c)))
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
        // INVESTIGATION STATUS DISPLAY
        // =============================================

        protected string GetProgressStatusText()
        {
            if (Session == null) return "No active investigation";

            int progress = Session.CurrentProgress;
            int threshold = Session.VictoryThreshold;
            int exposure = Session.CurrentExposure;
            int maxExposure = Session.MaxExposure;

            return $"Progress: {progress}/{threshold} | Exposure: {exposure}/{maxExposure}";
        }

        protected bool ShouldShowVictoryIndicator()
        {
            return IsInvestigationComplete();
        }

        protected bool ShouldShowFailureIndicator()
        {
            return IsInvestigationFailed();
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
        // GOAL CARD PLAY
        // =============================================

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

            foreach (Goal goal in GameWorld.Goals.Values)
            {
                if (goal.GoalCards != null && goal.GoalCards.Any(gc => gc.Id == goalCardId))
                {
                    return goal;
                }
            }
            return null;
        }

        /// <summary>
        /// Play a goal card to complete the investigation
        /// Goal cards end the session immediately with success
        /// </summary>
        protected async Task PlayGoalCard(CardInstance goalCard)
        {
            if (goalCard == null || !IsGoalCard(goalCard)) return;
            if (IsProcessing) return;

            IsProcessing = true;
            StateHasChanged();

            try
            {
                // Goal cards use ExecuteAct - MentalFacade handles goal card logic
                MentalTurnResult result = await GameFacade.ExecuteAct(goalCard);

                if (result != null && result.Success)
                {
                    LastNarrative = result.Narrative;
                    IsInvestigationEnded = true;
                    EndReason = "Investigation complete";

                    // Refresh resource display
                    if (GameScreen != null)
                    {
                        await GameScreen.RefreshResourceDisplay();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MentalContent] Error playing goal card: {ex.Message}");
                LastNarrative = $"Error: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
                StateHasChanged();
            }
        }

    }
}
