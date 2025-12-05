/// <summary>
/// Templated observation system to generate varied content without explosion
/// Based on Alex's recommendation: 24 base templates + 120 detail phrases = 1000+ combinations
/// DDR-007: All observation generation is deterministic based on location properties
/// DOMAIN COLLECTION PRINCIPLE: Uses switch-based lookup instead of Dictionary
/// </summary>
public class ObservationTemplates
{
    /// <summary>
    /// Generate an observation based on Venue tag and context
    /// DDR-007: Deterministic generation based on venueId and tag (always predictable)
    /// HIGHLANDER: seed is REQUIRED. Pass 0 to derive seed from inputs, or pass specific seed.
    /// </summary>
    public string GenerateObservation(LocationTag tag, string venueId, int seed)
    {
        ObservationTemplate template = GetTemplateForTag(tag);
        if (template == null)
            return "You notice nothing unusual.";

        // DDR-007: Use first template variation (categorical selection, no hash-based pseudo-randomness)
        string templateText = template.Variations[0];

        // Get location-specific detail (uses first option)
        string detail = GetLocationDetail(tag, venueId, 0);

        // Replace placeholder
        return templateText.Replace("{detail}", detail);
    }

    /// <summary>
    /// DOMAIN COLLECTION PRINCIPLE: Switch-based lookup for fixed enum (LocationTag)
    /// </summary>
    private ObservationTemplate GetTemplateForTag(LocationTag tag)
    {
        return tag switch
        {
            LocationTag.Crowded => new ObservationTemplate
            {
                Variations = new List<string>
                {
                    "The press of bodies reveals {detail}",
                    "Through the crowd, you location {detail}",
                    "Amid the bustle, {detail} catches your attention"
                }
            },
            LocationTag.Quiet => new ObservationTemplate
            {
                Variations = new List<string>
                {
                    "In the silence, you hear {detail}",
                    "The stillness allows you to notice {detail}",
                    "Without distraction, {detail} becomes apparent"
                }
            },
            LocationTag.HearthWarmed => new ObservationTemplate
            {
                Variations = new List<string>
                {
                    "By the firelight, you see {detail}",
                    "The warm glow reveals {detail}",
                    "Near the hearth, {detail} is visible"
                }
            },
            LocationTag.AleScented => new ObservationTemplate
            {
                Variations = new List<string>
                {
                    "Over the ale fumes, you notice {detail}",
                    "Between drinks being poured, {detail} occurs",
                    "The tavern atmosphere shows {detail}"
                }
            },
            LocationTag.MusicDrifting => new ObservationTemplate
            {
                Variations = new List<string>
                {
                    "As the music plays, you observe {detail}",
                    "The melody distracts most, but you see {detail}",
                    "Under cover of song, {detail} happens"
                }
            },
            LocationTag.MarketDay => new ObservationTemplate
            {
                Variations = new List<string>
                {
                    "Among the merchants, {detail}",
                    "Between stalls, you glimpse {detail}",
                    "The market reveals {detail}"
                }
            },
            LocationTag.GuardPatrol => new ObservationTemplate
            {
                Variations = new List<string>
                {
                    "The guards' focus shows {detail}",
                    "Watching the patrol, you note {detail}",
                    "Security measures reveal {detail}"
                }
            },
            LocationTag.Shadowed => new ObservationTemplate
            {
                Variations = new List<string>
                {
                    "In the shadows, {detail} moves",
                    "The darkness conceals {detail}, barely",
                    "Half-hidden, {detail} waits"
                }
            },
            _ => null
        };
    }

    /// <summary>
    /// Get location detail deterministically based on seed
    /// DDR-007: Deterministic selection ensures consistent observations
    /// </summary>
    private string GetLocationDetail(LocationTag tag, string venueId, int seed)
    {
        // Location-specific details based on tag and location
        List<string> details = GetDetailsForLocation(tag, venueId);

        if (details.Any())
        {
            int detailIndex = seed % details.Count;
            return details[detailIndex];
        }

        // Fallback generic details
        return GetGenericDetail(tag, seed);
    }

