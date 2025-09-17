using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.Subsystems.LocationSubsystem;

namespace Wayfarer.Subsystems.NarrativeSubsystem
{
    /// <summary>
    /// Public API for all narrative operations.
    /// Centralizes narrative generation, messaging, and observation handling.
    /// </summary>
    public class NarrativeFacade
    {
        private readonly GameWorld _gameWorld;
        private readonly ObservationManagerWrapper _observationManager;
        private readonly MessageSystem _messageSystem;
        private readonly NarrativeRenderer _narrativeRenderer;
        private readonly NarrativeService _narrativeService;
        private readonly LocationNarrativeGenerator _locationNarrativeGenerator;

        public NarrativeFacade(
            GameWorld gameWorld,
            ObservationManagerWrapper observationManager,
            MessageSystem messageSystem,
            NarrativeRenderer narrativeRenderer,
            NarrativeService narrativeService,
            LocationNarrativeGenerator locationNarrativeGenerator)
        {
            _gameWorld = gameWorld;
            _observationManager = observationManager;
            _messageSystem = messageSystem;
            _narrativeRenderer = narrativeRenderer;
            _narrativeService = narrativeService;
            _locationNarrativeGenerator = locationNarrativeGenerator;
        }

        // ========== OBSERVATION OPERATIONS ==========

        /// <summary>
        /// Take an observation and generate an observation card
        /// </summary>
        public ObservationCard TakeObservation(string observationId, TokenMechanicsManager tokenManager)
        {
            Observation observation = _observationManager.GetObservation(observationId);
            if (observation == null)
            {
                Console.WriteLine($"[NarrativeFacade] Observation {observationId} not found");
                return null;
            }

            // Take the observation through the manager
            ObservationCard? card = _observationManager.TakeObservation(observation, tokenManager);

            if (card != null)
            {
                // Generate narrative for the observation
                string narrative = $"You observe {observation.Text} and gain insight.";
                _messageSystem.AddSystemMessage(narrative, SystemMessageTypes.Info);
            }

            return card;
        }

