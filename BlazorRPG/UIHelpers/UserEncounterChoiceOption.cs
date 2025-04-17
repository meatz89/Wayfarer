public record UserEncounterChoiceOption(
    int Index,
    string ChoiceDescription,
    string Narrative,
    string LocationName,
    string locationSpotName,
    EncounterResult encounterResult,
    NarrativeResult NarrativeResult,
    ChoiceCard Choice)
{
    public string Display()
    {
        return $"{Index}. {ChoiceDescription}";
    }
}
