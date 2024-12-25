public class GameState
{
    private ActionResultMessages outstandingChanges = new();
    private ActionResultMessages processedChanges = new();
    public Narrative CurrentNarrative { get; set; }
    public NarrativeStage CurrentNarrativeStage { get; set; }
    public List<Location> Locations { get; set; }
    public LocationNames CurrentLocation { get; set; }
    public PlayerInfo PlayerInfo { get; set; }
    
    public List<PlayerAction> GetGlobalActions()
    {
        List<PlayerAction> actions = new List<PlayerAction>();
        actions.Add(new PlayerAction()
        {
            ActionType = BasicActionTypes.GlobalStatus,
            Description = "[Player] Check Status"
        });
        actions.Add(new PlayerAction()
        {
            ActionType = BasicActionTypes.GlobalTravel,
            Description = "[Player] Travel"
        });

        return actions;
    }

    public List<PlayerAction> GetLocationActions()
    {
        List<PlayerAction> actions = new List<PlayerAction>();
        actions.Add(new PlayerAction()
        {
            ActionType = BasicActionTypes.Investigate,
            Description = "[Docks] Investigate"
        });

        return actions;
    }

    public List<PlayerAction> GetCharacterActions()
    {
        List<PlayerAction> actions = new List<PlayerAction>();

        return actions;
    }

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

    public void AddMoneyChange(MoneyOutcome moneyOutcome)
    {
        outstandingChanges.Money.Add(moneyOutcome);
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
            for (int i = 0; i < outstandingChanges.Money.Count; i++)
            {
                MoneyOutcome money = outstandingChanges.Money[i];
                bool neededChange = this.ModifyMoney(money.Amount);
                if (neededChange)
                {
                    processedChanges.Money.Add(money);
                }
                outstandingChanges.Money.RemoveAt(i--);
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

    private bool ModifyMoney(int amount)
    {
        int newMoney = Math.Max(0, PlayerInfo.Money + amount);
        if (newMoney != PlayerInfo.Money)
        {
            PlayerInfo.Money = newMoney;
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

    public ActionResult TravelTo(LocationNames locationName)
    {
        Location location = FindLocation(locationName);

        CurrentLocation = location.Name;
        return ActionResult.Success($"Moved to {location.Name}.", new ActionResultMessages());
    }

    public List<LocationNames> GetConnectedLocations()
    {
        Location location = FindLocation(CurrentLocation);

        List<LocationNames> loc = location.ConnectedLocations;
        return loc;
    }

    private Location FindLocation(LocationNames locationName)
    {
        return Locations.Where(x => x.Name == locationName).FirstOrDefault();
    }

}