        /// <summary>
        /// Take an observation (simplified version for facade delegation)
        /// </summary>
        public bool TakeObservation(string observationId)
        {
            Observation observation = _observationManager.GetObservation(observationId);
            if (observation == null)
            {
                Console.WriteLine($"[NarrativeFacade] Observation {observationId} not found");
                return false;
            }

            // For now, pass null for token manager since observation taking doesn't require complex token mechanics
            TokenMechanicsManager? tokenManager = (TokenMechanicsManager)null;
            ObservationCard card = _observationManager.TakeObservation(observation, tokenManager);

            if (card != null)
            {
                // Generate narrative for the observation
                string narrative = $"You observe {observation.Text} and gain insight.";
                _messageSystem.AddSystemMessage(narrative, SystemMessageTypes.Info);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get all observations for current location
        /// </summary>
        public List<Observation> GetLocationObservations(string locationId, string spotId = null)
        {
            if (string.IsNullOrEmpty(spotId))
            {
                return _observationManager.GetAllObservationsForLocation(locationId);
            }
            else
            {
                return _observationManager.GetObservationsForLocationSpot(locationId, spotId);
            }
        }

        /// <summary>
        /// Check if an observation has been taken this time block
        /// </summary>
        public bool HasTakenObservation(string observationId)
        {
            return _observationManager.HasTakenObservation(observationId);
        }

        /// <summary>
        /// Get active observation cards
        /// </summary>
        public List<ObservationCard> GetActiveObservationCards()
        {
            return _observationManager.GetObservationCards();
        }

        /// <summary>
        /// Get observation cards as conversation cards for a specific NPC
        /// </summary>
        public List<ConversationCard> GetObservationCardsAsConversationCards(string npcId)
        {
            return _observationManager.GetObservationCardsAsConversationCards(npcId);
        }

        /// <summary>
        /// Remove an observation card from a specific NPC's deck after it's been played
        /// </summary>
        public void RemoveObservationCard(string npcId, string cardId)
        {
            _observationManager.RemoveObservationCard(npcId, cardId);
        }

        /// <summary>
        /// Refresh observations for new time block
        /// </summary>
        public void RefreshObservationsForNewTimeBlock()
        {
            _observationManager.RefreshForNewTimeBlock();
            string narrative = "New opportunities become available as time passes.";
            _messageSystem.AddSystemMessage(narrative, SystemMessageTypes.Info);
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
            bool success = _observationManager.CompleteObservationReward(locationId, reward);
            if (success)
            {
                string narrative = $"You observe {reward.ObservationCard.Name} and gain valuable insight.";
                _messageSystem.AddSystemMessage(narrative, SystemMessageTypes.Info);
            }
            return success;
        }

        // ========== MESSAGE OPERATIONS ==========

        /// <summary>
        /// Add a system message with narrative flair
        /// </summary>
        public void AddSystemMessage(string message, SystemMessageTypes type = SystemMessageTypes.Info)
        {
            _messageSystem.AddSystemMessage(message, type);
        }

        /// <summary>
        /// Add a narrative event message
        /// </summary>
        public void AddNarrativeMessage(string message, SystemMessageTypes type = SystemMessageTypes.Info)
        {
            _messageSystem.AddSystemMessage(message, type);
        }

        /// <summary>
        /// Generate and add letter positioning narrative
        /// </summary>
        public void AddLetterPositioningNarrative(string senderName, LetterPositioningReason reason, int position, int strength, int debt)
        {
            string narrative = $"{senderName}'s letter positioned at {position} based on relationship strength {strength}.";
            _messageSystem.AddLetterPositioningMessage(senderName, reason, position, strength, debt);
            _messageSystem.AddSystemMessage(narrative, SystemMessageTypes.Info);
        }

        /// <summary>
        /// Generate and add special letter event narrative
        /// </summary>
        public void AddSpecialLetterEvent(SpecialLetterEvent letterEvent)
        {
            string narrative = $"Special letter event: {letterEvent.EventType}";
            _messageSystem.AddSpecialLetterEvent(letterEvent);

            SystemMessageTypes severity = GetSeverityForLetterEvent(letterEvent);
            _messageSystem.AddSystemMessage(narrative, severity);
        }

        // ========== NARRATIVE GENERATION ==========

        /// <summary>
        /// Generate token gain narrative
        /// </summary>
        public TokenNarrativeResult GenerateTokenGainNarrative(ConnectionType type, int count, string npcId)
        {
            return _narrativeService.GenerateTokenGainNarrative(type, count, npcId);
        }

        /// <summary>
        /// Generate relationship milestone narrative
        /// </summary>
        public MilestoneNarrativeResult GenerateRelationshipMilestone(string npcId, int totalTokens)
        {
            return _narrativeService.GenerateRelationshipMilestone(npcId, totalTokens);
        }

        /// <summary>
        /// Generate relationship damage narrative
        /// </summary>
        public NarrativeResult GenerateRelationshipDamageNarrative(string npcId, ConnectionType type, int remainingTokens)
        {
            return _narrativeService.GenerateRelationshipDamageNarrative(npcId, type, remainingTokens);
        }

        /// <summary>
        /// Generate queue reorganization narrative
        /// </summary>
        public string[] GenerateQueueReorganizationNarrative(int removedPosition, int lettersShifted)
        {
            return _narrativeService.GenerateQueueReorganizationNarrative(removedPosition, lettersShifted);
        }

        /// <summary>
        /// Generate morning letter narrative
        /// </summary>
        public MorningNarrativeResult GenerateMorningLetterNarrative(int lettersGenerated, bool queueFull)
        {
            return _narrativeService.GenerateMorningLetterNarrative(lettersGenerated, queueFull);
        }

        /// <summary>
        /// Generate time transition narrative
        /// </summary>
        public TransitionNarrativeResult GenerateTimeTransitionNarrative(TimeBlocks from, TimeBlocks to, string actionDescription = null)
        {
            return _narrativeService.GenerateTimeTransitionNarrative(from, to, actionDescription);
        }

        /// <summary>
        /// Generate obligation warning narrative
        /// </summary>
        public NarrativeResult GenerateObligationWarning(StandingObligation obligation, int daysUntilForced)
        {
            return _narrativeService.GenerateObligationWarning(obligation, daysUntilForced);
        }

        /// <summary>
        /// Generate deadline warning narrative
        /// </summary>
        public NarrativeResult GenerateDeadlineWarning(DeliveryObligation letter, int daysRemaining)
        {
            return _narrativeService.GenerateDeadlineWarning(letter, daysRemaining);
        }

        /// <summary>
        /// Generate queue entry narrative
        /// </summary>
        public string GenerateQueueEntryNarrative(DeliveryObligation letter, int position)
        {
            return _narrativeService.GenerateQueueEntryNarrative(letter, position);
        }

        /// <summary>
        /// Generate token spending narrative
        /// </summary>
        public string GenerateTokenSpendingNarrative(ConnectionType type, int amount, string action)
        {
            return _narrativeService.GenerateTokenSpendingNarrative(type, amount, action);
        }

        /// <summary>
        /// Generate obligation acceptance narrative
        /// </summary>
        public string GenerateObligationAcceptanceNarrative(StandingObligation obligation)
        {
            return _narrativeService.GenerateObligationAcceptanceNarrative(obligation);
        }

        /// <summary>
        /// Generate obligation conflict narrative
        /// </summary>
        public string GenerateObligationConflictNarrative(string newObligation, List<string> conflicts)
        {
            return _narrativeService.GenerateObligationConflictNarrative(newObligation, conflicts);
        }

        /// <summary>
        /// Generate obligation removal narrative
        /// </summary>
        public string GenerateObligationRemovalNarrative(StandingObligation obligation, bool isVoluntary)
        {
            return _narrativeService.GenerateObligationRemovalNarrative(obligation, isVoluntary);
        }

        /// <summary>
        /// Generate forced letter narrative
        /// </summary>
        public string GenerateForcedLetterNarrative(StandingObligation obligation, DeliveryObligation letter)
        {
            return _narrativeService.GenerateForcedLetterNarrative(obligation, letter);
        }

        /// <summary>
        /// Generate obligation breaking narrative
        /// </summary>
        public string GenerateObligationBreakingNarrative(StandingObligation obligation, int tokenLoss)
        {
            return _narrativeService.GenerateObligationBreakingNarrative(obligation, tokenLoss);
        }

        // ========== TEMPLATE RENDERING ==========

        /// <summary>
        /// Render a categorical template to human-readable text
        /// </summary>
        public string RenderTemplate(string template)
        {
            return _narrativeRenderer.RenderTemplate(template);
        }

        /// <summary>
        /// Generate location arrival narrative
        /// </summary>
        public string GenerateArrivalText(Location location, LocationSpot entrySpot)
        {
            return _locationNarrativeGenerator.GenerateArrivalText(location, entrySpot);
        }

        /// <summary>
        /// Generate location departure narrative
        /// </summary>
        public string GenerateDepartureText(Location location, LocationSpot exitSpot)
        {
            return _locationNarrativeGenerator.GenerateDepartureText(location, exitSpot);
        }

        /// <summary>
        /// Generate movement between spots narrative
        /// </summary>
        public string GenerateMovementText(LocationSpot fromSpot, LocationSpot toSpot)
        {
            return _locationNarrativeGenerator.GenerateMovementText(fromSpot, toSpot);
        }

        // ========== HELPER METHODS ==========

        private SystemMessageTypes GetSeverityForLetterEvent(SpecialLetterEvent letterEvent)
        {
            return letterEvent.Severity switch
            {
                NarrativeSeverity.Success => SystemMessageTypes.Success,
                NarrativeSeverity.Warning => SystemMessageTypes.Warning,
                NarrativeSeverity.Danger => SystemMessageTypes.Danger,
                NarrativeSeverity.Celebration => SystemMessageTypes.Success,
                _ => SystemMessageTypes.Info
            };
        }
    }
}