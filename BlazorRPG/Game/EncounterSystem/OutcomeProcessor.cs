public class OutcomeProcessor
{
    public GameState GameState { get; }
    public PlayerProgression PlayerProgression { get; }
    public MessageSystem MessageSystem { get; }

    public OutcomeProcessor(
        GameState gameState,
        PlayerProgression playerProgression,
        MessageSystem messageSystem)
    {
        GameState = gameState;
        PlayerProgression = playerProgression;
        MessageSystem = messageSystem;
    }

    public void ProcessActionYields(ActionImplementation actionImplementation)
    {

        foreach (Outcome reward in actionImplementation.Yields)
        {
            reward.Apply(GameState);
            MessageSystem.AddOutcome(reward);
        }
    }


    //public void ProcessActionYields(ActionImplementation action, GameState gameState)
    //{
    //    // Get the current location and spot
    //    Location location = gameState.WorldState.CurrentLocation;
    //    LocationSpot spot = gameState.WorldState.CurrentLocationSpot;

    //    // Process all yields from the action definition
    //    if (action?.Yields != null)
    //    {
    //        foreach (YieldDefinition yield in action.Yields)
    //        {
    //            // Check if conditions are met
    //            if (!AreConditionsMet(yield, gameState, spot))
    //                continue;

    //            // Calculate effective yield amount
    //            int amount = CalculateYieldAmount(yield, gameState.PlayerState.PlayerSkills);

    //            // Apply yield based on type
    //            ApplyYield(yield.Type, yield.TargetId, amount, gameState, location, spot);
    //        }
    //    }

    //    // Always deplete the node if it's a resource node
    //    if (spot != null && spot.NodeType != ResourceNodeTypes.None)
    //    {
    //        spot.Deplete(CalculateDepletionAmount(spot.BaseDepletion, gameState.PlayerState.PlayerSkills));
    //    }
    //}

    //private bool AreConditionsMet(YieldDefinition yield, GameState gameState, LocationSpot spot)
    //{
    //    if (yield.Conditions == null || !yield.Conditions.Any())
    //        return true; // No conditions means always valid

    //    foreach (YieldCondition condition in yield.Conditions)
    //    {
    //        switch (condition.Type)
    //        {
    //            case ConditionTypes.SkillLevel:
    //                int skillLevel = gameState.PlayerState.PlayerSkills.GetLevelForSkill(
    //                    (SkillTypes)Enum.Parse(typeof(SkillTypes), condition.TargetId));
    //                if (skillLevel < condition.RequiredValue)
    //                    return false;
    //                break;

    //            case ConditionTypes.FirstDiscovery:
    //                // Check if this is the first discovery of this action
    //                if (gameState.WorldState.IsEncounterCompleted(condition.TargetId))
    //                    return false;
    //                break;

    //            case ConditionTypes.NodeState:
    //                // Check node aspect discovery state
    //                if (spot != null)
    //                {
    //                    NodeAspectDefinition aspect = spot.DiscoverableAspects
    //                        .FirstOrDefault(a => a.Id == condition.TargetId);
    //                    if (aspect == null || aspect.IsDiscovered != (condition.RequiredValue == 1))
    //                        return false;
    //                }
    //                break;

    //            case ConditionTypes.ProgressiveAction:
    //                // Check how many times action has been performed
    //                int actionCount = gameState.WorldState.GetActionCount(condition.TargetId);
    //                if (actionCount != condition.RequiredValue)
    //                    return false;
    //                break;
    //        }

    //        // Apply random chance factor
    //        if (condition.Chance < 100.0f)
    //        {
    //            Random rand = new Random();
    //            if (rand.Next(100) >= condition.Chance)
    //                return false;
    //        }
    //    }

    //    return true;
    //}

    //private int CalculateYieldAmount(YieldDefinition yield, PlayerSkills skills)
    //{
    //    int amount = yield.BaseAmount;

    //    // Apply skill scaling if applicable
    //    if (yield.ScalingSkillType.HasValue && yield.SkillMultiplier > 0)
    //    {
    //        int skillLevel = skills.GetLevelForSkill(yield.ScalingSkillType.Value);
    //        amount += (int) (skillLevel * yield.SkillMultiplier);
    //    }

    //    return amount;
    //}

    private int CalculateDepletionAmount(float baseDepletion, PlayerSkills skills)
    {
        // More skilled players deplete resources less
        float skillFactor = 1.0f;

        // Apply skill-based reduction (example using Foraging)
        int foragingLevel = skills.GetLevelForSkill(SkillTypes.Warfare);
        if (foragingLevel > 0)
        {
            skillFactor = 1.0f - (0.05f * foragingLevel); // 5% reduction per level
            skillFactor = Math.Max(0.5f, skillFactor); // Cap at 50% reduction
        }

        return (int)(baseDepletion * skillFactor);
    }

    //private void ApplyOutcome(YieldTypes type, string targetId, int amount, GameState gameState, Location location, LocationSpot spot)
    //{
    //    switch (type)
    //    {
    //        case YieldTypes.Resource:
    //            // Add resources based on targetId
    //            switch (targetId.ToLower())
    //            {
    //                case "food":
    //                    gameState.PlayerState.ModifyFood(amount);
    //                    MessageSystem.AddSystemMessage($"Gained {amount} food");
    //                    break;
    //                case "energy":
    //                    gameState.PlayerState.ModifyEnergy(amount);
    //                    MessageSystem.AddSystemMessage($"Gained {amount} energy");
    //                    break;
    //                case "medicinal_herbs":
    //                case "medicinalherbs":
    //                    gameState.PlayerState.ModifyMedicinalHerbs(amount);
    //                    MessageSystem.AddSystemMessage($"Gained {amount} medicinal herbs");
    //                    break;
    //                case "health":
    //                    gameState.PlayerState.ModifyHealth(amount);
    //                    MessageSystem.AddSystemMessage($"Gained {amount} health");
    //                    break;
    //                case "concentration":
    //                    gameState.PlayerState.ModifyConcentration(amount);
    //                    MessageSystem.AddSystemMessage($"Gained {amount} concentration");
    //                    break;
    //                case "confidence":
    //                    gameState.PlayerState.ModifyConfidence(amount);
    //                    MessageSystem.AddSystemMessage($"Gained {amount} confidence");
    //                    break;
    //                case "coins":
    //                    gameState.PlayerState.AddCoins(amount);
    //                    MessageSystem.AddSystemMessage($"Gained {amount} coins");
    //                    break;
    //            }
    //            break;

    //        case YieldTypes.SkillXP:
    //            SkillTypes skillType = (SkillTypes)Enum.Parse(typeof(SkillTypes), targetId);
    //            PlayerProgression.AddSkillExp(skillType, amount);
    //            MessageSystem.AddSystemMessage($"Gained {amount} {skillType} skill experience");
    //            break;

    //        case YieldTypes.LocationAccess:
    //            // Unlock a new location
    //            if (!gameState.PlayerState.DiscoveredLocationIds.Contains(targetId))
    //            {
    //                gameState.PlayerState.DiscoveredLocationIds.Add(targetId);
    //                gameState.PlayerState.AddKnownLocation(targetId);
    //                MessageSystem.AddSystemMessage($"Discovered new location: {targetId}");
    //            }
    //            break;

    //        case YieldTypes.LocationSpotAccess:
    //            // Unlock a new location spot
    //            if (!gameState.WorldState.KnownLocationSpotIds.Contains(targetId))
    //            {
    //                gameState.WorldState.KnownLocationSpotIds.Add(targetId);
    //                gameState.PlayerState.AddKnownLocationSpot(targetId);
    //                MessageSystem.AddSystemMessage($"Discovered new area: {targetId}");
    //            }
    //            break;

    //        case YieldTypes.ActionUnlock:
    //            // Unlock a new action
    //            if (!gameState.WorldState.UnlockedActionIds.Contains(targetId))
    //            {
    //                gameState.WorldState.UnlockedActionIds.Add(targetId);
    //                MessageSystem.AddSystemMessage($"Learned new action: {targetId}");
    //            }
    //            break;

    //        case YieldTypes.NodeDiscovery:
    //            // Discover a node aspect
    //            if (spot != null)
    //            {
    //                NodeAspectDefinition aspect = spot.DiscoverableAspects
    //                    .FirstOrDefault(a => a.Id == targetId);
    //                if (aspect != null && !aspect.IsDiscovered)
    //                {
    //                    aspect.IsDiscovered = true;
    //                    MessageSystem.AddSystemMessage($"Discovered: {aspect.Id}");

    //                    // Apply any yields from the aspect
    //                    foreach (YieldDefinition aspectYield in aspect.Yields)
    //                    {
    //                        int aspectAmount = CalculateYieldAmount(aspectYield, gameState.PlayerState.PlayerSkills);
    //                        ApplyYield(aspectYield.Type, aspectYield.TargetId, aspectAmount, gameState, location, spot);
    //                    }

    //                    // Give skill XP for discovery
    //                    SkillTypes discoverySkillType = (SkillTypes)Enum.Parse(typeof(SkillTypes), targetId);
    //                    PlayerProgression.AddSkillExp(discoverySkillType, amount);
    //                    MessageSystem.AddSystemMessage($"Gained {amount} {discoverySkillType} skill experience");
    //                    break;
    //                }
    //            }
    //            break;

    //        case YieldTypes.TravelDiscount:
    //            // Add travel discount
    //            if (!gameState.WorldState.TravelDiscounts.ContainsKey(targetId))
    //            {
    //                gameState.WorldState.TravelDiscounts[targetId] = amount;
    //            }
    //            else
    //            {
    //                gameState.WorldState.TravelDiscounts[targetId] += amount;
    //            }
    //            MessageSystem.AddSystemMessage($"Travel to {targetId} now costs {amount} less energy");
    //            break;

    //        case YieldTypes.EncounterChanceReduction:
    //            // Reduce encounter chance
    //            if (!gameState.WorldState.EncounterChanceReductions.ContainsKey(targetId))
    //            {
    //                gameState.WorldState.EncounterChanceReductions[targetId] = amount;
    //            }
    //            else
    //            {
    //                gameState.WorldState.EncounterChanceReductions[targetId] += amount;
    //            }
    //            MessageSystem.AddSystemMessage($"Chance of {targetId} encounters reduced by {amount}%");
    //            break;

    //        case YieldTypes.EfficiencyBoost:
    //            // Add efficiency boost
    //            if (!gameState.WorldState.EfficiencyBoosts.ContainsKey(targetId))
    //            {
    //                gameState.WorldState.EfficiencyBoosts[targetId] = amount;
    //            }
    //            else
    //            {
    //                gameState.WorldState.EfficiencyBoosts[targetId] += amount;
    //            }
    //            MessageSystem.AddSystemMessage($"{targetId} efficiency improved by {amount * 100}%");
    //            break;

    //        case YieldTypes.NodeReplenish:
    //            // Replenish node resources
    //            if (targetId == "global")
    //            {
    //                foreach (Location loc in gameState.WorldState.Locations)
    //                {
    //                    foreach (LocationSpot s in loc.LocationSpots)
    //                    {
    //                        if (s.NodeType != ResourceNodeTypes.None)
    //                        {
    //                            s.CurrentDepletion = Math.Max(0, s.CurrentDepletion - amount);
    //                        }
    //                    }
    //                }
    //                MessageSystem.AddSystemMessage("Resources have been replenished");
    //            }
    //            else if (spot != null && spot.NodeType != ResourceNodeTypes.None)
    //            {
    //                spot.CurrentDepletion = Math.Max(0, spot.CurrentDepletion - amount);
    //                MessageSystem.AddSystemMessage($"Resources at {spot.Name} have been replenished");
    //            }
    //            break;
    //    }
    //}
}