public sealed record ActionResult
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; }
    public ActionResultMessages Messages { get; init; }

    public static ActionResult Success(string message, ActionResultMessages changes) =>
        new() { IsSuccess = true, Message = message, Messages = changes };

    public static ActionResult Failure(string message) =>
        new() { IsSuccess = false, Message = message, Messages = new() };
}

public class ActionCreationResult
{
    public ActionTemplate Action { get; set; }
}
