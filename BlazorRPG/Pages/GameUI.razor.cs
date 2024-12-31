using Microsoft.AspNetCore.Components;
using System.Reflection;

namespace BlazorRPG.Pages;

public partial class GameUI : ComponentBase
{
    [Inject] private GameState GameState { get; set; }
    [Inject] private ActionManager ActionManager { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; }

    private Stack<UIScreens> screenStack = new();
    public List<string> ResultMessages => GetResultMessages();

    public int physicalEnergyCurrent => GameState.Player.PhysicalEnergy;
    public int physicalEnergyMax => 10;
    public int socialEnergyCurrent => GameState.Player.SocialEnergy;
    public int socialEnergyMax => 10;
    public int coins => GameState.Player.Coins;
    public int food => GameState.Player.Inventory.GetItemCount(ResourceTypes.Food);
    public bool hasShelter => false;
    public List<Location> Locations => ActionManager.GetAllLocations();

    public Player Player => GameState.Player;
    public Location CurrentLocation => GameState.CurrentLocation;
    public TimeWindows CurrentTime => GameState.CurrentTimeSlot;
    public List<UserActionOption> CurrentActions => GameState.ValidUserActions;
    public UserActionOption CurrentUserAction => GameState.CurrentUserAction;
    public List<UserTravelOption> CurrentTravelOptions => GameState.CurrentTravelOptions;
    public ActionResult LastActionResult => GameState.LastActionResult;

    private UIScreens CurrentScreen => screenStack.Count > 0 ? screenStack.Peek() : UIScreens.MainGame;

    // Tooltip Logic
    private bool showTooltip = false;
    private UserActionOption hoveredAction = null;

    private void ShowTooltip(UserActionOption action)
    {
        hoveredAction = action;
        showTooltip = true;
    }

    private void HideTooltip()
    {
        hoveredAction = null;
        showTooltip = false;
    }

    private bool CanTravelTo(LocationNames locationName)
    {
        List<LocationNames> locs = ActionManager.GetConnectedLocations();
        return locs.Contains(locationName);
    }

    protected bool AreRequirementsMet(UserActionOption action)
    {
        return action.BasicAction.Requirements.All(requirement => requirement switch
        {
            PhysicalEnergyRequirement r => Player.PhysicalEnergy >= r.Amount,
            FocusEnergyRequirement r => Player.FocusEnergy >= r.Amount,
            SocialEnergyRequirement r => Player.SocialEnergy >= r.Amount,
            InventorySlotsRequirement r => Player.Inventory.GetEmptySlots() >= r.Count,
            HealthRequirement r => Player.Health >= r.Amount,
            CoinsRequirement r => Player.Coins >= r.Amount,
            FoodRequirement r => Player.Inventory.GetItemCount(ResourceTypes.Food) >= r.Amount,
            SkillLevelRequirement r => Player.Skills.ContainsKey(r.SkillType) && Player.Skills[r.SkillType] >= r.Amount,
            ItemRequirement r => Player.Inventory.GetItemCount(r.ResourceType) >= r.Count,
            _ => false 
        });
    }


    protected override void OnInitialized()
    {
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
            string s = $"Item {item.ChangeType.ToString()} : {item.ResourceType.ToString()} ({item.Count})";
            list.Add(s);
        }

        return list;
    }

    private void HandleActionSelection(UserActionOption action)
    {
        if (action.IsDisabled) return; // Prevent action if disabled

        if (action.BasicAction.Id == BasicActionTypes.CheckStatus)
        {
            PushScreen(UIScreens.Status);
        }
        else if (action.BasicAction.Id == BasicActionTypes.Travel)
        {
            PushScreen(UIScreens.Travel);
        }
        else if (action.BasicAction.Id == BasicActionTypes.Wait)
        {
            ActionManager.AdvanceTime();
            CompleteActionExecution();
        }
        else
        {
            GameState.SetCurrentUserAction(action);

            // Execute the action immediately
            bool hasNarrative = ActionManager.HasNarrative(action.BasicAction);
            if (hasNarrative)
            {
                bool startedNarrative = ActionManager.StartNarrativeFor(action.BasicAction);
                if (startedNarrative)
                {
                    PushScreen(UIScreens.ActionNarrative);
                }
            }
            else
            {
                ActionResult result = ActionManager.ExecuteBasicAction(action.BasicAction);

                if (result.IsSuccess)
                {
                    PushScreen(UIScreens.ActionResult);
                    CompleteActionExecution();
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

        ActionResult result = ActionManager.MoveToLocation(location.Location);

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
