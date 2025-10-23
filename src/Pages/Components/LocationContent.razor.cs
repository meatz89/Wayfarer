using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Wayfarer.Pages.Components
{
    /// <summary>
    /// DUMB DISPLAY COMPONENT - NO BUSINESS LOGIC
    /// All filtering/querying/view model building happens in LocationFacade
    /// This component ONLY displays pre-built view models and delegates actions to backend
    /// </summary>
    public class LocationContentBase : ComponentBase
    {
        [Inject] protected GameFacade GameFacade { get; set; }
        [Inject] protected GameWorld GameWorld { get; set; }

        [Parameter] public EventCallback OnActionExecuted { get; set; }

        [CascadingParameter] protected GameScreenBase GameScreen { get; set; }

        // VISUAL NOVEL NAVIGATION STATE
        protected LocationViewState ViewState { get; set; } = LocationViewState.Landing;
        protected Stack<LocationViewState> NavigationStack { get; set; } = new();
        protected Goal SelectedGoal { get; set; }

        // VIEW MODEL STORAGE - all pre-built by facade
        protected LocationContentViewModel ViewModel { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            ResetNavigation();
            await RefreshLocationData();
        }

        protected override async Task OnParametersSetAsync()
        {
            ResetNavigation();
            await RefreshLocationData();
        }

        /// <summary>
        /// Refresh location data - ONE facade call, that's it
        /// </summary>
        private async Task RefreshLocationData()
        {
            // Evaluate obligation discovery
            GameFacade.EvaluateObligationDiscovery();

            // ONE call to backend - receives ALL data pre-built
            ViewModel = GameFacade.GetLocationFacade().GetLocationContentViewModel();

            await Task.CompletedTask;
        }

        // ============================================
        // ACTION DELEGATION - Intent-based execution
        // Backend authority: UI creates intent, backend determines effects
        // UI interprets result without making decisions
        // ============================================

        protected async Task ExecuteLocationAction(LocationActionViewModel action)
        {
            PlayerIntent intent = null;

            // Parse enum and create strongly-typed intent
            if (Enum.TryParse<PlayerActionType>(action.ActionType, true, out PlayerActionType playerActionType))
            {
                intent = playerActionType switch
                {
                    PlayerActionType.CheckBelongings => new CheckBelongingsIntent(),
                    PlayerActionType.Wait => new WaitIntent(),
                    PlayerActionType.SleepOutside => new SleepOutsideIntent(),
                    _ => null
                };
            }
            else if (Enum.TryParse<LocationActionType>(action.ActionType, true, out LocationActionType locationActionType))
            {
                intent = locationActionType switch
                {
                    LocationActionType.Rest => new RestAtLocationIntent(),
                    LocationActionType.SecureRoom => new SecureRoomIntent(),
                    LocationActionType.Work => new WorkIntent(),
                    LocationActionType.Investigate => new InvestigateLocationIntent(),
                    LocationActionType.Travel => new OpenTravelScreenIntent(),
                    _ => null
                };
            }

            if (intent == null)
            {
                throw new InvalidOperationException($"No intent mapping for action type: {action.ActionType}");
            }

            // Execute via intent system - backend decides everything
            IntentResult result = await GameFacade.ProcessIntent(intent);

            // Interpret result without making decisions - just follow backend instructions
            if (result.Success)
            {
                // Screen-level navigation (GameScreen handles)
                if (result.NavigateToScreen.HasValue && GameScreen != null)
                {
                    await GameScreen.NavigateToScreen(result.NavigateToScreen.Value);
                }

                // View-level navigation (LocationContent handles)
                if (result.NavigateToView.HasValue)
                {
                    NavigateToView(result.NavigateToView.Value);
                }

                // Refresh if backend says to refresh
                if (result.RequiresLocationRefresh)
                {
                    await RefreshLocationData();
                    await OnActionExecuted.InvokeAsync();
                }
            }
        }

        protected async Task HandleCommitToGoal(Goal goal)
        {
            if (goal.SystemType == TacticalSystemType.Social)
            {
                if (GameScreen != null)
                {
                    await GameScreen.StartConversationSession(goal.PlacementNpcId, goal.Id);
                }
            }
            else if (goal.SystemType == TacticalSystemType.Mental)
            {
                if (GameScreen == null)
                    throw new InvalidOperationException("GameScreen not available");

                Player player = GameWorld.GetPlayer();
                if (player.CurrentLocation == null)
                    throw new InvalidOperationException("Player has no current location");

                await GameScreen.StartMentalSession(goal.DeckId, player.CurrentLocation.Id, goal.Id, goal.ObligationId);
            }
            else if (goal.SystemType == TacticalSystemType.Physical)
            {
                if (GameScreen == null)
                    throw new InvalidOperationException("GameScreen not available");

                Player player = GameWorld.GetPlayer();
                if (player.CurrentLocation == null)
                    throw new InvalidOperationException("Player has no current location");

                await GameScreen.StartPhysicalSession(goal.DeckId, player.CurrentLocation.Id, goal.Id, goal.ObligationId);
            }
        }

        protected async Task MoveToSpot(string spotId)
        {
            bool success = GameFacade.MoveToSpot(spotId);

            if (success)
            {
                ResetNavigation();
                await RefreshLocationData();
                await OnActionExecuted.InvokeAsync();
            }
        }

        protected async Task HandleInventoryChanged()
        {
            await RefreshLocationData();
            await OnActionExecuted.InvokeAsync();
            StateHasChanged();
        }

        // ============================================
        // VISUAL NOVEL NAVIGATION
        // ============================================

        protected void NavigateToView(LocationViewState newView, object context = null)
        {
            NavigationStack.Push(ViewState);
            ViewState = newView;

            if (newView == LocationViewState.GoalDetail && context is Goal goal)
            {
                SelectedGoal = goal;
            }

            StateHasChanged();
        }

        protected void NavigateBack()
        {
            if (NavigationStack.Count > 0)
            {
                LocationViewState previousView = NavigationStack.Pop();
                ViewState = previousView;

                if (ViewState != LocationViewState.GoalDetail)
                {
                    SelectedGoal = null;
                }

                StateHasChanged();
            }
            else
            {
                ResetNavigation();
            }
        }

        protected void ResetNavigation()
        {
            ViewState = LocationViewState.Landing;
            NavigationStack.Clear();
            SelectedGoal = null;
            StateHasChanged();
        }

        // ============================================
        // EVENT HANDLERS - simple wrappers
        // ============================================

        protected void HandleNavigateToView(LocationViewState newView)
        {
            NavigateToView(newView);
        }

        protected async Task HandleExecuteLocationAction(LocationActionViewModel action)
        {
            await ExecuteLocationAction(action);
        }

        protected void HandleNavigateToGoal(string goalId)
        {
            Goal goal = GameWorld.Goals.FirstOrDefault(g => g.Id == goalId);
            if (goal != null)
            {
                NavigateToView(LocationViewState.GoalDetail, goal);
            }
        }

        protected async Task HandleMoveToSpot(string spotId)
        {
            await MoveToSpot(spotId);
        }

        protected async Task HandleStartExchange(string npcId)
        {
            if (GameScreen != null)
            {
                await GameScreen.StartExchange(npcId);
            }
        }

        // ============================================
        // VIEWMODEL PREPARATION - trivial wrappers
        // ============================================

        protected GoalDetailViewModel GetGoalDetailViewModel()
        {
            if (SelectedGoal == null) return null;

            // Find the goal in view model to get pre-calculated difficulty
            // Search in Social goals (ambient + obstacles)
            GoalCardViewModel goalCard = ViewModel.NPCsWithGoals
                .SelectMany(npc => npc.AmbientSocialGoals)
                .FirstOrDefault(g => g.Id == SelectedGoal.Id);

            if (goalCard == null)
            {
                goalCard = ViewModel.NPCsWithGoals
                    .SelectMany(npc => npc.SocialObstacles)
                    .SelectMany(obstacle => obstacle.Goals)
                    .FirstOrDefault(g => g.Id == SelectedGoal.Id);
            }

            // Search in Mental goals (ambient + obstacles)
            if (goalCard == null)
            {
                goalCard = ViewModel.AmbientMentalGoals.FirstOrDefault(g => g.Id == SelectedGoal.Id);
            }

            if (goalCard == null)
            {
                goalCard = ViewModel.MentalObstacles
                    .SelectMany(obstacle => obstacle.Goals)
                    .FirstOrDefault(g => g.Id == SelectedGoal.Id);
            }

            // Search in Physical goals (ambient + obstacles)
            if (goalCard == null)
            {
                goalCard = ViewModel.AmbientPhysicalGoals.FirstOrDefault(g => g.Id == SelectedGoal.Id);
            }

            if (goalCard == null)
            {
                goalCard = ViewModel.PhysicalObstacles
                    .SelectMany(obstacle => obstacle.Goals)
                    .FirstOrDefault(g => g.Id == SelectedGoal.Id);
            }

            int difficulty = goalCard?.Difficulty ?? 0;

            return new GoalDetailViewModel
            {
                Goal = SelectedGoal,
                Name = SelectedGoal.Name,
                Description = SelectedGoal.Description,
                SystemType = SelectedGoal.SystemType,
                SystemTypeLowercase = SelectedGoal.SystemType.ToString().ToLower(),
                Difficulty = difficulty.ToString(),
                HasCosts = SelectedGoal.Costs.Focus > 0 || SelectedGoal.Costs.Stamina > 0,
                FocusCost = SelectedGoal.Costs.Focus,
                StaminaCost = SelectedGoal.Costs.Stamina
            };
        }
    }
}
