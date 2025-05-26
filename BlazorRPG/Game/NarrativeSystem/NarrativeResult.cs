public class NarrativeResult
{
    public string SceneNarrative { get; private set; }
    public string ObjectDescription { get; private set; }
    public List<AiChoice> Choices { get; private set; }
    public List<ChoiceProjection> Projections { get; private set; }
    public bool IsEncounterOver { get; private set; }
    public EncounterOutcomes Outcome { get; private set; }

    public NarrativeResult(
        string sceneNarrative,
        string objectDescription,
        List<AiChoice> choices,
        List<ChoiceProjection> projections)
    {
        SceneNarrative = sceneNarrative;
        ObjectDescription = objectDescription;
        Choices = choices;
        Projections = projections;
        IsEncounterOver = false;
        Outcome = EncounterOutcomes.None;
    }

    public void SetOutcome(EncounterOutcomes outcome)
    {
        Outcome = outcome;
    }

    public void SetIsEncounterOver(bool isEncounterOver)
    {
        IsEncounterOver = isEncounterOver;
    }

    public override string ToString()
    {
        return SceneNarrative;
    }
}