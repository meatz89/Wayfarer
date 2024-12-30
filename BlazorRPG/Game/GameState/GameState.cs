public class GameState
{
    public PlayerInfo PlayerInfo { get; set; }
    public PlayerInventory PlayerInventory { get; set; }

    private ActionResultMessages outstandingChanges = new();
    private ActionResultMessages processedChanges = new();
    public Narrative CurrentNarrative { get; set; }
    public NarrativeStage CurrentNarrativeStage { get; set; }
    public List<Location> Locations { get; set; }
    public LocationSystem LocationSystem { get; }
    public LocationNames CurrentLocation { get; set; }
    public TimeWindows CurrentTime { get; private set; } = TimeWindows.Morning;

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
        int newCoins = Math.Max(0, PlayerInfo.Coins + amount);
        if (newCoins != PlayerInfo.Coins)
        {
            PlayerInfo.Coins = newCoins;
            return true;
        }
        return false;
    }

    private bool ModifyFood(int amount)
    {
        int newFood = Math.Max(0, PlayerInventory.Food + amount);
        if (newFood != PlayerInventory.Food)
        {
            PlayerInventory.Food = newFood;
            return true;
        }
        return false;
    }

    private bool ModifyHealth(int amount)
    {
        int newHealth = Math.Clamp(PlayerInfo.Health + amount, 0, PlayerInfo.MaxHealth);
        if (newHealth != PlayerInfo.Health)
        {
            PlayerInfo.Health = newHealth;
            return true;
        }
        return false;
    }

    private bool ModifyPhysicalEnergy(int amount)
    {
        int newEnergy = Math.Clamp(PlayerInfo.PhysicalEnergy + amount, 0, PlayerInfo.MaxPhysicalEnergy);
        if (newEnergy != PlayerInfo.PhysicalEnergy)
        {
            PlayerInfo.PhysicalEnergy = newEnergy;
            return true;
        }
        return false;
    }

    private bool ModifyFocusEnergy(int amount)
    {
        int newEnergy = Math.Clamp(PlayerInfo.FocusEnergy + amount, 0, PlayerInfo.MaxFocusEnergy);
        if (newEnergy != PlayerInfo.FocusEnergy)
        {
            PlayerInfo.FocusEnergy = newEnergy;
            return true;
        }
        return false;
    }

    private bool ModifySocialEnergy(int amount)
    {
        int newEnergy = Math.Clamp(PlayerInfo.SocialEnergy + amount, 0, PlayerInfo.MaxSocialEnergy);
        if (newEnergy != PlayerInfo.SocialEnergy)
        {
            PlayerInfo.SocialEnergy = newEnergy;
            return true;
        }
        return false;
    }

    private bool ModifySkillLevel(SkillTypes skillType, int amount)
    {
        int newSkillLevel = Math.Max(0, PlayerInfo.Skills[skillType] + amount);
        if (newSkillLevel != PlayerInfo.Skills[skillType])
        {
            PlayerInfo.Skills[skillType] = newSkillLevel;
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

    public void AdvanceTime(int timeSlots)
    {
        switch (CurrentTime)
        {
            case TimeWindows.Morning:
                CurrentTime = TimeWindows.Afternoon; break;

            case TimeWindows.Afternoon:
                CurrentTime = TimeWindows.Evening; break;

            case TimeWindows.Evening:
                CurrentTime = TimeWindows.Night; break;

            case TimeWindows.Night:
                CurrentTime = TimeWindows.Morning; break;

        }
    }
}