using Microsoft.AspNetCore.Components;

public partial class NarrativeViewBase : ComponentBase
{
    [Inject] public GameState GameState { get; set; }
    [Parameter] public string LocationName { get; set; }
    [Parameter] public EventCallback OnNarrativeCompleted { get; set; }
    [Parameter] public EncounterResult EncounterResult { get; set; }
    [Parameter] public bool ShowResult { get; set; } = true;

    protected override void OnParametersSet()
    {
    }

    public bool HasPostEncounterEvolution()
    {
        return EncounterResult?.PostEncounterEvolution != null;
    }

    // In NarrativeViewBase
    public string GetLocationTypeDisplay(LocationTypes locationType)
    {
        return locationType switch
        {
            LocationTypes.Hub => "Settlement Hub",
            LocationTypes.Landmark => "Notable Location",
            LocationTypes.Hazard => "Dangerous Area",
            _ => "Path"
        };
    }

    public string GetDepthDisplay(int depth)
    {
        return depth switch
        {
            0 => "Starting Area",
            1 => "Nearby",
            2 => "Short Journey",
            3 => "Moderate Journey",
            4 => "Distant",
            5 => "Far",
            _ => $"Deep Journey (Depth {depth})"
        };
    }

    public string GetServiceIcon(ServiceTypes service)
    {
        return service switch
        {
            ServiceTypes.Rest => "🛌",
            ServiceTypes.Trade => "🛒",
            ServiceTypes.Healing => "❤️",
            ServiceTypes.Information => "📖",
            ServiceTypes.Training => "⚔️",
            ServiceTypes.EquipmentRepair => "🔨",
            ServiceTypes.FoodProduction => "🍲",
            _ => "⚙️"
        };
    }

    public List<LocationChangeWithDepth> GetLocationChangesWithDepth()
    {
        if (!HasPostEncounterEvolution()) return new List<LocationChangeWithDepth>();

        List<LocationChangeWithDepth> changes = new List<LocationChangeWithDepth>();

        foreach (Location location in EncounterResult.PostEncounterEvolution.NewLocations)
        {
            changes.Add(new LocationChangeWithDepth
            {
                Name = location.Id,
                Description = location.Description,
                Type = location.LocationType.ToString(),
                Depth = location.Depth,
                DepthDisplay = GetDepthDisplay(location.Depth),
                TypeDisplay = GetLocationTypeDisplay(location.LocationType),
                AvailableServices = location.AvailableServices
            });
        }

        return changes;
    }

