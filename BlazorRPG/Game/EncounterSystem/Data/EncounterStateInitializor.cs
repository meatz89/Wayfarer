
public class EncounterStateInitializer
{
    public static EncounterStateValues Generate(int difficulty, int playerLevel)
    {
        return EncounterStateValues.WithValues(
            outcome: 5 + (playerLevel - difficulty),
            momentum: 5,
            insight: 4,
            resonance: 4,
            pressure: 2
        );
    }
}
