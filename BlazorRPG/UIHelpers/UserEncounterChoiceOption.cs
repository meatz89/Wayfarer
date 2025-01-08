public record UserEncounterChoiceOption(
    int Index,
    string Description,
    LocationNames LocationName,
    Encounter Encounter,
    EncounterStage EncounterStage,
    EncounterChoice EncounterChoice)
{
    public string Display()
    {
        return $"{Index}. {Description}";
    }
}
