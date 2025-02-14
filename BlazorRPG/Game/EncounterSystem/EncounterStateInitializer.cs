public class EncounterStateInitializer
{
    public static EncounterStageState Generate(
        Location location,
        LocationSpot locationSpot,
        GameState gameState,
        int playerLevel
        )
    {
        return new EncounterStageState(0);
    }
}
