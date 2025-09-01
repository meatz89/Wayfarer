using System;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.Subsystems.NarrativeSubsystem
{
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
            foreach (var location in _gameWorld.Locations)
            {
                var observations = _observationSystem.GetAllObservationsForLocation(location.ID);
                var observation = observations.FirstOrDefault(o => o.Id == observationId);
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
        /// Get all active observation cards
        /// </summary>
        public List<ObservationCard> GetObservationCards()
        {
            return _observationManager.GetObservationCards();
        }

        /// <summary>
        /// Get observation cards as conversation cards
        /// </summary>
        public List<ConversationCard> GetObservationCardsAsConversationCards()
        {
            return _observationManager.GetObservationCardsAsConversationCards();
        }

        /// <summary>
        /// Remove an observation card
        /// </summary>
        public void RemoveObservationCard(string cardId)
        {
            _observationManager.RemoveObservationCard(cardId);
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
    }
}