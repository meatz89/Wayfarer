public class OllamaResponseData
{
    public string Model { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string MessageRole { get; set; } = string.Empty;
    public string MessageContent { get; set; } = string.Empty;
    public bool Done { get; set; }
    public string DoneReason { get; set; } = string.Empty;
}