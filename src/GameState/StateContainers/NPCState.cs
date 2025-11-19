using System.Collections.Immutable;

/// <summary>
/// Immutable state container for NPC data.
/// All modifications must go through operations/commands.
/// HIGHLANDER: No ID property, object references only
/// </summary>
public sealed class NPCState
{
    // Identity
    public string Name { get; }
    public string Role { get; }
    public string Description { get; }
    public Location Location { get; }

    // Categorical Properties
    public Professions Profession { get; }
    public NPCRelationship PlayerRelationship { get; }

    public NPCState(
        string name,
        string role,
        string description,
        Location location,
        Professions profession,
        NPCRelationship playerRelationship)
    {
        Name = name;
        Role = role;
        Description = description;
        Location = location;
        Profession = profession;
        PlayerRelationship = playerRelationship;
    }

    /// <summary>
    /// Creates a new NPCState with updated relationship.
    /// </summary>
    public NPCState WithRelationship(NPCRelationship relationship)
    {
        return new NPCState(
        Name, Role, Description, Location,
        Profession, relationship);
    }

    /// <summary>
    /// Creates a new NPCState with updated location.
    /// HIGHLANDER: Accept Location object
    /// </summary>
    public NPCState WithLocation(Location newLocation)
    {
        return new NPCState(
        Name, Role, Description, newLocation,
        Profession, PlayerRelationship);
    }

    /// <summary>
    /// Creates an NPCState from a mutable NPC object.
    /// HIGHLANDER: Pass Location object
    /// </summary>
    public static NPCState FromNPC(NPC npc)
    {
        return new NPCState(
            npc.Name,
            npc.Role,
            npc.Description,
            npc.Location,
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