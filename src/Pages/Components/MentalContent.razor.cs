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
        protected string LastNarrative { get; set; } = "";
        protected bool IsProcessing { get; set; } = false;
        protected bool IsInvestigationEnded { get; set; } = false;
        protected string EndReason { get; set; } = "";

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

        protected int GetCurrentObserveActBalance()
        {
            return Session?.ObserveActBalance ?? 0;
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

        protected int GetBalancePosition()
        {
            // Returns position on -10 to +10 scale as 0-20 for UI display
            int balance = Session?.ObserveActBalance ?? 0;
            return balance + 10; // Convert -10..10 to 0..20
        }

        protected string GetBalanceClass()
        {
            int balance = Session?.ObserveActBalance ?? 0;
            if (balance < -5) return "overcautious";
            if (balance > 5) return "reckless";
            return "balanced";
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
            if (SelectedCard == null || IsProcessing || Session == null)
                return;

            IsProcessing = true;
            StateHasChanged();

            try
            {
                var result = await GameFacade.ExecuteObserve(SelectedCard);

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
                var result = await GameFacade.ExecuteAct(SelectedCard);

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
                var outcome = GameFacade.EndMentalSession();

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

        protected int GetMaxUnderstanding()
        {
            return 100; // Maximum Understanding value
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
            // Mental uses simplified tier system compared to Social's complex tier cards
            return tier * 20;
        }

        protected MentalTier[] GetAllTiers()
        {
            // Mental uses simplified tier system without tier card definitions
            // Tiers unlock based solely on Understanding thresholds
            return new MentalTier[0];
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
        // BALANCE STATE DISPLAY
        // =============================================

        protected string GetBalanceState()
        {
            if (Session == null) return "Balanced";

            if (Session.IsRecklessBalance()) return "Reckless";
            if (Session.IsOvercautiousBalance()) return "Overcautious";
            return "Balanced";
        }

        protected string GetBalanceStateClass()
        {
            if (Session == null) return "balanced";

            if (Session.IsRecklessBalance()) return "reckless";
            if (Session.IsOvercautiousBalance()) return "overcautious";
            return "balanced";
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
        // LOCATION CONTEXT (SHARED ACROSS ALL CHALLENGES)
        // =============================================

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

        private string GetSpotTraits(LocationSpot spot)
        {
            if (spot?.SpotProperties == null || !spot.SpotProperties.Any())
                return "";

            List<string> propertyDescriptions = new List<string>();

            foreach (SpotPropertyType property in spot.SpotProperties)
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
        // CARD DISPLAY LIST
        // =============================================

        protected List<CardDisplayInfo> GetAllDisplayCards()
        {
            List<CardInstance> handCards = Hand ?? new List<CardInstance>();
            List<CardDisplayInfo> displayCards = new List<CardDisplayInfo>();

            foreach (CardInstance card in handCards)
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

    }
}
