using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Templated observation system to generate varied content without explosion
/// Based on Alex's recommendation: 24 base templates + 120 detail phrases = 1000+ combinations
/// </summary>
public class ObservationTemplates
{
    private readonly Dictionary<LocationTag, ObservationTemplate> _templates;
    private readonly Random _random;

    public ObservationTemplates()
    {
        _random = new Random();
        _templates = InitializeTemplates();
    }

    /// <summary>
    /// Generate an observation based on location tag and context
    /// </summary>
    public string GenerateObservation(LocationTag tag, string locationId, int seed = 0)
    {
        if (!_templates.ContainsKey(tag))
            return "You notice nothing unusual.";

        ObservationTemplate template = _templates[tag];

        // Use seed for deterministic generation if provided
        Random rnd = seed > 0 ? new Random(seed) : _random;

        // Select template variation
        string templateText = template.Variations[rnd.Next(template.Variations.Count)];

        // Get location-specific detail
        string detail = GetLocationDetail(tag, locationId, rnd);

        // Replace placeholder
        return templateText.Replace("{detail}", detail);
    }

    private Dictionary<LocationTag, ObservationTemplate> InitializeTemplates()
    {
        return new Dictionary<LocationTag, ObservationTemplate>
        {
            [LocationTag.Crowded] = new ObservationTemplate
            {
                Variations = new List<string>
                {
                    "The press of bodies reveals {detail}",
                    "Through the crowd, you spot {detail}",
                    "Amid the bustle, {detail} catches your attention"
                }
            },

            [LocationTag.Quiet] = new ObservationTemplate
            {
                Variations = new List<string>
                {
                    "In the silence, you hear {detail}",
                    "The stillness allows you to notice {detail}",
                    "Without distraction, {detail} becomes apparent"
                }
            },

            [LocationTag.HearthWarmed] = new ObservationTemplate
            {
                Variations = new List<string>
                {
                    "By the firelight, you see {detail}",
                    "The warm glow reveals {detail}",
                    "Near the hearth, {detail} is visible"
                }
            },

            [LocationTag.AleScented] = new ObservationTemplate
            {
                Variations = new List<string>
                {
                    "Over the ale fumes, you notice {detail}",
                    "Between drinks being poured, {detail} occurs",
                    "The tavern atmosphere shows {detail}"
                }
            },

            [LocationTag.MusicDrifting] = new ObservationTemplate
            {
                Variations = new List<string>
                {
                    "As the music plays, you observe {detail}",
                    "The melody distracts most, but you see {detail}",
                    "Under cover of song, {detail} happens"
                }
            },

            [LocationTag.MarketDay] = new ObservationTemplate
            {
                Variations = new List<string>
                {
                    "Among the merchants, {detail}",
                    "Between stalls, you glimpse {detail}",
                    "The market reveals {detail}"
                }
            },

            [LocationTag.GuardPatrol] = new ObservationTemplate
            {
                Variations = new List<string>
                {
                    "The guards' focus shows {detail}",
                    "Watching the patrol, you note {detail}",
                    "Security measures reveal {detail}"
                }
            },

            [LocationTag.Shadowed] = new ObservationTemplate
            {
                Variations = new List<string>
                {
                    "In the shadows, {detail} moves",
                    "The darkness conceals {detail}, barely",
                    "Half-hidden, {detail} waits"
                }
            }
        };
    }

    private string GetLocationDetail(LocationTag tag, string locationId, Random rnd)
    {
        // Location-specific details based on tag and location
        List<string> details = GetDetailsForLocation(tag, locationId);

        if (details.Any())
            return details[rnd.Next(details.Count)];

        // Fallback generic details
        return GetGenericDetail(tag, rnd);
    }

