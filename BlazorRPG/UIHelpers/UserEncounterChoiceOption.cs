using BlazorRPG.Game.EncounterManager;

public record UserEncounterChoiceOption(
    int Index,
    string Description,
    string Narrative,
    LocationNames LocationName,
    string locationSpotName,
    Encounter Encounter,
    IChoice Choice)
{
    public string Display()
    {
        return $"{Index}. {Description}";
    }
}
