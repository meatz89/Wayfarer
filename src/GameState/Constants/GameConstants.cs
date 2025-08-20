/// <summary>
/// Central location for all game constants that aren't part of the configurable GameConfiguration.
/// These are core game mechanics that shouldn't change between games.
/// </summary>
public static class GameConstants
{
    /// <summary>
    /// Weight thresholds for load categories
    /// </summary>
    public static class LoadWeight
    {
        public const int LIGHT_LOAD_MAX = 3;
        public const int MEDIUM_LOAD_MAX = 6;

        // Stamina penalties for load weight
        public const int LIGHT_LOAD_STAMINA_PENALTY = 0;
        public const int MEDIUM_LOAD_STAMINA_PENALTY = 1;
        public const int HEAVY_LOAD_STAMINA_PENALTY = 2;
    }

    /// <summary>
    /// UI and display related constants
    /// </summary>
    public static class UI
    {
        // Wait options for rest/wait actions
        public const int WAIT_OPTION_SHORT_HOURS = 2;
        public const int WAIT_OPTION_LONG_HOURS = 4;

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
        public const int COINS_PER_WEIGHT_UNIT = 10;
        public const int HEAVY_ITEM_WEIGHT_THRESHOLD = 4;
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
    /// Patron and debt related constants
    /// </summary>
    public static class Patron
    {
        public const int INITIAL_PATRON_DEBT = -20;
        public const int PATRON_DEBT_THRESHOLD = -10;

        // Debt leverage thresholds
        public const int EXTREME_DEBT_THRESHOLD = -10;
        public const int MODERATE_DEBT_THRESHOLD = -5;
        public const int SMALL_DEBT_THRESHOLD = -2;
    }

    /// <summary>
    /// DeliveryObligation queue position constants
    /// </summary>
    public static class LetterQueue
    {
        public const int FIRST_POSITION = 1;
        public const int DEADLINE_URGENT_THRESHOLD = 3;
        public const int DEADLINE_CRITICAL_THRESHOLD = 2;
        public const int DEADLINE_EXPIRED = 0;

        // Token relationship thresholds
        public const int HIGH_POSITIVE_RELATIONSHIP_THRESHOLD = 4;
        public const int REPEATED_SKIP_THRESHOLD = 2;
        public const int SKIP_WARNING_THRESHOLD = 3;
    }

    /// <summary>
    /// Miscellaneous game constants
    /// </summary>
    public static class Game
    {
        public const int XP_TO_NEXT_LEVEL_BASE = 100;
        public const int MINUTES_PER_HOUR = 60;
        public const float DEFAULT_ENCOUNTER_CHANCE = 0.3f;
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