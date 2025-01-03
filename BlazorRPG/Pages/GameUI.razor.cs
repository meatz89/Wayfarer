using Microsoft.AspNetCore.Components;

namespace BlazorRPG.Pages;

public partial class GameUI : ComponentBase
{
    [Inject] private GameState GameState { get; set; }
    [Inject] private GameManager GameManager { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; }

    public List<string> ResultMessages => GetResultMessages();

    public int physicalEnergyCurrent => GameState.Player.PhysicalEnergy;
    public int physicalEnergyMax => GameState.Player.MaxPhysicalEnergy;
    public int focusEnergyCurrent => GameState.Player.FocusEnergy;
    public int focusEnergyMax => GameState.Player.MaxFocusEnergy;
    public int socialEnergyCurrent => GameState.Player.SocialEnergy;
    public int socialEnergyMax => GameState.Player.MaxSocialEnergy;
    public int health => GameState.Player.Health;
    public int maxHealth => GameState.Player.MaxHealth;
    public int coins => GameState.Player.Coins;
    public int food => GameState.Player.Inventory.GetItemCount(ResourceTypes.Food);
    public bool hasShelter => false;

    public List<Location> Locations => GameManager.GetAllLocations();

    public Player Player => GameState.Player;
    public Location CurrentLocation => GameState.CurrentLocation;
    public LocationSpot CurrentSpot => GameState.CurrentLocationSpot;
    public TimeWindows CurrentTime => GameState.CurrentTimeSlot;
    public int CurrentHour => GameState.CurrentTimeInHours;

    public UserActionOption CurrentUserAction => GameState.CurrentUserAction;
    public List<UserLocationTravelOption> CurrentTravelOptions => GameState.CurrentTravelOptions;

    // Tooltip Logic
    public bool showAreaMap = true;
    public bool showTooltip = false;
    public UserActionOption hoveredAction = null;

    private double mouseX;
    private double mouseY;

    protected override void OnInitialized()
    {
        GameManager.StartGame();
    }
    
    public string GetModifierDescription(IGameStateModifier modifier)
    {
        if (modifier is FoodModfier modfier)
        {
            return $"Need additional Food: {modfier.AdditionalFood}";
        }

        return string.Empty;
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
        foreach (ResourceOutcome item in messages.Resources)
        {
            string s = $"Item {item.ChangeType.ToString()} : {item.Resource.ToString()} ({item.Count})";
            list.Add(s);
        }

        return list;
    }

    private void HandleActionSelection(UserActionOption action)
    {
        if (action.IsDisabled) return; // Prevent action if disabled

        else if (action.BasicAction.ActionType == BasicActionTypes.Wait)
        {
            GameManager.AdvanceTime();
            CompleteActionExecution();
        }
        else
        {
            GameState.SetCurrentUserAction(action);

            // Execute the action immediately
            bool hasNarrative = GameManager.HasNarrative(action.BasicAction);
            if (hasNarrative)
            {
                bool startedNarrative = GameManager.StartNarrativeFor(action.BasicAction);
                if (startedNarrative)
                {
                }
            }
            else
            {
                ActionResult result = GameManager.ExecuteBasicAction(action.BasicAction);

                if (result.IsSuccess)
                {
                    CompleteActionExecution();
                }
            }
        }
    }

    public List<Quest> GetActiveQuests()
    {
        return GameState.ActiveQuests;
    }

    //private void HandleNarrativeChoice(int choiceIndex)
    //{
    //    ActionResult result = ActionManager.MakeChoiceForNarrative(
    //        GameState.CurrentNarrative,
    //        GameState.CurrentNarrativeStage,
    //        choiceIndex);


    //    if (result.IsSuccess)
    //    {
    //        CompleteActionExecution();
    //    }
    //}

    private void HandleSpotSelection(LocationSpot locationSpot)
    {
        List<UserLocationSpotOption> userLocationSpotOptions = GameState.CurrentLocationSpotOptions;
        UserLocationSpotOption userLocationSpot = userLocationSpotOptions.FirstOrDefault(x => x.LocationSpot == locationSpot.Name);

        GameManager.MoveToLocationSpot(userLocationSpot.Location, locationSpot.Name);
    }

    private void HandleLocationSelection(LocationNames locationNames)
    {
        List<UserLocationTravelOption> currentTravelOptions = GameState.CurrentTravelOptions;
        UserLocationTravelOption location = currentTravelOptions.FirstOrDefault(x => x.Location == locationNames);

        ActionResult result = GameManager.MoveToLocation(location.Location);

        if (result.IsSuccess)
        {
            CompleteActionExecution();
            showAreaMap = false;
        }
    }

    private void CompleteActionExecution()
    {
        GameState.ClearCurrentUserAction();
        GameState.ClearCurrentNarrative();

        GameManager.UpdateState();
    }

}
