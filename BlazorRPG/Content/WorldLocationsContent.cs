public class WorldLocationsContent
{
    public static string StartingLocation = ElmridgeValleyRegion.CreateCrossroadsLocation().Name;

    public static List<Location> AllLocations = new List<Location>()
    {
        ElmridgeValleyRegion.CreateCrossroadsLocation(),
    };

    public static class ElmridgeValleyRegion
    {
        public static Location CreateCrossroadsLocation()
        {
            Location location =
                new LocationBuilder()
                .WithName("Elmridge Crossroads")
                .WithDescription("A well-worn intersection where several paths meet, with a weathered signpost marking the ways.")
                .WithDetailedDescription("Dirt paths from several directions converge at this grassy clearing. A weathered wooden signpost stands in the center, its arms pointing in different directions with carved names and distances. Wildflowers grow along the path edges, and birds call from nearby trees. This spot offers clear views of the surrounding landscape.")
                .WithDifficultyLevel(1)
                .WithLocationType(LocationTypes.Connective)
                .WithPlayerKnowledge(true)
                .AddLocationSpot(spot => spot
                    .WithName("Signpost")
                    .WithDescription("A weathered wooden signpost with directions carved into its arms.")
                    .AddActionId("Read Signpost")
                    .AddActionId("Observe Travelers")
                    .AddActionId("Rest at Crossroads")
                    .AddConnectionTo("Elmridge Village")
                    .WithPlayerKnowledge(true))
               .Build();

            return location;
        }
    }
}