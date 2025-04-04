public class NarrativeResult
{
    public string SceneNarrative { get; }
    public List<IChoice> Choices { get; }
    public List<ChoiceProjection> Projections { get; }
    public Dictionary<IChoice, ChoiceNarrative> ChoiceDescriptions { get; }
    public ChoiceNarrative LastChoiceNarrative { get; }
    public bool IsEncounterOver { get; private set; }
    public EncounterOutcomes? Outcome { get; private set; }

    public NarrativeResult(
        string narrative,
        string initialGoal,
        List<IChoice> choices,
        List<ChoiceProjection> projections,
        Dictionary<IChoice, ChoiceNarrative> choiceDescriptions,
        ChoiceNarrative lastChoiceNarrative)
    {
        SceneNarrative = narrative;
        Choices = choices;
        Projections = projections;
        ChoiceDescriptions = choiceDescriptions;
        LastChoiceNarrative = lastChoiceNarrative;
        if (LastChoiceNarrative == null)
            LastChoiceNarrative = new ChoiceNarrative(initialGoal, "");
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
