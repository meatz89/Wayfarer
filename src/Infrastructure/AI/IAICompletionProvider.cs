/// <summary>
/// Interface for AI text completion providers.
/// Enables mocking for testability and swapping AI backends (Ollama, OpenAI, etc).
///
/// TESTABILITY: Inject mock implementations to test:
/// - Defined contexts → expected narrative outputs
/// - Prompt optimization (verify context inclusion)
/// - Model comparison (same prompt, different implementations)
/// </summary>
public interface IAICompletionProvider
{
    /// <summary>
    /// Stream text completion from AI model.
    /// Yields tokens as they arrive for real-time display.
    /// </summary>
    /// <param name="prompt">The complete prompt to send to AI</param>
    /// <param name="cancellationToken">Cancellation token for timeout control</param>
    /// <returns>Async enumerable of text tokens</returns>
    IAsyncEnumerable<string> StreamCompletionAsync(string prompt, CancellationToken cancellationToken);

    /// <summary>
    /// Check if AI provider is available and healthy.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if provider is ready to accept requests</returns>
    Task<bool> CheckHealthAsync(CancellationToken cancellationToken);
}
