using System.Text;

public class ActionSystem
{
    private ActionRepository actionRepository;
    private LocationSystem locationSystem;

    public ActionSystem(
        ActionRepository actionRepository,
        LocationSystem locationSystem
        )
    {
        this.actionRepository = actionRepository;
        this.locationSystem = locationSystem;
    }

    public static object KnowledgeRequirement { get; internal set; }

    public string FormatExistingActions(List<Location> locations)
    {
        StringBuilder sb = new StringBuilder();

        if (locations == null || !locations.Any())
            return "None";

        foreach (Location location in locations)
        {
            foreach (LocationSpot spot in locationSystem.GetLocationSpots(location.Id))
            {
                sb.AppendLine($"## Actions at {location.Id} / {spot.SpotID}:");

                foreach (ActionDefinition actionTemplate in actionRepository.GetActionsForSpot(spot.SpotID))
                {
                    if (actionTemplate != null)
                    {
                        sb.AppendLine($"- {actionTemplate.Name}: {actionTemplate.Description}");
                    }
                }
                sb.AppendLine();
            }
        }

        return sb.Length > 0 ? sb.ToString() : "None";
    }

}