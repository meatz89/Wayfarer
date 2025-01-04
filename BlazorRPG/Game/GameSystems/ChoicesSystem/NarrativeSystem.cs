public class NarrativeSystem
{
    private readonly GameState gameState;
    private readonly List<Narrative> narratives;

    public NarrativeSystem(
        GameState gameState,
        GameContentProvider contentProvider
        )
    {
        this.gameState = gameState;
        this.narratives = contentProvider.GetNarratives();
    }

    public Narrative GetAvailableNarrative(BasicActionTypes action, LocationNames location, LocationSpotNames locationSpot)
    {
        return narratives.FirstOrDefault(x =>
            x.ActionType == action &&
            x.LocationName == location &&
            x.LocationSpot == locationSpot);
    }

    public void ExecuteChoice(Narrative narrative, NarrativeChoice narrativeChoice)
    {
        NarrativeStage stage = GetCurrentStage(narrative);
        NarrativeState changes = narrativeChoice.NarrativeStateChanges;
        narrative.InitialState.ApplyChanges(changes);

    }

    public List<NarrativeChoice> GetChoicesForCurrentStage(Narrative narrative)
    {
        NarrativeStage stage = GetCurrentStage(narrative);
        List<NarrativeChoice> choices = stage.Choices;
        return choices;
    }

    public bool GetNextStage(Narrative narrative)
    {
        if (IsComplete(narrative))
        {
            return false;
        }

        narrative.currentStage++;
        return true;
    }

    public bool IsComplete(Narrative narrative)
    {
        if (narrative.currentStage >= narrative.numberOfStages) return true;
        return false;
    }

    public NarrativeStage GetCurrentStage(Narrative narrative)
    {
        return narrative.Stages[narrative.currentStage - 1];
    }

    public List<NarrativeChoice> GetCurrentStageChoices(Narrative narrative)
    {
        NarrativeStage stage = GetCurrentStage(narrative);
        List<NarrativeChoice> options = stage.Choices;
        return options;
    }

    public void SetActiveNarrative(Narrative narrative)
    {
        gameState.Actions.SetActiveNarrative(narrative);
    }
}