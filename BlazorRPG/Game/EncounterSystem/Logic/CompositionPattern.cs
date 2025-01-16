public class CompositionPattern
{
    public int PrimaryCount { get; set; }
    public ChoiceArchetypes PrimaryArchetype { get; set; }
    public int SecondaryCount { get; set; }
    public ChoiceArchetypes SecondaryArchetype { get; set; }

    public CompositionPattern()
    {

    }

    public CompositionPattern(
        int primaryCount,
        ChoiceArchetypes primaryArchetype,
        int secondaryCount,
        ChoiceArchetypes secondaryArchetype)
    {
        PrimaryCount = primaryCount;
        PrimaryArchetype = primaryArchetype;
        SecondaryCount = secondaryCount;
        SecondaryArchetype = secondaryArchetype;
    }
}