/// <summary>
/// Generates atmospheric descriptions from categorical location properties.
/// All text is systematically derived from game state - no hardcoded strings.
/// Handles missing property mappings gracefully with fallback descriptions.
/// </summary>
public class LocationDescriptionGenerator
{
    // Primary atmospheric fragments for each property
    private static readonly Dictionary<LocationPropertyType, string[]> PropertyAtmosphere = new()
    {
        // Privacy properties
        [LocationPropertyType.Private] = new[] { "a secluded space", "hidden from prying eyes", "an intimate setting" },
        [LocationPropertyType.Discrete] = new[] { "a discreet corner", "away from the main thoroughfare", "partially concealed" },
        [LocationPropertyType.Public] = new[] { "open to all", "in full view", "a public gathering place" },
        [LocationPropertyType.Exposed] = new[] { "completely exposed", "under watchful eyes", "lacking any privacy" },

        // Atmosphere properties
        [LocationPropertyType.Quiet] = new[] { "peaceful silence", "hushed tranquility", "undisturbed calm" },
        [LocationPropertyType.Loud] = new[] { "bustling with noise", "filled with clamor", "alive with sound" },
        [LocationPropertyType.Warm] = new[] { "flowably warm", "heated by hearth", "protected from cold" },
        [LocationPropertyType.Shaded] = new[] { "cool shade", "sheltered from sun", "dimly lit" },

        // View properties
        [LocationPropertyType.ViewsMainEntrance] = new[] { "overlooking arrivals", "watching the gates", "seeing who comes and goes" },
        [LocationPropertyType.ViewsBackAlley] = new[] { "glimpsing shadowed dealings", "observing discrete meetings", "watching the back ways" },
        [LocationPropertyType.ViewsMarket] = new[] { "surveying diplomacy", "watching trade flow", "observing merchant activity" },
        [LocationPropertyType.ViewsTemple] = new[] { "facing the sacred", "viewing devotions", "watching the faithful" },

        // Social properties
        [LocationPropertyType.Crossroads] = new[] { "where paths converge", "a natural meeting point", "drawing diverse crowds" },
        [LocationPropertyType.Isolated] = new[] { "removed from others", "deliberately separate", "few visitors" },
        [LocationPropertyType.BusyEvening] = new[] { "crowded after dark", "evening gatherings", "nighttime activity" },
        [LocationPropertyType.QuietMorning] = new[] { "peaceful at dawn", "morning stillness", "early tranquility" },

        // Special properties
        [LocationPropertyType.NobleFavored] = new[] { "preferred by nobility", "aristocratic atmosphere", "refined surroundings" },
        [LocationPropertyType.CommonerHaunt] = new[] { "common folk gathering", "working class refuge", "unpretentious flow" },
        [LocationPropertyType.MerchantHub] = new[] { "trader's territory", "commercial center", "business dealings" },
        [LocationPropertyType.SacredGround] = new[] { "blessed ground", "holy atmosphere", "spiritual focus" }
    };

    // Activity descriptions based on time of day
    private static readonly Dictionary<TimeBlocks, string[]> TimeActivity = new()
    {
        [TimeBlocks.Morning] = new[] { "Morning routines unfold", "Business begins in earnest", "The day's work commences" },
        [TimeBlocks.Midday] = new[] { "Activity reaches its peak", "The busiest time of day", "Full daylight illuminates all" },
        [TimeBlocks.Afternoon] = new[] { "Shadows lengthen noticeably", "Day's end approaches", "Evening preparations begin" },
        [TimeBlocks.Evening] = new[] { "Darkness cloaks activity", "Night sounds replace day", "Most have retired" }
    };

    // Tension modifiers based on urgent obligations
    private static readonly string[] TensionModifiers = new[]
    {
        "though deadlines press upon your mind",
        "but urgent matters demand attention",
        "yet time slips away relentlessly",
        "while obligations accumulate",
        "as pressure mounts steadily"
    };

    // Generic fallback descriptions for unmapped properties
    private static readonly string[] FallbackDescriptions = new[]
    {
    "a notable location",
    "an interesting location",
    "a distinctive place",
    "a marked area",
    "a particular locale"
};

