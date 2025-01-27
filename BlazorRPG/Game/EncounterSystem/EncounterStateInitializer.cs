public class EncounterStateInitializer
{
    public static EncounterValues Generate(
        Location location,
        LocationSpot locationSpot,
        GameState gameState,
        int playerLevel
        )
    {
        int outgomeChange = 0;
        int momentumChange = 0;
        int insightChange = 0;
        int resonanceChange = 0;
        int pressureChange = 0;


        return EncounterValues.WithValues(
            momentum: 2 + momentumChange,
            insight: 2 + insightChange,
            resonance: 2 + resonanceChange,
            outcome: 5 + (playerLevel - location.Difficulty) + outgomeChange,
            pressure: 5 + pressureChange
        );
    }
}
