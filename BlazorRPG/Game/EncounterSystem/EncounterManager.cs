using BlazorRPG.Game.EncounterManager.NarrativeAi;

namespace BlazorRPG.Game.EncounterManager
{
    /// <summary>
    /// Manages the overall encounter flow
    /// </summary>
    public class EncounterManager
    {
        public bool useAiNarrative = true;

        public ActionImplementation ActionImplementation;
        private readonly CardSelectionAlgorithm _cardSelector;
        private readonly NarrativePresenter _narrativePresenter;
        public EncounterState State;

        private INarrativeAIService _narrativeService;
        private NarrativeContext _narrativeContext;


        public List<IChoice> CurrentChoices = new List<IChoice>();

        public EncounterManager(
            ActionImplementation actionImplementation,
            CardSelectionAlgorithm cardSelector,
            NarrativePresenter narrativePresenter)
        {
            ActionImplementation = actionImplementation;
            _cardSelector = cardSelector;
            _narrativePresenter = narrativePresenter;
        }

        // Get the current choices for the player
        public List<IChoice> GetCurrentChoices()
        {
            return CurrentChoices;
        }

        public void GenerateChoices()
        {
            CurrentChoices = _cardSelector.SelectChoices(State);
        }

        // Apply a choice using its projection
        private ChoiceOutcome ApplyChoiceProjection(ChoiceProjection projection)
        {
            State.ApplyChoiceProjection(projection);

            return new ChoiceOutcome(
                projection.MomentumGained,
                projection.PressureBuilt,
                projection.NarrativeDescription,
                projection.EncounterWillEnd,
                projection.ProjectedOutcome
            );
        }

        // Get formatted choice descriptions
        public string GetFormattedChoiceDescription(IChoice choice)
        {
            return _narrativePresenter.FormatChoiceDescription(choice, State.Location.Style);
        }

        // Get current encounter state information
        public EncounterStatus GetEncounterStatus()
        {
            return new EncounterStatus(
                State.CurrentTurn,
                State.Location.Duration,
                State.Momentum,
                State.Pressure,
                State.TagSystem.GetAllApproachTags(),
                State.TagSystem.GetAllFocusTags(),
                State.ActiveTags.Select(t => t.Name).ToList()
            );
        }

        /// <summary>
        /// Gets the current narrative context
        /// </summary>
        public NarrativeContext GetNarrativeContext()
        {
            return _narrativeContext;
        }

        public ChoiceProjection ProjectChoice(IChoice choice)
        {
            ChoiceProjection projection = State.CreateChoiceProjection(choice);

            // Add narrative description
            projection.NarrativeDescription = _narrativePresenter.FormatOutcome(
                choice,
                State.Location.Style,
                projection.MomentumGained,
                projection.PressureBuilt
            );

            return projection;
        }


        // Start a new encounter at a specific location
        private void StartEncounter(LocationInfo location)
        {
            State = new EncounterState(location);

            // Activate initial tags
            State.UpdateActiveTags(location.AvailableTags);
        }

        /// <summary>
        /// Starts an encounter with narrative AI generation for the introduction
        /// </summary>
        public async Task<NarrativeResult> StartEncounterWithNarrativeAsync(
            LocationInfo location,
            string incitingAction,
            INarrativeAIService narrativeService)
        {
            // Store the narrative service
            _narrativeService = narrativeService;

            // Start the encounter mechanically
            StartEncounter(location);

            // Create narrative context
            _narrativeContext = new NarrativeContext(location.Name, incitingAction, location.Style);

            // Generate introduction
            EncounterStatus status = GetEncounterStatus();

            string introduction = "introduction";
            if (useAiNarrative)
            {
                introduction =
                    await _narrativeService.GenerateIntroductionAsync(
                        location.Name,
                        incitingAction,
                        status);
            }

            // Get available choices
            GenerateChoices();
            List<IChoice> choices = GetCurrentChoices();
            List<ChoiceProjection> projections = choices.Select(ProjectChoice).ToList();

            // Generate choice descriptions
            Dictionary<IChoice, ChoiceNarrative> choiceDescriptions = null;
            if (useAiNarrative)
            {
                choiceDescriptions =
                    await _narrativeService.GenerateChoiceDescriptionsAsync(
                        _narrativeContext,
                        choices,
                        projections,
                        status);
            }

            // Create first narrative event
            NarrativeEvent firstEvent = new NarrativeEvent(
                State.CurrentTurn,
                introduction,
                null,
                null,
                null,
                choiceDescriptions);

            _narrativeContext.AddEvent(firstEvent);

            // Return the narrative result
            return new NarrativeResult(
                introduction,
                choices,
                projections,
                choiceDescriptions);
        }

        /// <summary>
        /// Applies the player character (PC) choice and generates narrative for the result
        /// </summary>
        public async Task<NarrativeResult> ApplyChoiceWithNarrativeAsync(
            IChoice choice,
            ChoiceNarrative choiceDescription)
        {
            // Get projection
            ChoiceProjection projection = ProjectChoice(choice);

            // Apply the choice
            ChoiceOutcome outcome = ApplyChoiceProjection(projection);

            // Get status after the choice
            EncounterStatus newStatus = GetEncounterStatus();

            // Generate narrative for the reaction and new scene
            string narrative = "Continued Narrative";

            if (useAiNarrative)
            {
                narrative =
                    await _narrativeService.GenerateReactionAndSceneAsync(
                        _narrativeContext,
                        choice,
                        choiceDescription,
                        outcome,
                        newStatus);
            }

            // Create the narrative event for this turn
            NarrativeEvent narrativeEvent = new NarrativeEvent(
                State.CurrentTurn - 1, // The turn counter increases after application
                narrative,
                choice,
                choiceDescription,
            outcome.Description);

            _narrativeContext.AddEvent(narrativeEvent);

            // If the encounter is over, return the outcome
            if (outcome.IsEncounterOver)
            {
                return new NarrativeResult(
                    narrative,
                    new List<IChoice>(),
                    new List<ChoiceProjection>(),
                    new Dictionary<IChoice, ChoiceNarrative>(),
                    outcome.IsEncounterOver,
                    outcome.Outcome);
            }

            // Get the new choices and projections
            GenerateChoices();
            List<IChoice> newChoices = GetCurrentChoices();
            List<ChoiceProjection> newProjections = newChoices.Select(ProjectChoice).ToList();

            // Generate descriptive narratives for each choice
            Dictionary<IChoice, ChoiceNarrative> newChoiceDescriptions = null;
            if(useAiNarrative)
            {
                newChoiceDescriptions =
                    await _narrativeService.GenerateChoiceDescriptionsAsync(
                        _narrativeContext,
                        newChoices,
                        newProjections,
                        newStatus);
            }
            
            // Add the choice descriptions to the latest event
            narrativeEvent.AvailableChoiceDescriptions.Clear();
            foreach (KeyValuePair<IChoice, ChoiceNarrative> kvp in newChoiceDescriptions)
            {
                narrativeEvent.AvailableChoiceDescriptions[kvp.Key] = kvp.Value;
            }

            // Return the narrative result
            return new NarrativeResult(
                narrative,
                newChoices,
                newProjections,
                newChoiceDescriptions);
        }

    }
}