public record UserEncounterChoiceOption(
    int Index,
    string ChoiceShortName,
    string ChoiceDescription,
    string LocationName,
    string locationSpotName,
    EncounterResult encounterResult,
    AIGameMasterResponse AIResponse,
    EncounterChoice Choice)
{
}
