using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Generates atmospheric descriptions from categorical spot properties.
/// All text is systematically derived from game state - no hardcoded strings.
/// Handles missing property mappings gracefully with fallback descriptions.
/// </summary>
public class SpotDescriptionGenerator
{
    // Primary atmospheric fragments for each property
    private static readonly Dictionary<SpotPropertyType, string[]> PropertyAtmosphere = new()
    {
        // Privacy properties
        [SpotPropertyType.Private] = new[] { "a secluded space", "hidden from prying eyes", "an intimate setting" },
        [SpotPropertyType.Discrete] = new[] { "a discreet corner", "away from the main thoroughfare", "partially concealed" },
        [SpotPropertyType.Public] = new[] { "open to all", "in full view", "a public gathering place" },
        [SpotPropertyType.Exposed] = new[] { "completely exposed", "under watchful eyes", "lacking any privacy" },

        // Atmosphere properties
        [SpotPropertyType.Quiet] = new[] { "peaceful silence", "hushed tranquility", "undisturbed calm" },
        [SpotPropertyType.Loud] = new[] { "bustling with noise", "filled with clamor", "alive with sound" },
        [SpotPropertyType.Warm] = new[] { "flowably warm", "heated by hearth", "protected from cold" },
        [SpotPropertyType.Shaded] = new[] { "cool shade", "sheltered from sun", "dimly lit" },

        // View properties
        [SpotPropertyType.ViewsMainEntrance] = new[] { "overlooking arrivals", "watching the gates", "seeing who comes and goes" },
        [SpotPropertyType.ViewsBackAlley] = new[] { "glimpsing shadowed dealings", "observing discrete meetings", "watching the back ways" },
        [SpotPropertyType.ViewsMarket] = new[] { "surveying commerce", "watching trade flow", "observing merchant activity" },
        [SpotPropertyType.ViewsTemple] = new[] { "facing the sacred", "viewing devotions", "watching the faithful" },

        // Social properties
        [SpotPropertyType.Crossroads] = new[] { "where paths converge", "a natural meeting point", "drawing diverse crowds" },
        [SpotPropertyType.Isolated] = new[] { "removed from others", "deliberately separate", "few visitors" },
        [SpotPropertyType.BusyEvening] = new[] { "crowded after dark", "evening gatherings", "nighttime activity" },
        [SpotPropertyType.QuietMorning] = new[] { "peaceful at dawn", "morning stillness", "early tranquility" },

        // Special properties
        [SpotPropertyType.NobleFavored] = new[] { "preferred by nobility", "aristocratic atmosphere", "refined surroundings" },
        [SpotPropertyType.CommonerHaunt] = new[] { "common folk gathering", "working class refuge", "unpretentious flow" },
        [SpotPropertyType.MerchantHub] = new[] { "trader's territory", "commercial center", "business dealings" },
        [SpotPropertyType.SacredGround] = new[] { "blessed ground", "holy atmosphere", "spiritual focus" }
    };

