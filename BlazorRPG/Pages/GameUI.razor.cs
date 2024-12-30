using Microsoft.AspNetCore.Components;

namespace BlazorRPG.Pages;

public partial class GameUI : ComponentBase
{
    [Inject] private GameState GameState { get; set; }
    [Inject] private ActionManager ActionManager { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; }

    private Stack<UIScreens> screenStack = new();
    public List<string> ResultMessages => GetResultMessages();

    public Player Player => GameState.Player;
    public LocationNames CurrentLocation => GameState.CurrentLocation;
    public TimeWindows CurrentTime => GameState.CurrentTimeSlot;
    public List<UserActionOption> CurrentActions => GameState.ValidUserActions;
    public UserActionOption CurrentUserAction => GameState.CurrentUserAction;
    public List<UserTravelOption> CurrentTravelOptions => GameState.CurrentTravelOptions;
    public ActionResult LastActionResult => GameState.LastActionResult;

    private UIScreens CurrentScreen => screenStack.Count > 0 ? screenStack.Peek() : UIScreens.MainGame;

    protected override void OnInitialized()
    {
        screenStack.Push(UIScreens.ActionSelection);
        ActionManager.Initialize();
    }

    public string GetActionDescription(UserActionOption userActionOption)
    {
        string description = string.Empty;

        if (userActionOption.IsDisabled)
        {
            description = "";
        }

        description += userActionOption.Description;
        return description;
    }


    public List<string> GetResultMessages()
    {
        ActionResultMessages messages = GameState.LastActionResult.Messages;
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
            string s = $"Coins changed by {money.Amount}";
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
        else if (action.Action.ActionType == BasicActionTypes.Wait)
        {
            ActionManager.AdvanceTime();
            CompleteActionExecution();
        }
        else
        {
            GameState.SetCurrentUserAction(action);
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

        bool hasNarrative = ActionManager.HasNarrative(GameState.CurrentUserAction.Action);
        if (hasNarrative)
        {
            bool startedNarrative = ActionManager.StartNarrativeFor(GameState.CurrentUserAction.Action);
            if (startedNarrative)
            {
                PopScreen();
                PushScreen(UIScreens.ActionNarrative);
            }
        }
        else
        {
            ActionResult result = ActionManager.ExecuteBasicAction(GameState.CurrentUserAction.Action);

            if (result.IsSuccess)
            {
                PopScreen();
                PushScreen(UIScreens.ActionResult);
                CompleteActionExecution();
            }
        }
    }

    private void HandleNarrativeChoice(int choiceIndex)
    {
        ActionResult result = ActionManager.MakeChoiceForNarrative(
            GameState.CurrentNarrative,
            GameState.CurrentNarrativeStage,
            choiceIndex);


        if (result.IsSuccess)
        {
            PopScreen();
            PushScreen(UIScreens.ActionResult);
            CompleteActionExecution();
        }
    }

    private void HandleTravel(int locationIndex)
    {
        List<UserTravelOption> currentTravelOptions = GameState.CurrentTravelOptions;
        UserTravelOption location = currentTravelOptions.FirstOrDefault(x => x.Index == locationIndex);

        ActionResult result = ActionManager.ExecuteTravelAction(location.Location);

        if (result.IsSuccess)
        {
            PopScreen();
            CompleteActionExecution();
        }
    }

    private void CompleteActionExecution()
    {
        GameState.ClearCurrentUserAction();

        GameState.ClearCurrentNarrative();

        ActionManager.UpdateTavelOptions();
        ActionManager.UpdateAvailableActions();
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
        GameState.ClearLastActionResult();
        PushScreen(UIScreens.ActionSelection);
    }

    private void ExitGame()
    {
        NavigationManager.NavigateTo("/");
    }

}
