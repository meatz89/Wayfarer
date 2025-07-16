using System;
using System.Linq;
using Wayfarer.GameState;

namespace Wayfarer.GameState
{
    public class StandingObligationManager
    {
        private readonly GameWorld _gameWorld;
        private readonly MessageSystem _messageSystem;
        
        public StandingObligationManager(GameWorld gameWorld, MessageSystem messageSystem)
        {
            _gameWorld = gameWorld;
            _messageSystem = messageSystem;
        }
        
        // Get all active obligations for the player
        public List<StandingObligation> GetActiveObligations()
        {
            return _gameWorld.GetPlayer().StandingObligations
                .Where(o => o.IsActive)
                .ToList();
        }
        
        // Add a new obligation to the player
        public bool AddObligation(StandingObligation obligation)
        {
            if (obligation == null) return false;
            
            var player = _gameWorld.GetPlayer();
            
            // Check for conflicts with existing obligations
            var conflicts = CheckObligationConflicts(obligation);
            if (conflicts.Any())
            {
                _messageSystem.AddSystemMessage(
                    $"Cannot accept {obligation.Name}: conflicts with {string.Join(", ", conflicts.Select(c => c.Name))}",
                    SystemMessageTypes.Warning
                );
                return false;
            }
            
            // Add the obligation
            player.StandingObligations.Add(obligation);
            
            _messageSystem.AddSystemMessage(
                $"Accepted Standing Obligation: {obligation.Name}",
                SystemMessageTypes.Success
            );
            
            // Log the effects for the player
            _messageSystem.AddSystemMessage(
                obligation.GetEffectsSummary(),
                SystemMessageTypes.Info
            );
            
            return true;
        }
        
        // Remove an obligation (rare, usually has consequences)
        public bool RemoveObligation(string obligationId, bool isVoluntary = true)
        {
            var player = _gameWorld.GetPlayer();
            var obligation = player.StandingObligations.FirstOrDefault(o => o.ID == obligationId);
            
            if (obligation == null) return false;
            
            // Apply consequences for breaking obligations voluntarily
            if (isVoluntary)
            {
                ApplyBreakingConsequences(obligation);
            }
            
            obligation.IsActive = false;
            
            _messageSystem.AddSystemMessage(
                $"Standing Obligation removed: {obligation.Name}",
                isVoluntary ? SystemMessageTypes.Warning : SystemMessageTypes.Info
            );
            
            return true;
        }
        
        // Check for conflicts between obligations
        public List<StandingObligation> CheckObligationConflicts(StandingObligation newObligation)
        {
            var conflicts = new List<StandingObligation>();
            var activeObligations = GetActiveObligations();
            
            foreach (var existing in activeObligations)
            {
                // Same token type with conflicting effects
                if (existing.RelatedTokenType == newObligation.RelatedTokenType)
                {
                    // Check for directly conflicting effects
                    if (HasConflictingEffects(existing, newObligation))
                    {
                        conflicts.Add(existing);
                    }
                }
                
                // Special conflict cases
                if (IsSpecialConflict(existing, newObligation))
                {
                    conflicts.Add(existing);
                }
            }
            
            return conflicts;
        }
        
        // Check if we should generate forced letters today
        public List<StandingObligation> GetObligationsRequiringForcedLetters()
        {
            return GetActiveObligations()
                .Where(o => o.ShouldGenerateForcedLetter())
                .ToList();
        }
        
        // Record that forced letters were generated for obligations
        public void RecordForcedLettersGenerated(List<StandingObligation> obligations)
        {
            foreach (var obligation in obligations)
            {
                obligation.RecordForcedLetterGenerated();
            }
        }
        
        // Advance time for all obligations (called daily)
        public void AdvanceDailyTime()
        {
            var activeObligations = GetActiveObligations();
            
            foreach (var obligation in activeObligations)
            {
                obligation.DaysSinceLastForcedLetter++;
            }
        }
        
        // Calculate total coin bonus for a letter delivery
        public int CalculateTotalCoinBonus(Letter letter)
        {
            int totalBonus = 0;
            var activeObligations = GetActiveObligations();
            
            foreach (var obligation in activeObligations)
            {
                totalBonus += obligation.CalculateCoinBonus(letter, letter.Payment);
            }
            
            return totalBonus;
        }
        
        // Calculate the best entry position for a new letter
        public int CalculateBestEntryPosition(Letter letter, int basePosition)
        {
            int bestPosition = basePosition;
            var activeObligations = GetActiveObligations();
            
            foreach (var obligation in activeObligations)
            {
                int obligationPosition = obligation.CalculateEntryPosition(letter, bestPosition);
                bestPosition = Math.Min(bestPosition, obligationPosition);
            }
            
            return bestPosition;
        }
        
