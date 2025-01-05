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

    public PlayerState Player => GameState.Player;
    public Location CurrentLocation => GameState.World.CurrentLocation;
    public LocationSpot CurrentSpot => GameState.World.CurrentLocationSpot;
    public TimeSlots CurrentTime => GameState.World.CurrentTimeSlot;
    public int CurrentHour => GameState.World.CurrentTimeInHours;

    public UserActionOption CurrentUserAction => GameState.Actions.CurrentUserAction;
    public List<UserLocationTravelOption> CurrentTravelOptions => GameState.World.CurrentTravelOptions;

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

    public bool HasNarrative()
    {
        return GameState.Actions.CurrentNarrative != null;
    }

    private void HandleNarrativeCompleted()
    {
        // Force a re-render of the GameUI component
        StateHasChanged();
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
        ActionResultMessages messages = GameState.Actions.LastActionResultMessages;

        List<string> list = new();
        if (messages == null) return list;

        // Show outcomes with their previews
        foreach (Outcome outcome in messages.Outcomes)
        {
            string description = outcome.GetDescription();
            string preview = outcome.GetPreview(Player);
            list.Add($"{description}");
        }

        foreach (SystemMessage sysMsg in messages.SystemMessages)
        {
            // Add CSS class based on message type
            string cssClass = sysMsg.Type switch
            {
                SystemMessageTypes.Warning => "warning",
                SystemMessageTypes.Danger => "danger",
                SystemMessageTypes.Success => "success",
                _ => "info"
            };

            list.Add($"{sysMsg.Message}");
            //list.Add($"<span class='{cssClass}'>{sysMsg.Message}</span>");
        }

        return list;
    }

    private void HandleActionSelection(UserActionOption action)
    {
        if (action.IsDisabled) return; // Prevent action if disabled
        //else if (action.BasicAction.ActionType == BasicActionTypes.Wait)
        //{
        //    GameManager.AdvanceTime();
        //    CompleteActionExecution();
        //}
        else
        {
            // Execute the action immediately
            ActionResult result = GameManager.ExecuteBasicAction(action, action.BasicAction);
            if (result.IsSuccess)
            {
                CompleteActionExecution();
            }
        }
    }

    public List<Quest> GetActiveQuests()
    {
        return GameState.Actions.ActiveQuests;
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
        List<UserLocationSpotOption> userLocationSpotOptions = GameState.World.CurrentLocationSpotOptions;
        UserLocationSpotOption userLocationSpot = userLocationSpotOptions.FirstOrDefault(x => x.LocationSpot == locationSpot.Name);

        GameManager.MoveToLocationSpot(userLocationSpot.Location, locationSpot.Name);
    }

    private void HandleLocationSelection(LocationNames locationNames)
    {
        List<UserLocationTravelOption> currentTravelOptions = GameState.World.CurrentTravelOptions;
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
        GameManager.UpdateState();
    }

}
