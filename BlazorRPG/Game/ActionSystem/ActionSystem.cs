using System.Text;

public class ActionSystem
{
    private readonly ActionRepository actionRepository;
    private readonly LocationSystem locationSystem;

    public ActionSystem(
        ActionRepository actionRepository,
        LocationSystem locationSystem
        )
    {
        this.actionRepository = actionRepository;
        this.locationSystem = locationSystem;
    }

    public string FormatExistingActions(List<Location> locations)
    {
        StringBuilder sb = new StringBuilder();

        if (locations == null || !locations.Any())
            return "None";

        foreach (Location location in locations)
        {
            foreach (LocationSpot spot in locationSystem.GetLocationSpots(location.Name))
            {
                if (spot.GetActionsForLevel(spot.CurrentLevel).Any())
                    continue;

                sb.AppendLine($"## Actions at {location.Name} / {spot.Name}:");

                foreach (string actionTemplate in spot.GetActionsForLevel(spot.CurrentLevel))
                {
                    ActionDefinition action = actionRepository.GetAction(actionTemplate);
                    if (action != null)
                    {
                        sb.AppendLine($"- {action.Name}: {action.Goal}");
                    }
                }
                sb.AppendLine();
            }
        }

        return sb.Length > 0 ? sb.ToString() : "None";
    }

}