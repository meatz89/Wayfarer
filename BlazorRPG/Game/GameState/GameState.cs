public class GameState
{
    private ActionResultMessages outstandingChanges = new();
    private ActionResultMessages processedChanges = new();
    public Narrative CurrentNarrative { get; set; }
    public List<Location> Locations { get; set; }
    public string CurrentLocation { get; set; }
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
    }

    public void ClearCurrentNarrative()
    {
        CurrentNarrative = null;
    }

    public void AddResourceChange(ResourceOutcome resourceOutcome)
    {
        outstandingChanges.Resources.Add(resourceOutcome);
    }

    public void ApplyAllChanges()
    {
        while (true)
        {
            bool changeProcessed = false;

            // Process Resource Changes
            for (int i = 0; i < outstandingChanges.Resources.Count; i++)
            {
                ResourceOutcome resource = outstandingChanges.Resources[i];
                this.ModifyResource(resource.ResourceType, resource.Amount);
                processedChanges.Resources.Add(resource);
                outstandingChanges.Resources.RemoveAt(i--);
                changeProcessed = true;
            }
            // If no changes were processed, break the loop
            if (!changeProcessed)
            {
                break;
            }
        }
    }

    private bool ModifyResource(ResourceTypes resourceType, int amount)
    {
        if (resourceType == ResourceTypes.Money)
        {
            int newMoney = Math.Max(0, PlayerInfo.Money + amount);
            if (newMoney != PlayerInfo.Money)
            {
                PlayerInfo.Money = newMoney;
                return true;
            }
        }
        if (resourceType == ResourceTypes.Health)
        {
            int newHealth = Math.Clamp(PlayerInfo.Health + amount, 0, PlayerInfo.MaxHealth);
            if (newHealth != PlayerInfo.Health)
            {
                PlayerInfo.Health = newHealth;
                return true;
            }
        }
        if(resourceType == ResourceTypes.PhysicalEnergy)
        {
            int newEnergy = Math.Clamp(PlayerInfo.PhysicalEnergy + amount, 0, PlayerInfo.MaxPhysicalEnergy);
            if (newEnergy != PlayerInfo.PhysicalEnergy)
            {
                PlayerInfo.PhysicalEnergy = newEnergy;
                return true;
            }
        }
        if (resourceType == ResourceTypes.FocusEnergy)
        {
            int newEnergy = Math.Clamp(PlayerInfo.FocusEnergy + amount, 0, PlayerInfo.MaxFocusEnergy);
            if (newEnergy != PlayerInfo.FocusEnergy)
            {
                PlayerInfo.FocusEnergy = newEnergy;
                return true;
            }
        }
        if (resourceType == ResourceTypes.SocialEnergy)
        {
            int newEnergy = Math.Clamp(PlayerInfo.SocialEnergy + amount, 0, PlayerInfo.MaxSocialEnergy);
            if (newEnergy != PlayerInfo.SocialEnergy)
            {
                PlayerInfo.SocialEnergy = newEnergy;
                return true;
            }
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

    public ActionResult TravelTo(Location location)
    {
        CurrentLocation = location.Name;
        return ActionResult.Success($"Moved to {location.Name}.", new ActionResultMessages());
    }

    public List<Location> GetLocations()
    {
        return Locations;
    }
}