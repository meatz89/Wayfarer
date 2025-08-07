using System;
using System.Collections.Generic;
using System.Linq;

public class ActionGenerator
{
    private readonly ITimeManager _timeManager;
    private readonly LocationRepository _locationRepository;
    private readonly NPCRepository _npcRepository;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly GameWorld _gameWorld;

    public ActionGenerator(
        ITimeManager timeManager,
        LocationRepository locationRepository,
        NPCRepository npcRepository,
        ConnectionTokenManager tokenManager,
        GameWorld gameWorld)
    {
        _timeManager = timeManager;
        _locationRepository = locationRepository;
        _npcRepository = npcRepository;
        _tokenManager = tokenManager;
        _gameWorld = gameWorld;
    }

    /// <summary>
    /// Generate actions for a location based on its data and current time
    /// </summary>
    public List<Wayfarer.ViewModels.LocationActionViewModel> GenerateActionsForLocation(Location location, LocationSpot spot)
    {
        var actions = new List<Wayfarer.ViewModels.LocationActionViewModel>();
        var currentTime = _timeManager.GetCurrentTimeBlock();

        // Generate service-based actions
        if (location.AvailableServices != null)
        {
            foreach (var service in location.AvailableServices)
            {
                actions.AddRange(GenerateServiceActions(service, location, currentTime));
            }
        }

        // Generate spot-specific actions
        if (spot != null)
        {
            actions.AddRange(GenerateSpotActions(spot, currentTime));
        }

        // Generate time-based actions
        actions.AddRange(GenerateTimeBasedActions(location, currentTime));

        // Generate atmosphere-based actions
        actions.AddRange(GenerateAtmosphereActions(location));

        // Limit to 4-5 actions max (per mockup)
        return actions.Take(5).ToList();
    }

    private List<Wayfarer.ViewModels.LocationActionViewModel> GenerateServiceActions(ServiceTypes service, Location location, TimeBlocks currentTime)
    {
        var actions = new List<Wayfarer.ViewModels.LocationActionViewModel>();

        switch (service)
        {
            case ServiceTypes.Rest:
                actions.Add(new Wayfarer.ViewModels.LocationActionViewModel
                {
                    Icon = GetRestIcon(location),
                    Title = GetRestTitle(location),
                    Detail = GetRestDetail(location, currentTime),
                    Cost = GetRestCost(location)
                });
                break;

            case ServiceTypes.Trade:
                if (IsMarketOpen(currentTime))
                {
                    actions.Add(new Wayfarer.ViewModels.LocationActionViewModel
                    {
                        Icon = "🛍️",
                        Title = "Browse Wares",
                        Detail = GetMarketDetail(location, currentTime),
                        Cost = "FREE"
                    });
                }
                break;

            // Letter checking would be a specific location feature, not a service type

            case ServiceTypes.Information:
                actions.Add(new Wayfarer.ViewModels.LocationActionViewModel
                {
                    Icon = "👂",
                    Title = "Listen In",
                    Detail = GetInformationDetail(location, currentTime),
                    Cost = "10m"
                });
                break;
        }

        return actions;
    }

    private List<Wayfarer.ViewModels.LocationActionViewModel> GenerateSpotActions(LocationSpot spot, TimeBlocks currentTime)
    {
        var actions = new List<Wayfarer.ViewModels.LocationActionViewModel>();

        // Generate actions based on spot's domain tags
        if (spot.DomainTags != null)
        {
            foreach (var tag in spot.DomainTags)
            {
                var action = GenerateTagAction(tag, spot);
                if (action != null)
                    actions.Add(action);
            }
        }

        return actions;
    }

    private Wayfarer.ViewModels.LocationActionViewModel GenerateTagAction(string tag, LocationSpot spot)
    {
        return tag.ToLower() switch
        {
            "social" => new Wayfarer.ViewModels.LocationActionViewModel
            {
                Icon = "💬",
                Title = "Join Conversation",
                Detail = "Locals chatting",
                Cost = "15m"
            },
            "commerce" => new Wayfarer.ViewModels.LocationActionViewModel
            {
                Icon = "🪙",
                Title = "Trade Gossip",
                Detail = "Exchange news",
                Cost = "FREE"
            },
            "religious" => new Wayfarer.ViewModels.LocationActionViewModel
            {
                Icon = "🕯️",
                Title = "Light Candle",
                Detail = "Quiet moment",
                Cost = "1c"
            },
            "nature" => new Wayfarer.ViewModels.LocationActionViewModel
            {
                Icon = "🌿",
                Title = "Gather Herbs",
                Detail = "If you know them",
                Cost = "30m"
            },
            "shadow" => new Wayfarer.ViewModels.LocationActionViewModel
            {
                Icon = "🎭",
                Title = "Whispered Deal",
                Detail = "Risky business",
                Cost = "2s"
            },
            _ => null
        };
    }

