public class EncounterStateInitializer
{
    public static EncounterValues Generate(
        int difficulty,
        int playerLevel,
        LocationProperties locationProperties,
        List<PlayerStatusTypes> playerStatusTypes
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
            locationProperties.Accessability.Value == Accessability.Private)
        {
            insightChange += 1;
        }

        if (locationProperties.IsRoomLayoutSet &&
            locationProperties.RoomLayout.Value == RoomLayout.Secluded)
        {
            pressureChange -= 1;
        }

        if (locationProperties.IsCrowdDensitySet &&
            locationProperties.CrowdDensity.Value == CrowdDensity.Busy)
        {
            pressureChange += 1;
        }

        if (playerStatusTypes.Contains(PlayerStatusTypes.Wet) ||
            playerStatusTypes.Contains(PlayerStatusTypes.Cold))
        {
            resonanceChange -= 1;
        }


        return EncounterValues.WithValues(
            momentum: 5 + momentumChange,
            insight: 5 + insightChange,
            resonance: 5 + resonanceChange,
            outcome: 2 + (playerLevel - difficulty) + outgomeChange,
            pressure: 2 + pressureChange
        );
    }
}
