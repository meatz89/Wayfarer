using System;
using System.Collections.Generic;

/// <summary>
/// Integrates strategic and narrative layers in the encounter system
/// </summary>
public class EncounterProcessor
{
    private readonly EncounterState _state;
    private readonly StrategicLayer _strategicLayer;
    private readonly NarrativeChoiceGenerator _narrativeGenerator;
    private readonly SpecialTagEffectProcessor _specialEffectProcessor;

    public EncounterProcessor(EncounterState state, LocationStrategicProperties locationProperties,
                             EncounterTagRepository tagRepository)
    {
        _state = state;
        _strategicLayer = new StrategicLayer(locationProperties, tagRepository);
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
}
