using Microsoft.AspNetCore.Mvc.TagHelpers;

public class GameState
{
    public Player Player { get; set; }

    private ActionResultMessages outstandingChanges = new();
    private ActionResultMessages processedChanges = new();
    public List<Location> Locations { get; set; }
    public LocationSystem LocationSystem { get; }

    public int CurrentTimeInHours { get; private set; }
    public TimeWindows CurrentTimeSlot { get; private set; } = TimeWindows.Morning;
    public Narrative CurrentNarrative { get; private set; }
    public NarrativeStage CurrentNarrativeStage { get; private set; }
    public LocationNames CurrentLocation { get; private set; }
    public UserActionOption CurrentUserAction { get; private set; }
    public List<UserTravelOption> CurrentTravelOptions { get; private set; } = new();
    public List<UserActionOption> CurrentActions { get; private set; } = new();
    public ActionResult LastActionResult { get; private set; }

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
                bool neededChange = this.ModifyItem(item.Name);
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
        int newFood = Math.Max(0, Player.Inventory.Food + amount);
        if (newFood != Player.Inventory.Food)
        {
            Player.Inventory.Food = newFood;
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

    private bool ModifyItem(string name)
    {
        return false;
    }

    public ActionResultMessages GetAndClearChanges()
    {
        ActionResultMessages changes = processedChanges;
        outstandingChanges = new ActionResultMessages();
        processedChanges = new ActionResultMessages();
        return changes;
    }

    public void AdvanceTime(int inHours)
    {
        const int hoursPerWindow = 6;
        int timeSlot = (int)CurrentTimeInHours / 6;

        switch (timeSlot)
        {
            case 0:
                CurrentTimeSlot = TimeWindows.Night; break;

            case 1:
                CurrentTimeSlot = TimeWindows.Afternoon; break;

            case 2:
                CurrentTimeSlot = TimeWindows.Evening; break;

            case 3:
                if (CurrentTimeSlot != TimeWindows.Morning)
                {
                    StartNewDay();
                }
                CurrentTimeSlot = TimeWindows.Morning; break;
        }
    }

    private void StartNewDay()
    {
        bool hasShelter = true;

        int food = Player.Inventory.Food;
        int foodNeeded = GameRules.DailyFoodRequirement;
        bool hasFood = foodNeeded >= food;

        food = hasFood ? food - foodNeeded : 0;

        int health = Player.Health;
        int minHealth = Player.MinHealth;
        int noFoodHealthLoss = GameRules.HealthLossNoFood;
        int noShelterHealthLoss = GameRules.HealthLossNoShelter;

        if (!hasFood) health = health - noFoodHealthLoss;

        if (health < minHealth)
        {
            throw new Exception("You Lost");
            Environment.Exit(0);
        }
    }

    internal void UpdateTavelOptions(List<LocationNames> locations)
    {
        CurrentTravelOptions.Clear();
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


    public void CreateUserActionsFromPlayerActions(List<PlayerAction> playerActions)
    {
        List<UserActionOption> userActions = new List<UserActionOption>();
        int actionIndex = 1;

        foreach (PlayerAction ga in playerActions)
        {
            bool isDisabled = ga.Action.TimeSlots.Count > 0 && !ga.Action.TimeSlots.Contains(CurrentTimeSlot);

            UserActionOption ua = new UserActionOption
            {
                Action = ga.Action,
                Description = ga.Description,
                Index = actionIndex++,
                IsDisabled = isDisabled
            };
            userActions.Add(ua);
        }

        CurrentActions = userActions;
    }

    internal void SetLastActionResult(ActionResult result)
    {
        LastActionResult = result;
    }

    internal void SetCurrentLocation(LocationNames name)
    {
        CurrentLocation = name;
    }

    internal void ClearLastActionResult()
    {
        LastActionResult = null;
    }

    internal void ClearCurrentUserAction()
    {
        CurrentUserAction = null;
    }

    internal void SetCurrentUserAction(UserActionOption action)
    {
        CurrentUserAction = action;
    }

    internal void SetCurrentTime(int hours)
    {
        CurrentTimeInHours = hours - 1;
        AdvanceTime(1);
    }
}