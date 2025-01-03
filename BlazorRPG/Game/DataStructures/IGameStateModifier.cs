public interface IGameStateModifier
{
    public Quest GetSource();
}

public class FoodModfier : IGameStateModifier
{
    private Quest source;
    public int AdditionalFood { get; }

    public Quest GetSource()
    {
        return source;
    }
}