public record UserEncounterChoiceOption(int Index, string Description, bool IsDisabled, Encounter Encounter, EncounterStage EncounterStage, EncounterChoice EncounterChoice, LocationNames Location, string LocationSpot, CharacterNames Character)
{
    public string Display()
    {
        return $"{Index}. {Description}";
    }
}
