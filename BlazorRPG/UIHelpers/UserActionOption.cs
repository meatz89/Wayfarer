public record UserActionOption
    (
    int Index,
    string Description,
    bool IsDisabled,
    ActionImplementation ActionImplementation,
    LocationNames Location,
    string LocationSpot,
    CharacterNames Character)
{
    public string Display()
    {
        return $"{Index}. {Description}";
    }
}
