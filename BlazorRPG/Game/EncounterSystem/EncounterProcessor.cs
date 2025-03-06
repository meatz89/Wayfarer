/// <summary>
/// Extended encounter processor with choice projection as single source of truth
/// </summary>
public class EncounterProcessor
{
    public EncounterState State;
    private readonly StrategicLayer _strategicLayer;
    private readonly NarrativeChoiceGenerator _narrativeGenerator;
    private readonly SpecialTagEffectProcessor _specialEffectProcessor;
    private readonly LocationStrategicProperties _locationProperties;
    private readonly EncounterTagRepository _tagRepository;

    public EncounterProcessor(EncounterState state, LocationStrategicProperties locationProperties,
                             EncounterTagRepository tagRepository)
    {
        State = state;
        _locationProperties = locationProperties;
        _tagRepository = tagRepository;
        _strategicLayer = new StrategicLayer(locationProperties, tagRepository);
        _narrativeGenerator = new NarrativeChoiceGenerator(new ChoiceRepository());
        _specialEffectProcessor = new SpecialTagEffectProcessor();

        // Initialize state
        InitializeState();
    }

    /// <summary>
    /// Project the outcome of a choice without changing state
    /// </summary>
    public ChoiceProjection ProjectChoice(Choice choice)
    {
        return _strategicLayer.ProjectChoice(choice, State);
    }

    /// <summary>
    /// Process a player's choice - now uses projection as single source of truth
    /// </summary>
    public void ProcessChoice(Choice choice)
    {
        // Update narrative tags
        State.ApproachTypesDic[choice.ApproachType] += 1;
        State.FocusTypesDic[choice.FocusType] += 1;

        // Step 1: Generate a projection of the choice's effects
        ChoiceProjection projection = ProjectChoice(choice);

        // Step 2: Apply the projection to the strategic layer
        _strategicLayer.ApplyProjection(projection);

        // Step 3: Apply the projection to the encounter state
        projection.ApplyToState(State);

        // Step 4: Handle special tag effects that need encounter processor integration
        _specialEffectProcessor.ProcessSpecialTagEffects(_strategicLayer.GetActiveTags(), State, projection);

        // Step 5: Increment turn counter
        State.CurrentTurn += 1;

        // Step 6: Generate next set of choices
        GenerateChoices();
    }

    /// <summary>
    /// Check for encounter success/failure conditions
    /// </summary>
    public EncounterStatus CheckEncounterConditions()
    {
        // Check pressure failure condition
        if (State.Pressure >= 10)
        {
            State.EncounterStatus = EncounterStatus.Failed;
            return State.EncounterStatus;
        }

        // Check if we've reached the end of turns
        if (State.CurrentTurn >= State.MaxTurns)
        {
            // Determine success level based on momentum thresholds
            if (State.Momentum >= 12)
            {
                State.EncounterStatus = EncounterStatus.ExceptionalSuccess;
            }
            else if (State.Momentum >= 10)
            {
                State.EncounterStatus = EncounterStatus.StandardSuccess;
            }
            else if (State.Momentum >= 8)
            {
                State.EncounterStatus = EncounterStatus.PartialSuccess;
            }
            else
            {
                State.EncounterStatus = EncounterStatus.Failed;
            }
        }

        return State.EncounterStatus;
    }

    /// <summary>
    /// Initialize encounter state
    /// </summary>
    private void InitializeState()
    {
        State.EncounterStatus = EncounterStatus.InProgress;
        State.CurrentTurn = 0;

        // Generate initial choices
        GenerateChoices();
    }

    /// <summary>
    /// Project outcomes for all available choices
    /// </summary>
    public Dictionary<Choice, ChoiceProjection> ProjectAllChoices()
    {
        Dictionary<Choice, ChoiceProjection> projections = new Dictionary<Choice, ChoiceProjection>();

        foreach (Choice choice in State.CurrentChoices)
        {
            projections[choice] = ProjectChoice(choice);
        }

        return projections;
    }

