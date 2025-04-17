using System.Text;

public class ActionSystem
{
    private readonly ActionRepository actionRepository;

    public ActionSystem(ActionRepository actionRepository)
    {
        this.actionRepository = actionRepository;
    }

    public string FormatExistingActions(List<Location> locations)
    {
        StringBuilder sb = new StringBuilder();

        if (locations == null || !locations.Any())
            return "None";

        foreach (Location location in locations)
        {
            if (location.LocationSpots == null || !location.LocationSpots.Any())
                continue;

            foreach (LocationSpot spot in location.LocationSpots)
            {
                if (spot.ActionIds == null || !spot.ActionIds.Any())
                    continue;

                sb.AppendLine($"## Actions at {location.Name} / {spot.Name}:");

                foreach (string actionTemplate in spot.ActionIds)
                {
                    ActionTemplate action = actionRepository.GetAction(actionTemplate);
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