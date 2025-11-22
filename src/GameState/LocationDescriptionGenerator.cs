/// <summary>
/// Generates atmospheric descriptions from LocationCapability flags and categorical dimensions.
/// Replaces LocationPropertyType with Flags-based capabilities and orthogonal categorical properties.
/// All text is systematically derived from game state - no hardcoded location-specific strings.
/// </summary>
public class LocationDescriptionGenerator
{
    // PRIMARY CAPABILITY ATMOSPHERES
    // Maps capability flags to atmospheric descriptions
    private static readonly Dictionary<LocationCapability, string[]> CapabilityAtmosphere = new()
    {
        // Navigation/Routing
        [LocationCapability.Crossroads] = new[] { "where paths converge", "a natural meeting point", "drawing diverse travelers" },
        [LocationCapability.TransitHub] = new[] { "a major junction", "bustling with arrivals and departures", "connecting many routes" },
        [LocationCapability.Gateway] = new[] { "passage between districts", "a threshold location", "marking territory boundaries" },
        [LocationCapability.Transit] = new[] { "facilitating movement", "a passage point", "connecting different areas" },
        [LocationCapability.Transport] = new[] { "offering coach services", "where transport departs", "providing conveyance" },

        // Economic/Work
        [LocationCapability.Commercial] = new[] { "focused on trade", "where business happens", "commercially active" },
        [LocationCapability.Market] = new[] { "filled with merchant stalls", "alive with haggling", "where goods exchange hands" },
        [LocationCapability.Tavern] = new[] { "offering food and drink", "a social gathering place", "warm and inviting" },

        // Rest/Service
        [LocationCapability.SleepingSpace] = new[] { "providing shelter", "offering beds", "a place to rest" },
        [LocationCapability.Restful] = new[] { "peaceful and calm", "promoting relaxation", "soothing to weary travelers" },
        [LocationCapability.LodgingProvider] = new[] { "renting rooms", "offering accommodation", "housing travelers" },
        [LocationCapability.Service] = new[] { "providing various services", "meeting practical needs", "offering assistance" },
        [LocationCapability.Rest] = new[] { "designated for resting", "a respite from travel", "encouraging pause" },

        // Environmental
        [LocationCapability.Indoor] = new[] { "sheltered from weather", "enclosed and protected", "under roof" },
        [LocationCapability.Outdoor] = new[] { "open to the sky", "exposed to elements", "in the fresh air" },

        // Social/Gathering
        [LocationCapability.Social] = new[] { "encouraging interaction", "where people gather", "socially active" },
        [LocationCapability.Gathering] = new[] { "a community meeting place", "drawing crowds together", "hosting assemblies" },

        // Special/Functional
        [LocationCapability.Temple] = new[] { "sacred and reverent", "devoted to worship", "spiritually focused" },
        [LocationCapability.Noble] = new[] { "aristocratic in atmosphere", "favored by nobility", "refined and elegant" },
        [LocationCapability.Water] = new[] { "on the waterfront", "near water", "with river access" },
        [LocationCapability.River] = new[] { "along the riverbank", "where water flows", "beside the current" },

        // Authority/Security
        [LocationCapability.Official] = new[] { "under official authority", "formally administered", "governmentally controlled" },
        [LocationCapability.Authority] = new[] { "where power resides", "under watchful governance", "officially regulated" },
        [LocationCapability.Guarded] = new[] { "actively patrolled", "under armed guard", "secured by sentries" },
        [LocationCapability.Checkpoint] = new[] { "requiring inspection", "checking credentials", "controlling passage" },

        // Wealth/Status
        [LocationCapability.Wealthy] = new[] { "displaying prosperity", "affluent and expensive", "wealth on display" },
        [LocationCapability.Prestigious] = new[] { "highly regarded", "socially elevated", "esteemed location" },

        // Environmental Context
        [LocationCapability.Urban] = new[] { "in the city", "urban and developed", "part of the metropolis" },
        [LocationCapability.Rural] = new[] { "in the countryside", "rustic and pastoral", "away from the city" },

        // Views/Observations
        [LocationCapability.ViewsMainEntrance] = new[] { "overlooking arrivals", "watching the gates", "seeing who enters" },
        [LocationCapability.ViewsBackAlley] = new[] { "glimpsing shadowed dealings", "observing discrete meetings", "watching the back ways" }
    };

