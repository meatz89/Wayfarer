using System.Text;

public interface IMechanicalEffect
{
    void Apply(EncounterState state);
    string GetDescriptionForPlayer();
}


// Concrete effect implementations
public class SetFlagEffect : IMechanicalEffect
{
    private FlagStates flagToSet;

    public SetFlagEffect(FlagStates flagToSet)
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
        return $"Advances encounter duration by {amount}";
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
