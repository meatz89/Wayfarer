/// <summary>
/// DTO for projected bond strength change with an NPC
/// </summary>
public class BondChangeDTO
{
    public string NpcId { get; set; }
    public int Delta { get; set; }
    public string Reason { get; set; }
}
