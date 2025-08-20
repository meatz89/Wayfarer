using System.Collections.Generic;
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
    public string Location { get; }
    public string SpotId { get; }

    // Categorical Properties
    public Professions Profession { get; }
    public ImmutableList<ServiceTypes> ProvidedServices { get; }
    public NPCRelationship PlayerRelationship { get; }

    // DeliveryObligation Queue Properties
    public ImmutableList<ConnectionType> LetterTokenTypes { get; }

    public NPCState(
        string id,
        string name,
        string role,
        string description,
        string location,
        string spotId,
        Professions profession,
        IEnumerable<ServiceTypes> providedServices,
        NPCRelationship playerRelationship,
        IEnumerable<ConnectionType> letterTokenTypes)
    {
        ID = id;
        Name = name;
        Role = role;
        Description = description;
        Location = location;
        SpotId = spotId;
        Profession = profession;
        ProvidedServices = providedServices?.ToImmutableList() ?? ImmutableList<ServiceTypes>.Empty;
        PlayerRelationship = playerRelationship;
        LetterTokenTypes = letterTokenTypes?.ToImmutableList() ?? ImmutableList<ConnectionType>.Empty;
    }

    /// <summary>
    /// Creates a new NPCState with updated relationship.
    /// </summary>
    public NPCState WithRelationship(NPCRelationship relationship)
    {
        return new NPCState(
        ID, Name, Role, Description, Location, SpotId,
        Profession, ProvidedServices, relationship, LetterTokenTypes);
    }

    /// <summary>
    /// Creates a new NPCState with updated location.
    /// </summary>
    public NPCState WithLocation(string newLocation, string newSpotId)
    {
        return new NPCState(
        ID, Name, Role, Description, newLocation, newSpotId,
        Profession, ProvidedServices, PlayerRelationship, LetterTokenTypes);
    }

    /// <summary>
    /// Creates a new NPCState with added service.
    /// </summary>
    public NPCState WithAddedService(ServiceTypes service)
    {
        return new NPCState(
        ID, Name, Role, Description, Location, SpotId,
        Profession, ProvidedServices.Add(service), PlayerRelationship, LetterTokenTypes);
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
            npc.Location,
            npc.SpotId,
            npc.Profession,
            npc.ProvidedServices,
            npc.PlayerRelationship,
            npc.LetterTokenTypes);
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

    public bool CanProvideService(ServiceTypes requestedService)
    {
        return ProvidedServices.Contains(requestedService);
    }
}