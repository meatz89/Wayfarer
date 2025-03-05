
/// <summary>
/// Extended encounter processor with choice projection
/// </summary>
public class EncounterProcessor
{
    private readonly EncounterState _state;
    private readonly ProjectableStrategicLayer _strategicLayer;
    private readonly NarrativeChoiceGenerator _narrativeGenerator;
    private readonly SpecialTagEffectProcessor _specialEffectProcessor;
    private readonly LocationStrategicProperties _locationProperties;
    private readonly EncounterTagRepository _tagRepository;

    public EncounterProcessor(EncounterState state, LocationStrategicProperties locationProperties,
                             EncounterTagRepository tagRepository)
    {
        _state = state;
        _locationProperties = locationProperties;
        _tagRepository = tagRepository;
        _strategicLayer = new ProjectableStrategicLayer(locationProperties, tagRepository);
        _narrativeGenerator = new NarrativeChoiceGenerator(new ChoiceRepository());
        _specialEffectProcessor = new SpecialTagEffectProcessor();

        // Initialize state
        InitializeState();
    }

    /// <summary>
    /// Initialize encounter state
    /// </summary>
    private void InitializeState()
    {
        _state.EncounterStatus = EncounterStatus.InProgress;
        _state.CurrentTurn = 0;

        // Generate initial choices
        List<Choice> initialChoices = _narrativeGenerator.GenerateChoiceSet(_state);
        _state.CurrentChoices = initialChoices;
    }

    /// <summary>
    /// Project the outcome of a choice without changing state
    /// </summary>
    public ChoiceProjection ProjectChoice(Choice choice)
    {
        return _strategicLayer.ProjectChoice(choice, _state);
    }

    /// <summary>
    /// Project outcomes for all available choices
    /// </summary>
    public Dictionary<Choice, ChoiceProjection> ProjectAllChoices()
    {
        Dictionary<Choice, ChoiceProjection> projections = new Dictionary<Choice, ChoiceProjection>();

        foreach (Choice choice in _state.CurrentChoices)
        {
            projections[choice] = ProjectChoice(choice);
        }

        return projections;
    }

    /// <summary>
    /// Process a player's choice
    /// </summary>
    public void ProcessChoice(Choice choice)
    {
        // Process strategic effects
        ChoiceOutcome outcome = _strategicLayer.ProcessChoice(choice, _state);

        // Update narrative tags
        _state.ApproachTypesDic[choice.ApproachType] += 1;
        _state.FocusTypesDic[choice.FocusType] += 1;

        // Update core encounter state
        _state.Momentum += outcome.Momentum;
        _state.Pressure += outcome.Pressure;

        // Add base pressure at end of turn
        _state.Pressure += 1;

        // Handle special tag effects that need encounter processor integration
        _specialEffectProcessor.ProcessSpecialTagEffects(_strategicLayer.GetActiveTags(), _state);

        // Increment turn counter
        _state.CurrentTurn += 1;

        // Check for encounter success/failure conditions
        CheckEncounterConditions();

        // Generate new choices for next turn if encounter is still in progress
        if (_state.EncounterStatus == EncounterStatus.InProgress)
        {
            List<Choice> nextChoices = _narrativeGenerator.GenerateChoiceSet(_state);
            _state.CurrentChoices = nextChoices;
        }
    }

    /// <summary>
    /// Check for encounter success/failure conditions
    /// </summary>
    private void CheckEncounterConditions()
    {
        // Check pressure failure condition
        if (_state.Pressure >= 10)
        {
            _state.EncounterStatus = EncounterStatus.Failed;
            return;
        }

        // Check if we've reached the end of turns
        if (_state.CurrentTurn >= _state.MaxTurns)
        {
            // Determine success level based on momentum thresholds
            if (_state.Momentum >= 12)
            {
                _state.EncounterStatus = EncounterStatus.ExceptionalSuccess;
            }
            else if (_state.Momentum >= 10)
            {
                _state.EncounterStatus = EncounterStatus.StandardSuccess;
            }
            else if (_state.Momentum >= 8)
            {
                _state.EncounterStatus = EncounterStatus.PartialSuccess;
            }
            else
            {
                _state.EncounterStatus = EncounterStatus.Failed;
            }
        }
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
        return _state;
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