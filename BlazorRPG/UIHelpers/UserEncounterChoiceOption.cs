public record UserEncounterChoiceOption(
    int Index,
    string ChoiceShortName,
    string ChoiceDescription,
    string LocationName,
    string locationSpotName,
    EncounterResult encounterResult,
    NarrativeResult NarrativeResult,
    AiChoice Choice)
{
}
