using BlazorRPG.Game.EncounterManager;
using BlazorRPG.Game.EncounterManager.NarrativeAi;

public class EncounterResult
{
    public EncounterManager Encounter;
    public EncounterResults EncounterResults;
    public string EncounterEndMessage;

    public NarrativeResult NarrativeResult { get; internal set; }
}
