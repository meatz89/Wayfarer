public class NarrativeResult
{
    public string SceneNarrative { get; private set; }
    public string ActionDescription { get; private set; }
    public List<AiChoice> Choices { get; private set; }
    public List<ChoiceProjection> Projections { get; private set; }
    public string LastchoiceDescription { get; private set; } 
    public EncounterOutcomes Outcome { get; private set; }
    public bool IsEncounterOver { get; private set; }

    public NarrativeResult(
        string sceneNarrative,
        string actionDescription,
        List<AiChoice> choices,
        List<ChoiceProjection> projections)
    {
        SceneNarrative = sceneNarrative;
        ActionDescription = actionDescription;
        Choices = choices;
        Projections = projections;
    }

    public void SetOutcome(EncounterOutcomes outcome)
    {
        Outcome = outcome;
    }

    public void SetIsEncounterOver(bool isEncounterOver)
    {
        IsEncounterOver = isEncounterOver;
    }
}