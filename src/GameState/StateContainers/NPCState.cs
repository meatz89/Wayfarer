using System.Collections.Immutable;

/// <summary>
/// Immutable state container for NPC data.
/// All modifications must go through operations/commands.
/// </summary>
public sealed class NPCState
{
// Identity
public string ID { get; }
public string Name { get; }
public string Role { get; }
public string Description { get; }
public string LocationId { get; }

// Categorical Properties
public Professions Profession { get; }
public NPCRelationship PlayerRelationship { get; }

public NPCState(
    string id,
    string name,
    string role,
    string description,
    string locationId,
    Professions profession,
    NPCRelationship playerRelationship)
{
    ID = id;
    Name = name;
    Role = role;
    Description = description;
    LocationId = locationId;
    Profession = profession;
    PlayerRelationship = playerRelationship;
}

/// <summary>
/// Creates a new NPCState with updated relationship.
/// </summary>
public NPCState WithRelationship(NPCRelationship relationship)
{
    return new NPCState(
    ID, Name, Role, Description, LocationId,
    Profession, relationship);
}

/// <summary>
/// Creates a new NPCState with updated location.
/// </summary>
public NPCState WithLocation(string newSpotId)
{
    return new NPCState(
    ID, Name, Role, Description, newSpotId,
    Profession, PlayerRelationship);
}

/// <summary>
/// Creates an NPCState from a mutable NPC object.
/// </summary>
public static NPCState FromNPC(NPC npc)
{
    return new NPCState(
        npc.ID,
        npc.Name,
        npc.Role,
        npc.Description,
        npc.Location?.Id,
        npc.Profession,
        npc.PlayerRelationship);
}

/// <summary>
/// Helper properties for UI display
/// </summary>
public string ProfessionDescription => Profession.ToString().Replace('_', ' ');

public string ScheduleDescription => "Always available";

public bool IsAvailable(TimeBlocks currentTime)
{
    return true; // NPCs are always available
}
}