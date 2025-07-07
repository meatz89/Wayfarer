public class RestOption
{
    public string Name { get; set; }
    public int CoinCost { get; set; }
    public int StaminaRecovery { get; set; }
    public bool EnablesDawnDeparture { get; set; } = false;
    public bool IsAvailable { get; set; } = true;
    public string RequiredItem { get; set; } = null;  // For church requiring pilgrim token, etc.
}
