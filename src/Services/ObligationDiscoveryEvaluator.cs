/// <summary>
/// Evaluates which obligations can be discovered based on game state
/// STATELESS service - all state in GameWorld
/// Follows ARCHITECTURE.md principles: service operates on GameWorld, doesn't store state
/// </summary>
public class ObligationDiscoveryEvaluator
{
    private readonly GameWorld _gameWorld;

    public ObligationDiscoveryEvaluator(GameWorld gameWorld)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
    }

    /// <summary>
    /// Evaluate all Potential obligations and return those ready to be discovered
    /// Called on: Venue entry, knowledge gain, item acquisition, obligation acceptance
    /// </summary>
    public List<Obligation> EvaluateDiscoverableObligations()
    {
        List<Obligation> discoverable = new List<Obligation>();

        // HIGHLANDER: Object references ONLY, iterate over objects directly
        foreach (Obligation obligation in _gameWorld.ObligationJournal.PotentialObligations)
        {
            if (obligation == null)
            {
                continue;
            }

            // Skip if no intro action defined
            if (obligation.IntroAction == null)
            {
                continue;
            }

            if (IsTriggerConditionMet(obligation))
            {
                discoverable.Add(obligation);
            }
        }

        return discoverable;
    }

    /// <summary>
    /// Check if specific obligation's trigger condition is met
    /// </summary>
    private bool IsTriggerConditionMet(Obligation obligation)
    {
        ObligationPrerequisites prereqs = obligation.IntroAction.TriggerPrerequisites;
        if (prereqs == null) return true; // No prerequisites = always available

        // Check prerequisites based on trigger type
        return obligation.IntroAction.TriggerType switch
        {
            DiscoveryTriggerType.ImmediateVisibility => CheckImmediateVisibility(prereqs),
            DiscoveryTriggerType.EnvironmentalObservation => CheckEnvironmentalObservation(prereqs),
            _ => false
        };
    }

    /// <summary>
    /// ImmediateVisibility: Player is at required location
    /// HIGHLANDER: Prerequisites.Location is object reference, not string ID
    /// </summary>
    private bool CheckImmediateVisibility(ObligationPrerequisites prereqs)
    {
        // Check if player is at required location
        if (prereqs.Location != null)
        {
            // Compare Location objects directly (HIGHLANDER)
            if (_gameWorld.GetPlayerCurrentLocation() != prereqs.Location)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// EnvironmentalObservation: Player is at required location
    /// HIGHLANDER: Prerequisites.Location is object reference, not string ID
    /// </summary>
    private bool CheckEnvironmentalObservation(ObligationPrerequisites prereqs)
    {
        // Check if player is at required location
        if (prereqs.Location != null)
        {
            // Compare Location objects directly (HIGHLANDER)
            if (_gameWorld.GetPlayerCurrentLocation() != prereqs.Location)
                return false;
        }

        return true;
    }
}
