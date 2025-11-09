/// <summary>
/// Runtime projection service for state clearing decisions
///
/// ARCHITECTURE: Catalogue + Resolver Pattern (Runtime Projection)
/// - Injected into facades via DI
/// - Methods return projections (what SHOULD be cleared)
/// - Does NOT modify state directly (no side effects)
/// - Centralizes ALL state clearing logic (single source of truth)
/// - Enables UI preview, testing, consistent decisions
///
/// Parallel to SocialEffectResolver:
/// - SocialEffectResolver: Projects card effects (what WOULD happen)
/// - StateClearingResolver: Projects state clearing (what states SHOULD clear)
///
/// Usage Pattern:
/// 1. Facade calls resolver method to get projection
/// 2. Resolver queries Player.ActiveStates and State definitions
/// 3. Resolver returns List<StateType> of states that should be cleared
/// 4. Facade applies clearing (removes from Player.ActiveStates)
/// 5. Facade triggers cascade (SpawnFacade.EvaluateDormantSituations)
/// </summary>
public class StateClearingResolver
{
private readonly GameWorld _gameWorld;

public StateClearingResolver(GameWorld gameWorld)
{
    _gameWorld = gameWorld;
}

/// <summary>
/// Get which states should be cleared when player rests
/// Projection: Does NOT modify Player.ActiveStates
/// </summary>
/// <param name="isSafeLocation">Whether rest is happening at a safe location</param>
/// <returns>List of state types that should be cleared</returns>
public List<StateType> GetStatesToClearOnRest(bool isSafeLocation)
{
    List<StateType> statesToClear = new List<StateType>();

    foreach (ActiveState activeState in _gameWorld.GetPlayer().ActiveStates)
    {
        State stateDef = _gameWorld.GetStateDefinition(activeState.Type);
        if (stateDef == null || stateDef.ClearingBehavior == null)
            continue;

        // Check if state clears on rest
        if (!stateDef.ClearingBehavior.ClearsOnRest)
            continue;

        // If requires safe location, check safety requirement
        if (stateDef.ClearingBehavior.RequiresSafeLocation && !isSafeLocation)
            continue;

        statesToClear.Add(activeState.Type);
    }

    return statesToClear;
}

/// <summary>
/// Get which states should be cleared when player consumes an item
/// Projection: Does NOT modify Player.ActiveStates
/// </summary>
/// <param name="itemType">Type of item being consumed</param>
/// <returns>List of state types that should be cleared</returns>
public List<StateType> GetStatesToClearOnItemConsumption(ItemType itemType)
{
    List<StateType> statesToClear = new List<StateType>();

    foreach (ActiveState activeState in _gameWorld.GetPlayer().ActiveStates)
    {
        State stateDef = _gameWorld.GetStateDefinition(activeState.Type);
        if (stateDef == null || stateDef.ClearingBehavior == null)
            continue;

        // Check if this item type clears the state
        if (stateDef.ClearingBehavior.ClearingItemTypes.Contains(itemType))
        {
            statesToClear.Add(activeState.Type);
        }
    }

    return statesToClear;
}

/// <summary>
/// Get which states should be cleared when time passes (duration-based auto-clearing)
/// Projection: Does NOT modify Player.ActiveStates
/// </summary>
/// <param name="currentDay">Current day</param>
/// <param name="currentTimeBlock">Current time block</param>
/// <param name="currentSegment">Current segment within time block</param>
/// <returns>List of state types that should be cleared</returns>
public List<StateType> GetStatesToClearOnTimePassage(int currentDay, TimeBlocks currentTimeBlock, int currentSegment)
{
    List<StateType> statesToClear = new List<StateType>();

    foreach (ActiveState activeState in _gameWorld.GetPlayer().ActiveStates)
    {
        State stateDef = _gameWorld.GetStateDefinition(activeState.Type);
        if (stateDef == null)
            continue;

        // Check if state has duration-based clearing
        if (!stateDef.Duration.HasValue)
            continue;

        // Check if duration has elapsed
        if (activeState.ShouldAutoClear(currentDay, currentTimeBlock, currentSegment))
        {
            statesToClear.Add(activeState.Type);
        }
    }

    return statesToClear;
}

/// <summary>
/// Get which states should be cleared when player successfully completes a challenge
/// Projection: Does NOT modify Player.ActiveStates
/// </summary>
/// <returns>List of state types that should be cleared</returns>
public List<StateType> GetStatesToClearOnChallengeSuccess()
{
    List<StateType> statesToClear = new List<StateType>();

    foreach (ActiveState activeState in _gameWorld.GetPlayer().ActiveStates)
    {
        State stateDef = _gameWorld.GetStateDefinition(activeState.Type);
        if (stateDef == null || stateDef.ClearingBehavior == null)
            continue;

        if (stateDef.ClearingBehavior.ClearsOnChallengeSuccess)
        {
            statesToClear.Add(activeState.Type);
        }
    }

    return statesToClear;
}

/// <summary>
/// Get which states should be cleared when player fails a challenge
/// Projection: Does NOT modify Player.ActiveStates
/// </summary>
/// <returns>List of state types that should be cleared</returns>
public List<StateType> GetStatesToClearOnChallengeFailure()
{
    List<StateType> statesToClear = new List<StateType>();

    foreach (ActiveState activeState in _gameWorld.GetPlayer().ActiveStates)
    {
        State stateDef = _gameWorld.GetStateDefinition(activeState.Type);
        if (stateDef == null || stateDef.ClearingBehavior == null)
            continue;

        if (stateDef.ClearingBehavior.ClearsOnChallengeFailure)
        {
            statesToClear.Add(activeState.Type);
        }
    }

    return statesToClear;
}

/// <summary>
/// Get which states can be manually cleared by player
/// Used by UI to show "Clear State" options
/// Projection: Does NOT modify Player.ActiveStates
/// </summary>
/// <returns>List of state types that can be manually cleared</returns>
public List<StateType> GetManuallyClearableStates()
{
    List<StateType> manualStates = new List<StateType>();

    foreach (ActiveState activeState in _gameWorld.GetPlayer().ActiveStates)
    {
        State stateDef = _gameWorld.GetStateDefinition(activeState.Type);
        if (stateDef == null || stateDef.ClearingBehavior == null)
            continue;

        if (stateDef.ClearingBehavior.AllowsManualClear)
        {
            manualStates.Add(activeState.Type);
        }
    }

    return manualStates;
}

// ========================================
// PHASE 6 METHODS (ConsequenceFacade Integration)
// ========================================

/// <summary>
/// Get which states should be cleared when player resolves a penalty
/// Projection: Does NOT modify Player.ActiveStates
/// Phase 6 implementation
/// </summary>
/// <param name="penaltyType">Type of penalty being resolved</param>
/// <returns>List of state types that should be cleared</returns>
public List<StateType> GetStatesToClearOnPenaltyResolution(PenaltyResolutionType penaltyType)
{
    List<StateType> statesToClear = new List<StateType>();

    foreach (ActiveState activeState in _gameWorld.GetPlayer().ActiveStates)
    {
        State stateDef = _gameWorld.GetStateDefinition(activeState.Type);
        if (stateDef == null || stateDef.ClearingBehavior == null)
            continue;

        if (stateDef.ClearingBehavior.ClearingPenaltyTypes.Contains(penaltyType))
        {
            statesToClear.Add(activeState.Type);
        }
    }

    return statesToClear;
}

/// <summary>
/// Get which states should be cleared when player completes a quest
/// Projection: Does NOT modify Player.ActiveStates
/// Phase 6 implementation
/// </summary>
/// <param name="questType">Type of quest being completed</param>
/// <returns>List of state types that should be cleared</returns>
public List<StateType> GetStatesToClearOnQuestCompletion(QuestCompletionType questType)
{
    List<StateType> statesToClear = new List<StateType>();

    foreach (ActiveState activeState in _gameWorld.GetPlayer().ActiveStates)
    {
        State stateDef = _gameWorld.GetStateDefinition(activeState.Type);
        if (stateDef == null || stateDef.ClearingBehavior == null)
            continue;

        if (stateDef.ClearingBehavior.ClearingQuestTypes.Contains(questType))
        {
            statesToClear.Add(activeState.Type);
        }
    }

    return statesToClear;
}

/// <summary>
/// Get which states should be cleared when a social event occurs
/// Projection: Does NOT modify Player.ActiveStates
/// Phase 6 implementation
/// </summary>
/// <param name="eventType">Type of social event occurring</param>
/// <returns>List of state types that should be cleared</returns>
public List<StateType> GetStatesToClearOnSocialEvent(SocialEventType eventType)
{
    List<StateType> statesToClear = new List<StateType>();

    foreach (ActiveState activeState in _gameWorld.GetPlayer().ActiveStates)
    {
        State stateDef = _gameWorld.GetStateDefinition(activeState.Type);
        if (stateDef == null || stateDef.ClearingBehavior == null)
            continue;

        if (stateDef.ClearingBehavior.ClearingSocialEventTypes.Contains(eventType))
        {
            statesToClear.Add(activeState.Type);
        }
    }

    return statesToClear;
}
}
