public record UserEncounterChoiceOption(
    int Index,
    string Description,
    string Narrative,
    string LocationName,
    string locationSpotName,
    EncounterManager encounter,
    NarrativeResult narrativeResult,
    ChoiceCard Choice)
{
    public string Display()
    {
        return $"{Index}. {Description}";
    }
}
