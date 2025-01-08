public record UserEncounterChoiceOption(
    int Index, string Description, Encounter Encounter,
    EncounterStage EncounterStage, EncounterChoice EncounterChoice)
{
    public string Display()
    {
        return $"{Index}. {Description}";
    }
}
