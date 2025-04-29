public record UserActionOption
(
    string actionName,
    bool IsDisabled,
    ActionImplementation ActionImplementation,
    string LocationId,
    string LocationSpot,
    string Character,
    int LocationDifficulty,
    string DisabledReason
);