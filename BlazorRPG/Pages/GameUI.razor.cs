using Microsoft.AspNetCore.Components;

namespace BlazorRPG.Pages;

public partial class GameUI : ComponentBase
{
    [Inject] private GameState GameState { get; set; }
    [Inject] private QueryManager QueryManager { get; set; }
    [Inject] private ActionManager ActionManager { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; }

    private Stack<UIScreens> screenStack = new();

    private LocationNames CurrentLocation { get; set; }
    private List<UserTravelOption> CurrentTravelOptions { get; set; } = new();
    private List<UserActionOption> CurrentActions { get; set; } = new();
    private UserActionOption CurrentUserAction { get; set; }
    
    private ActionResult LastActionResult { get; set; }
    public List<string> ResultMessages => GetResultMessages();

    private UIScreens CurrentScreen => screenStack.Count > 0 ? screenStack.Peek() : UIScreens.MainGame;

    protected override void OnInitialized()
    {
        screenStack.Push(UIScreens.MainGame);
        CurrentLocation = QueryManager.GetCurrentLocation();

        UpdateAvailableActions();
        UpdateTavelOptions();
    }

    private LocationNames GetCurrentLocation()
    {
        return QueryManager.GetCurrentLocation();
    }

    private void UpdateTavelOptions()
    {
        CurrentTravelOptions.Clear();

        List<LocationNames> locations = QueryManager.GetConnectedLocations();
        for (int i = 0; i < locations.Count; i++)
        {
            LocationNames location = locations[i];

            UserTravelOption travel = new UserTravelOption()
            {
                Index = i + 1,
                Location = location
            };

            CurrentTravelOptions.Add(travel);
        }
    }

    private void UpdateAvailableActions()
    {
        List<PlayerAction> global = QueryManager.GetGlobalActions();
        List<PlayerAction> location = QueryManager.GetLocationActions();
        List<PlayerAction> character = QueryManager.GetCharacterActions();

        List<PlayerAction> playerActions = new List<PlayerAction>();
        playerActions.AddRange(global);
        playerActions.AddRange(location);
        playerActions.AddRange(character);

        CurrentActions = CreateUserActionsFromPlayerActions(playerActions);
    }

    public List<string> GetResultMessages()
    {
        ActionResultMessages messages = LastActionResult.Messages;
        List<string> list = new List<string>();

        foreach (HealthOutcome health in messages.Health)
        {
            string s = $"Health changed by {health.Amount}";
            list.Add(s);
        }
        foreach (FoodOutcome food in messages.Food)
        {
            string s = $"Food changed by {food.Amount}";
            list.Add(s);
        }
        foreach (CoinsOutcome money in messages.Coins)
        {
            string s = $"Money changed by {money.Amount}";
            list.Add(s);
        }
        foreach (PhysicalEnergyOutcome physicalEnergy in messages.PhysicalEnergy)
        {
            string s = $"P. Energy changed by {physicalEnergy.Amount}";
            list.Add(s);
        }
        foreach (FocusEnergyOutcome focusEnergy in messages.FocusEnergy)
        {
            string s = $"F. Energy changed by {focusEnergy.Amount}";
            list.Add(s);
        }
        foreach (SocialEnergyOutcome socialEnergy in messages.SocialEnergy)
        {
            string s = $"S. Energy changed by {socialEnergy.Amount}";
            list.Add(s);
        }
        foreach (SkillLevelOutcome skillLevel in messages.SkillLevel)
        {
            string s = $"Skill Level in {skillLevel.SkillType} changed by {skillLevel.Amount}";
            list.Add(s);
        }
        foreach (ItemOutcome item in messages.Item)
        {
            string s = $"Item acquired: {item.Name}";
            list.Add(s);
        }

        return list;
    }

    private void PushScreen(UIScreens screen)
    {
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

    private void ToActionSelection()
    {
        LastActionResult = null;
        PushScreen(UIScreens.ActionSelection);
    }

    private void HandleActionSelection(UserActionOption action)
    {
        if (action.Action.ActionType == BasicActionTypes.CheckStatus)
        {
            PushScreen(UIScreens.Status);
        }
        else if (action.Action.ActionType == BasicActionTypes.Travel)
        {
            PushScreen(UIScreens.Travel);
        }
        else
        {
            CurrentUserAction = action;
            PushScreen(UIScreens.ActionPreview);
        }
    }

    private void HandleActionConfirmation(bool confirmed)
    {
        if (!confirmed)
        {
            PopScreen();
            return;
        }

        bool hasNarrative = ActionManager.HasNarrative(CurrentUserAction.Action);
        if (hasNarrative)
        {
            bool startedNarrative = ActionManager.StartNarrativeFor(CurrentUserAction.Action);
            if (startedNarrative)
            {
                PopScreen();
                PushScreen(UIScreens.ActionNarrative);
            }
        }
        else
        {
            ActionResult result = ActionManager.ExecuteBasicAction(CurrentUserAction.Action);
            LastActionResult = result;

            if (result.IsSuccess)
            {
                CurrentUserAction = null;
                GameState.ClearCurrentNarrative();
                PopScreen();
                PushScreen(UIScreens.ActionResult);
                UpdateAvailableActions();
            }
        }
    }

    private void HandleNarrativeChoice(int choiceIndex)
    {
        ActionResult result = ActionManager.MakeChoiceForNarrative(
            GameState.CurrentNarrative,
            GameState.CurrentNarrativeStage,
            choiceIndex);
        LastActionResult = result;

        if (result.IsSuccess)
        {
            CurrentUserAction = null;
            GameState.ClearCurrentNarrative();
            PopScreen();
            PushScreen(UIScreens.ActionResult);
            UpdateAvailableActions();
        }
    }

    private void HandleTravel(int locationIndex)
    {
        UserTravelOption location = CurrentTravelOptions.FirstOrDefault(x => x.Index == locationIndex);

        ActionResult result = ActionManager.TravelTo(location.Location);
        LastActionResult = result;
        if (result.IsSuccess)
        {
            PopScreen();
            UpdateAvailableActions();
            UpdateTavelOptions();
            CurrentLocation = QueryManager.GetCurrentLocation();
        }
    }

    private static List<UserActionOption> CreateUserActionsFromPlayerActions(List<PlayerAction> playerActions)
    {
        List<UserActionOption> userActions = new List<UserActionOption>();
        int actionIndex = 1;
        foreach (PlayerAction ga in playerActions)
        {
            UserActionOption ua = new UserActionOption
            {
                Action = ga.Action,
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