    private List<Wayfarer.ViewModels.LocationActionViewModel> GenerateTimeBasedActions(Location location, TimeBlocks currentTime)
    {
        var actions = new List<Wayfarer.ViewModels.LocationActionViewModel>();

        switch (currentTime)
        {
            case TimeBlocks.Morning:
                if (location.MorningProperties?.Contains("market_day") == true)
                {
                    actions.Add(new Wayfarer.ViewModels.LocationActionViewModel
                    {
                        Icon = "🥖",
                        Title = "Fresh Bread",
                        Detail = "Still warm",
                        Cost = "2c"
                    });
                }
                break;

            case TimeBlocks.Afternoon:
                if (location.Population?.GetPropertyValue() == "Crowded")
                {
                    actions.Add(new Wayfarer.ViewModels.LocationActionViewModel
                    {
                        Icon = "👥",
                        Title = "People Watch",
                        Detail = "Learn patterns",
                        Cost = "20m"
                    });
                }
                break;

            case TimeBlocks.Evening:
                if (location.Illumination?.GetPropertyValue() == "Thiefy" || location.Illumination?.GetPropertyValue() == "Dark")
                {
                    actions.Add(new Wayfarer.ViewModels.LocationActionViewModel
                    {
                        Icon = "🕯️",
                        Title = "Find Shadows",
                        Detail = "Avoid notice",
                        Cost = "FREE"
                    });
                }
                break;

            case TimeBlocks.Night:
                // Most actions unavailable at night
                break;
        }

        return actions;
    }

    private List<Wayfarer.ViewModels.LocationActionViewModel> GenerateAtmosphereActions(Location location)
    {
        var actions = new List<Wayfarer.ViewModels.LocationActionViewModel>();

        // Generate actions based on atmosphere
        var atmosphereValue = location.Atmosphere?.GetPropertyValue();
        switch (atmosphereValue)
        {
            case "Tense":
                actions.Add(new Wayfarer.ViewModels.LocationActionViewModel
                {
                    Icon = "⚠️",
                    Title = "Stay Alert",
                    Detail = "Watch carefully",
                    Cost = "FREE"
                });
                break;

            case "Chaotic":
                actions.Add(new Wayfarer.ViewModels.LocationActionViewModel
                {
                    Icon = "🎉",
                    Title = "Navigate Chaos",
                    Detail = "Find your way",
                    Cost = "5m"
                });
                break;

            case "Formal":
                actions.Add(new Wayfarer.ViewModels.LocationActionViewModel
                {
                    Icon = "🤫",
                    Title = "Respectful Silence",
                    Detail = "Observe quietly",
                    Cost = "FREE"
                });
                break;
        }

        // Generate actions based on physical properties
        var physicalValue = location.Physical?.GetPropertyValue();
        switch (physicalValue)
        {
            case "Expansive":
                actions.Add(new Wayfarer.ViewModels.LocationActionViewModel
                {
                    Icon = "👀",
                    Title = "Survey Area",
                    Detail = "Get bearings",
                    Cost = "5m"
                });
                break;

            case "Confined":
                actions.Add(new Wayfarer.ViewModels.LocationActionViewModel
                {
                    Icon = "🚪",
                    Title = "Find Exit",
                    Detail = "Note escape routes",
                    Cost = "FREE"
                });
                break;

            case "Hazardous":
                actions.Add(new Wayfarer.ViewModels.LocationActionViewModel
                {
                    Icon = "⚠️",
                    Title = "Check Safety",
                    Detail = "Avoid dangers",
                    Cost = "FREE"
                });
                break;
        }

        return actions;
    }

    // Helper methods for generating contextual details
    private string GetRestIcon(Location location)
    {
        if (location.LocationType == LocationTypes.Landmark) return "🕯️";
        if (location.LocationType == LocationTypes.Forest) return "🌳";
        if (location.Physical?.GetPropertyValue() == "Expansive") return "⛲";
        return "🪑";
    }

    private string GetRestTitle(Location location)
    {
        if (location.LocationType == LocationTypes.Landmark) return "Quiet Rest";
        if (location.LocationType == LocationTypes.Forest) return "Rest in Shade";
        if (location.Physical?.GetPropertyValue() == "Expansive") return "Rest at Fountain";
        return "Take a Seat";
    }

    private string GetRestDetail(Location location, TimeBlocks time)
    {
        var baseTime = location.Physical?.GetPropertyValue() == "Expansive" ? "5 min" : "10 min";
        var detail = time == TimeBlocks.Morning ? "Clear head" : "Catch breath";
        return $"{baseTime} • {detail}";
    }

    private string GetRestCost(Location location)
    {
        if (location.LocationType == LocationTypes.Landmark) return "1c"; // Donation
        return "FREE";
    }

    private bool IsMarketOpen(TimeBlocks time)
    {
        return time == TimeBlocks.Morning || time == TimeBlocks.Afternoon;
    }

    private string GetMarketDetail(Location location, TimeBlocks time)
    {
        if (time == TimeBlocks.Morning) return "Fresh goods";
        if (time == TimeBlocks.Afternoon) return "Closing soon";
        return "Limited stock";
    }

    private string GetInformationDetail(Location location, TimeBlocks time)
    {
        if (location.Population?.GetPropertyValue() == "Crowded") return "Rumors abound";
        if (time == TimeBlocks.Evening) return "Whispered tales";
        return "Local gossip";
    }
}