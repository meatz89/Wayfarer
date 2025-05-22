public class ChoiceCardSelector
{
    private readonly ChoiceRepository _narrativeChoiceRepository;

    public ChoiceCardSelector(ChoiceRepository choiceRepository)
    {
        _narrativeChoiceRepository = choiceRepository;
    }

    public List<EncounterOption> SelectChoices(EncounterState state, PlayerState playerState)
    {
        // Get choices from the repository based on encounter state
        List<EncounterOption> availableChoices = _narrativeChoiceRepository.GetForEncounter(state);

        // Apply location property modifiers
        List<EncounterOption> modifiedChoices = ApplyLocationModifiers(availableChoices, playerState.CurrentLocation);

        // Filter by Focus affordability
        List<EncounterOption> affordableChoices = FilterByFocusAffordability(modifiedChoices, state);

        return affordableChoices;
    }

    private List<EncounterOption> ApplyLocationModifiers(List<EncounterOption> choices, Location location)
    {
        List<EncounterOption> modifiedChoices = new List<EncounterOption>();

        foreach (EncounterOption choice in choices)
        {
            // Create a copy to avoid modifying the original
            EncounterOption modifiedChoice = new EncounterOption(choice.Id, choice.Name)
            {
                Description = choice.Description,
                Skill = choice.Skill,
                Difficulty = choice.Difficulty,
                SuccessProgress = choice.SuccessProgress,
                FailureProgress = choice.FailureProgress,
                FocusCost = choice.FocusCost,
                ActionType = choice.ActionType,
                TokenGeneration = new Dictionary<AspectTokenTypes, int>(choice.TokenGeneration),
                TokenCosts = new Dictionary<AspectTokenTypes, int>(choice.TokenCosts),
                NegativeConsequenceType = choice.NegativeConsequenceType,
                Tags = new List<string>(choice.Tags)
            };

            // Apply location modifier to difficulty
            modifiedChoice.LocationModifier = ChoiceProjectionService.GetLocationPropertyModifier(choice.Skill, location);
            modifiedChoice.Difficulty += modifiedChoice.LocationModifier;

            modifiedChoices.Add(modifiedChoice);
        }

        return modifiedChoices;
    }

    private List<EncounterOption> FilterByFocusAffordability(List<EncounterOption> choices, EncounterState state)
    {
        List<EncounterOption> encounterOptions = choices.Where(choice => state.CanAffordFocusCost(choice.FocusCost)).ToList();
        return encounterOptions;
    }
}