public class NewAction
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Goal { get; set; } = "";
    public string Complication { get; set; } = "";
    public string ActionType { get; set; } = "";
    public string SpotName { get; set; } = "";
    public string LocationName { get; set; } = "";
    public bool IsRepeatable { get; set; } = true;
    public int EnergyCost { get; set; } = 1;
}