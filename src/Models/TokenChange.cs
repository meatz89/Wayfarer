namespace Wayfarer.Models;

public class TokenChange
{
    public ConnectionType TokenType { get; set; }
    public int Amount { get; set; }  // Positive for gains, negative for costs
    public string NpcName { get; set; }
    public string Effect { get; set; }  // e.g., "Reduces leverage", "Unlocks special letters"
}