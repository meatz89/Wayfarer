public class TutorialCondition
{
    public Func<GameState, bool> Check { get; }
    public TutorialState.TutorialObjective UnlocksObjective { get; }

    public TutorialCondition(Func<GameState, bool> check, TutorialState.TutorialObjective unlocksObjective)
    {
        Check = check;
        UnlocksObjective = unlocksObjective;
    }
}
