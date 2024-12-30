public static class GameRules
{
    public static int StartingHealth { get; } = 10;
    public static int StartingCoins { get; } = 0;

    public static int StartingPhysicalEnergy { get; } = 3;
    public static int StartingFocusEnergy { get; } = 3;
    public static int StartingSocialEnergy { get; } = 3;

    public static int MinimumHealth { get; } = 0;
    public static int DailyFoodRequirement { get; } = 2;
    public static int HealthLossNoFood { get; } = 2;
    public static int HealthLossNoShelter { get; } = 2;
}
