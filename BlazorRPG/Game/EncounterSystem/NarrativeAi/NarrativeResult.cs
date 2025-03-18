public class NarrativeResult
{
    public string SceneNarrative { get; }
    public List<IChoice> Choices { get; }
    public List<ChoiceProjection> Projections { get; }
    public Dictionary<IChoice, ChoiceNarrative> ChoiceDescriptions { get; }
    public bool IsEncounterOver { get; private set; }
    public EncounterOutcomes? Outcome { get; private set; }

    public NarrativeResult(
        string narrative,
        List<IChoice> choices,
        List<ChoiceProjection> projections,
        Dictionary<IChoice, ChoiceNarrative> choiceDescriptions
        )
    {
        SceneNarrative = narrative;
        Choices = choices;
        Projections = projections;
        ChoiceDescriptions = choiceDescriptions;
    }

    public void SetOutcome(EncounterOutcomes outcome)
    {
        this.Outcome = outcome;
    }

    public void SetIsEncounterOver(bool isOver)
    {
        this.IsEncounterOver = isOver;
    }
}
