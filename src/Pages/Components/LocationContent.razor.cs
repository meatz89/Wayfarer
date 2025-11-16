using Microsoft.AspNetCore.Components;

/// <summary>
/// DUMB DISPLAY COMPONENT - NO BUSINESS LOGIC
/// All filtering/querying/view model building happens in LocationFacade
/// This component ONLY displays pre-built view models and delegates actions to backend
/// </summary>
public class LocationContentBase : ComponentBase
{
    [Inject] protected GameFacade GameFacade { get; set; }
    [Inject] protected GameWorld GameWorld { get; set; }
    [Inject] protected SceneFacade SceneFacade { get; set; }

    [Parameter] public EventCallback OnActionExecuted { get; set; }

    [CascadingParameter] protected GameScreenBase GameScreen { get; set; }

    // VISUAL NOVEL NAVIGATION STATE
    protected LocationViewState ViewState { get; set; } = LocationViewState.Landing;
    protected Stack<LocationViewState> NavigationStack { get; set; } = new();
    protected Situation SelectedSituation { get; set; }

    // VIEW MODEL STORAGE - all pre-built by facade
    protected LocationContentViewModel ViewModel { get; set; } = new();

    // SCREEN EXPANSION - Conversation trees and observation scenes
    protected List<ConversationTree> AvailableConversationTrees { get; set; } = new();
    protected List<ObservationScene> AvailableObservationScenes { get; set; } = new();

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
        await GameFacade.EvaluateObligationDiscovery();

        // ONE call to backend - receives ALL data pre-built
        ViewModel = GameFacade.GetLocationFacade().GetLocationContentViewModel();

        // Load available conversation trees and observation scenes for current location
        Location currentLocation = GameWorld.GetPlayerCurrentLocation();
        string locationId = currentLocation?.Id;
        if (locationId != null)
        {
            AvailableConversationTrees = GameFacade.GetAvailableConversationTreesAtLocation(locationId);
            AvailableObservationScenes = GameFacade.GetAvailableObservationScenesAtLocation(locationId);

            // MULTI-SITUATION SCENE RESUMPTION: Check if player navigated to location required by waiting scene
            // Scene completed Situation 1 with ExitToWorld routing (different context required)
            // Player navigated to required location - auto-resume scene to continue progression
            List<Scene> resumableScenes = SceneFacade.GetResumableScenesAtContext(locationId, null);
            if (resumableScenes.Count > 0)
            {
                // Auto-resume first waiting scene (should only be one per location context)
                Scene resumableScene = resumableScenes[0];
                await GameScreen.StartScene(resumableScene);
            }
        }

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
                PlayerActionType.LookAround => new LookAroundIntent(),
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
                LocationActionType.IntraVenueMove => CreateIntraVenueMoveIntent(action),
                LocationActionType.ViewJobBoard => new ViewJobBoardIntent(),
                LocationActionType.CompleteDelivery => new CompleteDeliveryIntent(),
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
            if (result.NavigateToScreen.HasValue)
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

