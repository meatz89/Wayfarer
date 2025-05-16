public record UserActionOption
(
    string ActionName,
    bool IsDisabled,
    ActionImplementation ActionImplementation,
    string LocationId,
    string LocationSpot,
    string Character,
    int LocationDifficulty,
    string DisabledReason,
    ApproachOption SelectedApproach
);