    public class LocationChangeWithDepth
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public int Depth { get; set; }
        public string DepthDisplay { get; set; }
        public string TypeDisplay { get; set; }
        public List<ServiceTypes> AvailableServices { get; set; }
    }

    public List<ActionWithEnergy> GetActionChangesWithEnergy()
    {
        if (!HasPostEncounterEvolution()) return new List<ActionWithEnergy>();

        List<ActionWithEnergy> changes = new List<ActionWithEnergy>();

        foreach (NewAction action in EncounterResult.PostEncounterEvolution.NewActions)
        {
            changes.Add(new ActionWithEnergy
            {
                Name = action.Name,
                Description = action.Description,
                Type = action.ActionType,
                IsRepeatable = action.IsRepeatable,
                EnergyCost = action.EnergyCost
            });
        }

        return changes;
    }

    public class ActionWithEnergy
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public bool IsRepeatable { get; set; }
        public int EnergyCost { get; set; }
    }

    public List<Outcome> GetActionOutcomesSuccess()
    {
        ActionImplementation actionImplementation = EncounterResult.ActionImplementation;

        List<Outcome> outcomes = new List<Outcome>();
        outcomes.AddRange(actionImplementation.Costs.ToList());
        outcomes.AddRange(actionImplementation.Yields.ToList());

        return outcomes;
    }

    public List<Outcome> GetActionOutcomesFailure()
    {
        ActionImplementation actionImplementation = EncounterResult.ActionImplementation;

        List<Outcome> outcomes = new List<Outcome>();
        outcomes.AddRange(actionImplementation.Costs.ToList());
        outcomes.AddRange(actionImplementation.Yields.ToList());

        return outcomes;
    }

    public MarkupString GetOutcomeIcon(Outcome outcome)
    {
        return outcome switch
        {
            HealthOutcome => new MarkupString("<i class='value-icon health-icon'>❤️</i>"),
            ConcentrationOutcome => new MarkupString("<i class='value-icon focus-icon'>🌀</i>"),
            ConfidenceOutcome => new MarkupString("<i class='value-icon spirit-icon'>👤</i>"),
            CoinOutcome => new MarkupString("<i class='value-icon coins-icon'>💰</i>"),
            _ => new MarkupString("")
        };
    }

    public List<CharacterChangeDisplay> GetCharacterChanges()
    {
        if (!HasPostEncounterEvolution()) return new List<CharacterChangeDisplay>();

        List<CharacterChangeDisplay> changes = new List<CharacterChangeDisplay>();

        // Add new characters
        foreach (Character character in EncounterResult.PostEncounterEvolution.NewCharacters)
        {
            changes.Add(new CharacterChangeDisplay
            {
                Name = character.Name,
                Description = character.Role,
                Location = character.Location
            });
        }

        return changes;
    }

    public List<RelationshipChangeDisplay> GetRelationshipChanges()
    {
        if (!HasPostEncounterEvolution()) return new List<RelationshipChangeDisplay>();

        List<RelationshipChangeDisplay> changes = new List<RelationshipChangeDisplay>();

        // Add relationship changes
        if (EncounterResult.PostEncounterEvolution.RelationshipChanges != null)
        {
            foreach (RelationshipChange relationship in EncounterResult.PostEncounterEvolution.RelationshipChanges)
            {
                changes.Add(new RelationshipChangeDisplay
                {
                    CharacterName = relationship.CharacterName,
                    Change = relationship.ChangeAmount
                });
            }
        }

        return changes;
    }

    public CoinsChangeDisplay GetCoinsChange()
    {
        if (!HasPostEncounterEvolution() || EncounterResult.PostEncounterEvolution.CoinChange == 0)
            return null;

        return new CoinsChangeDisplay
        {
            Amount = EncounterResult.PostEncounterEvolution.CoinChange,
            Current = GameState.PlayerState.Coins,
            New = GameState.PlayerState.Coins + EncounterResult.PostEncounterEvolution.CoinChange
        };
    }

    public List<ResourceChangeDisplay> GetResourceChanges()
    {
        if (!HasPostEncounterEvolution() || EncounterResult.PostEncounterEvolution.ResourceChanges == null)
            return new List<ResourceChangeDisplay>();

        List<ResourceChangeDisplay> changes = new List<ResourceChangeDisplay>();

        // Add items added
        foreach (string item in EncounterResult.PostEncounterEvolution.ResourceChanges.ItemsAdded)
        {
            changes.Add(new ResourceChangeDisplay
            {
                Name = item,
                ChangeType = "Added"
            });
        }

        // Add items removed
        foreach (string item in EncounterResult.PostEncounterEvolution.ResourceChanges.ItemsRemoved)
        {
            changes.Add(new ResourceChangeDisplay
            {
                Name = item,
                ChangeType = "Removed"
            });
        }

        return changes;
    }

    public MarkupString GetEvolutionIcon(string type)
    {
        return type switch
        {
            "Location" => new MarkupString("<i class='value-icon location-icon'>🗺️</i>"),
            "Spot" => new MarkupString("<i class='value-icon spot-icon'>📍</i>"),
            "Action" => new MarkupString("<i class='value-icon action-icon'>⚙️</i>"),
            "Character" => new MarkupString("<i class='value-icon character-icon'>👤</i>"),
            "Relationship" => new MarkupString("<i class='value-icon relationship-icon'>🤝</i>"),
            "Travel" => new MarkupString("<i class='value-icon travel-icon'>🚶</i>"),
            "Added" => new MarkupString("<i class='value-icon added-icon'>➕</i>"),
            "Removed" => new MarkupString("<i class='value-icon removed-icon'>➖</i>"),
            _ => new MarkupString("<i class='value-icon generic-icon'>ℹ️</i>")
        };
    }
}

public class CharacterChangeDisplay
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
}

public class RelationshipChangeDisplay
{
    public string CharacterName { get; set; }
    public int Change { get; set; }
}

public class CoinsChangeDisplay
{
    public int Amount { get; set; }
    public int Current { get; set; }
    public int New { get; set; }
}

public class ResourceChangeDisplay
{
    public string Name { get; set; }
    public string ChangeType { get; set; }
}