public record UserEncounterChoiceOption(
    int Index,
    string ChoiceShortName,
    string ChoiceNarrative,
    string Narrative,
    string LocationName,
    string locationSpotName,
    EncounterResult encounterResult,
    NarrativeResult NarrativeResult,
    EncounterOption Choice)
{
}