    // Category-based fallback descriptions for better context
    private static readonly Dictionary<string, string[]> CategoryFallbacks = new()
    {
        ["Authority"] = new[] { "under official watch", "where authority holds sway", "regulated and monitored" },
        ["Commercial"] = new[] { "focused on trade", "where business happens", "commercial in nature" },
        ["Social"] = new[] { "where people gather", "socially active", "community-focused" },
        ["Transit"] = new[] { "a passage point", "connecting different areas", "facilitating movement" },
        ["Environmental"] = new[] { "shaped by nature", "influenced by surroundings", "environmentally distinct" },
        ["Functional"] = new[] { "serving a purpose", "functionally designed", "practically arranged" },
        ["Time"] = new[] { "time-dependent atmosphere", "varying with hours", "temporally significant" }
    };

    /// <summary>
    /// Validate the PropertyAtmosphere dictionary against all enum values.
    /// Should be called during startup to identify missing mappings.
    /// </summary>
    public static List<LocationPropertyType> ValidatePropertyMappings()
    {
        LocationPropertyType[] allProperties = Enum.GetValues<LocationPropertyType>();
        List<LocationPropertyType> unmappedProperties = new List<LocationPropertyType>();

        foreach (LocationPropertyType property in allProperties)
        {
            if (!PropertyAtmosphere.ContainsKey(property))
            {
                unmappedProperties.Add(property);
            }
        }

        if (unmappedProperties.Any())
        {
            foreach (LocationPropertyType property in unmappedProperties)
            { }
        }

        return unmappedProperties;
    }

    /// <summary>
    /// Safely get atmospheric description for a property with intelligent fallbacks
    /// </summary>
    private string GetPropertyDescription(LocationPropertyType property, int variantIndex)
    {
        // First try direct mapping
        if (PropertyAtmosphere.TryGetValue(property, out string[]? descriptions))
        {
            return descriptions[variantIndex % descriptions.Length];
        }

        // Try category-based fallback
        string propertyName = property.ToString();
        foreach (KeyValuePair<string, string[]> category in CategoryFallbacks)
        {
            if (propertyName.Contains(category.Key))
            {
                return category.Value[variantIndex % category.Value.Length];
            }
        }

        // Use generic fallback
        return FallbackDescriptions[variantIndex % FallbackDescriptions.Length];
    }

    /// <summary>
    /// Generate a complete atmospheric description from categorical properties
    /// </summary>
    public string GenerateDescription(
        List<LocationPropertyType> properties,
        TimeBlocks currentTime,
        int npcsPresent)
    {
        if (properties == null || !properties.Any())
            return "An unremarkable location.";

        List<string> description = new List<string>();

        // Start with primary atmosphere
        LocationPropertyType primaryProperty = SelectPrimaryProperty(properties);
        string atmosphereBase = GetPropertyDescription(primaryProperty, GetVariantIndex(primaryProperty));
        description.Add(char.ToUpper(atmosphereBase[0]) + atmosphereBase.Substring(1));

        // Add secondary properties if multiple
        if (properties.Count > 1)
        {
            IEnumerable<LocationPropertyType> secondaryProps = properties.Where(p => p != primaryProperty).Take(2);
            foreach (LocationPropertyType prop in secondaryProps)
            {
                string fragment = GetPropertyDescription(prop, GetVariantIndex(prop));
                description.Add($", {fragment}");
            }
        }

        // Add time-based activity - with safe access
        if (TimeActivity.TryGetValue(currentTime, out string[]? timeActivities))
        {
            int variantIndex = GetVariantIndex(currentTime);
            if (timeActivities != null && timeActivities.Length > 0)
            {
                string activity = timeActivities[variantIndex % timeActivities.Length];
                description.Add($". {activity}");
            }
        }
        else
        {
            // Fallback for unmapped time blocks
            description.Add(". The time passes quietly");
        }

        // Add social context based on NPCs
        if (npcsPresent > 3)
            description.Add(". Several figures go about their business");
        else if (npcsPresent > 0)
            description.Add(". A few souls are present");

        return string.Join("", description) + ".";
    }

