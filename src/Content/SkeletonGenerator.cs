using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Generates mechanically complete but narratively generic skeleton content
/// for missing references. These skeletons allow the game to run without AI
/// while supporting lazy content resolution.
/// </summary>
public static class SkeletonGenerator
{
    private static readonly Random _random = new Random();

    // Generic name templates
    private static readonly string[] GenericNpcNames =
    {
        "Unnamed Merchant", "Unknown Traveler", "Mysterious Figure",
        "Local Resident", "Wandering Soul", "Silent Observer"
    };

    private static readonly string[] GenericLocationNames =
    {
        "Unknown District", "Unexplored Area", "Mysterious Place",
        "Hidden Location", "Undiscovered Region", "Remote Spot"
    };

    private static readonly string[] GenericSpotNames =
    {
        "Central Area", "Quiet Corner", "Main Square",
        "Side Street", "Meeting Point", "Resting Place"
    };

    /// <summary>
    /// Generate a skeleton NPC with mechanical defaults
    /// </summary>
    public static NPC GenerateSkeletonNPC(string id, string source)
    {
        // Use hash of ID for deterministic randomness
        int hash = Math.Abs(id.GetHashCode());
        PersonalityType[] personalityValues = Enum.GetValues<PersonalityType>();
        Professions[] professionValues = Enum.GetValues<Professions>();

        NPC npc = new NPC
        {
            ID = id,
            Name = $"{GenericNpcNames[hash % GenericNpcNames.Length]} #{hash % 100}",
            Description = "This person's story has not yet been told.",
            Role = "Unknown",
            IsSkeleton = true,
            SkeletonSource = source,

            // Random but deterministic mechanical values
            PersonalityType = personalityValues[hash % personalityValues.Length],
            Profession = professionValues[(hash / 7) % professionValues.Length],
            Tier = 1 + (hash % 3), // Tier 1-3

            // Default connection state
            CurrentState = ConnectionState.NEUTRAL,

            // Empty collections
            ProvidedServices = new List<ServiceTypes>(),
            LetterTokenTypes = new List<ConnectionType>()
        };

        // Initialize card decks (required for NPC to function)
        npc.ConversationDeck = new CardDeck();
        npc.RequestDeck = new CardDeck();
        npc.ExchangeDeck = new CardDeck();

        // Add a default service based on profession
        switch (npc.Profession)
        {
            case Professions.Merchant:
                npc.ProvidedServices.Add(ServiceTypes.Trade);
                break;
            case Professions.Innkeeper:
                npc.ProvidedServices.Add(ServiceTypes.Rest);
                break;
            case Professions.Guard:
                npc.ProvidedServices.Add(ServiceTypes.Information);
                break;
        }

        return npc;
    }

    /// <summary>
    /// Generate a skeleton location with mechanical defaults
    /// </summary>
    public static Location GenerateSkeletonLocation(string id, string source)
    {
        int hash = Math.Abs(id.GetHashCode());
        LocationTypes[] locationTypes = Enum.GetValues<LocationTypes>();

        Location location = new Location(id, $"{GenericLocationNames[hash % GenericLocationNames.Length]} #{hash % 100}")
        {
            Description = "This place remains to be discovered.",
            IsSkeleton = true,
            SkeletonSource = source,

            // Random but deterministic mechanical values
            LocationType = locationTypes[hash % locationTypes.Length],
            Tier = 1 + (hash % 3), // Tier 1-3
            TravelTimeMinutes = 10 + (hash % 50), // 10-60 minutes
            Difficulty = 1 + (hash % 3),

            // Empty collections
            DomainTags = new List<string>(),
            AvailableServices = new List<ServiceTypes>(),
            LocationSpotIds = new List<string> { $"{id}_hub" }
        };

        // Add services based on location type
        switch (location.LocationType)
        {
            case LocationTypes.Town:
            case LocationTypes.City:
                location.AvailableServices.Add(ServiceTypes.Trade);
                location.AvailableServices.Add(ServiceTypes.Rest);
                break;
            case LocationTypes.Outpost:
                location.AvailableServices.Add(ServiceTypes.Information);
                break;
        }

        return location;
    }

    /// <summary>
    /// Generate a skeleton location spot with mechanical defaults
    /// </summary>
    public static LocationSpot GenerateSkeletonSpot(string id, string locationId, string source)
    {
        int hash = Math.Abs(id.GetHashCode());

        LocationSpot spot = new LocationSpot(id, $"{GenericSpotNames[hash % GenericSpotNames.Length]} #{hash % 100}")
        {
            LocationId = locationId,
            IsSkeleton = true,
            SkeletonSource = source,

            // Random but deterministic mechanical values
            Tier = 1 + (hash % 3), // Tier 1-3
            FlowModifier = -1 + (hash % 3), // -1 to +1
            PlayerKnowledge = false,

            // Add some random spot properties
            SpotProperties = new List<SpotPropertyType>()
        };

        // Add 1-2 random properties
        SpotPropertyType[] propertyValues = Enum.GetValues<SpotPropertyType>();
        int numProperties = 1 + (hash % 2);
        for (int i = 0; i < numProperties; i++)
        {
            SpotPropertyType property = propertyValues[(hash + i * 13) % propertyValues.Length];
            if (!spot.SpotProperties.Contains(property))
            {
                spot.SpotProperties.Add(property);
            }
        }

        return spot;
    }

    /// <summary>
    /// Generate a skeleton conversation card with mechanical defaults
    /// </summary>
    public static ConversationCard GenerateSkeletonCard(string id, string source)
    {
        int hash = Math.Abs(id.GetHashCode());
        TokenType[] tokenTypes = Enum.GetValues<TokenType>();
        ConnectionType[] connectionTypes = Enum.GetValues<ConnectionType>();

        ConversationCard card = new ConversationCard
        {
            Id = id,
            Description = "The conversation continues.",
            Properties = new List<CardProperty> { CardProperty.Skeleton, CardProperty.Persistent },
            SkeletonSource = source,
            IsSkeleton = true,

            // Random but deterministic mechanical values
            TokenType = tokenTypes[hash % tokenTypes.Length],
            Focus = 1 + (hash % 3),
            Difficulty = (Difficulty)(hash % 3), // Easy, Medium, or Hard

            // Generic dialogue
            DialogueFragment = "You discuss matters of mutual interest.",
            VerbPhrase = "converse about topics"
        };

        return card;
    }

    /// <summary>
    /// Check if an entity needs a skeleton and generate it
    /// </summary>
    public static bool TryGenerateSkeleton<T>(string id, string source, GameWorld gameWorld, out T skeleton) where T : class
    {
        skeleton = null;

        if (typeof(T) == typeof(NPC))
        {
            if (!gameWorld.NPCs.Any(n => n.ID == id))
            {
                skeleton = GenerateSkeletonNPC(id, source) as T;
                return true;
            }
        }
        else if (typeof(T) == typeof(Location))
        {
            if (!gameWorld.WorldState.locations.Any(l => l.Id == id))
            {
                skeleton = GenerateSkeletonLocation(id, source) as T;
                return true;
            }
        }
        else if (typeof(T) == typeof(LocationSpot))
        {
            if (!gameWorld.WorldState.locationSpots.Any(s => s.SpotID == id))
            {
                // Need to find or create parent location first
                string locationId = source.Contains("location_")
                    ? source.Substring(source.IndexOf("location_") + 9).Split('_')[0]
                    : "unknown_location";
                skeleton = GenerateSkeletonSpot(id, locationId, source) as T;
                return true;
            }
        }

        return false;
    }
}