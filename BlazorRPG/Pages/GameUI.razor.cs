using Microsoft.AspNetCore.Components;

namespace BlazorRPG.Pages;

public partial class GameUI : ComponentBase
{
    [Inject] private GameState GameState { get; set; }
    [Inject] private QueryManager QueryManager { get; set; }
    [Inject] private ActionManager ActionManager { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; }

    private Stack<UIScreen> screenStack = new();
    private List<TravelOption> CurrentTravelOptions { get; set; } = new();
    private LocationNames CurrentLocation { get; set; }
    private List<UserActionOption> CurrentActions { get; set; } = new();
    private UserActionOption CurrentUserAction { get; set; }
    private ActionResult LastActionResult { get; set; }
    public List<string> ResultMessages => GetResultMessages();

    private UIScreen CurrentScreen => screenStack.Count > 0 ? screenStack.Peek() : UIScreen.MainGame;

    protected override void OnInitialized()
    {
        screenStack.Push(UIScreen.MainGame);
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

            TravelOption travel = new TravelOption()
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
            string s = $"Money changed by {food.Amount}";
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

    private void PushScreen(UIScreen screen)
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
        PushScreen(UIScreen.ActionSelection);
    }

    private void HandleActionSelection(UserActionOption action)
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

            bool hasNarrative = ActionManager.HasNarrative(action.ActionType);
            if (hasNarrative)
            {
                bool startedNarrative = ActionManager.StartNarrativeFor(action.ActionType);
                if (startedNarrative)
                {
                    PushScreen(UIScreen.ActionNarrative);
                }
            }
            else
            {
                ActionResult result = ActionManager.ExecuteBasicAction(CurrentLocation);
                LastActionResult = result;

                if (result.IsSuccess)
                {
                    CurrentUserAction = null;
                    GameState.ClearCurrentNarrative();
                    PopScreen();
                    PushScreen(UIScreen.ActionResult);
                    UpdateAvailableActions();
                }
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
            PushScreen(UIScreen.ActionResult);
            UpdateAvailableActions();
        }
    }

    private void HandleTravel(int locationIndex)
    {
        TravelOption location = CurrentTravelOptions.FirstOrDefault(x => x.Index == locationIndex);

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