    /// <summary>
    /// Get active tags for UI display
    /// </summary>
    public List<EncounterTag> GetAllAvailableTags()
    {
        return _strategicLayer.GetAllAvailableTags();
    }

    /// <summary>
    /// Get active tags for UI display
    /// </summary>
    public List<EncounterTag> GetActiveTags()
    {
        return _strategicLayer.GetActiveTags();
    }

    /// <summary>
    /// Get current signature for UI display
    /// </summary>
    public StrategicSignature GetSignature()
    {
        return _strategicLayer.GetSignature();
    }

    /// <summary>
    /// Get current encounter state for UI display
    /// </summary>
    public EncounterState GetState()
    {
        return State;
    }

    /// <summary>
    /// Generate new choices based on current state
    /// </summary>
    public void GenerateChoices()
    {
        List<Choice> nextChoices = _narrativeGenerator.GenerateChoiceSet(State);
        State.NarrativePhase = _narrativeGenerator.NarrativePhase;
        State.CurrentChoices = nextChoices;
    }

    /// <summary>
    /// Get comprehensive information about all tags for this location
    /// </summary>
    public LocationTagInformation GetLocationTagInformation()
    {
        LocationTagInformation info = new LocationTagInformation
        {
            LocationId = _locationProperties.LocationId,
            LocationName = _locationProperties.LocationName
        };

        // Get all active tags
        List<EncounterTag> activeTags = _strategicLayer.GetActiveTags();

        // Add player tags
        foreach (string tagId in _locationProperties.AvailableTagIds)
        {
            EncounterTag tag = _tagRepository.GetTag(tagId);
            if (tag != null)
            {
                TagDetailInfo tagInfo = new TagDetailInfo(tag);
                tagInfo.IsActive = activeTags.Any(t => t.Id == tag.Id);
                info.PlayerTags.Add(tagInfo);

                if (tagInfo.IsActive)
                {
                    info.ActiveTags.Add(tagInfo);
                }
            }
        }

        // Add location reaction tags
        foreach (string tagId in _locationProperties.LocationReactionTagIds)
        {
            EncounterTag tag = _tagRepository.GetTag(tagId);
            if (tag != null)
            {
                TagDetailInfo tagInfo = new TagDetailInfo(tag);
                tagInfo.IsActive = activeTags.Any(t => t.Id == tag.Id);
                info.LocationReactionTags.Add(tagInfo);

                if (tagInfo.IsActive)
                {
                    info.ActiveTags.Add(tagInfo);
                }
            }
        }

        // Sort tags by threshold/element
        info.PlayerTags = info.PlayerTags
            .OrderBy(t => t.SourceElement)
            .ThenBy(t => t.ThresholdValue)
            .ToList();

        // Sort active tags by type
        info.ActiveTags = info.ActiveTags
            .OrderBy(t => t.IsLocationReaction)
            .ThenBy(t => t.Name)
            .ToList();

        return info;
    }

    /// <summary>
    /// Get complete information about a specific tag
    /// </summary>
    public TagDetailInfo GetTagInformation(string tagId)
    {
        EncounterTag tag = _tagRepository.GetTag(tagId);
        if (tag == null)
        {
            return null;
        }

        TagDetailInfo info = new TagDetailInfo(tag);
        info.IsActive = _strategicLayer.GetActiveTags().Any(t => t.Id == tag.Id);
        return info;
    }

    /// <summary>
    /// Get information about all active tags
    /// </summary>
    public List<TagDetailInfo> GetActiveTagInformation()
    {
        List<TagDetailInfo> activeTags = new List<TagDetailInfo>();

        foreach (EncounterTag tag in _strategicLayer.GetActiveTags())
        {
            activeTags.Add(new TagDetailInfo(tag));
        }

        return activeTags
            .OrderBy(t => t.IsLocationReaction)
            .ThenBy(t => t.Name)
            .ToList();
    }
}