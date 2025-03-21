public record UserEncounterChoiceOption(
    int Index,
    string Description,
    string Narrative,
    LocationNames LocationName,
    string locationSpotName,
    EncounterManager encounter,
    IChoice Choice)
{
    public string Display()
    {
        return $"{Index}. {Description}";
    }
}