    /// <summary>
    /// DOMAIN COLLECTION PRINCIPLE: Switch-based lookup for location+tag combinations
    /// </summary>
    private List<string> GetDetailsForLocation(LocationTag tag, string venueId)
    {
        string key = $"{venueId}_{tag}";

        return key switch
        {
            // Market Square
            "market_square_Crowded" => new List<string>
            {
                "a pickpocket at work",
                "fresh bread being sold",
                "guards pushing through"
            },
            "market_square_MarketDay" => new List<string>
            {
                "a merchant counting coins nervously",
                "rare spices from the east",
                "a heated haggling match"
            },
            "market_square_Public" => new List<string>
            {
                "the magistrate making rounds",
                "beggars being moved along",
                "children chasing a dog"
            },

            // Copper Kettle Tavern
            "copper_kettle_HearthWarmed" => new List<string>
            {
                "someone crying quietly in the corner",
                "a letter being passed secretly",
                "warm bread just from the oven"
            },
            "copper_kettle_AleScented" => new List<string>
            {
                "a merchant drowning his losses",
                "sailors telling tall tales",
                "the barkeep watering drinks"
            },
            "copper_kettle_MusicDrifting" => new List<string>
            {
                "a couple dancing slowly",
                "someone humming along sadly",
                "the lute player's broken string"
            },

            // Noble District
            "noble_district_Quiet" => new List<string>
            {
                "servants whispering urgently",
                "a door closing softly",
                "footsteps on marble"
            },
            "noble_district_Private" => new List<string>
            {
                "a hidden servant's door",
                "fresh flowers being delivered",
                "someone watching from a window"
            },
            "noble_district_GuardPatrol" => new List<string>
            {
                "guards checking papers",
                "a nervous visitor waiting",
                "the captain taking bribes"
            },

            // Merchant Row
            "merchant_row_Crowded" => new List<string>
            {
                "laborers unloading crates",
                "someone slipping between stalls",
                "a child stealing fruit"
            },
            "merchant_row_MarketDay" => new List<string>
            {
                "exotic goods being unveiled",
                "fierce competition for customers",
                "a merchant closing early"
            },
            "merchant_row_Industrial" => new List<string>
            {
                "smoke from the smithy",
                "leather being worked",
                "wheels being repaired"
            },

            // Riverside
            "riverside_Public" => new List<string>
            {
                "fishermen hauling nets",
                "travelers disembarking",
                "cargo being inspected"
            },
            "riverside_Shadowed" => new List<string>
            {
                "figures moving in the fog",
                "a boat without lights",
                "someone signaling from shore"
            },
            "riverside_Industrial" => new List<string>
            {
                "dock workers taking breaks",
                "ropes being coiled",
                "tar being heated"
            },

            // City Gates
            "city_gates_Public" => new List<string>
            {
                "travelers showing papers",
                "merchants declaring goods",
                "guards searching wagons"
            },
            "city_gates_GuardPatrol" => new List<string>
            {
                "shift change happening",
                "wanted posters being updated",
                "someone being turned away"
            },
            "city_gates_Crowded" => new List<string>
            {
                "refugees seeking entry",
                "a noble's carriage arriving",
                "peddlers hawking to travelers"
            },
            _ => new List<string>()
        };
    }

    /// <summary>
    /// Get generic detail deterministically based on seed
    /// DDR-007: Deterministic selection ensures consistent observations
    /// DOMAIN COLLECTION PRINCIPLE: Switch-based lookup for fixed enum (LocationTag)
    /// </summary>
    private string GetGenericDetail(LocationTag tag, int seed)
    {
        List<string> details = GetGenericDetailsForTag(tag);
        if (details.Any())
        {
            int detailIndex = seed % details.Count;
            return details[detailIndex];
        }
        return "something noteworthy";
    }

    private List<string> GetGenericDetailsForTag(LocationTag tag)
    {
        return tag switch
        {
            LocationTag.Crowded => new List<string>
            {
                "too many people to track",
                "someone pushing past",
                "voices overlapping"
            },
            LocationTag.Quiet => new List<string>
            {
                "your own breathing",
                "a distant sound",
                "unexpected stillness"
            },
            LocationTag.Public => new List<string>
            {
                "people going about their day",
                "normal city life",
                "nothing suspicious"
            },
            LocationTag.Private => new List<string>
            {
                "closed doors",
                "muffled sounds",
                "hidden spaces"
            },
            LocationTag.HearthWarmed => new List<string>
            {
                "flowable warmth",
                "flickering shadows",
                "glowing embers"
            },
            LocationTag.AleScented => new List<string>
            {
                "spilled drinks",
                "empty mugs",
                "brewing smells"
            },
            LocationTag.MusicDrifting => new List<string>
            {
                "a familiar tune",
                "missed notes",
                "rhythmic sounds"
            },
            LocationTag.MarketDay => new List<string>
            {
                "coins changing hands",
                "goods being displayed",
                "busy diplomacy"
            },
            LocationTag.GuardPatrol => new List<string>
            {
                "watchful eyes",
                "official business",
                "maintained order"
            },
            LocationTag.Shadowed => new List<string>
            {
                "unclear shapes",
                "hidden figures",
                "concealed activity"
            },
            LocationTag.Sunny => new List<string>
            {
                "bright reflections",
                "clear visibility",
                "warming light"
            },
            LocationTag.Industrial => new List<string>
            {
                "work in progress",
                "tools and materials",
                "busy workers"
            },
            LocationTag.Religious => new List<string>
            {
                "quiet prayers",
                "ceremonial preparations",
                "faithful gathered"
            },
            _ => new List<string>()
        };
    }
}

/// <summary>
/// Template structure for observations
/// </summary>
public class ObservationTemplate
{
    public List<string> Variations { get; init; } = new();
}