    // PRIVACY DESCRIPTIONS
    private static readonly Dictionary<LocationPrivacy, string[]> PrivacyDescriptions = new()
    {
        [LocationPrivacy.Public] = new[] { "open to all", "in full view", "publicly accessible" },
        [LocationPrivacy.SemiPublic] = new[] { "somewhat private", "for patrons only", "restricted but not secluded" },
        [LocationPrivacy.Private] = new[] { "secluded and intimate", "hidden from prying eyes", "offering privacy" }
    };

    // SAFETY DESCRIPTIONS
    private static readonly Dictionary<LocationSafety, string[]> SafetyDescriptions = new()
    {
        [LocationSafety.Dangerous] = new[] { "hazardous and threatening", "unsafe conditions", "danger present" },
        [LocationSafety.Neutral] = new[] { "typical safety", "unremarkable security", "neither safe nor dangerous" },
        [LocationSafety.Safe] = new[] { "well-protected", "secure and guarded", "a safe haven" }
    };

    // ACTIVITY DESCRIPTIONS
    private static readonly Dictionary<LocationActivity, string[]> ActivityDescriptions = new()
    {
        [LocationActivity.Quiet] = new[] { "peaceful silence", "hushed tranquility", "few people present" },
        [LocationActivity.Moderate] = new[] { "typical activity", "moderate foot traffic", "normal bustle" },
        [LocationActivity.Busy] = new[] { "bustling with people", "crowded and active", "alive with activity" }
    };

    // PURPOSE DESCRIPTIONS
    private static readonly Dictionary<LocationPurpose, string[]> PurposeDescriptions = new()
    {
        [LocationPurpose.Transit] = new[] { "a passage point", "facilitating movement", "connecting destinations" },
        [LocationPurpose.Dwelling] = new[] { "a living space", "residential in nature", "offering lodging" },
        [LocationPurpose.Commerce] = new[] { "focused on trade", "commercially driven", "where business thrives" },
        [LocationPurpose.Civic] = new[] { "officially administered", "serving the public", "governmental in function" },
        [LocationPurpose.Defense] = new[] { "militarily strategic", "defensively positioned", "protecting the city" },
        [LocationPurpose.Governance] = new[] { "administratively focused", "bureaucratically organized", "handling official business" },
        [LocationPurpose.Worship] = new[] { "spiritually devoted", "sacred in purpose", "religiously significant" },
        [LocationPurpose.Learning] = new[] { "dedicated to knowledge", "educationally focused", "scholarly in nature" },
        [LocationPurpose.Entertainment] = new[] { "offering amusement", "recreationally oriented", "providing diversion" },
        [LocationPurpose.Generic] = new[] { "serving various purposes", "flexibly used", "adaptable space" }
    };

    // TIME-BASED ACTIVITY (static descriptions, no time variation per capabilities architecture)
    private static readonly Dictionary<TimeBlocks, string[]> TimeActivity = new()
    {
        [TimeBlocks.Morning] = new[] { "Morning routines unfold", "The day begins in earnest", "Dawn activity commences" },
        [TimeBlocks.Midday] = new[] { "Midday activity peaks", "Afternoon bustle continues", "The busiest hours" },
        [TimeBlocks.Afternoon] = new[] { "Shadows lengthen", "Day winds down", "Evening approaches" },
        [TimeBlocks.Evening] = new[] { "Night has fallen", "Darkness settles", "Evening quiet descends" }
    };

