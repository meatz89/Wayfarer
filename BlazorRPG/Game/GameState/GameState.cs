public class GameState
{
    public Player Player { get; set; }

    private ActionResultMessages outstandingChanges = new();
    private ActionResultMessages processedChanges = new();
    public int CurrentTimeInHours { get; private set; }
    public TimeWindows CurrentTimeSlot { get; private set; } = TimeWindows.Morning;
    public Narrative CurrentNarrative { get; private set; }
    public NarrativeStage CurrentNarrativeStage { get; private set; }
    public Location CurrentLocation { get; private set; }
    public ActionResult LastActionResult { get; private set; }

    public UserActionOption CurrentUserAction { get; private set; }

    public List<UserActionOption> GlobalActions { get; private set; } = new();
    public List<UserActionOption> LocationActions { get; private set; } = new();
    public List<UserActionOption> LocationSpotActions { get; private set; } = new();
    public List<UserActionOption> CharacterActions { get; private set; } = new();

    public List<UserLocationTravelOption> CurrentTravelOptions { get; private set; } = new();
    public List<UserLocationSpotOption> CurrentLocationSpotOptions { get; private set; }
    public LocationSpot CurrentLocationSpot { get; internal set; }

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

    public void AddCoinsChange(CoinsOutcome moneyOutcome)
    {
        outstandingChanges.Coins.Add(moneyOutcome);
    }

    public void AddFoodChange(FoodOutcome foodOutcome)
    {
        outstandingChanges.Food.Add(foodOutcome);
    }

    public void AddHealthChange(HealthOutcome healthOutcome)
    {
        outstandingChanges.Health.Add(healthOutcome);
    }

    public void AddPhysicalEnergyChange(PhysicalEnergyOutcome physicalEnergyOutcome)
    {
        outstandingChanges.PhysicalEnergy.Add(physicalEnergyOutcome);
    }

    public void AddFocusEnergyChange(FocusEnergyOutcome focusEnergyOutcome)
    {
        outstandingChanges.FocusEnergy.Add(focusEnergyOutcome);
    }

    public void AddSocialEnergyChange(SocialEnergyOutcome socialEnergyOutcome)
    {
        outstandingChanges.SocialEnergy.Add(socialEnergyOutcome);
    }

    public void AddSkillLevelChange(SkillLevelOutcome skillLevelOutcome)
    {
        outstandingChanges.SkillLevel.Add(skillLevelOutcome);
    }

    public void AddItemChange(ItemOutcome itemOutcome)
    {
        outstandingChanges.Item.Add(itemOutcome);
    }

    public void ApplyAllChanges()
    {
        while (true)
        {
            bool changeProcessed = false;

            // Process Changes
            for (int i = 0; i < outstandingChanges.Coins.Count; i++)
            {
                CoinsOutcome money = outstandingChanges.Coins[i];
                bool neededChange = this.ModifyCoins(money.Amount);
                if (neededChange)
                {
                    processedChanges.Coins.Add(money);
                }
                outstandingChanges.Coins.RemoveAt(i--);
                changeProcessed = true;
            }
            for (int i = 0; i < outstandingChanges.Food.Count; i++)
            {
                FoodOutcome food = outstandingChanges.Food[i];
                bool neededChange = this.ModifyFood(food.Amount);
                if (neededChange)
                {
                    processedChanges.Food.Add(food);
                }
                outstandingChanges.Food.RemoveAt(i--);
                changeProcessed = true;
            }
            for (int i = 0; i < outstandingChanges.Health.Count; i++)
            {
                HealthOutcome health = outstandingChanges.Health[i];
                bool neededChange = this.ModifyHealth(health.Amount);
                if (neededChange)
                {
                    processedChanges.Health.Add(health);
                }
                outstandingChanges.Health.RemoveAt(i--);
                changeProcessed = true;
            }
            for (int i = 0; i < outstandingChanges.PhysicalEnergy.Count; i++)
            {
                PhysicalEnergyOutcome physicalEnergy = outstandingChanges.PhysicalEnergy[i];
                bool neededChange = this.ModifyPhysicalEnergy(physicalEnergy.Amount);
                if (neededChange)
                {
                    processedChanges.PhysicalEnergy.Add(physicalEnergy);
                }
                outstandingChanges.PhysicalEnergy.RemoveAt(i--);
                changeProcessed = true;
            }
            for (int i = 0; i < outstandingChanges.FocusEnergy.Count; i++)
            {
                FocusEnergyOutcome focusEnergy = outstandingChanges.FocusEnergy[i];
                bool neededChange = this.ModifyFocusEnergy(focusEnergy.Amount);
                if (neededChange)
                {
                    processedChanges.FocusEnergy.Add(focusEnergy);
                }
                outstandingChanges.FocusEnergy.RemoveAt(i--);
                changeProcessed = true;
            }
            for (int i = 0; i < outstandingChanges.SocialEnergy.Count; i++)
            {
                SocialEnergyOutcome socialEnergy = outstandingChanges.SocialEnergy[i];
                bool neededChange = this.ModifySocialEnergy(socialEnergy.Amount);
                if (neededChange)
                {
                    processedChanges.SocialEnergy.Add(socialEnergy);
                }
                outstandingChanges.SocialEnergy.RemoveAt(i--);
                changeProcessed = true;
            }
            for (int i = 0; i < outstandingChanges.SkillLevel.Count; i++)
            {
                SkillLevelOutcome skillLevel = outstandingChanges.SkillLevel[i];
                bool neededChange = this.ModifySkillLevel(skillLevel.SkillType, skillLevel.Amount);
                if (neededChange)
                {
                    processedChanges.SkillLevel.Add(skillLevel);
                }
                outstandingChanges.SkillLevel.RemoveAt(i--);
                changeProcessed = true;
            }
            for (int i = 0; i < outstandingChanges.Item.Count; i++)
            {
                ItemOutcome item = outstandingChanges.Item[i];
                bool neededChange = this.ModifyItem(item.ChangeType, item.ResourceType, item.Count);
                if (neededChange)
                {
                    processedChanges.Item.Add(item);
                }
                outstandingChanges.Item.RemoveAt(i--);
                changeProcessed = true;
            }
            // If no changes were processed, break the loop
            if (!changeProcessed)
            {
                break;
            }
        }
    }

    private bool ModifyCoins(int amount)
    {
        int newCoins = Math.Max(0, Player.Coins + amount);
        if (newCoins != Player.Coins)
        {
            Player.Coins = newCoins;
            return true;
        }
        return false;
    }

    private bool ModifyFood(int amount)
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

    private bool ModifyHealth(int amount)
    {
        int newHealth = Math.Clamp(Player.Health + amount, 0, Player.MaxHealth);
        if (newHealth != Player.Health)
        {
            Player.Health = newHealth;
            return true;
        }
        return false;
    }

    private bool ModifyPhysicalEnergy(int amount)
    {
        int newEnergy = Math.Clamp(Player.PhysicalEnergy + amount, 0, Player.MaxPhysicalEnergy);
        if (newEnergy != Player.PhysicalEnergy)
        {
            Player.PhysicalEnergy = newEnergy;
            return true;
        }
        return false;
    }

    private bool ModifyFocusEnergy(int amount)
    {
        int newEnergy = Math.Clamp(Player.FocusEnergy + amount, 0, Player.MaxFocusEnergy);
        if (newEnergy != Player.FocusEnergy)
        {
            Player.FocusEnergy = newEnergy;
            return true;
        }
        return false;
    }

    private bool ModifySocialEnergy(int amount)
    {
        int newEnergy = Math.Clamp(Player.SocialEnergy + amount, 0, Player.MaxSocialEnergy);
        if (newEnergy != Player.SocialEnergy)
        {
            Player.SocialEnergy = newEnergy;
            return true;
        }
        return false;
    }

    private bool ModifySkillLevel(SkillTypes skillType, int amount)
    {
        int newSkillLevel = Math.Max(0, Player.Skills[skillType] + amount);
        if (newSkillLevel != Player.Skills[skillType])
        {
            Player.Skills[skillType] = newSkillLevel;
            return true;
        }
        return false;
    }

    private bool ModifyItem(ItemChangeType itemChange, ResourceTypes resourceType, int count)
    {
        if (itemChange == ItemChangeType.Added)
        {
            int itemsAdded = Player.Inventory.AddItems(resourceType, count);
            return itemsAdded == count;
        }
        else if (itemChange == ItemChangeType.Removed)
        {
            int itemsRemoved = Player.Inventory.RemoveItems(resourceType, count);
            return itemsRemoved == count;
        }

        return false;
    }


    public ActionResultMessages GetAndClearChanges()
    {
        ActionResultMessages changes = processedChanges;
        outstandingChanges = new ActionResultMessages();
        processedChanges = new ActionResultMessages();
        return changes;
    }

    public void SetNewTime(int hours)
    {
        CurrentTimeInHours = hours - 1;
        bool stillAlive = AdvanceTime(1);
    }

    public bool AdvanceTime(int inHours)
    {
        // Advance the current time
        CurrentTimeInHours += inHours;

        // Constants and calculation for time slot determination
        const int hoursPerWindow = 6;
        int timeSlot = (CurrentTimeInHours / hoursPerWindow) % 4; // There are 4 time windows in a day

        // Store the previous time slot to detect transitions
        TimeWindows previousTimeSlot = CurrentTimeSlot;

        // Determine the current time slot based on the calculated timeSlot value
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

        // If transitioning from Night to Morning, start a new day
        if (previousTimeSlot == TimeWindows.Night && CurrentTimeSlot == TimeWindows.Morning)
        {
            return StartNewDay(); // Assuming StartNewDay returns a bool indicating success or failure
        }

        // Return true to indicate normal time advancement
        return true;
    }

    private bool StartNewDay()
    {
        bool hasShelter = LocationActions
            .Where(x => x.BasicAction.Id == BasicActionTypes.Rest)
            .FirstOrDefault() != null;

        int food = Player.Inventory.GetItemCount(ResourceTypes.Food);
        int foodNeeded = GameRules.DailyFoodRequirement;
        bool hasFood = food >= foodNeeded;

        food = hasFood ? food - foodNeeded : 0;

        int health = Player.Health;
        int minHealth = Player.MinHealth;
        int noFoodHealthLoss = GameRules.HealthLossNoFood;
        int noShelterHealthLoss = GameRules.HealthLossNoShelter;

        if (!hasFood) Player.Health = health - noFoodHealthLoss;
        if (!hasShelter) Player.Health = health - noShelterHealthLoss;

        return Player.Health > Player.MinHealth;
    }

    public void SetLastActionResult(ActionResult result)
    {
        LastActionResult = result;
    }

    public void ClearLastActionResult()
    {
        LastActionResult = null;
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

    public void SetLocationActions(List<UserActionOption> userActions)
    {
        this.LocationActions = userActions;
    }

    public void AddLocationActions(List<UserActionOption> userActions)
    {
        this.CharacterActions.AddRange(userActions);
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

    public List<UserActionOption> GetLocationSpotActions(LocationSpot locationSpot)
    {
        List<UserActionOption> locationSpotActions = this.LocationSpotActions;
        List<UserActionOption> userActionOptions =
            locationSpotActions.Where(x => x.LocationSpot == locationSpot.Name &&
            x.Location == locationSpot.Location).ToList();
        return userActionOptions;
    }

}