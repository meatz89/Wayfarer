public record UserActionOption
    (
    int Index,
    string ActionId,
    string ActionName,
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
        return $"{Index}. {ActionName}";
    }
}
