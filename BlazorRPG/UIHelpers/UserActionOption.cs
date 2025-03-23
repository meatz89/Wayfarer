public record UserActionOption
    (
    int Index,
    string Description,
    bool IsDisabled,
    ActionImplementation ActionImplementation,
    string Location,
    string LocationSpot,
    string Character,
    int LocationDifficulty
    )
{
    public string Display()
    {
        return $"{Index}. {Description}";
    }
}
