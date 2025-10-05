using System.Collections.Generic;

/// <summary>
/// V3 Investigation Phase - fixed sequential progression
/// Each phase has dedicated card deck and Progress threshold
/// </summary>
public class InvestigationPhase
{
    public string Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public string Goal { get; init; } // What player is trying to accomplish

    public int ProgressThreshold { get; init; } // Progress required to complete (10, 15, 20 typical)

    public TacticalSystemType SystemType { get; init; } // Which tactical system to spawn

    // THREE PARALLEL SYSTEMS: Which engagement type within that system
    public string EngagementTypeId { get; init; }

    public List<string> CardDeckIds { get; init; } = new List<string>(); // Card IDs available in this phase

    // Requirements to unlock this phase
    public PhaseRequirements Requirements { get; init; } = new PhaseRequirements();

    // Completion reward
    public PhaseCompletionReward CompletionReward { get; init; }
}

public class PhaseRequirements
{
    public List<int> CompletedPhases { get; set; } = new List<int>(); // Phase indices that must be complete
    public Dictionary<DiscoveryType, int> DiscoveryQuantities { get; set; } = new Dictionary<DiscoveryType, int>(); // {Structural: 3}
    public List<string> SpecificDiscoveries { get; set; } = new List<string>(); // Specific discovery IDs
    public List<string> Equipment { get; set; } = new List<string>();
    public List<string> Knowledge { get; set; } = new List<string>();
}

public class PhaseCompletionReward
{
    public string Narrative { get; set; } // Authored description of completion
    public List<string> DiscoveriesGranted { get; set; } = new List<string>(); // Major discoveries granted
    public string UnlocksPhaseId { get; set; } // Next phase ID (if applicable)
}
