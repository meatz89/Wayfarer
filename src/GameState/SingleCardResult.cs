public class SingleCardResult
{
    public CardInstance Card { get; init; }
    public bool Success { get; init; }
    public int Flow { get; init; }
    public int Roll { get; init; }
    public int SuccessChance { get; init; }
    public int PatienceAdded { get; init; }
}
