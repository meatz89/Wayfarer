/// <summary>
/// DTO for projected state application or removal
/// </summary>
public class StateApplicationDTO
{
public string StateType { get; set; }
public bool Apply { get; set; }
public string Reason { get; set; }
}
