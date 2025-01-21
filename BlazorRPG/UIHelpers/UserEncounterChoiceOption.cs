public record UserEncounterChoiceOption(
    int Index,
    string ChoiceType,
    string Description,
    string Narrative,
    LocationNames LocationName,
    string locationSpotName,
    Encounter Encounter,
    EncounterStage EncounterStage,
    EncounterChoice EncounterChoice)
{
    public string Display()
    {
        return $"{Index}. {Description}";
    }
}