    /// <summary>
    /// Generate complete atmospheric description from capabilities and categorical dimensions.
    /// Capabilities are STATIC (no time variation), but time affects activity levels.
    /// </summary>
    public string GenerateDescription(
        LocationCapability capabilities,
        TimeBlocks currentTime,
        int npcsPresent)
    {
        if (capabilities == LocationCapability.None)
            return "An unremarkable location.";

        List<string> description = new List<string>();

        // Select primary capability (highest priority)
        LocationCapability primary = SelectPrimaryCapability(capabilities);
        string atmosphereBase = GetCapabilityDescription(primary, GetVariantIndex(primary));
        description.Add(char.ToUpper(atmosphereBase[0]) + atmosphereBase.Substring(1));

        // Add secondary capabilities if multiple flags set
        List<LocationCapability> secondaryCapabilities = GetSecondaryCapabilities(capabilities, primary);
        foreach (LocationCapability secondary in secondaryCapabilities.Take(2))
        {
            string fragment = GetCapabilityDescription(secondary, GetVariantIndex(secondary));
            description.Add($", {fragment}");
        }

        // Add time-based activity (time affects atmosphere, not capabilities)
        if (TimeActivity.TryGetValue(currentTime, out string[] timeActivities))
        {
            int variantIndex = GetVariantIndex(currentTime);
            string activity = timeActivities[variantIndex % timeActivities.Length];
            description.Add($". {activity}");
        }

        // Add NPC presence
        if (npcsPresent > 3)
            description.Add(". Several figures go about their business");
        else if (npcsPresent > 0)
            description.Add(". A few souls are present");

        return string.Join("", description) + ".";
    }

    /// <summary>
    /// Generate brief location identifier (used in navigation lists).
    /// Maps capability flags to short categorical descriptions.
    /// </summary>
    public string GenerateBriefDescription(LocationCapability capabilities)
    {
        if (capabilities == LocationCapability.None)
            return "Unremarkable location";

        // Select primary capability for brief description
        LocationCapability primary = SelectPrimaryCapability(capabilities);

        return primary switch
        {
            // Navigation/Routing
            LocationCapability.Crossroads => "Crossroads",
            LocationCapability.TransitHub => "Transit hub",
            LocationCapability.Gateway => "Gateway",
            LocationCapability.Transit => "Transit point",
            LocationCapability.Transport => "Transport services",

            // Economic/Work
            LocationCapability.Commercial => "Commercial area",
            LocationCapability.Market => "Market",
            LocationCapability.Tavern => "Tavern",

            // Rest/Service
            LocationCapability.SleepingSpace => "Sleeping quarters",
            LocationCapability.Restful => "Rest area",
            LocationCapability.LodgingProvider => "Lodging available",
            LocationCapability.Service => "Service point",
            LocationCapability.Rest => "Resting place",

            // Environmental
            LocationCapability.Indoor => "Indoor",
            LocationCapability.Outdoor => "Outdoor",

            // Social/Gathering
            LocationCapability.Social => "Social hub",
            LocationCapability.Gathering => "Gathering place",

            // Special/Functional
            LocationCapability.Temple => "Temple",
            LocationCapability.Noble => "Noble area",
            LocationCapability.Water => "Waterfront",
            LocationCapability.River => "Riverside",

            // Authority/Security
            LocationCapability.Official => "Official site",
            LocationCapability.Authority => "Authority post",
            LocationCapability.Guarded => "Guarded zone",
            LocationCapability.Checkpoint => "Checkpoint",

            // Wealth/Status
            LocationCapability.Wealthy => "Wealthy district",
            LocationCapability.Prestigious => "Prestigious location",

            // Environmental Context
            LocationCapability.Urban => "Urban area",
            LocationCapability.Rural => "Rural area",

            // Views/Observations
            LocationCapability.ViewsMainEntrance => "Gate view",
            LocationCapability.ViewsBackAlley => "Alley overlook",

            // Fallback
            _ => "Notable location"
        };
    }

    /// <summary>
    /// Generate observation suggestions based on capabilities.
    /// Returns empty string if no special observations available.
    /// </summary>
    public string SuggestObservations(
        LocationCapability capabilities,
        TimeBlocks currentTime)
    {
        // View capabilities suggest observation opportunities
        if (capabilities.HasFlag(LocationCapability.ViewsMainEntrance))
            return "From here, you can observe arrivals at the main entrance.";

        if (capabilities.HasFlag(LocationCapability.ViewsBackAlley))
            return "The back alley view reveals shadowy dealings.";

        // Market and commercial areas have observable activity
        if (capabilities.HasFlag(LocationCapability.Market))
            return "The market bustle provides many details to observe.";

        if (capabilities.HasFlag(LocationCapability.Commercial))
            return "Commercial activity offers interesting observations.";

        // Social and gathering places have people to observe
        if (capabilities.HasFlag(LocationCapability.Social) || capabilities.HasFlag(LocationCapability.Gathering))
            return "Social interactions here reveal much about the community.";

        // Authority and official locations have structural observations
        if (capabilities.HasFlag(LocationCapability.Authority) || capabilities.HasFlag(LocationCapability.Official))
            return "Official protocols and hierarchies are on display.";

        // Temple locations have ritualistic observations
        if (capabilities.HasFlag(LocationCapability.Temple))
            return "Religious practices and devotions can be observed.";

        // No specific observation suggestions for this location
        return "";
    }

