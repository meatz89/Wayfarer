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
                    .WithNodeType(ResourceNodeTypes.Landmark)
                    .WithQuality(QualityTiers.Novice)
                    .AddActionId("Read Signpost")
                    .AddActionId("Observe Travelers")
                    .AddActionId("Rest at Crossroads")
                    .AddConnectionTo("Elmridge Village")
                    .WithPlayerKnowledge(true)
                    .WithDiscoverableAspect(nodeAspect => nodeAspect
                        .WithName("Worn Cart Tracks")
                        .WithDescription("Deep wheel ruts showing frequent travel to the village")
                        .WithSkillType(SkillTypes.Subterfuge)
                        .WithSkillXpGain(1)
                        .WithYields(yield => yield
                            .WithType(YieldTypes.TravelDiscount)
                            .WithTargetId("Elmridge Village")
                            .WithBaseAmount(5))
                        )
                    .WithDiscoverableAspect(nodeAspect => nodeAspect
                        .WithName("Hidden Footpath")
                        .WithDescription("A narrow trail leading toward the river")
                        .WithSkillType(SkillTypes.Subterfuge)
                        .WithSkillXpGain(1)
                        .WithYields(yield => yield
                            .WithType(YieldTypes.ActionUnlock)
                            .WithTargetId("take_shortcut")
                            .WithBaseAmount(1))
                        )
                    )
               .Build();

            return location;
        }
    }
}