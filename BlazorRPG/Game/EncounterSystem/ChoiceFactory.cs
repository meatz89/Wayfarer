/// <summary>
/// Factory for creating common choice types
/// </summary>
public static class ChoiceFactory
{
    // Create a standard momentum choice
    public static Choice CreateMomentumChoice(string name, string description,FocusTags focus,
                                                params TagModification[] tagModifications)
    {
        return new Choice(name, description, focus, EffectTypes.Momentum, tagModifications);
    }

    // Create a standard pressure choice
    public static Choice CreatePressureChoice(string name, string description,
                                                FocusTags focus,
                                                params TagModification[] tagModifications)
    {
        return new Choice(name, description, focus, EffectTypes.Pressure, tagModifications);
    }

    // Create a tag requirement function for a specific approach tag threshold
    public static Func<BaseTagSystem, bool> EncounterStateTagRequirement(EncounterStateTags tag, int threshold)
    {
        return tagSystem => tagSystem.GetEncounterStateTagValue(tag) >= threshold;
    }

    // Create a tag requirement function for a specific focus tag threshold
    public static Func<BaseTagSystem, bool> FocusTagRequirement(FocusTags tag, int threshold)
    {
        return tagSystem => tagSystem.GetFocusTagValue(tag) >= threshold;
    }

}
