public record UserEncounterChoiceOption(
    int Index,
    string ChoiceShortName,
    string ChoiceDescription,
    string LocationName,
    string locationSpotName,
    EncounterResult encounterResult,
    BeatResponse AIResponse,
    EncounterChoice Choice)
{
}
