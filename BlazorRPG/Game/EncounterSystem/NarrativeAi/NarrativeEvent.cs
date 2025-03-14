/// <summary>
/// Represents a single narrative event in the encounter timeline
/// </summary>
public class NarrativeEvent
{
    public int TurnNumber { get; }
    public string SceneDescription { get; }
    public IChoice ChosenOption { get; }
    public ChoiceNarrative ChoiceNarrative { get; }
    public string Outcome { get; }
    public Dictionary<IChoice, ChoiceNarrative> AvailableChoiceDescriptions { get; }

    public NarrativeEvent(
        int turnNumber,
        string sceneDescription,
        IChoice chosenOption = null,
        ChoiceNarrative choiceNarrative = null,
        string outcome = null,
        Dictionary<IChoice, ChoiceNarrative> availableChoiceDescriptions = null)
    {
        TurnNumber = turnNumber;
        SceneDescription = sceneDescription;
        ChosenOption = chosenOption;
        ChoiceNarrative = choiceNarrative;
        Outcome = outcome;
        AvailableChoiceDescriptions = availableChoiceDescriptions ?? new Dictionary<IChoice, ChoiceNarrative>();
    }
}
