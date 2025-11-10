/// <summary>
/// Catalogue for translating categorical clearConditions strings → strongly-typed StateClearingBehavior
///
/// ARCHITECTURE: Catalogue + Resolver Pattern (Parse-Time Translation)
/// - Called ONCE by StateParser during initialization
/// - Returns StateClearingBehavior object stored on State entity
/// - NEVER called at runtime (zero overhead)
/// - Fail-fast: Throws on unknown strings (catches JSON typos at parse time)
///
/// Parallel to SocialCardEffectCatalog:
/// - SocialCardEffectCatalog: Categorical properties → CardEffectFormula
/// - StateClearConditionsCatalog: Categorical strings → StateClearingBehavior
/// </summary>
public static class StateClearConditionsCatalog
{
/// <summary>
/// Translate categorical clearConditions strings to strongly-typed StateClearingBehavior
/// Called by StateParser.ConvertDTOToState() during game initialization
/// </summary>
/// <param name="clearConditions">List of categorical strings from 18_states.json</param>
/// <returns>Strongly-typed behavior object describing HOW state can be cleared</returns>
public static StateClearingBehavior GetClearingBehavior(List<string> clearConditions)
{
    StateClearingBehavior behavior = new StateClearingBehavior();

    if (clearConditions == null || clearConditions.Count == 0)
    {
        // No clearing conditions = state can only be manually cleared
        behavior.AllowsManualClear = true;
        return behavior;
    }

    foreach (string condition in clearConditions)
    {
        switch (condition)
        {
            // ========================================
            // REST CLEARING (ResourceFacade)
            // ========================================
            case "Rest":
                behavior.ClearsOnRest = true;
                break;

            case "RestAtSafeLocation":
                behavior.ClearsOnRest = true;
                behavior.RequiresSafeLocation = true;
                break;

            // ========================================
            // ITEM CONSUMPTION (ResourceFacade)
            // ========================================
            case "UseMedicalSupplies":
                behavior.ClearingItemTypes.Add(ItemType.Medical);
                break;

            case "UseRemedy":
                behavior.ClearingItemTypes.Add(ItemType.Remedy);
                break;

            case "ConsumeFood":
                behavior.ClearingItemTypes.Add(ItemType.Food);
                break;

            case "ConsumeProvisions":
                behavior.ClearingItemTypes.Add(ItemType.Provisions);
                break;

            // ========================================
            // CHALLENGE COMPLETION (Challenge Facades)
            // ========================================
            case "CompleteMentalChallenge":
            case "CompletePhysicalChallenge":
            case "CompleteSocialChallenge":
                // All challenge types map to same property (success clears state)
                behavior.ClearsOnChallengeSuccess = true;
                break;

            case "FailChallenge":
                behavior.ClearsOnChallengeFailure = true;
                break;

            // ========================================
            // TIME PASSAGE (TimeFacade)
            // ========================================
            case "TimePassage":
                // Duration-based clearing uses State.Duration property directly
                // This condition is informational only (indicates time-based clearing exists)
                // TimeFacade checks Duration, not ClearingBehavior
                break;

            // ========================================
            // MANUAL CLEARING (Player Action Facade)
            // ========================================
            case "Manual":
                behavior.AllowsManualClear = true;
                break;

            // ========================================
            // SOCIAL EVENTS (ConsequenceFacade - Phase 6)
            // ========================================
            case "ReceiveComfort":
                behavior.ClearingSocialEventTypes.Add(SocialEventType.ReceiveComfort);
                break;

            case "BetrayTrust":
                behavior.ClearingSocialEventTypes.Add(SocialEventType.BetrayTrust);
                break;

            case "RemoveDisguise":
                behavior.ClearingSocialEventTypes.Add(SocialEventType.RemoveDisguise);
                break;

            case "IdentityRevealed":
                behavior.ClearingSocialEventTypes.Add(SocialEventType.IdentityRevealed);
                break;

            // ========================================
            // PENALTY RESOLUTION (ConsequenceFacade - Phase 6)
            // ========================================
            case "PayFine":
                behavior.ClearingPenaltyTypes.Add(PenaltyResolutionType.PayFine);
                break;

            case "RepayDebt":
                behavior.ClearingPenaltyTypes.Add(PenaltyResolutionType.RepayDebt);
                break;

            case "ServeTime":
                behavior.ClearingPenaltyTypes.Add(PenaltyResolutionType.ServeTime);
                break;

            // ========================================
            // QUEST COMPLETION (ConsequenceFacade - Phase 6)
            // ========================================
            case "ClearName":
                behavior.ClearingQuestTypes.Add(QuestCompletionType.ClearName);
                break;

            case "RestoreReputation":
                behavior.ClearingQuestTypes.Add(QuestCompletionType.RestoreReputation);
                break;

            case "RestoreHonor":
                behavior.ClearingQuestTypes.Add(QuestCompletionType.RestoreHonor);
                break;

            case "AchieveObsessionGoal":
                behavior.ClearingQuestTypes.Add(QuestCompletionType.AchieveGoal);
                break;

            // ========================================
            // UNKNOWN CONDITION (FAIL FAST)
            // ========================================
            default:
                throw new InvalidDataException(
                    $"Unknown clearCondition: '{condition}'. " +
                    $"Valid conditions: Rest, RestAtSafeLocation, UseMedicalSupplies, UseRemedy, ConsumeFood, ConsumeProvisions, " +
                    $"CompleteMentalChallenge, CompletePhysicalChallenge, CompleteSocialChallenge, FailChallenge, " +
                    $"TimePassage, Manual, ReceiveComfort, BetrayTrust, RemoveDisguise, IdentityRevealed, " +
                    $"PayFine, RepayDebt, ServeTime, ClearName, RestoreReputation, RestoreHonor, AchieveObsessionGoal");
        }
    }

    return behavior;
}
}
