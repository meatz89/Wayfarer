public class WorldLocationsContent
{
    public static Location DeepForest => new LocationBuilder()
         .ForLocation(LocationNames.DeepForest)
         .WithDescription("A dense, ancient forest with towering trees and winding paths.")
         .WithDetailedDescription("The forest stretches before you, thick with ancient trees and a carpet of fallen leaves. The canopy above filters the sunlight, creating a dappled pattern on the forest floor. Animal sounds echo occasionally in the distance. You feel lost and must find your way out.")
         .WithDifficultyLevel(1)
         .AddLocationSpot(spot => spot
             .WithPlayerKnowledge(true)
             .WithName("Forest Clearing")
             .WithDescription("A small open area where sunlight breaks through the canopy.")
             .WithIllumination(Illumination.Bright)
             .WithAtmosphere(Atmosphere.Formal)
             .WithPopulation(Population.Quiet)
             .WithPhysical(Physical.Expansive)
             .AddAction(ActionNames.Rest)
             .AddAction(ActionNames.ForageForFood)
             .AddAction(ActionNames.SearchSurroundings))
         .AddLocationSpot(spot => spot
             .WithName("Forest Stream")
             .WithDescription("A clear stream cutting through the dense forest.")
             .WithIllumination(Illumination.Shadowy)
             .WithAtmosphere(Atmosphere.Formal)
             .WithPopulation(Population.Quiet)
             .WithPhysical(Physical.Hazardous)
             .AddAction(ActionNames.DrinkWater)
             .AddAction(ActionNames.GatherHerbs)
             .AddAction(ActionNames.FollowStream))
         .AddLocationSpot(spot => spot
             .WithName("High Ground")
             .WithDescription("An elevated position offering a vantage point over parts of the forest.")
             .WithIllumination(Illumination.Bright)
             .WithAtmosphere(Atmosphere.Formal)
             .WithPopulation(Population.Quiet)
             .WithPhysical(Physical.Expansive)
             .AddAction(ActionNames.HuntGame)
             .AddAction(ActionNames.NightWatch)
             .AddAction(ActionNames.RestProperly))
         .AddLocationSpot(spot => spot
             .WithName("Forest Edge")
             .WithDescription("The border between the forest and what lies beyond.")
             .WithIllumination(Illumination.Shadowy)
             .WithAtmosphere(Atmosphere.Rough)
             .WithPopulation(Population.Quiet)
             .WithPhysical(Physical.Expansive)
             .AddAction(ActionNames.FindPathOut)
             .AddAction(ActionNames.ObserveArea))
         .WithPlayerKnowledge(true)
         .Build();

    public static Location Village => new LocationBuilder()
        .ForLocation(LocationNames.Village)
        .WithDescription("A small rural settlement with modest wooden buildings.")
        .WithDifficultyLevel(1)
        .WithPlayerKnowledge(false) // Unknown initially
        .AddTravelConnection(LocationNames.DeepForest)
        .Build();
}