    // Activity descriptions based on time of day
    private static readonly Dictionary<TimeBlocks, string[]> TimeActivity = new()
    {
        [TimeBlocks.Dawn] = new[] { "Early risers begin their day", "First light reveals empty streets", "The city slowly awakens" },
        [TimeBlocks.Morning] = new[] { "Morning routines unfold", "Business begins in earnest", "The day's work commences" },
        [TimeBlocks.Midday] = new[] { "Activity reaches its peak", "The busiest time of day", "Full daylight illuminates all" },
        [TimeBlocks.Afternoon] = new[] { "Shadows lengthen noticeably", "Day's end approaches", "Evening preparations begin" },
        [TimeBlocks.Evening] = new[] { "Darkness cloaks activity", "Night sounds replace day", "Most have retired" },
        [TimeBlocks.Night] = new[] { "Deep night silence", "Only the disconnected move", "The darkest hours" }
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
        "an interesting spot",
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
    public static List<SpotPropertyType> ValidatePropertyMappings()
    {
        SpotPropertyType[] allProperties = Enum.GetValues<SpotPropertyType>();
        List<SpotPropertyType> unmappedProperties = new List<SpotPropertyType>();

        foreach (SpotPropertyType property in allProperties)
        {
            if (!PropertyAtmosphere.ContainsKey(property))
            {
                unmappedProperties.Add(property);
            }
        }

        if (unmappedProperties.Any())
        {
            Console.WriteLine($"SpotDescriptionGenerator: Found {unmappedProperties.Count} unmapped properties:");
            foreach (SpotPropertyType property in unmappedProperties)
            {
                Console.WriteLine($"  - {property}");
            }
        }

        return unmappedProperties;
    }

    /// <summary>
    /// Safely get atmospheric description for a property with intelligent fallbacks
    /// </summary>
    private string GetPropertyDescription(SpotPropertyType property, int variantIndex)
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
        List<SpotPropertyType> properties,
        TimeBlocks currentTime,
        int urgentObligationCount,
        int npcsPresent)
    {
        if (properties == null || !properties.Any())
            return "An unremarkable location.";

        List<string> description = new List<string>();

        // Start with primary atmosphere
        SpotPropertyType primaryProperty = SelectPrimaryProperty(properties);
        string atmosphereBase = GetPropertyDescription(primaryProperty, GetVariantIndex(primaryProperty));
        description.Add(char.ToUpper(atmosphereBase[0]) + atmosphereBase.Substring(1));

        // Add secondary properties if multiple
        if (properties.Count > 1)
        {
            IEnumerable<SpotPropertyType> secondaryProps = properties.Where(p => p != primaryProperty).Take(2);
            foreach (SpotPropertyType prop in secondaryProps)
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

        // Add tension if urgent obligations exist
        if (urgentObligationCount > 0)
        {
            string tensionPhrase = TensionModifiers[Math.Min(urgentObligationCount - 1, TensionModifiers.Length - 1)];
            description.Add($", {tensionPhrase}");
        }

        return string.Join("", description) + ".";
    }

    /// <summary>
    /// Generate a brief spot identifier (used in navigation lists)
    /// </summary>
    public string GenerateBriefDescription(List<SpotPropertyType> properties)
    {
        if (properties == null || !properties.Any())
            return "Unremarkable spot";

        SpotPropertyType primary = SelectPrimaryProperty(properties);

        // Create brief categorical description with fallbacks for unmapped properties
        return primary switch
        {
            // Mapped properties (existing)
            SpotPropertyType.Private => "Private area",
            SpotPropertyType.Discrete => "Discrete spot",
            SpotPropertyType.Public => "Public space",
            SpotPropertyType.Exposed => "Exposed location",
            SpotPropertyType.Quiet => "Quiet place",
            SpotPropertyType.Loud => "Noisy area",
            SpotPropertyType.Warm => "Warm shelter",
            SpotPropertyType.Shaded => "Shaded spot",
            SpotPropertyType.ViewsMainEntrance => "Gate view",
            SpotPropertyType.ViewsBackAlley => "Alley overlook",
            SpotPropertyType.ViewsMarket => "Market view",
            SpotPropertyType.ViewsTemple => "Temple view",
            SpotPropertyType.Crossroads => "Crossroads",
            SpotPropertyType.Isolated => "Isolated spot",
            SpotPropertyType.NobleFavored => "Noble quarter",
            SpotPropertyType.CommonerHaunt => "Common area",
            SpotPropertyType.MerchantHub => "Trade center",
            SpotPropertyType.SacredGround => "Sacred place",

            // Common unmapped properties (intelligent fallbacks)
            SpotPropertyType.Authority => "Authority post",
            SpotPropertyType.Commercial => "Trade spot",
            SpotPropertyType.Watched => "Watched area",
            SpotPropertyType.Official => "Official site",
            SpotPropertyType.Guarded => "Guarded zone",
            SpotPropertyType.Secure => "Secure area",
            SpotPropertyType.Transit => "Transit point",
            SpotPropertyType.TransitHub => "Transit hub",
            SpotPropertyType.Checkpoint => "Checkpoint",
            SpotPropertyType.Gateway => "Gateway",
            SpotPropertyType.Social => "Social hub",
            SpotPropertyType.Service => "Service point",
            SpotPropertyType.Rest => "Rest area",
            SpotPropertyType.Lodging => "Lodging",
            SpotPropertyType.Tavern => "Tavern",
            SpotPropertyType.Market => "Market",
            SpotPropertyType.Temple => "Temple",
            SpotPropertyType.Noble => "Noble area",
            SpotPropertyType.Wealthy => "Wealthy district",
            SpotPropertyType.Water => "Waterfront",
            SpotPropertyType.River => "Riverside",
            SpotPropertyType.Busy => "Busy place",
            SpotPropertyType.Central => "Central hub",
            SpotPropertyType.Hidden => "Hidden spot",

            // Generic fallback
            _ => $"{primary} area"
        };
    }

    /// <summary>
    /// Select the most important property for primary description
    /// </summary>
    private SpotPropertyType SelectPrimaryProperty(List<SpotPropertyType> properties)
    {
        // Priority order for primary description
        SpotPropertyType[] priorityOrder = new[]
        {
                SpotPropertyType.Private,
                SpotPropertyType.SacredGround,
                SpotPropertyType.NobleFavored,
                SpotPropertyType.MerchantHub,
                SpotPropertyType.Crossroads,
                SpotPropertyType.ViewsMarket,
                SpotPropertyType.ViewsTemple,
                SpotPropertyType.ViewsMainEntrance,
                SpotPropertyType.Quiet,
                SpotPropertyType.Public
            };

        foreach (SpotPropertyType priority in priorityOrder)
        {
            if (properties.Contains(priority))
                return priority;
        }

        return properties.First();
    }

    /// <summary>
    /// Get a consistent variant index for text variety
    /// Uses simple hash to ensure same input produces same output
    /// </summary>
    private int GetVariantIndex(object input)
    {
        int hash = input.GetHashCode();
        return Math.Abs(hash) % 3; // We have 3 variants per property
    }
}