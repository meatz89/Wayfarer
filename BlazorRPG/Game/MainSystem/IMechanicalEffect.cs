using System.Text;

public interface IMechanicalEffect
{
    void Apply(EncounterState state);
    string GetDescriptionForPlayer();
}


public class SetFlagEffect : IMechanicalEffect
{
    private readonly FlagStates flagToSet;

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
        return $"Sets {flagToSet}";
    }
}

public class ClearFlagEffect : IMechanicalEffect
{
    private readonly FlagStates flagToClear;

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
        return $"Clears {flagToClear}";
    }
}

public class FocusChangeEffect : IMechanicalEffect
{
    private readonly int amount;

    public FocusChangeEffect(int amount)
    {
        this.amount = amount;
    }

    public void Apply(EncounterState state)
    {
        state.FocusPoints = Math.Max(0, Math.Min(state.MaxFocusPoints, state.FocusPoints + amount));
    }

    public string GetDescriptionForPlayer()
    {
        return amount >= 0 ? $"Gain {amount} Focus" : $"Lose {Math.Abs(amount)} Focus";
    }
}

public class DurationAdvanceEffect : IMechanicalEffect
{
    private readonly int amount;

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

public class NextCheckModifierEffect : IMechanicalEffect
{
    private readonly int modifierValue;

    public NextCheckModifierEffect(int modifierValue)
    {
        this.modifierValue = modifierValue;
    }

    public void Apply(EncounterState state)
    {
        state.SetNextCheckModifier(modifierValue);
    }

    public string GetDescriptionForPlayer()
    {
        string direction = modifierValue >= 0 ? "+" : "";
        return $"{direction}{modifierValue} to next skill check";
    }
}

public class CurrencyChangeEffect : IMechanicalEffect
{
    private readonly int minAmount;
    private readonly int maxAmount;

    public CurrencyChangeEffect(int amount)
    {
        minAmount = amount;
        maxAmount = amount;
    }

    public CurrencyChangeEffect(int minAmount, int maxAmount)
    {
        this.minAmount = minAmount;
        this.maxAmount = maxAmount;
    }

    public void Apply(EncounterState state)
    {
        int amount = minAmount == maxAmount ? minAmount : state.GetDeterministicRandom(minAmount, maxAmount + 1);
        state.Player.ModifyCoins(amount);
    }

    public string GetDescriptionForPlayer()
    {
        if (minAmount == maxAmount)
        {
            return minAmount >= 0 ? $"Gain {minAmount} coins" : $"Lose {Math.Abs(minAmount)} coins";
        }
        else
        {
            return minAmount >= 0 ? $"Gain {minAmount}-{maxAmount} coins" : $"Lose {Math.Abs(maxAmount)}-{Math.Abs(minAmount)} coins";
        }
    }
}

public class RelationshipModifierEffect : IMechanicalEffect
{
    private readonly int amount;
    private readonly string source;

    public RelationshipModifierEffect(int amount, string source)
    {
        this.amount = amount;
        this.source = source;
    }

    public void Apply(EncounterState state)
    {
        if (state.CurrentNPC != null)
        {
            state.Player.ModifyRelationship(state.CurrentNPC.Id, amount, source);
        }
    }

    public string GetDescriptionForPlayer()
    {
        return amount > 0 ? $"Improves relationship by {amount}" : $"Worsens relationship by {Math.Abs(amount)}";
    }
}

public class RecoveryEffect : IMechanicalEffect
{
    private readonly bool success;

    public RecoveryEffect(bool success)
    {
        this.success = success;
    }

    public void Apply(EncounterState state)
    {
        if (success)
        {
            state.FocusPoints = Math.Min(state.FocusPoints + 1, state.MaxFocusPoints);
            state.ConsecutiveRecoveryCount++;
        }
        else
        {
            state.AdvanceDuration(1);
        }
    }

    public string GetDescriptionForPlayer()
    {
        return success ? "Regain 1 Focus Point" : "Wastes valuable time";
    }
}

public class CompoundEffect : IMechanicalEffect
{
    private readonly List<IMechanicalEffect> effects;

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
        StringBuilder description = new StringBuilder();
        for (int i = 0; i < effects.Count; i++)
        {
            if (i > 0)
            {
                description.Append(", ");
            }
            description.Append(effects[i].GetDescriptionForPlayer());
        }
        return description.ToString();
    }
}