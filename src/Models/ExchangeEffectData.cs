/// <summary>
/// Strongly typed exchange data - no more dictionary bullshit
/// </summary>
public class ExchangeEffectData
{
    public ExchangeCost Cost { get; set; }
    public ExchangeReward Reward { get; set; }
}

public class ExchangeCost
{
    public int? Coins { get; set; }
    public int? Health { get; set; }
    public int? Hunger { get; set; }
    public int? Attention { get; set; }
    public int? Stamina { get; set; }
}

public class ExchangeReward
{
    public int? Coins { get; set; }
    public int? Health { get; set; }
    public int? Hunger { get; set; }
    public int? Attention { get; set; }
    public int? Stamina { get; set; }
}