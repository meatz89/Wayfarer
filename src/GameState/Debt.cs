using System;

/// <summary>
/// Represents a debt owed to an NPC with interest accumulation
/// </summary>
public class Debt
{
    public string CreditorId { get; set; }
    public int Principal { get; set; }
    public int InterestRate { get; set; } // Percentage per day
    public int StartDay { get; set; }
    public bool IsPaid { get; set; }
    
    /// <summary>
    /// Calculate total amount owed including interest
    /// </summary>
    public int GetTotalOwed(int currentDay)
    {
        if (IsPaid) return 0;
        
        int daysActive = currentDay - StartDay;
        int interest = (int)(Principal * InterestRate * daysActive / 100.0);
        return Principal + interest;
    }
}