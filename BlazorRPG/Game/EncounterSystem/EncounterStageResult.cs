public class EncounterStageResult
{
    public ActionImplementation ActionImplementation { get; set; }
    public ActionResults ActionResult { get; set; }
    public List<ActionCardDefinition> Choices { get; set; }
    public List<ChoiceProjection> Projections { get; set; }
    public string SceneNarrative { get; set; }
    public Dictionary<ActionCardDefinition, ChoiceNarrative> ChoiceDescriptions { get; set; }
    public EncounterOutcomes? Outcome { get; set; }
    public string EncounterEndMessage { get; set; }
    public string LocationName { get; set; }
    public string LocationSpotName { get; set; }

    public EncounterStageResult Copy()
    {
        return new EncounterStageResult()
        {
            ActionImplementation = ActionImplementation,
            ActionResult = ActionResult,
            Choices = Choices?.Select(c => c).ToList(),
            Projections = Projections?.Select(p => p).ToList(),
            SceneNarrative = SceneNarrative,
            ChoiceDescriptions = ChoiceDescriptions?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            Outcome = Outcome,
            EncounterEndMessage = EncounterEndMessage,
            LocationName = LocationName,
            LocationSpotName = LocationSpotName
        };
    }
}