        // Check if any obligation provides free deadline extension
        public bool HasFreeDeadlineExtension(Letter letter)
        {
            return GetActiveObligations()
                .Any(o => o.IsFreeDeadlineExtension(letter));
        }
        
        // Calculate total skip cost multiplier
        public int CalculateSkipCostMultiplier(Letter letter)
        {
            int maxMultiplier = 1;
            var activeObligations = GetActiveObligations();
            
            foreach (var obligation in activeObligations)
            {
                int multiplier = obligation.CalculateSkipCostMultiplier(letter);
                maxMultiplier = Math.Max(maxMultiplier, multiplier);
            }
            
            return maxMultiplier;
        }
        
        // Check if any obligation forbids an action
        public bool IsActionForbidden(string actionType, Letter letter, out string reason)
        {
            reason = "";
            var activeObligations = GetActiveObligations();
            
            foreach (var obligation in activeObligations)
            {
                if (obligation.IsForbiddenAction(actionType, letter))
                {
                    reason = $"Forbidden by {obligation.Name}";
                    return true;
                }
            }
            
            return false;
        }
        
        // Record a constraint violation
        public void RecordConstraintViolation(string obligationId, string violationType)
        {
            var obligation = GetActiveObligations().FirstOrDefault(o => o.ID == obligationId);
            if (obligation != null)
            {
                obligation.RecordViolation();
                
                _messageSystem.AddSystemMessage(
                    $"Constraint violated: {obligation.Name} ({violationType})",
                    SystemMessageTypes.Warning
                );
                
                // Apply immediate consequences for some violations
                ApplyViolationConsequences(obligation, violationType);
            }
        }
        
        // Get obligations affecting a specific token type
        public List<StandingObligation> GetObligationsForTokenType(ConnectionType tokenType)
        {
            return GetActiveObligations()
                .Where(o => o.AppliesTo(tokenType))
                .ToList();
        }
        
        // Get summary of all active obligation effects
        public string GetObligationsSummary()
        {
            var activeObligations = GetActiveObligations();
            if (!activeObligations.Any())
            {
                return "No active standing obligations";
            }
            
            var lines = new List<string>();
            foreach (var obligation in activeObligations)
            {
                lines.Add($"• {obligation.Name} (Day {obligation.DaysSinceAccepted})");
                lines.Add($"  {obligation.GetEffectsSummary()}");
            }
            
            return string.Join("\n", lines);
        }
        
        // Helper methods
        private bool HasConflictingEffects(StandingObligation existing, StandingObligation newObligation)
        {
            // Check for effects that cannot coexist
            var allExistingEffects = existing.BenefitEffects.Concat(existing.ConstraintEffects);
            var allNewEffects = newObligation.BenefitEffects.Concat(newObligation.ConstraintEffects);
            
            // Example: Cannot have both "Cannot purge trade letters" and "Must purge trade letters"
            // This is a simplified check - expand as needed
            
            return false; // No conflicts for now
        }
        
        private bool IsSpecialConflict(StandingObligation existing, StandingObligation newObligation)
        {
            // Special cases where obligations conflict regardless of token type
            // For example, patron-related obligations might conflict
            
            return false; // No special conflicts for now
        }
        
        private void ApplyBreakingConsequences(StandingObligation obligation)
        {
            // Apply relationship damage or other consequences for breaking obligations
            var tokenManager = new ConnectionTokenManager(_gameWorld);
            
            // Remove tokens based on obligation type
            if (obligation.RelatedTokenType.HasValue)
            {
                int penaltyTokens = Math.Min(5, tokenManager.GetTokenCount(obligation.RelatedTokenType.Value));
                if (penaltyTokens > 0)
                {
                    tokenManager.SpendTokens(obligation.RelatedTokenType.Value, penaltyTokens);
                    
                    _messageSystem.AddSystemMessage(
                        $"Lost {penaltyTokens} {obligation.RelatedTokenType} tokens for breaking {obligation.Name}",
                        SystemMessageTypes.Danger
                    );
                }
            }
        }
        
        private void ApplyViolationConsequences(StandingObligation obligation, string violationType)
        {
            // Apply minor consequences for constraint violations
            
            if (violationType == "refused_common" && obligation.HasEffect(ObligationEffect.NoCommonRefusal))
            {
                var tokenManager = new ConnectionTokenManager(_gameWorld);
                tokenManager.SpendTokens(ConnectionType.Common, 2);
                
                _messageSystem.AddSystemMessage(
                    "Lost 2 Common tokens for refusing common letter",
                    SystemMessageTypes.Warning
                );
            }
        }
    }
}