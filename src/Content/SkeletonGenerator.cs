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

            // Default relationship flow (NEUTRAL state at neutral position)
            RelationshipFlow = 12,

            // Empty collections
            ProvidedServices = new List<ServiceTypes>(),
        };

        // Initialize card decks (required for NPC to function)
        npc.Requests = new List<GoalCard>();
        npc.ExchangeDeck = new List<ExchangeCard>();

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
    /// Generate a skeleton venue with mechanical defaults
    /// Venue is a CONTAINER - gameplay properties belong on LocationSpot
    /// </summary>
    public static Venue GenerateSkeletonVenue(string id, string source)
    {
        int hash = Math.Abs(id.GetHashCode());
        LocationSpotTypes[] locationTypes = Enum.GetValues<LocationSpotTypes>();
        LocationSpotTypes selectedType = locationTypes[hash % locationTypes.Length];

        Venue venue = new Venue(id, $"{GenericLocationNames[hash % GenericLocationNames.Length]} #{hash % 100}")
        {
            Description = "This place remains to be discovered.",
            IsSkeleton = true,
            SkeletonSource = source,
            Tier = 1 + (hash % 3), // Organizational tier 1-3
            LocationTypeString = selectedType.ToString(), // Display string only
            LocationSpotIds = new List<string> { $"{id}_hub" } // Reference to hub spot
        };

        return venue;
    }

    /// <summary>
    /// Generate a skeleton Venue spot with mechanical defaults
    /// LocationSpot is the gameplay entity with all mechanical properties
    /// </summary>
    public static LocationSpot GenerateSkeletonSpot(string id, string venueId, string source)
    {
        int hash = Math.Abs(id.GetHashCode());
        LocationSpotTypes[] locationTypes = Enum.GetValues<LocationSpotTypes>();
        LocationSpotTypes selectedType = locationTypes[hash % locationTypes.Length];

        LocationSpot spot = new LocationSpot(id, $"{GenericSpotNames[hash % GenericSpotNames.Length]} #{hash % 100}")
        {
            VenueId = venueId,
            IsSkeleton = true,
            SkeletonSource = source,

            // Random but deterministic mechanical values
            Tier = 1 + (hash % 3), // Tier 1-3
            FlowModifier = -1 + (hash % 3), // -1 to +1
            PlayerKnowledge = false,

            // Gameplay properties moved from Location
            LocationType = selectedType,
            TravelTimeSegments = 1 + (hash % 5), // 1-5 segments
            Difficulty = 1 + (hash % 3), // 1-3
            DomainTags = new List<string>(),
            AvailableServices = new List<ServiceTypes>(),

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

        // Add services based on Venue type (moved from GenerateSkeletonLocation)
        switch (spot.LocationType)
        {
            case LocationSpotTypes.Town:
            case LocationSpotTypes.City:
                spot.AvailableServices.Add(ServiceTypes.Trade);
                spot.AvailableServices.Add(ServiceTypes.Rest);
                break;
            case LocationSpotTypes.Outpost:
                spot.AvailableServices.Add(ServiceTypes.Information);
                break;
        }

        return spot;
    }


    /// <summary>
    /// Generate a skeleton exchange card with mechanical defaults
    /// </summary>
    public static ExchangeCard GenerateSkeletonExchangeCard(string id, string npcId, string source)
    {
        int hash = Math.Abs(id.GetHashCode());
        ExchangeType[] exchangeTypes = Enum.GetValues<ExchangeType>();

        ExchangeCard card = new ExchangeCard
        {
            Id = id,
            Name = $"Exchange #{hash % 100}",
            Description = "A simple trade of resources.",
            IsSkeleton = true,
            SkeletonSource = source,
            NpcId = npcId,

            // Random but deterministic exchange type
            ExchangeType = exchangeTypes[hash % exchangeTypes.Length],

            // Simple cost structure (coins for now)
            Cost = new ExchangeCostStructure
            {
                Resources = new List<ResourceAmount>
                {
                    new ResourceAmount { Type = ResourceType.Coins, Amount = 5 + (hash % 10) } // 5-14 coins
                }
            },

            // Simple reward structure
            Reward = new ExchangeRewardStructure
            {
                Resources = new List<ResourceAmount>
                {
                    new ResourceAmount { Type = ResourceType.Coins, Amount = 10 + (hash % 15) } // 10-24 coins
                }
            },

            // Default exchange properties
            SingleUse = false,
            SuccessRate = 100,

            // Generic flavor
            FlavorText = "An exchange of mutual benefit."
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
        else if (typeof(T) == typeof(Venue))
        {
            if (!gameWorld.WorldState.locations.Any(l => l.Id == id))
            {
                skeleton = GenerateSkeletonVenue(id, source) as T;
                return true;
            }
        }
        else if (typeof(T) == typeof(LocationSpot))
        {
            if (!gameWorld.WorldState.locationSpots.Any(s => s.Id == id))
            {
                // Need to find or create parent Venue first
                string venueId = source.Contains("location_")
                    ? source.Substring(source.IndexOf("location_") + 9).Split('_')[0]
                    : "unknown_location";
                skeleton = GenerateSkeletonSpot(id, venueId, source) as T;
                return true;
            }
        }

        return false;
    }
}