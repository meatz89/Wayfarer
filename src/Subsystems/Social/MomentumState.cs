/// <summary>
/// Current momentum state for UI display in Social challenges
/// Replaces value tuple (int momentum, int doubt, int doubtPenalty)
/// </summary>
public class MomentumState
{
public int Momentum { get; init; }
public int Doubt { get; init; }
public int DoubtPenalty { get; init; }

public MomentumState(int momentum, int doubt, int doubtPenalty)
{
    Momentum = momentum;
    Doubt = doubt;
    DoubtPenalty = doubtPenalty;
}
}