                // MODAL SCENE FORCING: Check if action triggered a forced modal scene
                // GameFacade sets PendingForcedSceneId after successful movement actions
                // If found, navigate to forced scene immediately (Sir Brante forced moment pattern)
                if (!string.IsNullOrEmpty(GameWorld.PendingForcedSceneId))
                {
                    string forcedSceneId = GameWorld.PendingForcedSceneId;
                    GameWorld.PendingForcedSceneId = null; // Clear pending flag

                    Scene forcedScene = GameWorld.Scenes.FirstOrDefault(s => s.Id == forcedSceneId);
                    if (forcedScene != null)
                    {
                        await GameScreen.StartScene(forcedScene);
                    }
                }
            }
        }
    }

    protected async Task HandleCommitToSituation(Situation situation)
    {
        // STRATEGIC LAYER: Validate requirements, consume Resolve/Time/Coins, route to appropriate subsystem
        SituationSelectionResult result = GameFacade.GetSituationFacade().SelectAndExecuteSituation(situation.Id);

        if (!result.Success)
        {
            // Failed validation or resource check - show error
            return;
        }

        // Handle result based on type
        if (result.ResultType == SituationResultType.InstantResolution)
        {
            // Instant resolution - consequences already applied, refresh location
            await RefreshLocationData();
            await OnActionExecuted.InvokeAsync();
            StateHasChanged();
        }
        else if (result.ResultType == SituationResultType.LaunchChallenge)
        {
            // TACTICAL LAYER: Route to appropriate challenge facade
            // Strategic costs already consumed (Resolve, Time, Coins)
            // Challenge facade will consume tactical costs (Focus/Stamina)
            if (result.ChallengeType == TacticalSystemType.Social)
            {
                await GameScreen.StartConversationSession(result.ChallengeTargetId, result.ChallengeSituationId);
            }
            else if (result.ChallengeType == TacticalSystemType.Mental)
            {
                Player player = GameWorld.GetPlayer();
                await GameScreen.StartMentalSession(result.ChallengeDeckId, result.ChallengeTargetId, result.ChallengeSituationId, situation.Obligation?.Id);
            }
            else if (result.ChallengeType == TacticalSystemType.Physical)
            {
                Player player = GameWorld.GetPlayer();
                await GameScreen.StartPhysicalSession(result.ChallengeDeckId, result.ChallengeTargetId, result.ChallengeSituationId, situation.Obligation?.Id);
            }
        }
        else if (result.ResultType == SituationResultType.Navigation)
        {
            // Navigation - move player and optionally trigger scene at destination
            bool success = await GameFacade.MoveToSpot(result.NavigationDestinationId);
            if (success)
            {
                ResetNavigation();
                await RefreshLocationData();
                await OnActionExecuted.InvokeAsync();
                StateHasChanged();
            }
        }
    }

    protected async Task MoveToSpot(Location spot)
    {
        bool success = await GameFacade.MoveToSpot(spot.Id);

        if (success)
        {
            ResetNavigation();
            await RefreshLocationData();
            await OnActionExecuted.InvokeAsync();

            // MODAL SCENE FORCING: Check if movement triggered a forced modal scene
            // GameFacade sets PendingForcedSceneId after successful movement
            // If found, navigate to forced scene immediately (Sir Brante forced moment pattern)
            if (!string.IsNullOrEmpty(GameWorld.PendingForcedSceneId))
            {
                string forcedSceneId = GameWorld.PendingForcedSceneId;
                GameWorld.PendingForcedSceneId = null; // Clear pending flag

                Scene forcedScene = GameWorld.Scenes.FirstOrDefault(s => s.Id == forcedSceneId);
                if (forcedScene != null)
                {
                    await GameScreen.StartScene(forcedScene);
                }
            }
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

        if (newView == LocationViewState.SituationDetail && context is Situation situation)
        {
            SelectedSituation = situation;
        }

        StateHasChanged();
    }

    protected void NavigateBack()
    {
        if (NavigationStack.Count > 0)
        {
            LocationViewState previousView = NavigationStack.Pop();
            ViewState = previousView;

            if (ViewState != LocationViewState.SituationDetail)
            {
                SelectedSituation = null;
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
        SelectedSituation = null;
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

    protected void HandleNavigateToSituation(Situation situation)
    {
        if (situation != null)
        {
            NavigateToView(LocationViewState.SituationDetail, situation);
        }
    }

    protected async Task HandleMoveToSpot(Location spot)
    {
        await MoveToSpot(spot);
    }

    protected async Task HandleStartExchange(NPC npc)
    {
        await GameScreen.StartExchange(npc.ID);
    }

    protected async Task HandleTalkToNPC(NPC npc, Scene scene)
    {
        await GameScreen.StartNPCEngagement(npc.ID, scene);
    }

    protected async Task HandleAcceptJob(DeliveryJob job)
    {
        // Execute through intent system - backend handles validation
        IntentResult result = await GameFacade.ProcessIntent(new AcceptDeliveryJobIntent(job.Id));

        if (result.Success)
        {
            // Job accepted - close modal and refresh
            NavigateBack();
            await RefreshLocationData();
            await OnActionExecuted.InvokeAsync();
        }
    }

    // ============================================
    // VIEWMODEL PREPARATION - trivial wrappers
    // ============================================

    protected SituationDetailViewModel GetSituationDetailViewModel()
    {
        if (SelectedSituation == null) return null;

        // Find the situation in view model to get pre-calculated difficulty
        // NOTE: Social situations from NPCs removed - they appear in Scene view after engagement
        SituationCardViewModel situationCard = null;

        // Search in Mental situations (ambient + scenes)
        if (situationCard == null)
        {
            situationCard = ViewModel.AmbientMentalSituations.FirstOrDefault(g => g.Id == SelectedSituation.Id);
        }

        if (situationCard == null)
        {
            situationCard = ViewModel.MentalScenes
                .SelectMany(scene => scene.Situations)
                .FirstOrDefault(g => g.Id == SelectedSituation.Id);
        }

        // Search in Physical situations (ambient + scenes)
        if (situationCard == null)
        {
            situationCard = ViewModel.AmbientPhysicalSituations.FirstOrDefault(g => g.Id == SelectedSituation.Id);
        }

        if (situationCard == null)
        {
            situationCard = ViewModel.PhysicalScenes
                .SelectMany(scene => scene.Situations)
                .FirstOrDefault(g => g.Id == SelectedSituation.Id);
        }

        int difficulty = situationCard?.Difficulty ?? 0;

        return new SituationDetailViewModel
        {
            Situation = SelectedSituation,
            Name = SelectedSituation.Name,
            Description = SelectedSituation.Description,
            SystemType = SelectedSituation.SystemType,
            SystemTypeLowercase = SelectedSituation.SystemType.ToString().ToLower(),
            Difficulty = difficulty.ToString(),
            HasCosts = SelectedSituation.Costs.Focus > 0 || SelectedSituation.Costs.Stamina > 0,
            FocusCost = SelectedSituation.Costs.Focus,
            StaminaCost = SelectedSituation.Costs.Stamina
        };
    }

    // ============================================
    // SCREEN EXPANSION HANDLERS
    // ============================================

    protected async Task HandleStartConversationTree(ConversationTree tree)
    {
        await GameScreen.StartConversationTree(tree.Id);
    }

    protected async Task HandleStartObservationScene(ObservationScene scene)
    {
        await GameScreen.StartObservationScene(scene.Id);
    }

    // ============================================
    // SCENE-SITUATION ARCHITECTURE: NPCAction Execution
    // ============================================

    protected async Task HandleExecuteNPCAction(ActionCardViewModel action)
    {
        // Execute NPCAction through GameFacade (direct object reference)
        IntentResult result = await GameFacade.ExecuteNPCAction(action.SourceAction);

        if (result.Success)
        {
            // Screen-level navigation (GameScreen handles)
            if (result.NavigateToScreen.HasValue)
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

    /// <summary>
    /// Create MoveIntent from intra-venue movement action
    /// Uses strongly-typed DestinationLocationId property (no ID parsing)
    /// </summary>
    private MoveIntent CreateIntraVenueMoveIntent(LocationActionViewModel action)
    {
        if (string.IsNullOrEmpty(action.DestinationLocationId))
            throw new InvalidOperationException("IntraVenueMove action missing DestinationLocationId property");

        return new MoveIntent(action.DestinationLocationId);
    }
}
