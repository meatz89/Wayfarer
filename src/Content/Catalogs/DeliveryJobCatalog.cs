using Wayfarer.GameState.Enums;
/// <summary>
/// Catalogue for generating DeliveryJob entities from routes at parse time.
/// Called by PackageLoader ONLY - runtime never touches this.
///
/// PARSE-TIME ENTITY GENERATION:
/// PackageLoader calls GenerateJobsFromRoutes() → Catalogue generates complete DeliveryJob entities
/// → PackageLoader adds to GameWorld.AvailableDeliveryJobs → Runtime queries GameWorld (NO catalogue calls)
///
/// ECONOMIC FORMULA:
/// Payment = RoomCost(10) + TravelCost(segments*2) + ProfitMargin(tier-based)
/// Simple: 1-2 segments, +3 profit (minimum 17 coins)
/// Moderate: 3-4 segments, +5 profit
/// Dangerous: 5+ segments, +10 profit
/// </summary>
public static class DeliveryJobCatalog
{
    // Economic constants
    private const int ROOM_COST = 10;
    private const int COST_PER_SEGMENT = 2;
    private const int SIMPLE_PROFIT = 3;
    private const int MODERATE_PROFIT = 5;
    private const int DANGEROUS_PROFIT = 10;

    /// <summary>
    /// Generate ALL delivery jobs from available routes.
    /// Only generates jobs connecting two Commercial locations.
    /// Called once at parse time by PackageLoader.
    /// </summary>
    public static List<DeliveryJob> GenerateJobsFromRoutes(List<RouteOption> routes, List<Location> locations)
    {
        List<DeliveryJob> jobs = new List<DeliveryJob>();

        Console.WriteLine($"[DeliveryJobCatalog] Generating delivery jobs from {routes.Count} routes");

        // Filter routes connecting Commercial locations
        foreach (RouteOption route in routes)
        {
            // Get origin and destination locations
            Location origin = locations.FirstOrDefault(l => l.Id == route.OriginLocationId);
            Location destination = locations.FirstOrDefault(l => l.Id == route.DestinationLocationId);

            // Skip if locations not found
            if (origin == null || destination == null)
            {
                Console.WriteLine($"[DeliveryJobCatalog] ⚠️ Skipping route '{route.Id}' - origin or destination not found");
                continue;
            }

            // Only generate jobs for routes connecting Commercial locations
            if (!origin.LocationProperties.Contains(LocationPropertyType.Commercial) ||
                !destination.LocationProperties.Contains(LocationPropertyType.Commercial))
            {
                continue;
            }

            // Generate job for this route
            DeliveryJob job = GenerateJob(route, origin, destination);
            jobs.Add(job);

            Console.WriteLine($"[DeliveryJobCatalog] ✅ Generated job '{job.Id}' - {job.DifficultyTier} - {job.Payment} coins");
        }

        Console.WriteLine($"[DeliveryJobCatalog] Generated {jobs.Count} delivery jobs total");
        return jobs;
    }

    /// <summary>
    /// Generate a single delivery job from a route.
    /// Calculates payment, determines difficulty, generates cargo description.
    /// </summary>
    private static DeliveryJob GenerateJob(RouteOption route, Location origin, Location destination)
    {
        // Determine difficulty tier based on segment count
        DifficultyTier tier = DetermineDifficulty(route.Segments.Count);

        // Calculate payment based on economic formula
        int payment = CalculatePayment(route.Segments.Count, tier);

        // Generate cargo description based on tier
        string cargo = GenerateCargoDescription(tier);

        // Create job entity
        DeliveryJob job = new DeliveryJob
        {
            Id = $"delivery_{origin.Id}_to_{destination.Id}",
            OriginLocationId = origin.Id,
            DestinationLocationId = destination.Id,
            RouteId = route.Id,
            Payment = payment,
            CargoDescription = cargo,
            JobDescription = $"Deliver {cargo} to {destination.Name}",
            DifficultyTier = tier,
            IsAvailable = true,
            AvailableAt = new List<TimeBlocks> { TimeBlocks.Morning, TimeBlocks.Midday }
        };

        return job;
    }

    /// <summary>
    /// Determine difficulty tier based on route segment count.
    /// Simple: 1-2 segments (short routes, low risk)
    /// Moderate: 3-4 segments (medium routes, medium risk)
    /// Dangerous: 5+ segments (long routes, high risk)
    /// </summary>
    private static DifficultyTier DetermineDifficulty(int segments)
    {
        if (segments <= 2)
            return DifficultyTier.Simple;
        else if (segments <= 4)
            return DifficultyTier.Moderate;
        else
            return DifficultyTier.Dangerous;
    }

    /// <summary>
    /// Calculate job payment based on economic formula.
    /// Payment = RoomCost(10) + TravelCost(segments*2) + ProfitMargin(tier-based)
    /// Ensures player can afford room cost + travel costs + earn profit
    /// </summary>
    private static int CalculatePayment(int segments, DifficultyTier tier)
    {
        int travelCost = segments * COST_PER_SEGMENT;
        int profitMargin = tier switch
        {
            DifficultyTier.Simple => SIMPLE_PROFIT,
            DifficultyTier.Moderate => MODERATE_PROFIT,
            DifficultyTier.Dangerous => DANGEROUS_PROFIT,
            _ => SIMPLE_PROFIT
        };

        return ROOM_COST + travelCost + profitMargin;
    }

    /// <summary>
    /// Generate procedural cargo description based on difficulty tier.
    /// Simple: letters, documents (small, low-value)
    /// Moderate: packages, goods (medium-value)
    /// Dangerous: valuables, urgent packages (high-value, time-sensitive)
    /// </summary>
    private static string GenerateCargoDescription(DifficultyTier tier)
    {
        return tier switch
        {
            DifficultyTier.Simple => GetSimpleCargo(),
            DifficultyTier.Moderate => GetModerateCargo(),
            DifficultyTier.Dangerous => GetDangerousCargo(),
            _ => "a package"
        };
    }

    /// <summary>
    /// Get random simple cargo description.
    /// </summary>
    private static string GetSimpleCargo()
    {
        string[] cargos = new[]
        {
            "a letter",
            "some documents",
            "a small parcel",
            "correspondence"
        };
        return cargos[Random.Shared.Next(cargos.Length)];
    }

    /// <summary>
    /// Get random moderate cargo description.
    /// </summary>
    private static string GetModerateCargo()
    {
        string[] cargos = new[]
        {
            "a package",
            "a sack of grain",
            "trade goods",
            "merchant supplies"
        };
        return cargos[Random.Shared.Next(cargos.Length)];
    }

    /// <summary>
    /// Get random dangerous cargo description.
    /// </summary>
    private static string GetDangerousCargo()
    {
        string[] cargos = new[]
        {
            "valuable documents",
            "an urgent package",
            "rare goods",
            "critical supplies"
        };
        return cargos[Random.Shared.Next(cargos.Length)];
    }
}
