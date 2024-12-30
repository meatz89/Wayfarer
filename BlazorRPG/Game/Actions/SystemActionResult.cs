public sealed record SystemActionResult
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; }

    public static SystemActionResult Success(string message) =>
        new() { IsSuccess = true, Message = message };

    public static SystemActionResult Failure(string message) =>
        new() { IsSuccess = false, Message = message };
}
