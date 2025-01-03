public class GameState
{
    public int CurrentTimeInHours { get; set; }
    public Player Player { get; set; }

    public TimeWindows CurrentTimeSlot { get; private set; } = TimeWindows.Morning;
    public Narrative CurrentNarrative { get; private set; }
    public NarrativeStage CurrentNarrativeStage { get; private set; }
    public Location CurrentLocation { get; private set; }

    public UserActionOption CurrentUserAction { get; private set; }

    public List<UserActionOption> GlobalActions { get; private set; } = new();
    public List<UserActionOption> LocationSpotActions { get; private set; } = new();
    public List<UserActionOption> CharacterActions { get; private set; } = new();
    public List<UserActionOption> QuestActions { get; private set; } = new();

    public List<UserLocationTravelOption> CurrentTravelOptions { get; private set; } = new();
    public List<UserLocationSpotOption> CurrentLocationSpotOptions { get; private set; }
    public LocationSpot CurrentLocationSpot { get; set; }
    public List<Quest> ActiveQuests { get; set; }
    public ActionResult LastActionResult { get; set; }

    public void SetCurrentNarrative(Narrative narrative)
    {
        CurrentNarrative = narrative;
        CurrentNarrativeStage = narrative.Stages[0];
    }

    public void ClearCurrentNarrative()
    {
        CurrentNarrative = null;
        CurrentNarrativeStage = null;
    }


    public bool ModifyCoins(int amount)
    {
        int newCoins = Math.Max(0, Player.Coins + amount);
        if (newCoins != Player.Coins)
        {
            Player.Coins = newCoins;
            return true;
        }
        return false;
    }

    public bool ModifyFood(int amount)
    {
        Inventory inventory = Player.Inventory;
        int currentFood = inventory.GetItemCount(ResourceTypes.Food);

        int updatedFood = Math.Clamp(currentFood + amount, 0, inventory.GetCapacityFor(ResourceTypes.Food));
        if (updatedFood != currentFood)
        {
            inventory.SetItemCount(ResourceTypes.Food, updatedFood);
            return true;
        }
        return false;
    }

    public bool ModifyHealth(int amount)
    {
        int newHealth = Math.Clamp(Player.Health + amount, 0, Player.MaxHealth);
        if (newHealth != Player.Health)
        {
            Player.Health = newHealth;
            return true;
        }
        return false;
    }

    public bool ModifyPhysicalEnergy(int amount)
    {
        int newEnergy = Math.Clamp(Player.PhysicalEnergy + amount, 0, Player.MaxPhysicalEnergy);
        if (newEnergy != Player.PhysicalEnergy)
        {
            Player.PhysicalEnergy = newEnergy;
            return true;
        }
        return false;
    }

    public bool ModifyFocusEnergy(int amount)
    {
        int newEnergy = Math.Clamp(Player.FocusEnergy + amount, 0, Player.MaxFocusEnergy);
        if (newEnergy != Player.FocusEnergy)
        {
            Player.FocusEnergy = newEnergy;
            return true;
        }
        return false;
    }

    public bool ModifySocialEnergy(int amount)
    {
        int newEnergy = Math.Clamp(Player.SocialEnergy + amount, 0, Player.MaxSocialEnergy);
        if (newEnergy != Player.SocialEnergy)
        {
            Player.SocialEnergy = newEnergy;
            return true;
        }
        return false;
    }

    public bool ModifySkillLevel(SkillTypes skillType, int amount)
    {
        int newSkillLevel = Math.Max(0, Player.Skills[skillType] + amount);
        if (newSkillLevel != Player.Skills[skillType])
        {
            Player.Skills[skillType] = newSkillLevel;
            return true;
        }
        return false;
    }

    public bool ModifyItem(ResourceChangeType itemChange, ResourceTypes resourceType, int count)
    {
        if (itemChange == ResourceChangeType.Added)
        {
            int itemsAdded = Player.Inventory.AddItems(resourceType, count);
            return itemsAdded == count;
        }
        else if (itemChange == ResourceChangeType.Removed)
        {
            int itemsRemoved = Player.Inventory.RemoveItems(resourceType, count);
            return itemsRemoved == count;
        }

        return false;
    }


    public void DetermineCurrentTimeSlot(int timeSlot)
    {
        switch (timeSlot)
        {
            case 0:
                CurrentTimeSlot = TimeWindows.Night;
                break;
            case 1:
                CurrentTimeSlot = TimeWindows.Morning;
                break;
            case 2:
                CurrentTimeSlot = TimeWindows.Afternoon;
                break;
            case 3:
                CurrentTimeSlot = TimeWindows.Evening;
                break;
        }
    }

    public void ChangeHealth(int healthGain)
    {
        Player.Health = Math.Min(Player.MaxHealth, Math.Max(Player.MinHealth, Player.Health + healthGain));
    }

    public void ClearCurrentUserAction()
    {
        CurrentUserAction = null;
    }

    public void SetCurrentUserAction(UserActionOption action)
    {
        CurrentUserAction = action;
    }

    public void SetCurrentLocationSpotOptions(List<UserLocationSpotOption> userLocationSpotOption)
    {
        this.CurrentLocationSpotOptions = userLocationSpotOption;
    }

    public void SetCurrentTravelOptions(List<UserLocationTravelOption> userTravelOptions)
    {
        this.CurrentTravelOptions = userTravelOptions;
    }

    public void SetGlobalActions(List<UserActionOption> userActions)
    {
        this.GlobalActions = userActions;
    }

    public void SetLocationSpotActions(List<UserActionOption> userActions)
    {
        this.LocationSpotActions = userActions;
    }

    public void AddLocationSpotActions(List<UserActionOption> userActions)
    {
        this.LocationSpotActions.AddRange(userActions);
    }

    public void SetNewLocation(Location location)
    {
        CurrentLocation = location;
        CurrentLocationSpot = location.Spots.FirstOrDefault();
    }

    public void SetNewLocationSpot(LocationSpot locationSpot)
    {
        CurrentLocationSpot = locationSpot;
    }

    public List<UserActionOption> GetActions(LocationSpot locationSpot)
    {
        List<UserActionOption> locationActions =
            this.LocationSpotActions
            .Where(x => x.Location == locationSpot.Location)
            .Where(x => x.LocationSpot == locationSpot.Name)
            .ToList();

        List<UserActionOption> characterActions =
            CharacterActions
            .Where(x => x.Location == locationSpot.Location)
            .Where(x => x.LocationSpot == locationSpot.Name)
            .ToList();

        List<UserActionOption> questActions =
            QuestActions
            .Where(x => x.Location == locationSpot.Location)
            .Where(x => x.LocationSpot == locationSpot.Name)
            .ToList();

        List<UserActionOption> actions = new List<UserActionOption>();
        actions.AddRange(locationActions);
        actions.AddRange(characterActions);
        actions.AddRange(questActions);

        return actions;
    }

    public void AddCharacterActions(List<UserActionOption> userActions)
    {
        CharacterActions.AddRange(userActions);
    }

    public void SetCharacterActions(List<UserActionOption> userActionOptions)
    {
        CharacterActions = userActionOptions;
    }

    public void SetQuestActions(List<UserActionOption> userActionOptions)
    {
        QuestActions = userActionOptions;
    }

    public void AddQuestActions(List<UserActionOption> userActions)
    {
        QuestActions.AddRange(userActions);
    }

    public void SetLastActionResult(ActionResult actionResult)
    {
        LastActionResult = actionResult;
    }
}