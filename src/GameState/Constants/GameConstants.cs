/// <summary>
/// Central Venue for all game constants that aren't part of the configurable GameConfiguration.
/// These are core game mechanics that shouldn't change between games.
/// </summary>
public static class GameConstants
{
    /// <summary>
    /// Focus thresholds for load categories
    /// </summary>
    public static class LoadFocus
    {
        public const int LIGHT_LOAD_MAX = 3;
        public const int MEDIUM_LOAD_MAX = 6;

        // Hunger increase for load focus
        public const int LIGHT_LOAD_HUNGER_INCREASE = 0;
        public const int MEDIUM_LOAD_HUNGER_INCREASE = 1;
        public const int HEAVY_LOAD_HUNGER_INCREASE = 2;
    }

    /// <summary>
    /// UI and display related constants
    /// </summary>
    public static class UI
    {
        // Wait options for rest/wait actions (in segments)
        public const int WAIT_OPTION_SHORT_SEGMENTS = 2; // Short wait option
        public const int WAIT_OPTION_LONG_SEGMENTS = 4; // Long wait option

        // Streaming content estimation
        public const int ESTIMATED_STREAMING_TOKENS = 1000;
        public const int CHARS_PER_TOKEN_ESTIMATE = 4;

        // Display limits
        public const int MAX_ERROR_DISPLAY_COUNT = 10;
        public const int COMMAND_DESCRIPTION_MAX_LENGTH = 200;
    }

    /// <summary>
    /// Inventory and item related constants
    /// </summary>
    public static class Inventory
    {
        public const int DEFAULT_INVENTORY_CAPACITY = 10;
        public const int COINS_PER_FOCUS_UNIT = 10;
        public const int HEAVY_ITEM_FOCUS_THRESHOLD = 4;
    }

    /// <summary>
    /// Network and timeout related constants
    /// </summary>
    public static class Network
    {
        public const int HTTP_CLIENT_TIMEOUT_SECONDS = 5;
        public const int SERVICE_STARTUP_MAX_ATTEMPTS = 30;
        public const int SERVICE_STARTUP_RETRY_DELAY_MS = 100;
    }

    /// <summary>
    /// Miscellaneous game constants
    /// </summary>
    public static class Game
    {
        public const int MINUTES_PER_HOUR = 60;
        public const int DEFAULT_ENCOUNTER_CHANCE = 30; // 30%
    }

    /// <summary>
    /// Validation and limits
    /// </summary>
    public static class Validation
    {
        public const int MIN_NPC_COUNT = 6;
        public const int MIN_PROFESSION_TYPES = 3;
        public const int MIN_LOCATION_COUNT = 2;
    }

    /// <summary>
    /// String parsing constants
    /// </summary>
    public static class StringParsing
    {
        public const string WITH_SEPARATOR = " with ";
        public const int WITH_SEPARATOR_LENGTH = 6;
        public const string AT_SEPARATOR = " at ";
        public const int AT_SEPARATOR_LENGTH = 4;
    }
}