    /// <summary>
    /// Select primary capability for description (highest priority).
    /// Priority order determines which capability defines the location's identity.
    /// </summary>
    private LocationCapability SelectPrimaryCapability(LocationCapability capabilities)
    {
        // Priority order for primary description (most distinctive first)
        LocationCapability[] priorityOrder = new[]
        {
            LocationCapability.Temple,           // Religious identity is strongest
            LocationCapability.TransitHub,       // Major infrastructure
            LocationCapability.Market,           // Distinctive economic
            LocationCapability.Tavern,           // Specific venue type
            LocationCapability.Crossroads,       // Navigation focal point
            LocationCapability.Gateway,          // Boundary marker
            LocationCapability.Checkpoint,       // Control point
            LocationCapability.Noble,            // Status marker
            LocationCapability.Prestigious,      // Status marker
            LocationCapability.LodgingProvider,  // Service provider
            LocationCapability.SleepingSpace,    // Rest capability
            LocationCapability.Guarded,          // Security presence
            LocationCapability.Authority,        // Official presence
            LocationCapability.Commercial,       // Economic general
            LocationCapability.Social,           // Social general
            LocationCapability.Gathering,        // Social gathering
            LocationCapability.Transport,        // Transport services
            LocationCapability.Transit,          // General transit
            LocationCapability.Wealthy,          // Economic status
            LocationCapability.Water,            // Geographic
            LocationCapability.River,            // Geographic specific
            LocationCapability.Restful,          // Atmosphere
            LocationCapability.Service,          // Generic service
            LocationCapability.Rest,             // Generic rest
            LocationCapability.Official,         // Official general
            LocationCapability.Urban,            // Environment
            LocationCapability.Rural,            // Environment
            LocationCapability.ViewsMainEntrance, // Observational
            LocationCapability.ViewsBackAlley,   // Observational
            LocationCapability.Indoor,           // Environmental basic
            LocationCapability.Outdoor           // Environmental basic
        };

        foreach (LocationCapability priority in priorityOrder)
        {
            if (capabilities.HasFlag(priority))
                return priority;
        }

        // Fallback: return the lowest set bit (first capability in enum order)
        foreach (LocationCapability capability in Enum.GetValues<LocationCapability>())
        {
            if (capability != LocationCapability.None && capabilities.HasFlag(capability))
                return capability;
        }

        return LocationCapability.None;
    }

    /// <summary>
    /// Get secondary capabilities for description enrichment.
    /// Returns all capabilities except the primary, in priority order.
    /// </summary>
    private List<LocationCapability> GetSecondaryCapabilities(
        LocationCapability capabilities,
        LocationCapability primary)
    {
        List<LocationCapability> secondary = new List<LocationCapability>();

        foreach (LocationCapability capability in Enum.GetValues<LocationCapability>())
        {
            if (capability == LocationCapability.None)
                continue;

            if (capability == primary)
                continue;

            if (capabilities.HasFlag(capability))
                secondary.Add(capability);
        }

        return secondary;
    }

    /// <summary>
    /// Get atmospheric description for a capability with variant selection.
    /// Returns fallback if capability not mapped.
    /// </summary>
    private string GetCapabilityDescription(LocationCapability capability, int variantIndex)
    {
        if (CapabilityAtmosphere.TryGetValue(capability, out string[] descriptions))
        {
            return descriptions[variantIndex % descriptions.Length];
        }

        // Fallback for unmapped capabilities
        return "a notable location";
    }

    /// <summary>
    /// Get consistent variant index for text variety.
    /// Uses explicit enum value cast for determinism (no GetHashCode).
    /// ADR-007: Explicit deterministic hash, platform-independent.
    /// </summary>
    private int GetVariantIndex(object input)
    {
        int hash = input switch
        {
            LocationCapability capability => (int)capability,
            TimeBlocks time => (int)time,
            _ => 0
        };
        return Math.Abs(hash) % 3; // Three variants per capability
    }
}
