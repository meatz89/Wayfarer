public class RestOption
{
    public string Name { get; set; }
    public int CoinCost { get; set; }
    public int StaminaRecovery { get; set; }
    public int TimeBlockCost { get; set; } = 1;  // Rest actions consume 1 time block by default
    public bool EnablesDawnDeparture { get; set; } = false;
    public bool IsAvailable { get; set; } = true;
    public string RequiredItem { get; set; } = null;  // For church requiring pilgrim token, etc.
    public bool OffersExclusiveContract { get; internal set; }
    public bool ProvidesMarketRumors { get; internal set; }
    public bool CleansesContraband { get; internal set; }
    public string Id { get; internal set; }
}
