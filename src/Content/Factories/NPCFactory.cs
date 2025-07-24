using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Factory for creating NPCs with guaranteed valid references.
/// NPCs reference their Location by ID string.
/// </summary>
public class NPCFactory
{
    public NPCFactory()
    {
        // No dependencies - factory is stateless
    }
    
    /// <summary>
    /// Create a minimal NPC with just an ID.
    /// Used for dummy/placeholder creation when references are missing.
    /// </summary>
    public NPC CreateMinimalNPC(string id, string locationId = null)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("NPC ID cannot be empty", nameof(id));
            
        var name = FormatIdAsName(id);
        
        return new NPC
        {
            ID = id,
            Name = name,
            Location = locationId ?? "unknown_location",
            Profession = Professions.Merchant, // Default profession
            Description = $"{name}, a local merchant",
            LetterTokenTypes = new List<ConnectionType> { ConnectionType.Common }
        };
    }
    
    private string FormatIdAsName(string id)
    {
        // Convert snake_case or kebab-case to Title Case
        return string.Join(" ", 
            id.Replace('_', ' ').Replace('-', ' ')
              .Split(' ')
              .Select(word => string.IsNullOrEmpty(word) ? "" : 
                  char.ToUpper(word[0]) + word.Substring(1).ToLower()));
    }

    /// <summary>
    /// Create an NPC with validated location reference
    /// </summary>
    public NPC CreateNPC(
        string id,
        string name,
        Location location,  // Not string - actual Location object
        Professions profession,
        string spotId,
        string role,
        string description,
        List<ServiceTypes> providedServices,
        List<ConnectionType> letterTokenTypes)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("NPC ID cannot be empty", nameof(id));
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("NPC name cannot be empty", nameof(name));
        if (location == null)
            throw new ArgumentNullException(nameof(location), "Location cannot be null");

        NPC npc = new NPC
        {
            ID = id,
            Name = name,
            Location = location.Id,  // Extract ID from validated object
            SpotId = spotId,
            Profession = profession,
            Role = role ?? profession.ToString().Replace('_', ' '),
            Description = description ?? $"A {profession} in {location.Name}",
            ProvidedServices = providedServices ?? new List<ServiceTypes>(),
            LetterTokenTypes = letterTokenTypes ?? new List<ConnectionType>()
        };

        return npc;
    }

    /// <summary>
    /// Create an NPC from string IDs with validation
    /// </summary>
    public NPC CreateNPCFromIds(
        string id,
        string name,
        string locationId,
        IEnumerable<Location> availableLocations,
        Professions profession,
        string spotId,
        string role,
        string description,
        List<ServiceTypes> providedServices,
        List<ConnectionType> letterTokenTypes)
    {
        // Resolve location
        Location? location = availableLocations.FirstOrDefault(l => l.Id == locationId);
        if (location == null)
            throw new InvalidOperationException($"Cannot create NPC: location '{locationId}' not found");

        return CreateNPC(id, name, location, profession, spotId, role, description,
                        providedServices, letterTokenTypes);
    }

    /// <summary>
    /// Validate that an NPC's location exists.
    /// This should be called after all locations are loaded.
    /// </summary>
    public static bool ValidateNPCLocation(NPC npc, IEnumerable<Location> availableLocations)
    {
        bool locationExists = availableLocations.Any(l => l.Id == npc.Location);
        if (!locationExists)
        {
            Console.WriteLine($"WARNING: NPC '{npc.Name}' references non-existent location '{npc.Location}'");
        }
        return locationExists;
    }
}