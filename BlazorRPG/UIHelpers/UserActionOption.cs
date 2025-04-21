public record UserActionOption
    (
    string ActionId,
    bool IsDisabled,
    ActionImplementation ActionImplementation,
    string Location,
    string LocationSpot,
    string Character,
    int LocationDifficulty,
    string DisabledReason
    )
{
    public string Display()
    {
        return $"{ActionId}";
    }
}
