public class FlagDefinition
{
    public FlagStates Flag { get; }
    public FlagCategories Category { get; }
    public FlagStates OpposingFlag { get; }

    public FlagDefinition(FlagStates flag, FlagCategories category, FlagStates opposingFlag)
    {
        Flag = flag;
        Category = category;
        OpposingFlag = opposingFlag;
    }
}