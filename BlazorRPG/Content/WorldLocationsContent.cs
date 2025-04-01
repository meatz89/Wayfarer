public class WorldLocationsContent
{
    public static Location Forest => new LocationBuilder()
         .ForLocation(LocationNames.Forest)
         .WithDescription("An ancient forest with towering trees and winding paths.")
         .WithDetailedDescription("The forest stretches before you, thick with ancient trees and a carpet of fallen leaves. The canopy above filters the sunlight, creating a dappled pattern on the forest floor. Animal sounds echo occasionally in the distance, punctuating the otherwise eerie silence.")
         .WithDifficultyLevel(1)
         .AddLocationSpot(spot => spot
             .WithName("Direct Path")
             .WithDescription("A narrow dirt path cutting directly through the woods.")
             .WithIllumination(Illumination.Shadowy)
             .WithAtmosphere(Atmosphere.Tense)
             .WithPopulation(Population.Isolated)
             .WithPhysical(Physical.Hazardous)
             .AddAction(ActionNames.DirectForestTravel))

         .AddLocationSpot(spot => spot
             .WithName("Scenic Route")
             .WithDescription("A winding trail that takes longer but avoids the most dangerous areas.")
             .WithIllumination(Illumination.Bright)
             .WithAtmosphere(Atmosphere.Formal)
             .WithPopulation(Population.Quiet)
             .WithPhysical(Physical.Expansive)
             .AddAction(ActionNames.ScenicForestTravel))

         .WithPlayerKnowledge(true)
         .AddTravelConnection(LocationNames.Village)
         .Build();

    public static Location Village => new LocationBuilder()
        .ForLocation(LocationNames.Village)
        .WithDescription("A small rural settlement with modest wooden buildings.")
        .WithDifficultyLevel(1)
        .WithPlayerKnowledge(false) // Unknown initially
        .AddTravelConnection(LocationNames.Forest)
        .Build(); // Village spots will be defined elsewhere

}
