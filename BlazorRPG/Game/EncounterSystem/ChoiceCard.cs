/// <summary>
/// Standard choice implementation - either builds momentum or pressure
/// </summary>
public class ChoiceCard
{
    public string Name { get; }
    public string Description { get; }
    public bool IsBlocked { get; private set; }
    public CardTiers Tier { get; }
    public EffectTypes EffectType { get; }
    public int BaseEffectValue { get; }
    public ApproachTags Approach { get; }
    public FocusTags Focus { get; }
    public int OptimalFocusValue { get; }
    public IReadOnlyList<TagModification> TagModifications { get; }
    public StrategicEffect StrategicEffect { get; set;  }

    public string GetDetails()
    {
        string desc = $"Tier {(int)Tier} (Requires: {Focus} {OptimalFocusValue})";
        return desc;
    }

    public override string ToString()
    {
        string desc = $"Tier {(int)Tier} (Requires: {Focus} {OptimalFocusValue})";
        return desc;
    }

    public object GetName()
    {
        var name = $"{Name} ({Approach} - {Focus})";
        return name;
    }

    public ChoiceCard(string name, string description, 
        EffectTypes effectType, CardTiers tier, int baseEffectValue, StrategicEffect strategicEffect, IReadOnlyList<TagModification> tagModifications,
        ApproachTags approach, FocusTags focus, int focusPosition)
    {
        Approach = approach;
        OptimalFocusValue = focusPosition;
        Name = name;
        Description = description;
        Focus = focus;
        EffectType = effectType;
        Tier = tier;
        BaseEffectValue = baseEffectValue;
        TagModifications = tagModifications;
        StrategicEffect = strategicEffect;
    }
}

