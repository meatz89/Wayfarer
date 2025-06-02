public interface IMechanicalEffect
{
    void Apply(EncounterState state);
    string GetDescriptionForPlayer();
}

public class CreateMemoryEffect : IMechanicalEffect
{
    private string memoryKey;
    private string memoryDescription;
    private int importance;
    private int expirationDays;

    public CreateMemoryEffect(string memoryKey, string memoryDescription, int importance, int expirationDays = -1)
    {
        this.memoryKey = memoryKey;
        this.memoryDescription = memoryDescription;
        this.importance = importance;
        this.expirationDays = expirationDays;
    }

    public void Apply(EncounterState state)
    {
        state.Player.AddMemory(memoryKey, memoryDescription, importance, expirationDays);
    }

    public string GetDescriptionForPlayer()
    {
        return "You will remember this event";
    }
}

// Create a memory check effect class
public class CheckMemoryEffect : IMechanicalEffect
{
    private string memoryKey;
    private IMechanicalEffect effectIfPresent;
    private IMechanicalEffect effectIfAbsent;

    public CheckMemoryEffect(string memoryKey, IMechanicalEffect effectIfPresent, IMechanicalEffect effectIfAbsent)
    {
        this.memoryKey = memoryKey;
        this.effectIfPresent = effectIfPresent;
        this.effectIfAbsent = effectIfAbsent;
    }

    public void Apply(EncounterState state)
    {
        if (state.Player.HasMemory(memoryKey))
        {
            effectIfPresent.Apply(state);
        }
        else
        {
            effectIfAbsent.Apply(state);
        }
    }

    public string GetDescriptionForPlayer()
    {
        return "Your past experiences affect this outcome";
    }

    public class CompleteGoalRequirementEffect : IMechanicalEffect
    {
        private string goalName;
        private string requirementName;

        public CompleteGoalRequirementEffect(string goalName, string requirementName)
        {
            this.goalName = goalName;
            this.requirementName = requirementName;
        }

        public void Apply(EncounterState state)
        {
            Goal goal = state.Player.ActiveGoals.FirstOrDefault(g => g.Name == goalName);
            if (goal != null)
            {
                goal.CompleteRequirement(requirementName);
            }
        }

        public string GetDescriptionForPlayer()
        {
            return $"Progress toward {goalName}";
        }
    }

    public class AddGoalEffect : IMechanicalEffect
    {
        private Goal goalToAdd;

        public AddGoalEffect(Goal goalToAdd)
        {
            this.goalToAdd = goalToAdd;
        }

        public void Apply(EncounterState state)
        {
            state.Player.AddGoal(goalToAdd);
        }

        public string GetDescriptionForPlayer()
        {
            return $"New goal: {goalToAdd.Name}";
        }
    }

    public class DiscoverRouteEffect : IMechanicalEffect
    {
        private TravelRoute routeToDiscover;

        public DiscoverRouteEffect(TravelRoute routeToDiscover)
        {
            this.routeToDiscover = routeToDiscover;
        }

        public void Apply(EncounterState state)
        {
            state.Player.AddKnownRoute(routeToDiscover);
        }

        public string GetDescriptionForPlayer()
        {
            return $"Discovered route to {routeToDiscover.Destination.Name}";
        }
    }

    public class LearnInformationEffect : IMechanicalEffect
    {
        private InformationItem informationToLearn;

        public LearnInformationEffect(InformationItem informationToLearn)
        {
            this.informationToLearn = informationToLearn;
        }

        public void Apply(EncounterState state)
        {
            state.Player.LearnInformation(informationToLearn);
        }

        public string GetDescriptionForPlayer()
        {
            return $"Learned information about {informationToLearn.Title}";
        }
    }

    public class SetFlagEffect : IMechanicalEffect
    {
        private FlagStates flagToSet;

        setFlagEffect(FlagStates flagToSet)
        {
            this.flagToSet = flagToSet;
        }

        public void Apply(EncounterState state)
        {
            state.FlagManager.SetFlag(flagToSet);
        }

        public string GetDescriptionForPlayer()
        {
            return $"Sets {flagToSet.ToString().SpaceBeforeCapitals()}";
        }
    }

    public class ModifyFocusEffect : IMechanicalEffect
    {
        private int amount;

        public ModifyFocusEffect(int amount)
        {
            this.amount = amount;
        }

        public void Apply(EncounterState state)
        {
            state.FocusPoints += amount;
            state.FocusPoints = Math.Max(0, Math.Min(state.FocusPoints, state.MaxFocusPoints));
        }

        public string GetDescriptionForPlayer()
        {
            if (amount > 0)
                return "Regain " + amount + " Focus";
            else
                return "Lose " + Math.Abs(amount) + " Focus";
        }
    }

    public class ClearFlagEffect : IMechanicalEffect
    {
        private FlagStates flagToClear;

        public ClearFlagEffect(FlagStates flagToClear)
        {
            this.flagToClear = flagToClear;
        }

        public void Apply(EncounterState state)
        {
            state.FlagManager.ClearFlag(flagToClear);
        }

        public string GetDescriptionForPlayer()
        {
            return $"Clears {flagToClear.ToString().SpaceBeforeCapitals()}";
        }
    }

    public class FocusChangeEffect : IMechanicalEffect
    {
        private int amount;

        public FocusChangeEffect(int amount)
        {
            this.amount = amount;
        }

        public void Apply(EncounterState state)
        {
            if (amount > 0)
            {
                // Cannot exceed maximum
                state.FocusPoints = Math.Min(state.FocusPoints + amount, state.MaxFocusPoints);
            }
            else
            {
                // Cannot go below zero
                state.FocusPoints = Math.Max(0, state.FocusPoints + amount);
            }
        }

        public string GetDescriptionForPlayer()
        {
            if (amount > 0)
            {
                return $"+{amount} Focus Point{(amount > 1 ? "s" : "")}";
            }
            else
            {
                return $"{amount} Focus Point{(amount < -1 ? "s" : "")}";
            }
        }
    }

    public class DurationAdvanceEffect : IMechanicalEffect
    {
        private int amount;

        public DurationAdvanceEffect(int amount)
        {
            this.amount = amount;
        }

        public void Apply(EncounterState state)
        {
            state.AdvanceDuration(amount);
        }

        public string GetDescriptionForPlayer()
        {
            return $"Advances encounterContext duration by {amount}";
        }
    }

    public class ProgressChangeEffect : IMechanicalEffect
    {
        private int amount;

        public ProgressChangeEffect(int amount)
        {
            this.amount = amount;
        }

        public void Apply(EncounterState state)
        {
            state.AddProgress(amount);
        }

        public string GetDescriptionForPlayer()
        {
            if (amount > 0)
            {
                return $"+{amount} Progress";
            }
            else
            {
                return $"{amount} Progress";
            }
        }
    }

    public class CompoundEffect : IMechanicalEffect
    {
        private List<IMechanicalEffect> effects;

        public CompoundEffect(List<IMechanicalEffect> effects)
        {
            this.effects = effects;
        }

        public void Apply(EncounterState state)
        {
            foreach (IMechanicalEffect effect in effects)
            {
                effect.Apply(state);
            }
        }

        public string GetDescriptionForPlayer()
        {
            List<string> descriptions = new List<string>();

            foreach (IMechanicalEffect effect in effects)
            {
                descriptions.Add(effect.GetDescriptionForPlayer());
            }

            return string.Join(", ", descriptions);
        }
    }
}