/// <summary>
/// Time-specific property mapping (strongly-typed, no Dictionary)
/// </summary>
public class TimeSpecificProperty
{
public TimeBlocks TimeBlock { get; set; }
public List<LocationPropertyType> Properties { get; set; } = new List<LocationPropertyType>();
}
