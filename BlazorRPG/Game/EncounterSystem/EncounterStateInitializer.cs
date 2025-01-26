public class EncounterStateInitializer
{
    public static EncounterValues Generate(
        int difficulty,
        int playerLevel,
        LocationSpotProperties locationProperties,
        List<PlayerStatus> playerStatusTypes
        )
    {
        int outgomeChange = 0;
        int momentumChange = 0;
        int insightChange = 0;
        int resonanceChange = 0;
        int pressureChange = 0;

        if (locationProperties.IsTemperatureSet &&
            locationProperties.Temperature.Value == Temperature.Warm)
        {
            outgomeChange += 1;
            pressureChange -= 1;
        }

        if (locationProperties.IsAccessabilitySet &&
            locationProperties.Accessibility.Value == Accessability.Private)
        {
            insightChange += 1;
        }

        if (locationProperties.IsRoomLayoutSet &&
            locationProperties.RoomLayout.Value == RoomLayout.Secluded)
        {
            pressureChange -= 1;
        }

        if (playerStatusTypes.Contains(PlayerStatus.COLD))
        {
            resonanceChange -= 1;
        }


        return EncounterValues.WithValues(
            momentum: 2 + momentumChange,
            insight: 2 + insightChange,
            resonance: 2 + resonanceChange,
            outcome: 5 + (playerLevel - difficulty) + outgomeChange,
            pressure: 5 + pressureChange
        );
    }
}
