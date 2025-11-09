public class DeckInfo
{
public int Current { get; init; }
public int Max { get; init; }

public DeckInfo(int current, int max)
{
    Current = current;
    Max = max;
}
}