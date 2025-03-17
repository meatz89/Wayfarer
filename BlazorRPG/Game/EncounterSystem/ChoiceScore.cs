public class ChoiceScore
{
    public IChoice Choice { get; }
    public int Score { get; }

    public ChoiceScore(IChoice choice, int score)
    {
        Choice = choice;
        Score = score;
    }
}