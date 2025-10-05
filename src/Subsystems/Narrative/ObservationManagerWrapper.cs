using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Wrapper for the existing ObservationManager to integrate it into the Narrative subsystem.
/// Provides observation-related operations for the NarrativeFacade.
/// </summary>
public class ObservationManagerWrapper
{
    private readonly ObservationManager _observationManager;
    private readonly ObservationSystem _observationSystem;
    private readonly GameWorld _gameWorld;

    public ObservationManagerWrapper(
        ObservationManager observationManager,
        ObservationSystem observationSystem,
        GameWorld gameWorld)
    {
        _observationManager = observationManager;
        _observationSystem = observationSystem;
        _gameWorld = gameWorld;
    }

    /// <summary>
    /// Get an observation by ID from the system
    /// </summary>
    public Observation GetObservation(string observationId)
    {
        // Search through all locations for the observation
        foreach (Location location in _gameWorld.Locations)
        {
            List<Observation> observations = _observationSystem.GetAllObservationsForLocation(location.Id);
            Observation? observation = observations.FirstOrDefault(o => o.Id == observationId);
            if (observation != null)
            {
                return observation;
            }
        }
        return null;
    }

    /// <summary>
    /// Take an observation and generate an observation card
    /// </summary>
    public ObservationCard TakeObservation(Observation observation, TokenMechanicsManager tokenManager)
    {
        return _observationManager.TakeObservation(observation, tokenManager);
    }

    /// <summary>
    /// Check if an observation has been taken this time block
    /// </summary>
    public bool HasTakenObservation(string observationId)
    {
        return _observationManager.HasTakenObservation(observationId);
    }

    /// <summary>
    /// Get all active observation cards for a specific NPC
    /// </summary>
    public List<ObservationCard> GetObservationCards(string npcId)
    {
        return _observationManager.GetObservationCards(npcId);
    }

    /// <summary>
    /// Get observation cards as conversation cards for a specific NPC
    /// </summary>
    public List<ConversationCard> GetObservationCardsAsConversationCards(string npcId)
    {
        return _observationManager.GetObservationCardsAsConversationCards(npcId);
    }

    /// <summary>
    /// Remove an observation card from a specific NPC's deck
    /// </summary>
    public void RemoveObservationCard(string npcId, string cardId)
    {
        _observationManager.RemoveObservationCard(npcId, cardId);
    }

    /// <summary>
    /// Get all taken observations for current time block
    /// </summary>
    public List<TakenObservation> GetTakenObservations()
    {
        return _observationManager.GetTakenObservations();
    }

    /// <summary>
    /// Refresh observations for new time block
    /// </summary>
    public void RefreshForNewTimeBlock()
    {
        _observationManager.RefreshForNewTimeBlock();
    }

    /// <summary>
    /// Start a new day - clear all observations
    /// </summary>
    public void StartNewDay()
    {
        _observationManager.StartNewDay();
    }

    /// <summary>
    /// Get observations for a specific location and spot
    /// </summary>
    public List<Observation> GetObservationsForLocationSpot(string locationId, string spotId)
    {
        return _observationSystem.GetObservationsForLocationSpot(locationId, spotId);
    }

    /// <summary>
    /// Get all observations for a location
    /// </summary>
    public List<Observation> GetAllObservationsForLocation(string locationId)
    {
        return _observationSystem.GetAllObservationsForLocation(locationId);
    }

    /// <summary>
    /// Mark an observation as revealed
    /// </summary>
    public void MarkObservationRevealed(string observationId)
    {
        _observationSystem.MarkObservationRevealed(observationId);
    }

    /// <summary>
    /// Check if an observation has been revealed
    /// </summary>
    public bool IsObservationRevealed(string observationId)
    {
        return _observationSystem.IsObservationRevealed(observationId);
    }

    /// <summary>
    /// Get available observation rewards for a location based on familiarity
    /// </summary>
    public List<ObservationReward> GetAvailableObservationRewards(string locationId)
    {
        return _observationManager.GetAvailableObservationRewards(locationId);
    }

    /// <summary>
    /// Complete an observation reward and add card to NPC observation deck
    /// </summary>
    public bool CompleteObservationReward(string locationId, ObservationReward reward)
    {
        return _observationManager.CompleteObservationReward(locationId, reward);
    }
}
