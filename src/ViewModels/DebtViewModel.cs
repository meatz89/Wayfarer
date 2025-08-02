using System.Collections.Generic;

/// <summary>
/// View model for debt management screen
/// </summary>
public class DebtViewModel
{
    public List<DebtInfo> ActiveDebts { get; set; } = new();
    public List<LenderInfo> AvailableLenders { get; set; } = new();
    public int PlayerCoins { get; set; }
    public int TotalDebt { get; set; }
    public int TotalDailyInterest { get; set; }
}

public class DebtInfo
{
    public string CreditorId { get; set; }
    public string CreditorName { get; set; }
    public int Principal { get; set; }
    public int InterestRate { get; set; }
    public int DaysActive { get; set; }
    public int AccruedInterest { get; set; }
    public int TotalOwed { get; set; }
    public int TokenDebt { get; set; }
}

public class LenderInfo
{
    public string NPCId { get; set; }
    public string Name { get; set; }
    public string LocationId { get; set; }
    public string LocationName { get; set; }
    public int MaxLoanAmount { get; set; }
    public int InterestRate { get; set; }
    public bool IsAvailable { get; set; }
}