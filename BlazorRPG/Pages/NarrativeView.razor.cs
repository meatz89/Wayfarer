using Microsoft.AspNetCore.Components;
public partial class NarrativeViewBase : ComponentBase
{
    [Inject] public GameState GameState { get; set; }
    [Inject] public GameManager GameManager { get; set; }
    [Parameter] public string LocationName { get; set; }
    [Parameter] public EventCallback OnNarrativeCompleted { get; set; }
    [Parameter] public EncounterResult Result { get; set; }
    [Parameter] public bool ShowResult { get; set; } = false;

    protected override void OnParametersSet()
    {
    }

    public List<Outcome> GetActionOutcomesSuccess()
    {
        ActionImplementation actionImplementation = Result.Encounter.ActionImplementation;

        List<Outcome> outcomes = new List<Outcome>();
        outcomes.AddRange(actionImplementation.Costs.ToList());
        outcomes.AddRange(actionImplementation.Rewards.ToList());

        return outcomes;
    }

    public List<Outcome> GetActionOutcomesFailure()
    {
        ActionImplementation actionImplementation = Result.Encounter.ActionImplementation;

        List<Outcome> outcomes = new List<Outcome>();
        outcomes.AddRange(actionImplementation.Costs.ToList());
        outcomes.AddRange(actionImplementation.Rewards.ToList());

        return outcomes;
    }

    public List<Outcome> GetEnergyCosts()
    {
        return Result.Encounter.ActionImplementation.EnergyCosts
            .ToList();
    }

    public MarkupString GetOutcomeIcon(Outcome outcome)
    {
        if (outcome is EnergyOutcome energyOutcome)
        {
            return GetEnergyTypeIcon();
        }

        return outcome switch
        {
            HealthOutcome => new MarkupString("<i class='value-icon health-icon'>❤️</i>"),
            ConcentrationOutcome => new MarkupString("<i class='value-icon concentration-icon'>🌀</i>"),
            ConfidenceOutcome => new MarkupString("<i class='value-icon confidence-icon'>👤</i>"),
            CoinsOutcome => new MarkupString("<i class='value-icon coins-icon'>💰</i>"),
            ResourceOutcome => new MarkupString("<i class='value-icon resource-icon'>📦</i>"),
            KnowledgeOutcome => new MarkupString("<i class='value-icon knowledge-icon'>📚</i>"),
            _ => new MarkupString("")
        };
    }

    public MarkupString GetEnergyTypeIcon()
    {
        return new MarkupString("<i class='value-icon physical-icon'>💪</i>");
    }

    public bool HasWorldEvolution()
    {
        return Result?.WorldEvolution != null;
    }

    public List<LocationChangeDisplay> GetLocationChanges()
    {
        if (!HasWorldEvolution()) return new List<LocationChangeDisplay>();

        var changes = new List<LocationChangeDisplay>();

        // Add new locations
        foreach (var location in Result.WorldEvolution.NewLocations)
        {
            changes.Add(new LocationChangeDisplay
            {
                Name = location.Name,
                Description = $"New location discovered",
                Type = "Location"
            });
        }

        // Add new location spots
        foreach (var spot in Result.WorldEvolution.NewLocationSpots)
        {
            changes.Add(new LocationChangeDisplay
            {
                Name = spot.Name,
                Description = $"New area in {spot.LocationName}",
                Type = "Spot"
            });
        }

        return changes;
    }

    public List<ActionChangeDisplay> GetActionChanges()
    {
        if (!HasWorldEvolution()) return new List<ActionChangeDisplay>();

        var changes = new List<ActionChangeDisplay>();

        // Add new actions
        foreach (var action in Result.WorldEvolution.NewActions)
        {
            changes.Add(new ActionChangeDisplay
            {
                Name = action.Name,
                Description = $"New action at {action.SpotName}",
                Type = action.ActionType
            });
        }

        return changes;
    }

    public List<CharacterChangeDisplay> GetCharacterChanges()
    {
        if (!HasWorldEvolution()) return new List<CharacterChangeDisplay>();

        var changes = new List<CharacterChangeDisplay>();

        // Add new characters
        foreach (var character in Result.WorldEvolution.NewCharacters)
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
        if (!HasWorldEvolution()) return new List<RelationshipChangeDisplay>();

        var changes = new List<RelationshipChangeDisplay>();

        // Add relationship changes
        if (Result.WorldEvolution.RelationshipChanges != null)
        {
            foreach (var relationship in Result.WorldEvolution.RelationshipChanges)
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
        if (!HasWorldEvolution() || Result.WorldEvolution.CoinChange == 0)
            return null;

        return new CoinsChangeDisplay
        {
            Amount = Result.WorldEvolution.CoinChange,
            Current = GameState.PlayerState.Coins,
            New = GameState.PlayerState.Coins + Result.WorldEvolution.CoinChange
        };
    }

    public LocationUpdateDisplay GetLocationUpdate()
    {
        if (!HasWorldEvolution() || !Result.WorldEvolution.LocationUpdate.LocationChanged)
            return null;

        return new LocationUpdateDisplay
        {
            NewLocationName = Result.WorldEvolution.LocationUpdate.NewLocationName
        };
    }

    public List<ResourceChangeDisplay> GetResourceChanges()
    {
        if (!HasWorldEvolution() || Result.WorldEvolution.ResourceChanges == null)
            return new List<ResourceChangeDisplay>();

        var changes = new List<ResourceChangeDisplay>();

        // Add items added
        foreach (var item in Result.WorldEvolution.ResourceChanges.ItemsAdded)
        {
            changes.Add(new ResourceChangeDisplay
            {
                Name = item,
                ChangeType = "Added"
            });
        }

        // Add items removed
        foreach (var item in Result.WorldEvolution.ResourceChanges.ItemsRemoved)
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

// Helper classes for display
public class LocationChangeDisplay
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
}

public class ActionChangeDisplay
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
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

public class LocationUpdateDisplay
{
    public string NewLocationName { get; set; }
}

public class ResourceChangeDisplay
{
    public string Name { get; set; }
    public string ChangeType { get; set; }
}