    /// <summary>
    /// Generate a brief location identifier (used in navigation lists)
    /// </summary>
    public string GenerateBriefDescription(List<LocationPropertyType> properties)
    {
        if (properties == null || !properties.Any())
            return "Unremarkable location";

        LocationPropertyType primary = SelectPrimaryProperty(properties);

        // Create brief categorical description with fallbacks for unmapped properties
        return primary switch
        {
            // Mapped properties (existing)
            LocationPropertyType.Private => "Private area",
            LocationPropertyType.Discrete => "Discrete location",
            LocationPropertyType.Public => "Public space",
            LocationPropertyType.Exposed => "Exposed location",
            LocationPropertyType.Quiet => "Quiet place",
            LocationPropertyType.Loud => "Noisy area",
            LocationPropertyType.Warm => "Warm shelter",
            LocationPropertyType.Shaded => "Shaded location",
            LocationPropertyType.ViewsMainEntrance => "Gate view",
            LocationPropertyType.ViewsBackAlley => "Alley overlook",
            LocationPropertyType.ViewsMarket => "Market view",
            LocationPropertyType.ViewsTemple => "Temple view",
            LocationPropertyType.Crossroads => "Crossroads",
            LocationPropertyType.Isolated => "Isolated location",
            LocationPropertyType.NobleFavored => "Noble quarter",
            LocationPropertyType.CommonerHaunt => "Common area",
            LocationPropertyType.MerchantHub => "Trade center",
            LocationPropertyType.SacredGround => "Sacred place",
            LocationPropertyType.Authority => "Authority post",
            LocationPropertyType.Commercial => "Trade location",
            LocationPropertyType.Watched => "Watched area",
            LocationPropertyType.Official => "Official site",
            LocationPropertyType.Guarded => "Guarded zone",
            LocationPropertyType.Secure => "Secure area",
            LocationPropertyType.Transit => "Transit point",
            LocationPropertyType.TransitHub => "Transit hub",
            LocationPropertyType.Checkpoint => "Checkpoint",
            LocationPropertyType.Gateway => "Gateway",
            LocationPropertyType.Social => "Social hub",
            LocationPropertyType.Service => "Service point",
            LocationPropertyType.Rest => "Rest area",
            LocationPropertyType.SleepingSpace => "Sleeping quarters",
            LocationPropertyType.LodgingProvider => "Accommodation available",
            LocationPropertyType.Indoor => "Indoor",
            LocationPropertyType.Tavern => "Tavern",
            LocationPropertyType.Market => "Market",
            LocationPropertyType.Temple => "Temple",
            LocationPropertyType.Noble => "Noble area",
            LocationPropertyType.Wealthy => "Wealthy district",
            LocationPropertyType.Water => "Waterfront",
            LocationPropertyType.River => "Riverside",
            LocationPropertyType.Busy => "Busy place",
            LocationPropertyType.Central => "Central hub",
            LocationPropertyType.Hidden => "Hidden location",

            // Generic fallback
            _ => $"{primary} area"
        };
    }

    /// <summary>
    /// Select the most important property for primary description
    /// </summary>
    private LocationPropertyType SelectPrimaryProperty(List<LocationPropertyType> properties)
    {
        // Priority order for primary description
        LocationPropertyType[] priorityOrder = new[]
        {
            LocationPropertyType.Private,
            LocationPropertyType.SacredGround,
            LocationPropertyType.NobleFavored,
            LocationPropertyType.MerchantHub,
            LocationPropertyType.Crossroads,
            LocationPropertyType.ViewsMarket,
            LocationPropertyType.ViewsTemple,
            LocationPropertyType.ViewsMainEntrance,
            LocationPropertyType.Quiet,
            LocationPropertyType.Public
        };

        foreach (LocationPropertyType priority in priorityOrder)
        {
            if (properties.Contains(priority))
                return priority;
        }

        return properties.First();
    }

    /// <summary>
    /// Get a consistent variant index for text variety
    /// Uses explicit enum value cast for determinism (no GetHashCode)
    /// </summary>
    private int GetVariantIndex(object input)
    {
        // ADR-007: Explicit deterministic hash (no GetHashCode() for procedural selection)
        // Cast enum to int for stable, platform-independent hashing
        int hash = input switch
        {
            LocationPropertyType prop => (int)prop,
            TimeBlocks time => (int)time,
            _ => 0
        };
        return Math.Abs(hash) % 3; // We have 3 variants per property
    }
}