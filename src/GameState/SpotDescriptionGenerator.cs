using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Generates atmospheric descriptions from categorical spot properties.
/// All text is systematically derived from game state - no hardcoded strings.
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
        [TimeBlocks.Afternoon] = new[] { "Activity reaches its peak", "The busiest time of day", "Full daylight illuminates all" },
        [TimeBlocks.Evening] = new[] { "Shadows lengthen noticeably", "Day's end approaches", "Evening preparations begin" },
        [TimeBlocks.Night] = new[] { "Darkness cloaks activity", "Night sounds replace day", "Most have retired" },
        [TimeBlocks.LateNight] = new[] { "Deep night silence", "Only the desperate move", "The darkest hours" }
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
        string atmosphereBase = PropertyAtmosphere[primaryProperty][GetVariantIndex(primaryProperty)];
        description.Add(char.ToUpper(atmosphereBase[0]) + atmosphereBase.Substring(1));

        // Add secondary properties if multiple
        if (properties.Count > 1)
        {
            IEnumerable<SpotPropertyType> secondaryProps = properties.Where(p => p != primaryProperty).Take(2);
            foreach (SpotPropertyType prop in secondaryProps)
            {
                if (PropertyAtmosphere.ContainsKey(prop))
                {
                    string fragment = PropertyAtmosphere[prop][GetVariantIndex(prop)];
                    description.Add($", {fragment}");
                }
            }
        }

        // Add time-based activity
        description.Add($". {TimeActivity[currentTime][GetVariantIndex(currentTime)]}");

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

        // Create brief categorical description
        return primary switch
        {
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

            _ => "Notable spot"
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