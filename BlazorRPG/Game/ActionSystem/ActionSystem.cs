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
            foreach (LocationSpot spot in locationSystem.GetLocationSpots(location.Id))
            {
                sb.AppendLine($"## Actions at {location.Id} / {spot.Id}:");

                foreach (ActionDefinition actionTemplate in actionRepository.GetActionsForSpot(spot.Id))
                {
                    if (actionTemplate != null)
                    {
                        sb.AppendLine($"- {actionTemplate.Id}: {actionTemplate.Goal}");
                    }
                }
                sb.AppendLine();
            }
        }

        return sb.Length > 0 ? sb.ToString() : "None";
    }

}