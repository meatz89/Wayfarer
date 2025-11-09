/// <summary>
/// DTO for ChoiceReward - consequences HIDDEN until Choice selected
/// Sir Brante pattern: Player commits to action before seeing results
/// Maps to ChoiceReward domain entity
/// </summary>
public class ChoiceRewardDTO
{
/// <summary>
/// Currency grant (positive) or penalty (negative)
/// 0 = no currency change
/// </summary>
public int Coins { get; set; } = 0;

/// <summary>
/// Willpower grant (positive) or penalty (negative)
/// 0 = no Resolve change
/// </summary>
public int Resolve { get; set; } = 0;

/// <summary>
/// Time advancement (positive) or rewind (negative, rare)
/// 0 = no time change (beyond cost already paid)
/// </summary>
public int TimeSegments { get; set; } = 0;

/// <summary>
/// Advance to specific time block (overrides TimeSegments)
/// Values: "Morning", "Midday", "Afternoon", "Evening"
/// null = use TimeSegments instead
/// </summary>
public string? AdvanceToBlock { get; set; }

/// <summary>
/// Advance to next day
/// Values: "CurrentDay", "NextDay"
/// null = stay on current day
/// </summary>
public string? AdvanceToDay { get; set; }

/// <summary>
/// Health grant (positive) or penalty (negative)
/// Rest and healing restore health
/// 0 = no health change
/// </summary>
public int Health { get; set; } = 0;

/// <summary>
/// Hunger change (positive increases hunger, negative decreases)
/// Eating food reduces hunger
/// 0 = no hunger change
/// </summary>
public int Hunger { get; set; } = 0;

/// <summary>
/// Stamina grant (positive) or penalty (negative)
/// Rest restores stamina
/// 0 = no stamina change
/// </summary>
public int Stamina { get; set; } = 0;

/// <summary>
/// Focus grant (positive) or penalty (negative)
/// Rest restores focus
/// 0 = no focus change
/// </summary>
public int Focus { get; set; } = 0;

/// <summary>
/// Full recovery flag - restores all resources to maximum
/// Used for securing room at inn
/// false = normal resource grants, true = full recovery
/// </summary>
public bool FullRecovery { get; set; } = false;

/// <summary>
/// Relationship changes (transparent before selection)
/// +/- bond deltas with NPCs
/// </summary>
public List<BondChangeDTO> BondChanges { get; set; } = new List<BondChangeDTO>();

/// <summary>
/// Behavioral reputation changes (transparent before selection)
/// +/- deltas on Morality, Lawfulness, Method, etc. scales
/// </summary>
public List<ScaleShiftDTO> ScaleShifts { get; set; } = new List<ScaleShiftDTO>();

/// <summary>
/// Temporary condition applications/removals (transparent before selection)
/// Apply or remove states like "Injured", "Suspicious", "Blessed"
/// </summary>
public List<StateApplicationDTO> StateApplications { get; set; } = new List<StateApplicationDTO>();

/// <summary>
/// Achievement IDs to grant
/// Milestone unlocks
/// </summary>
public List<string> AchievementIds { get; set; } = new List<string>();

/// <summary>
/// Item IDs to grant
/// Equipment, consumables, quest items
/// </summary>
public List<string> ItemIds { get; set; } = new List<string>();

/// <summary>
/// Item IDs to remove from player inventory
/// Used for consuming keys, removing temporary access tokens
/// Part of item lifecycle: REMOVE phase
/// </summary>
public List<string> ItemsToRemove { get; set; } = new List<string>();

/// <summary>
/// Location IDs to unlock (set IsLocked = false)
/// Multi-Situation Scene Pattern: unlock upper_floor when key acquired
/// </summary>
public List<string> LocationsToUnlock { get; set; } = new List<string>();

/// <summary>
/// Location IDs to lock (set IsLocked = true)
/// Multi-Situation Scene Pattern: relock upper_floor on departure
/// </summary>
public List<string> LocationsToLock { get; set; } = new List<string>();

/// <summary>
/// Scenes to spawn (cascading narrative)
/// Procedural Scene generation with placement strategies
/// PERFECT INFORMATION: Created eagerly, player sees WHERE Scene spawns BEFORE selecting Choice
/// </summary>
public List<SceneSpawnRewardDTO> ScenesToSpawn { get; set; } = new List<SceneSpawnRewardDTO>();
}
