using Microsoft.AspNetCore.Components;

namespace BlazorRPG.Pages;

public partial class GameUI : ComponentBase
{
    [Inject] private GameState GameState { get; set; }
    [Inject] private ActionManager ActionManager { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; }

    private Stack<UIScreen> screenStack = new();
    private List<UserAction> CurrentActions { get; set; } = new();
    private UserAction CurrentUserAction { get; set; }
    private ActionResult LastActionResult { get; set; }

    private UIScreen CurrentScreen => screenStack.Count > 0 ? screenStack.Peek() : UIScreen.MainGame;

    protected override void OnInitialized()
    {
        screenStack.Push(UIScreen.MainGame);
        UpdateAvailableActions();
    }

    private void UpdateAvailableActions()
    {
        List<PlayerAction> global = GameState.GetGlobalActions();
        List<PlayerAction> location = GameState.GetLocationActions();
        List<PlayerAction> character = GameState.GetCharacterActions();

        List<PlayerAction> playerActions = new List<PlayerAction>();
        playerActions.AddRange(global);
        playerActions.AddRange(location);
        playerActions.AddRange(character);

        CurrentActions = CreateUserActionsFromPlayerActions(playerActions);
    }

    private void PushScreen(UIScreen screen)
    {
        LastActionResult = null;
        screenStack.Push(screen);
        StateHasChanged();
    }

    private void PopScreen()
    {
        if (screenStack.Count > 1)
        {
            screenStack.Pop();
            StateHasChanged();
        }
    }

    private void HandleNewGame()
    {
        PushScreen(UIScreen.ActionSelection);
    }

    private void HandleActionSelection(UserAction action)
    {
        if (action.ActionType == BasicActionTypes.GlobalStatus)
        {
            PushScreen(UIScreen.Status);
        }
        else if (action.ActionType == BasicActionTypes.GlobalTravel)
        {
            PushScreen(UIScreen.Travel);
        }
        else
        {
            CurrentUserAction = action;
            bool startedNarrative = ActionManager.StartNarrativeFor(action.ActionType);
            if (startedNarrative)
            {
                PushScreen(UIScreen.ActionNarrative);
            }
        }
    }

    private void HandleNarrativeChoice(int choiceIndex)
    {
        ActionResult result = ActionManager.MakeChoiceForNarrative(GameState.CurrentNarrative, choiceIndex);
        LastActionResult = result;

        if (result.IsSuccess)
        {
            CurrentUserAction = null;
            GameState.ClearCurrentNarrative();
            PopScreen();
            UpdateAvailableActions();
        }
    }

    private IEnumerable<Location> GetAvailableLocations()
    {
        return GameState.GetLocations();
    }

    private void HandleTravel(Location location)
    {
        ActionResult result = GameState.TravelTo(location);
        LastActionResult = result;
        if (result.IsSuccess)
        {
            PopScreen();
            UpdateAvailableActions();
        }
    }

    private static List<UserAction> CreateUserActionsFromPlayerActions(List<PlayerAction> playerActions)
    {
        List<UserAction> userActions = new List<UserAction>();
        int actionIndex = 1;
        foreach (PlayerAction ga in playerActions)
        {
            UserAction ua = new UserAction
            {
                ActionType = ga.ActionType,
                Description = ga.Description,
                Index = actionIndex++
            };
            userActions.Add(ua);
        }
        return userActions;
    }

    private void ExitGame()
    {
        NavigationManager.NavigateTo("/");
    }
}