    private List<string> GetDetailsForLocation(LocationTag tag, string locationId)
    {
        string key = $"{locationId}_{tag}";

        Dictionary<string, List<string>> locationDetails = new Dictionary<string, List<string>>
        {
            // Market Square
            ["market_square_Crowded"] = new()
            {
                "a pickpocket at work",
                "fresh bread being sold",
                "guards pushing through"
            },
            ["market_square_MarketDay"] = new()
            {
                "a merchant counting coins nervously",
                "rare spices from the east",
                "a heated haggling match"
            },
            ["market_square_Public"] = new()
            {
                "the magistrate making rounds",
                "beggars being moved along",
                "children chasing a dog"
            },

            // Copper Kettle Tavern
            ["copper_kettle_HearthWarmed"] = new()
            {
                "someone crying quietly in the corner",
                "a letter being passed secretly",
                "warm bread just from the oven"
            },
            ["copper_kettle_AleScented"] = new()
            {
                "a merchant drowning his losses",
                "sailors telling tall tales",
                "the barkeep watering drinks"
            },
            ["copper_kettle_MusicDrifting"] = new()
            {
                "a couple dancing slowly",
                "someone humming along sadly",
                "the lute player's broken string"
            },

            // Noble District
            ["noble_district_Quiet"] = new()
            {
                "servants whispering urgently",
                "a door closing softly",
                "footsteps on marble"
            },
            ["noble_district_Private"] = new()
            {
                "a hidden servant's door",
                "fresh flowers being delivered",
                "someone watching from a window"
            },
            ["noble_district_GuardPatrol"] = new()
            {
                "guards checking papers",
                "a nervous visitor waiting",
                "the captain taking bribes"
            },

            // Merchant Row
            ["merchant_row_Crowded"] = new()
            {
                "laborers unloading crates",
                "someone slipping between stalls",
                "a child stealing fruit"
            },
            ["merchant_row_MarketDay"] = new()
            {
                "exotic goods being unveiled",
                "fierce competition for customers",
                "a merchant closing early"
            },
            ["merchant_row_Industrial"] = new()
            {
                "smoke from the smithy",
                "leather being worked",
                "wheels being repaired"
            },

            // Riverside
            ["riverside_Public"] = new()
            {
                "fishermen hauling nets",
                "travelers disembarking",
                "cargo being inspected"
            },
            ["riverside_Shadowed"] = new()
            {
                "figures moving in the fog",
                "a boat without lights",
                "someone signaling from shore"
            },
            ["riverside_Industrial"] = new()
            {
                "dock workers taking breaks",
                "ropes being coiled",
                "tar being heated"
            },

            // City Gates
            ["city_gates_Public"] = new()
            {
                "travelers showing papers",
                "merchants declaring goods",
                "guards searching wagons"
            },
            ["city_gates_GuardPatrol"] = new()
            {
                "shift change happening",
                "wanted posters being updated",
                "someone being turned away"
            },
            ["city_gates_Crowded"] = new()
            {
                "refugees seeking entry",
                "a noble's carriage arriving",
                "peddlers hawking to travelers"
            }
        };

        return locationDetails.ContainsKey(key) ? locationDetails[key] : new List<string>();
    }

    private string GetGenericDetail(LocationTag tag, Random rnd)
    {
        Dictionary<LocationTag, List<string>> genericDetails = new Dictionary<LocationTag, List<string>>
        {
            [LocationTag.Crowded] = new()
            {
                "too many people to track",
                "someone pushing past",
                "voices overlapping"
            },
            [LocationTag.Quiet] = new()
            {
                "your own breathing",
                "a distant sound",
                "unexpected stillness"
            },
            [LocationTag.Public] = new()
            {
                "people going about their day",
                "normal city life",
                "nothing suspicious"
            },
            [LocationTag.Private] = new()
            {
                "closed doors",
                "muffled sounds",
                "hidden spaces"
            },
            [LocationTag.HearthWarmed] = new()
            {
                "flowable warmth",
                "flickering shadows",
                "glowing embers"
            },
            [LocationTag.AleScented] = new()
            {
                "spilled drinks",
                "empty mugs",
                "brewing smells"
            },
            [LocationTag.MusicDrifting] = new()
            {
                "a familiar tune",
                "missed notes",
                "rhythmic sounds"
            },
            [LocationTag.MarketDay] = new()
            {
                "coins changing hands",
                "goods being displayed",
                "busy commerce"
            },
            [LocationTag.GuardPatrol] = new()
            {
                "watchful eyes",
                "official business",
                "maintained order"
            },
            [LocationTag.Shadowed] = new()
            {
                "unclear shapes",
                "hidden figures",
                "concealed activity"
            },
            [LocationTag.Sunny] = new()
            {
                "bright reflections",
                "clear visibility",
                "warming light"
            },
            [LocationTag.Industrial] = new()
            {
                "work in progress",
                "tools and materials",
                "busy workers"
            },
            [LocationTag.Religious] = new()
            {
                "quiet prayers",
                "ceremonial preparations",
                "faithful gathered"
            }
        };

        if (genericDetails.ContainsKey(tag))
        {
            List<string> details = genericDetails[tag];
            return details[rnd.Next(details.Count)];
        }

        return "something noteworthy";
    }
}

/// <summary>
/// Template structure for observations
/// </summary>
public class ObservationTemplate
{
    public List<string> Variations { get; set; } = new();
}