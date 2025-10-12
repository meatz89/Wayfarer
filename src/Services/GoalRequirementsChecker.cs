using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.Services
{
    /// <summary>
    /// Service for validating goal requirements before making goals available
    /// Checks knowledge, equipment, stats, location familiarity, and completed goals
    /// </summary>
    public class GoalRequirementsChecker
    {
        private readonly GameWorld _gameWorld;

        public GoalRequirementsChecker(GameWorld gameWorld)
        {
            _gameWorld = gameWorld;
        }

        /// <summary>
        /// Check if player meets all requirements for a goal
        /// Returns true if goal should be available, false otherwise
        /// </summary>
        public bool CheckGoalRequirements(Goal goal)
        {
            if (goal.Requirements == null)
                return true; // No requirements = always available

            Player player = _gameWorld.GetPlayer();

            // Check all requirement categories
            bool knowledgeMet = CheckKnowledgeRequirements(goal.Requirements, player);
            bool equipmentMet = CheckEquipmentRequirements(goal.Requirements, player);
            bool statsMet = CheckStatsRequirements(goal.Requirements, player);
            bool familiarityMet = CheckFamiliarityRequirement(goal.Requirements, player, goal.LocationId);
            bool completedGoalsMet = CheckCompletedGoalsRequirements(goal.Requirements);

            return knowledgeMet && equipmentMet && statsMet && familiarityMet && completedGoalsMet;
        }

        /// <summary>
        /// Check if player meets all requirements and return detailed result
        /// Includes list of missing requirements for UI display
        /// </summary>
        public GoalRequirementResult CheckGoalRequirementsDetailed(Goal goal)
        {
            if (goal.Requirements == null)
                return GoalRequirementResult.Met();

            Player player = _gameWorld.GetPlayer();
            List<string> missingRequirements = new List<string>();

            // Check knowledge requirements
            if (goal.Requirements.RequiredKnowledge != null && goal.Requirements.RequiredKnowledge.Any())
            {
                List<string> missing = goal.Requirements.RequiredKnowledge
                    .Where(k => !player.Knowledge.HasKnowledge(k))
                    .ToList();

                if (missing.Any())
                {
                    foreach (string knowledge in missing)
                    {
                        missingRequirements.Add($"Missing knowledge: {knowledge}");
                    }
                }
            }

            // Check equipment requirements
            if (goal.Requirements.RequiredEquipment != null && goal.Requirements.RequiredEquipment.Any())
            {
                List<string> playerItems = player.Inventory.GetAllItems().ToList();
                List<string> missing = goal.Requirements.RequiredEquipment
                    .Where(e => !playerItems.Contains(e))
                    .ToList();

                if (missing.Any())
                {
                    foreach (string equipment in missing)
                    {
                        missingRequirements.Add($"Missing equipment: {equipment}");
                    }
                }
            }

            // Check stat requirements
            if (goal.Requirements.RequiredStats != null && goal.Requirements.RequiredStats.Any())
            {
                foreach (KeyValuePair<PlayerStatType, int> statReq in goal.Requirements.RequiredStats)
                {
                    int currentLevel = player.Stats.GetLevel(statReq.Key);
                    if (currentLevel < statReq.Value)
                    {
                        missingRequirements.Add($"{statReq.Key} Level {statReq.Value} required (you have {currentLevel})");
                    }
                }
            }

            // Check location familiarity requirement
            if (goal.Requirements.MinimumLocationFamiliarity > 0 && !string.IsNullOrEmpty(goal.LocationId))
            {
                int currentFamiliarity = player.GetLocationFamiliarity(goal.LocationId);
                if (currentFamiliarity < goal.Requirements.MinimumLocationFamiliarity)
                {
                    missingRequirements.Add($"Location familiarity {goal.Requirements.MinimumLocationFamiliarity} required (you have {currentFamiliarity})");
                }
            }

            // Check completed goals requirements
            if (goal.Requirements.CompletedGoals != null && goal.Requirements.CompletedGoals.Any())
            {
                List<string> missing = goal.Requirements.CompletedGoals
                    .Where(g => !IsGoalCompleted(g))
                    .ToList();

                if (missing.Any())
                {
                    foreach (string goalId in missing)
                    {
                        string goalName = GetGoalName(goalId);
                        missingRequirements.Add($"Must complete: {goalName}");
                    }
                }
            }

            if (missingRequirements.Any())
            {
                return GoalRequirementResult.NotMet(missingRequirements);
            }

            return GoalRequirementResult.Met();
        }

        /// <summary>
        /// Check if player has all required knowledge
        /// </summary>
        private bool CheckKnowledgeRequirements(GoalRequirements requirements, Player player)
        {
            if (requirements.RequiredKnowledge == null || !requirements.RequiredKnowledge.Any())
                return true;

            return requirements.RequiredKnowledge.All(k => player.Knowledge.HasKnowledge(k));
        }

        /// <summary>
        /// Check if player has all required equipment
        /// </summary>
        private bool CheckEquipmentRequirements(GoalRequirements requirements, Player player)
        {
            if (requirements.RequiredEquipment == null || !requirements.RequiredEquipment.Any())
                return true;

            List<string> playerItems = player.Inventory.GetAllItems().ToList();
            return requirements.RequiredEquipment.All(e => playerItems.Contains(e));
        }

        /// <summary>
        /// Check if player meets all stat requirements
        /// </summary>
        private bool CheckStatsRequirements(GoalRequirements requirements, Player player)
        {
            if (requirements.RequiredStats == null || !requirements.RequiredStats.Any())
                return true;

            foreach (KeyValuePair<PlayerStatType, int> statReq in requirements.RequiredStats)
            {
                if (player.Stats.GetLevel(statReq.Key) < statReq.Value)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Check if player meets location familiarity requirement
        /// </summary>
        private bool CheckFamiliarityRequirement(GoalRequirements requirements, Player player, string locationId)
        {
            if (requirements.MinimumLocationFamiliarity <= 0)
                return true;

            if (string.IsNullOrEmpty(locationId))
                return true; // No location specified = no familiarity requirement

            int currentFamiliarity = player.GetLocationFamiliarity(locationId);
            return currentFamiliarity >= requirements.MinimumLocationFamiliarity;
        }

        /// <summary>
        /// Check if player has completed all required goals
        /// </summary>
        private bool CheckCompletedGoalsRequirements(GoalRequirements requirements)
        {
            if (requirements.CompletedGoals == null || !requirements.CompletedGoals.Any())
                return true;

            return requirements.CompletedGoals.All(g => IsGoalCompleted(g));
        }

        /// <summary>
        /// Check if a goal has been completed
        /// </summary>
        private bool IsGoalCompleted(string goalId)
        {
            if (_gameWorld.Goals.TryGetValue(goalId, out Goal goal))
            {
                return goal.IsCompleted;
            }

            return false; // Goal not found = not completed
        }

        /// <summary>
        /// Get goal name for display purposes
        /// </summary>
        private string GetGoalName(string goalId)
        {
            if (_gameWorld.Goals.TryGetValue(goalId, out Goal goal))
            {
                return goal.Name;
            }

            return goalId; // Fallback to ID if goal not found
        }
    }

    /// <summary>
    /// Result of goal requirement checking
    /// </summary>
    public class GoalRequirementResult
    {
        public bool RequirementsMet { get; set; }
        public List<string> MissingRequirements { get; set; } = new List<string>();

        public static GoalRequirementResult Met()
        {
            return new GoalRequirementResult { RequirementsMet = true };
        }

        public static GoalRequirementResult NotMet(List<string> missing)
        {
            return new GoalRequirementResult
            {
                RequirementsMet = false,
                MissingRequirements = missing
            };
        }
    }
}
